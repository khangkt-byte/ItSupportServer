using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Issue
{
    /// <summary>
    /// Issue response DTO
    /// </summary>
    public record IssueDto
    {
        public int IssueId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// Create issue request DTO
    /// </summary>
    public record CreateIssueDto
    {        
        public required string Name { get; init; }
        public string? Description { get; init; }
    }

    /// <summary>
    /// Update issue request DTO (partial updates supported)
    /// </summary>
    public record UpdateIssueDto
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
    }
}
