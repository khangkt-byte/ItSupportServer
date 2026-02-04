using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("issue_logs")]
    public class IssueLogs : BaseEntity<Guid>
    {
        [Key]
        [Column("iss_log_id")]
        [Required]
        public Guid IssLogId { get; set; }
        public override Guid Id => IssLogId;

        [Column("requester")]
        public string? Requester { get; set; }

        [Column("dpt_id")]
        [Required]
        public int DptId { get; set; }

        [ForeignKey(nameof(DptId))]
        public Departments Department { get; set; }

        [Column("issue_description")]
        [Required]
        required public string IssueDescription { get; set; }

        [Column("cause")]
        public string? Cause { get; set; }

        [Column("resolution")]
        public string? Resolution { get; set; }

        [Column("permanent_fix")]
        public string? PermanentFix { get; set; }

        [Column("area_id")]
        [Required]
        public int AreaId { get; set; }

        [ForeignKey(nameof(AreaId))]
        public Areas Area { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("date_reported")]
        [Required]
        public DateTime DateReported { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        public ICollection<IssueLogEmployees> Operators { get; set; } = new List<IssueLogEmployees>();
    }
}
