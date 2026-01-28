using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Authentication
{
    public interface IAuthenticationService
    {
        Task<TokenResponseDto> LoginAsync(LoginDto dto);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto req);
        Task<OtpResponseDto> ConfirmOtpAsync(OtpDto dto);
        Task<OtpSentResponseDto> RefreshOtpAsync(string email);
        Task<bool> ForgotPasswordAsync(string emailOrUsername);
        Task<bool> LogoutAsync(Guid accountId, string refreshToken);
    }
}
