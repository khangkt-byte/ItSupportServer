using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;
using System.Security.Claims;
using ItSupportServer.src.Modules.Employee;

namespace ItSupportServer.src.Modules.User
{
    [Route("api/users")]
    [ApiController]
    public class UsersController(IEmployeeService service) : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileAsync()
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await service.GetProfileAsync(UserId);
            return this.MyStatusCode(result);
        }

        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAvt([FromForm] updateProfileDto dto)
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await service.UpdateProfileAsync(UserId, dto);
            return this.MyStatusCode(result);
        }
    }
}
