using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptimizationHeuristics.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "algorithm_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AlgorithmType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Parameters = table.Column<string>(type: "jsonb", nullable: false),
                    MaxIterations = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_algorithm_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "problem_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Cities = table.Column<string>(type: "jsonb", nullable: false),
                    CityCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_problem_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "optimization_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlgorithmConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BestDistance = table.Column<double>(type: "double precision", nullable: true),
                    BestRoute = table.Column<string>(type: "jsonb", nullable: true),
                    IterationHistory = table.Column<string>(type: "jsonb", nullable: true),
                    TotalIterations = table.Column<int>(type: "integer", nullable: false),
                    ExecutionTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimization_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_optimization_runs_algorithm_configurations_AlgorithmConfigu~",
                        column: x => x.AlgorithmConfigurationId,
                        principalTable: "algorithm_configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_optimization_runs_problem_definitions_ProblemDefinitionId",
                        column: x => x.ProblemDefinitionId,
                        principalTable: "problem_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_optimization_runs_AlgorithmConfigurationId",
                table: "optimization_runs",
                column: "AlgorithmConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_optimization_runs_ProblemDefinitionId",
                table: "optimization_runs",
                column: "ProblemDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "optimization_runs");

            migrationBuilder.DropTable(
                name: "algorithm_configurations");

            migrationBuilder.DropTable(
                name: "problem_definitions");
        }
    }
}
