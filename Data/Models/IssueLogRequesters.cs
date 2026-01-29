using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    /// <summary>
    /// Junction table: IssueLog ↔ Requesters
    /// Supports both employees and external requesters
    /// </summary>
    [Table("issue_log_requesters")]
    public class IssueLogRequesters
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("iss_log_id")]
        [Required]
        public Guid IssLogId { get; set; }

        [ForeignKey(nameof(IssLogId))]
        public IssueLogs IssueLog { get; set; } = null!;

        // ✅ NULL if requester is not an employee
        [Column("emp_id")]
        public Guid? EmpId { get; set; }

        [ForeignKey(nameof(EmpId))]
        public Employees? Employee { get; set; }

        // ✅ For non-employee requesters or Excel import
        [Column("requester_name")]
        [MaxLength(255)]
        public string? RequesterName { get; set; }

        [Column("requester_type")]
        [MaxLength(50)]
        public string? RequesterType { get; set; }  // "Employee", "Department", "External"
    }
}