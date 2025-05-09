using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeWiseBackend.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategiesAndTransitionsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StrategyStages",
                columns: table => new
                {
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyStages", x => new { x.StageId, x.StrategyId });
                    table.ForeignKey(
                        name: "FK_StrategyStages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyTransitions",
                columns: table => new
                {
                    StrategyTransitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StageDestinationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatType = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyTransitions", x => x.StrategyTransitionId);
                    table.ForeignKey(
                        name: "FK_StrategyTransitions_StrategyStages_StageDestinationId_Strat~",
                        columns: x => new { x.StageDestinationId, x.StrategyId },
                        principalTable: "StrategyStages",
                        principalColumns: new[] { "StageId", "StrategyId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StrategyTransitions_StrategyStages_StageSourceId_StrategyId",
                        columns: x => new { x.StageSourceId, x.StrategyId },
                        principalTable: "StrategyStages",
                        principalColumns: new[] { "StageId", "StrategyId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyStages_UserId",
                table: "StrategyStages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTransitions_StageDestinationId_StrategyId",
                table: "StrategyTransitions",
                columns: new[] { "StageDestinationId", "StrategyId" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTransitions_StageSourceId_StrategyId",
                table: "StrategyTransitions",
                columns: new[] { "StageSourceId", "StrategyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrategyTransitions");

            migrationBuilder.DropTable(
                name: "StrategyStages");
        }
    }
}
