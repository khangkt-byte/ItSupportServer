using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Cause
{
    /// <summary>
    /// API quản lý nguyên nhân sự cố (Knowledge Base)
    /// Pattern: RESTful API, Knowledge Management
    /// Security: JWT + Permission-based authorization
    /// Reference: ServiceNow Knowledge Base API
    /// </summary>
    [ApiController]
    [Route("api/causes")]
    [Produces("application/json")]
    public class CausesController : ControllerBase
    {
        private readonly ICauseService _service;

        public CausesController(ICauseService service)
        {
            _service = service;
        }

        /// <summary>
        /// [ADMIN] Lấy danh sách nguyên nhân (có phân trang)
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.CauseClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<ListCauseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<ListCauseDto>>> GetCauses(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetCausesAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy chi tiết nguyên nhân
        /// </summary>
        [HttpGet("{causeId}")]
        [HasPermission(Permissions.CauseClaims.View)]
        [ProducesResponseType(typeof(CauseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CauseDto>> GetCause([FromRoute] long causeId)
        {
            var result = await _service.GetCauseByIdAsync(causeId);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy danh sách nguyên nhân theo issue ID
        /// </summary>
        /// <param name="issId">Issue ID</param>
        /// <returns>List of causes for the issue</returns>
        /// <remarks>
        /// **Use case:** Populate dropdown khi user chọn Issue trong IssueLog form
        /// 
        /// **Not Found (404):**
        /// - Issue ID không tồn tại
        /// 
        /// **Example:**
        /// ```
        /// GET /api/causes/by-issue/5
        /// ```
        /// </remarks>
        [HttpGet("by-issue/{issId}")]
        [HasPermission(Permissions.CauseClaims.View)]
        [ProducesResponseType(typeof(List<CauseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CauseDto>>> GetCausesByIssue([FromRoute] long issId)
        {
            var result = await _service.GetCausesByIssueIdAsync(issId);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Tạo nguyên nhân mới
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.CauseClaims.Create)]
        [ProducesResponseType(typeof(CauseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CauseDto>> CreateCause([FromBody] CreateCauseDto dto)
        {
            var result = await _service.CreateCauseAsync(dto);
            return CreatedAtAction(nameof(GetCause), new { causeId = result.CauseId }, result);
        }

        /// <summary>
        /// [ADMIN] Cập nhật nguyên nhân
        /// </summary>
        [HttpPut("{causeId}")]
        [HasPermission(Permissions.CauseClaims.Edit)]
        [ProducesResponseType(typeof(CauseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CauseDto>> UpdateCause(
            [FromRoute] long causeId,
            [FromBody] UpdateCauseDto dto)
        {
            var result = await _service.UpdateCauseAsync(causeId, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa nguyên nhân
        /// </summary>
        /// <param name="causeId">Cause ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Pattern:** RESTful single resource delete
        /// **Reference:** Microsoft REST API Guidelines
        /// 
        /// **Business rules:**
        /// - Không thể xóa nếu nguyên nhân đang được tham chiếu trong IssueLog (422)
        /// </remarks>
        [HttpDelete("{causeId}")]
        [HasPermission(Permissions.CauseClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCause([FromRoute] long causeId)
        {
            await _service.DeleteCauseAsync(causeId);
            return NoContent();
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều nguyên nhân
        /// </summary>
        /// <param name="causeIds">Danh sách cause IDs cần xóa</param>
        /// <param name="softDelete">Soft delete (mặc định: true)</param>
        /// <returns>Kết quả xóa hàng loạt</returns>
        /// <remarks>
        /// **Strategy:** All-or-nothing (transaction-based)
        /// - Nếu TẤT CẢ thành công → 200 OK với summary
        /// - Nếu BẤT KỲ lỗi nào → Rollback, throw error (4xx/5xx)
        /// 
        /// **Pattern:** Microsoft Dynamics 365 standard tables
        /// **Reference:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// 
        /// **Business rules:**
        /// - Không thể xóa nguyên nhân đang được tham chiếu trong IssueLog (422)
        /// - Transaction rollback nếu ANY item fails
        /// 
        /// **Response:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "deletedCount": 3,
        ///   "totalRequested": 3,
        ///   "message": "Đã xóa 3 nguyên nhân thành công"
        /// }
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.CauseClaims.Delete)]
        [ProducesResponseType(typeof(BulkDeleteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkDeleteResultDto>> DeleteCauses(
            [FromBody] List<long> causeIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteCausesAsync(causeIds, softDelete);
            return Ok(result);
        }

        /// <summary>
        /// [HELPER] Lấy gợi ý causes cho autocomplete (dùng bởi IssueLog form)
        /// </summary>
        /// <param name="issId">Issue ID</param>
        /// <param name="search">Search term (optional)</param>
        /// <returns>Top 10 most used causes for the issue</returns>
        /// <remarks>
        /// **Use case:**
        /// Endpoint này được sử dụng bởi IssueLog form để hiển thị cause suggestions
        /// sau khi user chọn issue từ knowledge base.
        /// 
        /// **Algorithm:**
        /// - Order by usage count (most used first)
        /// - Filter by search term (if provided)
        /// - Limit 10 results
        /// 
        /// **Not Found (404):**
        /// - Issue ID không tồn tại
        /// 
        /// **Example:**
        /// ```
        /// GET /api/causes/suggestions?issId=5&amp;search=nguồn
        /// ```
        /// 
        /// **Response:**
        /// ```json
        /// [
        ///   { "causeId": 10, "name": "Nguồn điện hỏng", "usageCount": 15 },
        ///   { "causeId": 12, "name": "Nguồn UPS hết pin", "usageCount": 8 }
        /// ]
        /// ```
        /// </remarks>
        [HttpGet("suggestions")]
        [HasPermission(Permissions.CauseClaims.View)]
        [ProducesResponseType(typeof(List<CauseSuggestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CauseSuggestionDto>>> GetCauseSuggestions(
            [FromQuery] long issId,
            [FromQuery] string? search = null)
        {
            var result = await _service.GetCauseSuggestionsAsync(issId, search);
            return Ok(result);
        }
    }
}