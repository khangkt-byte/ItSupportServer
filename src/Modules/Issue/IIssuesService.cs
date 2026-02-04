using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Issue
{
    public interface IIssuesService
    {
        Task<BaseResult<PaginatedResult<List<IssueDto>>>> GetIssuesAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<IssueDto>> GetIssueByIdAsync(string issueId);
        Task<BaseResult<IssueCreateDto>> CreateIssueAsync(IssueCreateDto dto);
        Task<BaseResult<IssueUpdateDto>> UpdateIssueAsync(IssueUpdateDto dto);
        Task<BaseResult<bool>> DeleteIssueAsync(List<string> issueIds, bool softDelete = true);
    }
}
