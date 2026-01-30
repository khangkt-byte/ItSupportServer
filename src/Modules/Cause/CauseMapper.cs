using ItSupportServer.Data.Models.Entities;
using Riok.Mapperly.Abstractions;

namespace ItSupportServer.src.Modules.Cause
{
    [Mapper]
    public partial class CauseMapper
    {
        // ===== Entity → DTOs (not used, only projections) =====

        [MapperIgnoreSource(nameof(Causes.Id))]
        [MapperIgnoreSource(nameof(Causes.DeletedAt))]
        [MapperIgnoreSource(nameof(Causes.Issues))]
        [MapperIgnoreSource(nameof(Causes.IssueLogs))]
        [MapperIgnoreTarget(nameof(CauseDto.UsageCount))]
        [MapperIgnoreTarget(nameof(CauseDto.IssueName))]
        public partial CauseDto MapToCauseDto(Causes cause);

        // ===== DTO → Entity (Create) =====

        [MapperIgnoreTarget(nameof(Causes.CauseId))]  // Auto-increment
        [MapperIgnoreTarget(nameof(Causes.Id))]
        [MapperIgnoreTarget(nameof(Causes.CreatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Causes.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Causes.DeletedAt))]
        [MapperIgnoreTarget(nameof(Causes.Issues))]
        [MapperIgnoreTarget(nameof(Causes.IssueLogs))]
        public partial Causes MapToCause(CreateCauseDto dto);

        // ===== DTO → Entity (Update) =====

        [MapperIgnoreTarget(nameof(Causes.CauseId))]  // Never change
        [MapperIgnoreTarget(nameof(Causes.Id))]
        [MapperIgnoreTarget(nameof(Causes.IssId))]  // Cannot change parent issue
        [MapperIgnoreTarget(nameof(Causes.CreatedAt))]  // Never change
        [MapperIgnoreTarget(nameof(Causes.UpdatedAt))]  // Interceptor
        [MapperIgnoreTarget(nameof(Causes.DeletedAt))]
        [MapperIgnoreTarget(nameof(Causes.Issues))]
        [MapperIgnoreTarget(nameof(Causes.IssueLogs))]
        public partial void MapToCause(UpdateCauseDto dto, Causes cause);

        // ===== Projections =====

        /// <summary>
        /// Project to CauseDto with Issue name
        /// </summary>
        public IQueryable<CauseDto> ProjectToCauseDto(IQueryable<Causes> query)
        {
            return query.Select(c => new CauseDto
            {
                CauseId = c.CauseId,
                IssId = c.IssId,
                IssueName = c.Issues.Name,
                Name = c.Name,
                Description = c.Description,
                UsageCount = 0,  // Calculate separately in service
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
        }

        /// <summary>
        /// Project to ListCauseDto (for paginated list)
        /// </summary>
        public IQueryable<ListCauseDto> ProjectToListCauseDto(IQueryable<Causes> query)
        {
            return query.Select(c => new ListCauseDto
            {
                CauseId = c.CauseId,
                IssId = c.IssId,
                IssueName = c.Issues.Name,
                Name = c.Name,
                UsageCount = 0,  // Calculate separately
                CreatedAt = c.CreatedAt
            });
        }
    }
}