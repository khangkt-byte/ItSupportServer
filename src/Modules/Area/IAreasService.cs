using ItSupportServer.Data;
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Area
{
    public interface IAreasService
    {
        Task<BaseResult<PaginatedResult<List<Areas>>>> GetAreasAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<Areas>> GetAreaByIdAsync(string areaId);
        Task<BaseResult<AreaCreateDto>> CreateAreaAsync(AreaCreateDto dto);
        Task<BaseResult<AreaUpdateDto>> UpdateAreaAsync(AreaUpdateDto dto);
        Task<BaseResult<bool>> DeleteAreaAsync(string areaId);
    }
}
