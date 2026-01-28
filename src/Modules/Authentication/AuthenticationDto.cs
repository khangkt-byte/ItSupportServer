using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Authentication
{
    public record LoginDto
    {
        public required string Identifier { get; init; }
        public required string Password { get; init; }
    }

    public record TokenResponseDto
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }

    public record RefreshTokenRequestDto
    {
        public required string RefreshToken { get; init; }
    }

    public record OtpDto
    {
        public required string Otp { get; init; }
        public required Guid AccountId { get; init; }
    }

    public record OtpResponseDto
    {
        public required TokenResponseDto Token { get; init; }
    }

    public record OtpSentResponseDto
    {
        public required Guid AccountId { get; init; }
        public required string Message { get; init; }
    }
}
