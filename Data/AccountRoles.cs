using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("account_roles")]
    [PrimaryKey(nameof(AccountId), nameof(RoleId))]
    [Index(nameof(AccountId))]
    [Index(nameof(RoleId))]
    public class AccountRoles
    {
        [Column("account_id")]
        [Required]
        public string AccountId { get; set; }
        
        [ForeignKey(nameof(AccountId))]
        public Accounts Account { get; set; }

        [Column("role_id")]
        [Required]
        public string RoleId { get; set; }
        
        [ForeignKey(nameof(RoleId))]
        public Roles Role { get; set; }
    }
}
