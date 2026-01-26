using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.IssueLog
{
    public interface IIssueLogsService
    {
        Task<BaseResult<PaginatedResult<List<IssueLogDto>>>> GetIssueLogsAsync(string? query, int page, int pageSize, SortOBJ? sort);
        Task<BaseResult<IssueLogDto>> GetIssueLogByIdAsync(Guid issLogId);
        Task<BaseResult<CreateIssueLogDto>> CreateIssueLogAsync(CreateIssueLogDto dto);
        Task<BaseResult<UpdateIssueLogDto>> UpdateIssueLogAsync(Guid issLogId, UpdateIssueLogDto dto);
        Task<BaseResult<bool>> DeleteIssueLogsAsync(List<Guid> issLogId, bool softDelete = true);
    }
}
