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
        public int? Severity { get; init; }
        public int UsageCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<CauseDto>? Causes { get; init; }  // Related causes
    }

    /// <summary>
    /// Create Issue request DTO
    /// Pattern: Command DTO (CQRS)
    /// </summary>
    public record CreateIssueDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? Category { get; init; }
        public int? Severity { get; init; }  // ✅ Nullable for optional
    }

    /// <summary>
    /// Update Issue request DTO
    /// Pattern: Partial update DTO (PATCH semantics)
    /// Null = don't update (keep current value)
    /// Empty string = clear value
    /// Value = update to new value
    /// </summary>
    public record UpdateIssueDto
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? Category { get; init; }
        public int? Severity { get; init; }  // ✅ Nullable means optional update
    }

    /// <summary>
    /// Issue suggestion DTO (for autocomplete)
    /// Pattern: Projection DTO (CQRS read model)
    /// </summary>
    public record IssueSuggestionDto
    {
        public long IssId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int UsageCount { get; init; }
        public DateTime? LastUsed { get; init; }
    }
}
