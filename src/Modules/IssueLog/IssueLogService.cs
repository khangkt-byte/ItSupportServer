using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Issue log service implementation
    /// Pattern: Domain-Driven Design (DDD) service layer
    /// Security: Input validation, FK verification, SQL injection prevention
    /// Reference: ServiceNow incident management, Microsoft best practices
    /// </summary>
    public class IssueLogService : IIssueLogService
    {
        private readonly AppDbContext _db;
        private readonly IssueLogMapper _mapper;
        private readonly ILogger<IssueLogService> _logger;
        private readonly IValidator<CreateIssueLogDto> _createValidator;
        private readonly IValidator<UpdateIssueLogDto> _updateValidator;

        public IssueLogService(
            AppDbContext db,
            IssueLogMapper mapper,
            ILogger<IssueLogService> logger,
            IValidator<CreateIssueLogDto> createValidator,
            IValidator<UpdateIssueLogDto> updateValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PaginatedResult<IssueLogDto>> GetIssueLogsAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching issue logs with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var query = _mapper.ProjectToIssueLogDto(_db.IssueLogs
                .Include(il => il.Department)
                .Include(il => il.Area)
                .Include(il => il.Issue)
                .Include(il => il.CauseRef)
                .Where(il => il.DeletedAt == null)
                .AsNoTracking());

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(il =>
                    il.Operator.Contains(parameters.Search) ||
                    (il.Requester != null && il.Requester.Contains(parameters.Search)) ||
                    il.DepartmentName.Contains(parameters.Search) ||
                    il.AreaName.Contains(parameters.Search) ||
                    il.IssueDescription.Contains(parameters.Search) ||
                    (il.Resolution != null && il.Resolution.Contains(parameters.Search)));
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "DateReported");

            _logger.LogInformation("Retrieved {Count} issue logs", result.TotalCount);

            return result;
        }

        public async Task<IssueLogDto> GetIssueLogByIdAsync(Guid issLogId)
        {
            _logger.LogInformation("Fetching issue log {IssLogId}", issLogId);

            var issLog = await _mapper.ProjectToIssueLogDto(_db.IssueLogs
                .Include(il => il.Department)
                .Include(il => il.Area)
                .Include(il => il.Issue)
                .Include(il => il.CauseRef)
                .Where(il => il.IssLogId == issLogId && il.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (issLog is null)
            {
                _logger.LogWarning("Issue log {IssLogId} not found", issLogId);
                throw new NotFoundException("Nhật ký sự cố", issLogId);
            }

            return issLog;
        }

        public async Task<IssueLogDto> CreateIssueLogAsync(CreateIssueLogDto dto)
        {
            _logger.LogInformation("Creating new issue log");

            // Step 1: Validate input
            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            // Step 2: Validate Department exists
            var deptExists = await _db.Departments
                .AnyAsync(d => d.DptId == dto.DepartmentId && d.DeletedAt == null);
            
            if (!deptExists)
            {
                throw new NotFoundException("Bộ phận", dto.DepartmentId);
            }

            // Step 3: Validate Area exists
            var areaExists = await _db.Areas
                .AnyAsync(a => a.AreaId == dto.AreaId && a.DeletedAt == null);
            
            if (!areaExists)
            {
                throw new NotFoundException("Khu vực", dto.AreaId);
            }

            // ===== SMART AUTO-MATCHING (ServiceNow pattern) =====
            
            long? resolvedIssueId = dto.IssueId;
            
            // If IssueId not provided, try to auto-match by text
            if (!resolvedIssueId.HasValue && !string.IsNullOrWhiteSpace(dto.IssueDescription))
            {
                resolvedIssueId = await TryAutoMatchIssueAsync(dto.IssueDescription);
                
                if (resolvedIssueId.HasValue)
                {
                    _logger.LogInformation(
                        "Auto-matched issue text '{Text}' to KB Issue {IssueId}",
                        dto.IssueDescription, resolvedIssueId.Value);
                }
            }
            
            long? resolvedCauseId = dto.CauseId;
            
            // Auto-match cause if text matches KB
            if (!resolvedCauseId.HasValue && !string.IsNullOrWhiteSpace(dto.Cause))
            {
                resolvedCauseId = await TryAutoMatchCauseAsync(dto.Cause, resolvedIssueId);
                
                if (resolvedCauseId.HasValue)
                {
                    _logger.LogInformation(
                        "Auto-matched cause text '{Text}' to KB Cause {CauseId}",
                        dto.Cause, resolvedCauseId.Value);
                }
            }

            // Validate resolved IssueId exists
            if (resolvedIssueId.HasValue)
            {
                var issueExists = await _db.Issues
                    .AnyAsync(i => i.IssId == resolvedIssueId.Value && i.DeletedAt == null);
                
                if (!issueExists)
                {
                    _logger.LogWarning("Auto-matched IssueId {IssueId} not found, clearing", resolvedIssueId.Value);
                    resolvedIssueId = null;
                }
            }

            // Validate resolved CauseId exists
            if (resolvedCauseId.HasValue)
            {
                var causeExists = await _db.Causes
                    .AnyAsync(c => c.CauseId == resolvedCauseId.Value && c.DeletedAt == null);
                
                if (!causeExists)
                {
                    _logger.LogWarning("Auto-matched CauseId {CauseId} not found, clearing", resolvedCauseId.Value);
                    resolvedCauseId = null;
                }
                
                // Verify cause belongs to issue
                if (resolvedIssueId.HasValue && resolvedCauseId.HasValue)
                {
                    var causeMatchesIssue = await _db.Causes
                        .AnyAsync(c => c.CauseId == resolvedCauseId.Value && c.IssId == resolvedIssueId.Value);
                    
                    if (!causeMatchesIssue)
                    {
                        _logger.LogWarning(
                            "Cause {CauseId} doesn't match Issue {IssueId}, clearing cause link",
                            resolvedCauseId.Value, resolvedIssueId.Value);
                        resolvedCauseId = null;
                    }
                }
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var newIssueLog = new IssueLogs
                {
                    IssLogId = Guid.CreateVersion7(),
                    Operator = dto.Operator,
                    Requester = dto.Requester,
                    DepartmentId = dto.DepartmentId,
                    AreaId = dto.AreaId,
                    
                    // Set resolved IDs (may be auto-matched)
                    IssueId = resolvedIssueId,
                    IssueDescription = dto.IssueDescription,
                    
                    CauseId = resolvedCauseId,
                    Cause = dto.Cause,
                    
                    Resolution = dto.Resolution,
                    PermanentFix = dto.PermanentFix,
                    Notes = dto.Notes,
                    DateReported = dto.DateReported,
                    Status = dto.Status
                };

                await _db.IssueLogs.AddAsync(newIssueLog);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created issue log {IssLogId}", newIssueLog.IssLogId);

                return await GetIssueLogByIdAsync(newIssueLog.IssLogId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IssueLogDto> UpdateIssueLogAsync(Guid issLogId, UpdateIssueLogDto dto)
        {
            _logger.LogInformation("Updating issue log {IssLogId}", issLogId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var issueLog = await _db.IssueLogs.FindAsync(issLogId);

            if (issueLog is null || issueLog.DeletedAt != null)
            {
                _logger.LogWarning("Issue log {IssLogId} not found", issLogId);
                throw new NotFoundException("Nhật ký sự cố", issLogId);
            }

            bool hasChanges = false;

            // Update Operator
            if (dto.Operator != null && issueLog.Operator != dto.Operator)
            {
                issueLog.Operator = dto.Operator;
                hasChanges = true;
            }

            // Update Requester
            if (dto.Requester != null && issueLog.Requester != dto.Requester)
            {
                issueLog.Requester = string.IsNullOrWhiteSpace(dto.Requester) ? null : dto.Requester;
                hasChanges = true;
            }

            // Update Department (with validation)
            if (dto.DepartmentId.HasValue && issueLog.DepartmentId != dto.DepartmentId.Value)
            {
                var deptExists = await _db.Departments
                    .AnyAsync(d => d.DptId == dto.DepartmentId.Value && d.DeletedAt == null);
                
                if (!deptExists)
                {
                    throw new NotFoundException("Bộ phận", dto.DepartmentId.Value);
                }
                
                issueLog.DepartmentId = dto.DepartmentId.Value;
                hasChanges = true;
            }

            // Update Area (with validation)
            if (dto.AreaId.HasValue && issueLog.AreaId != dto.AreaId.Value)
            {
                var areaExists = await _db.Areas
                    .AnyAsync(a => a.AreaId == dto.AreaId.Value && a.DeletedAt == null);
                
                if (!areaExists)
                {
                    throw new NotFoundException("Khu vực", dto.AreaId.Value);
                }
                
                issueLog.AreaId = dto.AreaId.Value;
                hasChanges = true;
            }

            // Update IssueId (with validation)
            if (dto.IssueId.HasValue && issueLog.IssueId != dto.IssueId.Value)
            {
                var issueExists = await _db.Issues
                    .AnyAsync(i => i.IssId == dto.IssueId.Value && i.DeletedAt == null);
                
                if (!issueExists)
                {
                    throw new NotFoundException("Issue", dto.IssueId.Value);
                }
                
                issueLog.IssueId = dto.IssueId.Value;
                hasChanges = true;
            }

            // Update IssueDescription
            if (dto.IssueDescription != null && issueLog.IssueDescription != dto.IssueDescription)
            {
                issueLog.IssueDescription = dto.IssueDescription;
                hasChanges = true;
            }

            // Update CauseId (with validation)
            if (dto.CauseId.HasValue && issueLog.CauseId != dto.CauseId.Value)
            {
                var causeExists = await _db.Causes
                    .AnyAsync(c => c.CauseId == dto.CauseId.Value && c.DeletedAt == null);
                
                if (!causeExists)
                {
                    throw new NotFoundException("Cause", dto.CauseId.Value);
                }
                
                // Verify cause belongs to issue
                var currentIssueId = dto.IssueId ?? issueLog.IssueId;
                if (currentIssueId.HasValue)
                {
                    var causeMatchesIssue = await _db.Causes
                        .AnyAsync(c => c.CauseId == dto.CauseId.Value && c.IssId == currentIssueId.Value);
                    
                    if (!causeMatchesIssue)
                    {
                        throw new BusinessRuleException(
                            "Nguyên nhân đã chọn không thuộc về issue đã chọn");
                    }
                }
                
                issueLog.CauseId = dto.CauseId.Value;
                hasChanges = true;
            }

            // Update Cause
            if (dto.Cause != null && issueLog.Cause != dto.Cause)
            {
                issueLog.Cause = string.IsNullOrWhiteSpace(dto.Cause) ? null : dto.Cause;
                hasChanges = true;
            }

            // Update Resolution
            if (dto.Resolution != null && issueLog.Resolution != dto.Resolution)
            {
                issueLog.Resolution = string.IsNullOrWhiteSpace(dto.Resolution) ? null : dto.Resolution;
                hasChanges = true;
            }

            // Update PermanentFix
            if (dto.PermanentFix != null && issueLog.PermanentFix != dto.PermanentFix)
            {
                issueLog.PermanentFix = string.IsNullOrWhiteSpace(dto.PermanentFix) ? null : dto.PermanentFix;
                hasChanges = true;
            }

            // Update Notes
            if (dto.Notes != null && issueLog.Notes != dto.Notes)
            {
                issueLog.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
                hasChanges = true;
            }

            // Update DateReported
            if (dto.DateReported.HasValue && issueLog.DateReported != dto.DateReported.Value)
            {
                issueLog.DateReported = dto.DateReported.Value;
                hasChanges = true;
            }

            // Update Status
            if (dto.Status != null && issueLog.Status != dto.Status)
            {
                issueLog.Status = string.IsNullOrWhiteSpace(dto.Status) ? null : dto.Status;
                hasChanges = true;
            }

            if (hasChanges)
            {
                // UpdatedAt set automatically by interceptor
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated issue log {IssLogId}", issLogId);
            }
            else
            {
                _logger.LogInformation("No changes detected for issue log {IssLogId}", issLogId);
            }

            return await GetIssueLogByIdAsync(issLogId);
        }

        public async Task<bool> DeleteIssueLogsAsync(List<Guid> issLogIds, bool softDelete = true)
        {
            _logger.LogInformation("Deleting {Count} issue logs (soft: {SoftDelete})",
                issLogIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(issLogIds);

            if (issLogIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("issLogIds",
                    "Vui lòng chọn nhật ký sự cố để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existing = await _db.IssueLogs
                    .Where(il => issLogIds.Contains(il.IssLogId) && il.DeletedAt == null)
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy nhật ký sự cố để xóa");
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }
                    _db.IssueLogs.UpdateRange(existing);
                }
                else
                {
                    _db.IssueLogs.RemoveRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} issue logs", existing.Count);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ===== AUTO-MATCHING HELPER METHODS =====
        // Pattern: ServiceNow auto-categorization

        /// <summary>
        /// Try to auto-match issue description to KB issue
        /// Pattern: Exact match (case-insensitive) + accent normalization
        /// </summary>
        private async Task<long?> TryAutoMatchIssueAsync(string issueDescription)
        {
            // 1. Exact match (case-insensitive)
            var exactMatch = await _db.Issues
                .Where(i => i.DeletedAt == null)
                .FirstOrDefaultAsync(i => i.Name.ToLower() == issueDescription.ToLower());

            if (exactMatch != null)
                return exactMatch.IssId;

            // 2. Fuzzy match using PostgreSQL trigram
            var normalizedSearch = NormalizeText(issueDescription);

            var fuzzyMatch = await _db.Issues
                .Where(i => i.DeletedAt == null)
                .Where(i =>
                    EF.Functions.ILike(i.Name, $"%{normalizedSearch}%")
                    || EF.Functions.TrigramsSimilarity(i.Name, normalizedSearch) > 0.35
                )
                .OrderByDescending(i => EF.Functions.TrigramsSimilarity(i.Name, normalizedSearch))
                .FirstOrDefaultAsync();

            return fuzzyMatch?.IssId;
        }

        /// <summary>
        /// Try to auto-match cause to KB cause
        /// </summary>
        private async Task<long?> TryAutoMatchCauseAsync(string causeText, long? issueId)
        {
            var query = _db.Causes.Where(c => c.DeletedAt == null);
            
            // If issue is known, only search causes for that issue
            if (issueId.HasValue)
            {
                query = query.Where(c => c.IssId == issueId.Value);
            }
            
            // Exact match
            var exactMatch = await query
                .FirstOrDefaultAsync(c => c.Name.ToLower() == causeText.ToLower());
            
            if (exactMatch != null)
            {
                return exactMatch.CauseId;
            }
            
            // Fuzzy match
            var normalizedSearch = NormalizeText(causeText);
            
            var fuzzyMatch = await query
                .AsEnumerable()
                .FirstOrDefault(c => NormalizeText(c.Name) == normalizedSearch);
            
            return fuzzyMatch?.CauseId;
        }

        /// <summary>
        /// Normalize text for fuzzy matching (remove accents, trim, lowercase)
        /// Pattern: Unicode normalization (W3C standard)
        /// </summary>
        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            
            // Remove Vietnamese accents
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var result = new string(normalized
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != 
                    System.Globalization.CharUnicodeInfo.UnicodeCategory.NonSpacingMark)
                .ToArray());
            
            return result.Normalize(System.Text.NormalizationForm.FormC)
                .Trim()
                .ToLower();
        }
    }
}
