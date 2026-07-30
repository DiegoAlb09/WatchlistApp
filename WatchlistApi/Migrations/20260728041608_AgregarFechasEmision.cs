using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchlistApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechasEmision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinEmision",
                table: "WatchlistItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioEmision",
                table: "WatchlistItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFinEmision",
                table: "WatchlistItems");

            migrationBuilder.DropColumn(
                name: "FechaInicioEmision",
                table: "WatchlistItems");
        }
    }
}
