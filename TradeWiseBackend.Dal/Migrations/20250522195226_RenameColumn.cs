using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeWiseBackend.Dal.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StageExecutions_StrategyExecutions_ExecutionId",
                table: "StageExecutions");

            migrationBuilder.RenameColumn(
                name: "ExecutionId",
                table: "StageExecutions",
                newName: "StrategyExecutionId");

            migrationBuilder.RenameIndex(
                name: "IX_StageExecutions_ExecutionId",
                table: "StageExecutions",
                newName: "IX_StageExecutions_StrategyExecutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StageExecutions_StrategyExecutions_StrategyExecutionId",
                table: "StageExecutions",
                column: "StrategyExecutionId",
                principalTable: "StrategyExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StageExecutions_StrategyExecutions_StrategyExecutionId",
                table: "StageExecutions");

            migrationBuilder.RenameColumn(
                name: "StrategyExecutionId",
                table: "StageExecutions",
                newName: "ExecutionId");

            migrationBuilder.RenameIndex(
                name: "IX_StageExecutions_StrategyExecutionId",
                table: "StageExecutions",
                newName: "IX_StageExecutions_ExecutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StageExecutions_StrategyExecutions_ExecutionId",
                table: "StageExecutions",
                column: "ExecutionId",
                principalTable: "StrategyExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
