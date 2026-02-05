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
        /// Delete single area (soft delete by default)
        /// Pattern: RESTful single resource delete
        /// Reference: Microsoft REST API Guidelines
        /// </summary>
        Task DeleteAreaAsync(int areaId, bool softDelete = true);

        /// <summary>
        /// Delete multiple areas (all-or-nothing transaction)
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        Task<BulkDeleteResultDto> DeleteAreasAsync(List<int> areaIds, bool softDelete = true);
    }
}
