namespace WatchlistApi.Models;

public class LibroItemDto
{
  public Guid Id { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public TipoLibro Tipo { get; set; } = TipoLibro.Libro;
  
  public int CapituloInicio { get; set; } = 1;
  public int? CapituloFin { get; set; }
  public int CapituloActual { get; set; } = 1;
  public DateTime FechaAgregado { get; set; }
}