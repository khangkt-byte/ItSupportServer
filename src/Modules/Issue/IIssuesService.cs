using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Issue
{
    public interface IIssuesService
    {
        Task<BaseResult<PaginatedResult<List<IssueDto>>>> GetIssuesAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<IssueDto>> GetIssueByIdAsync(long issueId);
        Task<BaseResult<IssueCreateDto>> CreateIssueAsync(IssueCreateDto dto);
        Task<BaseResult<IssueUpdateDto>> UpdateIssueAsync(IssueUpdateDto dto);
        Task<BaseResult<bool>> DeleteIssuesAsync(List<long> issueIds, bool softDelete = true);
    }
}
