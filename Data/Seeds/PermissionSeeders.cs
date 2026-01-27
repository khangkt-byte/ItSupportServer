using ItSupportServer.Data.Models;
using ItSupportServer.src.Modules.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    public static class PermissionSeeders
    {
        public static void SeedPermissions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Roles>().HasData(
                new Roles { RoleId = 1, Name = "Admin", CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc) },
                new Roles { RoleId = 2, Name = "Employee", CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc) }

            );
            modelBuilder.Entity<Claims>().HasData(
                new Claims { ClaimId = 1, Claim = Permissions.AreaClaims.View },
                new Claims { ClaimId = 2, Claim = Permissions.AreaClaims.Create },
                new Claims { ClaimId = 3, Claim = Permissions.AreaClaims.Edit },
                new Claims { ClaimId = 4, Claim = Permissions.AreaClaims.Delete },

                new Claims { ClaimId = 5, Claim = Permissions.DepartmentClaims.View },
                new Claims { ClaimId = 6, Claim = Permissions.DepartmentClaims.Create },
                new Claims { ClaimId = 7, Claim = Permissions.DepartmentClaims.Edit },
                new Claims { ClaimId = 8, Claim = Permissions.DepartmentClaims.Delete },

                new Claims { ClaimId = 9, Claim = Permissions.EmployeeClaims.View },
                new Claims { ClaimId = 10, Claim = Permissions.EmployeeClaims.Create },
                new Claims { ClaimId = 11, Claim = Permissions.EmployeeClaims.Edit },
                new Claims { ClaimId = 12, Claim = Permissions.EmployeeClaims.Delete },

                new Claims { ClaimId = 13, Claim = Permissions.AccountClaims.View },
                new Claims { ClaimId = 14, Claim = Permissions.AccountClaims.Create },
                new Claims { ClaimId = 15, Claim = Permissions.AccountClaims.Edit },
                new Claims { ClaimId = 16, Claim = Permissions.AccountClaims.Delete },

                new Claims { ClaimId = 17, Claim = Permissions.RoleClaims.View },
                new Claims { ClaimId = 18, Claim = Permissions.RoleClaims.Create },
                new Claims { ClaimId = 19, Claim = Permissions.RoleClaims.Edit },
                new Claims { ClaimId = 20, Claim = Permissions.RoleClaims.Delete },

                new Claims { ClaimId = 21, Claim = Permissions.IssueLogClaims.View },
                new Claims { ClaimId = 22, Claim = Permissions.IssueLogClaims.Create },
                new Claims { ClaimId = 23, Claim = Permissions.IssueLogClaims.Edit },
                new Claims { ClaimId = 24, Claim = Permissions.IssueLogClaims.Delete },

                new Claims { ClaimId = 25, Claim = Permissions.IssueClaims.View },
                new Claims { ClaimId = 26, Claim = Permissions.IssueClaims.Create },
                new Claims { ClaimId = 27, Claim = Permissions.IssueClaims.Edit },
                new Claims { ClaimId = 28, Claim = Permissions.IssueClaims.Delete },

                new Claims { ClaimId = 29, Claim = Permissions.CauseClaims.View },
                new Claims { ClaimId = 30, Claim = Permissions.CauseClaims.Create },
                new Claims { ClaimId = 31, Claim = Permissions.CauseClaims.Edit },
                new Claims { ClaimId = 32, Claim = Permissions.CauseClaims.Delete },

                new Claims { ClaimId = 33, Claim = Permissions.DeviceClaims.View },
                new Claims { ClaimId = 34, Claim = Permissions.DeviceClaims.Create },
                new Claims { ClaimId = 35, Claim = Permissions.DeviceClaims.Edit },
                new Claims { ClaimId = 36, Claim = Permissions.DeviceClaims.Delete },

                new Claims { ClaimId = 37, Claim = Permissions.DeviceTypeClaims.View },
                new Claims { ClaimId = 38, Claim = Permissions.DeviceTypeClaims.Create },
                new Claims { ClaimId = 39, Claim = Permissions.DeviceTypeClaims.Edit },
                new Claims { ClaimId = 40, Claim = Permissions.DeviceTypeClaims.Delete }
                );
            modelBuilder.Entity<RoleClaims>().HasData(
                new RoleClaims { RoleId = 1, ClaimId = 1 },
                new RoleClaims { RoleId = 2, ClaimId = 2 }
                );
        }
    }
}
