namespace WatchlistApi.Models;

public enum EstadoAviso { Pendiente, Aceptado, Mantenido }

/// <summary>
/// Aviso para un usuario: "alguien cambio los datos de un titulo que tienes".
/// Puede aceptar (su vinculo pasa a la version nueva) o mantener la suya.
/// No hay FK a los catalogos porque apunta a series o libros (EsLibros), y
/// ademas el catalogo puede limpiarse; el aviso guarda los titulos para poder
/// mostrarse aunque la version vieja ya no exista.
/// </summary>

public class AvisoCambioEntity
{
  public Guid Id { get; set; }

  // Destinatario
  public string UserId { get; set; } = string.Empty;

  public bool EsLibro { get; set; }
  public Guid CatalogoAnteriorId { get; set; }
  public Guid CatalogoNuevoId { get; set; }

  public string TituloAnterior { get; set; } = string.Empty;
  public string TituloNuevo { get; set; } = string.Empty;

  // Texto corto: titulo, portada, rango de capitulos
  public string Resumen { get; set; } = string.Empty;

  // Solo auditoria. Nunca se devuelve al cliente: los avisos no relevan quien edito
  public string? EditadoPorUserId { get; set; }

  public EstadoAviso Estado { get; set; } = EstadoAviso.Pendiente;
  public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
  public DateTime? FechaResolucion { get; set; }
}