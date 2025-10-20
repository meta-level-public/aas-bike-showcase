using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Refac1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragTypId",
                table: "ConfiguredProducts"
            );

            migrationBuilder.DropIndex(
                name: "IX_ConfiguredProducts_KatalogEintragTypId",
                table: "ConfiguredProducts"
            );

            migrationBuilder.DropColumn(name: "KatalogEintragTypId", table: "ConfiguredProducts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "KatalogEintragTypId",
                table: "ConfiguredProducts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguredProducts_KatalogEintragTypId",
                table: "ConfiguredProducts",
                column: "KatalogEintragTypId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguredProducts_KatalogEintraege_KatalogEintragTypId",
                table: "ConfiguredProducts",
                column: "KatalogEintragTypId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
