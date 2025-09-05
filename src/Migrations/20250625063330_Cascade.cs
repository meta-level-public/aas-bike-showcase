using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Cascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragId",
                table: "ConfiguredProducts"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_ReferencedTypeId",
                table: "KatalogEintraege"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProducedProducts_ConfiguredProducts_ConfiguredProductId",
                table: "ProducedProducts"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_KatalogEintraege_KatalogEintragId",
                table: "ProductParts"
            );

            migrationBuilder.DropIndex(
                name: "IX_ConfiguredProducts_KatalogEintragId",
                table: "ConfiguredProducts"
            );

            migrationBuilder.DropColumn(name: "KatalogEintragId", table: "ConfiguredProducts");

            migrationBuilder.AlterColumn<long>(
                name: "KatalogEintragId",
                table: "ProductParts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_ReferencedTypeId",
                table: "KatalogEintraege",
                column: "ReferencedTypeId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProducedProducts_ConfiguredProducts_ConfiguredProductId",
                table: "ProducedProducts",
                column: "ConfiguredProductId",
                principalTable: "ConfiguredProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_KatalogEintraege_KatalogEintragId",
                table: "ProductParts",
                column: "KatalogEintragId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_ReferencedTypeId",
                table: "KatalogEintraege"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProducedProducts_ConfiguredProducts_ConfiguredProductId",
                table: "ProducedProducts"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_KatalogEintraege_KatalogEintragId",
                table: "ProductParts"
            );

            migrationBuilder.AlterColumn<long>(
                name: "KatalogEintragId",
                table: "ProductParts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER"
            );

            migrationBuilder.AddColumn<long>(
                name: "KatalogEintragId",
                table: "ConfiguredProducts",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguredProducts_KatalogEintragId",
                table: "ConfiguredProducts",
                column: "KatalogEintragId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragId",
                table: "ConfiguredProducts",
                column: "KatalogEintragId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_ReferencedTypeId",
                table: "KatalogEintraege",
                column: "ReferencedTypeId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProducedProducts_ConfiguredProducts_ConfiguredProductId",
                table: "ProducedProducts",
                column: "ConfiguredProductId",
                principalTable: "ConfiguredProducts",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_KatalogEintraege_KatalogEintragId",
                table: "ProductParts",
                column: "KatalogEintragId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id"
            );
        }
    }
}
