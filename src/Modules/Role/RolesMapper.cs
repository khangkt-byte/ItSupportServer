using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Role
{
    [Mapper]
    public partial class RolesMapper
    {
        [MapperIgnoreSource(nameof(Claims.AccountClaims))]
        [MapperIgnoreSource(nameof(Claims.RoleClaims))]
        private partial ClaimDto MapToClaimDto(Claims claim);

        [MapperIgnoreSource(nameof(Roles.Id))]
        [MapperIgnoreSource(nameof(Roles.RoleClaims))]
        [MapperIgnoreSource(nameof(Roles.AccountRoles))]
        [MapperIgnoreSource(nameof(Roles.CreatedAt))]
        [MapperIgnoreSource(nameof(Roles.UpdatedAt))]
        [MapperIgnoreSource(nameof(Roles.DeletedAt))]
        [MapperIgnoreTarget(nameof(RolesDto.Claims))]
        public partial RolesDto MapToRolesDto(Roles role);

        public partial IQueryable<RolesDto> ProjectToRolesDto(IQueryable<Roles> query);
        public partial IQueryable<ClaimDto> ProjectToClaimDto(IQueryable<Claims> query);
    }
}
