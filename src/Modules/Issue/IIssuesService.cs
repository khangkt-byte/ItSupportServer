using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Issue
{
    public interface IIssuesService
    {
        Task<BaseResult<PaginatedResult<List<IssueDto>>>> GetIssuesAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<IssueDto>> GetIssueByIdAsync(long issueId);
        Task<BaseResult<CreateIssueDto>> CreateIssueAsync(CreateIssueDto dto);
        Task<BaseResult<UpdateIssueDto>> UpdateIssueAsync(UpdateIssueDto dto);
        Task<BaseResult<bool>> DeleteIssuesAsync(List<long> issueIds, bool softDelete = true);
    }
}
