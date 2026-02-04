using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.src.Modules.Issue
{
    public class IssuesService(AppDbContext db) : IIssuesService
    {
        public async Task<BaseResult<PaginatedResult<List<IssueDto>>>> GetIssuesAsync(string? query, int page, int pageSize, SortOBJ? sort)
        {
            try
            {
                var issuesQuery = db.Issues
                                           .Where(i => i.DeletedAt != null)
                                           .Select(i => new IssueDto
                                           {
                                               Name = i.Name,
                                               Description = i.Description
                                           })
                                           .AsNoTracking();
                                           //.ToListAsync();

                //return BaseResult<PaginatedResult<List<IssueDto>>>.Ok(new PaginatedResult<List<IssueDto>>
                //{
                //    CurrentPage = page,
                //    PageSize = pageSize,
                //    TotalItems = issuesQuery.Count,
                //    TotalPages = (int)Math.Ceiling(issuesQuery.Count / (double)pageSize),
                //    Data = issuesQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                //});

                if (query is not null)
                {
                    issuesQuery = issuesQuery.Where(a => a.Name.Contains(query) || (a.Description != null && a.Description.Contains(query)));
                }

                var result = await Pagination<IssueDto>.PaginationAsync(issuesQuery, page, pageSize, sort);
                return BaseResult<PaginatedResult<List<IssueDto>>>.Ok(result);
            }
            catch (Exception ex)
            {
                return BaseResult<PaginatedResult<List<IssueDto>>>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<IssueDto>> GetIssueByIdAsync(long issueId)
        {
            try
            {
                var issue = await db.Issues
                                    .Where(i => i.IssId == issueId && i.DeletedAt == null)
                                    .Select(i => new IssueDto
                                    {
                                        Name = i.Name,
                                        Description = i.Description
                                    })
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

                if (issue is null)
                    return BaseResult<IssueDto>.Fail("Vấn đề không tồn tại.", 404);

                return BaseResult<IssueDto>.Ok(issue);
            }
            catch (Exception ex)
            {
                return BaseResult<IssueDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<IssueCreateDto>> CreateIssueAsync(IssueCreateDto dto)
        {
            try
            {
                var existing = await db.Issues
                                       .Where(i => i.Name == dto.Name && i.DeletedAt == null)
                                       .FirstOrDefaultAsync();

                if (existing is not null)
                    return BaseResult<IssueCreateDto>.Fail("Vấn đề đã tồn tại.", 400);

                var newIssue = new Issues()
                {
                    Name = dto.Name,
                    Description = dto.Description
                };

                await db.Issues.AddAsync(newIssue);
                await db.SaveChangesAsync();

                return BaseResult<IssueCreateDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                return BaseResult<IssueCreateDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<IssueUpdateDto>> UpdateIssueAsync(IssueUpdateDto dto)
        {
            try
            {
                var issue = await db.Issues
                                    .Where(i => i.IssId == dto.IssId && i.DeletedAt == null)
                                    .FirstOrDefaultAsync();

                if (issue is null)
                    return BaseResult<IssueUpdateDto>.Fail("Vấn đề không tồn tại.", 404);

                if (!string.IsNullOrEmpty(dto.Name))
                    issue.Name = dto.Name;
                if (!string.IsNullOrEmpty(dto.Description))
                    issue.Description = dto.Description;
                issue.UpdatedAt = DateTime.UtcNow;

                db.Issues.Update(issue);
                await db.SaveChangesAsync();

                return BaseResult<IssueUpdateDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                return BaseResult<IssueUpdateDto>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<BaseResult<bool>> DeleteIssuesAsync(List<long> issueIds, bool softDelete = true)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                if (issueIds == null || issueIds.Count == 0)
                    return BaseResult<bool>.Fail("Vui lòng chọn khu vực để xóa.", 400);

                var existing = await db.Issues
                                       .Where(i => issueIds.Contains(i.IssId) && i.DeletedAt == null)
                                       .ToListAsync();

                if (existing is null || existing.Count == 0)
                    return BaseResult<bool>.Fail("Khu vực không tồn tại.", 404);

                var usedIssueIds = await db.Causes
                                     .Where(e => e.DeletedAt == null && issueIds.Contains(e.IssId))
                                     .Select(e => e.IssId)
                                     .Distinct()
                                     .ToListAsync();

                if (usedIssueIds.Any() || usedIssueIds.Count > 0)
                {
                    var usedIssueNames = existing.Where(c => usedIssueIds.Contains(c.IssId))
                                            .Select(c => c.Name)
                                            .Distinct()
                                            .ToList();

                    return BaseResult<bool>.Fail($"Xóa thất bại. Không thể xóa vấn đề {string.Join(", ", usedIssueNames)} vì đang được sử dụng.", 400);
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;
                    }

                    db.Issues.UpdateRange(existing);
                }
                else
                {
                    db.Issues.RemoveRange(existing);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return BaseResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResult<bool>.Fail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }
    }
}
