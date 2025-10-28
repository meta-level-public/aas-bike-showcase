using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Bestand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_KatalogEintragId",
                table: "KatalogEintraege"
            );

            migrationBuilder.RenameColumn(
                name: "KatalogEintragId",
                table: "KatalogEintraege",
                newName: "ReferencedTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_KatalogEintraege_KatalogEintragId",
                table: "KatalogEintraege",
                newName: "IX_KatalogEintraege_ReferencedTypeId"
            );

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_ReferencedTypeId",
                table: "KatalogEintraege",
                column: "ReferencedTypeId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_ReferencedTypeId",
                table: "KatalogEintraege"
            );

            migrationBuilder.DropColumn(name: "Amount", table: "KatalogEintraege");

            migrationBuilder.RenameColumn(
                name: "ReferencedTypeId",
                table: "KatalogEintraege",
                newName: "KatalogEintragId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_KatalogEintraege_ReferencedTypeId",
                table: "KatalogEintraege",
                newName: "IX_KatalogEintraege_KatalogEintragId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_KatalogEintraege_KatalogEintragId",
                table: "KatalogEintraege",
                column: "KatalogEintragId",
                principalTable: "KatalogEintraege",
                principalColumn: "Id"
            );
        }
    }
}
