using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhaHangApi.EF_Core.Data
{
    [Table("account_roles")]
    [PrimaryKey(nameof(AccountId), nameof(RoleId))]
    [Index(nameof(AccountId))]
    [Index(nameof(RoleId))]
    public class AccountRole
    {
        [Column("account_id")]
        [Required]
        public Guid AccountId { get; set; }
        
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Column("role_id")]
        [Required]
        public string RoleId { get; set; }
        
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }
}
