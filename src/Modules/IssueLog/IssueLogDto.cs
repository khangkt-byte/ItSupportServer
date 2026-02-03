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
    /// Pattern: ServiceNow incident response
    /// </summary>
    public record IssueLogDto
    {
        public Guid IssLogId { get; init; }
        
        // ===== Operators & Requesters =====
        public required string Operator { get; init; }
        public string? Requester { get; init; }
        
        // ===== Department & Area =====
        public int DepartmentId { get; init; }
        public required string DepartmentName { get; init; }
        
        public int AreaId { get; init; }
        public required string AreaName { get; init; }
        
        // ===== Issue (Knowledge Base - Optional) =====
        public long? IssueId { get; init; }                    // KB reference (nullable)
        public string? IssueName { get; init; }                // KB issue name (if selected)
        public required string IssueDescription { get; init; }  // Actual description (always)
        
        // ===== Cause (Knowledge Base - Optional) =====
        public long? CauseId { get; init; }                    // KB reference (nullable)
        public string? CauseName { get; init; }                // KB cause name (if selected)
        public string? Cause { get; init; }                    // Actual cause text
        
        // ===== Resolution =====
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public string? Notes { get; init; }
        
        // ===== Metadata =====
        public DateOnly DateReported { get; init; }
        public string? Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// Create issue log request DTO
    /// Pattern: ServiceNow incident create request
    /// Security: Validate all FKs, sanitize text inputs
    /// </summary>
    public record CreateIssueLogDto
    {
        // ===== Operators & Requesters =====
        public required string Operator { get; init; }
        public string? Requester { get; init; }
        
        // ===== Department & Area (Validated IDs) =====
        public int DepartmentId { get; init; }
        public int AreaId { get; init; }
        
        // ===== Issue (Flexible: KB ID or free text) =====
        public long? IssueId { get; init; }                    // Optional: KB reference
        public required string IssueDescription { get; init; }  // Required: actual text
        
        // ===== Cause (Flexible: KB ID or free text) =====
        public long? CauseId { get; init; }                    // Optional: KB reference
        public string? Cause { get; init; }                    // Optional: actual text
        
        // ===== Resolution =====
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public string? Notes { get; init; }
        
        // ===== Metadata =====
        public DateOnly DateReported { get; init; }
        public string? Status { get; init; }
    }

    /// <summary>
    /// Update issue log request DTO (partial update supported)
    /// Pattern: ServiceNow incident update (PATCH semantics)
    /// </summary>
    public record UpdateIssueLogDto
    {
        public string? Operator { get; init; }
        public string? Requester { get; init; }
        public int? DepartmentId { get; init; }
        public int? AreaId { get; init; }
        public long? IssueId { get; init; }
        public string? IssueDescription { get; init; }
        public long? CauseId { get; init; }
        public string? Cause { get; init; }
        public string? Resolution { get; init; }
        public string? PermanentFix { get; init; }
        public string? Notes { get; init; }
        public DateOnly? DateReported { get; init; }
        public string? Status { get; init; }
    }
}
