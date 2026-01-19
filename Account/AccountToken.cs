using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhaHangApi.EF_Core.Data
{
    [Table("account_tokens")]
    [Index(nameof(IdAccount))]
    public class AccountToken
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("expiry_time")]
        public DateTime ExpiryTime { get; set; }

        [Column("id_account")]
        public Guid IdAccount { get; set; }

        [ForeignKey(nameof(IdAccount))]
        public Account Account { get; set; }
    }
}
