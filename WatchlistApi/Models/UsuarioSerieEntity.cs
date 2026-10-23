namespace WatchlistApi.Models;

/// <summary>
/// La relacion "este usuario tiene este titulo en su lista", mas lo que es
/// personal: su progreso. Un titulo del catalogo puede estar en muchos
/// usuarios, y un usuario no puede tenerlo dos veces (indice unico).
/// </summary>

public class UsuarioSerieEntity
{
  // En la migracion de datos viejos este Id conserva el Id de la fila original
  // de WatchlistItems, para que HistorialVistos.ItemId siga apuntando a algo valido.

  public Guid Id { get; set; }

  public string UserId { get; set; } = string.Empty;
  public Guid CatalogoSerieId { get; set; }
  public CatalogoSerieEntity? Catalogo { get; set; }

  // Progreso personal
  public bool Visto { get; set; }
  public List<int> EpisodiosVistos { get; set; } = new();

  public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;
}