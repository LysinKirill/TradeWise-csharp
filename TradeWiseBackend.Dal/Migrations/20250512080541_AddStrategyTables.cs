using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeWiseBackend.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Strategies_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategyStages_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StageSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StageDestinationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatType = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategyTransitions_StrategyStages_StageDestinationId",
                        column: x => x.StageDestinationId,
                        principalTable: "StrategyStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StrategyTransitions_StrategyStages_StageSourceId",
                        column: x => x.StageSourceId,
                        principalTable: "StrategyStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_UserId",
                table: "Strategies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyStages_StrategyId",
                table: "StrategyStages",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTransitions_StageDestinationId",
                table: "StrategyTransitions",
                column: "StageDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTransitions_StageSourceId",
                table: "StrategyTransitions",
                column: "StageSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrategyTransitions");

            migrationBuilder.DropTable(
                name: "StrategyStages");

            migrationBuilder.DropTable(
                name: "Strategies");
        }
    }
}
