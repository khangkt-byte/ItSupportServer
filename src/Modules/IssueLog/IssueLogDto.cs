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

    public record IssueLogDto(
        Guid IssLogId,
        string Operator,
        string? Requester,
        //int DptId,
        //int AreaId,
        string Department,
        string Area,
        string IssueDescription,
        string? Cause,
        string? Resolution,
        string? PermanentFix,
        string? Notes,
        DateTime DateReported,
        string? Status
        );

    public record CreateIssueLogDto(
        Guid IssLogId,
        string Operator,
        string? Requester,
        //int DptId,
        //int AreaId,
        string Department,
        string Area,
        string IssueDescription,
        string? Cause,
        string? Resolution,
        string? PermanentFix,
        string? Notes,
        DateTime DateReported,
        string? Status,
        DateTime CreatedAt
        );

    public record UpdateIssueLogDto(
        string? Operator,
        string? Requester,
        //int? DptId,
        //int? AreaId,
        string? Department,
        string? Area,
        string? IssueDescription,
        string? Cause,
        string? Resolution,
        string? PermanentFix,
        string? Notes,
        DateTime? DateReported,
        string? Status,
        DateTime UpdatedAt
        );
}
