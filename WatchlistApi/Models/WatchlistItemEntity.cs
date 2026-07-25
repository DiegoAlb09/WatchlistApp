namespace WatchlistApi.Models;

public enum TipoContenido
{
  Serie,
  Pelicula
}

public class WatchlistItemEntity
{
  public Guid Id { get; set; }
  public string? UserId { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public TipoContenido Tipo { get; set; } = TipoContenido.Pelicula;
  public int? EpisodioInicio { get; set; }
  public int? EpisodioFin { get; set; }

  public bool Visto { get; set; }

  public List<int> EpisodiosVistos { get; set; } = new();

  // Solo aplica si Tipo == Serie: para el calendario de estrenos
  public bool EnEmision { get; set; }
  public DayOfWeek? DiaEmision { get; set; }
  public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;
}