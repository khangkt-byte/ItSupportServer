using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Authentication
{
    [Route("api/authentications")]
    [ApiController]
    public class AuthenticationsController(IAuthenticationsService service) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await service.LoginAsync(dto);
            return this.MyStatusCode(result!);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await service.RefreshTokenAsync(dto);
            return this.MyStatusCode(result!);
        }

        [HttpPost("/confirm-otp")]
        public async Task<IActionResult> ConfirmOtp([FromBody] OtpDto dto)
        {
            var result = await service.ConfirmOtp(dto);
            return this.MyStatusCode(result!);
        }

        [HttpPost("/refresh-otp")]
        public async Task<IActionResult> RefreshOtp([FromBody] string email)
        {
            var result = await service.RefreshOtp(email);
            return this.MyStatusCode(result!);
        }
        [HttpPost("/forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string EmailOrUserName)
        {
            var result = await service.ForgotPassword(EmailOrUserName);
            return this.MyStatusCode(result);
        }
    }
}
