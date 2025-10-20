using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Refac4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ConfiguredProductKatalogEintrag");

            migrationBuilder.CreateTable(
                name: "ProductToKatalogeintragJointable",
                columns: table => new
                {
                    BestandteileId = table.Column<long>(type: "INTEGER", nullable: false),
                    ConfiguredProductsId = table.Column<long>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ProductToKatalogeintragJointable",
                        x => new { x.BestandteileId, x.ConfiguredProductsId }
                    );
                    table.ForeignKey(
                        name: "FK_ProductToKatalogeintragJointable_ConfiguredProducts_ConfiguredProductsId",
                        column: x => x.ConfiguredProductsId,
                        principalTable: "ConfiguredProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ProductToKatalogeintragJointable_KatalogEintraege_BestandteileId",
                        column: x => x.BestandteileId,
                        principalTable: "KatalogEintraege",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProductToKatalogeintragJointable_ConfiguredProductsId",
                table: "ProductToKatalogeintragJointable",
                column: "ConfiguredProductsId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductToKatalogeintragJointable");

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
    }
}
