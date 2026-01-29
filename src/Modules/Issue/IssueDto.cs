using System.ComponentModel.DataAnnotations;
using ItSupportServer.src.Modules.Cause;

namespace ItSupportServer.src.Modules.Issue
{
    /// <summary>
    /// Issue response DTO
    /// </summary>
    public record IssueDto
    {
        public long IssId { get; init; }  // ✅ Match entity type (long)
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? Category { get; init; }
        public int? Severity { get; init; }  // 1-5
        public int UsageCount { get; init; }  // Number of times used in IssueLogs
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<CauseDto>? Causes { get; init; }  // Related causes
    }

    /// <summary>
    /// Issue suggestion DTO (for autocomplete)
    /// </summary>
    public record IssueSuggestionDto
    {
        public long IssId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int UsageCount { get; init; }
        public DateTime? LastUsed { get; init; }
    }

    /// <summary>
    /// Create issue request DTO
    /// </summary>
    public record CreateIssueDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? Category { get; init; }
        public int? Severity { get; init; }  // 1-5
    }

    /// <summary>
    /// Update issue request DTO
    /// </summary>
    public record UpdateIssueDto
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? Category { get; init; }
        public int? Severity { get; init; }
    }
}
