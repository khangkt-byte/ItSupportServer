using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Roles : BaseEntity<int>
    {
        public int RoleId { get; set; }
        public override int Id => RoleId;

        public string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<RoleClaims> RoleClaims { get; set; } = new List<RoleClaims>();
        public ICollection<AccountRoles> AccountRoles { get; set; } = new List<AccountRoles>();
    }
}
