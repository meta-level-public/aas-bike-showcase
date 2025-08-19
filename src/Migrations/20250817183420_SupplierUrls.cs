using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AasDemoapp.Migrations
{
    /// <inheritdoc />
    public partial class SupplierUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemoteRepositoryUrl",
                table: "Suppliers",
                newName: "RemoteAasRepositoryUrl");

            migrationBuilder.AddColumn<string>(
                name: "RemoteAasRegistryUrl",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RemoteSmRepositoryUrl",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RemoteCdRepositoryUrl",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RemoteDiscoveryUrl",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RemoteSmRegistryUrl",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RemoteAasRegistryUrl",
                table: "ImportedShells",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RemoteSmRegistryUrl",
                table: "ImportedShells",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemoteAasRegistryUrl",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RemoteAasRepositoryUrl",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RemoteCdRepositoryUrl",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RemoteDiscoveryUrl",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RemoteSmRegistryUrl",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RemoteAasRegistryUrl",
                table: "ImportedShells");

            migrationBuilder.DropColumn(
                name: "RemoteSmRegistryUrl",
                table: "ImportedShells");

            migrationBuilder.RenameColumn(
                name: "RemoteAasRepositoryUrl",
                table: "Suppliers",
                newName: "RemoteRepositoryUrl");
        }
    }
}
