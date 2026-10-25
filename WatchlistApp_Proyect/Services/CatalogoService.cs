using System.Net.Http.Json;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

/// <summary>
/// Buscar en el catalogo compartido y agregar un titulo existente a mi lista.
/// </summary>

public class CatalogoService
{
  private readonly HttpClient _http;

  public CatalogoService(HttpClient http) => _http = http;

  public async Task<List<CatalogoResultado>> BuscarAsync(bool libros, string texto)
  {
    var ruta = libros ? "api/catalogo/libros" : "api/catalogo/series";
    var lista = await _http.GetFromJsonAsync<List<CatalogoResultado>>($"{ruta}?q={Uri.EscapeDataString(texto)}");
    return lista ?? new List<CatalogoResultado>();
  }

  public async Task AgregarAsync(bool libros, Guid catalogoId)
  {
    var ruta = libros ? "api/libros" : "api/watchlist";
    var respuesta = await _http.PostAsync($"{ruta}/desde-catalogo/{catalogoId}", null);
    await ApiHelper.VerificarAsync(respuesta);
  }
}