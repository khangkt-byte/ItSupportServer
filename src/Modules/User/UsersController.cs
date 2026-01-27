using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;
using System.Security.Claims;
using ItSupportServer.src.Modules.Employee;
using FluentValidation;

namespace ItSupportServer.src.Modules.User
{
    [Route("api/users")]
    [ApiController]
    public class UsersController(IEmployeesService service) : ControllerBase
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
        public async Task<IActionResult> UpdateAvt(
            [FromForm] UpdateProfileDto dto,
            [FromServices] IValidator<UpdateProfileDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await service.UpdateProfileAsync(UserId, dto);
            return this.MyStatusCode(result);
        }
    }
}
