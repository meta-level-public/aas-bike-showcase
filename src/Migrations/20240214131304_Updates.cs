using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class Updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpdateableShells",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KatalogEintragId = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdateFoundTimestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateableShells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpdateableShells_KatalogEintraege_KatalogEintragId",
                        column: x => x.KatalogEintragId,
                        principalTable: "KatalogEintraege",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UpdateableShells_KatalogEintragId",
                table: "UpdateableShells",
                column: "KatalogEintragId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpdateableShells");
        }
    }
}
