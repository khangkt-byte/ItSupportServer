namespace ItSupportServer.src.Shared.Base
{
    /// <summary>
    /// Bulk delete operation result
    /// Pattern: Shared infrastructure DTO (Microsoft eShopOnContainers, Azure APIs)
    /// 
    /// This DTO is SHARED across all modules because:
    /// - Generic structure (not domain-specific)
    /// - Stable contract (unlikely to change)
    /// - Infrastructure concern (technical, not business)
    /// - Reusable across ALL bulk delete operations
    /// 
    /// References:
    /// - Microsoft eShopOnContainers: BuildingBlocks pattern
    /// - Azure REST API Guidelines: Common types for batch operations
    /// - Google API Design: Standard message types
    /// </summary>
    public record BulkDeleteResultDto
    {
        /// <summary>
        /// Indicates if ALL items were deleted successfully
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Number of items successfully deleted
        /// </summary>
        public int DeletedCount { get; init; }

        /// <summary>
        /// Total number of items requested for deletion
        /// </summary>
        public int TotalRequested { get; init; }

        /// <summary>
        /// Human-readable result message
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Optional: IDs of deleted items
        /// </summary>
        //public List<int>? DeletedIds { get; init; }
    }
}