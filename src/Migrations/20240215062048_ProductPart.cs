using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class ProductPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_ProducedProducts_ProducedProductId",
                table: "KatalogEintraege");

            migrationBuilder.DropTable(
                name: "ProductToKatalogeintragJointable");

            migrationBuilder.DropIndex(
                name: "IX_KatalogEintraege_ProducedProductId",
                table: "KatalogEintraege");

            migrationBuilder.DropColumn(
                name: "ProducedProductId",
                table: "KatalogEintraege");

            migrationBuilder.AddColumn<long>(
                name: "KatalogEintragId",
                table: "ConfiguredProducts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductParts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BestandteilId = table.Column<long>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    UsageDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfiguredProductId = table.Column<long>(type: "INTEGER", nullable: true),
                    ProducedProductId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductParts_ConfiguredProducts_ConfiguredProductId",
                        column: x => x.ConfiguredProductId,
                        principalTable: "ConfiguredProducts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductParts_KatalogEintraege_BestandteilId",
                        column: x => x.BestandteilId,
                        principalTable: "KatalogEintraege",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductParts_ProducedProducts_ProducedProductId",
                        column: x => x.ProducedProductId,
                        principalTable: "ProducedProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguredProducts_KatalogEintragId",
                table: "ConfiguredProducts",
                column: "KatalogEintragId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_BestandteilId",
                table: "ProductParts",
                column: "BestandteilId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_ConfiguredProductId",
                table: "ProductParts",
                column: "ConfiguredProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_ProducedProductId",
                table: "ProductParts",
                column: "ProducedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragId",
                table: "ConfiguredProducts",
                column: "KatalogEintragId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragId",
                table: "ConfiguredProducts");

            migrationBuilder.DropTable(
                name: "ProductParts");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguredProducts_KatalogEintragId",
                table: "ConfiguredProducts");

            migrationBuilder.DropColumn(
                name: "KatalogEintragId",
                table: "ConfiguredProducts");

            migrationBuilder.AddColumn<long>(
                name: "ProducedProductId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductToKatalogeintragJointable",
                columns: table => new
                {
                    BestandteileId = table.Column<long>(type: "INTEGER", nullable: false),
                    ConfiguredProductsId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductToKatalogeintragJointable", x => new { x.BestandteileId, x.ConfiguredProductsId });
                    table.ForeignKey(
                        name: "FK_ProductToKatalogeintragJointable_ConfiguredProducts_ConfiguredProductsId",
                        column: x => x.ConfiguredProductsId,
                        principalTable: "ConfiguredProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductToKatalogeintragJointable_KatalogEintraege_BestandteileId",
                        column: x => x.BestandteileId,
                        principalTable: "KatalogEintraege",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KatalogEintraege_ProducedProductId",
                table: "KatalogEintraege",
                column: "ProducedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductToKatalogeintragJointable_ConfiguredProductsId",
                table: "ProductToKatalogeintragJointable",
                column: "ConfiguredProductsId");

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_ProducedProducts_ProducedProductId",
                table: "KatalogEintraege",
                column: "ProducedProductId",
                principalTable: "ProducedProducts",
                principalColumn: "Id");
        }
    }
}
