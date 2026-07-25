using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize]
public class WatchlistController : ControllerBase
{
  private readonly AppDbContext _db;

  public WatchlistController(AppDbContext db)
  {
    _db = db;
  }

  private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

  [HttpGet]
  public async Task<ActionResult<List<WatchlistItemEntity>>> GetAll()
  {
    var items = await _db.WatchlistItems
        .Where(i => i.UserId == UserId)
        .OrderBy(i => i.FechaAgregado)
        .ToListAsync();
    return Ok(items);
  }

  [HttpPost]
  public async Task<ActionResult> Create(WatchlistItemEntity item)
  {
    if (item.Id == Guid.Empty)
      item.Id = Guid.NewGuid();
    item.UserId = UserId;

    _db.WatchlistItems.Add(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> Update(Guid id, WatchlistItemEntity item)
  {
    var existente = await _db.WatchlistItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == UserId);
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
    var existente = await _db.WatchlistItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == UserId);
    if (existente is null)
      return NotFound();

    _db.WatchlistItems.Remove(existente);
    await _db.SaveChangesAsync();
    return Ok();
  }

  // Usado una sola vez por el cliente para migrar lo que tenia en localStorage.
  [HttpPost("importar")]
  public async Task<ActionResult> Importar(List<WatchlistItemEntity> items)
  {
    foreach (var item in items)
    {
      var yaExiste = await _db.WatchlistItems.AnyAsync(i => i.Id == item.Id);
      if (!yaExiste)
      {
        item.UserId = UserId;
        _db.WatchlistItems.Add(item);
      }
    }

    await _db.SaveChangesAsync();
    return Ok();
  }
}
