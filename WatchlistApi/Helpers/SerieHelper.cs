using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Helpers;

public static class SerieHelper
{
  // Devuelve null si todo salio bien, o un mensaje si no se pudo mover.
  public static async Task<string?> AceptarAsync(AppDbContext db, string userId, AvisoCambioEntity aviso)
  {
    var vinculo = await db.UsuarioSeries
      .FirstOrDefaultAsync(u => u.UserId == userId && u.CatalogoSerieId == aviso.CatalogoAnteriorId);
    if (vinculo is null) return "Ya no tienes este título en tu watchlist.";

    var destino = await UltimaAsync(db, aviso.CatalogoNuevoId);
    if (destino is null) return "La versión nueva ya no está disponible.";

    if (destino.Id == vinculo.CatalogoSerieId) return null; // ya estaba en la ultima
    
    var yaTieneDestino = await db.UsuarioSeries
      .AnyAsync(u => u.UserId == userId && u.CatalogoSerieId == destino.Id);
    if (yaTieneDestino) return "Ya tienes la versión nueva en tu watchlist.";

    vinculo.CatalogoSerieId = destino.Id;

    // Si el rango nuevo es mas corto, no dejar episodios marcados fuera del rango.
    if (destino.EpisodioInicio is not null && destino.EpisodioFin is not null)
      vinculo.EpisodiosVistos = vinculo.EpisodiosVistos
        .Where(e => e >= destino.EpisodioInicio && e <= destino.EpisodioFin)
        .ToList();
    
    return null;
  }

  public static async Task<CatalogoSerieEntity?> UltimaAsync(AppDbContext db, Guid id)
  {
    var c = await db.CatalogoSeries.FindAsync(id);
    var saltos = 0;
    while (c?.ReemplazadoPorId is Guid sigId && saltos++ < 50)
    {
      var sig = await db.CatalogoSeries.FindAsync(sigId);
      if (sig is null) break;
      c = sig;
    }
    return c;
  }
}