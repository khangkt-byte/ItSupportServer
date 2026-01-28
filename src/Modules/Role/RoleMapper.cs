using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Role
{
    [Mapper]
    public partial class RoleMapper
    {
        // ===== Entity → DTOs =====

        [MapperIgnoreSource(nameof(Roles.Id))]
        [MapperIgnoreSource(nameof(Roles.RoleClaims))]
        [MapperIgnoreSource(nameof(Roles.AccountRoles))]
        [MapperIgnoreSource(nameof(Roles.DeletedAt))]
        [MapperIgnoreTarget(nameof(RoleDto.Claims))]
        public partial RoleDto MapToRoleDto(Roles role);

        [MapperIgnoreSource(nameof(Claims.AccountClaims))]
        [MapperIgnoreSource(nameof(Claims.RoleClaims))]
        public partial ClaimDto MapToClaimDto(Claims claim);

        // ===== DTOs → Entity =====

        [MapperIgnoreTarget(nameof(Roles.RoleId))]  // Auto-increment
        [MapperIgnoreTarget(nameof(Roles.Id))]
        [MapperIgnoreTarget(nameof(Roles.CreatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Roles.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Roles.DeletedAt))]
        [MapperIgnoreTarget(nameof(Roles.RoleClaims))]  // Handle separately
        [MapperIgnoreTarget(nameof(Roles.AccountRoles))]  // Navigation
        [MapperIgnoreSource(nameof(CreateRoleDto.ClaimIds))]
        public partial Roles MapToRole(CreateRoleDto dto);

        [MapperIgnoreTarget(nameof(Roles.RoleId))]  // Never change
        [MapperIgnoreTarget(nameof(Roles.Id))]
        [MapperIgnoreTarget(nameof(Roles.CreatedAt))]  // Never change
        [MapperIgnoreTarget(nameof(Roles.UpdatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Roles.DeletedAt))]
        [MapperIgnoreTarget(nameof(Roles.RoleClaims))]  // Handle separately
        [MapperIgnoreTarget(nameof(Roles.AccountRoles))]
        [MapperIgnoreSource(nameof(UpdateRoleDto.ClaimIds))]
        public partial void MapToRole(UpdateRoleDto dto, Roles role);

        // ===== Projections =====

        public partial IQueryable<RoleDto> ProjectToRoleDto(IQueryable<Roles> query);
        public partial IQueryable<ClaimDto> ProjectToClaimDto(IQueryable<Claims> query);
    }
}
