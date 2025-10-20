using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Refac3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_ConfiguredProducts_ConfiguredProductId",
                table: "KatalogEintraege"
            );

            migrationBuilder.DropIndex(
                name: "IX_KatalogEintraege_ConfiguredProductId",
                table: "KatalogEintraege"
            );

            migrationBuilder.DropColumn(name: "ConfiguredProductId", table: "KatalogEintraege");

            migrationBuilder.CreateTable(
                name: "ConfiguredProductKatalogEintrag",
                columns: table => new
                {
                    BestandteileId = table.Column<long>(type: "INTEGER", nullable: false),
                    ConfiguredProductsId = table.Column<long>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ConfiguredProductKatalogEintrag",
                        x => new { x.BestandteileId, x.ConfiguredProductsId }
                    );
                    table.ForeignKey(
                        name: "FK_ConfiguredProductKatalogEintrag_ConfiguredProducts_ConfiguredProductsId",
                        column: x => x.ConfiguredProductsId,
                        principalTable: "ConfiguredProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ConfiguredProductKatalogEintrag_KatalogEintraege_BestandteileId",
                        column: x => x.BestandteileId,
                        principalTable: "KatalogEintraege",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguredProductKatalogEintrag_ConfiguredProductsId",
                table: "ConfiguredProductKatalogEintrag",
                column: "ConfiguredProductsId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ConfiguredProductKatalogEintrag");

            migrationBuilder.AddColumn<long>(
                name: "ConfiguredProductId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_KatalogEintraege_ConfiguredProductId",
                table: "KatalogEintraege",
                column: "ConfiguredProductId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_ConfiguredProducts_ConfiguredProductId",
                table: "KatalogEintraege",
                column: "ConfiguredProductId",
                principalTable: "ConfiguredProducts",
                principalColumn: "Id"
            );
        }
    }
}
