using ItSupportServer.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.src.Modules.IssueLog
{
    //public enum ISSUE_STATUS
    //{
    //    OPEN = 1,
    //    IN_PROGRESS = 2,
    //    RESOLVED = 3,
    //    CLOSED = 4
    //}

    public class IssueLogDto
    {
        public Guid IssLogId { get; set; }
        public List<string> OperatorNames { get; set; }
        public string? Requester { get; set; }
        public int DptId { get; set; }
        public int AreaId { get; set; }
        public string IssueDescription { get; set; } = null!;
        public string? Cause { get; set; }
        public string? Resolution { get; set; }
        public string? PermanentFix { get; set; }
        public string? Notes { get; set; }
        public DateTime DateReported { get; set; }
        public string? Status { get; set; }
    }

    public class IssueLogsCreateDto
    {
        [Required(ErrorMessage = "Người thực hiện là bắt buộc")]
        required public List<Guid> OperatorId { get; set; }
        public string? Requester { get; set; }
        [Required(ErrorMessage = "Bộ phận là bắt buộc")]
        public int DptId { get; set; }
        [Required(ErrorMessage = "Tình trạng lỗi là bắt buộc")]
        public int AreaId { get; set; }
        required public string IssueDescription { get; set; }
        public string? Cause { get; set; }
        public string? Resolution { get; set; }
        public string? PermanentFix { get; set; }
        [Required(ErrorMessage = "Khu vực là bắt buộc")]
        public string? Notes { get; set; }
        [Required(ErrorMessage = "Ngày thực hiện là bắt buộc")]
        public DateTime DateReported { get; set; }
        public string? Status { get; set; }
    }

    public class IssueLogsUpdateDto
    {
        public Guid? OperatorId { get; set; }
        public string? Requester { get; set; }
        public int? DptId { get; set; }
        public int? AreaId { get; set; }
        public string? IssueDescription { get; set; }
        public string? Cause { get; set; }
        public string? Resolution { get; set; }
        public string? PermanentFix { get; set; }
        public string? Notes { get; set; }
        public DateTime? DateReported { get; set; }
        public string? Status { get; set; }
    }
}
