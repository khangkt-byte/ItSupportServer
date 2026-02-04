using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models.Entities
{
    [Table("account_tokens")]
    [Index(nameof(AccountId))]
    [Index(nameof(ExpiryTime))]  // ✅ For cleanup queries
    public class AccountTokens : IAuditableEntity  // ✅ Implement interface
    {
        [Key]
        [Column("account_token_id")]
        public Guid AccountTokenId { get; set; }

        [Column("account_id")]
        [Required]
        public required Guid AccountId { get; set; }

        [Column("expiry_time")]
        [Required]
        public DateTime ExpiryTime { get; set; }

        /// <summary>
        /// Timestamp when token was revoked (for logout/security)
        /// </summary>
        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Timestamp when token was created
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when token was last updated
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete timestamp
        /// </summary>
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(AccountId))]
        public Accounts Account { get; set; } = null!;
    }
}
