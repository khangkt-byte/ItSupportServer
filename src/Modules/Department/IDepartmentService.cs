using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;

namespace ItSupportServer.src.Modules.Department
{
    /// <summary>
    /// Department service interface
    /// Pattern: Repository pattern with business logic
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// Get paginated list of departments
        /// </summary>
        Task<PaginatedResult<DepartmentDto>> GetDepartmentsAsync(QueryParameters parameters);

        /// <summary>
        /// Get department by ID
        /// </summary>
        Task<DepartmentDto> GetDepartmentByIdAsync(int dptId);

        /// <summary>
        /// Get department suggestions for autocomplete
        /// </summary>
        Task<List<DepartmentSuggestionDto>> GetDepartmentSuggestionsAsync(string? search = null);

        /// <summary>
        /// Create new department
        /// </summary>
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto);

        /// <summary>
        /// Update existing department
        /// </summary>
        Task<DepartmentDto> UpdateDepartmentAsync(int dptId, UpdateDepartmentDto dto);

        /// <summary>
        /// Delete single department (soft delete by default)
        /// Pattern: RESTful single resource delete
        /// Reference: Microsoft REST API Guidelines
        /// Business Rules: Cannot delete if has Employees or IssueLogs
        /// </summary>
        Task DeleteDepartmentAsync(int dptId, bool softDelete = true);

        /// <summary>
        /// Delete multiple departments (all-or-nothing transaction)
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        Task<BulkDeleteResultDto> DeleteDepartmentsAsync(List<int> dptIds, bool softDelete = true);
    }
}