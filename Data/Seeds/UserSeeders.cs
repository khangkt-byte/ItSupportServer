using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    /// <summary>
    /// User seeder - Creates default accounts for all roles
    /// Pattern: Idempotent seeding (safe to run multiple times)
    /// Security: Passwords are pre-hashed with BCrypt (Work Factor: 12)
    /// Reference: ASP.NET Core Identity seeding pattern
    /// 
    /// DEFAULT ACCOUNTS (FOR TESTING ONLY):
    /// ┌──────────┬──────────┬──────────────┬─────────────────────┐
    /// │ Username │ Password │ Role         │ Permissions         │
    /// ├──────────┼──────────┼──────────────┼─────────────────────┤
    /// │ admin    │ Admin@123│ Admin        │ Full Access         │
    /// │ manager  │ Mgr@123  │ Manager      │ Team Management     │
    /// │ employee │ Emp@123  │ Employee     │ Standard CRUD       │
    /// │ viewer   │ View@123 │ Viewer       │ Read-Only           │
    /// └──────────┴──────────┴──────────────┴─────────────────────┘
    /// 
    /// ⚠️ SECURITY WARNING: 
    /// - These are DEMO accounts
    /// - Change ALL passwords before production deployment
    /// - Consider deleting demo accounts in production
    /// 
    /// Pattern Reference:
    /// - Microsoft Identity: Default admin seeding
    /// - Auth0: Multi-tenant demo users
    /// - Okta: Sample organization structure
    /// </summary>
    public static class UserSeeders
    {
        // ✅ Deterministic GUIDs for seeding (same across migrations)
        private static readonly Guid AdminAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ManagerAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid EmployeeAccountId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid ViewerAccountId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        private static readonly DateTime SeedDate = new(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc);

        // Role IDs (must match PermissionSeeders)
        private const int AdminRoleId = 1;
        private const int EmployeeRoleId = 2;
        private const int ManagerRoleId = 3;
        private const int ViewerRoleId = 4;

        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            SeedEmployees(modelBuilder);
            SeedAccounts(modelBuilder);
            SeedAccountRoles(modelBuilder);
        }

        /// <summary>
        /// Seed employee entities (HR master data)
        /// Pattern: One employee per role for testing
        /// </summary>
        private static void SeedEmployees(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employees>().HasData(
                // ===== 1. ADMIN EMPLOYEE =====
                // Pattern: System account (protected)
                new Employees
                {
                    EmpId = AdminAccountId,
                    EmpCode = "ADMIN001",
                    FullName = "System Administrator",
                    Email = "admin@itsupport.local",
                    PhoneNumber = "0900000001",
                    Position = "Super_Admin",  // ✅ Protected position (cannot delete)
                    DptId = 1,  // IT Department (from MasterDataSeeders)
                    AreaId = 1,  // Company (from MasterDataSeeders)
                    CreatedAt = SeedDate
                },

                // ===== 2. MANAGER EMPLOYEE =====
                // Pattern: Department head with extended permissions
                new Employees
                {
                    EmpId = ManagerAccountId,
                    EmpCode = "MGR001",
                    FullName = "Nguyễn Văn Quản Lý",
                    Email = "manager@itsupport.local",
                    PhoneNumber = "0900000002",
                    Position = "IT Manager",
                    DptId = 1,  // IT Department
                    AreaId = 1,  // Company
                    CreatedAt = SeedDate
                },

                // ===== 3. EMPLOYEE (STANDARD) =====
                // Pattern: Standard worker with basic permissions
                new Employees
                {
                    EmpId = EmployeeAccountId,
                    EmpCode = "EMP001",
                    FullName = "Trần Thị Nhân Viên",
                    Email = "employee@itsupport.local",
                    PhoneNumber = "0900000003",
                    Position = "IT Support Staff",
                    DptId = 1,  // IT Department
                    AreaId = 1,  // Company
                    CreatedAt = SeedDate
                },

                // ===== 4. VIEWER (READ-ONLY) =====
                // Pattern: Auditor/Reporter with read-only access
                new Employees
                {
                    EmpId = ViewerAccountId,
                    EmpCode = "VIEW001",
                    FullName = "Lê Văn Xem",
                    Email = "viewer@itsupport.local",
                    PhoneNumber = "0900000004",
                    Position = "IT Auditor",
                    DptId = 1,  // IT Department
                    AreaId = 1,  // Company
                    CreatedAt = SeedDate
                }
            );
        }

        /// <summary>
        /// Seed account entities (authentication data)
        /// Security: BCrypt hashes with Work Factor 12
        /// Reference: OWASP Password Storage Cheat Sheet
        /// </summary>
        private static void SeedAccounts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accounts>().HasData(
                // ===== 1. ADMIN ACCOUNT =====
                // Username: admin | Password: admin123
                // BCrypt online: https://bcrypt-generator.com/ (Rounds: 12)
                new Accounts
                {
                    AccountId = AdminAccountId,
                    Username = "admin",
                    // ✅ BCrypt hash of "admin123" (Work Factor: 12)
                    // Generated using: BCrypt.Net.BCrypt.HashPassword("admin123", 12)
                    Password = "$2a$12$6exAjNpv8RlZNtYhvgt0oOd0r.D.C2NaKASd34aqMgwjfFhI7aSk.",
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    CreatedAt = SeedDate
                },

                // ===== 2. MANAGER ACCOUNT =====
                // Username: manager | Password: manager123
                new Accounts
                {
                    AccountId = ManagerAccountId,
                    Username = "manager",
                    // ✅ BCrypt hash of "manager123" (Work Factor: 12)
                    Password = "$2a$12$Ojrekj8GLlfpt4vrT0PO9eTpg8nTWpnBcungeJdlEhxTStZfVMQK.",
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    CreatedAt = SeedDate
                },

                // ===== 3. EMPLOYEE ACCOUNT =====
                // Username: employee | Password: employee123
                new Accounts
                {
                    AccountId = EmployeeAccountId,
                    Username = "employee",
                    // ✅ BCrypt hash of "employee123" (Work Factor: 12)
                    Password = "$2a$12$1d4.6EhIqH1fMu/M3FE0gecU.dPNWkYA2HKe8wwNcQCtBTeNjAGse",
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    CreatedAt = SeedDate
                },

                // ===== 4. VIEWER ACCOUNT =====
                // Username: viewer | Password: viewer123
                new Accounts
                {
                    AccountId = ViewerAccountId,
                    Username = "viewer",
                    // ✅ BCrypt hash of "viewer123" (Work Factor: 12)
                    Password = "$2a$12$GHnHxA3xSTKRoAEfVwn7zOEm2j0fpsnNd3xUGNXBomkJTDy74HeGK",
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    CreatedAt = SeedDate
                }
            );
        }

        /// <summary>
        /// Seed account-role mappings (many-to-many)
        /// Pattern: RBAC (Role-Based Access Control)
        /// Reference: Microsoft Identity, Auth0, Okta
        /// </summary>
        private static void SeedAccountRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountRoles>().HasData(
                // ===== ADMIN → Admin Role =====
                // Has "Admin" claim → bypasses all permission checks
                new AccountRoles
                {
                    AccountId = AdminAccountId,
                    RoleId = AdminRoleId
                },

                // ===== MANAGER → Manager Role =====
                // Has extended permissions (IssueLog CRUD, KB management)
                new AccountRoles
                {
                    AccountId = ManagerAccountId,
                    RoleId = ManagerRoleId
                },

                // ===== EMPLOYEE → Employee Role =====
                // Has standard permissions (IssueLog Create/Edit, View only for others)
                new AccountRoles
                {
                    AccountId = EmployeeAccountId,
                    RoleId = EmployeeRoleId
                },

                // ===== VIEWER → Viewer Role =====
                // Has read-only permissions (View all, no modifications)
                new AccountRoles
                {
                    AccountId = ViewerAccountId,
                    RoleId = ViewerRoleId
                }
            );
        }
    }
}
