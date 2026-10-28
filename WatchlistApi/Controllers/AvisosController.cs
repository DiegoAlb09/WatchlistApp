using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Helpers;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/avisos")]
[Authorize]
public class AvisosController : ControllerBase
{
  private readonly AppDbContext _db;

  public AvisosController(AppDbContext db) => _db = db;

  private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

  // Avisos pendientes de ESTE usuario (el contador de la campana sale de aqui).
  [HttpGet]
  public async Task<ActionResult<List<AvisoDto>>> Pendientes()
  {
    var avisos = await _db.AvisosCambio
        .Where(a => a.UserId == UserId && a.Estado == EstadoAviso.Pendiente)
        .OrderByDescending(a => a.FechaCreacion)
        .Select(a => new AvisoDto
        {
          Id = a.Id,
          EsLibro = a.EsLibro,
          TituloAnterior = a.TituloAnterior,
          TituloNuevo = a.TituloNuevo,
          Resumen = a.Resumen,
          FechaCreacion = a.FechaCreacion
        })
        .ToListAsync();

    return Ok(avisos);
  }

  // Aceptar> mi vinculo pasa a la version mas reciente del titulo. Mi progreso se conserva.
  [HttpPost("{id:guid}/aceptar")]
  public async Task<ActionResult> Aceptar(Guid id)
  {
    var aviso = await _db.AvisosCambio
      .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId && a.Estado == EstadoAviso.Pendiente);

      if (aviso is null) return NotFound();

      var resultado = aviso.EsLibro 
        ? await LibroHelper.AceptarAsync(_db, UserId, aviso)
        : await SerieHelper.AceptarAsync(_db, UserId, aviso);
      
      aviso.Estado = resultado is null ? EstadoAviso.Aceptado : EstadoAviso.Mantenido;
      aviso.FechaResolucion = DateTime.UtcNow;
      await _db.SaveChangesAsync();
      await LimpiezaCatalogo.LimpiarAsync(_db);

      // resultado = mensaje de por que no se pudo mover (el aviso se cierra igual)
      return resultado is null ? Ok() : Conflict(resultado);
  }

  // Mantener la mia: no cambia nada, solo se cierra el aviso.
  [HttpPost("{id:guid}/mantener")]
  public async Task<ActionResult> Mantener(Guid id)
  {
    var aviso = await _db.AvisosCambio
      .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId && a.Estado == EstadoAviso.Pendiente);
    if (aviso is null) return NotFound();

    aviso.Estado = EstadoAviso.Mantenido;
    aviso.FechaResolucion = DateTime.UtcNow;
    await _db.SaveChangesAsync();
    await LimpiezaCatalogo.LimpiarAsync(_db);
    return Ok();
  }
}