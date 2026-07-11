using System.Text.Json.Serialization;
namespace WatchlistApp_Proyect.Models;

public enum TipoLibro { Libro, Manga, Comic }

public class LibroItem
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public string Autor { get; set; } = string.Empty;
  public string Genero { get; set; } = string.Empty;
  public TipoLibro Tipo { get; set; } = TipoLibro.Libro;
  public int CapituloInicio { get; set; } = 1;
  public int? CapituloFin { get; set; }
  public int CapituloActual { get; set; } = 1;
  public DateTime FechaAgregado { get; set; } = DateTime.Now;

  [JsonIgnore]
  public int? TotalCapitulos => CapituloFin is not null ? (CapituloFin - CapituloInicio + 1) : null;

  [JsonIgnore]
  public int CapitulosLeidos
  {
    get
    {
      var leidos = CapituloActual - CapituloInicio + 1;
      if (leidos < 0) leidos = 0;
      if (TotalCapitulos is not null && leidos > TotalCapitulos) leidos = TotalCapitulos.Value;
      return leidos;
    }
  }

  [JsonIgnore]
  public bool EstaCompleta => CapituloFin is not null && CapituloActual >= CapituloFin;

  [JsonIgnore]
  public double? Porcentaje => (TotalCapitulos is null || TotalCapitulos == 0) ? null : Math.Round(CapitulosLeidos * 100.0 / TotalCapitulos.Value, 0);
}