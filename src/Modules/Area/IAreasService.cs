using ItSupportServer.Data;
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Area
{
    public interface IAreasService
    {
        Task<BaseResult<PaginatedResult<List<AreaDto>>>> GetAreasAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<AreaDto>> GetAreaByIdAsync(int areaId);
        Task<BaseResult<CreateAreaDto>> CreateAreaAsync(CreateAreaDto dto);
        Task<BaseResult<UpdateAreaDto>> UpdateAreaAsync(UpdateAreaDto dto);
        Task<BaseResult<bool>> DeleteAreasAsync(List<int> areaId, bool softDelete = true);
    }
}
