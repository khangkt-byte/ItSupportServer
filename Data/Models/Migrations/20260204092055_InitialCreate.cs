using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ItSupportServer.Data.Models.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "areas",
                columns: table => new
                {
                    area_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_areas", x => x.area_id);
                });

            migrationBuilder.CreateTable(
                name: "claims",
                columns: table => new
                {
                    claim_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    claim = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claims", x => x.claim_id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    dpt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.dpt_id);
                });

            migrationBuilder.CreateTable(
                name: "device_types",
                columns: table => new
                {
                    device_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_types", x => x.device_type_id);
                });

            migrationBuilder.CreateTable(
                name: "issues",
                columns: table => new
                {
                    iss_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    severity = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issues", x => x.iss_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    emp_id = table.Column<Guid>(type: "uuid", nullable: false),
                    emp_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    dpt_id = table.Column<int>(type: "integer", nullable: false),
                    area_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.emp_id);
                    table.ForeignKey(
                        name: "FK_employees_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "area_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employees_departments_dpt_id",
                        column: x => x.dpt_id,
                        principalTable: "departments",
                        principalColumn: "dpt_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    device_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    device_type_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    brand = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    serial_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.device_id);
                    table.ForeignKey(
                        name: "FK_devices_device_types_device_type_id",
                        column: x => x.device_type_id,
                        principalTable: "device_types",
                        principalColumn: "device_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "causes",
                columns: table => new
                {
                    cause_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iss_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_causes", x => x.cause_id);
                    table.ForeignKey(
                        name: "FK_causes_issues_iss_id",
                        column: x => x.iss_id,
                        principalTable: "issues",
                        principalColumn: "iss_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    claim_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claims", x => new { x.role_id, x.claim_id });
                    table.ForeignKey(
                        name: "FK_role_claims_claims_claim_id",
                        column: x => x.claim_id,
                        principalTable: "claims",
                        principalColumn: "claim_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    acc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    current_points = table.Column<int>(type: "integer", nullable: true),
                    lifetime_points = table.Column<int>(type: "integer", nullable: true),
                    otp = table.Column<string>(type: "text", nullable: true),
                    expired_otp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.acc_id);
                    table.ForeignKey(
                        name: "FK_accounts_employees_acc_id",
                        column: x => x.acc_id,
                        principalTable: "employees",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_logs",
                columns: table => new
                {
                    iss_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    @operator = table.Column<string>(name: "operator", type: "character varying(500)", maxLength: 500, nullable: false),
                    requester = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    dpt_id = table.Column<int>(type: "integer", nullable: false),
                    area_id = table.Column<int>(type: "integer", nullable: false),
                    issue_id = table.Column<long>(type: "bigint", nullable: true),
                    issue_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    cause_id = table.Column<long>(type: "bigint", nullable: true),
                    cause = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    permanent_fix = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    date_reported = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_logs", x => x.iss_log_id);
                    table.ForeignKey(
                        name: "FK_issue_logs_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "area_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_issue_logs_causes_cause_id",
                        column: x => x.cause_id,
                        principalTable: "causes",
                        principalColumn: "cause_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_issue_logs_departments_dpt_id",
                        column: x => x.dpt_id,
                        principalTable: "departments",
                        principalColumn: "dpt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_issue_logs_issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "issues",
                        principalColumn: "iss_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "account_claims",
                columns: table => new
                {
                    acc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_claims", x => new { x.acc_id, x.claim_id });
                    table.ForeignKey(
                        name: "FK_account_claims_accounts_acc_id",
                        column: x => x.acc_id,
                        principalTable: "accounts",
                        principalColumn: "acc_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_claims_claims_claim_id",
                        column: x => x.claim_id,
                        principalTable: "claims",
                        principalColumn: "claim_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_roles",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_roles", x => new { x.account_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_account_roles_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "acc_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_tokens",
                columns: table => new
                {
                    account_token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_tokens", x => x.account_token_id);
                    table.ForeignKey(
                        name: "FK_account_tokens_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "acc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "acc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_log_operators",
                columns: table => new
                {
                    iss_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    emp_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hours_spent = table.Column<decimal>(type: "numeric(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_log_operators", x => new { x.iss_log_id, x.emp_id });
                    table.ForeignKey(
                        name: "FK_issue_log_operators_employees_emp_id",
                        column: x => x.emp_id,
                        principalTable: "employees",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_issue_log_operators_issue_logs_iss_log_id",
                        column: x => x.iss_log_id,
                        principalTable: "issue_logs",
                        principalColumn: "iss_log_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_log_requesters",
                columns: table => new
                {
                    iss_log_req_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iss_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    emp_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requester_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_log_requesters", x => x.iss_log_req_id);
                    table.ForeignKey(
                        name: "FK_issue_log_requesters_employees_emp_id",
                        column: x => x.emp_id,
                        principalTable: "employees",
                        principalColumn: "emp_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_issue_log_requesters_issue_logs_iss_log_id",
                        column: x => x.iss_log_id,
                        principalTable: "issue_logs",
                        principalColumn: "iss_log_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "areas",
                columns: new[] { "area_id", "created_at", "deleted_at", "description", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Khu vực tiếp tân, phòng họp chính", "Tầng 1", null },
                    { 2, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Văn phòng làm việc phòng Kinh doanh, Kế toán", "Tầng 2", null },
                    { 3, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Văn phòng phòng CNTT, Server room", "Tầng 3", null },
                    { 4, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Phòng họp, khu vực đào tạo", "Tầng 4", null },
                    { 5, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Khu vực sản xuất, bảo trì thiết bị", "Nhà xưởng", null }
                });

            migrationBuilder.InsertData(
                table: "claims",
                columns: new[] { "claim_id", "category", "claim" },
                values: new object[,]
                {
                    { 1, null, "Admin" },
                    { 2, null, "Area.View" },
                    { 3, null, "Area.Create" },
                    { 4, null, "Area.Edit" },
                    { 5, null, "Area.Delete" },
                    { 6, null, "Department.View" },
                    { 7, null, "Department.Create" },
                    { 8, null, "Department.Edit" },
                    { 9, null, "Department.Delete" },
                    { 10, null, "Employee.View" },
                    { 11, null, "Employee.Create" },
                    { 12, null, "Employee.Edit" },
                    { 13, null, "Employee.Delete" },
                    { 14, null, "Account.View" },
                    { 15, null, "Account.Create" },
                    { 16, null, "Account.Edit" },
                    { 17, null, "Account.Delete" },
                    { 18, null, "Role.View" },
                    { 19, null, "Role.Create" },
                    { 20, null, "Role.Edit" },
                    { 21, null, "Role.Delete" },
                    { 22, null, "IssueLog.View" },
                    { 23, null, "IssueLog.Create" },
                    { 24, null, "IssueLog.Edit" },
                    { 25, null, "IssueLog.Delete" },
                    { 26, null, "Issue.View" },
                    { 27, null, "Issue.Create" },
                    { 28, null, "Issue.Edit" },
                    { 29, null, "Issue.Delete" },
                    { 30, null, "Cause.View" },
                    { 31, null, "Cause.Create" },
                    { 32, null, "Cause.Edit" },
                    { 33, null, "Cause.Delete" },
                    { 34, null, "Device.View" },
                    { 35, null, "Device.Create" },
                    { 36, null, "Device.Edit" },
                    { 37, null, "Device.Delete" },
                    { 38, null, "DeviceType.View" },
                    { 39, null, "DeviceType.Create" },
                    { 40, null, "DeviceType.Edit" },
                    { 41, null, "DeviceType.Delete" }
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "dpt_id", "created_at", "deleted_at", "description", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Phòng Công nghệ thông tin - Quản lý hạ tầng IT, phát triển phần mềm", "Phòng CNTT", null },
                    { 2, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Quản lý nhân sự, hành chính văn phòng", "Phòng Hành chính - Nhân sự", null },
                    { 3, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Quản lý tài chính, kế toán doanh nghiệp", "Phòng Kế toán", null },
                    { 4, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Phát triển kinh doanh, chăm sóc khách hàng", "Phòng Kinh doanh", null },
                    { 5, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Sản xuất, bảo trì thiết bị kỹ thuật", "Phòng Kỹ thuật", null }
                });

            migrationBuilder.InsertData(
                table: "device_types",
                columns: new[] { "device_type_id", "created_at", "deleted_at", "description", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Desktop computer, PC, workstation", "Máy tính để bàn", null },
                    { 2, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Máy tính xách tay", "Laptop", null },
                    { 3, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Printer, máy in văn phòng", "Máy in", null },
                    { 4, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Máy photocopy, scan đa chức năng", "Máy photocopy", null },
                    { 5, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Monitor, màn hình máy tính", "Màn hình", null },
                    { 6, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Network switch, thiết bị chuyển mạch", "Switch mạng", null },
                    { 7, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Bộ định tuyến, router wifi", "Router", null },
                    { 8, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Máy chủ, server vật lý", "Server", null },
                    { 9, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Bộ lưu điện, uninterruptible power supply", "UPS", null },
                    { 10, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "IP Phone, điện thoại nội bộ", "Điện thoại IP", null }
                });

            migrationBuilder.InsertData(
                table: "issues",
                columns: new[] { "iss_id", "category", "created_at", "deleted_at", "description", "name", "severity", "updated_at" },
                values: new object[,]
                {
                    { 1L, "Hardware", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "PC/Laptop không lên nguồn hoặc không vào được Windows", "Máy tính không khởi động được", 4, null },
                    { 2L, "Hardware", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Màn hình bị đen, không có tín hiệu", "Màn hình không hiển thị", 3, null },
                    { 3L, "Hardware", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Bàn phím hoặc chuột không phản hồi", "Bàn phím/Chuột không hoạt động", 2, null },
                    { 4L, "Hardware", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Máy in bị kẹt giấy, không in được", "Máy in bị kẹt giấy", 2, null },
                    { 5L, "Network", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Không vào được internet, mất kết nối LAN", "Mất kết nối mạng", 4, null },
                    { 6L, "Network", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Internet/Mạng nội bộ chậm, lag", "Kết nối mạng chậm", 3, null },
                    { 7L, "Network", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Không tìm thấy hoặc không kết nối được mạng Wifi", "Không kết nối được Wifi", 3, null },
                    { 8L, "Software", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Ứng dụng bị treo, thoát đột ngột", "Phần mềm bị lỗi/crash", 3, null },
                    { 9L, "Software", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Lỗi khi cài đặt ứng dụng/phần mềm", "Không cài đặt được phần mềm", 2, null },
                    { 10L, "Software", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Quên mật khẩu Windows, Email, ứng dụng nội bộ", "Quên mật khẩu", 2, null },
                    { 11L, "Email", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Lỗi khi gửi hoặc nhận email", "Không gửi/nhận email được", 4, null },
                    { 12L, "Email", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Mailbox đạt giới hạn dung lượng", "Email bị đầy dung lượng", 2, null },
                    { 13L, "System", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "PC/Laptop chạy chậm, lag", "Máy tính chạy chậm", 2, null },
                    { 14L, "System", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Màn hình xanh chết, Windows crash", "Blue Screen (BSOD)", 5, null },
                    { 15L, "System", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Dung lượng ổ cứng không đủ", "Ổ cứng đầy", 3, null }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "role_id", "created_at", "deleted_at", "description", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "System Administrator - Full access to all features", "Admin", null },
                    { 2, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Standard Employee - Create and manage issue logs", "Employee", null },
                    { 3, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Department Manager - Extended permissions for team management", "Manager", null },
                    { 4, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Read-Only User - View reports and statistics", "Viewer", null }
                });

            migrationBuilder.InsertData(
                table: "causes",
                columns: new[] { "cause_id", "created_at", "deleted_at", "description", "iss_id", "name", "updated_at" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Bộ nguồn PSU bị hỏng hoặc dây nguồn lỏng", 1L, "Nguồn điện hỏng", null },
                    { 2L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "RAM không nhận hoặc bị lỗi", 1L, "RAM bị lỏng/hỏng", null },
                    { 3L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Bo mạch chủ bị chập cháy hoặc hỏng", 1L, "Mainboard hỏng", null },
                    { 4L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Dây tín hiệu màn hình bị lỏng hoặc hỏng", 2L, "Cáp VGA/HDMI lỏng", null },
                    { 5L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "VGA/GPU bị lỗi hoặc hỏng", 2L, "Card đồ họa hỏng", null },
                    { 6L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Cáp Ethernet bị đứt hoặc tiếp xúc kém", 5L, "Dây mạng bị đứt", null },
                    { 7L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Thiết bị mạng bị hỏng hoặc mất điện", 5L, "Switch/Router hỏng", null },
                    { 8L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "IP tĩnh sai hoặc DHCP không cấp IP", 5L, "Cấu hình IP sai", null },
                    { 9L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "File DLL thiếu hoặc bị corrupt", 8L, "File hệ thống bị lỗi", null },
                    { 10L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Phiên bản cũ có bug, cần update", 8L, "Phần mềm chưa update", null },
                    { 11L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Thông tin server SMTP không đúng", 11L, "Cấu hình SMTP sai", null },
                    { 12L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Account email bị khóa do gửi spam hoặc vi phạm chính sách", 11L, "Tài khoản email bị khóa", null },
                    { 13L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Máy nhiễm virus, phần mềm gián điệp", 13L, "Virus/Malware", null },
                    { 14L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "Dung lượng RAM không đủ cho ứng dụng", 13L, "RAM không đủ", null },
                    { 15L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, "HDD/SSD dung lượng >90%", 13L, "Ổ cứng gần đầy", null }
                });

            migrationBuilder.InsertData(
                table: "devices",
                columns: new[] { "device_id", "brand", "created_at", "deleted_at", "device_type_id", "model", "name", "notes", "serial_number", "updated_at" },
                values: new object[,]
                {
                    { 1L, "Dell", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 1, "OptiPlex 7090", "PC-IT-001", "Máy trưởng phòng CNTT", "DELL-PC-001-2024", null },
                    { 2L, "HP", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 1, "EliteDesk 800 G8", "PC-KD-001", "Máy phòng kinh doanh", "HP-PC-001-2024", null },
                    { 3L, "Lenovo", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 2, "ThinkPad X1 Carbon", "LT-IT-001", "Laptop IT Support", "LN-LT-001-2024", null },
                    { 4L, "Dell", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 2, "Latitude 5420", "LT-KD-001", "Laptop nhân viên kinh doanh", "DELL-LT-001-2024", null },
                    { 5L, "HP", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 3, "LaserJet Pro MFP M428fdw", "PRINTER-T1", "Máy in tầng 1", "HP-PR-001-2024", null },
                    { 6L, "Canon", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 3, "imageRUNNER 2425", "PRINTER-T3", "Máy in phòng CNTT tầng 3", "CN-PR-001-2024", null },
                    { 7L, "Cisco", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 6, "Catalyst 2960X", "SW-CORE-01", "Core switch tầng 3 - Server room", "CISCO-SW-001-2024", null },
                    { 8L, "Cisco", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 7, "ISR 4331", "ROUTER-MAIN", "Router chính kết nối internet", "CISCO-RT-001-2024", null },
                    { 9L, "Dell", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 8, "PowerEdge R750", "SRV-APP-01", "Application server - Production", "DELL-SRV-001-2024", null },
                    { 10L, "HP", new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 8, "ProLiant DL380 Gen10", "SRV-DB-01", "Database server - PostgreSQL", "HP-SRV-001-2024", null }
                });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "emp_id", "area_id", "created_at", "deleted_at", "dpt_id", "email", "emp_code", "full_name", "phone_number", "position", "updated_at" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 1, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, 1, "admin@itsupport.local", "ADMIN001", "System Administrator", "0900000000", null, null });

            migrationBuilder.InsertData(
                table: "issue_logs",
                columns: new[] { "iss_log_id", "area_id", "cause", "cause_id", "created_at", "date_reported", "deleted_at", "dpt_id", "issue_description", "issue_id", "notes", "operator", "permanent_fix", "requester", "resolution", "status", "updated_at" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1, "Giấy bị ẩm do để gần cửa sổ", null, new DateTime(2025, 10, 27, 9, 38, 50, 0, DateTimeKind.Utc), new DateOnly(2025, 10, 27), null, 2, "Máy in tầng 1 bị kẹt giấy liên tục", 4L, "Đã hướng dẫn user cách xử lý kẹt giấy cơ bản", "Nguyễn Văn A", "Di chuyển máy in ra xa cửa sổ, bảo quản giấy trong tủ kín", "Phòng Hành chính, Lê Văn C", "Lấy giấy kẹt ra, thay giấy mới khô ráo", "Resolved", null },
                    { new Guid("22222222-2222-2222-2222-222222222223"), 2, "User tự đổi mật khẩu và quên", null, new DateTime(2025, 10, 29, 9, 38, 50, 0, DateTimeKind.Utc), new DateOnly(2025, 10, 29), null, 4, "Nhân viên quên mật khẩu đăng nhập Windows", 10L, "Đã gửi email hướng dẫn sử dụng LastPass cho toàn công ty", "Nguyễn Văn A", "Hướng dẫn user sử dụng password manager", "Phòng Kinh doanh, Phạm Thị D", "Reset mật khẩu Windows qua Active Directory", "Resolved", null }
                });

            migrationBuilder.InsertData(
                table: "role_claims",
                columns: new[] { "claim_id", "role_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 6, 2 },
                    { 10, 2 },
                    { 22, 2 },
                    { 23, 2 },
                    { 24, 2 },
                    { 26, 2 },
                    { 30, 2 },
                    { 34, 2 },
                    { 2, 3 },
                    { 6, 3 },
                    { 10, 3 },
                    { 22, 3 },
                    { 23, 3 },
                    { 24, 3 },
                    { 25, 3 },
                    { 26, 3 },
                    { 27, 3 },
                    { 28, 3 },
                    { 30, 3 },
                    { 31, 3 },
                    { 32, 3 },
                    { 2, 4 },
                    { 6, 4 },
                    { 10, 4 },
                    { 22, 4 },
                    { 26, 4 },
                    { 30, 4 },
                    { 34, 4 },
                    { 38, 4 }
                });

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] { "acc_id", "created_at", "current_points", "deleted_at", "expired_otp", "failed_login_attempts", "is_locked", "last_login_at", "lifetime_points", "locked_until", "otp", "password", "updated_at", "username" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), null, null, null, 0, false, null, null, null, null, "$2a$12$UuDUXpLPwXBUMRJ8Grpz6.e7dnHvCJQ/SxAXqN.RC4Y2i0GR2Il9W", null, "admin" });

            migrationBuilder.InsertData(
                table: "issue_logs",
                columns: new[] { "iss_log_id", "area_id", "cause", "cause_id", "created_at", "date_reported", "deleted_at", "dpt_id", "issue_description", "issue_id", "notes", "operator", "permanent_fix", "requester", "resolution", "status", "updated_at" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222221"), 2, "Dây mạng bị đứt do bị kéo lê", 6L, new DateTime(2025, 10, 25, 9, 38, 50, 0, DateTimeKind.Utc), new DateOnly(2025, 10, 25), null, 3, "Máy tính tại phòng kế toán không vào được mạng nội bộ", 5L, "Đã cảnh báo user không kéo lê dây mạng", "Nguyễn Văn A", "Dùng ống luồn dây để bảo vệ cáp mạng", "Phòng Kế toán, Trần Thị B", "Thay dây mạng Cat6 mới, test kết nối OK", "Resolved", null },
                    { new Guid("22222222-2222-2222-2222-222222222224"), 3, "RAM 4GB không đủ cho Visual Studio + Docker", 14L, new DateTime(2025, 10, 30, 9, 38, 50, 0, DateTimeKind.Utc), new DateOnly(2025, 10, 30), null, 1, "Laptop chạy rất chậm khi mở nhiều ứng dụng", 13L, "Đã đề xuất mua thêm RAM cho 5 máy dev khác", "Nguyễn Văn A", "Khuyến nghị tối thiểu 16GB RAM cho dev machine", "Phòng CNTT, Hoàng Văn E", "Nâng cấp RAM từ 4GB lên 16GB", "In Progress", null }
                });

            migrationBuilder.InsertData(
                table: "account_roles",
                columns: new[] { "account_id", "role_id" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 1 });

            migrationBuilder.CreateIndex(
                name: "IX_account_claims_claim_id",
                table: "account_claims",
                column: "claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_roles_role_id",
                table: "account_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_tokens_account_id",
                table: "account_tokens",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_tokens_expiry_time",
                table: "account_tokens",
                column: "expiry_time");

            migrationBuilder.CreateIndex(
                name: "ix_AccountTokens_created_at",
                table: "account_tokens",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_Accounts_deleted_at",
                table: "accounts",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_username",
                table: "accounts",
                column: "username",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_Areas_deleted_at",
                table: "areas",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_areas_name",
                table: "areas",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_Causes_deleted_at",
                table: "causes",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_causes_iss_id",
                table: "causes",
                column: "iss_id");

            migrationBuilder.CreateIndex(
                name: "IX_causes_name",
                table: "causes",
                column: "name",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_claims_claim",
                table: "claims",
                column: "claim",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_Departments_deleted_at",
                table: "departments",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_departments_name",
                table: "departments",
                column: "name",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_device_types_name",
                table: "device_types",
                column: "name",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_DeviceTypes_deleted_at",
                table: "device_types",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_Devices_deleted_at",
                table: "devices",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_devices_device_type_id",
                table: "devices",
                column: "device_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_devices_name",
                table: "devices",
                column: "name",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_area_id",
                table: "employees",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "ix_Employees_deleted_at",
                table: "employees",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_employees_dpt_id",
                table: "employees",
                column: "dpt_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_email",
                table: "employees",
                column: "email",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_employees_emp_code",
                table: "employees",
                column: "emp_code",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_employees_full_name",
                table: "employees",
                column: "full_name",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_phone_number",
                table: "employees",
                column: "phone_number",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_issue_log_operators_emp_id",
                table: "issue_log_operators",
                column: "emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_log_requesters_emp_id",
                table: "issue_log_requesters",
                column: "emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_log_requesters_iss_log_id",
                table: "issue_log_requesters",
                column: "iss_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_logs_area_id",
                table: "issue_logs",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_logs_cause_id",
                table: "issue_logs",
                column: "cause_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_logs_dpt_id",
                table: "issue_logs",
                column: "dpt_id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_logs_issue_id",
                table: "issue_logs",
                column: "issue_id");

            migrationBuilder.CreateIndex(
                name: "ix_IssueLogs_deleted_at",
                table: "issue_logs",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_Issues_deleted_at",
                table: "issues",
                column: "deleted_at",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_issues_name",
                table: "issues",
                column: "name",
                filter: "deleted_at IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_account_id",
                table: "password_reset_tokens",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_expiry_status",
                table: "password_reset_tokens",
                columns: new[] { "expires_at", "used_at" });

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_token",
                table: "password_reset_tokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "ix_PasswordResetTokens_created_at",
                table: "password_reset_tokens",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_role_claims_claim_id",
                table: "role_claims",
                column: "claim_id");

            migrationBuilder.CreateIndex(
                name: "ix_Roles_deleted_at",
                table: "roles",
                column: "deleted_at",
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_claims");

            migrationBuilder.DropTable(
                name: "account_roles");

            migrationBuilder.DropTable(
                name: "account_tokens");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "issue_log_operators");

            migrationBuilder.DropTable(
                name: "issue_log_requesters");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "device_types");

            migrationBuilder.DropTable(
                name: "issue_logs");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "claims");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "causes");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "issues");

            migrationBuilder.DropTable(
                name: "areas");

            migrationBuilder.DropTable(
                name: "departments");
        }
    }
}
