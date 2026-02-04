using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models.Entities
{
    [Table("account_claims")]
    [PrimaryKey(nameof(AccountId), nameof(ClaimId))]
    [Index(nameof(AccountId))]
    [Index(nameof(ClaimId))]
    public class AccountClaims
    {
        [Column("account_id")]
        [Required]
        required public Guid AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Accounts Account { get; set; }

        [Column("claim_id")]
        [Required]
        public int ClaimId { get; set; }

        [ForeignKey(nameof(ClaimId))]
        public Claims Claim { get; set; }
    }
}
