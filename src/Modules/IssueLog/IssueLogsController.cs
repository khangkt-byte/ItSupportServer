using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.IssueLog
{
    [Route("api/issue-logs")]
    [ApiController]
    public class IssueLogsController(IIssueLogsService service) : ControllerBase
    {

    }
}
