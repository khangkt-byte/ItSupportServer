namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// DTO for Excel row data (before mapping to entity)
    /// Purpose: Bridge between Excel parsing and entity mapping
    /// </summary>
    public record ExcelRowDto
    {
        public required string Operator { get; init; }
        public string? Requester { get; init; }
        public int DepartmentId { get; init; }
        public int AreaId { get; init; }
        public required string IssueDescription { get; init; }
        public string? Cause { get; init; }
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public DateTime DateReported { get; init; }
        public string? Status { get; init; }
    }
    
    /// <summary>
    /// DTO for updating existing entity
    /// </summary>
    public record ExcelUpdateDto
    {
        public string? Cause { get; init; }
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public string? Status { get; init; }
    }
}