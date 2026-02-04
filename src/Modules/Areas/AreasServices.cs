using ItSupportServer.Data;

namespace ItSupportServer.src.Modules.Areas
{
    public class AreasServices(AppDbContext db) : IAreasServices
    {
        public async Task<BaseResult<PaginationResult<List<Areas>>>> GetAreasAsynce(string? query, int page, int pageSize, SortOBJ sort)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult<Areas>> GetAreaByIdAsync(string areaId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult<AreaCreateDto>> CreateAreaAsync(AreaCreateDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult<AreaUpdateDto>> UpdateAreaAsync(AreaUpdateDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
