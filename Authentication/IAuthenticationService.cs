using NhaHangApi.src.Shared.Base;

namespace NhaHangApi.src.Modules.Authentication
{
    public interface IAuthenticationService
    {
        Task<BaseResult<TokenResponseDto>?> LoginAsync(LoginDto dto);
        Task<BaseResult<TokenResponseDto>?> RefreshTokenAsync(RefreshTokenRequestDto req);
        Task<BaseResult<TokenResponseDto>>? LoginWithGG(GoogleAuthDto dto);
        Task<BaseResult<TokenResponseDto>> RegisterGGAsync(GoogleAuthDto dto);
        Task<BaseResult<TokenResponseDto>> LinkGGAccoung(GoogleAuthDto dto, Guid UserId);
        Task<BaseResult<TokenResponseDto>?> ConfirmOtp(OtpDto dto);
        Task<BaseResult<TokenResponseDto>?> RefreshOtp(string email);
        Task<BaseResult<bool>> ForgotPassword(string EmailOrUserName);
    }
}
