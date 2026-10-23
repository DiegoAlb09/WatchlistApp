using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchlistApi.Migrations
{
  /// <inheritdoc />
  public partial class CrearCatalogos : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "CatalogoLibros",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Titulo = table.Column<string>(type: "TEXT", nullable: false),
            PortadaUrl = table.Column<string>(type: "TEXT", nullable: false),
            Tipo = table.Column<int>(type: "INTEGER", nullable: false),
            CapituloInicio = table.Column<int>(type: "INTEGER", nullable: false),
            CapituloFin = table.Column<int>(type: "INTEGER", nullable: true),
            FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
            CreadoPorUserId = table.Column<string>(type: "TEXT", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_CatalogoLibros", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "CatalogoSeries",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Titulo = table.Column<string>(type: "TEXT", nullable: false),
            PortadaUrl = table.Column<string>(type: "TEXT", nullable: false),
            Tipo = table.Column<int>(type: "INTEGER", nullable: false),
            EpisodioInicio = table.Column<int>(type: "INTEGER", nullable: true),
            EpisodioFin = table.Column<int>(type: "INTEGER", nullable: true),
            EnEmision = table.Column<bool>(type: "INTEGER", nullable: false),
            DiaEmision = table.Column<int>(type: "INTEGER", nullable: true),
            FechaInicioEmision = table.Column<DateTime>(type: "TEXT", nullable: true),
            FechaFinEmision = table.Column<DateTime>(type: "TEXT", nullable: true),
            FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
            CreadoPorUserId = table.Column<string>(type: "TEXT", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_CatalogoSeries", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "UsuarioLibros",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            UserId = table.Column<string>(type: "TEXT", nullable: false),
            CatalogoLibroId = table.Column<Guid>(type: "TEXT", nullable: false),
            CapituloActual = table.Column<int>(type: "INTEGER", nullable: false),
            FechaAgregado = table.Column<DateTime>(type: "TEXT", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_UsuarioLibros", x => x.Id);
            table.ForeignKey(
                      name: "FK_UsuarioLibros_AspNetUsers_UserId",
                      column: x => x.UserId,
                      principalTable: "AspNetUsers",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_UsuarioLibros_CatalogoLibros_CatalogoLibroId",
                      column: x => x.CatalogoLibroId,
                      principalTable: "CatalogoLibros",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Restrict);
          });

      migrationBuilder.CreateTable(
          name: "UsuarioSeries",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            UserId = table.Column<string>(type: "TEXT", nullable: false),
            CatalogoSerieId = table.Column<Guid>(type: "TEXT", nullable: false),
            Visto = table.Column<bool>(type: "INTEGER", nullable: false),
            EpisodiosVistos = table.Column<string>(type: "TEXT", nullable: false),
            FechaAgregado = table.Column<DateTime>(type: "TEXT", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_UsuarioSeries", x => x.Id);
            table.ForeignKey(
                      name: "FK_UsuarioSeries_AspNetUsers_UserId",
                      column: x => x.UserId,
                      principalTable: "AspNetUsers",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_UsuarioSeries_CatalogoSeries_CatalogoSerieId",
                      column: x => x.CatalogoSerieId,
                      principalTable: "CatalogoSeries",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Restrict);
          });

      migrationBuilder.CreateIndex(
          name: "IX_CatalogoLibros_Titulo",
          table: "CatalogoLibros",
          column: "Titulo");

      migrationBuilder.CreateIndex(
          name: "IX_CatalogoSeries_Titulo",
          table: "CatalogoSeries",
          column: "Titulo");

      migrationBuilder.CreateIndex(
          name: "IX_UsuarioLibros_CatalogoLibroId",
          table: "UsuarioLibros",
          column: "CatalogoLibroId");

      migrationBuilder.CreateIndex(
          name: "IX_UsuarioLibros_UserId_CatalogoLibroId",
          table: "UsuarioLibros",
          columns: new[] { "UserId", "CatalogoLibroId" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_UsuarioSeries_CatalogoSerieId",
          table: "UsuarioSeries",
          column: "CatalogoSerieId");

      migrationBuilder.CreateIndex(
          name: "IX_UsuarioSeries_UserId_CatalogoSerieId",
          table: "UsuarioSeries",
          columns: new[] { "UserId", "CatalogoSerieId" },
          unique: true);
      
      // ---- Copia de datos: cada fila vieja pasa a catalogo + vinculo ----
      // El Id se conserva en ambas tablas para que HistorialVistos.ItemId siga valido.
      migrationBuilder.Sql(@"
        INSERT INTO CatalogoSeries (Id, Titulo, PortadaUrl, Tipo, EpisodioInicio, EpisodioFin,
            EnEmision, DiaEmision, FechaInicioEmision, FechaFinEmision, FechaCreacion, CreadoPorUserId)
        SELECT Id, Titulo, PortadaUrl, Tipo, EpisodioInicio, EpisodioFin,
            EnEmision, DiaEmision, FechaInicioEmision, FechaFinEmision, FechaAgregado, UserId
        FROM WatchlistItems;");

      migrationBuilder.Sql(@"
        INSERT INTO UsuarioSeries (Id, UserId, CatalogoSerieId, Visto, EpisodiosVistos, FechaAgregado)
        SELECT Id, UserId, Id, Visto, EpisodiosVistos, FechaAgregado
        FROM WatchlistItems WHERE UserId IS NOT NULL;");

      migrationBuilder.Sql(@"
        INSERT INTO CatalogoLibros (Id, Titulo, PortadaUrl, Tipo, CapituloInicio, CapituloFin,
            FechaCreacion, CreadoPorUserId)
        SELECT Id, Titulo, PortadaUrl, Tipo, CapituloInicio, CapituloFin, FechaAgregado, UserId
        FROM LibroItems;");

      migrationBuilder.Sql(@"
        INSERT INTO UsuarioLibros (Id, UserId, CatalogoLibroId, CapituloActual, FechaAgregado)
        SELECT Id, UserId, Id, CapituloActual, FechaAgregado
        FROM LibroItems WHERE UserId IS NOT NULL;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "UsuarioLibros");

      migrationBuilder.DropTable(
          name: "UsuarioSeries");

      migrationBuilder.DropTable(
          name: "CatalogoLibros");

      migrationBuilder.DropTable(
          name: "CatalogoSeries");
    }
  }
}
