using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Shared.Services
{
    /// <summary>
    /// Generic audit query service
    /// Pattern: Reporting and compliance queries
    /// Use: Track entity lifecycle (created, updated, deleted)
    /// </summary>
    public class AuditQueryService
    {
        private readonly AppDbContext _db;

        public AuditQueryService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get entity history (all versions including deleted)
        /// </summary>
        public async Task<List<EntityAuditDto<TKey>>> GetEntityHistoryAsync<T, TKey>(
            TKey id)
            where T : BaseEntity<TKey>
        {
            var entity = await _db.Set<T>()
                .Where(e => e.Id!.Equals(id))  // ✅ Uses abstract Id
                .Select(e => new EntityAuditDto<TKey>
                {
                    Id = e.Id,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    DeletedAt = e.DeletedAt,
                    EntityType = typeof(T).Name,
                    IsDeleted = e.DeletedAt != null
                })
                .ToListAsync();

            return entity;
        }

        /// <summary>
        /// Get recently deleted entities (for restore functionality)
        /// </summary>
        public async Task<List<T>> GetRecentlyDeletedAsync<T, TKey>(
            int days = 30)
            where T : BaseEntity<TKey>
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            return await _db.Set<T>()
                .Where(e => e.DeletedAt != null && e.DeletedAt >= cutoffDate)
                .OrderByDescending(e => e.DeletedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get entity count by status
        /// </summary>
        public async Task<EntityCountDto> GetEntityCountAsync<T, TKey>()
            where T : BaseEntity<TKey>
        {
            var total = await _db.Set<T>().CountAsync();
            var active = await _db.Set<T>().Where(e => e.DeletedAt == null).CountAsync();
            var deleted = total - active;

            return new EntityCountDto
            {
                EntityType = typeof(T).Name,
                Total = total,
                Active = active,
                Deleted = deleted
            };
        }
    }

    public record EntityAuditDto<TKey>
    {
        public TKey Id { get; init; }
        public string EntityType { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public DateTime? DeletedAt { get; init; }
        public bool IsDeleted { get; init; }
    }

    public record EntityCountDto
    {
        public string EntityType { get; init; } = string.Empty;
        public int Total { get; init; }
        public int Active { get; init; }
        public int Deleted { get; init; }
    }
}