using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Role
{
    public interface IRolesService
    {
        Task<BaseResult<PaginatedResult<List<RolesDto>>>> GetRolesAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<RolesDto>> GetRoleAsync(int roleId);

        Task<BaseResult<CreateRoleDto>> CreateRoleAsync(CreateRoleDto dto);
        Task<BaseResult<UpdateRoleDto>> UpdateRoleAsync(UpdateRoleDto dto);
        Task<BaseResult<bool>> DeleteRoleAsync(int roleId);
        Task<BaseResult<List<ClaimDto>>> GetAllClaimsAsync();
        Task<BaseResult<AccountRoleResponseDto>> SetRoleAsync(AccountRoleDto dto);
    }
}
