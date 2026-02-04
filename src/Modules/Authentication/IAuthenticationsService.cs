using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Authentication
{
    public interface IAuthenticationsService
    {
        Task<BaseResult<TokenResponseDto>?> LoginAsync(LoginDto dto);
        Task<BaseResult<TokenResponseDto>?> RefreshTokenAsync(RefreshTokenRequestDto req);
        Task<BaseResult<TokenResponseDto>?> ConfirmOtp(OtpDto dto);
        Task<BaseResult<TokenResponseDto>?> RefreshOtp(string email);
        Task<BaseResult<bool>> ForgotPassword(string EmailOrUserName);
    }
}
