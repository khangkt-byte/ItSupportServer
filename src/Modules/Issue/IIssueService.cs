using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Issue
{
    public interface IIssueService
    {
        /// <summary>
        /// Get paginated list of issues
        /// </summary>
        Task<PaginatedResult<IssueDto>> GetIssuesAsync(QueryParameters parameters);

        /// <summary>
        /// Get issue by ID with causes
        /// </summary>
        Task<IssueDto> GetIssueByIdAsync(long issId);

        /// <summary>
        /// Create new issue
        /// </summary>
        Task<IssueDto> CreateIssueAsync(CreateIssueDto dto);

        /// <summary>
        /// Update existing issue
        /// </summary>
        Task<IssueDto> UpdateIssueAsync(long issId, UpdateIssueDto dto);

        /// <summary>
        /// Delete single issue (soft delete by default)
        /// Pattern: RESTful single resource delete
        /// Reference: Microsoft REST API Guidelines
        /// Business Rules: Cannot delete if has Causes or IssueLogs
        /// </summary>
        Task DeleteIssueAsync(long issId, bool softDelete = true);

        /// <summary>
        /// Delete multiple issues (all-or-nothing transaction)
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        Task<BulkDeleteResultDto> DeleteIssuesAsync(List<long> issIds, bool softDelete = true);

        /// <summary>
        /// Get issue suggestions for autocomplete (used by IssueLog)
        /// </summary>
        Task<List<IssueSuggestionDto>> GetIssueSuggestionsAsync(string? search = null);
    }
}
