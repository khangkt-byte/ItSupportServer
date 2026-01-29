namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// DTO for recent issue logs (duplicate detection)
    /// Pattern: Projection DTO for specific use case
    /// Performance: Only load needed fields
    /// </summary>
    public record RecentIssueLogDto
    {
        public Guid IssLogId { get; init; }
        public required string Operator { get; init; }
        public int DepartmentId { get; init; }
        public required string IssueDescription { get; init; }
        public DateTime DateReported { get; init; }
    }

    /// <summary>
    /// Duplicate match result
    /// </summary>
    public record DuplicateMatch
    {
        public Guid IssLogId { get; init; }
        public int MatchScore { get; init; }
        public string MatchReason { get; init; } = string.Empty;
        public string? IssueDescription { get; init; }
        public DateTime? DateReported { get; init; }
    }
}