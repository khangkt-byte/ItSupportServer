using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ItSupportServer.Data.Interceptors
{
    /// <summary>
    /// Automatically sets audit timestamps based on interface implementation
    /// Pattern: EF Core SaveChanges interceptor with Interface Segregation
    /// Security: Ensures audit trail integrity, prevents manual tampering
    /// Reference: Microsoft EF Core best practices, OWASP logging guidelines
    /// </summary>
    public class AuditInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void UpdateAuditFields(DbContext? context)
        {
            if (context == null) return;

            // ✅ Single time source for all audit fields
            var now = DateTime.UtcNow;

            // ===== HANDLE ICreatableEntity (CreatedAt only) =====
            
            var creatableEntries = context.ChangeTracker
                .Entries<ICreatableEntity>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in creatableEntries)
            {
                // ✅ Set CreatedAt for all new entities
                entry.Entity.CreatedAt = now;
            }

            // ===== HANDLE IModifiableEntity (CreatedAt + UpdatedAt) =====
            
            var modifiableEntries = context.ChangeTracker
                .Entries<IModifiableEntity>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in modifiableEntries)
            {
                // ✅ Set UpdatedAt on modifications
                entry.Entity.UpdatedAt = now;

                // ✅ Prevent CreatedAt from being modified (immutability)
                entry.Property(nameof(ICreatableEntity.CreatedAt)).IsModified = false;
            }

            // Note: Soft delete (DeletedAt) handled by service layer, not interceptor
            // Reason: Soft delete is a business operation, not automatic behavior
        }
    }
}