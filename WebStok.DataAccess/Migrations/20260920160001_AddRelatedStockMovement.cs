using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebStok.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RelatedMovementId",
                table: "StockMovements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_RelatedMovementId",
                table: "StockMovements",
                column: "RelatedMovementId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockMovements_RelatedMovementId",
                table: "StockMovements",
                column: "RelatedMovementId",
                principalTable: "StockMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockMovements_RelatedMovementId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_RelatedMovementId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "RelatedMovementId",
                table: "StockMovements");
        }
    }
}
