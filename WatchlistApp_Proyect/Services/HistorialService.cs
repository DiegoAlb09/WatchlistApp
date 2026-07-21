using System.Net.Http.Json;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

public class HistorialService
{
  private const string ApiBase = "api/historial";
  private readonly HttpClient _http;

  public HistorialService(HttpClient http)
  {
    _http = http;
  }

  public async Task<List<HistorialVisto>> GetAllAsync()
  {
    var items = await _http.GetFromJsonAsync<List<HistorialVisto>>(ApiBase);
    return items ?? new List<HistorialVisto>();
  }

  public async Task RegistrarAsync(Guid itemId, string titulo)
  {
    var evento = new HistorialVisto { ItemId = itemId, Titulo = titulo };
    await _http.PostAsJsonAsync(ApiBase, evento);
  }
}