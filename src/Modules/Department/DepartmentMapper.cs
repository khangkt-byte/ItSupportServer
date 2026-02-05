using ItSupportServer.Data.Models.Entities;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Department
{
    /// <summary>
    /// Department mapper using Mapperly (compile-time code generation)
    /// Pattern: Source generator for type-safe mapping
    /// Performance: Zero reflection, AOT-friendly
    /// Reference: AutoMapper, Mapster alternative
    /// </summary>
    [Mapper]
    public partial class DepartmentMapper
    {
        // ===== Entity → DTOs =====

        [MapperIgnoreSource(nameof(Departments.Id))]
        [MapperIgnoreSource(nameof(Departments.Employees))]
        [MapperIgnoreSource(nameof(Departments.IssueLogs))]
        [MapperIgnoreSource(nameof(Departments.DeletedAt))]
        [MapperIgnoreTarget(nameof(DepartmentDto.EmployeeCount))]
        [MapperIgnoreTarget(nameof(DepartmentDto.IssueLogCount))]
        public partial DepartmentDto MapToDepartmentDto(Departments department);

        [MapperIgnoreSource(nameof(Departments.Id))]
        [MapperIgnoreSource(nameof(Departments.Employees))]
        [MapperIgnoreSource(nameof(Departments.IssueLogs))]
        [MapperIgnoreSource(nameof(Departments.DeletedAt))]
        [MapperIgnoreSource(nameof(Departments.Description))]
        [MapperIgnoreSource(nameof(Departments.CreatedAt))]
        [MapperIgnoreSource(nameof(Departments.UpdatedAt))]
        [MapperIgnoreTarget(nameof(DepartmentSuggestionDto.EmployeeCount))]
        public partial DepartmentSuggestionDto MapToSuggestionDto(Departments department);

        // ===== DTOs → Entity =====

        [MapperIgnoreTarget(nameof(Departments.DptId))]  // Auto-increment
        [MapperIgnoreTarget(nameof(Departments.Id))]
        [MapperIgnoreTarget(nameof(Departments.CreatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Departments.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Departments.DeletedAt))]
        [MapperIgnoreTarget(nameof(Departments.Employees))]
        [MapperIgnoreTarget(nameof(Departments.IssueLogs))]
        public partial Departments MapToDepartment(CreateDepartmentDto dto);

        // ===== Projections (for EF Core IQueryable) =====

        public partial IQueryable<DepartmentDto> ProjectToDepartmentDto(IQueryable<Departments> query);
        public partial IQueryable<DepartmentSuggestionDto> ProjectToSuggestionDto(IQueryable<Departments> query);
    }
}