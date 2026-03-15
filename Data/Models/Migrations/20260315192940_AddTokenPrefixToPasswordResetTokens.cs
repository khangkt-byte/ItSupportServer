using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItSupportServer.Data.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenPrefixToPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenPrefix",
                table: "password_reset_tokens",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenPrefix",
                table: "password_reset_tokens");
        }
    }
}
