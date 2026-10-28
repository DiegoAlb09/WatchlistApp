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

  private static bool CambiaCatalogo(CatalogoLibroEntity c, LibroItemDto d) =>
    c.Titulo != d.Titulo || c.PortadaUrl != d.PortadaUrl || c.Tipo != d.Tipo ||
    c.CapituloInicio != d.CapituloInicio || c.CapituloFin != d.CapituloFin;

  private static string ResumenCambios(CatalogoLibroEntity c, LibroItemDto d)
  {
    var l = new List<string>();
    if (c.Titulo != d.Titulo) l.Add("título");
    if (c.PortadaUrl != d.PortadaUrl) l.Add("portada");
    if (c.Tipo != d.Tipo) l.Add("tipo");
    if (c.CapituloInicio != d.CapituloInicio || c.CapituloFin != d.CapituloFin) l.Add("rango de capítulos");
    return string.Join(", ", l);
  }

  private async Task<CatalogoLibroEntity?> UltimaVersionAsync(Guid id)
  {
    var c = await _db.CatalogoLibros.FindAsync(id);
    var saltos = 0;
    while (c?.ReemplazadoPorId is Guid siguienteId && saltos++ < 50)
    {
      var sig = await _db.CatalogoLibros.FindAsync(siguienteId);
      if (sig is null) break;
      c = sig;
    }
    return c;
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

    vinculo.CapituloActual = dto.CapituloActual; // personal

    var actual = vinculo.Catalogo!;
    if (CambiaCatalogo(actual, dto))
    {
      var otros = await _db.UsuarioLibros
          .Where(u => u.CatalogoLibroId == actual.Id && u.UserId != UserId)
          .Select(u => u.UserId)
          .ToListAsync();

      if (otros.Count == 0)
      {
        AplicarCatalogo(actual, dto);
      }
      else
      {
        var resumen = ResumenCambios(actual, dto);

        var nueva = new CatalogoLibroEntity { Id = Guid.NewGuid(), CreadoPorUserId = UserId };
        AplicarCatalogo(nueva, dto);
        _db.CatalogoLibros.Add(nueva);

        vinculo.CatalogoLibroId = nueva.Id;
        vinculo.Catalogo = nueva;

        if (actual.ReemplazadoPorId is null)
          actual.ReemplazadoPorId = nueva.Id;

        foreach (var destinatario in otros)
        {
          _db.AvisosCambio.Add(new AvisoCambioEntity
          {
            Id = Guid.NewGuid(),
            UserId = destinatario,
            EsLibro = true,
            CatalogoAnteriorId = actual.Id,
            CatalogoNuevoId = nueva.Id,
            TituloAnterior = actual.Titulo,
            TituloNuevo = nueva.Titulo,
            Resumen = resumen,
            EditadoPorUserId = UserId
          });
        }
      }
    }

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
    await _db.SaveChangesAsync();

    await LimpiezaCatalogo.LimpiarAsync(_db);
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
    var catalogo = await UltimaVersionAsync(catalogoId);
    if (catalogo is null) return NotFound();

    var yaLoTiene = await _db.UsuarioLibros
        .AnyAsync(u => u.CatalogoLibroId == catalogo.Id && u.UserId == UserId);
    if (yaLoTiene) return Conflict("Ya tienes este título en tu lista.");

    var vinculo = new UsuarioLibroEntity
    {
      Id = Guid.NewGuid(),
      UserId = UserId,
      CatalogoLibroId = catalogo.Id,
      Catalogo = catalogo,
      CapituloActual = catalogo.CapituloInicio,
      FechaAgregado = DateTime.UtcNow
    };
    _db.UsuarioLibros.Add(vinculo);
    await _db.SaveChangesAsync();
    return Ok(Mapear(vinculo));
  }
}