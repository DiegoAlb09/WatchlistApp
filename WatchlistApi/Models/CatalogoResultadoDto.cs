namespace WatchlistApi.Models;

// Lo que ve quien busca. A proposito NO incluye CreadoPorUserId ni cuantos o
// quienes lo tienen: el buscador muestra titulos, no personas.

public class CatalogoResultadoDto
{
  public Guid Id { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public string TipoEtiqueta { get; set; } = string.Empty;
  public string Detalle { get; set; } = string.Empty;
  public bool YaLoTengo { get; set; }
}