using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models.Entities
{
    /// <summary>
    /// Junction table: IssueLog ↔ Operators (many-to-many)
    /// </summary>
    [Table("issue_log_operators")]
    public class IssueLogOperators
    {
        [Column("iss_log_id")]
        [Required]
        public Guid IssLogId { get; set; }

        [ForeignKey(nameof(IssLogId))]
        public IssueLogs IssueLog { get; set; } = null!;

        [Column("emp_id")]
        [Required]
        public Guid EmpId { get; set; }

        [ForeignKey(nameof(EmpId))]
        public Employees Employee { get; set; } = null!;

        [Column("operator_role")]
        [MaxLength(50)]
        public string? OperatorRole { get; set; }  // "Primary", "Assistant"

        [Column("hours_spent")]
        public decimal? HoursSpent { get; set; }  // Track time
    }
}