using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("accounts")]
    [Index(nameof(Username), IsUnique = true)]
    public class Accounts : BaseEntity<Guid>
    {
        [Key]
        [Column("user_id")]
        [ForeignKey(nameof(Employee))]
        public Guid AccountId { get; set; }
        
        public override Guid Id => AccountId;

        public Employees Employee { get; set; } = null!;

        [Column("username")]
        [Required, MaxLength(32)]
        public required string Username { get; set; }

        [Column("password")]
        [Required] // BCrypt hash is 60 chars
        public required string Password { get; set; }

        // ===== Security Fields (ADD THESE) =====
        
        [Column("is_locked")]
        public bool IsLocked { get; set; } = false;

        [Column("failed_login_attempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("locked_until")]
        public DateTime? LockedUntil { get; set; }

        // ===== Points System =====
        
        [Column("current_points")]
        public int? CurrentPoints { get; set; }

        [Column("lifetime_points")]
        public int? LifetimePoints { get; set; }

        // ===== OTP for Password Reset =====
        
        [Column("otp")]
        public string? Otp { get; set; }

        [Column("expired_otp")]
        public DateTime? ExpiredOtp { get; set; }

        public ICollection<AccountRoles> AccountRoles { get; set; } = new List<AccountRoles>();
        public ICollection<AccountClaims> AccountClaims { get; set; } = new List<AccountClaims>();
        public ICollection<AccountTokens> AccountTokens { get; set; } = new List<AccountTokens>();
    }
}
