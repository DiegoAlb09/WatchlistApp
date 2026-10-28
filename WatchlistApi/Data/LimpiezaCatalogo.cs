using Microsoft.EntityFrameworkCore;
using WatchlistApi.Models;

namespace WatchlistApi.Data;

/// <summary>
/// Politica del catalogo: la version MAS RECIENTE de un titulo se conserva
/// aunque nadie la tenga (otro usuario puede buscarla despues). Lo unico que
/// se borra son las versiones VIEJAS (reemplazadas) que ya nadie tiene.
/// Al borrar una version hay que "coser" la cadena v1->v2->v3: quien
/// apuntaba a la version borrada pasa a apuntar a la siguiente, y los avisos
/// pendientes que decian "la version nueva es v2" pasan a devir v3.
/// </summary>

public static class LimpiezaCatalogo
{
  public static async Task LimpiarAsync(AppDbContext db)
  {
    await LimpiarSeriesAsync(db);
    await LimpiarLibrosAsync(db);
  }

  private static async Task LimpiarSeriesAsync(AppDbContext db)
  {
    var huerfanas = await db.CatalogoSeries
      .Where(c => c.ReemplazadoPorId != null && !c.Usuarios.Any())
      .ToListAsync();
    
    foreach (var r in huerfanas)
    {
      var siguiente = r.ReemplazadoPorId;

      var apuntan = await db.CatalogoSeries.Where(c => c.ReemplazadoPorId == r.Id).ToListAsync();
      foreach (var p in apuntan) p.ReemplazadoPorId = siguiente;

      var avisos = await db.AvisosCambio
        .Where(a => !a.EsLibro && a.CatalogoNuevoId == r.Id && a.Estado == EstadoAviso.Pendiente)
        .ToListAsync();
      
      foreach (var a in avisos) a.CatalogoNuevoId = siguiente!.Value;

      db.CatalogoSeries.Remove(r);

      // Se guarda en cada vulta: la siguiente consulta debe ver la cadena ya cosida.
      await db.SaveChangesAsync();
    }
  }

  private static async Task LimpiarLibrosAsync(AppDbContext db)
  {
    var huerfanos = await db.CatalogoLibros
      .Where(c => c.ReemplazadoPorId != null && !c.Usuarios.Any())
      .ToListAsync();
    
    foreach (var r in huerfanos)
    {
      var siguiente = r.ReemplazadoPorId;

      var apuntan = await db.CatalogoLibros.Where(c => c.ReemplazadoPorId == r.Id).ToListAsync();
      foreach (var p in apuntan) p.ReemplazadoPorId = siguiente;

      var avisos = await db.AvisosCambio
        .Where(a => a.EsLibro && a.CatalogoNuevoId == r.Id && a.Estado == EstadoAviso.Pendiente)
        .ToListAsync();
      
      foreach (var a in avisos) a.CatalogoNuevoId = siguiente!.Value;

      db.CatalogoLibros.Remove(r);
      await db.SaveChangesAsync();
    }
  }
}