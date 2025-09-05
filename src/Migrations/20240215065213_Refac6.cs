using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Refac6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_ConfiguredProducts_ConfiguredProductId",
                table: "ProductParts"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_ProducedProducts_ProducedProductId",
                table: "ProductParts"
            );

            migrationBuilder.AlterColumn<string>(
                name: "GlobalAssetId",
                table: "ConfiguredProducts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AlterColumn<string>(
                name: "AasId",
                table: "ConfiguredProducts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_ConfiguredProducts_ConfiguredProductId",
                table: "ProductParts",
                column: "ConfiguredProductId",
                principalTable: "ConfiguredProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_ProducedProducts_ProducedProductId",
                table: "ProductParts",
                column: "ProducedProductId",
                principalTable: "ProducedProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_ConfiguredProducts_ConfiguredProductId",
                table: "ProductParts"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_ProducedProducts_ProducedProductId",
                table: "ProductParts"
            );

            migrationBuilder.AlterColumn<string>(
                name: "GlobalAssetId",
                table: "ConfiguredProducts",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "AasId",
                table: "ConfiguredProducts",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_ConfiguredProducts_ConfiguredProductId",
                table: "ProductParts",
                column: "ConfiguredProductId",
                principalTable: "ConfiguredProducts",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_ProducedProducts_ProducedProductId",
                table: "ProductParts",
                column: "ProducedProductId",
                principalTable: "ProducedProducts",
                principalColumn: "Id"
            );
        }
    }
}
