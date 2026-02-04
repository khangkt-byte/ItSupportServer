using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("claims")]
    [Index(nameof(Claim), IsUnique = true)]
    public class Claims
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("claim_id")]
        [Required]
        public int ClaimId { get; set; }

        [Column("claim")]
        [Required]
        required public string Claim { get; set; }

        [Column("category")]
        public string? Category { get; set; }

        public ICollection<RoleClaims> RoleClaims { get; set; } = new List<RoleClaims>();
        public ICollection<AccountClaims> AccountClaims { get; set; } = new List<AccountClaims>();
    }
}
