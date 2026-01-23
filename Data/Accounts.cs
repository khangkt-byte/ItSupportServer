using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("accounts")]
    [Index(nameof(Username), IsUnique = true)]
    public class Accounts : BaseEntity<string>
    {
        [Key]
        [Column("user_id")]
        [ForeignKey(nameof(Employee))]
        [Required]
        required public string AccountId { get; set; }
        public override string Id => AccountId;

        public Employees Employee { get; set; }

        [Column("username")]
        [Required, MaxLength(32)]
        required public string Username { get; set; }

        [Column("password")]
        [Required, MinLength(8)]
        required public string Password { get; set; }

        [Column("current_points")]
        public int? CurrentPoints { get; set; }

        [Column("lifetime_points")]
        public int? LifetimePoints { get; set; }

        [Column("otp")]
        public string? Otp { get; set; }

        [Column("expired_otp")]
        public DateTime? ExpiredOtp { get; set; }

        public ICollection<AccountRoles> AccountRoles { get; set; } = new List<AccountRoles>();
        public ICollection<AccountClaims> AccountClaims { get; set; } = new List<AccountClaims>();
        public ICollection<AccountTokens> AccountTokens { get; set; } = new List<AccountTokens>();
    }
}
