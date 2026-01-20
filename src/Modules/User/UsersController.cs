using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Modules.User.Customer;
using ItSupportServer.src.Modules.User.Employee;
using ItSupportServer.src.Shared.Base;
using System.Security.Claims;

namespace ItSupportServer.src.Modules.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IEmployeeService sv, ICustomerService csv) : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Profile()
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await sv.Profile(UserId);
            return this.MyStatusCode(result);
        }

        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAvt([FromForm] updateProfileDto dto)
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await sv.UpdateProfile(UserId, dto);
            return this.MyStatusCode(result);
        }
        [Authorize(Roles = RoleUser.GroupEmployee)]
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null)
        {
            var result = await csv.GetCustomers(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }
    }
}
