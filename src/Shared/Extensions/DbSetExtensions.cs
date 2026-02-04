using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Shared.Extensions
{
    /// <summary>
    /// DbSet extension methods for common operations
    /// Pattern: Extension methods (NOT base class)
    /// Use: Reduce boilerplate for common patterns
    /// Reference: Microsoft EF Core extensions pattern
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Find active entity by ID or throw NotFoundException
        /// Pattern: Fail-fast with descriptive error
        /// </summary>
        public static async Task<T> FindActiveOrThrowAsync<T, TKey>(
            this DbSet<T> dbSet,
            TKey id,
            string resourceName,
            CancellationToken ct = default)
            where T : BaseEntity<TKey>
        {
            var entity = await dbSet
                .Where(e => e.Id!.Equals(id) && e.DeletedAt == null)  // ✅ Uses abstract Id
                .FirstOrDefaultAsync(ct);

            if (entity is null)
            {
                throw new NotFoundException(resourceName, id!);
            }

            return entity;
        }

        /// <summary>
        /// Check if entity exists (active only)
        /// </summary>
        public static async Task<bool> ExistsAsync<T, TKey>(
            this DbSet<T> dbSet,
            TKey id,
            CancellationToken ct = default)
            where T : BaseEntity<TKey>
        {
            return await dbSet
                .Where(e => e.Id!.Equals(id) && e.DeletedAt == null)  // ✅ Uses abstract Id
                .AnyAsync(ct);
        }

        /// <summary>
        /// Soft delete entity by ID
        /// </summary>
        public static async Task<bool> SoftDeleteByIdAsync<T, TKey>(
            this DbSet<T> dbSet,
            TKey id,
            CancellationToken ct = default)
            where T : BaseEntity<TKey>
        {
            var entity = await dbSet
                .Where(e => e.Id!.Equals(id))  // ✅ Uses abstract Id
                .FirstOrDefaultAsync(ct);

            if (entity is null)
                return false;

            entity.DeletedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Find entity with includes (generic eager loading)
        /// </summary>
        public static async Task<T?> FindWithIncludesAsync<T, TKey>(
            this DbSet<T> dbSet,
            TKey id,
            params string[] navigationPropertyPaths)
            where T : BaseEntity<TKey>
        {
            IQueryable<T> query = dbSet;

            foreach (var path in navigationPropertyPaths)
            {
                query = query.Include(path);
            }

            return await query
                .Where(e => e.Id!.Equals(id) && e.DeletedAt == null)  // ✅ Uses abstract Id
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}