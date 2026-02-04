using System.ComponentModel.DataAnnotations;

namespace ItSupportServer.src.Modules.Area
{
    /// <summary>
    /// Area response DTO
    /// </summary>
    public record AreaDto
    {
        public int AreaId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    /// <summary>
    /// Create area request DTO
    /// </summary>
    public record CreateAreaDto
    {
        /// <summary>
        /// Area name (required)
        /// </summary>
        public required string Name { get; init; }  // ✅ required for Create

        /// <summary>
        /// Area description (optional)
        /// </summary>
        public string? Description { get; init; }
    }

    /// <summary>
    /// Update area request DTO (supports partial updates)
    /// </summary>
    public record UpdateAreaDto
    {
        /// <summary>
        /// Area name (optional - null means don't update)
        /// </summary>
        public string? Name { get; init; }  // ✅ nullable for partial update

        /// <summary>
        /// Area description (optional - null means don't update, empty string means clear)
        /// </summary>
        public string? Description { get; init; }
    }
}
