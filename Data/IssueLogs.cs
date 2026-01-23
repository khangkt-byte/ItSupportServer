using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("issue_logs")]
    public class IssueLogs : BaseEntity<long>
    {
        [Key]
        [Column("iss_log_id")]
        [Required]
        public long IssLogId { get; set; }
        public override long Id => IssLogId;

        [Column("operator_id")]
        [Required]
        required public string OperatorId { get; set; }

        [ForeignKey(nameof(OperatorId))]
        public Employees Operator { get; set; }

        [Column("requester_id")]
        public string? RequesterId { get; set; }

        [ForeignKey(nameof(RequesterId))]
        public Employees? Requester { get; set; }

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
    }
}
