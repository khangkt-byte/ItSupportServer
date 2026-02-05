using ItSupportServer.src.Shared.Base;

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
        /// Get department by ID with counts
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
        /// Update existing department (partial update supported)
        /// </summary>
        Task<DepartmentDto> UpdateDepartmentAsync(int dptId, UpdateDepartmentDto dto);

        /// <summary>
        /// Delete department (soft delete by default)
        /// </summary>
        Task<bool> DeleteDepartmentAsync(int dptId, bool softDelete = true);

        /// <summary>
        /// Delete multiple departments
        /// </summary>
        Task<BulkDeleteResultDto> DeleteDepartmentsAsync(List<int> dptIds, bool softDelete = true);
    }

    /// <summary>
    /// Bulk delete result DTO
    /// </summary>
    public record BulkDeleteResultDto
    {
        public bool Success { get; init; }
        public int DeletedCount { get; init; }
        public int TotalRequested { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}