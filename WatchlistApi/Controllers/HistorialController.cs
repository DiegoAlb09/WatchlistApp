using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/historial")]
public class HistorialController : ControllerBase
{
  private readonly AppDbContext _db;

  public HistorialController(AppDbContext db)
  {
    _db = db;
  }

  [HttpGet]
  public async Task<ActionResult<List<HistorialVistoEntity>>> GetAll()
  {
    var items = await _db.HistorialVistos.OrderBy(h => h.Fecha).ToListAsync();
    return Ok(items);
  }

  [HttpPost]
  public async Task<ActionResult> Create(HistorialVistoEntity item)
  {
    if (item.Id == Guid.Empty)
      item.Id = Guid.NewGuid();

    if (item.Fecha == default)
      item.Fecha = DateTime.UtcNow;

    _db.HistorialVistos.Add(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }
}