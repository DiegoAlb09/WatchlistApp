namespace WatchlistApi.Models;

/// <summary>
/// Un Libro, manga o comic del catalogo COMPARTIDO. Mismo criterio que CatalogoSerieEntity: aqui van los hechos del titulo; el capitulo en el
/// que va cada persona vive en UsuarioLibroEntity
/// </summary>

public class CatalogoLibroEntity
{
  public Guid Id { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public TipoLibro Tipo { get; set; } = TipoLibro.Libro;

  public int CapituloInicio { get; set; } = 1;
  public int? CapituloFin { get; set; }
  
  public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
  public string? CreadoPorUserId { get; set; }

  // null = es la version más reciente
  public Guid? ReemplazadoPorId { get; set; }

  public List<UsuarioLibroEntity> Usuarios { get; set; } = new();
}