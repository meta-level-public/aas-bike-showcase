using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Bestandteile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "KatalogEintragId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KatalogEintraege_KatalogEintragId",
                table: "KatalogEintraege",
                column: "KatalogEintragId");

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_KatalogEintragId",
                table: "KatalogEintraege",
                column: "KatalogEintragId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_KatalogEintragId",
                table: "KatalogEintraege");

            migrationBuilder.DropIndex(
                name: "IX_KatalogEintraege_KatalogEintragId",
                table: "KatalogEintraege");

            migrationBuilder.DropColumn(
                name: "KatalogEintragId",
                table: "KatalogEintraege");
        }
    }
}
