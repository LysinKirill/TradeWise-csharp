using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeWiseBackend.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategyAndTransitionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StrategyStages",
                columns: table => new
                {
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    StageSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageDestinationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyStages", x => x.StageId);
                });

            migrationBuilder.CreateTable(
                name: "StrategyTransitions",
                columns: table => new
                {
                    StrategyTransitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageDestinationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp", nullable: false),
                    MiddleBand = table.Column<double>(type: "double precision", nullable: false),
                    UpperBand = table.Column<double>(type: "double precision", nullable: false),
                    LowerBand = table.Column<double>(type: "double precision", nullable: false),
                    Signal = table.Column<double>(type: "double precision", nullable: false),
                    Macd = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyTransitions", x => x.StrategyTransitionId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrategyStages");

            migrationBuilder.DropTable(
                name: "StrategyTransitions");
        }
    }
}
