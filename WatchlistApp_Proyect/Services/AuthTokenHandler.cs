using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WatchlistApp_Proyect.Services;

/// <summary>
/// Se engancha a TODAS las peticiones que hace HttpClient hacia el backend
/// (via AddHttpHandler en Program.cs). Antes de enviar, le agrega el
/// header "Authorization: Bearer &lt;token&gt;" si hay sesion. Si el backend
/// responde con 401 (token vencido, valido, o revocado), limpia la sesion local
/// y manda al usuario de vuelta a /login - asi ninguna pagina individual
/// necesita manejar ese caso a mano.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
  private const string TokenKey = "auth-token";
  private readonly IJSRuntime _js;
  private readonly NavigationManager _nav;

  public AuthTokenHandler(IJSRuntime js, NavigationManager nav)
  {
    _js = js;
    _nav = nav;
  }

  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, CancellationToken cancellationToken)
  {
    var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    if (!string.IsNullOrWhiteSpace(token))
    {
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    var respuesta = await base.SendAsync(request, cancellationToken);

    if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
    {
      await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
      _nav.NavigateTo("/login", forceLoad: true);
    }

    return respuesta;
  }
}