using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Models.Configurations
{
    /// <summary>
    /// Auto-configuration for auditable entities
    /// Pattern: Convention-based configuration with Interface Segregation
    /// Use: Automatically configure CreatedAt, UpdatedAt, DeletedAt based on interfaces
    /// Reference: SOLID ISP, ABP Framework pattern, EF Core conventions
    /// </summary>
    public static class AuditConfiguration
    {
        /// <summary>
        /// Configure audit fields for all entities implementing audit interfaces
        /// 
        /// Configures:
        /// - ICreatableEntity: CreatedAt only
        /// - IModifiableEntity: CreatedAt + UpdatedAt
        /// - IAuditableEntity: CreatedAt + UpdatedAt + DeletedAt
        /// </summary>
        public static ModelBuilder ConfigureAuditableEntities(this ModelBuilder modelBuilder)
        {
            // ===== AUTO-CONFIGURE ICreatableEntity (CreatedAt only) =====

            var creatableTypes = modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(ICreatableEntity).IsAssignableFrom(et.ClrType))
                .Where(et => !typeof(IModifiableEntity).IsAssignableFrom(et.ClrType));

            foreach (var entityType in creatableTypes)
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(ICreatableEntity.CreatedAt))
                       .HasColumnName("created_at")
                       .IsRequired();
            }

            // ===== AUTO-CONFIGURE IModifiableEntity (CreatedAt + UpdatedAt) =====

            var modifiableTypes = modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(IModifiableEntity).IsAssignableFrom(et.ClrType))
                .Where(et => !typeof(ISoftDeletableEntity).IsAssignableFrom(et.ClrType));

            foreach (var entityType in modifiableTypes)
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(ICreatableEntity.CreatedAt))
                       .HasColumnName("created_at")
                       .IsRequired();

                builder.Property(nameof(IModifiableEntity.UpdatedAt))
                       .HasColumnName("updated_at");
            }

            // ===== AUTO-CONFIGURE IAuditableEntity (Full audit trail) =====

            var auditableTypes = modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(IAuditableEntity).IsAssignableFrom(et.ClrType));

            foreach (var entityType in auditableTypes)
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(ICreatableEntity.CreatedAt))
                       .HasColumnName("created_at")
                       .IsRequired();

                builder.Property(nameof(IModifiableEntity.UpdatedAt))
                       .HasColumnName("updated_at");

                builder.Property(nameof(ISoftDeletableEntity.DeletedAt))
                       .HasColumnName("deleted_at");
            }

            return modelBuilder;
        }
    }
}