using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Employee
{
    public interface IEmployeeService
    {
        /// <summary>
        /// Get paginated list of employees
        /// </summary>
        Task<PaginatedResult<ListEmployeeDto>> GetEmployeesAsync(QueryParameters parameters);

        /// <summary>
        /// Get employee by ID with full details including roles
        /// </summary>
        Task<DetailEmployeeDto> GetEmployeeByIdAsync(Guid empId);

        /// <summary>
        /// Create new employee
        /// </summary>
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);

        /// <summary>
        /// Update existing employee (partial update supported)
        /// </summary>
        Task<EmployeeDto> UpdateEmployeeAsync(Guid empId, UpdateEmployeeDto dto);

        /// <summary>
        /// Delete single employee (soft delete by default)
        /// Pattern: RESTful single resource delete
        /// Reference: Microsoft REST API Guidelines
        /// Business Rules: Cannot delete Super_Admin, cannot delete if has IssueLogs
        /// </summary>
        Task DeleteEmployeeAsync(Guid empId, bool softDelete = true);

        /// <summary>
        /// Delete multiple employees (all-or-nothing transaction)
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        Task<BulkDeleteResultDto> DeleteEmployeesAsync(List<Guid> empIds, bool softDelete = true);

        /// <summary>
        /// Get current user's profile
        /// </summary>
        Task<ProfileDto> GetProfileAsync(Guid empId);

        /// <summary>
        /// Update current user's profile (self-service)
        /// </summary>
        Task<ProfileDto> UpdateProfileAsync(Guid empId, UpdateProfileDto dto);

        /// <summary>
        /// Assign roles to employee account
        /// </summary>
        Task<DetailEmployeeDto> AssignRolesToEmployeeAsync(Guid empId, List<int> roleIds);
    }
}
