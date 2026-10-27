using System;
using System.Net.Http.Json;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

public class AvisosService
{
  private readonly HttpClient _http; 
  public AvisosService(HttpClient http) => _http = http;

  public async Task<List<Aviso>> PendientesAsync()
  {
    var lista = await _http.GetFromJsonAsync<List<Aviso>>("api/avisos");
    return lista ?? new List<Aviso>();
  }

  public async Task AceptarAsync(Guid id)
  {
    var respuesta = await _http.PostAsync($"api/avisos/{id}/aceptar", null);
    await ApiHelper.VerificarAsync(respuesta);
  }

  public async Task MantenerAsync(Guid id)
  {
    var respuesta = await _http.PostAsync($"api/avisos/{id}/mantener", null);
    await ApiHelper.VerificarAsync(respuesta);
  }
}
