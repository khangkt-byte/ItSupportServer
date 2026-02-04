using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    /// <summary>
    /// Issue log entity - stores IT support incident records
    /// Pattern: Simple strings for operators/requesters (Excel compatible)
    /// References: ServiceNow incident table design
    /// </summary>
    [Table("issue_logs")]
    public class IssueLogs : BaseEntity<Guid>
    {
        [Key]
        [Column("iss_log_id")]
        public Guid IssLogId { get; set; }
        
        public override Guid Id => IssLogId;

        // ===== Operators & Requesters (String - Excel Compatible) =====

        /// <summary>
        /// Comma-separated operator names from Excel
        /// Example: "Nguyễn Văn A, Trần Văn B"
        /// Pattern: ServiceNow - assigned_to field
        /// </summary>
        [Column("operator")]
        [Required]
        [MaxLength(500)]
        public required string Operator { get; set; }

        /// <summary>
        /// Comma-separated requester names/departments
        /// Example: "Phòng IT, Nguyễn Văn C"
        /// Pattern: ServiceNow - caller_id field
        /// </summary>
        [Column("requester")]
        [MaxLength(500)]
        public string? Requester { get; set; }

        // ===== Department & Area (Validated FKs) =====

        /// <summary>
        /// Department foreign key (validated)
        /// Pattern: Jira - department reference
        /// </summary>
        [Column("department_id")]
        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Departments Department { get; set; } = null!;

        /// <summary>
        /// Area/Location foreign key (validated)
        /// Pattern: ServiceNow - location reference
        /// </summary>
        [Column("area_id")]
        [Required]
        public int AreaId { get; set; }

        [ForeignKey(nameof(AreaId))]
        public Areas Area { get; set; } = null!;

        // ===== Issue (Knowledge Base Reference - OPTIONAL) =====

        /// <summary>
        /// Optional reference to knowledge base issue
        /// Pattern: ServiceNow - problem_id (nullable)
        /// </summary>
        [Column("issue_id")]
        public long? IssueId { get; set; }

        [ForeignKey(nameof(IssueId))]
        public Issues? Issue { get; set; }

        /// <summary>
        /// Actual issue description (always required, free text)
        /// Can be selected from KB or custom text
        /// Pattern: ServiceNow - short_description (required)
        /// </summary>
        [Column("issue_description")]
        [Required]
        [MaxLength(2000)]
        public required string IssueDescription { get; set; }

        // ===== Cause (Knowledge Base Reference - OPTIONAL) =====

        /// <summary>
        /// Optional reference to knowledge base cause
        /// Pattern: ServiceNow - root_cause_id (nullable)
        /// </summary>
        [Column("cause_id")]
        public long? CauseId { get; set; }

        [ForeignKey(nameof(CauseId))]
        public Causes? CauseRef { get; set; }

        /// <summary>
        /// Actual cause description (optional, free text)
        /// Pattern: ServiceNow - close_notes
        /// </summary>
        [Column("cause")]
        [MaxLength(1000)]
        public string? Cause { get; set; }

        // ===== Resolution =====

        /// <summary>
        /// How the issue was resolved
        /// Pattern: ServiceNow - resolution_notes
        /// </summary>
        [Column("resolution")]
        [MaxLength(2000)]
        public string? Resolution { get; set; }

        /// <summary>
        /// Permanent fix/prevention measures
        /// Pattern: ServiceNow - work_notes
        /// </summary>
        [Column("permanent_fix")]
        [MaxLength(2000)]
        public string? PermanentFix { get; set; }

        /// <summary>
        /// Additional notes
        /// Pattern: ServiceNow - comments
        /// </summary>
        [Column("notes")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ===== Metadata =====

        /// <summary>
        /// When the issue was reported
        /// Pattern: ServiceNow - opened_at
        /// </summary>
        [Column("date_reported")]
        [Required]
        public DateTime DateReported { get; set; }

        /// <summary>
        /// Issue status (e.g., "Open", "In Progress", "Resolved")
        /// Pattern: ServiceNow - state
        /// </summary>
        [Column("status")]
        [MaxLength(50)]
        public string? Status { get; set; }

        // ===== Navigation Properties =====

        public ICollection<IssueLogOperators> IssueLogOperators { get; set; } = [];
        public ICollection<IssueLogRequesters> IssueLogRequesters { get; set; } = [];
    }
}
