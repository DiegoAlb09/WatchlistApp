using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

[ApiController]
[Route("api/catalogo")]
[Authorize]
public class CatalogoController : ControllerBase
{
  private const int MaxResultados = 10;
  private readonly AppDbContext _db;

  private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

  // Escapa los comodines de LIKE para que buscar "100%" no se interprete como patron
  private static string Patron(string q) => "%" + q.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";

  [HttpGet("series")]
  public async Task<ActionResult<List<CatalogoResultadoDto>>> BuscarSeries([FromQuery] string? q)
  {
    if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
      return Ok(new List<CatalogoResultadoDto>());

    var patron = Patron(q);
    var filas = await _db.CatalogoSeries
      .Where(c => EF.Functions.Like(c.Titulo, patron, "\\"))
      .OrderBy(c => c.Titulo)
      .Take(MaxResultados)
      .Select(c => new { Catalogo = c, Ya = c.Usuarios.Any(u => u.UserId == UserId) })
      .ToListAsync();

    var resultado = filas.Select(f =>
    {
      var c = f.Catalogo;
      var partes = new List<string>();
      if (c.EpisodioInicio is not null && c.EpisodioFin is not null)
        partes.Add($"Cap. {c.EpisodioInicio}-{c.EpisodioFin}");

      if (c.EnEmision) partes.Add("En emisión");

      return new CatalogoResultadoDto
      {
        Id = c.Id,
        Titulo = c.Titulo,
        PortadaUrl = c.PortadaUrl,
        TipoEtiqueta = c.Tipo == TipoContenido.Serie ? "Serie" : "Pelicula",
        Detalle = string.Join(" · ", partes),
        YaLoTengo = f.Ya
      };
    }).ToList();

    return Ok(resultado);
  }

  [HttpGet("libros")]
  public async Task<ActionResult<List<CatalogoResultadoDto>>> BuscarLibros([FromQuery] string? q)
  {
    if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
      return Ok(new List<CatalogoResultadoDto>());
    
    var patron = Patron(q);
    var filas = await _db.CatalogoLibros
      .Where(c => EF.Functions.Like(c.Titulo, patron, "\\"))
      .OrderBy(c => c.Titulo)
      .Take(MaxResultados)
      .Select(c => new { Catalogo = c, Ya = c.Usuarios.Any(u => u.UserId == UserId) })
      .ToListAsync();
    
    var resultado = filas.Select(f =>
    {
      var c = f.Catalogo;
      return new CatalogoResultadoDto
      {
        Id = c.Id,
        Titulo = c.Titulo,
        PortadaUrl = c.PortadaUrl,
        TipoEtiqueta = c.Tipo switch
        {
          TipoLibro.Manga => "Manga",
          TipoLibro.Comic => "Cómic",
          _ => "Libro"
        },
        Detalle = c.CapituloFin is not null ? $"Cap. {c.CapituloInicio}-{c.CapituloFin}" : "",
        YaLoTengo = f.Ya
      };
    }).ToList();

    return Ok(resultado);
  }
}