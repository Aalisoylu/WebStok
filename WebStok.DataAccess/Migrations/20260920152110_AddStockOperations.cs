using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebStok.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStockOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    RequestHash = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockOperations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockOperations_UserId",
                table: "StockOperations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockOperations_OperationId",
                table: "StockMovements",
                column: "OperationId",
                principalTable: "StockOperations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockOperations_OperationId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "StockOperations");
        }
    }
}
