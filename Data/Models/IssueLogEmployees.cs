using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("issue_log_employees")]
    [PrimaryKey(nameof(IssLogId), nameof(EmpId))]
    [Index(nameof(IssLogId))]
    [Index(nameof(EmpId))]
    public class IssueLogEmployees
    {
        [Column("iss_log_id")]
        [Required]
        public Guid IssLogId { get; set; }

        [ForeignKey(nameof(IssLogId))]
        public IssueLogs IssueLog { get; set; }

        [Column("emp_id")]
        [Required]
        public Guid EmpId { get; set; }

        [ForeignKey(nameof(EmpId))]
        public Employees Employee { get; set; }
    }
}
