using ITSupportServer.Base;

namespace ITSupportServer.Authentication
{
    public interface IAuthenticationService
    {
        Task<BaseResult<TokenResponseDto>?> LoginAsync(LoginDto dto);
        Task<BaseResult<TokenResponseDto>?> RefreshTokenAsync(RefreshTokenRequestDto req);
        Task<BaseResult<TokenResponseDto>?> ConfirmOtp(OtpDto dto);
        Task<BaseResult<TokenResponseDto>?> RefreshOtp(string email);
        Task<BaseResult<bool>> ForgotPassword(string EmailOrUserName);
    }
}
