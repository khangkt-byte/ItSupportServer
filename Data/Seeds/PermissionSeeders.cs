using Microsoft.EntityFrameworkCore;
using ItSupportServer.EF_Core.Data;

namespace ItSupportServer.EF_Core.Seeds
{
    public static class PermissionSeeders
    {
        public static void SeedPermissions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = "customer", Name = "Customer", CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc) },
                new Role { Id = "admin", Name = "Admin", CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc) },
                new Role { Id = "employee", Name = "Employee", CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc) }

            );
            modelBuilder.Entity<Claims>().HasData(
                new Claims { Id = 1, Claim = "Customer" },
                new Claims { Id = 2, Claim = "Admin" },
                new Claims { Id = 3, Claim = "Employee" }
                );
            modelBuilder.Entity<RoleClaims>().HasData(
                new RoleClaims { RoleId = "customer", ClaimId = 1 },
                new RoleClaims { RoleId = "admin", ClaimId = 2 },
                new RoleClaims { RoleId = "employee", ClaimId = 3 }
                );
        }
    }
}
