using ItSupportServer.Data.Models;
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
                new Claims { ClaimId = 1, Claim = "Admin" },
                new Claims { ClaimId = 2, Claim = "Employee" }
                );
            modelBuilder.Entity<RoleClaims>().HasData(
                new RoleClaims { RoleId = 1, ClaimId = 1 },
                new RoleClaims { RoleId = 2, ClaimId = 2 }
                );
        }
    }
}
