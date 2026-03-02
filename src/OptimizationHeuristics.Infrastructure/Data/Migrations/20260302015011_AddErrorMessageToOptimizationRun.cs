using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptimizationHeuristics.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorMessageToOptimizationRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "optimization_runs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "optimization_runs");
        }
    }
}
