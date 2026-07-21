using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchlistApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialVistos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialVistos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibroItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    PortadaUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    CapituloInicio = table.Column<int>(type: "INTEGER", nullable: false),
                    CapituloFin = table.Column<int>(type: "INTEGER", nullable: true),
                    CapituloActual = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaAgregado = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    PortadaUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    EpisodioInicio = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodioFin = table.Column<int>(type: "INTEGER", nullable: true),
                    Visto = table.Column<bool>(type: "INTEGER", nullable: false),
                    EpisodiosVistos = table.Column<string>(type: "TEXT", nullable: false),
                    EnEmision = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiaEmision = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaAgregado = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistItems", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialVistos");

            migrationBuilder.DropTable(
                name: "LibroItems");

            migrationBuilder.DropTable(
                name: "WatchlistItems");
        }
    }
}
