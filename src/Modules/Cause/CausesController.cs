using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Cause
{
    /// <summary>
    /// API quản lý nguyên nhân sự cố (Knowledge Base)
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
        [HttpGet("by-issue/{issId}")]
        [HasPermission(Permissions.CauseClaims.View)]
        [ProducesResponseType(typeof(List<CauseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CauseDto>> UpdateCause(
            [FromRoute] long causeId,
            [FromBody] UpdateCauseDto dto)
        {
            var result = await _service.UpdateCauseAsync(causeId, dto);
            return Ok(result);
        }

        /// <summary>
        /// [ADMIN] Xóa nhiều nguyên nhân
        /// </summary>
        [HttpDelete]
        [HasPermission(Permissions.CauseClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<bool>> DeleteCauses(
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
        /// Endpoint này được sử dụng bởi IssueLog form để hiển thị cause suggestions
        /// sau khi user chọn issue từ knowledge base
        /// </remarks>
        [HttpGet("suggestions")]
        [HasPermission(Permissions.CauseClaims.View)]
        [ProducesResponseType(typeof(List<CauseSuggestionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CauseSuggestionDto>>> GetCauseSuggestions(
            [FromQuery] long issId,
            [FromQuery] string? search = null)
        {
            var result = await _service.GetCauseSuggestionsAsync(issId, search);
            return Ok(result);
        }
    }
}