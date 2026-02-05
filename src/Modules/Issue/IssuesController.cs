using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Issue
{
    /// <summary>
    /// API quản lý vấn đề phổ biến (Knowledge Base)
    /// Pattern: RESTful API, Knowledge Management
    /// Reference: ServiceNow Knowledge Base API
    /// </summary>
    [ApiController]
    [Route("api/issues")]
    [Produces("application/json")]
    public class IssuesController : ControllerBase
    {
        private readonly IIssueService _service;

        public IssuesController(IIssueService service)
        {
            _service = service;
        }

        /// <summary>
        /// [ADMIN] Lấy danh sách issues (có phân trang)
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.IssueClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<IssueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<IssueDto>>> GetIssues(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetIssuesAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy chi tiết issue (bao gồm causes)
        /// </summary>
        [HttpGet("{issId}")]
        [HasPermission(Permissions.IssueClaims.View)]
        [ProducesResponseType(typeof(IssueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IssueDto>> GetIssue([FromRoute] long issId)
        {
            var result = await _service.GetIssueByIdAsync(issId);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Tạo issue mới
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.IssueClaims.Create)]
        [ProducesResponseType(typeof(IssueDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IssueDto>> CreateIssue([FromBody] CreateIssueDto dto)
        {
            var result = await _service.CreateIssueAsync(dto);
            return CreatedAtAction(nameof(GetIssue), new { issId = result.IssId }, result);
        }

        /// <summary>
        /// [ADMIN] Cập nhật issue
        /// </summary>
        [HttpPut("{issId}")]
        [HasPermission(Permissions.IssueClaims.Edit)]
        [ProducesResponseType(typeof(IssueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IssueDto>> UpdateIssue(
            [FromRoute] long issId,
            [FromBody] UpdateIssueDto dto)
        {
            var result = await _service.UpdateIssueAsync(issId, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa issue
        /// </summary>
        /// <param name="issId">Issue ID</param>
        /// <returns>No content (204)</returns>
        /// <remarks>
        /// **Pattern:** RESTful single resource delete
        /// **Reference:** Microsoft REST API Guidelines
        /// 
        /// **Business rules:**
        /// - Không thể xóa nếu issue có causes (422)
        /// - Không thể xóa nếu issue được tham chiếu trong IssueLogs (422)
        /// </remarks>
        [HttpDelete("{issId}")]
        [HasPermission(Permissions.IssueClaims.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteIssue([FromRoute] long issId)
        {
            await _service.DeleteIssueAsync(issId);
            return NoContent();
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều issues
        /// </summary>
        /// <param name="issIds">Danh sách issue IDs cần xóa</param>
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
        /// - Không thể xóa issue có causes (422)
        /// - Không thể xóa issue được tham chiếu trong IssueLogs (422)
        /// - Transaction rollback nếu ANY item fails
        /// 
        /// **Response:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "deletedCount": 3,
        ///   "totalRequested": 3,
        ///   "message": "Đã xóa 3 vấn đề thành công"
        /// }
        /// ```
        /// </remarks>
        [HttpDelete]
        [HasPermission(Permissions.IssueClaims.Delete)]
        [ProducesResponseType(typeof(BulkDeleteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkDeleteResultDto>> DeleteIssues(
            [FromBody] List<long> issIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteIssuesAsync(issIds, softDelete);
            return Ok(result);
        }

        /// <summary>
        /// [HELPER] Lấy gợi ý issues cho autocomplete
        /// </summary>
        [HttpGet("suggestions")]
        [HasPermission(Permissions.IssueClaims.View)]
        [ProducesResponseType(typeof(List<IssueSuggestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<IssueSuggestionDto>>> GetIssueSuggestions(
            [FromQuery] string? search = null)
        {
            var result = await _service.GetIssueSuggestionsAsync(search);
            return Ok(result);
        }
    }
}
