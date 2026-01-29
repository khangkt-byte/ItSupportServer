using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.IssueLog
{
    public interface IIssueLogService
    {
        /// <summary>
        /// Get paginated list of issue logs
        /// </summary>
        Task<PaginatedResult<IssueLogDto>> GetIssueLogsAsync(QueryParameters parameters);

        /// <summary>
        /// Get issue log by ID
        /// </summary>
        Task<IssueLogDto> GetIssueLogByIdAsync(Guid issLogId);

        /// <summary>
        /// Create new issue log
        /// </summary>
        Task<IssueLogDto> CreateIssueLogAsync(CreateIssueLogDto dto);

        /// <summary>
        /// Update existing issue log (partial update supported)
        /// </summary>
        Task<IssueLogDto> UpdateIssueLogAsync(Guid issLogId, UpdateIssueLogDto dto);

        /// <summary>
        /// Delete issue logs (soft delete by default)
        /// </summary>
        Task<bool> DeleteIssueLogsAsync(List<Guid> issLogIds, bool softDelete = true);
    }
}
