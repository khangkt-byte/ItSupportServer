using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Configuration; // ✅ ADD THIS
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ItSupportServer.src.Modules.Authentication
{
    /// <summary>
    /// Authentication API
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationsController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthenticationsController> _logger;

        public AuthenticationsController(
            IAuthenticationService authService,
            ILogger<AuthenticationsController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")] // ✅ Apply strict rate limiting
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] // Account locked
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)] // Rate limit
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto, HttpContext);  // ✅ PASS HTTPCONTEXT

                Response.Cookies.Append("refreshToken", result.RefreshToken,
                    CookieOptionsFactory.CreateRefreshTokenCookieOptions());

                return Ok(new { accessToken = result.AccessToken });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(
                    "Failed login attempt for user: {Identifier} | IP: {IP}",
                    dto.Identifier,
                    HttpContext.Connection.RemoteIpAddress
                );
                throw;
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [EnableRateLimiting("api")] // ✅ Apply moderate rate limiting
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken()
        {
            // ✅ Read refresh token from cookie
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized(new { error = "Refresh token không tồn tại" });
            }

            var result = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            }, HttpContext);  // ✅ ADD THIS PARAMETER

            // ✅ USE FACTORY - Consistent cookie configuration
            Response.Cookies.Append("refreshToken", result.RefreshToken,
                CookieOptionsFactory.CreateRefreshTokenCookieOptions());

            return Ok(new { accessToken = result.AccessToken });
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var accountId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                await _authService.LogoutAsync(accountId, refreshToken);
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OtpResponseDto>> ConfirmOtp([FromBody] OtpDto dto)
        {
            // ✅ FIX Bug 1: Pass HttpContext so session metadata (IP, UA, fingerprint) is captured
            var result = await _authService.ConfirmOtpAsync(dto, HttpContext);

            // ✅ USE FACTORY - Consistent cookie configuration
            Response.Cookies.Append("refreshToken", result.Token.RefreshToken,
                CookieOptionsFactory.CreateRefreshTokenCookieOptions());

            return Ok(new { accessToken = result.Token.AccessToken });
        }

        /// <summary>
        /// Yêu cầu OTP mới
        /// </summary>
        [HttpPost("resend-otp")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(OtpSentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OtpSentResponseDto>> ResendOtp([FromBody] string email)
        {
            var result = await _authService.RefreshOtpAsync(email);
            return Ok(result);
        }

        /// <summary>
        /// Quên mật khẩu
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)] // Rate limit (line 336)
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)] // Email service fail
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ForgotPassword([FromBody] string emailOrUsername)
        {
            await _authService.ForgotPasswordAsync(emailOrUsername);
            return Ok(new { message = "Nếu email tồn tại, link đặt lại mật khẩu đã được gửi" });
        }

        /// <summary>
        /// Đặt lại mật khẩu (từ email reset link)
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]  // Validation
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]  // Invalid/expired token
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new { message = "Đặt lại mật khẩu thành công" });
        }
    }
}
