using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class ProducedProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProducedProductId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProducedProducts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfiguredProductId = table.Column<long>(type: "INTEGER", nullable: false),
                    AasId = table.Column<string>(type: "TEXT", nullable: false),
                    GlobalAssetId = table.Column<string>(type: "TEXT", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducedProducts_ConfiguredProducts_ConfiguredProductId",
                        column: x => x.ConfiguredProductId,
                        principalTable: "ConfiguredProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KatalogEintraege_ProducedProductId",
                table: "KatalogEintraege",
                column: "ProducedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducedProducts_ConfiguredProductId",
                table: "ProducedProducts",
                column: "ConfiguredProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_ProducedProducts_ProducedProductId",
                table: "KatalogEintraege",
                column: "ProducedProductId",
                principalTable: "ProducedProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_ProducedProducts_ProducedProductId",
                table: "KatalogEintraege");

            migrationBuilder.DropTable(
                name: "ProducedProducts");

            migrationBuilder.DropIndex(
                name: "IX_KatalogEintraege_ProducedProductId",
                table: "KatalogEintraege");

            migrationBuilder.DropColumn(
                name: "ProducedProductId",
                table: "KatalogEintraege");
        }
    }
}
