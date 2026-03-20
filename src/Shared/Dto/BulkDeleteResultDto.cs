namespace ItSupportServer.src.Shared.Dto
{
    /// <summary>
    /// Result DTO for bulk delete operations.
    /// Shared across all modules (generic, not domain-specific).
    /// </summary>
    public record BulkDeleteResultDto
    {
        public bool Success { get; init; }
        public int DeletedCount { get; init; }
        public int TotalRequested { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}