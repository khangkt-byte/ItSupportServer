using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Models.Configurations
{
    /// <summary>
    /// Auto-configuration for auditable entities
    /// Pattern: Convention-based configuration with Interface Segregation Principle
    /// Purpose: Auto-configure audit fields + performance indexes
    /// References:
    /// - SOLID ISP: Interface Segregation Principle
    /// - ABP Framework: Auditing pattern
    /// - PostgreSQL: Partial index optimization
    /// - Microsoft: EF Core conventions
    /// </summary>
    public static class AuditConfiguration
    {
        /// <summary>
        /// Configure audit fields for all entities implementing audit interfaces
        /// Auto-applies:
        /// - Column name mapping (snake_case)
        /// - Required constraints
        /// - Performance indexes (partial indexes on deleted_at)
        /// </summary>
        public static ModelBuilder ConfigureAuditableEntities(this ModelBuilder modelBuilder)
        {
            // ===== CONFIGURE IAuditableEntity (CreatedAt + UpdatedAt + DeletedAt) =====
            // Most business entities use this interface
            
            var auditableTypes = modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(IAuditableEntity).IsAssignableFrom(et.ClrType));

            foreach (var entityType in auditableTypes)
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                // CreatedAt: Required, indexed
                builder.Property(nameof(ICreatableEntity.CreatedAt))
                       .HasColumnName("created_at")
                       .IsRequired();

                // UpdatedAt: Optional (null until first update)
                builder.Property(nameof(IModifiableEntity.UpdatedAt))
                       .HasColumnName("updated_at");

                // DeletedAt: Optional (null = active, value = deleted)
                builder.Property(nameof(ISoftDeletableEntity.DeletedAt))
                       .HasColumnName("deleted_at");

                // ✅ AUTO-CREATE PERFORMANCE INDEX
                // Pattern: Partial index for soft delete queries
                // Purpose: Optimize "WHERE deleted_at IS NULL" queries
                // Reference: PostgreSQL partial indexes (90% smaller than full index)
                // https://www.postgresql.org/docs/current/indexes-partial.html
                var tableName = entityType.GetTableName();
                builder.HasIndex(nameof(ISoftDeletableEntity.DeletedAt))
                       .HasDatabaseName($"ix_{tableName}_deleted_at")
                       .HasFilter("deleted_at IS NULL");  // ✅ FIXED: snake_case
            }

            // ===== CONFIGURE IModifiableEntity ONLY (CreatedAt + UpdatedAt, NO DeletedAt) =====
            // Use case: Entities that update but don't soft delete
            // Example: AccountTokens, PasswordResetTokens, audit logs
            
            var modifiableOnlyTypes = modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(IModifiableEntity).IsAssignableFrom(et.ClrType))
                .Where(et => !typeof(ISoftDeletableEntity).IsAssignableFrom(et.ClrType));

            foreach (var entityType in modifiableOnlyTypes)
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(ICreatableEntity.CreatedAt))
                       .HasColumnName("created_at")
                       .IsRequired();

                builder.Property(nameof(IModifiableEntity.UpdatedAt))
                       .HasColumnName("updated_at");
            }

            // ===== CONFIGURE ICreatableEntity ONLY (CreatedAt, NO UpdatedAt) =====
            // Use case: Immutable entities (write-once, never update)
            // Example: Audit logs, transaction records, historical data
            
            var creatableOnlyTypes = modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(ICreatableEntity).IsAssignableFrom(et.ClrType))
                .Where(et => !typeof(IModifiableEntity).IsAssignableFrom(et.ClrType));

            foreach (var entityType in creatableOnlyTypes)
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(ICreatableEntity.CreatedAt))
                       .HasColumnName("created_at")
                       .IsRequired();
                
                // ✅ Optional: Index on CreatedAt for time-based queries
                // Useful for: ORDER BY created_at DESC queries
                var tableName = entityType.GetTableName();
                builder.HasIndex(nameof(ICreatableEntity.CreatedAt))
                       .HasDatabaseName($"ix_{tableName}_created_at");
            }

            return modelBuilder;
        }
    }
}