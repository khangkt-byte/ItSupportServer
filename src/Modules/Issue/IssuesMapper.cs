using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Issue
{
    [Mapper]
    public partial class IssuesMapper
    {
        [MapperIgnoreSource(nameof(Issues.Id))]
        [MapperIgnoreSource(nameof(Issues.Causes))]
        [MapperIgnoreSource(nameof(Issues.CreatedAt))]
        [MapperIgnoreSource(nameof(Issues.UpdatedAt))]
        [MapperIgnoreSource(nameof(Issues.DeletedAt))]
        public partial IssueDto MapToIssueDto(Issues issue);

        [MapperIgnoreTarget(nameof(Issues.IssId))]
        [MapperIgnoreTarget(nameof(Issues.Causes))]
        [MapperIgnoreTarget(nameof(Issues.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        public partial Issues MapToIssue(CreateIssueDto issueDto);

        [MapperIgnoreTarget(nameof(Issues.Causes))]
        [MapperIgnoreTarget(nameof(Issues.CreatedAt))]
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        public partial void MapToIssue(UpdateIssueDto issueDto, Issues issue);

        public partial IQueryable<IssueDto> ProjectToIssueDto(IQueryable<Issues> issues);
    }
}
