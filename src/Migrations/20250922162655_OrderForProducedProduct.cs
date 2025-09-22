using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class OrderForProducedProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                table: "ProducedProducts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProducedProducts_OrderId",
                table: "ProducedProducts",
                column: "OrderId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProducedProducts_ProductionOrders_OrderId",
                table: "ProducedProducts",
                column: "OrderId",
                principalTable: "ProductionOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProducedProducts_ProductionOrders_OrderId",
                table: "ProducedProducts"
            );

            migrationBuilder.DropIndex(
                name: "IX_ProducedProducts_OrderId",
                table: "ProducedProducts"
            );

            migrationBuilder.DropColumn(name: "OrderId", table: "ProducedProducts");
        }
    }
}
