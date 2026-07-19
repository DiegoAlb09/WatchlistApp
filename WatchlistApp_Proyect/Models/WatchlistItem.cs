using System.Text.Json.Serialization;

namespace WatchlistApp_Proyect.Models;

public enum TipoContenido
{
  Serie,
  Pelicula
}

public class WatchlistItem
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public TipoContenido Tipo { get; set; } = TipoContenido.Pelicula;

  // Solo aplica si Tipo == Serie
  public int? EpisodioInicio { get; set; }
  public int? EpisodioFin { get; set; }

  // Usado solo para Pelicula (checkbox simple "Vista")
  public bool Visto { get; set; } = false;

  // Usado solo para Serie: numeros de episodio (dentro del rango) marcados como vistos
  public List<int> EpisodiosVistos { get; set; } = new();

  // Solo aplica si Tipo == Serie: para el calendario de estrenos
  public bool EnEmision { get; set; } = false;
  public DayOfWeek? DiaEmision { get; set; }

  public DateTime FechaAgregado { get; set; } = DateTime.Now;

  // --- Propiedades calculadas: no se guardan en localStorage/backend ---

  [JsonIgnore]
  public int TotalEpisodiosRango =>
      (EpisodioInicio is not null && EpisodioFin is not null && EpisodioFin >= EpisodioInicio)
          ? (EpisodioFin.Value - EpisodioInicio.Value + 1)
          : 0;

  [JsonIgnore]
  public bool EstaCompleta =>
      Tipo == TipoContenido.Pelicula
          ? Visto
          : (TotalEpisodiosRango > 0 && EpisodiosVistos.Count >= TotalEpisodiosRango);

  [JsonIgnore]
  public int TotalUnidades => Tipo == TipoContenido.Pelicula ? 1 : Math.Max(TotalEpisodiosRango, 0);

  [JsonIgnore]
  public int UnidadesVistas => Tipo == TipoContenido.Pelicula ? (Visto ? 1 : 0) : EpisodiosVistos.Count;
}
