using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Shared.Services
{
    /// <summary>
    /// Generic soft delete service
    /// Pattern: Centralized soft delete logic
    /// Use: Consistent soft delete behavior across entities
    /// </summary>
    public class SoftDeleteService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SoftDeleteService> _logger;

        public SoftDeleteService(AppDbContext db, ILogger<SoftDeleteService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Soft delete single entity
        /// </summary>
        public async Task<bool> SoftDeleteAsync<T, TKey>(
            TKey id,
            string resourceName,
            CancellationToken ct = default)
            where T : BaseEntity<TKey>
        {
            var entity = await _db.Set<T>()
                .Where(e => e.Id!.Equals(id))  // ✅ Uses abstract Id
                .FirstOrDefaultAsync(ct);

            if (entity is null || entity.DeletedAt != null)
            {
                _logger.LogWarning("{Resource} with ID {Id} not found or already deleted",
                    resourceName, id);
                return false;
            }

            entity.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("{Resource} with ID {Id} soft deleted",
                resourceName, id);

            return true;
        }

        /// <summary>
        /// Restore soft deleted entity
        /// </summary>
        public async Task<bool> RestoreAsync<T, TKey>(
            TKey id,
            string resourceName,
            CancellationToken ct = default)
            where T : BaseEntity<TKey>
        {
            var entity = await _db.Set<T>()
                .Where(e => e.Id!.Equals(id))  // ✅ Uses abstract Id
                .FirstOrDefaultAsync(ct);

            if (entity is null)
            {
                _logger.LogWarning("{Resource} with ID {Id} not found",
                    resourceName, id);
                return false;
            }

            if (entity.DeletedAt is null)
            {
                _logger.LogWarning("{Resource} with ID {Id} is not deleted",
                    resourceName, id);
                return false;
            }

            entity.DeletedAt = null;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("{Resource} with ID {Id} restored",
                resourceName, id);

            return true;
        }

        /// <summary>
        /// Bulk soft delete
        /// </summary>
        public async Task<int> BulkSoftDeleteAsync<T, TKey>(
            List<TKey> ids,
            string resourceName,
            CancellationToken ct = default)
            where T : BaseEntity<TKey>
        {
            var entities = await _db.Set<T>()
                .Where(e => ids.Contains(e.Id!))  // ✅ Uses abstract Id
                .Where(e => e.DeletedAt == null)
                .ToListAsync(ct);

            if (!entities.Any())
                return 0;

            var now = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                entity.DeletedAt = now;
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("{Count} {Resource}(s) soft deleted",
                entities.Count, resourceName);

            return entities.Count;
        }
    }
}