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
        /// Delete issues
        /// </summary>
        Task<bool> DeleteIssuesAsync(List<long> issIds, bool softDelete = true);

        /// <summary>
        /// Get issue suggestions for autocomplete (used by IssueLog)
        /// </summary>
        Task<List<IssueSuggestionDto>> GetIssueSuggestionsAsync(string? search = null);
    }
}
