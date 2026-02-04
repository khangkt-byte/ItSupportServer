using ItSupportServer.Data.Models;
using ItSupportServer.src.Modules.Cause;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Issue
{
    [Mapper]
    public partial class IssueMapper
    {
        // ===== Entity → DTO =====

        [MapperIgnoreSource(nameof(Issues.Id))]
        [MapperIgnoreSource(nameof(Issues.DeletedAt))]
        [MapperIgnoreTarget(nameof(IssueDto.UsageCount))]
        public partial IssueDto MapToIssueDto(Issues issue);

        // ===== DTO → Entity (Create) =====

        [MapperIgnoreTarget(nameof(Issues.IssId))]  // Auto-increment
        [MapperIgnoreTarget(nameof(Issues.Id))]
        [MapperIgnoreTarget(nameof(Issues.CreatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Issues.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        [MapperIgnoreTarget(nameof(Issues.Causes))]
        public partial Issues MapToIssue(CreateIssueDto dto);

        // ===== DTO → Entity (Update) =====

        [MapperIgnoreTarget(nameof(Issues.IssId))]  // Never change
        [MapperIgnoreTarget(nameof(Issues.Id))]
        [MapperIgnoreTarget(nameof(Issues.CreatedAt))]  // Never change
        [MapperIgnoreTarget(nameof(Issues.UpdatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Issues.DeletedAt))]
        [MapperIgnoreTarget(nameof(Issues.Causes))]
        public partial void MapToIssue(UpdateIssueDto dto, Issues issue);

        // ===== Projection (with Causes) =====

        // Option 1: Calculate in Service layer instead
        public IQueryable<IssueDto> ProjectToIssueDto(IQueryable<Issues> query)
        {
            return query.Select(i => new IssueDto
            {
                IssId = i.IssId,
                Name = i.Name,
                Description = i.Description,
                Category = i.Category,
                Severity = i.Severity,
                UsageCount = 0,  // ⚠️ Will be calculated in service
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                Causes = i.Causes
                    .Where(c => c.DeletedAt == null)
                    .Select(c => new CauseDto
                    {
                        CauseId = c.CauseId,
                        IssId = c.IssId,
                        Name = c.Name,
                        Description = c.Description,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToList()
            });
        }

        // Note: Need AppDbContext reference for UsageCount
        // You may need to inject it in mapper or calculate differently
    }
}
