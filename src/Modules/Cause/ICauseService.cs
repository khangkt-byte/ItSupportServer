using ItSupportServer.src.Shared.Base;

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
        /// Delete causes
        /// </summary>
        Task<bool> DeleteCausesAsync(List<long> causeIds, bool softDelete = true);

        /// <summary>
        /// Get cause suggestions for autocomplete (used by IssueLog)
        /// </summary>
        Task<List<CauseSuggestionDto>> GetCauseSuggestionsAsync(long issId, string? search = null);
    }
}