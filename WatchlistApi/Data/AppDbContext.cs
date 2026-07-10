using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WatchlistApi.Models;

namespace WatchlistApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WatchlistItemEntity> WatchlistItems => Set<WatchlistItemEntity>();
    public DbSet<LibroItemEntity> LibroItems => Set<LibroItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite no soporta listas nativamente: se guarda como "1,2,3" y se convierte de vuelta a List<int>
        var episodiosComparer = new ValueComparer<List<int>>(
            (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
            a => a.Aggregate(0, (hash, v) => HashCode.Combine(hash, v)),
            a => a.ToList());

        modelBuilder.Entity<WatchlistItemEntity>()
            .Property(e => e.EpisodiosVistos)
            .HasConversion(
                v => string.Join(',', v),
                v => string.IsNullOrEmpty(v)
                    ? new List<int>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList())
            .Metadata.SetValueComparer(episodiosComparer);
    }
}
