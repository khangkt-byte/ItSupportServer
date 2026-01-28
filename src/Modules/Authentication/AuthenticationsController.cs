using ItSupportServer.src.Modules.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Authentication
{
    /// <summary>
    /// Authentication API
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _service;

        public AuthenticationController(IAuthenticationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            var result = await _service.LoginAsync(dto);

            // ✅ Set refresh token in HTTP-Only cookie
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = result.AccessToken });
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken()
        {
            // ✅ Read refresh token from cookie
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized(new { error = "Refresh token không tồn tại" });
            }

            var result = await _service.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            });

            // Update cookie
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = result.AccessToken });
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Logout()
        {
            var accountId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                await _service.LogoutAsync(accountId, refreshToken);
            }

            // Clear cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Đăng xuất thành công" });
        }

        /// <summary>
        /// Xác nhận OTP
        /// </summary>
        [HttpPost("confirm-otp")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(OtpResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OtpResponseDto>> ConfirmOtp([FromBody] OtpDto dto)
        {
            var result = await _service.ConfirmOtpAsync(dto);

            // Set refresh token cookie
            Response.Cookies.Append("refreshToken", result.Token.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = result.Token.AccessToken });
        }

        /// <summary>
        /// Yêu cầu OTP mới
        /// </summary>
        [HttpPost("resend-otp")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(OtpSentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<OtpSentResponseDto>> ResendOtp([FromBody] string email)
        {
            var result = await _service.RefreshOtpAsync(email);
            return Ok(result);
        }

        /// <summary>
        /// Quên mật khẩu
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ForgotPassword([FromBody] string emailOrUsername)
        {
            await _service.ForgotPasswordAsync(emailOrUsername);
            return Ok(new { message = "Nếu email tồn tại, mật khẩu mới đã được gửi" });
        }
    }
}
