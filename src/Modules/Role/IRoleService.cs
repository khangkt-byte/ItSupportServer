using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Role
{
    public interface IRoleService
    {
        /// <summary>
        /// Get paginated list of roles with their claims
        /// </summary>
        Task<PaginatedResult<RoleDto>> GetRolesAsync(QueryParameters parameters);

        /// <summary>
        /// Get role by ID with claims
        /// </summary>
        Task<RoleDto> GetRoleByIdAsync(int roleId);

        /// <summary>
        /// Create new role
        /// </summary>
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);

        /// <summary>
        /// Update existing role (partial update supported)
        /// </summary>
        Task<RoleDto> UpdateRoleAsync(int roleId, UpdateRoleDto dto);

        /// <summary>
        /// Delete role (soft delete by default)
        /// </summary>
        Task<bool> DeleteRoleAsync(int roleId, bool softDelete = true);

        /// <summary>
        /// Delete multiple roles
        /// </summary>
        Task<BulkDeleteResultDto> DeleteRolesAsync(List<int> roleIds, bool softDelete = true);

        /// <summary>
        /// Get all available claims
        /// </summary>
        Task<List<ClaimDto>> GetAllClaimsAsync();

        /// <summary>
        /// Assign roles to an account
        /// </summary>
        Task<AccountRolesDto> AssignRolesToAccountAsync(AssignRolesDto dto);
    }
}
