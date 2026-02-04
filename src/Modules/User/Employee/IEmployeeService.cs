using ItSupportServer.Data;
using ItSupportServer.src.Modules.User;
using ItSupportServer.src.Shared.Base;
using static ItSupportServer.src.Shared.Base.BaseEnum;
using static ItSupportServer.src.Modules.User.UsersEnum;

namespace ItSupportServer.src.Modules.User.Employee
{
    public interface IEmployeeService
    {
        Task<BaseResult<CreateEmployeeDto>> CreateEmployee(CreateEmployeeDto dto);
        Task<BaseResult<PaginatedResult<List<ListEmployeeDto>>>> GetEmployees(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<DetailUserDto>> GetEmployee(string Id);
        Task<BaseResult<Users>> UpdateEmployee(string Id, UpdateEmployeeDto dto);
        Task<BaseResult<STATUS_EMP>> ChangeStatus(string Id, STATUS_EMP status);
        Task<BaseResult<ProfileDto>> Profile(string Id);
        Task<BaseResult<ProfileDto>> UpdateProfile(string Id, updateProfileDto dto);

        Task<BaseResult<string>>? ChangeRole(string Id, ROLE newRole);
    }
}
