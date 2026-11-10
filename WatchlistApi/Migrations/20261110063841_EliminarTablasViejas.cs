using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchlistApi.Migrations
{
    /// <inheritdoc />
    public partial class EliminarTablasViejas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibroItems");

            migrationBuilder.DropTable(
                name: "WatchlistItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibroItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CapituloActual = table.Column<int>(type: "INTEGER", nullable: false),
                    CapituloFin = table.Column<int>(type: "INTEGER", nullable: true),
                    CapituloInicio = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaAgregado = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PortadaUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibroItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiaEmision = table.Column<int>(type: "INTEGER", nullable: true),
                    EnEmision = table.Column<bool>(type: "INTEGER", nullable: false),
                    EpisodioFin = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodioInicio = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodiosVistos = table.Column<string>(type: "TEXT", nullable: false),
                    FechaAgregado = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFinEmision = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaInicioEmision = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PortadaUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Visto = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistItems", x => x.Id);
                });
        }
    }
}
