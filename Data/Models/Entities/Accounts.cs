using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Accounts : BaseEntity<Guid>
    {
        public Guid AccountId { get; set; }
        public override Guid Id => AccountId;

        public Employees Employee { get; set; } = null!;

        public required string Username { get; set; }

        public required string Password { get; set; }

        // ===== Security Fields (ADD THESE) =====
        
        public bool IsLocked { get; set; } = false;

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LastLoginAt { get; set; }

        public DateTime? LockedUntil { get; set; }

        // ===== Points System =====
        
        public int? CurrentPoints { get; set; }

        public int? LifetimePoints { get; set; }

        // ===== OTP for Password Reset =====
        
        public string? Otp { get; set; }

        public DateTime? ExpiredOtp { get; set; }

        public ICollection<AccountRoles> AccountRoles { get; set; } = new List<AccountRoles>();
        public ICollection<AccountClaims> AccountClaims { get; set; } = new List<AccountClaims>();
        public ICollection<AccountTokens> AccountTokens { get; set; } = new List<AccountTokens>();
    }
}
