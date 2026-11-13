using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Options;

namespace WatchlistApi.Services.Correo;

public class SmtpCorreoService : ICorreoService
{
  private readonly SmtpOpciones _o;
  private readonly ILogger<SmtpCorreoService> _log;

  public SmtpCorreoService(IOptions<SmtpOpciones> opciones, ILogger<SmtpCorreoService> log)
  {
    _o = opciones.Value;
    _log = log;
  }

  public async Task EnviarAsync(string para, string asunto, string html, string texto, CancellationToken ct = default)
  {
    var remitente = string.IsNullOrWhiteSpace(_o.From) ? _o.User : _o.From;

    using var msg = new MailMessage
    {
      From = new MailAddress(remitente, _o.FromName, Encoding.UTF8),
      Subject = asunto,
      SubjectEncoding = Encoding.UTF8,
      BodyEncoding = Encoding.UTF8
    };
    msg.To.Add(para);

    // Version en texto plano + version HTML: cada cliente de correo elige la que soporta.

    msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(texto, Encoding.UTF8, MediaTypeNames.Text.Plain));

    msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html));

    using var smtp = new SmtpClient(_o.Host, _o.Port)
    {
      EnableSsl = _o.EnableSsl,
      Credentials = new NetworkCredential(_o.User, _o.Password),
      DeliveryMethod = SmtpDeliveryMethod.Network,
      Timeout = 15000
    };

    try
    {
      await smtp.SendMailAsync(msg, ct);
      _log.LogInformation("Correo enviado a {Destino}: {Asunto}", Ocultar(para), asunto);
    }
    catch (Exception ex)
    {
      _log.LogError(ex, "No se pudo enviar el correo a {Destino}", Ocultar(para));
      throw;
    }
  }

  // "ana@correo.com" -> "a***@correo.com": los logs no deben guardar correos completos.
  private static string Ocultar(string correo)
  {
    var arroba = correo.IndexOf('@');
    return arroba <= 1 ? "***" : correo[0] + "***" + correo[arroba..];
  }
}