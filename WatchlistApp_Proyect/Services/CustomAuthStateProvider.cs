using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace WatchlistApp_Proyect.Services;

/// <summary>
/// Le dice a Blazor (AuthorizeView, [Authorize] en paginas, etc.) si hay una
/// sesion iniciada, leyendo el JWT guardado en localStorage y decodificando
/// sus claims a mano - Blazor WASM no trae un decoder de JWT integrado.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
  private const string TokenKey = "auth-token";
  private readonly IJSRuntime _js;
  private readonly ClaimsPrincipal _anonimo = new(new ClaimsIdentity());

  public CustomAuthStateProvider(IJSRuntime js)
  {
    _js = js;
  }

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    if (string.IsNullOrEmpty(token))
      return new AuthenticationState(_anonimo);

    try
    {
      var claims = ParseClaimsFromJwt(token).ToList();

      // Si el token ya vencio, se trata como si no hubiera sesion.
      var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
      if (exp is not null && long.TryParse(exp, out var expUnix))
      {
        var expiraEn = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        if (expiraEn < DateTimeOffset.UtcNow)
          return new AuthenticationState(_anonimo);
      }

      var identidad = new ClaimsIdentity(claims, "jwt");
      return new AuthenticationState(new ClaimsPrincipal(identidad));
    }
    catch
    {
      return new AuthenticationState(_anonimo);
    }
  }

  /// <summary>
  /// Llamar despues de un login/registro exitoso
  /// </summary>
  public void NotificarUsuarioAutenticado()
  {
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }

  /// <summary>Llamar despues de cerrar sesion</summary>
  public void NotificarCierreSesion()
  {
    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonimo)));
  }
  private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
  {
    var payload = jwt.Split('.')[1];
    var jsonBytes = ParseBase64WithoutPadding(payload);
    var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)!;

    var claims = new List<Claim>();
    foreach (var kvp in keyValuePairs)
    {
      if (kvp.Value is JsonElement { ValueKind: JsonValueKind.Array } arreglo)
      {
        foreach (var item in arreglo.EnumerateArray())
          claims.Add(new Claim(kvp.Key, item.ToString()));
      }
      else
      {
        claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
      }
    }

    return claims;
  }

  private static byte[] ParseBase64WithoutPadding(string base64)
  {
    base64 = base64.Replace('-', '+').Replace('_', '/');
    switch (base64.Length % 4)
    {
      case 2: base64 += "=="; break;
      case 3: base64 += "="; break;
    }
    return Convert.FromBase64String(base64);
  }
}