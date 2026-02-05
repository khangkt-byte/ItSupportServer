namespace ItSupportServer.src.Modules.Department
{
    /// <summary>
    /// Department response DTO
    /// </summary>
    public record DepartmentDto
    {
        public int DptId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        
        /// <summary>
        /// Number of employees in this department
        /// </summary>
        public int EmployeeCount { get; init; }
        
        /// <summary>
        /// Number of issue logs from this department
        /// </summary>
        public int IssueLogCount { get; init; }
    }

    /// <summary>
    /// Create department request DTO
    /// </summary>
    public record CreateDepartmentDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
    }

    /// <summary>
    /// Update department request DTO (partial updates supported)
    /// </summary>
    public record UpdateDepartmentDto
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
    }

    /// <summary>
    /// Department suggestion for autocomplete/dropdown
    /// </summary>
    public record DepartmentSuggestionDto
    {
        public int DptId { get; init; }
        public required string Name { get; init; }
        public int EmployeeCount { get; init; }
    }
}