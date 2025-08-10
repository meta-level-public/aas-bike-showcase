using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class SetSupplierIdFromUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SupplierId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: true);

            // Setze die SupplierId basierend auf der RemoteRepositoryUrl
            // Aktualisiere KatalogEintraege mit der entsprechenden SupplierId von Suppliers
            // die die gleiche RemoteRepositoryUrl haben
            migrationBuilder.Sql(@"
                UPDATE KatalogEintraege 
                SET SupplierId = (
                    SELECT s.Id 
                    FROM Suppliers s 
                    WHERE s.RemoteRepositoryUrl = KatalogEintraege.RemoteRepositoryUrl
                    LIMIT 1
                )
                WHERE EXISTS (
                    SELECT 1 
                    FROM Suppliers s 
                    WHERE s.RemoteRepositoryUrl = KatalogEintraege.RemoteRepositoryUrl
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_KatalogEintraege_SupplierId",
                table: "KatalogEintraege",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_Suppliers_SupplierId",
                table: "KatalogEintraege",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_Suppliers_SupplierId",
                table: "KatalogEintraege");

            migrationBuilder.DropIndex(
                name: "IX_KatalogEintraege_SupplierId",
                table: "KatalogEintraege");

            // Entferne die SupplierId Spalte - die SQL-Updates werden automatisch rückgängig gemacht
            // da die gesamte Spalte gelöscht wird
            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "KatalogEintraege");
        }
    }
}
