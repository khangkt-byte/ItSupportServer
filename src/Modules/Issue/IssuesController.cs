using FluentValidation;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Attributes;
using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Issue
{
    [Route("api/issues")]
    [ApiController]
    public class IssuesController(IIssuesService service) : ControllerBase
    {
        [HttpGet]
        [HasPermission(Permissions.IssueClaims.View)]
        public async Task<IActionResult> GetIssuesAsync(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null)
        {
            var result = await service.GetIssuesAsync(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet("{issueId}")]
        [HasPermission(Permissions.IssueClaims.View)]
        public async Task<IActionResult> GetIssueByIdAsync([FromRoute] long issueId)
        {
            var result = await service.GetIssueByIdAsync(issueId);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        [HasPermission(Permissions.IssueClaims.Create)]
        public async Task<IActionResult> CreateIssueAsync(
            [FromBody] CreateIssueDto dto,
            [FromServices] IValidator<CreateIssueDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.CreateIssueAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut]
        [HasPermission(Permissions.IssueClaims.Edit)]
        public async Task<IActionResult> UpdateIssueAsync(
            [FromBody] UpdateIssueDto dto,
            [FromServices] IValidator<UpdateIssueDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var result = await service.UpdateIssueAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete]
        [HasPermission(Permissions.IssueClaims.Delete)]
        public async Task<IActionResult> DeleteIssueAsync([FromBody] List<long> issueIds)
        {
            var result = await service.DeleteIssuesAsync(issueIds);
            return this.MyStatusCode(result);
        }
    }
}
