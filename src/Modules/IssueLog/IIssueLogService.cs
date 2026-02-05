using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;

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
        /// Delete single issue log (soft delete by default)
        /// Pattern: RESTful single resource delete
        /// Reference: Microsoft REST API Guidelines
        /// </summary>
        Task DeleteIssueLogAsync(Guid issLogId, bool softDelete = true);

        /// <summary>
        /// Delete multiple issue logs (all-or-nothing transaction)
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        Task<BulkDeleteResultDto> DeleteIssueLogsAsync(List<Guid> issLogIds, bool softDelete = true);
    }
}
