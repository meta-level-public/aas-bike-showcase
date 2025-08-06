using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Refac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AasIds",
                table: "ConfiguredProducts",
                newName: "GlobalAssetId");

            migrationBuilder.AddColumn<long>(
                name: "ConfiguredProductId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AasId",
                table: "ConfiguredProducts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "KatalogEintragTypId",
                table: "ConfiguredProducts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "ConfiguredProducts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_KatalogEintraege_ConfiguredProductId",
                table: "KatalogEintraege",
                column: "ConfiguredProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguredProducts_KatalogEintragTypId",
                table: "ConfiguredProducts",
                column: "KatalogEintragTypId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragTypId",
                table: "ConfiguredProducts",
                column: "KatalogEintragTypId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_ConfiguredProducts_ConfiguredProductId",
                table: "KatalogEintraege",
                column: "ConfiguredProductId",
                principalTable: "ConfiguredProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragTypId",
                table: "ConfiguredProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_ConfiguredProducts_ConfiguredProductId",
                table: "KatalogEintraege");

            migrationBuilder.DropIndex(
                name: "IX_KatalogEintraege_ConfiguredProductId",
                table: "KatalogEintraege");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguredProducts_KatalogEintragTypId",
                table: "ConfiguredProducts");

            migrationBuilder.DropColumn(
                name: "ConfiguredProductId",
                table: "KatalogEintraege");

            migrationBuilder.DropColumn(
                name: "AasId",
                table: "ConfiguredProducts");

            migrationBuilder.DropColumn(
                name: "KatalogEintragTypId",
                table: "ConfiguredProducts");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ConfiguredProducts");

            migrationBuilder.RenameColumn(
                name: "GlobalAssetId",
                table: "ConfiguredProducts",
                newName: "AasIds");
        }
    }
}
