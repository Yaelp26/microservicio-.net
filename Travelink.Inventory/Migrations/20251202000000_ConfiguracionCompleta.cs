using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Travelink.Inventory.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguracionCompleta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hoteles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Ciudad = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    EstadoHotel = table.Column<string>(type: "text", nullable: false),
                    Imagenes = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hoteles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HotelId = table.Column<int>(type: "integer", nullable: false),
                    NumeroHabitacion = table.Column<int>(type: "integer", nullable: false),
                    TipoHabitacion = table.Column<string>(type: "text", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric", nullable: false),
                    EstadoHabitacion = table.Column<string>(type: "text", nullable: false),
                    Imagenes = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Habitaciones_Hoteles_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hoteles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HotelId = table.Column<int>(type: "integer", nullable: false),
                    HabitacionesIds = table.Column<string>(type: "jsonb", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstadoReserva = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClienteId = table.Column<string>(type: "text", nullable: false),
                    ClienteNombre = table.Column<string>(type: "text", nullable: false),
                    ClienteEmail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Hoteles_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hoteles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_HotelId",
                table: "Habitaciones",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HotelId",
                table: "Reservas",
                column: "HotelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Habitaciones");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Hoteles");
        }
    }
}
