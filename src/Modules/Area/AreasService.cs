using ItSupportServer.Data;
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Area
{
    public class AreasService(AppDbContext db) : IAreasService
    {
        public async Task<BaseResult<PaginatedResult<List<Areas>>>> GetAreasAsync(string? query, int page, int pageSize, SortOBJ? sort)
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

        public async Task<BaseResult<bool>> DeleteAreaAsync(string areaId)
        {
            throw new NotImplementedException();
        }
    }
}
