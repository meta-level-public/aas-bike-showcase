using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class SetSupplierMandatory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_Suppliers_SupplierId",
                table: "KatalogEintraege"
            );

            migrationBuilder.AlterColumn<long>(
                name: "SupplierId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_Suppliers_SupplierId",
                table: "KatalogEintraege",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KatalogEintraege_Suppliers_SupplierId",
                table: "KatalogEintraege"
            );

            migrationBuilder.AlterColumn<long>(
                name: "SupplierId",
                table: "KatalogEintraege",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KatalogEintraege_Suppliers_SupplierId",
                table: "KatalogEintraege",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id"
            );
        }
    }
}
