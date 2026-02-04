using FastFood.Base;
using FastFood.Data;
using FastFood.Helper;
using FastFood.Models;
using Microsoft.EntityFrameworkCore;
using FastFood.MenuItem;

namespace FastFood.Category
{
    public class CategoryServices(AppDbContext db, GitHubImageService git)
    {
        public async Task<BaseResult<PaginatedResult<List<CategoryDTO>>>> GetCategoriesAsync(string? query, int page, int pageSize, SortOBJ? sort)
        {
            try
            {
                var categoriesQuery = db.Categories
                                        .Where(c => c.DeletedAt == null)
                                        .Select(c => new CategoryDTO
                                        {
                                            Id = c.Id,
                                            Name = c.Name,
                                            Description = c.Description,
                                            UrlImage = c.UrlImage,
                                            IsActive = c.IsActive,

                                            MenuItems = db.MenuItems
                                                        .Where(mi => mi.CategoryId == c.Id)
                                                        .Select(mi => new MenuItemDTO
                                                        {
                                                            Id = mi.Id,
                                                            Name = mi.Name,
                                                            Description = mi.Description,
                                                            Slug = mi.Slug,
                                                            Unit = mi.Unit,
                                                            UnitPrice = mi.UnitPrice,
                                                            TotalStock = mi.TotalStock,
                                                            TotalSold = mi.TotalSold,
                                                            UrlImage = mi.UrlImage,
                                                            Status = mi.Status,
                                                        }).ToList()
                                        }).AsQueryable();
                if (query is not null)
                {
                    categoriesQuery = categoriesQuery.Where(c => c.Name.Contains(query) || (c.Description != null && c.Description.Contains(query)));
                }

                var result = await Pagination<CategoryDTO>.PaginationAsync(categoriesQuery, page, pageSize, sort);
                return BaseResult<PaginatedResult<List<CategoryDTO>>>.Ok(result);
            }
            catch (Exception ex)
            {
                return BaseResult<PaginatedResult<List<CategoryDTO>>>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<Categories>> GetCategoryAsync(Guid id)
        {
            try
            {
                var category = await db.Categories.FindAsync(id);

                if (category is null)
                    return BaseResult<Categories>.Fail("Danh mục không tồn tại", 404);

                return BaseResult<Categories>.Ok(category);
            }
            catch (Exception ex)
            {
                return BaseResult<Categories>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<Categories>> CreateCategoryAsync(CreateCategoryDTO dto)
        {
            try
            {
                var isCategoryExist = db.Categories.Any(c => c.Name == dto.Name);
                if (isCategoryExist)
                    return BaseResult<Categories>.Fail("Danh mục đã tồn tại", 400);

                var category = new Categories()
                {
                    Name = dto.Name,
                    Description = dto.Description,
                };

                if (dto.Image is not null)
                {
                    var url = await git.UpdateOneImg(dto.Image, "Category");
                    category.UrlImage = url.Url;
                }

                await db.Categories.AddAsync(category);
                await db.SaveChangesAsync();

                return BaseResult<Categories>.Ok(category);
            }
            catch (Exception ex)
            {
                return BaseResult<Categories>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<Categories>> UpdateCategoryAsync(Guid id, UpdateCategoryDTO dto)
        {
            try
            {
                var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (category is null)
                    return BaseResult<Categories>.Fail("Danh mục không tồn tại", 404);

                var isCategoryNameExist = db.Categories.Any(c => c.Name == dto.Name && c.Id != id);
                if (isCategoryNameExist)
                    return BaseResult<Categories>.Fail("Tên danh mục đã tồn tại", 400);

                var updatedCategory = new Categories()
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    UpdatedAt = DateTime.UtcNow
                };

                if (dto.NewImage is not null)
                {
                    var url = await git.UpdateOneImg(dto.NewImage, "Category");
                    updatedCategory.UrlImage = url.Url;
                }

                db.Categories.Update(updatedCategory);
                await db.SaveChangesAsync();

                return BaseResult<Categories>.Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                return BaseResult<Categories>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<bool>> DeleteCategoriesAsync(List<Guid> ids, bool SoftDelete = true)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                    return BaseResult<bool>.Fail("Danh sách danh mục cần xóa trống", 400);

                var existing = await db.Categories
                                       .Where(c => ids.Contains(c.Id))
                                       .ToListAsync();
                if (existing is null || existing.Count == 0) return BaseResult<bool>.Fail("Danh mục không tồn tại", 404);

                var usedCategoryIds = await db.MenuItems
                                     .Where(mi => mi.CategoryId.HasValue && ids.Contains(mi.CategoryId.Value))
                                     .Select(mi => mi.CategoryId)
                                     .Distinct()
                                     .ToListAsync();

                if (usedCategoryIds.Any() || usedCategoryIds.Count > 0)
                {
                    var usedCategoryNames = existing.Where(c => usedCategoryIds.Contains(c.Id))
                                            .Select(c => c.Name)
                                            .Distinct()
                                            .ToList();

                    return BaseResult<bool>.Fail($"Không thể xóa danh mục {string.Join(", ", usedCategoryNames)} vì đang được sử dụng", 400);
                }

                if (SoftDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }

                    db.Categories.UpdateRange(existing);
                }
                else
                {
                    db.Categories.RemoveRange(existing);
                }

                await db.SaveChangesAsync();
                return BaseResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return BaseResult<bool>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }
    }
}