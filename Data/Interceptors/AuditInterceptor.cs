using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ItSupportServer.Data.Interceptors
{
    /// <summary>
    /// Automatically sets CreatedAt and UpdatedAt timestamps for entities implementing IAuditableEntity
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

            var now = DateTime.UtcNow;

            var entries = context.ChangeTracker
                .Entries<IAuditableEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    // Set CreatedAt only on new entities
                    entry.Entity.CreatedAt = now;
                }

                if (entry.State == EntityState.Modified)
                {
                    // Set UpdatedAt only on modified entities
                    entry.Entity.UpdatedAt = now;
                    
                    // Prevent CreatedAt from being modified
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                }
            }
        }
    }
}