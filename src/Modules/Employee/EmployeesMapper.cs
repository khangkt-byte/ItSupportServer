using ItSupportServer.Data.Models;
using ItSupportServer.src.Modules.Role;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Employee
{
    [Mapper]
    public partial class EmployeesMapper
    {
        [UseMapper]
        private readonly RolesMapper _rolesMapper;

        [MapperIgnoreTarget(nameof(Employees.Department))]
        [MapperIgnoreTarget(nameof(Employees.Area))]
        [MapperIgnoreTarget(nameof(Employees.Account))]
        [MapperIgnoreTarget(nameof(Employees.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Employees.DeletedAt))]
        public partial Employees MapToEmployee(CreateEmployeeDto createEmpDto);

        [MapperIgnoreTarget(nameof(Employees.EmpId))]
        [MapperIgnoreTarget(nameof(Employees.Department))]
        [MapperIgnoreTarget(nameof(Employees.Area))]
        [MapperIgnoreTarget(nameof(Employees.Account))]
        [MapperIgnoreTarget(nameof(Employees.CreatedAt))]
        [MapperIgnoreTarget(nameof(Employees.DeletedAt))]
        public partial void MapToEmployee(UpdateEmployeeDto updateEmpDto, Employees employee);

        [MapperIgnoreSource(nameof(Employees.Id))]
        [MapperIgnoreSource(nameof(Employees.DptId))]
        [MapperIgnoreSource(nameof(Employees.Department))]
        [MapperIgnoreSource(nameof(Employees.AreaId))]
        [MapperIgnoreSource(nameof(Employees.Area))]
        [MapperIgnoreSource(nameof(Employees.Account))]
        [MapperIgnoreSource(nameof(Employees.UpdatedAt))]
        [MapperIgnoreSource(nameof(Employees.DeletedAt))]
        private partial ListEmployeeDto MapToListEmployeeDto(Employees employee);

        [MapperIgnoreTarget(nameof(Employees.EmpId))]
        [MapperIgnoreTarget(nameof(Employees.EmpCode))]
        [MapperIgnoreTarget(nameof(Employees.DptId))]
        [MapperIgnoreTarget(nameof(Employees.Department))]
        [MapperIgnoreTarget(nameof(Employees.AreaId))]
        [MapperIgnoreTarget(nameof(Employees.Area))]
        [MapperIgnoreTarget(nameof(Employees.Account))]
        [MapperIgnoreTarget(nameof(Employees.Position))]
        [MapperIgnoreTarget(nameof(Employees.CreatedAt))]
        [MapperIgnoreTarget(nameof(Employees.DeletedAt))]
        public partial void MapToEmployee(UpdateProfileDto updateProfileDto, Employees employee);

        [MapperIgnoreSource(nameof(Employees.Id))]
        [MapperIgnoreSource(nameof(Employees.Department))]
        [MapperIgnoreSource(nameof(Employees.Area))]
        [MapperIgnoreSource(nameof(Employees.Position))]
        [MapperIgnoreSource(nameof(Employees.Account))]
        [MapperIgnoreSource(nameof(Employees.DeletedAt))]
        [MapPropertyFromSource(nameof(DetailUserDto.Roles), Use = nameof(MapRoles))]
        private partial DetailUserDto MapToDetailUserDto(Employees employee);

        private List<RolesDto>? MapRoles(Employees employee)
        {
            if (employee.Account?.AccountRoles == null)
                return null;

            return employee.Account.AccountRoles
                .Select(ar => _rolesMapper.MapToRolesDto(ar.Role))
                .ToList();
        }

        public partial IQueryable<ListEmployeeDto> ProjectToListEmployeeDto(IQueryable<Employees> employees);
        public partial IQueryable<DetailUserDto> ProjectToDetailUserDto(IQueryable<Employees> employees);
    }
}
