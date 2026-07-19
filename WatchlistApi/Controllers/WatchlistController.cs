using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/watchlist")]
public class WatchlistController : ControllerBase
{
  private readonly AppDbContext _db;

  public WatchlistController(AppDbContext db)
  {
    _db = db;
  }

  [HttpGet]
  public async Task<ActionResult<List<WatchlistItemEntity>>> GetAll()
  {
    var items = await _db.WatchlistItems.OrderBy(i => i.FechaAgregado).ToListAsync();
    return Ok(items);
  }

  [HttpPost]
  public async Task<ActionResult> Create(WatchlistItemEntity item)
  {
    if (item.Id == Guid.Empty)
      item.Id = Guid.NewGuid();

    _db.WatchlistItems.Add(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> Update(Guid id, WatchlistItemEntity item)
  {
    var existente = await _db.WatchlistItems.FindAsync(id);
    if (existente is null)
      return NotFound();

    existente.Titulo = item.Titulo;
    existente.PortadaUrl = item.PortadaUrl;
    existente.Tipo = item.Tipo;
    existente.EpisodioInicio = item.EpisodioInicio;
    existente.EpisodioFin = item.EpisodioFin;
    existente.Visto = item.Visto;
    existente.EpisodiosVistos = item.EpisodiosVistos;
    existente.EnEmision = item.EnEmision;
    existente.DiaEmision = item.DiaEmision;

    await _db.SaveChangesAsync();
    return Ok(existente);
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(Guid id)
  {
    var existente = await _db.WatchlistItems.FindAsync(id);
    if (existente is null)
      return NotFound();

    _db.WatchlistItems.Remove(existente);
    await _db.SaveChangesAsync();
    return Ok();
  }

  // Usado una sola vez por el cliente para migrar lo que tenia en localStorage.
  // Es idempotente: si el Id ya existe en la base de datos, lo ignora (no duplica).
  [HttpPost("importar")]
  public async Task<ActionResult> Importar(List<WatchlistItemEntity> items)
  {
    foreach (var item in items)
    {
      var yaExiste = await _db.WatchlistItems.AnyAsync(i => i.Id == item.Id);
      if (!yaExiste)
        _db.WatchlistItems.Add(item);
    }

    await _db.SaveChangesAsync();
    return Ok();
  }
}