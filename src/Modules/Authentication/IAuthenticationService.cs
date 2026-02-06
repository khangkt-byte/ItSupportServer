using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Authentication
{
    public interface IAuthenticationService
    {
        Task<TokenResponseDto> LoginAsync(LoginDto dto, HttpContext httpContext);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto req, HttpContext httpContext);
        Task<bool> LogoutAsync(Guid accountId, string refreshToken);
        Task<OtpResponseDto> ConfirmOtpAsync(OtpDto dto);
        Task<OtpSentResponseDto> RefreshOtpAsync(string email);
        
        /// <summary>
        /// Send password reset token via email
        /// Security: OWASP compliant (token-based, not password)
        /// </summary>
        Task<bool> ForgotPasswordAsync(string emailOrUsername);
        
        /// <summary>
        /// Reset password using secure token
        /// Security: One-time use, time-limited token
        /// </summary>
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);  // ✅ ADD THIS
    }
}
