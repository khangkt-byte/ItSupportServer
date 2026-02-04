namespace ItSupportServer.src.Modules.Cause
{
    /// <summary>
    /// Cause response DTO
    /// </summary>
    public record CauseDto
    {
        public long CauseId { get; init; }
        public long IssId { get; init; }
        
        /// <summary>
        /// Issue name - loaded from navigation property
        /// </summary>
        public string? IssueName { get; init; }
        
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int UsageCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// List view of cause (minimal info)
    /// </summary>
    public record ListCauseDto
    {
        public long CauseId { get; init; }
        public long IssId { get; init; }
        public string? IssueName { get; init; }
        
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int UsageCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// Cause suggestion DTO (for IssueLog autocomplete)
    /// </summary>
    public record CauseSuggestionDto
    {
        public long CauseId { get; init; }
        public long IssId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int UsageCount { get; init; }
    }

    /// <summary>
    /// Create cause request DTO
    /// </summary>
    public record CreateCauseDto
    {
        /// <summary>
        /// Issue ID this cause belongs to
        /// </summary>
        public long IssId { get; init; }

        /// <summary>
        /// Cause name (unique per issue)
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Detailed description (optional)
        /// </summary>
        public string? Description { get; init; }
    }

    /// <summary>
    /// Update cause request DTO
    /// </summary>
    public record UpdateCauseDto
    {
        /// <summary>
        /// Updated name (optional)
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Updated description (optional, null = don't update)
        /// </summary>
        public string? Description { get; init; }
    }
}