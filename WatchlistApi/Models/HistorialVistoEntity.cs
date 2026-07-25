namespace WatchlistApi.Models;

public class HistorialVistoEntity
{
  public Guid Id { get; set; }
  public string? UserId { get; set; }
  public Guid ItemId { get; set; }
  public string Titulo { get; set; } = string.Empty;
  public DateTime Fecha { get; set; } = DateTime.UtcNow;
}