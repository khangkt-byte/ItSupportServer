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
                new Claims { ClaimId = 1, Claim = Permissions.Areas.View },
                new Claims { ClaimId = 2, Claim = Permissions.Areas.Create },
                new Claims { ClaimId = 3, Claim = Permissions.Areas.Edit },
                new Claims { ClaimId = 4, Claim = Permissions.Areas.Delete },

                new Claims { ClaimId = 5, Claim = Permissions.Departments.View },
                new Claims { ClaimId = 6, Claim = Permissions.Departments.Create },
                new Claims { ClaimId = 7, Claim = Permissions.Departments.Edit },
                new Claims { ClaimId = 8, Claim = Permissions.Departments.Delete },

                new Claims { ClaimId = 9, Claim = Permissions.Employees.View },
                new Claims { ClaimId = 10, Claim = Permissions.Employees.Create },
                new Claims { ClaimId = 11, Claim = Permissions.Employees.Edit },
                new Claims { ClaimId = 12, Claim = Permissions.Employees.Delete },

                new Claims { ClaimId = 13, Claim = Permissions.Accounts.View },
                new Claims { ClaimId = 14, Claim = Permissions.Accounts.Create },
                new Claims { ClaimId = 15, Claim = Permissions.Accounts.Edit },
                new Claims { ClaimId = 16, Claim = Permissions.Accounts.Delete },

                new Claims { ClaimId = 17, Claim = Permissions.Roles.View },
                new Claims { ClaimId = 18, Claim = Permissions.Roles.Create },
                new Claims { ClaimId = 19, Claim = Permissions.Roles.Edit },
                new Claims { ClaimId = 20, Claim = Permissions.Roles.Delete },

                new Claims { ClaimId = 21, Claim = Permissions.IssueLogs.View },
                new Claims { ClaimId = 22, Claim = Permissions.IssueLogs.Create },
                new Claims { ClaimId = 23, Claim = Permissions.IssueLogs.Edit },
                new Claims { ClaimId = 24, Claim = Permissions.IssueLogs.Delete },

                new Claims { ClaimId = 25, Claim = Permissions.Issues.View },
                new Claims { ClaimId = 26, Claim = Permissions.Issues.Create },
                new Claims { ClaimId = 27, Claim = Permissions.Issues.Edit },
                new Claims { ClaimId = 28, Claim = Permissions.Issues.Delete },

                new Claims { ClaimId = 29, Claim = Permissions.Causes.View },
                new Claims { ClaimId = 30, Claim = Permissions.Causes.Create },
                new Claims { ClaimId = 31, Claim = Permissions.Causes.Edit },
                new Claims { ClaimId = 32, Claim = Permissions.Causes.Delete },

                new Claims { ClaimId = 33, Claim = Permissions.Devices.View },
                new Claims { ClaimId = 34, Claim = Permissions.Devices.Create },
                new Claims { ClaimId = 35, Claim = Permissions.Devices.Edit },
                new Claims { ClaimId = 36, Claim = Permissions.Devices.Delete },

                new Claims { ClaimId = 37, Claim = Permissions.DeviceTypes.View },
                new Claims { ClaimId = 38, Claim = Permissions.DeviceTypes.Create },
                new Claims { ClaimId = 39, Claim = Permissions.DeviceTypes.Edit },
                new Claims { ClaimId = 40, Claim = Permissions.DeviceTypes.Delete }
                );
            modelBuilder.Entity<RoleClaims>().HasData(
                new RoleClaims { RoleId = 1, ClaimId = 1 },
                new RoleClaims { RoleId = 2, ClaimId = 2 }
                );
        }
    }
}
