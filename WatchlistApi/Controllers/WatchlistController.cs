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
  
  private static bool CambiaCatalogo(CatalogoSerieEntity c, WatchlistItemDto d) =>
    c.Titulo != d.Titulo || c.PortadaUrl != d.PortadaUrl ||
    c.Tipo != d.Tipo || c.EpisodioInicio != d.EpisodioInicio ||
    c.EpisodioFin != d.EpisodioFin || c.EnEmision != d.EnEmision ||
    c.DiaEmision != d.DiaEmision || c.FechaInicioEmision != d.FechaInicioEmision ||
    c.FechaFinEmision != d.FechaFinEmision;

  private static string ResumenCambios(CatalogoSerieEntity c, WatchlistItemDto d)
  {
    var l = new List<string>();
    if (c.Titulo != d.Titulo) l.Add("título");
    if (c.PortadaUrl != d.PortadaUrl) l.Add("portada");
    if (c.Tipo != d.Tipo) l.Add("tipo");
    if (c.EpisodioInicio != d.EpisodioInicio || c.EpisodioFin != d.EpisodioFin) l.Add("rango de capítulos");
    if (c.EnEmision != d.EnEmision || c.DiaEmision != d.DiaEmision 
      || c.FechaInicioEmision != d.FechaInicioEmision || c.FechaFinEmision != d.FechaFinEmision) l.Add("datos de emisión");

    return string.Join(", ", l);
  }

  //Sigue la cadena de versiones hasta la mas reciente que todavia exista.
  private async Task<CatalogoSerieEntity?> UltimaVersionAsync(Guid id)
  {
    var c = await _db.CatalogoSeries.FindAsync(id);
    var saltos = 0;
    while (c?.ReemplazadoPorId is Guid siguienteId && saltos++ < 50)
    {
      var sig = await _db.CatalogoSeries.FindAsync(siguienteId);
      if (sig is null) break;
      c = sig;
    }

    return c;
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

    // El progreso es personal: siempre se guarda directo.
    vinculo.Visto = dto.Visto;
    vinculo.EpisodiosVistos = dto.EpisodiosVistos;

    var actual = vinculo.Catalogo!;
    if (CambiaCatalogo(actual, dto))
    {
      var otros = await _db.UsuarioSeries
        .Where(u => u.CatalogoSerieId == actual.Id && u.UserId != UserId)
        .Select(u => u.UserId)
        .ToListAsync();
      
      if (otros.Count == 0)
      {
        // Solo yo lo tengo: edicion directa, nadie a quien avisar.
        AplicarCatalogo(actual, dto);
      }
      else
      {
        // Otros lo tienen: version NUEVA. Ellos siguen viendo la anterior
        // hasta que acepten el cambio desde su aviso.
        var resumen = ResumenCambios(actual, dto);

        var nueva = new CatalogoSerieEntity { Id = Guid.NewGuid(), CreadoPorUserId = UserId };
        AplicarCatalogo(nueva, dto);
        _db.CatalogoSeries.Add(nueva);

        vinculo.CatalogoSerieId = nueva.Id;
        vinculo.Catalogo = nueva;

        if (actual.ReemplazadoPorId is null)
          actual.ReemplazadoPorId = nueva.Id;
        
        foreach (var destinatario in otros)
        {
          _db.AvisosCambio.Add(new AvisoCambioEntity
          {
            Id = Guid.NewGuid(),
            UserId = destinatario,
            EsLibro = false,
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

  [HttpPost("desde-catalogo/{catalogoId:guid}")]
  public async Task<ActionResult> AgregarDesdeCatalogo(Guid catalogoId)
  {
    var catalogo = await UltimaVersionAsync(catalogoId); // Por si pasan una version vieja
    if (catalogo is null) return NotFound();

    var yaLoTiene = await _db.UsuarioSeries
        .AnyAsync(u => u.CatalogoSerieId == catalogoId && u.UserId == UserId);
    if (yaLoTiene) return Conflict("Ya tienes este título en tu lista.");

    var vinculo = new UsuarioSerieEntity
    {
      Id = Guid.NewGuid(),
      UserId = UserId,
      CatalogoSerieId = catalogoId,
      Catalogo = catalogo,
      FechaAgregado = DateTime.UtcNow
    };
    _db.UsuarioSeries.Add(vinculo);
    await _db.SaveChangesAsync();
    return Ok(Mapear(vinculo));
  }
}
