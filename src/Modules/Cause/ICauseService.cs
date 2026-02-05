using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;

namespace ItSupportServer.src.Modules.Cause
{
    public interface ICauseService
    {
        /// <summary>
        /// Get paginated list of causes
        /// </summary>
        Task<PaginatedResult<ListCauseDto>> GetCausesAsync(QueryParameters parameters);

        /// <summary>
        /// Get cause by ID
        /// </summary>
        Task<CauseDto> GetCauseByIdAsync(long causeId);

        /// <summary>
        /// Get causes by issue ID
        /// </summary>
        Task<List<CauseDto>> GetCausesByIssueIdAsync(long issId);

        /// <summary>
        /// Create new cause
        /// </summary>
        Task<CauseDto> CreateCauseAsync(CreateCauseDto dto);

        /// <summary>
        /// Update existing cause
        /// </summary>
        Task<CauseDto> UpdateCauseAsync(long causeId, UpdateCauseDto dto);

        /// <summary>
        /// Delete single cause (soft delete by default)
        /// Pattern: RESTful single resource delete
        /// Reference: Microsoft REST API Guidelines
        /// </summary>
        Task DeleteCauseAsync(long causeId, bool softDelete = true);

        /// <summary>
        /// Delete multiple causes (all-or-nothing transaction)
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        Task<BulkDeleteResultDto> DeleteCausesAsync(List<long> causeIds, bool softDelete = true);

        /// <summary>
        /// Get cause suggestions for autocomplete (used by IssueLog)
        /// </summary>
        Task<List<CauseSuggestionDto>> GetCauseSuggestionsAsync(long issId, string? search = null);
    }
}