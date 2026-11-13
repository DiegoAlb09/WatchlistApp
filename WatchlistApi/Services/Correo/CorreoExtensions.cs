namespace WatchlistApi.Services.Correo;

public static class CorreoExtensions
{
  // Registra el servicio de correo. Si Smtp: Host, Smtp: User y Smtp: Password tienen valor se usa Smtp real
  // Si falta cualquiera, se usa la consola. El log de arranque dice cual quedo activo.

  public static IServiceCollection AddCorreo(this IServiceCollection services, IConfiguration config)
  {
    services.Configure<SmtpOpciones>(config.GetSection("Smtp"));

    var configurado =
      !string.IsNullOrWhiteSpace(config["Smtp:Host"]) &&
      !string.IsNullOrWhiteSpace(config["Smtp:User"]) &&
      !string.IsNullOrWhiteSpace(config["Smtp:Password"]);

    if (configurado)
      services.AddSingleton<ICorreoService, SmtpCorreoService>();
    else
      services.AddSingleton<ICorreoService, ConsolaCorreoService>();
    
    Console.WriteLine(configurado 
      ? $"[Correo] SMTP activo: {config["Smtp:Host"]}:{config["Smtp:Port"] ?? "587"}"
      : "[Correo] SMTP NO configurado: los correos se imprimiran en esta consola.");
    
    return services;
  }
}