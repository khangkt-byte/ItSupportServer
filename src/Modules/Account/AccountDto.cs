using ItSupportServer.src.Modules.Role;

namespace ItSupportServer.src.Modules.Account
{
    /// <summary>
    /// Account response DTO
    /// </summary>
    public record AccountDto
    {
        public Guid AccountId { get; init; }
        public required string Username { get; init; }
        public required string EmpName { get; init; }
        public string? EmpCode { get; init; }
        public string? Email { get; init; }
        public string? Position { get; init; }
        public bool IsLocked { get; init; }
        public int FailedLoginAttempts { get; init; }
        public DateTime? LastLoginAt { get; init; }
        public DateTime? LockedUntil { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<RoleDto>? Roles { get; init; }
        public List<ClaimDto>? Claims { get; init; }
    }

    /// <summary>
    /// List view of account (minimal info)
    /// </summary>
    public record ListAccountDto
    {
        public Guid AccountId { get; init; }
        public required string Username { get; init; }
        public required string EmpName { get; init; }
        public string? EmpCode { get; init; }
        public bool IsLocked { get; init; }
        public DateTime? LastLoginAt { get; init; }
        public DateTime CreatedAt { get; init; }
        // Total number of distinct claims available to the account
        // (includes claims from assigned roles and direct account claims)
        public int TotalClaims { get; init; }
    }

    /// <summary>
    /// Create account request DTO
    /// </summary>
    public record CreateAccountDto
    {
        /// <summary>
        /// Employee ID to create account for
        /// </summary>
        public required Guid EmpId { get; init; }

        /// <summary>
        /// Username for login (unique, 3-32 characters)
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// Initial password (min 6 characters)
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// Optional roles to assign
        /// </summary>
        public List<int>? RoleIds { get; init; }
    }

    /// <summary>
    /// Update account request DTO
    /// </summary>
    public record UpdateAccountDto
    {
        public string? Username { get; init; }
        public bool? IsLocked { get; init; }
    }

    /// <summary>
    /// Change password request DTO (self-service)
    /// </summary>
    public record ChangePasswordDto
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
        public required string ConfirmPassword { get; init; }
    }

    /// <summary>
    /// Reset password result DTO (admin action)
    /// </summary>
    public record ResetPasswordResultDto
    {
        public required string TemporaryPassword { get; init; }
        public required string Message { get; init; }
    }

    /// <summary>
    /// Login history entry DTO
    /// </summary>
    public record LoginHistoryDto
    {
        public DateTime LoginAt { get; init; }
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public bool Success { get; init; }
        public string? FailureReason { get; init; }
    }

    /// <summary>
    /// Unlock account request DTO
    /// </summary>
    public record UnlockAccountDto
    {
        public required Guid AccountId { get; init; }
        public string? Reason { get; init; }
    }
}
