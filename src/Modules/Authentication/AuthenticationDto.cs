using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Authentication
{
    public record LoginDto(
        string Identifier,
        string Password
        );

    public record TokenResponseDto(
        string? AccessToken = null,
        string? RefreshToken = null,
        Guid? AccountId = null
        );

    public record RefreshTokenRequestDto(
        string RefreshToken
        );

    public record OtpDto(
        string Otp,
        Guid AccountId
        );
}
