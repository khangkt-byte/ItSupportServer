using Microsoft.AspNetCore.Mvc;
using NhaHangApi.src.Modules.User.Customer;
using NhaHangApi.src.Shared.Base;

namespace NhaHangApi.src.Modules.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IAuthenticationService service, ICustomerService cus) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await service.LoginAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await service.RefreshTokenAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            var result = await cus.RegisterCustomerdto(dto);
            return this.MyStatusCode(result);
        }

        [HttpPost("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleAuthDto dto)
        {
            var result = await service.LoginWithGG(dto);
            return this.MyStatusCode(result);
        }

        [HttpPost("register-google")]
        public async Task<IActionResult> RegisterGoogle([FromBody] GoogleAuthDto dto)
        {
            var result = await service.RegisterGGAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPatch("link-account-google/{Id}")]
        public async Task<IActionResult> LinkAccountGoogle([FromBody] GoogleAuthDto dto, [FromRoute] Guid Id)
        {
            var result = await service.LinkGGAccoung(dto, Id);
            return this.MyStatusCode(result);
        }

        [HttpPost("/confirm-otp")]
        public async Task<IActionResult> ConfirmOtp([FromBody] OtpDto dto)
        {
            var result = await service.ConfirmOtp(dto);
            return this.MyStatusCode(result);
        }

        [HttpPost("/refresh-otp")]
        public async Task<IActionResult> RefreshOtp([FromBody] string email)
        {
            var result = await service.RefreshOtp(email);
            return this.MyStatusCode(result);
        }
        [HttpPost("/forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string EmailOrUserName)
        {
            var result = await service.ForgotPassword(EmailOrUserName);
            return this.MyStatusCode(result);
        }
    }
}
