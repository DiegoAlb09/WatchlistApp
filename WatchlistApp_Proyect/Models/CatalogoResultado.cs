namespace WatchlistApp_Proyect.Models;

public class CatalogoResultado
{
  public Guid Id { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public string TipoEtiqueta { get; set; } = string.Empty;
  public string Detalle { get; set; } = string.Empty;
  public bool YaLoTengo { get; set; }
}