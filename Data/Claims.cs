using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSupportServer.Data
{
    [Table("claims")]
    [Index(nameof(Claim), IsUnique = true)]
    public class Claims
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("claim")]
        [Required]
        public string Claim { get; set; }

        [Column("category")]
        public string? Category { get; set; }

        public ICollection<RoleClaims> RoleClaims { get; set; } = new List<RoleClaims>();
        public ICollection<AccountClaims> AccountClaims { get; set; } = new List<AccountClaims>();
    }
}
