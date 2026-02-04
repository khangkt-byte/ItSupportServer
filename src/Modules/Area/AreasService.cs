using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Modules.Area
{
    public class AreasService(AppDbContext db) : IAreasService
    {
        public async Task<BaseResult<PaginatedResult<List<AreaDto>>>> GetAreasAsync(string? query, int page, int pageSize, SortOBJ? sort)
        {
            try
            {
                var areasQuery = db.Areas
                                          .Where(a => a.DeletedAt == null)
                                          .Select(a => new AreaDto
                                          {
                                              Name = a.Name,
                                              Description = a.Description
                                          })
                                          .AsNoTracking();
                                          //.ToListAsync();
                if (query is not null)
                {
                    areasQuery = areasQuery.Where(a => a.Name.Contains(query) || (a.Description != null && a.Description.Contains(query)));
                }

                var result = await Pagination<AreaDto>.PaginationAsync(areasQuery, page, pageSize, sort);
                return BaseResult<PaginatedResult<List<AreaDto>>>.Ok(result);
            }
            catch (Exception ex)
            {
                return BaseResult<PaginatedResult<List<AreaDto>>>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<AreaDto>> GetAreaByIdAsync(int areaId)
        {
            try
            {
                var area = await db.Areas
                                    .Where(a => a.AreaId == areaId && a.DeletedAt == null)
                                    .Select(a => new AreaDto
                                    {
                                        Name = a.Name,
                                        Description = a.Description
                                    })
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

                if (area is null)
                    return BaseResult<AreaDto>.Fail("Khu vực không tồn tại", 404);

                return BaseResult<AreaDto>.Ok(area);
            }
            catch (Exception ex)
            {
                return BaseResult<AreaDto>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<AreaCreateDto>> CreateAreaAsync(AreaCreateDto dto)
        {
            try
            {
                var existing = await db.Areas
                                       .Where(a => a.Name == dto.Name && a.DeletedAt == null)
                                       .FirstOrDefaultAsync();

                if (existing is not null)
                    return BaseResult<AreaCreateDto>.Fail("Khu vực đã tồn tại", 400);

                var newArea = new Areas()
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                await db.Areas.AddAsync(newArea);
                await db.SaveChangesAsync();

                return BaseResult<AreaCreateDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                return BaseResult<AreaCreateDto>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<AreaUpdateDto>> UpdateAreaAsync(AreaUpdateDto dto)
        {
            try
            {
                var area = await db.Areas.FindAsync(dto.AreaId);

                if (area is null || area.DeletedAt != null)
                    return BaseResult<AreaUpdateDto>.Fail("Khu vực không tồn tại", 404);

                area.Name = dto.Name ?? area.Name;
                area.Description = dto.Description ?? area.Description;
                area.UpdatedAt = DateTime.UtcNow;

                db.Areas.Update(area);
                await db.SaveChangesAsync();

                return BaseResult<AreaUpdateDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                return BaseResult<AreaUpdateDto>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<bool>> DeleteAreasAsync(List<int> areaIds, bool softDelete = true)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                if (areaIds == null || areaIds.Count == 0)
                    return BaseResult<bool>.Fail("Vui lòng chọn khu vực để xóa", 400);

                var existing = await db.Areas
                                       .Where(c => areaIds.Contains(c.AreaId))
                                       .ToListAsync();
                if (existing is null || existing.Count == 0) return BaseResult<bool>.Fail("Khu vực không tồn tại", 404);

                var usedAreaIds = await db.Employees
                                     .Where(e => e.DeletedAt == null && areaIds.Contains(e.AreaId))
                                     .Select(e => e.AreaId)
                                     .Distinct()
                                     .ToListAsync();

                if (usedAreaIds.Any() || usedAreaIds.Count > 0)
                {
                    var usedAreaNames = existing.Where(c => usedAreaIds.Contains(c.AreaId))
                                            .Select(c => c.Name)
                                            .Distinct()
                                            .ToList();

                    return BaseResult<bool>.Fail($"Xóa thất bại. Không thể xóa khu vực {string.Join(", ", usedAreaNames)} vì đang được sử dụng", 400);
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }

                    db.Areas.UpdateRange(existing);
                }
                else
                {
                    db.Areas.RemoveRange(existing);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return BaseResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResult<bool>.Fail($"Lỗi Hệ thống: {ex.Message}", 500);
            }
        }
    }
}
