using System;
using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Helpers;

public class LibroHelper
{
  // Devuelve null si todo salio bien, o un mensaje si no se pudo mover.
  public static async Task<string?> AceptarAsync(AppDbContext db, string userId, AvisoCambioEntity aviso)
  {
    var vinculo = await db.UsuarioLibros
      .FirstOrDefaultAsync(u => u.UserId == userId && u.CatalogoLibroId == aviso.CatalogoAnteriorId);
    if (vinculo is null) return "Ya no tienes este título en tu watchlist.";

    var destino = await UltimaAsync(db, aviso.CatalogoNuevoId);
    if (destino is null) return "La versión nueva ya no está disponible.";

    if (destino.Id == vinculo.CatalogoLibroId) return null;

    var yaTieneDestino = await db.UsuarioLibros
      .AnyAsync(u => u.UserId == userId && u.CatalogoLibroId == destino.Id);
    if (yaTieneDestino) return "Ya tienes la versión nueva en tu watchlist.";
  
    vinculo.CatalogoLibroId = destino.Id;

    // Mantener el capitulo actual dentro del rango nuevo
    if (vinculo.CapituloActual < destino.CapituloInicio)
      vinculo.CapituloActual = destino.CapituloInicio;
    if (destino.CapituloFin is not null && vinculo.CapituloActual > destino.CapituloFin)
      vinculo.CapituloActual = destino.CapituloFin.Value;
    
    return null;
  }

  // Sigue la cadena ReemplazadoPorId hasta la version mas reciente.
  public static async Task<CatalogoLibroEntity?> UltimaAsync(AppDbContext db, Guid id)
  {
    var c = await db.CatalogoLibros.FindAsync(id);
    var saltos = 0;
    while (c?.ReemplazadoPorId is Guid sigId && saltos++ < 50)
    {
      var sig = await db.CatalogoLibros.FindAsync(sigId);
      if (sig is null) break;
      c = sig;
    }

    return c;
  }
}