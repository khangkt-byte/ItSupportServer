using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ItSupportServer.Data.Models.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedTestAccountsForAllRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "acc_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "password",
                value: "$2a$12$6exAjNpv8RlZNtYhvgt0oOd0r.D.C2NaKASd34aqMgwjfFhI7aSk.");

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "emp_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "phone_number", "position" },
                values: new object[] { "0900000001", "Super_Admin" });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "emp_id", "area_id", "created_at", "deleted_at", "dpt_id", "email", "emp_code", "full_name", "phone_number", "position", "updated_at" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 1, "manager@itsupport.local", "MGR001", "Nguyễn Văn Quản Lý", "0900000002", "IT Manager", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 1, "employee@itsupport.local", "EMP001", "Trần Thị Nhân Viên", "0900000003", "IT Support Staff", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 1, "viewer@itsupport.local", "VIEW001", "Lê Văn Xem", "0900000004", "IT Auditor", null }
                });

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] { "acc_id", "created_at", "current_points", "deleted_at", "expired_otp", "failed_login_attempts", "is_locked", "last_login_at", "lifetime_points", "locked_until", "otp", "password", "updated_at", "username" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, null, null, 0, false, null, null, null, null, "$2a$12$Ojrekj8GLlfpt4vrT0PO9eTpg8nTWpnBcungeJdlEhxTStZfVMQK.", null, "manager" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, null, null, 0, false, null, null, null, null, "$2a$12$1d4.6EhIqH1fMu/M3FE0gecU.dPNWkYA2HKe8wwNcQCtBTeNjAGse", null, "employee" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, null, null, 0, false, null, null, null, null, "$2a$12$GHnHxA3xSTKRoAEfVwn7zOEm2j0fpsnNd3xUGNXBomkJTDy74HeGK", null, "viewer" }
                });

            migrationBuilder.InsertData(
                table: "account_roles",
                columns: new[] { "account_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), 3 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 2 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "account_roles",
                keyColumns: new[] { "account_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), 3 });

            migrationBuilder.DeleteData(
                table: "account_roles",
                keyColumns: new[] { "account_id", "role_id" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), 2 });

            migrationBuilder.DeleteData(
                table: "account_roles",
                keyColumns: new[] { "account_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), 4 });

            migrationBuilder.DeleteData(
                table: "accounts",
                keyColumn: "acc_id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "accounts",
                keyColumn: "acc_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "accounts",
                keyColumn: "acc_id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "employees",
                keyColumn: "emp_id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "employees",
                keyColumn: "emp_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "employees",
                keyColumn: "emp_id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "acc_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "password",
                value: "$2a$12$UuDUXpLPwXBUMRJ8Grpz6.e7dnHvCJQ/SxAXqN.RC4Y2i0GR2Il9W");

            migrationBuilder.UpdateData(
                table: "employees",
                keyColumn: "emp_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "phone_number", "position" },
                values: new object[] { "0900000000", null });
        }
    }
}
