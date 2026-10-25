using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/libros")]
[Authorize]
public class LibrosController : ControllerBase
{
  private readonly AppDbContext _db;

  public LibrosController(AppDbContext db) => _db = db;

  private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

  private static LibroItemDto Mapear(UsuarioLibroEntity u)
  {
    var c = u.Catalogo!;
    return new LibroItemDto
    {
      Id = u.Id,
      Titulo = c.Titulo,
      PortadaUrl = c.PortadaUrl,
      Tipo = c.Tipo,
      CapituloInicio = c.CapituloInicio,
      CapituloFin = c.CapituloFin,
      CapituloActual = u.CapituloActual,
      FechaAgregado = u.FechaAgregado
    };
  }

  private static void AplicarCatalogo(CatalogoLibroEntity c, LibroItemDto d)
  {
    c.Titulo = d.Titulo;
    c.PortadaUrl = d.PortadaUrl;
    c.Tipo = d.Tipo;
    c.CapituloInicio = d.CapituloInicio;
    c.CapituloFin = d.CapituloFin;
  }

  private UsuarioLibroEntity CrearNuevo(LibroItemDto d)
  {
    var catalogo = new CatalogoLibroEntity { Id = Guid.NewGuid(), CreadoPorUserId = UserId };
    AplicarCatalogo(catalogo, d);

    var vinculo = new UsuarioLibroEntity
    {
      Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
      UserId = UserId,
      Catalogo = catalogo,
      CapituloActual = d.CapituloActual,
      FechaAgregado = d.FechaAgregado == default ? DateTime.UtcNow : d.FechaAgregado
    };
    _db.UsuarioLibros.Add(vinculo);
    return vinculo;
  }

  [HttpGet]
  public async Task<ActionResult<List<LibroItemDto>>> GetAll()
  {
    var filas = await _db.UsuarioLibros
        .Include(u => u.Catalogo)
        .Where(u => u.UserId == UserId)
        .OrderBy(u => u.FechaAgregado)
        .ToListAsync();
    return Ok(filas.Select(Mapear).ToList());
  }

  [HttpPost]
  public async Task<ActionResult> Create(LibroItemDto dto)
  {
    var vinculo = CrearNuevo(dto);
    await _db.SaveChangesAsync();
    return Ok(Mapear(vinculo));
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> Update(Guid id, LibroItemDto dto)
  {
    var vinculo = await _db.UsuarioLibros.Include(u => u.Catalogo)
        .FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);
    if (vinculo is null) return NotFound();

    // TEMPORAL (paso 2): ver nota en WatchlistController.Update.
    AplicarCatalogo(vinculo.Catalogo!, dto);
    vinculo.CapituloActual = dto.CapituloActual;

    await _db.SaveChangesAsync();
    return Ok(Mapear(vinculo));
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(Guid id)
  {
    var vinculo = await _db.UsuarioLibros.Include(u => u.Catalogo)
        .FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);
    if (vinculo is null) return NotFound();

    _db.UsuarioLibros.Remove(vinculo);

    var lotienenOtros = await _db.UsuarioLibros
        .AnyAsync(u => u.CatalogoLibroId == vinculo.CatalogoLibroId && u.Id != vinculo.Id);
    if (!lotienenOtros)
      _db.CatalogoLibros.Remove(vinculo.Catalogo!);

    await _db.SaveChangesAsync();
    return Ok();
  }

  [HttpPost("importar")]
  public async Task<ActionResult> Importar(List<LibroItemDto> items)
  {
    foreach (var dto in items)
    {
      var yaExiste = await _db.UsuarioLibros.AnyAsync(u => u.Id == dto.Id && u.UserId == UserId);
      if (!yaExiste)
        CrearNuevo(dto);
    }

    await _db.SaveChangesAsync();
    return Ok();
  }

  [HttpPost("desde-catalogo/{catalogoId:guid}")]
  public async Task<ActionResult> AgregarDesdeCatalogo(Guid catalogoId)
  {
    var catalogo = await _db.CatalogoLibros.FindAsync(catalogoId);
    if (catalogo is null) return NotFound();

    var yaLoTiene = await _db.UsuarioLibros
        .AnyAsync(u => u.CatalogoLibroId == catalogoId && u.UserId == UserId);
    if (yaLoTiene) return Conflict("Ya tienes este título en tu lista.");

    var vinculo = new UsuarioLibroEntity
    {
      Id = Guid.NewGuid(),
      UserId = UserId,
      CatalogoLibroId = catalogoId,
      Catalogo = catalogo,
      CapituloActual = catalogo.CapituloInicio,
      FechaAgregado = DateTime.UtcNow
    };
    _db.UsuarioLibros.Add(vinculo);
    await _db.SaveChangesAsync();
    return Ok(Mapear(vinculo));
  }
}