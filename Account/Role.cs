using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhaHangApi.EF_Core.Data
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public ICollection<RoleClaims> RoleClaims { get; set; } = new List<RoleClaims>();
        public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
        public ICollection<Discounts_Roles>? Discounts_Roles { get; set; } = new List<Discounts_Roles>();
    }
}
