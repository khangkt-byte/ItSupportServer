using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    public static class PermissionSeeders
    {
        public static void SeedPermissions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Roles>().HasData(
                new Roles { RoleId = "admin", Name = "Admin", CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc) },
                new Roles { RoleId = "employee", Name = "Employee", CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc) }

            );
            modelBuilder.Entity<Claims>().HasData(
                new Claims { ClaimId = 1, Claim = "Customer" },
                new Claims { ClaimId = 2, Claim = "Admin" },
                new Claims { ClaimId = 3, Claim = "Employee" }
                );
            modelBuilder.Entity<RoleClaims>().HasData(
                new RoleClaims { RoleId = "customer", ClaimId = 1 },
                new RoleClaims { RoleId = "admin", ClaimId = 2 },
                new RoleClaims { RoleId = "employee", ClaimId = 3 }
                );
        }
    }
}
