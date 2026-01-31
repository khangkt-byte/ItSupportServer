using ItSupportServer.Data.Models.Configurations;
using ItSupportServer.Data.Models.Entities;
using ItSupportServer.Data.Seeds;
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
        public DbSet<PasswordResetTokens> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== CONFIGURATION =====

            // ✅ PostgreSQL extensions
            modelBuilder.HasPostgresExtension("pg_trgm");

            // ✅ Auto-configure audit fields
            modelBuilder.ConfigureAuditableEntities();

            // ✅ Apply all entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // ===== SEED DATA =====

            // ✅ SIMPLE: One line to seed everything!
            modelBuilder.SeedAllData();

            // ✅ ALTERNATIVE: Environment-specific
            // modelBuilder.SeedProductionData();  // For production
            // modelBuilder.SeedMinimalData();      // For testing
        }
    }
}
