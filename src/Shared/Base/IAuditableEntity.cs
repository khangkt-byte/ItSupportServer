namespace ItSupportServer.src.Shared.Base
{
    // ===== GRANULAR INTERFACES (Interface Segregation Principle) =====

    /// <summary>
    /// Marker interface for entities that track creation time
    /// Use for: Immutable entities, tokens, logs
    /// Pattern: Interface Segregation Principle (SOLID)
    /// </summary>
    public interface ICreatableEntity
    {
        DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Marker interface for entities that track modifications
    /// Use for: Mutable business entities
    /// </summary>
    public interface IModifiableEntity : ICreatableEntity
    {
        DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Marker interface for entities that support soft delete
    /// Use for: Business entities requiring soft delete
    /// </summary>
    public interface ISoftDeletableEntity
    {
        DateTime? DeletedAt { get; set; }
    }

    /// <summary>
    /// Full audit trail (creation + modification + soft delete)
    /// Use for: Primary business entities (Employees, Departments, etc.)
    /// Pattern: Composition of smaller interfaces
    /// </summary>
    public interface IAuditableEntity : IModifiableEntity, ISoftDeletableEntity
    {
        // Inherits:
        // - DateTime CreatedAt (from ICreatableEntity via IModifiableEntity)
        // - DateTime? UpdatedAt (from IModifiableEntity)
        // - DateTime? DeletedAt (from ISoftDeletableEntity)
    }
}