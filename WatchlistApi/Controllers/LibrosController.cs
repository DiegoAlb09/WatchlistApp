using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/libros")]
public class LibrosController : ControllerBase
{
  private readonly AppDbContext _db;
  
  public LibrosController(AppDbContext db)
  {
    _db = db;
  }

  [HttpGet]
  public async Task<ActionResult<List<LibroItemEntity>>> GetAll()
  {
    var items = await _db.LibroItems.OrderBy(i => i.FechaAgregado).ToListAsync();
    return Ok(items);
  }

  [HttpPost]
  public async Task<ActionResult> Create(LibroItemEntity item)
  {
    if (item.Id == Guid.Empty)
        item.Id = Guid.NewGuid();

    _db.LibroItems.Add(item);
    await _db.SaveChangesAsync();
    return Ok(item);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> Update(Guid id, LibroItemEntity item)
  {
    var existente = await _db.LibroItems.FindAsync(id);
    if (existente is null)
        return NotFound();
    
    existente.Titulo = item.Titulo;
    existente.PortadaUrl = item.PortadaUrl;
    existente.Tipo = item.Tipo;
    existente.CapituloInicio = item.CapituloInicio;
    existente.CapituloFin = item.CapituloFin;
    existente.CapituloActual = item.CapituloActual;

    await _db.SaveChangesAsync();
    return Ok(existente);
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(Guid id)
  {
    var existente = await _db.LibroItems.FindAsync(id);
    if (existente is null)
        return NotFound();

    _db.LibroItems.Remove(existente);
    await _db.SaveChangesAsync();
    return Ok(existente);
  }

  [HttpPost("importar")]
  public async Task<ActionResult> Importar(List<LibroItemEntity> items)
  {
    foreach (var item in items)
    {
      var yaExiste = await _db.LibroItems.AnyAsync(i => i.Id == item.Id);
      if (!yaExiste)
          _db.LibroItems.Add(item);
    }
    await _db.SaveChangesAsync();
    return Ok();
  }
}