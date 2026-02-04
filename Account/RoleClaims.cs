using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhaHangApi.EF_Core.Data
{
    [Table("role_claims")]
    [PrimaryKey(nameof(RoleId), nameof(ClaimId))]
    [Index(nameof(RoleId))]
    [Index(nameof(ClaimId))]
    public class RoleClaims
    {
        [Column("role_id")]
        [Required]
        public string RoleId { get; set; }
        
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [Column("claim_id")]
        [Required]
        public int ClaimId { get; set; }
        
        [ForeignKey(nameof(ClaimId))]
        public Claims Claim { get; set; }
    }
}
