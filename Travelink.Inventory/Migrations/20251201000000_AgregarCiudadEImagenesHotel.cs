using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Travelink.Inventory.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCiudadEImagenesHotel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "Hoteles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Imagenes",
                table: "Hoteles",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Hoteles");

            migrationBuilder.DropColumn(
                name: "Imagenes",
                table: "Hoteles");
        }
    }
}
