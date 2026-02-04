using ItSupportServer.Data.Models.Entities;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Employee
{
    [Mapper]
    public partial class EmployeeMapper
    {
        // ===== Entity → DTOs =====

        [MapperIgnoreSource(nameof(Employees.Id))]
        [MapperIgnoreSource(nameof(Employees.Area))]
        [MapperIgnoreSource(nameof(Employees.Department))]
        [MapperIgnoreSource(nameof(Employees.Account))]
        [MapperIgnoreSource(nameof(Employees.DeletedAt))]
        [MapperIgnoreSource(nameof(Employees.IssueLogOperators))]
        [MapperIgnoreSource(nameof(Employees.IssueLogRequesters))]
        public partial EmployeeDto MapToEmployeeDto(Employees employee);

        [MapperIgnoreSource(nameof(Employees.Id))]
        [MapperIgnoreSource(nameof(Employees.AreaId))]
        [MapperIgnoreSource(nameof(Employees.Area))]
        [MapperIgnoreSource(nameof(Employees.DptId))]
        [MapperIgnoreSource(nameof(Employees.Department))]
        [MapperIgnoreSource(nameof(Employees.Account))]
        [MapperIgnoreSource(nameof(Employees.DeletedAt))]
        [MapperIgnoreSource(nameof(Employees.UpdatedAt))]
        [MapperIgnoreSource(nameof(Employees.IssueLogOperators))]
        [MapperIgnoreSource(nameof(Employees.IssueLogRequesters))]
        public partial ListEmployeeDto MapToListEmployeeDto(Employees employee);

        [MapperIgnoreSource(nameof(Employees.Id))]
        [MapperIgnoreSource(nameof(Employees.Area))]
        [MapperIgnoreSource(nameof(Employees.Department))]
        [MapperIgnoreSource(nameof(Employees.Account))]
        [MapperIgnoreSource(nameof(Employees.DeletedAt))]
        [MapperIgnoreTarget(nameof(DetailEmployeeDto.Roles))]
        [MapperIgnoreSource(nameof(Employees.IssueLogOperators))]
        [MapperIgnoreSource(nameof(Employees.IssueLogRequesters))]
        public partial DetailEmployeeDto MapToDetailEmployeeDto(Employees employee);

        [MapperIgnoreSource(nameof(Employees.Id))]
        [MapperIgnoreSource(nameof(Employees.Area))]
        [MapperIgnoreSource(nameof(Employees.Department))]
        [MapperIgnoreSource(nameof(Employees.Account))]
        [MapperIgnoreSource(nameof(Employees.DeletedAt))]
        [MapperIgnoreTarget(nameof(ProfileDto.Username))]
        [MapperIgnoreSource(nameof(Employees.IssueLogOperators))]
        [MapperIgnoreSource(nameof(Employees.IssueLogRequesters))]
        public partial ProfileDto MapToProfileDto(Employees employee);

        // ===== DTOs → Entity =====

        [MapperIgnoreTarget(nameof(Employees.EmpId))]  // Set in service
        [MapperIgnoreTarget(nameof(Employees.Id))]
        [MapperIgnoreTarget(nameof(Employees.Area))]
        [MapperIgnoreTarget(nameof(Employees.Department))]
        [MapperIgnoreTarget(nameof(Employees.CreatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Employees.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Employees.DeletedAt))]
        [MapperIgnoreTarget(nameof(Employees.Account))]  // Navigation property
        [MapperIgnoreTarget(nameof(Employees.IssueLogOperators))]
        [MapperIgnoreTarget(nameof(Employees.IssueLogRequesters))]
        public partial Employees MapToEmployee(CreateEmployeeDto dto);

        [MapperIgnoreTarget(nameof(Employees.EmpId))]  // Never change
        [MapperIgnoreTarget(nameof(Employees.Id))]
        [MapperIgnoreTarget(nameof(Employees.Area))]
        [MapperIgnoreTarget(nameof(Employees.Department))]
        [MapperIgnoreTarget(nameof(Employees.CreatedAt))]  // Never change
        [MapperIgnoreTarget(nameof(Employees.UpdatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Employees.DeletedAt))]
        [MapperIgnoreTarget(nameof(Employees.Account))]
        [MapperIgnoreTarget(nameof(Employees.IssueLogOperators))]
        [MapperIgnoreTarget(nameof(Employees.IssueLogRequesters))]
        public partial void MapToEmployee(UpdateEmployeeDto dto, Employees employee);

        [MapperIgnoreTarget(nameof(Employees.EmpId))]
        [MapperIgnoreTarget(nameof(Employees.Id))]
        [MapperIgnoreTarget(nameof(Employees.EmpCode))]
        [MapperIgnoreTarget(nameof(Employees.DptId))]
        [MapperIgnoreTarget(nameof(Employees.Department))]
        [MapperIgnoreTarget(nameof(Employees.AreaId))]
        [MapperIgnoreTarget(nameof(Employees.Area))]
        [MapperIgnoreTarget(nameof(Employees.Position))]
        [MapperIgnoreTarget(nameof(Employees.CreatedAt))]
        [MapperIgnoreTarget(nameof(Employees.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Employees.DeletedAt))]
        [MapperIgnoreTarget(nameof(Employees.Account))]
        [MapperIgnoreTarget(nameof(Employees.IssueLogOperators))]
        [MapperIgnoreTarget(nameof(Employees.IssueLogRequesters))]
        public partial void MapToEmployee(UpdateProfileDto dto, Employees employee);

        // ===== Projections for EF Core =====

        public partial IQueryable<ListEmployeeDto> ProjectToListEmployeeDto(IQueryable<Employees> query);
        public partial IQueryable<DetailEmployeeDto> ProjectToDetailEmployeeDto(IQueryable<Employees> query);
        public partial IQueryable<ProfileDto> ProjectToProfileDto(IQueryable<Employees> query);
    }
}
