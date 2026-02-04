using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Modules.Issue;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Issue log mapper using Mapperly
    /// Pattern: Microsoft recommendation for high-performance mapping
    /// Performance: Zero-allocation, compile-time generated code
    /// </summary>
    [Mapper]
    public partial class IssueLogMapper
    {
        [MapperIgnoreSource(nameof(IssueLogs.Id))]
        [MapperIgnoreSource(nameof(IssueLogs.DeletedAt))]
        [MapperIgnoreSource(nameof(IssueLogs.IssueLogOperators))]
        [MapperIgnoreSource(nameof(IssueLogs.IssueLogRequesters))]
        [MapProperty(nameof(IssueLogs.CauseRef.Name), nameof(IssueLogDto.CauseName))]
        public partial IssueLogDto MapToIssueLogDto(IssueLogs issueLog);

        [MapperIgnoreTarget(nameof(IssueLogs.IssLogId))]
        [MapperIgnoreTarget(nameof(IssueLogs.Id))]
        [MapperIgnoreTarget(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.DeletedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.Department))]
        [MapperIgnoreTarget(nameof(IssueLogs.Area))]
        [MapperIgnoreTarget(nameof(IssueLogs.Issue))]
        [MapperIgnoreTarget(nameof(IssueLogs.CauseRef))]
        [MapperIgnoreTarget(nameof(IssueLogs.IssueLogOperators))]
        [MapperIgnoreTarget(nameof(IssueLogs.IssueLogRequesters))]
        public partial IssueLogs MapToIssueLog(CreateIssueLogDto dto);

        [MapperIgnoreTarget(nameof(IssueLogs.IssLogId))]
        [MapperIgnoreTarget(nameof(IssueLogs.Id))]
        [MapperIgnoreTarget(nameof(IssueLogs.CreatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.UpdatedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.DeletedAt))]
        [MapperIgnoreTarget(nameof(IssueLogs.Department))]
        [MapperIgnoreTarget(nameof(IssueLogs.Area))]
        [MapperIgnoreTarget(nameof(IssueLogs.Issue))]
        [MapperIgnoreTarget(nameof(IssueLogs.CauseRef))]
        [MapperIgnoreTarget(nameof(IssueLogs.IssueLogOperators))]
        [MapperIgnoreTarget(nameof(IssueLogs.IssueLogRequesters))]
        public partial void MapToIssueLog(UpdateIssueLogDto dto, IssueLogs issueLog);

        /// <summary>
        /// Project to DTO with all navigation properties
        /// Pattern: Manual projection for EF Core query optimization
        /// Performance: Single SQL query with joins
        /// </summary>
        public partial IQueryable<IssueLogDto> ProjectToIssueLogDto(IQueryable<IssueLogs> query);
        //{
        //    return query.Select(il => new IssueLogDto
        //    {
        //        IssLogId = il.IssLogId,

        //        // Operators & Requesters
        //        Operator = il.Operator,
        //        Requester = il.Requester,

        //        // Department & Area
        //        DptId = il.DptId,
        //        DepartmentName = il.Department.Name,
        //        AreaId = il.AreaId,
        //        AreaName = il.Area.Name,

        //        // Issue (KB reference + actual text)
        //        IssueId = il.IssueId,
        //        IssueName = il.Issue != null ? il.Issue.Name : null,
        //        IssueDescription = il.IssueDescription,

        //        // Cause (KB reference + actual text)
        //        CauseId = il.CauseId,
        //        CauseName = il.CauseRef != null ? il.CauseRef.Name : null,
        //        Cause = il.Cause,

        //        // Resolution
        //        Resolution = il.Resolution,
        //        PermanentFix = il.PermanentFix,
        //        Notes = il.Notes,

        //        // Metadata
        //        DateReported = il.DateReported,
        //        Status = il.Status,
        //        CreatedAt = il.CreatedAt,
        //        UpdatedAt = il.UpdatedAt
        //    });
        //}

        public IQueryable<IssueSuggestionDto> ProjectToIssueSuggestion(
            IQueryable<Issues> issuesQuery,
            IQueryable<IssueLogs> issueLogsQuery)
        {
            return issuesQuery
                .GroupJoin(
                    issueLogsQuery.Where(il => il.DeletedAt == null),
                    issue => issue.IssId,
                    log => log.IssueId,  // EF Core handles long to long? comparison
                    (issue, logs) => new IssueSuggestionDto
                    {
                        IssId = issue.IssId,
                        Name = issue.Name,
                        Description = issue.Description,
                        UsageCount = logs.Count(),
                        LastUsed = logs.Any()
                            ? logs.Max(l => (DateTime?)l.CreatedAt)
                            : null
                    });
        }
    }
}
