using ItSupportServer.Data.Models;
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
        // ===== Entity → DTO (Simple - No nested Causes) =====

        [MapperIgnoreSource(nameof(Issues.Id))]
        [MapperIgnoreSource(nameof(Issues.DeletedAt))]
        [MapperIgnoreSource(nameof(Issues.Causes))]
        [MapperIgnoreTarget(nameof(IssueDto.UsageCount))]
        [MapperIgnoreTarget(nameof(IssueDto.Causes))]
        public partial IssueDto MapToIssueDto(Issues issue);

        // ===== DTO → Entity (Create) =====

        [MapperIgnoreTarget(nameof(Issues.IssId))]
        [MapperIgnoreTarget(nameof(Issues.Id))]
        [MapperIgnoreTarget(nameof(Issues.CreatedAt))]
        [MapperIgnoreTarget(nameof(Issues.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        [MapperIgnoreTarget(nameof(Issues.Causes))]
        public partial Issues MapToIssue(CreateIssueDto dto);

        // ===== DTO → Entity (Update) =====

        [MapperIgnoreTarget(nameof(Issues.IssId))]
        [MapperIgnoreTarget(nameof(Issues.Id))]
        [MapperIgnoreTarget(nameof(Issues.CreatedAt))]
        [MapperIgnoreTarget(nameof(Issues.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        [MapperIgnoreTarget(nameof(Issues.Causes))]
        public partial void MapToIssue(UpdateIssueDto dto, Issues issue);

        // ===== MANUAL PROJECTION (Complex - with nested Causes) =====

        /// <summary>
        /// Project to IssueDto with nested Causes
        /// Pattern: Manual projection for complex scenarios (EF Core query optimization)
        /// Performance: Single SQL query with joins
        /// Note: UsageCount calculated separately in service layer
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
                UsageCount = 0,  // ✅ Calculated separately in service
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,

                // ✅ FIX: Map nested Causes with all required fields
                Causes = i.Causes
                    .Where(c => c.DeletedAt == null)
                    .Select(c => new CauseDto
                    {
                        CauseId = c.CauseId,
                        IssId = c.IssId,
                        IssueName = c.Issues.Name,
                        Name = c.Name,
                        Description = c.Description,
                        UsageCount = 0,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToList()
            });
        }
    }
}
