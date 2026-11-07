namespace WatchlistApp_Proyect.Services;

/// <summary>
/// Reemplazo del confirm() nativo del navegador. Se usa igual que antes
/// (await, devuelve bool), pero se ve con el diseño de la app en vez del
/// dialogo feo del sistema operativo/navegador.
///
/// Uso:
///   var confirmado = await Confirm.PreguntarAsync("¿Eliminar \"X\"?");
///   if (!confirmado) return;
/// </summary>
public class ConfirmService
{
  public event Action? OnChange;

  public string Titulo { get; private set; } = "Confirmar";
  public string Mensaje { get; private set; } = "";
  public bool Visible { get; private set; }

  private TaskCompletionSource<bool>? _tcs;

  public Task<bool> PreguntarAsync(string mensaje, string titulo = "¿Estás seguro?")
  {
    // Si ya hay una pregunta pendiente (no debería pasar en el uso normal),
    // se responde "no" para no dejar la anterior colgada para siempre.
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