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
                name: "token_prefix",
                table: "password_reset_tokens",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_prefix",
                table: "password_reset_tokens",
                column: "token_prefix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_password_reset_tokens_prefix",
                table: "password_reset_tokens");

            migrationBuilder.DropColumn(
                name: "token_prefix",
                table: "password_reset_tokens");
        }
    }
}
