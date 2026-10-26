using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchlistApi.Migrations
{
    /// <inheritdoc />
    public partial class SyncAppDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CatalogoId",
                table: "AvisosCambio",
                newName: "CatalogoAnteriorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CatalogoAnteriorId",
                table: "AvisosCambio",
                newName: "CatalogoId");
        }
    }
}
