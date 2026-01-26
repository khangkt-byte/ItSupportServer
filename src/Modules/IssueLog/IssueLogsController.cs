using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.IssueLog
{
    [Route("api/issue-logs")]
    [ApiController]
    public class IssueLogsController(IIssueLogsService service) : ControllerBase
    {
        [HttpGet]
        [HasPermission(Permissions.IssueLogs.View)]
        public async Task<IActionResult> GetIssueLogsAsync(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null
            )
        {
            var result = await service.GetIssueLogsAsync(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet("{issLogId}")]
        [HasPermission(Permissions.IssueLogs.View)]
        public async Task<IActionResult> GetIssueLogByIdAsync([FromRoute] Guid issLogId)
        {
            var result = await service.GetIssueLogByIdAsync(issLogId);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        [HasPermission(Permissions.IssueLogs.Create)]
        public async Task<IActionResult> CreateIssueLogAsync(
            [FromBody] CreateIssueLogDto dto,
            [FromServices] IValidator<CreateIssueLogDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.CreateIssueLogAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut("{issLogId}")]
        [HasPermission(Permissions.IssueLogs.Edit)]
        public async Task<IActionResult> UpdateIssueLogAsync([FromRoute] Guid issLogId, [FromBody] UpdateIssueLogDto dto)
        {
            var result = await service.UpdateIssueLogAsync(issLogId, dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete]
        [HasPermission(Permissions.IssueLogs.Delete)]
        public async Task<IActionResult> DeleteIssueLogsAsync([FromBody] List<Guid> issLogId)
        {
            var result = await service.DeleteIssueLogsAsync(issLogId);
            return this.MyStatusCode(result);
        }
    }
}
