using ItSupportServer.src.Shared.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Issue
{
    [Route("api/issues")]
    [ApiController]
    public class IssuesController(IIssuesService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetIssues(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null)
        {
            var result = await service.GetIssuesAsync(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssueById([FromQuery] string issueId)
        {
            var result = await service.GetIssueByIdAsync(issueId);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIssue([FromBody] IssueCreateDto dto)
        {
            var result = await service.CreateIssueAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateIssue([FromBody] IssueUpdateDto dto)
        {
            var result = await service.UpdateIssueAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteIssue([FromBody] List<string> issueIds)
        {
            var result = await service.DeleteIssueAsync(issueIds);
            return this.MyStatusCode(result);
        }
    }
}
