using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    /// <summary>
    /// Issue log entity - stores IT support incident records
    /// Pattern: Simple strings for operators/requesters (Excel compatible)
    /// References: ServiceNow incident table design
    /// </summary>
    public class IssueLogs : BaseEntity<Guid>
    {
        public Guid IssLogId { get; set; }
        public override Guid Id => IssLogId;

        // ===== Operators & Requesters (String - Excel Compatible) =====

        /// <summary>
        /// Comma-separated operator names from Excel
        /// Example: "Nguyễn Văn A, Trần Văn B"
        /// Pattern: ServiceNow - assigned_to field
        /// </summary>
        public required string Operator { get; set; }

        /// <summary>
        /// Comma-separated requester names/departments
        /// Example: "Phòng IT, Nguyễn Văn C"
        /// Pattern: ServiceNow - caller_id field
        /// </summary>
        public string? Requester { get; set; }

        // ===== Department & Area (Validated FKs) =====

        /// <summary>
        /// Department foreign key (validated)
        /// Pattern: Jira - department reference
        /// </summary>
        public int DepartmentId { get; set; }
        public Departments Department { get; set; } = null!;

        /// <summary>
        /// Area/Location foreign key (validated)
        /// Pattern: ServiceNow - location reference
        /// </summary>
        public int AreaId { get; set; }
        public Areas Area { get; set; } = null!;

        // ===== Issue (Knowledge Base Reference - OPTIONAL) =====

        /// <summary>
        /// Optional reference to knowledge base issue
        /// Pattern: ServiceNow - problem_id (nullable)
        /// </summary>
        public long? IssueId { get; set; }
        public Issues? Issue { get; set; }

        /// <summary>
        /// Actual issue description (always required, free text)
        /// Can be selected from KB or custom text
        /// Pattern: ServiceNow - short_description (required)
        /// </summary>
        public required string IssueDescription { get; set; }

        // ===== Cause (Knowledge Base Reference - OPTIONAL) =====

        /// <summary>
        /// Optional reference to knowledge base cause
        /// Pattern: ServiceNow - root_cause_id (nullable)
        /// </summary>
        public long? CauseId { get; set; }
        public Causes? CauseRef { get; set; }

        /// <summary>
        /// Actual cause description (optional, free text)
        /// Pattern: ServiceNow - close_notes
        /// </summary>
        public string? Cause { get; set; }

        // ===== Resolution =====

        /// <summary>
        /// How the issue was resolved
        /// Pattern: ServiceNow - resolution_notes
        /// </summary>
        public string? Resolution { get; set; }

        /// <summary>
        /// Permanent fix/prevention measures
        /// Pattern: ServiceNow - work_notes
        /// </summary>
        public string? PermanentFix { get; set; }

        /// <summary>
        /// Additional notes
        /// Pattern: ServiceNow - comments
        /// </summary>
        public string? Notes { get; set; }

        // ===== Metadata =====

        /// <summary>
        /// When the issue was reported
        /// Pattern: ServiceNow - opened_at
        /// </summary>
        public DateTime DateReported { get; set; }

        /// <summary>
        /// Issue status (e.g., "Open", "In Progress", "Resolved")
        /// Pattern: ServiceNow - state
        /// </summary>
        public string? Status { get; set; }

        // ===== Navigation Properties =====

        public ICollection<IssueLogOperators> IssueLogOperators { get; set; } = [];
        public ICollection<IssueLogRequesters> IssueLogRequesters { get; set; } = [];
    }
}
