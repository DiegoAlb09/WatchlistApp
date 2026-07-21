namespace WatchlistApp_Proyect.Models;

public class HistorialVisto
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid ItemId { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public DateTime Fecha { get; set; } = DateTime.Now;
}