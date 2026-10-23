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

  public WatchlistController(AppDbContext db) => _db = db;

  private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

  private static WatchlistItemDto Mapear(UsuarioSerieEntity u)
  {
    var c = u.Catalogo!;
    return new WatchlistItemDto
    {
      Id = u.Id,
      Titulo = c.Titulo,
      PortadaUrl = c.PortadaUrl,
      Tipo = c.Tipo,
      EpisodioInicio = c.EpisodioInicio,
      EpisodioFin = c.EpisodioFin,
      EnEmision = c.EnEmision,
      DiaEmision = c.DiaEmision,
      FechaInicioEmision = c.FechaInicioEmision,
      FechaFinEmision = c.FechaFinEmision,
      Visto = u.Visto,
      EpisodiosVistos = u.EpisodiosVistos,
      FechaAgregado = u.FechaAgregado
    };
  }

  // Copia los datos COMPARTIDO del DTO al catalogo.
  private static void AplicarCatalogo(CatalogoSerieEntity c, WatchlistItemDto d)
  {
    c.Titulo = d.Titulo;
    c.PortadaUrl = d.PortadaUrl;
    c.Tipo = d.Tipo;
    c.EpisodioInicio = d.EpisodioInicio;
    c.EpisodioFin = d.EpisodioFin;
    c.EnEmision = d.EnEmision;
    c.DiaEmision = d.DiaEmision;
    c.FechaInicioEmision = d.FechaInicioEmision;
    c.FechaFinEmision = d.FechaFinEmision;
  }

  // Crea catalogo nuevo + vinculo con el usuario actual.
  private UsuarioSerieEntity CrearNuevo(WatchlistItemDto d)
  {
    var catalogo = new CatalogoSerieEntity { Id = Guid.NewGuid(), CreadoPorUserId = UserId };
    AplicarCatalogo(catalogo, d);

    var vinculo = new UsuarioSerieEntity
    {
      Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
      UserId = UserId,
      Catalogo = catalogo,
      Visto = d.Visto,
      EpisodiosVistos = d.EpisodiosVistos,
      FechaAgregado = d.FechaAgregado == default ? DateTime.UtcNow : d.FechaAgregado
    };
    _db.UsuarioSeries.Add(vinculo);
    return vinculo;
  }

  [HttpGet]
  public async Task<ActionResult<List<WatchlistItemDto>>> GetAll()
  {
    var filas = await _db.UsuarioSeries
        .Include(u => u.Catalogo)
        .Where(u => u.UserId == UserId)
        .OrderBy(u => u.FechaAgregado)
        .ToListAsync();
    return Ok(filas.Select(Mapear).ToList());
  }

  [HttpPost]
  public async Task<ActionResult> Create(WatchlistItemDto dto)
  {
    var vinculo = CrearNuevo(dto);
    await _db.SaveChangesAsync();
    return Ok(Mapear(vinculo));
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> Update(Guid id, WatchlistItemDto dto)
  {
    var vinculo = await _db.UsuarioSeries.Include(u => u.Catalogo)
        .FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);
    if (vinculo is null) return NotFound();

    // TEMPORAL (paso 2): como cada titulo solo lo tiene una persona todavia,
    // editar el catalogo directo es seguro. En el paso 4 esto pasa a ser una
    // edicion compartida con aviso a los demas usuarios.
    AplicarCatalogo(vinculo.Catalogo!, dto);
    vinculo.Visto = dto.Visto;
    vinculo.EpisodiosVistos = dto.EpisodiosVistos;

    await _db.SaveChangesAsync();
    return Ok(Mapear(vinculo));
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(Guid id)
  {
    var vinculo = await _db.UsuarioSeries.Include(u => u.Catalogo)
        .FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);
    if (vinculo is null) return NotFound();

    _db.UsuarioSeries.Remove(vinculo);

    // Quitar de MI lista no borra el titulo para los demas. Solo si yo era
    // el ultimo que lo tenia, se limpia tambien el catalogo.
    var lotienenOtros = await _db.UsuarioSeries
        .AnyAsync(u => u.CatalogoSerieId == vinculo.CatalogoSerieId && u.Id != vinculo.Id);
    if (!lotienenOtros)
      _db.CatalogoSeries.Remove(vinculo.Catalogo!);

    await _db.SaveChangesAsync();
    return Ok();
  }

  // Migracion unica desde localStorage. Idempotente por Id.
  [HttpPost("importar")]
  public async Task<ActionResult> Importar(List<WatchlistItemDto> items)
  {
    foreach (var dto in items)
    {
      var yaExiste = await _db.UsuarioSeries.AnyAsync(u => u.Id == dto.Id && u.UserId == UserId);
      if (!yaExiste)
        CrearNuevo(dto);
    }

    await _db.SaveChangesAsync();
    return Ok();
  }
}
