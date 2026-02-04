using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.IssueLog
{
    //public enum ISSUE_STATUS
    //{
    //    OPEN = 1,
    //    IN_PROGRESS = 2,
    //    RESOLVED = 3,
    //    CLOSED = 4
    //}

    /// <summary>
    /// Issue log response DTO
    /// </summary>
    public record IssueLogDto
    {
        public Guid IssLogId { get; init; }
        public required string Operator { get; init; }
        public string? Requester { get; init; }
        public required string Department { get; init; }
        public required string Area { get; init; }
        public required string IssueDescription { get; init; }
        public string? Cause { get; init; }
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public string? Notes { get; init; }
        public DateTime DateReported { get; init; }
        public string? Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// Create issue log request DTO
    /// </summary>
    public record CreateIssueLogDto
    {
        /// <summary>
        /// Operator name or comma-separated names
        /// </summary>
        public required string Operator { get; init; }

        /// <summary>
        /// Person who requested support
        /// </summary>
        public string? Requester { get; init; }

        /// <summary>
        /// Department name
        /// </summary>
        public required string Department { get; init; }

        /// <summary>
        /// Area name
        /// </summary>
        public required string Area { get; init; }

        /// <summary>
        /// Description of the issue
        /// </summary>
        public required string IssueDescription { get; init; }

        /// <summary>
        /// Root cause of the issue (optional)
        /// </summary>
        public string? Cause { get; init; }

        /// <summary>
        /// How the issue was resolved (optional)
        /// </summary>
        public string? Resolution { get; init; }

        /// <summary>
        /// Permanent fix applied (optional)
        /// </summary>
        public string? PermanentFix { get; init; }

        /// <summary>
        /// Additional notes (optional)
        /// </summary>
        public string? Notes { get; init; }

        /// <summary>
        /// Date when issue was reported
        /// </summary>
        public DateTime DateReported { get; init; }

        /// <summary>
        /// Current status (optional)
        /// </summary>
        public string? Status { get; init; }
    }

    /// <summary>
    /// Update issue log request DTO (partial updates supported)
    /// </summary>
    public record UpdateIssueLogDto
    {
        /// <summary>
        /// Operator name (optional - null means don't update)
        /// </summary>
        public string? Operator { get; init; }

        public string? Requester { get; init; }
        public string? Department { get; init; }
        public string? Area { get; init; }
        public string? IssueDescription { get; init; }
        public string? Cause { get; init; }
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public string? Notes { get; init; }
        public DateTime? DateReported { get; init; }
        public string? Status { get; init; }
    }
}
