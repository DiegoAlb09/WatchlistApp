using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/historial")]
[Authorize]
public class HistorialController : ControllerBase
{
  private readonly AppDbContext _db;

  public HistorialController(AppDbContext db)
  {
    _db = db;
  }

  private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

  [HttpGet]
  public async Task<ActionResult<List<HistorialVistoEntity>>> GetAll()
  {
    var items = await _db.HistorialVistos
        .Where(h => h.UserId == UserId)
        .OrderBy(h => h.Fecha)
        .ToListAsync();
    return Ok(items);
  }

  [HttpPost]
  public async Task<ActionResult> Create(HistorialVistoEntity item)
  {
    if (item.Id == Guid.Empty)
      item.Id = Guid.NewGuid();

    if (item.Fecha == default)
      item.Fecha = DateTime.UtcNow;

    item.UserId = UserId;

    _db.HistorialVistos.Add(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }

  // Nota: a proposito sigue sin PUT ni DELETE - el historial no se edita ni se borra.
}