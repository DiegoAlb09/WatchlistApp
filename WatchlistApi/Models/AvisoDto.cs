namespace WatchlistApi.Models;

// Lo que ve el cliente de un aviso. No incluye quien edito.
public class AvisoDto
{
  public Guid Id { get; set; }
  public bool EsLibro { get; set; }
  public string TituloAnterior { get; set; } = string.Empty;
  public string TituloNuevo { get; set; } = string.Empty;
  public string Resumen { get; set; } = string.Empty;
  public DateTime FechaCreacion { get; set; }
}