using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    /// <summary>
    /// Password reset tokens
    /// Pattern: Secure password reset (OWASP recommended)
    /// Security: Token hashed before storage, time-limited
    /// Reference: OWASP Authentication Cheat Sheet, NIST SP 800-63B
    /// </summary>
    public class PasswordResetTokens : ICreatableEntity
    {
        /// <summary>
        /// Primary key (UUID v7)
        /// </summary>
        public Guid TokenId { get; set; }

        /// <summary>
        /// Foreign key to Accounts
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Hashed reset token (SHA-256 or BCrypt)
        /// Security: Never store plain text tokens
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// Token expiration time
        /// Security: OWASP recommends 15-60 minutes
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When token was used (null if unused)
        /// Security: Prevent token reuse attacks
        /// </summary>
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Non-secret prefix of the reset token (first 8 chars) used to narrow DB lookup
        /// before BCrypt verification. Avoids O(N×BCrypt) full-table scan.
        /// Security: prefix alone is not sufficient to verify — BCrypt hash is still required.
        /// </summary>
        public string? TokenPrefix { get; set; }

        // ===== NAVIGATION PROPERTIES =====

        /// <summary>
        /// Account this token belongs to
        /// </summary>
        public Accounts Account { get; set; } = null!;
    }
}