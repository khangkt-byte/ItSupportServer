using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhaHangApi.EF_Core.Data
{
    [Table("account_claims")]
    [PrimaryKey(nameof(AccountId), nameof(ClaimId))]
    [Index(nameof(AccountId))]
    [Index(nameof(ClaimId))]
    public class AccountClaims
    {
        [Column("account_id")]
        [Required]
        public Guid AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Column("claim_id")]
        [Required]
        public int ClaimId { get; set; }

        [ForeignKey(nameof(ClaimId))]
        public Claims Claim { get; set; }
    }
}
