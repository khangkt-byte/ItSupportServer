using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Account
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController(IAccountsService service) : ControllerBase
    {

    }
}
