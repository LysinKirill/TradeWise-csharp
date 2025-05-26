using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeWiseBackend.Dal.Migrations
{
    /// <inheritdoc />
    public partial class RenameAllocatingBudgetColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllocatedBudget",
                table: "Strategies");

            migrationBuilder.RenameColumn(
                name: "UsedBudget",
                table: "StrategyExecutions",
                newName: "AllocatedBudget");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllocatedBudget",
                table: "StrategyExecutions",
                newName: "UsedBudget");

            migrationBuilder.AddColumn<double>(
                name: "AllocatedBudget",
                table: "Strategies",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
