using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeWiseBackend.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddInstrumentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstrumentId",
                table: "StrategyTransitions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstrumentId",
                table: "StrategyTransitions");
        }
    }
}
