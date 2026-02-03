using ClosedXML.Excel;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Issue logs API controller
    /// Pattern: RESTful API design (Roy Fielding)
    /// Security: JWT authentication, permission-based authorization
    /// Reference: ServiceNow Table API, Microsoft Web API guidelines
    /// </summary>
    [ApiController]
    [Route("api/issue-logs")]
    [Produces("application/json")]
    public class IssueLogsController : ControllerBase
    {
        private readonly IIssueLogService _service;
        private readonly IIssueLogImportService _importService;

        public IssueLogsController(IIssueLogService service, IIssueLogImportService importService)
        {
            _service = service;
            _importService = importService;
        }

        // ===== CRUD OPERATIONS =====

        /// <summary>
        /// [GET] Lấy danh sách nhật ký sự cố (có phân trang)
        /// </summary>
        /// <param name="parameters">Query parameters (page, pageSize, sortBy, search)</param>
        /// <returns>Paginated list of issue logs</returns>
        /// <remarks>
        /// **Hỗ trợ:**
        /// - Phân trang: page, pageSize
        /// - Sắp xếp: sortBy, sortDirection
        /// - Tìm kiếm: search (operator, requester, department, issue)
        /// 
        /// **Example:**
        /// ```
        /// GET /api/issue-logs?page=1&amp;pageSize=20&amp;search=máy chủ&amp;sortBy=DateReported
        /// ```
        /// </remarks>
        [HttpGet]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<IssueLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<IssueLogDto>>> GetIssueLogs(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetIssueLogsAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [GET] Lấy chi tiết nhật ký sự cố
        /// </summary>
        /// <param name="issLogId">Issue log ID (GUID)</param>
        /// <returns>Issue log details</returns>
        [HttpGet("{issLogId}")]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IssueLogDto>> GetIssueLog([FromRoute] Guid issLogId)
        {
            var result = await _service.GetIssueLogByIdAsync(issLogId);
            return Ok(result);
        }

        /// <summary>
        /// [POST] Tạo nhật ký sự cố mới
        /// </summary>
        /// <param name="dto">Create issue log request</param>
        /// <returns>Created issue log</returns>
        /// <remarks>
        /// **Tính năng:**
        /// - Auto-match Issue/Cause to knowledge base nếu text trùng khớp
        /// - Validate Department/Area existence
        /// - Verify Cause belongs to Issue (if both provided)
        /// 
        /// **Validation errors (400):**
        /// - Operator, DepartmentId, AreaId, IssueDescription, DateReported: required
        /// - Text fields: length limits, character restrictions
        /// 
        /// **Not Found errors (404):**
        /// - DepartmentId không tồn tại
        /// - AreaId không tồn tại
        /// - IssueId (nếu có) không tồn tại
        /// - CauseId (nếu có) không tồn tại
        /// 
        /// **Business rule errors (422):**
        /// - Cause không thuộc về Issue được chọn
        /// - DateReported trong tương lai
        /// 
        /// **Example:**
        /// ```json
        /// {
        ///   "operator": "Nguyễn Văn A",
        ///   "departmentId": 1,
        ///   "areaId": 1,
        ///   "issueDescription": "Máy chủ không khởi động",
        ///   "dateReported": "2025-02-03"
        /// }
        /// ```
        /// </remarks>
        [HttpPost]
        [HasPermission(Permissions.IssueLogClaims.Create)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IssueLogDto>> CreateIssueLog([FromBody] CreateIssueLogDto dto)
        {
            var result = await _service.CreateIssueLogAsync(dto);
            return CreatedAtAction(nameof(GetIssueLog), new { issLogId = result.IssLogId }, result);
        }

        /// <summary>
        /// [PUT] Cập nhật nhật ký sự cố (partial update)
        /// </summary>
        /// <param name="issLogId">Issue log ID</param>
        /// <param name="dto">Update issue log request (only changed fields)</param>
        /// <returns>Updated issue log</returns>
        /// <remarks>
        /// **Pattern:** PATCH semantics (chỉ update fields có giá trị)
        /// 
        /// **Validation (400):**
        /// - Các fields được cung cấp phải đúng format
        /// - Length limits, character restrictions
        /// 
        /// **Not Found (404):**
        /// - IssueLog không tồn tại
        /// - DepartmentId mới không tồn tại (nếu update)
        /// - AreaId mới không tồn tại (nếu update)
        /// 
        /// **Conflict (409):**
        /// - Update duplicate data (nếu có unique constraints)
        /// 
        /// **Example:**
        /// ```json
        /// {
        ///   "resolution": "Đã thay nguồn mới",
        ///   "permanentFix": "Nâng cấp UPS",
        ///   "status": "Resolved"
        /// }
        /// ```
        /// </remarks>
        [HttpPut("{issLogId}")]
        [HasPermission(Permissions.IssueLogClaims.Edit)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IssueLogDto>> UpdateIssueLog(
            [FromRoute] Guid issLogId,
            [FromBody] UpdateIssueLogDto dto)
        {
            var result = await _service.UpdateIssueLogAsync(issLogId, dto);
            return Ok(result);
        }

        /// <summary>
        /// [DELETE] Xóa nhiều nhật ký sự cố
        /// </summary>
        /// <param name="issLogIds">List of issue log IDs to delete</param>
        /// <param name="softDelete">Soft delete (default: true) or hard delete</param>
        /// <returns>Success status</returns>
        /// <remarks>
        /// **Security:** Soft delete by default (GDPR compliance)
        /// 
        /// **Bulk delete behavior:**
        /// - Transaction-based (all or nothing)
        /// - Nếu ANY ID không tồn tại → 404
        /// - Empty list → 400
        /// 
        /// **Example:**
        /// ```json
        /// ["a1b2c3d4-e5f6-7890-abcd-ef1234567890", "b2c3d4e5-f6a7-8901-bcde-f12345678901"]
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.IssueLogClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeleteIssueLogs(
            [FromBody] List<Guid> issLogIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteIssueLogsAsync(issLogIds, softDelete);
            return Ok(result);
        }

        // ===== IMPORT/EXPORT =====

        /// <summary>
        /// [POST] Step 1: Validate Excel file trước khi import
        /// </summary>
        /// <param name="file">Excel file (.xlsx)</param>
        /// <returns>Validation result với errors, warnings, và preview</returns>
        /// <remarks>
        /// **Two-step import process (ServiceNow pattern):**
        /// 
        /// Step 1: Validate
        /// - Check Excel format
        /// - Fuzzy match departments (auto-fix typos)
        /// - Detect duplicates
        /// - Return preview of first 100 rows
        /// 
        /// Step 2: Import (với user's decisions)
        /// 
        /// **Format Excel:**
        /// | Người thực hiện | Người yêu cầu | Bộ phận | Công ty | Chi nhánh | Mô tả sự cố | ... |
        /// 
        /// **Error codes:**
        /// - 400: File null, empty, hoặc không phải .xlsx
        /// - 413: File quá lớn (> 10MB)
        /// - 415: File type không hỗ trợ (not Excel)
        /// </remarks>
        [HttpPost("import/validate")]
        [HasPermission(Permissions.IssueLogClaims.Create)]
        [ProducesResponseType(typeof(ImportValidationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<ActionResult<ImportValidationResultDto>> ValidateImport([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file Excel" });
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Chỉ chấp nhận file Excel (.xlsx)" });
            }

            using var stream = file.OpenReadStream();
            var result = await _importService.ValidateImportAsync(stream);

            return Ok(result);
        }

        /// <summary>
        /// [POST] Step 2: Import sau khi user xác nhận validation
        /// </summary>
        /// <param name="file">Excel file (.xlsx)</param>
        /// <param name="optionsJson">Import options (JSON string)</param>
        /// <returns>Import result</returns>
        /// <remarks>
        /// **Import Options:**
        /// ```json
        /// {
        ///   "autoCreateMissingDepartments": true,
        ///   "skipRowsWithErrors": true,
        ///   "fuzzyMatchThreshold": 85,
        ///   "duplicateHandling": "Skip"
        /// }
        /// ```
        /// 
        /// **Duplicate Handling:**
        /// - `Skip`: Bỏ qua duplicates (default)
        /// - `Update`: Cập nhật existing records
        /// - `CreateNew`: Tạo mới anyway
        /// - `Fail`: Fail nếu có duplicate (422)
        /// 
        /// **Error codes:**
        /// - 400: File validation failed
        /// - 413: File too large (> 10MB)
        /// - 415: Unsupported media type
        /// - 422: Business rules violated (duplicate handling, required data)
        /// </remarks>
        [HttpPost("import")]
        [HasPermission(Permissions.IssueLogClaims.Create)]
        [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<ActionResult<ImportResultDto>> ImportFromExcel(
            [FromForm] IFormFile file,
            [FromForm] string? optionsJson = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file Excel" });
            }

            var options = string.IsNullOrWhiteSpace(optionsJson)
                ? new ImportOptionsDto()
                : System.Text.Json.JsonSerializer.Deserialize<ImportOptionsDto>(optionsJson)
                    ?? new ImportOptionsDto();

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportFromExcelAsync(stream, options);

            if (result.HasErrors && !options.SkipRowsWithErrors)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// [GET] Export nhật ký sự cố ra Excel
        /// </summary>
        /// <param name="parameters">Query parameters (filter, search)</param>
        /// <returns>Excel file (.xlsx)</returns>
        /// <remarks>
        /// **Tính năng:**
        /// - Export với filter/search hiện tại
        /// - Format giống Excel template (import-ready)
        /// - Include Department/Area names (not IDs)
        /// 
        /// **Example:**
        /// ```
        /// GET /api/issue-logs/export?search=máy chủ&amp;departmentId=1
        /// ```
        /// 
        /// **Response:**
        /// - Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        /// - Content-Disposition: attachment; filename="IssueLog_20250203_103000.xlsx"
        /// </remarks>
        [HttpGet("export")]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportToExcel([FromQuery] QueryParameters? parameters = null)
        {
            var excelData = await _importService.ExportToExcelAsync(parameters);

            var fileName = $"IssueLog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            return File(excelData,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>
        /// [GET] Download Excel template (for import)
        /// </summary>
        /// <returns>Excel template file</returns>
        /// <remarks>
        /// **Template includes:**
        /// - Header row với tên cột chuẩn
        /// - Example row để hướng dẫn format
        /// - Instructions sheet với hướng dẫn chi tiết
        /// - Data validation comments
        /// 
        /// **Usage:**
        /// 1. Download template
        /// 2. Fill in data theo format
        /// 3. Upload qua `/api/issue-logs/import/validate`
        /// 4. Review validation results
        /// 5. Confirm import qua `/api/issue-logs/import`
        /// </remarks>
        [HttpGet("export/template")]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult DownloadTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");

            // Header
            worksheet.Cell(1, 1).Value = "Người thực hiện";
            worksheet.Cell(1, 2).Value = "Người yêu cầu";
            worksheet.Cell(1, 3).Value = "Bộ phận";
            worksheet.Cell(1, 4).Value = "Công ty";
            worksheet.Cell(1, 5).Value = "Chi nhánh";
            worksheet.Cell(1, 6).Value = "Mô tả sự cố";
            worksheet.Cell(1, 7).Value = "Nguyên nhân";
            worksheet.Cell(1, 8).Value = "Cách xử lý";
            worksheet.Cell(1, 9).Value = "Giải pháp lâu dài";
            worksheet.Cell(1, 10).Value = "Ngày báo cáo";
            worksheet.Cell(1, 11).Value = "Trạng thái";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 11);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

            // Example row
            worksheet.Cell(2, 1).Value = "Nguyễn Văn A, Trần Văn B";
            worksheet.Cell(2, 2).Value = "Phòng IT";
            worksheet.Cell(2, 3).Value = "IT Department";
            worksheet.Cell(2, 4).Value = "X";
            worksheet.Cell(2, 5).Value = "";
            worksheet.Cell(2, 6).Value = "Máy chủ không khởi động được";
            worksheet.Cell(2, 7).Value = "Nguồn hỏng";
            worksheet.Cell(2, 8).Value = "Thay nguồn mới";
            worksheet.Cell(2, 9).Value = "";
            worksheet.Cell(2, 10).Value = DateTime.Now;
            worksheet.Cell(2, 11).Value = "Resolved";

            // Instructions sheet
            var instructionsSheet = workbook.Worksheets.Add("Hướng dẫn");
            instructionsSheet.Cell(1, 1).Value = "HƯỚNG DẪN NHẬP LIỆU";
            instructionsSheet.Cell(1, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(1, 1).Style.Font.FontSize = 14;

            instructionsSheet.Cell(3, 1).Value = "1. Người thực hiện: Tên nhân viên (có thể nhiều người, cách nhau bởi dấu phẩy)";
            instructionsSheet.Cell(4, 1).Value = "2. Người yêu cầu: Tên người/phòng ban yêu cầu";
            instructionsSheet.Cell(5, 1).Value = "3. Bộ phận: Tên bộ phận (phải khớp với danh sách trong hệ thống)";
            instructionsSheet.Cell(6, 1).Value = "4. Công ty/Chi nhánh: Đánh dấu X vào 1 trong 2 cột";
            instructionsSheet.Cell(7, 1).Value = "5. Mô tả sự cố: Mô tả chi tiết vấn đề";
            instructionsSheet.Cell(8, 1).Value = "6. Ngày báo cáo: Format: YYYY-MM-DD hoặc DD/MM/YYYY";

            instructionsSheet.Columns().AdjustToContents();
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "IssueLog_Template.xlsx");
        }
    }
}
