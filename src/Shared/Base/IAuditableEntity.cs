namespace ItSupportServer.src.Shared.Base
{
    /// <summary>
    /// Marker interface for entities that support automatic audit timestamps
    /// </summary>
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}