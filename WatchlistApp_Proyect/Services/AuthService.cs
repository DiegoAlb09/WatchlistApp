using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

public class ResultadoAuth
{
  public bool Exitoso { get; set; }
  public string? Error { get; set; }
}

public class AuthService
{
  private const string TokenKey = "auth-token";
  private readonly HttpClient _http;
  private readonly IJSRuntime _js;
  private readonly AuthenticationStateProvider _authStateProvider;

  public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
  {
    _http = http;
    _js = js;
    _authStateProvider = authStateProvider;
  }

  public Task<ResultadoAuth> RegistrarAsync(string email, string password) 
    => EnviarYProcesarAsync("api/auth/register", email, password);
  
  public Task<ResultadoAuth> IniciarSesionAsync(string email, string password) 
    => EnviarYProcesarAsync("api/auth/login", email, password);
  
  public async Task CerrarSesionAsync()
  {
    await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    ((CustomAuthStateProvider)_authStateProvider).NotificarCierreSesion();
  }

  public async Task<string?> ObtenerTokenAsync()
  {
    return await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
  }
  
  private async Task<ResultadoAuth> EnviarYProcesarAsync(string endpoint, string email, string password)
  {
    HttpResponseMessage respuesta;
    try
    {
      respuesta = await _http.PostAsJsonAsync(endpoint, new { Email = email, Password = password });
    }
    catch (Exception)
    {
      return new ResultadoAuth { Exitoso = false, Error = "No se pudo conectar con el servidor. ¿Está corriendo al backend?" };
    }

    if (!respuesta.IsSuccessStatusCode)
    {
      var mensaje = await respuesta.Content.ReadAsStringAsync();
      return new ResultadoAuth
      {
        Exitoso = false,
        Error = string.IsNullOrWhiteSpace(mensaje) ? "Correo o contraseña incorrectos." : mensaje
      };
    }

    var datos = await respuesta.Content.ReadFromJsonAsync<AuthResponse>();
    if (datos is null || string.IsNullOrWhiteSpace(datos.Token))
      return new ResultadoAuth { Exitoso = false, Error = "Respuesta inesperada del servidor." };
    
    await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, datos.Token);
    ((CustomAuthStateProvider)_authStateProvider).NotificarUsuarioAutenticado();

    return new ResultadoAuth { Exitoso = true };
  }
}