using ItSupportServer.src.Shared.Base;
using Microsoft.Extensions.Caching.Memory;

namespace ItSupportServer.src.Shared.Services
{
    /// <summary>
    /// Generic entity caching service
    /// Pattern: Cache-aside pattern
    /// Use: Cache frequently accessed entities
    /// </summary>
    public class EntityCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

        public EntityCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Get entity from cache or database
        /// </summary>
        public async Task<T?> GetOrCreateAsync<T, TKey>(
            TKey id,
            Func<TKey, Task<T?>> fetchFromDb,
            TimeSpan? expiration = null)
            where T : BaseEntity<TKey>
        {
            var cacheKey = GetCacheKey<T, TKey>(id);  // ✅ Uses generic Id

            if (_cache.TryGetValue(cacheKey, out T? cachedEntity))
            {
                return cachedEntity;
            }

            var entity = await fetchFromDb(id);

            if (entity != null)
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
                };

                _cache.Set(cacheKey, entity, options);
            }

            return entity;
        }

        /// <summary>
        /// Invalidate cache for specific entity
        /// </summary>
        public void InvalidateEntity<T, TKey>(TKey id)
            where T : BaseEntity<TKey>
        {
            var cacheKey = GetCacheKey<T, TKey>(id);  // ✅ Uses generic Id
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Invalidate cache for multiple entities
        /// </summary>
        public void InvalidateEntities<T, TKey>(IEnumerable<TKey> ids)
            where T : BaseEntity<TKey>
        {
            foreach (var id in ids)
            {
                InvalidateEntity<T, TKey>(id);
            }
        }

        private static string GetCacheKey<T, TKey>(TKey id)
            where T : BaseEntity<TKey>
        {
            return $"{typeof(T).Name}_{id}";
        }
    }
}

// ✅ USAGE in Service:
//public class AreaService : IAreaService
//{
//    private readonly EntityCacheService _cacheService;

//    public async Task<AreaDto> GetAreaByIdAsync(int areaId)
//    {
//        // ✅ Generic caching via abstract Id
//        var area = await _cacheService.GetOrCreateAsync<Areas, int>(
//            areaId,
//            async id => await _db.Areas.FindAsync(id),
//            TimeSpan.FromMinutes(30));

//        if (area is null || area.DeletedAt != null)
//            throw new NotFoundException("Khu vực", areaId);

//        return _mapper.MapToAreaDto(area);
//    }

    //public async Task<AreaDto> UpdateAreaAsync(int areaId, UpdateAreaDto dto)
    //{
    //    // ... update logic ...

    //    // ✅ Invalidate cache after update
    //    _cacheService.InvalidateEntity<Areas, int>(areaId);

    //    return result;
    //}
//}