using Microsoft.EntityFrameworkCore;
using ItSupportServer.EF_Core.Data;

namespace ItSupportServer.EF_Core.Seeds
{
    public static class UserSeeders
    {
        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().HasData(
                new Users
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    EmployeeCode = "NV001",
                    Name = "Nguyễn Văn A",
                    Email = "nguyenvana@gmail.com",
                    PhoneNumber = "0123456789",
                    Gender = "Nam",
                    Birthday = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = true,
                    CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc),
                    Position = "Supper_Admin"
                });
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    IdUser = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    UserName = "adminA",
                    Password = "AQAAAAIAAYagAAAAEEms0ysPRm2n5vnXRawAsarpqN71JIBmAsB6o/LwNQElvYkETT9sR3eCUBaE9SpJtA==",//admin1234
                    CreatedAt = new DateTime(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc)
                });

            modelBuilder.Entity<AccountRole>().HasData(
                new AccountRole
                {
                    AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    RoleId = "admin"
                });
        }
    }
}
