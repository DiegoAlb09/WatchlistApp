namespace WatchlistApi.Services.Correo;

// Envia un correo. Hay dos implementaciones y se elige al arrancar (ver CorreoExtensions):
// SmtpCorreoService si hay credenciales SMTP configuradas, ConsolaCorreoService si no.

public interface ICorreoService
{
  // Lanza excepcion si no se pudo enviar: quien llama decide que hacer (en "olvide mi contraseña" se registra el error y se responde igual, para no revelar si el correo existe)
  Task EnviarAsync(string para, string asunto, string html, string texto, CancellationToken ct = default);
}