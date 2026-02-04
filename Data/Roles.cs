using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("roles")]
    public class Roles
    {
        [Key]
        [Column("role_id")]
        [Required]
        public string RoleId { get; set; }

        [Column("name")]
        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public ICollection<RoleClaims> RoleClaims { get; set; } = new List<RoleClaims>();
        public ICollection<AccountRoles> AccountRoles { get; set; } = new List<AccountRoles>();
    }
}
