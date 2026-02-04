using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Role
{
    [Mapper]
    public partial class RolesMapper
    {
        public partial IQueryable<RolesDto> ProjectToRolesDtoPartial(IQueryable<Roles> query);
    }
}
