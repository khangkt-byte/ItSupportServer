using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.IssueLog
{
    [Route("api/issue-logs")]
    [ApiController]
    public class IssueLogsController : ControllerBase
    {
        private readonly IIssueLogService _service;

        public IssueLogsController(IIssueLogService service)
        {
            _service = service;
        }

        [HttpGet]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [ProducesResponseType(typeof(PaginatedResult<List<IssueLogDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<List<IssueLogDto>>>> GetIssueLogs(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null)
        {
            var result = await _service.GetIssueLogsAsync(query, page, pageSize, sort);
            return Ok(result);
        }

        [HttpGet("{issLogId}")]
        [HasPermission(Permissions.IssueLogClaims.View)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IssueLogDto>> GetIssueLog(Guid issLogId)
        {
            var result = await _service.GetIssueLogByIdAsync(issLogId);
            return Ok(result);
        }

        [HttpPost]
        [HasPermission(Permissions.IssueLogClaims.Create)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IssueLogDto>> CreateIssueLog([FromBody] CreateIssueLogDto dto)
        {
            var result = await _service.CreateIssueLogAsync(dto);
            return CreatedAtAction(nameof(GetIssueLog), new { issLogId = result.IssLogId }, result);
        }

        [HttpPut("{issLogId}")]
        [HasPermission(Permissions.IssueLogClaims.Edit)]
        [ProducesResponseType(typeof(IssueLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IssueLogDto>> UpdateIssueLog(
            Guid issLogId,
            [FromBody] UpdateIssueLogDto dto)
        {
            var result = await _service.UpdateIssueLogAsync(issLogId, dto);
            return Ok(result);
        }

        [HttpDelete]
        [HasPermission(Permissions.IssueLogClaims.Delete)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> DeleteIssueLogs([FromBody] List<Guid> issLogIds)
        {
            var result = await _service.DeleteIssueLogsAsync(issLogIds);
            return Ok(result);
        }
    }
}
