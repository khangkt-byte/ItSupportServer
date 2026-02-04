using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Modules.Cause;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Issue
{
    /// <summary>
    /// Issue mapper using Mapperly
    /// Pattern: Compile-time code generation
    /// Performance: Zero-allocation, no reflection
    /// </summary>
    [Mapper]
    public partial class IssueMapper
    {
        // ===== ENTITY → DTO (Simple Mapping) =====

        /// <summary>
        /// Map single entity to DTO (no nested data)
        /// Mapperly: Auto-generates optimized mapping code
        /// </summary>
        [MapperIgnoreSource(nameof(Issues.Id))]
        [MapperIgnoreSource(nameof(Issues.DeletedAt))]
        [MapperIgnoreSource(nameof(Issues.Causes))]
        [MapperIgnoreSource(nameof(Issues.IssueLogs))]
        [MapperIgnoreTarget(nameof(IssueDto.UsageCount))]  // Calculated separately
        [MapperIgnoreTarget(nameof(IssueDto.Causes))]
        public partial IssueDto MapToIssueDto(Issues issue);

        // ===== DTO → ENTITY (Create) =====

        /// <summary>
        /// Map CreateDto to entity
        /// Mapperly: Auto-generates property-by-property assignment
        /// </summary>
        [MapperIgnoreTarget(nameof(Issues.IssId))]  // Auto-increment
        [MapperIgnoreTarget(nameof(Issues.Id))]
        [MapperIgnoreTarget(nameof(Issues.CreatedAt))]  // AuditInterceptor
        [MapperIgnoreTarget(nameof(Issues.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        [MapperIgnoreTarget(nameof(Issues.Causes))]
        [MapperIgnoreTarget(nameof(Issues.IssueLogs))]
        public partial Issues MapToIssue(CreateIssueDto dto);

        // ===== DTO → ENTITY (Update - Partial) =====

        /// <summary>
        /// Map UpdateDto to entity (partial update)
        /// Mapperly: Only updates non-null properties
        /// Note: Cannot use auto-mapping for null-coalescing logic
        /// Reference: RFC 7396 - JSON Merge Patch requires custom logic
        /// </summary>
        [MapperIgnoreTarget(nameof(Issues.IssId))]
        [MapperIgnoreTarget(nameof(Issues.Id))]
        [MapperIgnoreTarget(nameof(Issues.CreatedAt))]
        [MapperIgnoreTarget(nameof(Issues.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        [MapperIgnoreTarget(nameof(Issues.Causes))]
        [MapperIgnoreTarget(nameof(Issues.IssueLogs))]
        public partial void MapToIssue(UpdateIssueDto dto, Issues issue);

        // ===== QUERY PROJECTIONS (EF Core Integration) =====

        /// <summary>
        /// Project to IssueDto with nested Causes
        /// Pattern: Manual projection for EF Core query composition
        /// Performance: Single SQL query with LEFT JOIN
        /// Mapperly: Cannot auto-generate (too complex for source generator)
        /// Reference: EF Core - Query projection best practices
        /// </summary>
        public IQueryable<IssueDto> ProjectToIssueDto(IQueryable<Issues> query)
        {
            return query.Select(i => new IssueDto
            {
                IssId = i.IssId,
                Name = i.Name,
                Description = i.Description,
                Category = i.Category,
                Severity = i.Severity,
                UsageCount = 0,  // Calculated separately in service
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,

                // Nested causes projection
                Causes = i.Causes
                    .Where(c => c.DeletedAt == null)
                    .Select(c => new CauseDto
                    {
                        CauseId = c.CauseId,
                        IssId = c.IssId,
                        IssueName = i.Name,  // From parent
                        Name = c.Name,
                        Description = c.Description,
                        UsageCount = 0,  // Calculated separately
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToList()
            });
        }

        /// <summary>
        /// ✅ NEW: Project to IssueSuggestionDto with usage statistics
        /// Pattern: Aggregation projection with type safety
        /// Performance: Database-side GROUP BY and aggregation
        /// Purpose: Replaces anonymous types with strongly-typed DTOs
        /// References:
        /// - EF Core: GroupJoin for LEFT JOIN with aggregation
        /// - SQL: SELECT ... LEFT JOIN ... GROUP BY pattern
        /// - Performance: Single query vs N+1 queries
        /// </summary>
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

        /// <summary>
        /// ✅ ALTERNATIVE: Project to lightweight issue (no stats)
        /// Use this for scenarios where you calculate stats separately
        /// </summary>
        public IQueryable<IssueSuggestionDto> ProjectToIssueSuggestionWithoutStats(
            IQueryable<Issues> query)
        {
            return query.Select(i => new IssueSuggestionDto
            {
                IssId = i.IssId,
                Name = i.Name,
                Description = i.Description,
                UsageCount = 0,  // Set later
                LastUsed = null  // Set later
            });
        }
    }
}
