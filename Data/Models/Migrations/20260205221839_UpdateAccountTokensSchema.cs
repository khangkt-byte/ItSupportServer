using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItSupportServer.Data.Models.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountTokensSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "device_info",
                table: "account_tokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "account_tokens",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_accessed_at",
                table: "account_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_id",
                table: "account_tokens",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "account_tokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_account_tokens_active_sessions",
                table: "account_tokens",
                columns: new[] { "account_id", "revoked_at", "expiry_time" });

            migrationBuilder.CreateIndex(
                name: "idx_account_tokens_revoked_at",
                table: "account_tokens",
                column: "revoked_at",
                filter: "revoked_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_account_tokens_session_id",
                table: "account_tokens",
                column: "session_id",
                filter: "session_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_account_tokens_active_sessions",
                table: "account_tokens");

            migrationBuilder.DropIndex(
                name: "idx_account_tokens_revoked_at",
                table: "account_tokens");

            migrationBuilder.DropIndex(
                name: "idx_account_tokens_session_id",
                table: "account_tokens");

            migrationBuilder.DropColumn(
                name: "device_info",
                table: "account_tokens");

            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "account_tokens");

            migrationBuilder.DropColumn(
                name: "last_accessed_at",
                table: "account_tokens");

            migrationBuilder.DropColumn(
                name: "session_id",
                table: "account_tokens");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "account_tokens");
        }
    }
}
