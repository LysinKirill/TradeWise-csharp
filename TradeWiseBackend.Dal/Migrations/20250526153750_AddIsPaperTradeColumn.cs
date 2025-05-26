using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeWiseBackend.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPaperTradeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaperTrade",
                table: "StrategyExecutions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaperTrade",
                table: "StrategyExecutions");
        }
    }
}
