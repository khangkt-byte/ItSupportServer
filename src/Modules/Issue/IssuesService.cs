using ItSupportServer.Data;
using ItSupportServer.src.Modules.Area;
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

        public async Task<BaseResult<IssueDto>> GetIssueByIdAsync(string issueId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult<IssueCreateDto>> CreateIssueAsync(IssueCreateDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult<IssueUpdateDto>> UpdateIssueAsync(IssueUpdateDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult<bool>> DeleteIssueAsync(List<string> issueIds)
        {
            throw new NotImplementedException();
        }
    }
}
