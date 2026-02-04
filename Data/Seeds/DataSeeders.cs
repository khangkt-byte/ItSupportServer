using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    /// <summary>
    /// Centralized seeder orchestrator
    /// Pattern: Master seeder with dependency-ordered execution
    /// Use: Single entry point for all seed data
    /// Reference: Repository pattern, Seeder orchestration (Laravel, NestJS)
    /// </summary>
    public static class DataSeeders
    {
        /// <summary>
        /// Seed all data in correct dependency order
        /// Order: Master Data → Devices → Knowledge Base → Permissions → Users → Demo
        /// </summary>
        public static void SeedAllData(this ModelBuilder modelBuilder)
        {
            // ===== PHASE 1: Master Data (no dependencies) =====
            
            MasterDataSeeders.SeedMasterData(modelBuilder);
            // Seeds: Departments (5), Areas (5)

            // ===== PHASE 2: Devices (depends on DeviceTypes) =====
            
            DeviceSeeders.SeedDevices(modelBuilder);
            // Seeds: DeviceTypes (10), Devices (10)

            // ===== PHASE 3: Knowledge Base (Issues → Causes dependency) =====
            
            KnowledgeBaseSeeders.SeedKnowledgeBase(modelBuilder);
            // Seeds: Issues (15), Causes (15)

            // ===== PHASE 4: Security (Roles → Claims → RoleClaims) =====
            
            PermissionSeeders.SeedPermissions(modelBuilder);
            // Seeds: Roles (4), Claims (40+), RoleClaims

            // ===== PHASE 5: Users (depends on Departments, Areas, Roles) =====
            
            UserSeeders.SeedUsers(modelBuilder);
            // Seeds: Employees (2), Accounts (2), AccountRoles (2)

            // ===== PHASE 6: Demo Data (depends on everything) =====
            // Only seed in development/staging
            
            #if DEBUG
            DemoDataSeeders.SeedDemoData(modelBuilder);
            // Seeds: IssueLogs (4 sample tickets)
            #endif
        }

        /// <summary>
        /// Seed only production-required data (no demo data)
        /// Use: Production environments
        /// </summary>
        public static void SeedProductionData(this ModelBuilder modelBuilder)
        {
            MasterDataSeeders.SeedMasterData(modelBuilder);
            DeviceSeeders.SeedDevices(modelBuilder);
            KnowledgeBaseSeeders.SeedKnowledgeBase(modelBuilder);
            PermissionSeeders.SeedPermissions(modelBuilder);
            UserSeeders.SeedUsers(modelBuilder);
            
            // ❌ NO DemoDataSeeders for production
        }

        /// <summary>
        /// Seed minimal data (only required for app to function)
        /// Use: CI/CD testing, minimal setup
        /// </summary>
        public static void SeedMinimalData(this ModelBuilder modelBuilder)
        {
            // Only permissions and 1 admin user
            PermissionSeeders.SeedPermissions(modelBuilder);
            UserSeeders.SeedUsers(modelBuilder);
            
            // User must create master data manually
        }
    }
}
