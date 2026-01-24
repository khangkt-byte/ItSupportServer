using ItSupportServer.Data;
using ItSupportServer.src.Modules.User;
using ItSupportServer.src.Shared.Base;
using static ItSupportServer.src.Shared.Base.BaseEnum;
using static ItSupportServer.src.Modules.User.UsersEnum;

namespace ItSupportServer.src.Modules.Employee
{
    public interface IEmployeeService
    {
        Task<BaseResult<PaginatedResult<List<ListEmployeeDto>>>> GetEmployeesAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<DetailUserDto>> GetEmployeeAsync(string Id);
        Task<BaseResult<CreateEmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task<BaseResult<Employees>> UpdateEmployeeAsync(string Id, UpdateEmployeeDto dto);
        Task<BaseResult<STATUS_EMP>> ChangeStatusAsync(string Id, STATUS_EMP status);
        Task<BaseResult<ProfileDto>> GetProfileAsync(string Id);
        Task<BaseResult<ProfileDto>> UpdateProfileAsync(string Id, updateProfileDto dto);

        Task<BaseResult<string>>? ChangeRoleAsync(string Id, ROLE newRole);
    }
}
