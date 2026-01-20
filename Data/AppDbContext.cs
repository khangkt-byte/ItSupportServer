using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data
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
        public DbSet<Issues> Issues { get; set; }
        public DbSet<Reasons> Reasons { get; set; }
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
            // Configure your entity mappings here
        }
    }
}
