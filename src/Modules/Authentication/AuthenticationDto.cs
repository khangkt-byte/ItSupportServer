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

    /// <summary>
    /// Password reset request DTO
    /// Pattern: OWASP secure password reset
    /// </summary>
    public record ResetPasswordDto
    {
        /// <summary>
        /// Secure reset token (from email link)
        /// </summary>
        public required string Token { get; init; }
        
        /// <summary>
        /// New password (min 8 chars, complexity required)
        /// </summary>
        public required string NewPassword { get; init; }
        
        /// <summary>
        /// Confirm password (must match NewPassword)
        /// </summary>
        public required string ConfirmPassword { get; init; }
    }
}
