using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("account_tokens")]
    [Index(nameof(AccountId))]
    public class AccountTokens
    {
        [Key]
        [Column("account_token_id")]
        [Required]
        public Guid AccountTokenId { get; set; }

        [Column("expiry_time")]
        [Required]
        public DateTime ExpiryTime { get; set; }

        [Column("id_account")]
        [Required]
        required public string AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Accounts Account { get; set; }
    }
}
