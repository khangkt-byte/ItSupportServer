using ItSupportServer.Data.Models;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.IssueLog
{
    [Mapper]
    public partial class IssueLogMapper
    {
        // ===== Entity → DTO =====

        [MapperIgnoreSource(nameof(IssueLogs.Id))]
        [MapperIgnoreSource(nameof(IssueLogs.DeletedAt))]
        public partial IssueLogDto MapToIssueLogDto(IssueLogs issueLog);

        // ===== DTO → Entity (Create) =====

        [MapperIgnoreTarget(nameof(IssueLogs.IssLogId))]
        [MapperIgnoreTarget(nameof(IssueLogs.Id))]
        [MapperIgnoreTarget(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.DeletedAt))]
        public partial IssueLogs MapToIssueLog(CreateIssueLogDto dto);

        // ===== DTO → Entity (Update - Partial) =====

        [MapperIgnoreTarget(nameof(IssueLogs.IssLogId))]
        [MapperIgnoreTarget(nameof(IssueLogs.Id))]
        [MapperIgnoreTarget(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.DeletedAt))]
        public partial void MapToIssueLog(UpdateIssueLogDto dto, IssueLogs issueLog);

        // ===== Projection for EF Core =====

        public partial IQueryable<IssueLogDto> ProjectToIssueLogDto(IQueryable<IssueLogs> query);
    }
}
