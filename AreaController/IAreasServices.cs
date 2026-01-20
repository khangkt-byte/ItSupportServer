using ITSupportServer.Data;

namespace ITSupportServer.AreasController
{
    public interface IAreasServices
    {
        Task<BaseResult<PaginationResult<List<Areas>>>> GetAreasAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<Areas>> GetAreaByIdAsync(string areaId);
        Task<BaseResult<AreaCreateDto>> CreateAreaAsync(AreaCreateDto dto);
        Task<BaseResult<AreaUpdateDto>> UpdateAreaAsync(AreaUpdateDto dto);
        Task<BaseResult<bool>> DeleteAreaAsync(string areaId);
    }
}
