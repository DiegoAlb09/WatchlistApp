using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WatchlistApi.Models;

namespace WatchlistApi.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  // ===== Tablas ANTIGUAS (se mantienen en el paso 1; se eliminan al final del rediseño) =====
  public DbSet<WatchlistItemEntity> WatchlistItems => Set<WatchlistItemEntity>();
  public DbSet<LibroItemEntity> LibroItems => Set<LibroItemEntity>();
  public DbSet<HistorialVistoEntity> HistorialVistos => Set<HistorialVistoEntity>();

  // ===== Tablas NUEVAS: catalogos compartidos + relacion con usuarios =====
  public DbSet<CatalogoSerieEntity> CatalogoSeries => Set<CatalogoSerieEntity>();
  public DbSet<UsuarioSerieEntity> UsuarioSeries => Set<UsuarioSerieEntity>();
  public DbSet<CatalogoLibroEntity> CatalogoLibros => Set<CatalogoLibroEntity>();
  public DbSet<UsuarioLibroEntity> UsuarioLibros => Set<UsuarioLibroEntity>();

  // SQLite no soporta listas nativamente: se guarda como "1,2,3" y se convierte de vuelta a List<int>.
  private static ValueComparer<List<int>> NuevoComparerEpisodios() => new(
      (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
      a => a.Aggregate(0, (hash, v) => HashCode.Combine(hash, v)),
      a => a.ToList());

  private static string ListaATexto(List<int> v) => string.Join(',', v);

  private static List<int> TextoALista(string v) =>
      string.IsNullOrEmpty(v)
          ? new List<int>()
          : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // OBLIGATORIO: configura las tablas de Identity.
    base.OnModelCreating(modelBuilder);

    // ----- Tabla antigua -----
    modelBuilder.Entity<WatchlistItemEntity>()
        .Property(e => e.EpisodiosVistos)
        .HasConversion(v => ListaATexto(v), v => TextoALista(v))
        .Metadata.SetValueComparer(NuevoComparerEpisodios());

    // ----- Catalogo de series/peliculas -----
    modelBuilder.Entity<CatalogoSerieEntity>(e =>
    {
      e.HasIndex(x => x.Titulo); // para el buscador del paso 3
    });

    modelBuilder.Entity<UsuarioSerieEntity>(e =>
    {
      // Un usuario no puede tener el mismo titulo dos veces.
      e.HasIndex(x => new { x.UserId, x.CatalogoSerieId }).IsUnique();

      // Proteccion: no se puede borrar un titulo del catalogo si alguien lo tiene.
      e.HasOne(x => x.Catalogo)
              .WithMany(c => c.Usuarios)
              .HasForeignKey(x => x.CatalogoSerieId)
              .OnDelete(DeleteBehavior.Restrict);

      // Si se borra la cuenta, se borra su lista (pero no el catalogo).
      e.HasOne<ApplicationUser>()
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

      e.Property(x => x.EpisodiosVistos)
              .HasConversion(v => ListaATexto(v), v => TextoALista(v))
              .Metadata.SetValueComparer(NuevoComparerEpisodios());
    });

    // ----- Catalogo de libros/manga/comics -----
    modelBuilder.Entity<CatalogoLibroEntity>(e =>
    {
      e.HasIndex(x => x.Titulo);
    });

    modelBuilder.Entity<UsuarioLibroEntity>(e =>
    {
      e.HasIndex(x => new { x.UserId, x.CatalogoLibroId }).IsUnique();

      e.HasOne(x => x.Catalogo)
              .WithMany(c => c.Usuarios)
              .HasForeignKey(x => x.CatalogoLibroId)
              .OnDelete(DeleteBehavior.Restrict);

      e.HasOne<ApplicationUser>()
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    });
  }
}