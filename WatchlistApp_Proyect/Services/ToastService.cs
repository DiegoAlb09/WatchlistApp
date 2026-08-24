namespace WatchlistApp_Proyect.Services;

public enum TipoToast { Exito, Error, Info }

public class ToastMensaje
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Texto { get; set; } = "";
  public TipoToast Tipo { get; set; } = TipoToast.Info;
}

public class ToastService
{
  public event Action? OnChange;
  public List<ToastMensaje> Mensajes { get; } = new();

  private const int DuracionMs = 3500;

  public void Exito(string texto) => Mostrar(texto, TipoToast.Exito);
  public void Error(string texto) => Mostrar(texto, TipoToast.Error);
  public void Info(string texto) => Mostrar(texto, TipoToast.Info);

  private void Mostrar(string texto, TipoToast tipo)
  {
    var toast = new ToastMensaje { Texto = texto, Tipo = tipo };
    Mensajes.Add(toast);
    OnChange?.Invoke();

    _ = QuitarDespuesAsync(toast.Id);
  }

  private async Task QuitarDespuesAsync(Guid id)
  {
    await Task.Delay(DuracionMs);
    Cerrar(id);
  }

  public void Cerrar(Guid id)
  {
    Mensajes.RemoveAll(m => m.Id == id);
    OnChange?.Invoke();
  }
}