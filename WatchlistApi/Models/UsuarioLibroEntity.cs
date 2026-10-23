namespace WatchlistApi.Models;

public class UsuarioLibroEntity
{
  // Conserva el Id de la fila original de LibroItems al migrar.
  public Guid Id { get; set; }

  public string UserId { get; set; } = string.Empty;
  public Guid CatalogoLibroId { get; set; }
  public CatalogoLibroEntity? Catalogo { get; set; }

  // Progreso personal
  public int CapituloActual { get; set; } = 1;
  public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;
}