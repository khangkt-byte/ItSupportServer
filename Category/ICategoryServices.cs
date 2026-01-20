using FastFood.Base;
using FastFood.Models;
using FastFood.Category;

namespace FastFood.Category
{
    public interface ICategoryServices
    {
        Task<BaseResult<PaginatedResult<List<CategoryDTO>>>> GetCategoriesAsync(
            string? query,
            int page,
            int pageSize,
            SortOBJ? sort);
        Task<BaseResult<Categories>> GetCategoryAsync(Guid id);
        Task<BaseResult<Categories>> CreateCategoryAsync(CreateCategoryDTO dto);
        Task<BaseResult<Categories>> UpdateCategoryAsync(Guid id, UpdateCategoryDTO dto);
        Task<BaseResult<bool>> DeleteCategoriesAsync(List<Guid> ids, bool SoftDelete = true);
    }
}
