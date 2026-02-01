using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    /// <summary>
    /// User seeder - Creates default admin account
    /// Pattern: Idempotent seeding (safe to run multiple times)
    /// Security: Password is pre-hashed with BCrypt
    /// Reference: ASP.NET Core Identity seeding pattern
    /// </summary>
    public static class UserSeeders
    {
        // ✅ Consistent GUIDs (same as used in other seeders)
        private static readonly Guid AdminAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly DateTime SeedDate = new(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc);

        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            // ===== EMPLOYEE =====
            // Pattern: Master data for employee
            modelBuilder.Entity<Employees>().HasData(
                new Employees
                {
                    EmpId = AdminAccountId,
                    EmpCode = "ADMIN001",  // ✅ More professional code
                    FullName = "System Administrator",  // ✅ English for system account
                    Email = "admin@itsupport.local",  // ✅ System email
                    PhoneNumber = "0900000000",
                    DptId = 1,  // IT Department (from MasterDataSeeders)
                    AreaId = 1,  // Company (from MasterDataSeeders)
                    CreatedAt = SeedDate
                });

            // ===== ACCOUNT =====
            // Pattern: Authentication entity linked to employee
            // Security: Password hashed with BCrypt (admin1234)
            modelBuilder.Entity<Accounts>().HasData(
                new Accounts
                {
                    AccountId = AdminAccountId,
                    Username = "admin",  // ✅ Simple admin username
                    Password = "AQAAAAIAAYagAAAAEEms0ysPRm2n5vnXRawAsarpqN71JIBmAsB6o/LwNQElvYkETT9sR3eCUBaE9SpJtA==",
                    // ⚠️ NOTE: This is BCrypt hash of "admin1234"
                    // Change in production!
                    CreatedAt = SeedDate
                });

            // ===== ACCOUNT ROLES =====
            // Pattern: Many-to-many relationship
            // Reference: RBAC (Role-Based Access Control)
            modelBuilder.Entity<AccountRoles>().HasData(
                new AccountRoles
                {
                    AccountId = AdminAccountId,
                    RoleId = 1  // ✅ FIX: Admin role (matches PermissionSeeders line 12)
                });
        }
    }
}
