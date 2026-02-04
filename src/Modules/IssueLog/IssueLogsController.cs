using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// API quản lý nhật ký sự cố
    /// </summary>
    [ApiController]
    [Route("api/issue-logs")]
    [Produces("application/json")]
    public class IssueLogsController : ControllerBase
    {
        private readonly IIssueLogService _service;

        public IssueLogsController(IIssueLogService service)
        {
            _service = service;
        }

        /// <summary>
        /// [ADMIN] Lấy danh sách nhật ký sự cố (có phân trang)
        /// </summary>
        /// <param name="parameters">Query parameters (page, pageSize, sortBy, search)</param>
        /// <returns>Paginated list of issue logs</returns>
        [HttpGet]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<IssueLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResult<IssueLogDto>>> GetIssueLogs(
            [FromQuery] QueryParameters parameters)
        {
            var result = await _service.GetIssueLogsAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Lấy chi tiết nhật ký sự cố
        /// </summary>
        /// <param name="issLogId">Issue log ID</param>
        /// <returns>Issue log details</returns>
        [HttpGet("{issLogId}")]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IssueLogDto>> GetIssueLog([FromRoute] Guid issLogId)
        {
            var result = await _service.GetIssueLogByIdAsync(issLogId);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Tạo nhật ký sự cố mới
        /// </summary>
        /// <param name="dto">Create issue log request</param>
        /// <returns>Created issue log</returns>
        [HttpPost]
        [HasPermission(Permissions.IssueLogClaims.Create)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IssueLogDto>> CreateIssueLog([FromBody] CreateIssueLogDto dto)
        {
            var result = await _service.CreateIssueLogAsync(dto);
            return CreatedAtAction(nameof(GetIssueLog), new { issLogId = result.IssLogId }, result);
        }

        /// <summary>
        /// [ADMIN] Cập nhật nhật ký sự cố
        /// </summary>
        /// <param name="issLogId">Issue log ID</param>
        /// <param name="dto">Update issue log request</param>
        /// <returns>Updated issue log</returns>
        [HttpPut("{issLogId}")]
        [HasPermission(Permissions.IssueLogClaims.Edit)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IssueLogDto>> UpdateIssueLog(
            [FromRoute] Guid issLogId,
            [FromBody] UpdateIssueLogDto dto)
        {
            var result = await _service.UpdateIssueLogAsync(issLogId, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều nhật ký sự cố
        /// </summary>
        /// <param name="issLogIds">List of issue log IDs to delete</param>
        /// <param name="softDelete">Soft delete (default: true)</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [HasPermission(Permissions.IssueLogClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> DeleteIssueLogs(
            [FromBody] List<Guid> issLogIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteIssueLogsAsync(issLogIds, softDelete);
            return Ok(result);
        }
    }
}
