using ItSupportServer.Data;
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Area
{
    public interface IAreaService
    {
        Task<PaginatedResult<List<AreaDto>>> GetAreasAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<AreaDto> GetAreaByIdAsync(int areaId);
        Task<AreaDto> CreateAreaAsync(CreateAreaDto dto);
        Task<AreaDto> UpdateAreaAsync(int areId, UpdateAreaDto dto);
        Task<bool> DeleteAreasAsync(List<int> areaIds, bool softDelete = true);
    }
}
