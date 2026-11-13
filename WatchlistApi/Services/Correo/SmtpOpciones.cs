namespace WatchlistApi.Services.Correo;

// Se llena desde la seccion "Smtp" de la configuracion (variables de entorno: Smtp__Host, etc.)

public class SmtpOpciones
{
  public string Host { get; set; } = "";
  public int Port { get; set; } = 587;
  public string User { get; set; } = "";
  public string Password { get; set; } = "";
  
  // Si queda vacio se usa User
  public string From { get; set; } = "";
  public string FromName { get; set; } = "Mi Watchlist";
  
  // true = STARTTLS en el puerto 587 (lo normal en Gmail y Brevo)
  public bool EnableSsl { get; set; } = true;
}