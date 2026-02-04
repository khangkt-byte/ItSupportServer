using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Issue
{
    public record IssueDto(
        string Name,
        string? Description
        );

    public record CreateIssueDto(
        string Name,
        string? Description
        );

    public record UpdateIssueDto(
        long IssId,
        string? Name,
        string? Description
        );
}
