using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Area
{
    public interface IAreaService
    {
        /// <summary>
        /// Get paginated list of areas
        /// </summary>
        Task<PaginatedResult<AreaDto>> GetAreasAsync(QueryParameters parameters);

        /// <summary>
        /// Get area by ID
        /// </summary>
        Task<AreaDto> GetAreaByIdAsync(int areaId);

        /// <summary>
        /// Create new area
        /// </summary>
        Task<AreaDto> CreateAreaAsync(CreateAreaDto dto);

        /// <summary>
        /// Update existing area (partial update supported)
        /// </summary>
        Task<AreaDto> UpdateAreaAsync(int areaId, UpdateAreaDto dto);

        /// <summary>
        /// Delete areas (soft delete by default)
        /// </summary>
        Task<bool> DeleteAreasAsync(List<int> areaIds, bool softDelete = true);
    }
}
