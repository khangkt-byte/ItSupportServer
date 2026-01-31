using ClosedXML.Excel;
using FuzzySharp;
using ItSupportServer.Data.Models;
using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Issue log import/export service implementation
    /// Pattern: ServiceNow data import API
    /// Security: Validation, duplicate detection, transaction support
    /// Performance: DTO projections, batch operations, single transaction
    /// Reference: Clean Architecture (Uncle Bob), CQRS pattern
    /// </summary>
    public class IssueLogImportService : IIssueLogImportService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<IssueLogImportService> _logger;
        private readonly IssueLogImportMapper _importMapper;

        private const string AreaCongTy = "Công ty";
        private const string AreaChiNhanh = "Chi nhánh";
        private const int FuzzyMatchThreshold = 85;

        public IssueLogImportService(
            AppDbContext db,
            ILogger<IssueLogImportService> logger,
            IssueLogImportMapper importMapper)
        {
            _db = db;
            _logger = logger;
            _importMapper = importMapper;
        }

        // ===== STEP 1: VALIDATION + DUPLICATE DETECTION =====

        public async Task<ImportValidationResultDto> ValidateImportAsync(Stream excelStream)
        {
            _logger.LogInformation("Validating Excel import with duplicate detection");

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1).ToList();

            var validationRows = new List<ImportRowValidation>();
            int validCount = 0, warningCount = 0, errorCount = 0, duplicateCount = 0;

            // ✅ USE MAPPER: Load department reference data as DTOs
            var departmentList = await _importMapper
                .ProjectToDepartmentReference(_db.Departments
                    .Where(d => d.DeletedAt == null))
                .ToListAsync();

            var departments = _importMapper.BuildDepartmentLookup(departmentList);

            // ✅ USE MAPPER: Load recent logs for duplicate detection
            var recentLogs = await _importMapper
                .ProjectToRecentIssueLogDto(_db.IssueLogs
                    .Where(il => il.DateReported >= DateTime.UtcNow.AddDays(-90) && il.DeletedAt == null))
                .ToListAsync();

            foreach (var row in rows.Take(100)) // Preview first 100 rows
            {
                var rowNumber = row.RowNumber();
                var rowValidation = new ImportRowValidation { RowNumber = rowNumber };

                try
                {
                    // Parse Excel data
                    var (operatorText, requesterText, departmentText, isCongTy, isChiNhanh, issueDesc, dateReported) 
                        = ParseExcelRow(row);

                    // Validate required fields
                    rowValidation = ValidateRequiredFields(rowValidation, operatorText, departmentText, issueDesc, dateReported);

                    // Validate area
                    rowValidation = ValidateArea(rowValidation, isCongTy, isChiNhanh);

                    string areaName = isCongTy ? AreaCongTy : AreaChiNhanh;

                    // ✅ FIX: Fuzzy match department (using DTO)
                    var matchResult = MatchDepartment(rowValidation, departmentText, departments);
                    var deptId = matchResult.departmentId;
                    rowValidation = matchResult.validation;

                    // Duplicate detection
                    if (deptId.HasValue && dateReported.HasValue && !string.IsNullOrWhiteSpace(issueDesc))
                    {
                        var duplicateResult = DetectDuplicates(issueDesc, deptId.Value, dateReported.Value, recentLogs);
                        
                        if (duplicateResult.duplicate != null)
                        {
                            rowValidation = rowValidation
                                .AddWarning(
                                    "Duplicate",
                                    $"Có thể trùng với log {duplicateResult.duplicate.IssLogId.ToString()[..8]}... ({duplicateResult.duplicate.MatchScore}% giống)",
                                    null, null, null, duplicateResult.duplicate)
                                with { DuplicateOf = duplicateResult.duplicate.IssLogId };
                            
                            duplicateCount++;
                        }
                    }

                    // ✅ USE MAPPER: Store preview data (type-safe DTO)
                    rowValidation = rowValidation with
                    {
                        PreviewData = _importMapper.CreatePreviewData(
                            operatorText, requesterText, departmentText,
                            rowValidation.MappedDepartmentName, areaName,
                            issueDesc, dateReported, rowValidation.DuplicateOf.HasValue)
                    };
                }
                catch (Exception ex)
                {
                    rowValidation = rowValidation.AddError("General", $"Lỗi đọc dữ liệu: {ex.Message}");
                }

                validationRows.Add(rowValidation);

                if (rowValidation.HasErrors)
                    errorCount++;
                else if (rowValidation.HasWarnings)
                    warningCount++;
                else
                    validCount++;
            }

            _logger.LogInformation(
                "Validation complete: {Valid} valid, {Warning} warnings, {Error} errors, {Duplicate} duplicates",
                validCount, warningCount, errorCount, duplicateCount);

            return new ImportValidationResultDto
            {
                TotalRows = rows.Count,
                ValidCount = validCount,
                WarningCount = warningCount,
                ErrorCount = errorCount,
                DuplicateCount = duplicateCount,
                IsValid = errorCount == 0,
                Rows = validationRows
            };
        }

        // ===== STEP 2: IMPORT WITH DUPLICATE HANDLING =====

        public async Task<ImportResultDto> ImportFromExcelAsync(Stream excelStream, ImportOptionsDto options)
        {
            _logger.LogInformation("Starting Excel import with duplicate handling: {Strategy}",
                options.DuplicateHandling);

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1).ToList();

            int successCount = 0, updatedCount = 0, skippedCount = 0, autoMatched = 0, departmentsCreated = 0;
            var errors = new List<string>();
            var warnings = new List<string>();

            // ✅ USE MAPPER: Load reference data as DTOs
            var departmentList = await _importMapper
                .ProjectToDepartmentReference(_db.Departments
                    .Where(d => d.DeletedAt == null))
                .ToListAsync();

            var departmentLookup = _importMapper.BuildDepartmentLookup(departmentList);

            // Simple ID lookup for fuzzy matching
            var departments = departmentList.ToDictionary(
                d => d.NormalizedName,
                d => d.DptId,
                StringComparer.OrdinalIgnoreCase);

            var areaList = await _importMapper
                .ProjectToAreaReference(_db.Areas
                    .Where(a => a.DeletedAt == null))
                .ToListAsync();

            var areas = areaList.ToDictionary(
                a => a.NormalizedName,
                a => a.AreaId,
                StringComparer.OrdinalIgnoreCase);

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                foreach (var row in rows)
                {
                    var rowNumber = row.RowNumber();

                    try
                    {
                        // Parse data
                        var operatorText = ExcelHelper.GetCellString(row, 1);
                        var requesterText = ExcelHelper.GetCellString(row, 2);
                        var departmentText = ExcelHelper.GetCellString(row, 3);
                        var isCongTy = ExcelHelper.GetCellString(row, 4).Equals("X", StringComparison.OrdinalIgnoreCase);
                        var isChiNhanh = ExcelHelper.GetCellString(row, 5).Equals("X", StringComparison.OrdinalIgnoreCase);
                        var issueDesc = ExcelHelper.GetCellString(row, 6);
                        var cause = ExcelHelper.GetCellString(row, 7);
                        var resolution = ExcelHelper.GetCellString(row, 8);
                        var permanentFix = ExcelHelper.GetCellString(row, 9);
                        var dateReported = ExcelHelper.TryGetDateTime(row.Cell(10));
                        var status = ExcelHelper.GetCellString(row, 11);

                        // Validate required
                        if (string.IsNullOrWhiteSpace(operatorText) || 
                            string.IsNullOrWhiteSpace(departmentText) || 
                            string.IsNullOrWhiteSpace(issueDesc) ||
                            !dateReported.HasValue)
                        {
                            if (options.SkipRowsWithErrors)
                            {
                                skippedCount++;
                                errors.Add($"Row {rowNumber}: Thiếu dữ liệu bắt buộc");
                                continue;
                            }
                            throw new BusinessRuleException($"Row {rowNumber}: Thiếu dữ liệu bắt buộc");
                        }

                        // Map Department with manual override support
                        var departmentId = await ResolveDepartmentIdAsync(
                            departmentText, departmentLookup, departments, options, rowNumber);

                        if (!departmentId.HasValue)
                        {
                            if (options.SkipRowsWithErrors)
                            {
                                skippedCount++;
                                errors.Add($"Row {rowNumber}: Không thể map bộ phận '{departmentText}'");
                                continue;
                            }
                            throw new NotFoundException($"Row {rowNumber}: Bộ phận '{departmentText}' không tồn tại");
                        }

                        // Track auto-matched
                        if (!departments.ContainsKey(departmentText.ToLower()) ||
                            departmentId.Value != departments.GetValueOrDefault(departmentText.ToLower(), -1))
                        {
                            autoMatched++;
                        }

                        // Map Area
                        string areaName = isCongTy ? AreaCongTy : AreaChiNhanh;
                        
                        if (!areas.TryGetValue(areaName.ToLower(), out var areaId))
                        {
                            areaId = await CreateAreaAsync(areaName);
                            areas[areaName.ToLower()] = areaId;
                        }

                        // Check for duplicates
                        var duplicateLog = await FindDuplicateInDbAsync(
                            issueDesc, departmentId.Value, dateReported.Value, operatorText);

                        if (duplicateLog != null)
                        {
                            var handleResult = HandleDuplicate(
                                duplicateLog, options.DuplicateHandling, rowNumber,
                                cause, resolution, permanentFix, status);

                            switch (handleResult.action)
                            {
                                case DuplicateAction.Skip:
                                    skippedCount++;
                                    warnings.Add(handleResult.message);
                                    continue;

                                case DuplicateAction.Update:
                                    _importMapper.UpdateEntityFromExcelRow(
                                        duplicateLog, cause, resolution, permanentFix, status);
                                    updatedCount++;
                                    warnings.Add(handleResult.message);
                                    continue;

                                case DuplicateAction.CreateNew:
                                    warnings.Add(handleResult.message);
                                    break;

                                case DuplicateAction.Fail:
                                    throw new BusinessRuleException(handleResult.message);
                            }
                        }

                        // ✅ USE MAPPER: Create new log from DTO
                        var excelRowDto = new ExcelRowDto
                        {
                            Operator = operatorText,
                            Requester = requesterText,
                            DepartmentId = departmentId.Value,
                            AreaId = areaId,
                            IssueDescription = issueDesc,
                            Cause = cause,
                            Resolution = resolution,
                            PermanentFix = permanentFix,
                            DateReported = dateReported.Value,
                            Status = status
                        };

                        var log = _importMapper.MapExcelRowToEntity(excelRowDto);

                        await _db.IssueLogs.AddAsync(log);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error importing row {Row}", rowNumber);
                        
                        if (options.SkipRowsWithErrors)
                        {
                            skippedCount++;
                            errors.Add($"Row {rowNumber}: {ex.Message}");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                await _db.SaveChangesAsync();

                if (errors.Count == 0 || options.SkipRowsWithErrors)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation(
                        "Import completed: {Success} created, {Updated} updated, {Skipped} skipped",
                        successCount, updatedCount, skippedCount);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Import failed with {Count} errors", errors.Count);
                }

                return new ImportResultDto
                {
                    SuccessCount = successCount,
                    UpdatedCount = updatedCount,
                    SkippedCount = skippedCount,
                    AutoMatched = autoMatched,
                    DepartmentsCreated = departmentsCreated,
                    Errors = errors,
                    Warnings = warnings
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ===== EXPORT TO EXCEL =====

        public async Task<byte[]> ExportToExcelAsync(QueryParameters? parameters = null)
        {
            _logger.LogInformation("Exporting issue logs to Excel");

            var query = _db.IssueLogs
                .Include(il => il.Department)
                .Include(il => il.Area)
                .Where(il => il.DeletedAt == null)
                .AsNoTracking();

            if (parameters?.Search != null)
            {
                query = query.Where(il =>
                    il.Operator.Contains(parameters.Search) ||
                    il.Department.Name.Contains(parameters.Search) ||
                    il.IssueDescription.Contains(parameters.Search));
            }

            var logs = await query.OrderByDescending(il => il.DateReported).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Nhật ký sự cố IT");

            // Header
            ExcelHelper.SetHeaderRow(worksheet);

            // Data rows
            var currentRow = 2;
            foreach (var log in logs)
            {
                worksheet.Cell(currentRow, 1).Value = log.Operator;
                worksheet.Cell(currentRow, 2).Value = log.Requester ?? "";
                worksheet.Cell(currentRow, 3).Value = log.Department.Name;
                worksheet.Cell(currentRow, 4).Value = log.Area.Name == AreaCongTy ? "X" : "";
                worksheet.Cell(currentRow, 5).Value = log.Area.Name == AreaChiNhanh ? "X" : "";
                worksheet.Cell(currentRow, 6).Value = log.IssueDescription;
                worksheet.Cell(currentRow, 7).Value = log.Cause ?? "";
                worksheet.Cell(currentRow, 8).Value = log.Resolution ?? "";
                worksheet.Cell(currentRow, 9).Value = log.PermanentFix ?? "";
                worksheet.Cell(currentRow, 10).Value = log.DateReported;
                worksheet.Cell(currentRow, 11).Value = log.Status ?? "";

                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            _logger.LogInformation("Exported {Count} issue logs to Excel", logs.Count);

            return stream.ToArray();
        }

        // ===== PRIVATE HELPER METHODS =====

        /// <summary>
        /// Parse Excel row to data tuple
        /// </summary>
        private static (string operatorText, string requesterText, string departmentText, 
            bool isCongTy, bool isChiNhanh, string issueDesc, DateTime? dateReported) ParseExcelRow(IXLRow row)
        {
            return (
                ExcelHelper.GetCellString(row, 1),
                ExcelHelper.GetCellString(row, 2),
                ExcelHelper.GetCellString(row, 3),
                ExcelHelper.GetCellString(row, 4).Equals("X", StringComparison.OrdinalIgnoreCase),
                ExcelHelper.GetCellString(row, 5).Equals("X", StringComparison.OrdinalIgnoreCase),
                ExcelHelper.GetCellString(row, 6),
                ExcelHelper.TryGetDateTime(row.Cell(10))
            );
        }

        /// <summary>
        /// Validate required fields
        /// </summary>
        private static ImportRowValidation ValidateRequiredFields(
            ImportRowValidation validation,
            string operatorText,
            string departmentText,
            string issueDesc,
            DateTime? dateReported)
        {
            if (string.IsNullOrWhiteSpace(operatorText))
                validation = validation.AddError("Operator", "Thiếu người thực hiện");

            if (string.IsNullOrWhiteSpace(departmentText))
                validation = validation.AddError("Department", "Thiếu bộ phận");

            if (string.IsNullOrWhiteSpace(issueDesc))
                validation = validation.AddError("IssueDescription", "Thiếu mô tả sự cố");

            if (!dateReported.HasValue)
                validation = validation.AddError("DateReported", "Ngày báo cáo không hợp lệ");

            return validation;
        }

        /// <summary>
        /// Validate area checkboxes
        /// </summary>
        private static ImportRowValidation ValidateArea(
            ImportRowValidation validation,
            bool isCongTy,
            bool isChiNhanh)
        {
            if (isCongTy && isChiNhanh)
                return validation.AddError("Area", "Chỉ được chọn 1 trong 2: Công ty HOẶC Chi nhánh");
            
            if (!isCongTy && !isChiNhanh)
                return validation.AddError("Area", "Phải chọn Công ty hoặc Chi nhánh");

            return validation;
        }

        /// <summary>
        /// Match department with fuzzy matching
        /// ✅ FIX: Use DepartmentReferenceDto instead of anonymous type
        /// </summary>
        private static (int? departmentId, ImportRowValidation validation) MatchDepartment(
            ImportRowValidation validation,
            string departmentText,
            Dictionary<string, DepartmentReferenceDto> departments)
        {
            if (string.IsNullOrWhiteSpace(departmentText))
                return (null, validation);

            if (departments.TryGetValue(departmentText.ToLower(), out var exactMatch))
            {
                return (
                    exactMatch.DptId,
                    validation with
                    {
                        MappedDepartmentId = exactMatch.DptId,
                        MappedDepartmentName = exactMatch.Name
                    }
                );
            }

            // Fuzzy match
            var bestMatch = FuzzySharp.Process.ExtractOne(
                departmentText,
                departments.Keys,
                cutoff: FuzzyMatchThreshold
            );

            if (bestMatch != null)
            {
                var matched = departments[bestMatch.Value];
                return (
                    matched.DptId,
                    validation
                        .AddWarning(
                            "Department",
                            $"'{departmentText}' → '{matched.Name}' ({bestMatch.Score}% khớp)",
                            departmentText, matched.Name, matched.DptId)
                        with
                        {
                            MappedDepartmentId = matched.DptId,
                            MappedDepartmentName = matched.Name
                        }
                );
            }

            return (
                null,
                validation.AddError("Department", $"Không tìm thấy bộ phận '{departmentText}'", departmentText)
            );
        }

        /// <summary>
        /// Resolve department ID with manual mappings, fuzzy match, or auto-create
        /// ✅ USE DTO: DepartmentReferenceDto lookup
        /// </summary>
        private async Task<int?> ResolveDepartmentIdAsync(
            string departmentText,
            Dictionary<string, DepartmentReferenceDto> departmentLookup,
            Dictionary<string, int> departments,
            ImportOptionsDto options,
            int rowNumber)
        {
            // Check manual mappings first
            if (options.ManualDepartmentMappings?.TryGetValue(departmentText, out var manualDeptId) == true)
            {
                return manualDeptId;
            }

            // Exact match using DTO
            if (departmentLookup.TryGetValue(departmentText.ToLower(), out var exactMatch))
            {
                return exactMatch.DptId;
            }

            // Fuzzy match
            var bestMatch = FuzzySharp.Process.ExtractOne(
                departmentText,
                departmentLookup.Keys,
                cutoff: options.FuzzyMatchThreshold
            );

            if (bestMatch != null)
            {
                var matched = departmentLookup[bestMatch.Value];
                _logger.LogInformation("Row {Row}: Auto-matched department '{Original}' → '{Matched}'",
                    rowNumber, departmentText, matched.Name);
                return matched.DptId;
            }

            // Auto-create if enabled
            if (options.AutoCreateMissingDepartments)
            {
                var newDeptId = await CreateDepartmentAsync(departmentText);
                departments[departmentText.ToLower()] = newDeptId;
                
                // ✅ Also update DTO lookup
                departmentLookup[departmentText.ToLower()] = new DepartmentReferenceDto
                {
                    DptId = newDeptId,
                    Name = departmentText
                };
                
                departmentsCreated++;
                return newDeptId;
            }

            return null;
        }

        /// <summary>
        /// Detect duplicates using DTO
        /// ✅ FIX: Use FuzzyMatchResultDto instead of anonymous type
        /// </summary>
        private (DuplicateMatch? duplicate, List<DuplicateMatch> all) DetectDuplicates(
            string issueDescription,
            int departmentId,
            DateTime dateReported,
            List<RecentIssueLogDto> recentLogs)
        {
            var duplicates = new List<DuplicateMatch>();

            // Exact match
            var exactMatches = recentLogs
                .Where(log =>
                    log.DepartmentId == departmentId &&
                    log.DateReported.Date == dateReported.Date &&
                    log.IssueDescription.Equals(issueDescription, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var match in exactMatches)
            {
                duplicates.Add(new DuplicateMatch
                {
                    IssLogId = match.IssLogId,
                    MatchScore = 100,
                    MatchReason = "Exact match",
                    IssueDescription = match.IssueDescription,
                    DateReported = match.DateReported
                });
            }

            // ✅ FIX: Fuzzy match with DTO (no anonymous types)
            if (duplicates.Count == 0)
            {
                var fuzzyMatches = recentLogs
                    .Where(log =>
                        log.DepartmentId == departmentId &&
                        Math.Abs((log.DateReported.Date - dateReported.Date).TotalDays) <= 3)
                    .Select(log => _importMapper.CreateFuzzyMatchResult(
                        log,
                        FuzzySharp.Fuzz.Ratio(
                            issueDescription.ToLower(),
                            log.IssueDescription.ToLower())))
                    .Where(x => x.Score >= 85)
                    .OrderByDescending(x => x.Score)
                    .ToList();

                foreach (var match in fuzzyMatches)
                {
                    duplicates.Add(new DuplicateMatch
                    {
                        IssLogId = match.Log.IssLogId,
                        MatchScore = match.Score,
                        MatchReason = $"Similar ({match.Score}% match)",
                        IssueDescription = match.Log.IssueDescription,
                        DateReported = match.Log.DateReported
                    });
                }
            }

            return (duplicates.FirstOrDefault(), duplicates);
        }

        /// <summary>
        /// Handle duplicate based on strategy
        /// </summary>
        private static (DuplicateAction action, string message) HandleDuplicate(
            IssueLogs duplicateLog,
            DuplicateHandlingStrategy strategy,
            int rowNumber,
            string? cause,
            string? resolution,
            string? permanentFix,
            string? status)
        {
            var logId = duplicateLog.IssLogId.ToString()[..8];

            return strategy switch
            {
                DuplicateHandlingStrategy.Skip => (
                    DuplicateAction.Skip,
                    $"Row {rowNumber}: Bỏ qua - Trùng với log {logId}..."
                ),
                DuplicateHandlingStrategy.Update => (
                    DuplicateAction.Update,
                    $"Row {rowNumber}: Cập nhật log {logId}..."
                ),
                DuplicateHandlingStrategy.CreateNew => (
                    DuplicateAction.CreateNew,
                    $"Row {rowNumber}: Tạo mới mặc dù có log tương tự {logId}..."
                ),
                DuplicateHandlingStrategy.Fail => (
                    DuplicateAction.Fail,
                    $"Row {rowNumber}: Trùng với log {logId}... Ngày: {duplicateLog.DateReported:yyyy-MM-dd}"
                ),
                _ => (DuplicateAction.CreateNew, "")
            };
        }

        private async Task<IssueLogs?> FindDuplicateInDbAsync(
            string issueDescription,
            int departmentId,
            DateTime dateReported,
            string operatorText)
        {
            // Exact match
            var exactMatch = await _db.IssueLogs
                .Where(il =>
                    il.IssueDescription == issueDescription &&
                    il.DepartmentId == departmentId &&
                    il.DateReported.Date == dateReported.Date &&
                    il.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (exactMatch != null)
            {
                _logger.LogDebug("Found exact duplicate: {LogId}", exactMatch.IssLogId);
                return exactMatch;
            }

            // Near match (within 7 days)
            var nearMatch = await _db.IssueLogs
                .Where(il =>
                    il.IssueDescription == issueDescription &&
                    il.DepartmentId == departmentId &&
                    il.Operator == operatorText &&
                    il.DateReported >= dateReported.AddDays(-7) &&
                    il.DateReported <= dateReported.AddDays(7) &&
                    il.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (nearMatch != null)
            {
                _logger.LogDebug("Found near duplicate: {LogId}", nearMatch.IssLogId);
            }

            return nearMatch;
        }

        private async Task<int> CreateDepartmentAsync(string name)
        {
            var newDept = new Departments
            {
                Name = name,
                Description = $"Auto-created from Excel import on {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
            };

            await _db.Departments.AddAsync(newDept);
            await _db.SaveChangesAsync();

            return newDept.DptId;
        }

        private async Task<int> CreateAreaAsync(string name)
        {
            var newArea = new Areas
            {
                Name = name,
                Description = $"Auto-created from Excel import on {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
            };

            await _db.Areas.AddAsync(newArea);
            await _db.SaveChangesAsync();

            return newArea.AreaId;
        }

        private enum DuplicateAction
        {
            Skip,
            Update,
            CreateNew,
            Fail
        }
    }
}