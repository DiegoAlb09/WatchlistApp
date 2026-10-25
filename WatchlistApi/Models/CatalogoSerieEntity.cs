namespace WatchlistApi.Models;

/// <summary>
/// Una serie o pelicula del catalogo COMPARTIDO. Existe una sola vez, sin 
/// importar cuantos usuarios la tengan en su lista. Aqui viven los datos que
/// son hechos sobre el titulo: nombre, portada, tipo, rango de capitulos y
/// datos de emision. El progreso de cada persona NO va aqui (ver UsuarioSerieEntity).
/// </summary>

public class CatalogoSerieEntity
{
  public Guid Id { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public string PortadaUrl { get; set; } = string.Empty;
  public TipoContenido Tipo { get; set; } = TipoContenido.Pelicula;

  public int? EpisodioInicio { get; set; }
  public int? EpisodioFin { get; set; }

  public bool EnEmision { get; set; }
  public DayOfWeek? DiaEmision { get; set; }
  public DateTime? FechaInicioEmision { get; set; }
  public DateTime? FechaFinEmision { get; set; }

  public DateTime FechaCreacion { get; set; }

  // Quien lo creo primero. Solo Informativo (auditoria), no da permisos
  public string? CreadoPorUserId { get; set; }

  // null = es la version mas reciente. Sin FK a proposito
  // se sigue la cadena y se tolera que una version intermedia ya no exista).
  public Guid? ReemplazadoPorId { get; set; }
  
  public List<UsuarioSerieEntity> Usuarios { get; set; } = new();
}