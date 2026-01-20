using FastFood.Base;
using FastFood.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.Category
{
    [Route("api/admin/categories")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class CategoryController(ICategoryServices service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetCategories(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOBJ? sort = null)
        {
            var result = await service.GetCategoriesAsync(query, page, pageSize, sort);
            return this.MyStatusCode(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory([FromRoute] Guid id)
        {
            var result = await service.GetCategoryAsync(id);
            return this.MyStatusCode(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryDTO dto)
        {
            var result = await service.CreateCategoryAsync(dto);
            return this.MyStatusCode(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromForm] UpdateCategoryDTO dto)
        {
            var result = await service.UpdateCategoryAsync(id, dto);
            return this.MyStatusCode(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategories([FromBody] List<Guid> ids)
        {
            var result = await service.DeleteCategoriesAsync(ids);
            return this.MyStatusCode(result);
        }
    }
}
