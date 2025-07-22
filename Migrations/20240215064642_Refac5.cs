using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Refac5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_KatalogEintraege_BestandteilId",
                table: "ProductParts");

            migrationBuilder.RenameColumn(
                name: "BestandteilId",
                table: "ProductParts",
                newName: "KatalogEintragId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductParts_BestandteilId",
                table: "ProductParts",
                newName: "IX_ProductParts_KatalogEintragId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_KatalogEintraege_KatalogEintragId",
                table: "ProductParts",
                column: "KatalogEintragId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_KatalogEintraege_KatalogEintragId",
                table: "ProductParts");

            migrationBuilder.RenameColumn(
                name: "KatalogEintragId",
                table: "ProductParts",
                newName: "BestandteilId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductParts_KatalogEintragId",
                table: "ProductParts",
                newName: "IX_ProductParts_BestandteilId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_KatalogEintraege_BestandteilId",
                table: "ProductParts",
                column: "BestandteilId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id");
        }
    }
}
