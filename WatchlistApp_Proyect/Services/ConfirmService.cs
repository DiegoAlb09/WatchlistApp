namespace WatchlistApp_Proyect.Services;

/// <summary>Reemplazo del confirm() nativo: await Confirm.PreguntarAsync("...") devuelve bool.</summary>
public class ConfirmService
{
  public event Action? OnChange;

  public string Titulo { get; private set; } = "¿Estás seguro?";
  public string Mensaje { get; private set; } = "";
  public bool Visible { get; private set; }

  private TaskCompletionSource<bool>? _tcs;

  public Task<bool> PreguntarAsync(string mensaje, string titulo = "¿Estás seguro?")
  {
    _tcs?.TrySetResult(false);
    Mensaje = mensaje;
    Titulo = titulo;
    Visible = true;
    _tcs = new TaskCompletionSource<bool>();
    OnChange?.Invoke();
    return _tcs.Task;
  }

  public void Responder(bool resultado)
  {
    Visible = false;
    _tcs?.TrySetResult(resultado);
    OnChange?.Invoke();
  }
}