using Microsoft.EntityFrameworkCore;

namespace ITSupportServer.Data
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure your entity mappings here
        }
    }
}
