using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.IssueLog
{
    [Mapper]
    public partial class IssueLogMapper
    {
        [MapperIgnoreSource(nameof(IssueLogs.Id))]
        [MapperIgnoreSource(nameof(IssueLogs.DeletedAt))]
        public partial IssueLogDto MapToIssueLogDto(IssueLogs issueLog);

        [MapperIgnoreTarget(nameof(IssueLogs.IssLogId))]
        [MapperIgnoreTarget(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.DeletedAt))]
        public partial IssueLogs MapToIssueLog(CreateIssueLogDto issueLogDto);


        [MapperIgnoreTarget(nameof(IssueLogs.IssLogId))]
        [MapperIgnoreTarget(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.DeletedAt))]
        public partial void MapToIssueLog(UpdateIssueLogDto issueLogDto, IssueLogs issueLog);

        public partial IQueryable<IssueLogDto> ProjectToIssueLogDto(IQueryable<IssueLogs> issueLogs);
    }
}
