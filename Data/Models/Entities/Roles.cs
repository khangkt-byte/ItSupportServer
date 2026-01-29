using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models.Entities
{
    [Table("roles")]
    public class Roles : BaseEntity<int>
    {
        [Key]
        [Column("role_id")]
        [Required]
        public int RoleId { get; set; }
        public override int Id => RoleId;

        [Column("name")]
        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public ICollection<RoleClaims> RoleClaims { get; set; } = new List<RoleClaims>();
        public ICollection<AccountRoles> AccountRoles { get; set; } = new List<AccountRoles>();
    }
}
