using ItSupportServer.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace ItSupportServer.Data.Models
{
    /// <summary>
    /// Design-time DbContext factory for EF Core tools
    /// Pattern: Factory pattern for design-time services
    /// Purpose: Enable migrations, scaffolding, and database commands
    /// References:
    /// - Microsoft Docs: Design-time DbContext Creation
    /// - EF Core: https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Create DbContext instance for design-time tools
        /// Called by: dotnet ef migrations add, dotnet ef database update, etc.
        /// </summary>
        public AppDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            // ✅ Build configuration to read appsettings.json
            // Pattern: Configuration builder (ASP.NET Core)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // ✅ Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in appsettings.json");

            // ✅ Create Npgsql data source (same as Program.cs)
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            // ✅ Build DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(dataSource, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));

            // ✅ Add interceptors (same as Program.cs)
            optionsBuilder.AddInterceptors(new AuditInterceptor());

            // ✅ Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}