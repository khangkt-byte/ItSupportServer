using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Issue log service interface
    /// Pattern: Repository pattern (Martin Fowler)
    /// Reference: Microsoft ASP.NET Core best practices
    /// </summary>
    public interface IIssueLogService
    {
        /// <summary>
        /// Get paginated list of issue logs
        /// Pattern: ServiceNow table API
        /// </summary>
        Task<PaginatedResult<IssueLogDto>> GetIssueLogsAsync(QueryParameters parameters);

        /// <summary>
        /// Get issue log by ID
        /// Pattern: ServiceNow get by sys_id
        /// </summary>
        Task<IssueLogDto> GetIssueLogByIdAsync(Guid issLogId);

        /// <summary>
        /// Create new issue log
        /// Pattern: ServiceNow incident create
        /// Security: Validates all FKs, auto-matches to KB if possible
        /// </summary>
        Task<IssueLogDto> CreateIssueLogAsync(CreateIssueLogDto dto);

        /// <summary>
        /// Update existing issue log (partial update supported)
        /// Pattern: ServiceNow incident update (PATCH)
        /// </summary>
        Task<IssueLogDto> UpdateIssueLogAsync(Guid issLogId, UpdateIssueLogDto dto);

        /// <summary>
        /// Delete issue logs (soft delete by default)
        /// Pattern: ServiceNow soft delete (deleted_at field)
        /// </summary>
        Task<bool> DeleteIssueLogsAsync(List<Guid> issLogIds, bool softDelete = true);
    }
}
