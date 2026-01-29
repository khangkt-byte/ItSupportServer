using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Issue
{
    /// <summary>
    /// API quản lý vấn đề phổ biến (Knowledge Base)
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<IssueDto>> UpdateIssue(
            [FromRoute] long issId,
            [FromBody] UpdateIssueDto dto)
        {
            var result = await _service.UpdateIssueAsync(issId, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều issues
        /// </summary>
        [HttpDelete]
        [HasPermission(Permissions.IssueClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<bool>> DeleteIssues(
            [FromBody] List<long> issIds,
            [FromQuery] bool softDelete = true)
        {
            var result = await _service.DeleteIssuesAsync(issIds, softDelete);
            return Ok(result);
        }

        /// <summary>
        /// [HELPER] Lấy gợi ý issues cho autocomplete (dùng bởi IssueLog form)
        /// </summary>
        /// <param name="search">Search term</param>
        /// <returns>Top 10 most used issues matching search</returns>
        /// <remarks>
        /// Endpoint này được sử dụng bởi IssueLog form để hiển thị autocomplete suggestions
        /// </remarks>
        [HttpGet("suggestions")]
        [HasPermission(Permissions.IssueClaims.View)]
        [ProducesResponseType(typeof(List<IssueSuggestionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<IssueSuggestionDto>>> GetIssueSuggestions(
            [FromQuery] string? search = null)
        {
            var result = await _service.GetIssueSuggestionsAsync(search);
            return Ok(result);
        }
    }
}
