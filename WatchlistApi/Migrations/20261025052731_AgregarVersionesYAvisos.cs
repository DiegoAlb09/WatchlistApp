using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchlistApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVersionesYAvisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReemplazadoPorId",
                table: "CatalogoSeries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReemplazadoPorId",
                table: "CatalogoLibros",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AvisosCambio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    EsLibro = table.Column<bool>(type: "INTEGER", nullable: false),
                    CatalogoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CatalogoNuevoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TituloAnterior = table.Column<string>(type: "TEXT", nullable: false),
                    TituloNuevo = table.Column<string>(type: "TEXT", nullable: false),
                    Resumen = table.Column<string>(type: "TEXT", nullable: false),
                    EditadoPorUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvisosCambio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvisosCambio_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvisosCambio_UserId_Estado",
                table: "AvisosCambio",
                columns: new[] { "UserId", "Estado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvisosCambio");

            migrationBuilder.DropColumn(
                name: "ReemplazadoPorId",
                table: "CatalogoSeries");

            migrationBuilder.DropColumn(
                name: "ReemplazadoPorId",
                table: "CatalogoLibros");
        }
    }
}
