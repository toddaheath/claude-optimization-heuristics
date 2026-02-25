using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptimizationHeuristics.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "problem_definitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "optimization_runs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "algorithm_configurations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            // Backfill: assign existing rows to a system user so they aren't orphaned.
            // The system user's data will not be visible to real authenticated users
            // since services filter by UserId == currentUserId.
            migrationBuilder.Sql(@"
                INSERT INTO users (""Id"", ""Email"", ""PasswordHash"", ""DisplayName"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES ('00000000-0000-0000-0000-000000000001', 'system@internal', 'n/a', 'System', FALSE, NOW(), NOW())
                ON CONFLICT DO NOTHING;

                UPDATE problem_definitions SET ""UserId"" = '00000000-0000-0000-0000-000000000001' WHERE ""UserId"" IS NULL;
                UPDATE algorithm_configurations SET ""UserId"" = '00000000-0000-0000-0000-000000000001' WHERE ""UserId"" IS NULL;
                UPDATE optimization_runs SET ""UserId"" = '00000000-0000-0000-0000-000000000001' WHERE ""UserId"" IS NULL;
            ");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_problem_definitions_UserId",
                table: "problem_definitions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_optimization_runs_UserId",
                table: "optimization_runs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_algorithm_configurations_UserId",
                table: "algorithm_configurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ExternalProvider_ExternalId",
                table: "users",
                columns: new[] { "ExternalProvider", "ExternalId" },
                filter: "\"ExternalProvider\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_algorithm_configurations_users_UserId",
                table: "algorithm_configurations",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_optimization_runs_users_UserId",
                table: "optimization_runs",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_problem_definitions_users_UserId",
                table: "problem_definitions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_algorithm_configurations_users_UserId",
                table: "algorithm_configurations");

            migrationBuilder.DropForeignKey(
                name: "FK_optimization_runs_users_UserId",
                table: "optimization_runs");

            migrationBuilder.DropForeignKey(
                name: "FK_problem_definitions_users_UserId",
                table: "problem_definitions");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_problem_definitions_UserId",
                table: "problem_definitions");

            migrationBuilder.DropIndex(
                name: "IX_optimization_runs_UserId",
                table: "optimization_runs");

            migrationBuilder.DropIndex(
                name: "IX_algorithm_configurations_UserId",
                table: "algorithm_configurations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "problem_definitions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "optimization_runs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "algorithm_configurations");
        }
    }
}
