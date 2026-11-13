namespace WatchlistApi.Services.Correo;

// Respaldo para desarrollo: no manda nada, imprime el correo en la consola del backend.
// Asi se puede probar "olvida mi contrasena" sin configurar SMTP

public class ConsolaCorreoService : ICorreoService
{
  private readonly ILogger<ConsolaCorreoService> _log;

  public ConsolaCorreoService(ILogger<ConsolaCorreoService> log) => _log = log;

  public Task EnviarAsync(string para, string asunto, string html, string texto, CancellationToken ct = default)
  {
    _log.LogWarning(
                "\n=========== CORREO (SMTP no configurado: no se envio, solo se muestra) ===========\n" +
                "Para:   {Para}\nAsunto: {Asunto}\n\n{Texto}\n" +
                "==================================================================================",
                para, asunto, texto);
    return Task.CompletedTask;
  }
}