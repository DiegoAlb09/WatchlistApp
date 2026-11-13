using System.Net;

namespace WatchlistApi.Services.Correo;

/// <summary>Cuerpos de los correos. Estilos en linea porque los clientes de correo ignoran las hojas de estilo.</summary>
public static class PlantillaCorreo
{
  public static (string Asunto, string Html, string Texto) RecuperarContrasena(string enlace, int minutosDeVigencia)
  {
    const string asunto = "Restablece tu contraseña de Mi Watchlist";
    var href = WebUtility.HtmlEncode(enlace);

    var html = $@"<!doctype html>
<html lang=""es"">
<body style=""margin:0;padding:24px;background:#e3f1fe;font-family:'Segoe UI',Arial,sans-serif;color:#232629;"">
  <div style=""max-width:480px;margin:0 auto;background:#ffffff;border:1px solid #e3f1fe;border-radius:16px;padding:32px;"">
    <p style=""margin:0 0 4px;font-size:12px;font-weight:700;letter-spacing:.06em;color:#5c6066;"">MI WATCHLIST</p>
    <h1 style=""margin:0 0 16px;font-size:22px;line-height:1.2;"">Restablece tu contraseña</h1>
    <p style=""margin:0 0 20px;font-size:15px;line-height:1.5;"">
      Recibimos una solicitud para cambiar la contraseña de tu cuenta. Pulsa el botón para elegir una nueva.
    </p>
    <p style=""margin:0 0 24px;"">
      <a href=""{href}"" style=""display:inline-block;background:#ff5e24;color:#101828;text-decoration:none;font-weight:700;font-size:15px;padding:12px 22px;border-radius:6px;"">
        Elegir nueva contraseña
      </a>
    </p>
    <p style=""margin:0 0 8px;font-size:13px;color:#5c6066;line-height:1.5;"">
      El enlace vence en {minutosDeVigencia} minutos y solo sirve una vez. Si no fuiste tú, ignora este correo: tu contraseña no cambia.
    </p>
    <p style=""margin:16px 0 0;font-size:12px;color:#5c6066;word-break:break-all;"">
      ¿El botón no funciona? Copia este enlace en tu navegador:<br>{href}
    </p>
  </div>
</body>
</html>";

    var texto =
        "Mi Watchlist\n\n" +
        "Recibimos una solicitud para cambiar la contraseña de tu cuenta.\n" +
        "Abre este enlace para elegir una nueva:\n\n" +
        $"{enlace}\n\n" +
        $"El enlace vence en {minutosDeVigencia} minutos y solo sirve una vez.\n" +
        "Si no fuiste tú, ignora este correo: tu contraseña no cambia.\n";

    return (asunto, html, texto);
  }
}
