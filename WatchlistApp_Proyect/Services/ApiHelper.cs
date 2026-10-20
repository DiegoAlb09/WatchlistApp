using System.Net;

namespace WatchlistApp_Proyect.Services;

public class ApiException : Exception
{
  public int? CodigoEstado { get; }
  public ApiException(string mensaje, int? codigoEstado = null) : base(mensaje)
  {
    CodigoEstado = codigoEstado;
  }
}

public static class ApiHelper
{
  public static async Task VerificarAsync(HttpResponseMessage respuesta)
  {
    if (respuesta.IsSuccessStatusCode)
      return;
    
    var detalle = await respuesta.Content.ReadAsStringAsync();

    var mensaje = respuesta.StatusCode switch
    {
      HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Inicia sesión nuevamente.",
      HttpStatusCode.NotFound => "No se encontró lo que intentabas modificar (puede que ya se haya eliminado).",
      HttpStatusCode.BadRequest => string.IsNullOrWhiteSpace(detalle) ? "Datos inválidos." : detalle,
      _ => "Ocurrió un error inesperado. Intenta nuevamente más tarde."
    };

    throw new ApiException(mensaje, (int)respuesta.StatusCode);
  }

  public static string MensajeParaToast(Exception ex, string mensajeGenerico) => ex is ApiException api ? api.Message : mensajeGenerico;
}