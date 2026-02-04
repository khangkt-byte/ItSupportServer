using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhaHangApi.EF_Core.Data
{
    [Table("accounts")]
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(GoogleId), IsUnique = true)]
    public class Account
    {
        [Key]
        [Column("id_user")]
        [ForeignKey(nameof(User))]
        public Guid IdUser { get; set; }

        public Users User { get; set; }

        [Required, MaxLength(200)]
        [Column("user_name")]
        public string UserName { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("current_points")]
        public int? CurrentPoints { get; set; }

        [Column("lifetime_points")]
        public int? LifetimePoints { get; set; }

        [Column("otp")]
        public string? Otp { get; set; }

        [Column("expries_otp")]
        public DateTime? ExpriesOtp { get; set; }

        [Column("google_id")]
        public string? GoogleId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
        public ICollection<AccountClaims> AccountClaims { get; set; } = new List<AccountClaims>();
        public ICollection<AccountToken> AccountTokens { get; set; } = new List<AccountToken>();
    }
}
