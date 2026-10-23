namespace WatchlistApi.Models;

public class WatchlistItemDto
{
  public Guid Id { get; set; } // Id del VINCULO usuario-titulo
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public TipoContenido Tipo { get; set; } = TipoContenido.Pelicula;
  
  public int? EpisodioInicio { get; set; }
  public int? EpisodioFin { get; set; }

  public bool Visto { get; set; }
  public List<int> EpisodiosVistos { get; set; } = new();
  public bool EnEmision { get; set; }
  
  public DayOfWeek? DiaEmision { get; set; }
  public DateTime? FechaInicioEmision { get; set; }
  public DateTime? FechaFinEmision { get; set; }
  public DateTime FechaAgregado { get; set; }
}