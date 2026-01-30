using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employees> Employees { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Areas> Areas { get; set; }
        public DbSet<DeviceTypes> DeviceTypes { get; set; }
        public DbSet<Devices> Devices { get; set; }
        public DbSet<IssueLogs> IssueLogs { get; set; }
        public DbSet<IssueLogOperators> IssueLogOperators { get; set; }
        public DbSet<IssueLogRequesters> IssueLogRequesters { get; set; }
        public DbSet<Issues> Issues { get; set; }
        public DbSet<Causes> Causes { get; set; }
        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Claims> Claims { get; set; }
        public DbSet<RoleClaims> RoleClaims { get; set; }
        public DbSet<AccountRoles> AccountRoles { get; set; }
        public DbSet<AccountClaims> AccountClaims { get; set; }
        public DbSet<AccountTokens> AccountTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("pg_trgm");

            // Lấy tất cả các Entity Type đang thực thi IAuditableEntity
            var auditableTypes = modelBuilder.Model.GetEntityTypes()
                .Where(et => typeof(IAuditableEntity).IsAssignableFrom(et.ClrType));

            foreach (var entityType in auditableTypes)
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(IAuditableEntity.CreatedAt))
                       .HasColumnName("created_at")
                       .IsRequired();

                builder.Property(nameof(IAuditableEntity.UpdatedAt))
                       .HasColumnName("updated_at");

                builder.Property(nameof(IAuditableEntity.DeletedAt))
                       .HasColumnName("deleted_at");
            }

            // Configure your entity mappings here
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
