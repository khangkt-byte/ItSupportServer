using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    public static class UserSeeders
    {
        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employees>().HasData(
                new Employees
                {
                    //EmpId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    EmpId = "NV001",
                    EmpCode = "NV001",
                    FullName = "Nguyễn Văn A",
                    Email = "nguyenvana@gmail.com",
                    PhoneNumber = "0123456789",
                    DptId = 1,
                    AreaId = 1,
                    Birthday = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = true,
                    CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc),
                });
            modelBuilder.Entity<Accounts>().HasData(
                new Accounts
                {
                    AccountId = "TK001",
                    Username = "adminA",
                    Password = "AQAAAAIAAYagAAAAEEms0ysPRm2n5vnXRawAsarpqN71JIBmAsB6o/LwNQElvYkETT9sR3eCUBaE9SpJtA==",//admin1234
                    CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc)
                });

            modelBuilder.Entity<AccountRoles>().HasData(
                new AccountRoles
                {
                    AccountId = "TK001",
                    RoleId = 1 // Supper_Admin
                });
        }
    }
}
