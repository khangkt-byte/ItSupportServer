using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class AccountTokens : IAuditableEntity  // ✅ Implement interface
    {
        public Guid AccountTokenId { get; set; }

        public required Guid AccountId { get; set; }

        public DateTime ExpiryTime { get; set; }

        /// <summary>
        /// Timestamp when token was revoked (for logout/security)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Timestamp when token was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when token was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete timestamp
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        // Navigation property
        public Accounts Account { get; set; } = null!;
    }
}
