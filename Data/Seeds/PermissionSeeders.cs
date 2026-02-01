using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Modules.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    public static class PermissionSeeders
    {
        // Role ID constants
        private const int AdminRoleId = 1;
        private const int EmployeeRoleId = 2;
        private const int ManagerRoleId = 3;
        private const int ViewerRoleId = 4;

        // Claim ID ranges (for organization)
        private const int AdminClaimId = 1;
        private const int AreaClaimIdStart = 2;
        private const int DepartmentClaimIdStart = 6;
        private const int EmployeeClaimIdStart = 10;
        private const int AccountClaimIdStart = 14;
        private const int RoleClaimIdStart = 18;
        private const int IssueLogClaimIdStart = 22;
        private const int IssueClaimIdStart = 26;
        private const int CauseClaimIdStart = 30;
        private const int DeviceClaimIdStart = 34;
        private const int DeviceTypeClaimIdStart = 38;

        private static readonly DateTime SeedDate = new(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc);

        public static void SeedPermissions(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedClaims(modelBuilder);
            SeedRoleClaims(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Roles>().HasData(
                new Roles 
                { 
                    RoleId = AdminRoleId, 
                    Name = "Admin",
                    Description = "System Administrator - Full access to all features",
                    CreatedAt = SeedDate 
                },
                new Roles 
                { 
                    RoleId = EmployeeRoleId, 
                    Name = "Employee",
                    Description = "Standard Employee - Create and manage issue logs",
                    CreatedAt = SeedDate 
                },
                new Roles 
                { 
                    RoleId = ManagerRoleId, 
                    Name = "Manager",
                    Description = "Department Manager - Extended permissions for team management",
                    CreatedAt = SeedDate 
                },
                new Roles 
                { 
                    RoleId = ViewerRoleId, 
                    Name = "Viewer",
                    Description = "Read-Only User - View reports and statistics",
                    CreatedAt = SeedDate 
                }
            );
        }

        private static void SeedClaims(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Claims>().HasData(
                // Admin superuser claim
                new Claims { ClaimId = AdminClaimId, Claim = "Admin" },

                // Area permissions (2-5)
                new Claims { ClaimId = 2, Claim = Permissions.AreaClaims.View },
                new Claims { ClaimId = 3, Claim = Permissions.AreaClaims.Create },
                new Claims { ClaimId = 4, Claim = Permissions.AreaClaims.Edit },
                new Claims { ClaimId = 5, Claim = Permissions.AreaClaims.Delete },

                // Department permissions (6-9)
                new Claims { ClaimId = 6, Claim = Permissions.DepartmentClaims.View },
                new Claims { ClaimId = 7, Claim = Permissions.DepartmentClaims.Create },
                new Claims { ClaimId = 8, Claim = Permissions.DepartmentClaims.Edit },
                new Claims { ClaimId = 9, Claim = Permissions.DepartmentClaims.Delete },

                // Employee permissions (10-13)
                new Claims { ClaimId = 10, Claim = Permissions.EmployeeClaims.View },
                new Claims { ClaimId = 11, Claim = Permissions.EmployeeClaims.Create },
                new Claims { ClaimId = 12, Claim = Permissions.EmployeeClaims.Edit },
                new Claims { ClaimId = 13, Claim = Permissions.EmployeeClaims.Delete },

                // Account permissions (14-17)
                new Claims { ClaimId = 14, Claim = Permissions.AccountClaims.View },
                new Claims { ClaimId = 15, Claim = Permissions.AccountClaims.Create },
                new Claims { ClaimId = 16, Claim = Permissions.AccountClaims.Edit },
                new Claims { ClaimId = 17, Claim = Permissions.AccountClaims.Delete },

                // Role permissions (18-21)
                new Claims { ClaimId = 18, Claim = Permissions.RoleClaims.View },
                new Claims { ClaimId = 19, Claim = Permissions.RoleClaims.Create },
                new Claims { ClaimId = 20, Claim = Permissions.RoleClaims.Edit },
                new Claims { ClaimId = 21, Claim = Permissions.RoleClaims.Delete },

                // IssueLog permissions (22-25)
                new Claims { ClaimId = 22, Claim = Permissions.IssueLogClaims.View },
                new Claims { ClaimId = 23, Claim = Permissions.IssueLogClaims.Create },
                new Claims { ClaimId = 24, Claim = Permissions.IssueLogClaims.Edit },
                new Claims { ClaimId = 25, Claim = Permissions.IssueLogClaims.Delete },

                // Issue permissions (26-29)
                new Claims { ClaimId = 26, Claim = Permissions.IssueClaims.View },
                new Claims { ClaimId = 27, Claim = Permissions.IssueClaims.Create },
                new Claims { ClaimId = 28, Claim = Permissions.IssueClaims.Edit },
                new Claims { ClaimId = 29, Claim = Permissions.IssueClaims.Delete },

                // Cause permissions (30-33)
                new Claims { ClaimId = 30, Claim = Permissions.CauseClaims.View },
                new Claims { ClaimId = 31, Claim = Permissions.CauseClaims.Create },
                new Claims { ClaimId = 32, Claim = Permissions.CauseClaims.Edit },
                new Claims { ClaimId = 33, Claim = Permissions.CauseClaims.Delete },

                // Device permissions (34-37)
                new Claims { ClaimId = 34, Claim = Permissions.DeviceClaims.View },
                new Claims { ClaimId = 35, Claim = Permissions.DeviceClaims.Create },
                new Claims { ClaimId = 36, Claim = Permissions.DeviceClaims.Edit },
                new Claims { ClaimId = 37, Claim = Permissions.DeviceClaims.Delete },

                // DeviceType permissions (38-41)
                new Claims { ClaimId = 38, Claim = Permissions.DeviceTypeClaims.View },
                new Claims { ClaimId = 39, Claim = Permissions.DeviceTypeClaims.Create },
                new Claims { ClaimId = 40, Claim = Permissions.DeviceTypeClaims.Edit },
                new Claims { ClaimId = 41, Claim = Permissions.DeviceTypeClaims.Delete }
            );
        }

        private static void SeedRoleClaims(ModelBuilder modelBuilder)
        {
            var roleClaims = new List<RoleClaims>();

            // ===== ADMIN ROLE =====
            // Only needs Admin claim (bypass all checks)
            roleClaims.Add(new RoleClaims { RoleId = AdminRoleId, ClaimId = 1 });

            // ===== EMPLOYEE ROLE =====
            roleClaims.AddRange(GetEmployeeRoleClaims());

            // ===== MANAGER ROLE =====
            roleClaims.AddRange(GetManagerRoleClaims());

            // ===== VIEWER ROLE =====
            roleClaims.AddRange(GetViewerRoleClaims());

            modelBuilder.Entity<RoleClaims>().HasData(roleClaims);
        }

        private static IEnumerable<RoleClaims> GetEmployeeRoleClaims()
        {
            return new[]
            {
                // IssueLog (main feature)
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 22 },  // View
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 23 },  // Create
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 24 },  // Edit

                // Reference data (read-only)
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 26 },  // Issue.View
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 30 },  // Cause.View
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 34 },  // Device.View
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 10 },  // Employee.View
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 6 },   // Department.View
                new RoleClaims { RoleId = EmployeeRoleId, ClaimId = 2 }    // Area.View
            };
        }

        private static IEnumerable<RoleClaims> GetManagerRoleClaims()
        {
            return new[]
            {
                // All IssueLog permissions
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 22 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 23 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 24 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 25 },

                // Manage knowledge base
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 26 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 27 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 28 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 30 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 31 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 32 },

                // View organizational data
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 10 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 6 },
                new RoleClaims { RoleId = ManagerRoleId, ClaimId = 2 }
            };
        }

        private static IEnumerable<RoleClaims> GetViewerRoleClaims()
        {
            return new[]
            {
                // View-only permissions
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 22 },  // IssueLog.View
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 26 },  // Issue.View
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 30 },  // Cause.View
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 34 },  // Device.View
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 38 },  // DeviceType.View
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 10 },  // Employee.View
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 6 },   // Department.View
                new RoleClaims { RoleId = ViewerRoleId, ClaimId = 2 }    // Area.View
            };
        }
    }
}
