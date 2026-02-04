using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.IssueLog
{
    public interface IIssueLogsService
    {
        Task<BaseResult<PaginatedResult<List<IssueLogDto>>>> GetIssueLogsAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<IssueLogDto>> GetIssueLogByIdAsync(Guid issLogId);
        Task<BaseResult<IssueLogsCreateDto>> CreateIssueLogAsync(IssueLogsCreateDto dto);
        Task<BaseResult<IssueLogsUpdateDto>> UpdateIssueLogAsync(Guid issLogId, IssueLogsUpdateDto dto);
        Task<BaseResult<bool>> DeleteIssueLogsAsync(List<Guid> issLogId, bool softDelete = true);
    }
}
