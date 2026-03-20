using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ItSupportServer.Data.Models.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePermissionSeeders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "claims",
                columns: new[] { "claim_id", "category", "claim" },
                values: new object[,]
                {
                    { 42, null, "Dashboard.View" },
                    { 43, null, "Account.ResetPassword" },
                    { 44, null, "Account.Lock" },
                    { 45, null, "Account.SetAccessControl" },
                    { 46, null, "IssueLog.Import" },
                    { 47, null, "IssueLog.Export" }
                });

            migrationBuilder.InsertData(
                table: "role_claims",
                columns: new[] { "claim_id", "role_id" },
                values: new object[,]
                {
                    { 42, 2 },
                    { 42, 3 },
                    { 42, 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "claims",
                keyColumn: "claim_id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "claims",
                keyColumn: "claim_id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "claims",
                keyColumn: "claim_id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "claims",
                keyColumn: "claim_id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "claims",
                keyColumn: "claim_id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumns: new[] { "claim_id", "role_id" },
                keyValues: new object[] { 42, 2 });

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumns: new[] { "claim_id", "role_id" },
                keyValues: new object[] { 42, 3 });

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumns: new[] { "claim_id", "role_id" },
                keyValues: new object[] { 42, 4 });

            migrationBuilder.DeleteData(
                table: "claims",
                keyColumn: "claim_id",
                keyValue: 42);
        }
    }
}
