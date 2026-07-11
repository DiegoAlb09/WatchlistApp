using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

/// <summary>
/// Mantiene el mismo nombre y los mismos metodos que antes (GetAllAsync, AddAsync, UpdateAsync, DeleteAsync)
/// para que Home.razor y AddItem.razor no necesiten ningun cambio.
/// Por dentro, ahora habla con el backend via HTTP, y la primera vez que se usa
/// migra automaticamente lo que hubiera en el localStorage viejo (sin borrarlo, como respaldo).
/// </summary>
public class LocalStorageService
{
    private const string OldStorageKey = "watchlist-items";
    private const string MigratedFlagKey = "watchlist-items-migrado";
    private const string ApiBase = "api/watchlist";

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private bool _migracionRevisada = false;

    public LocalStorageService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    private async Task AsegurarMigracionAsync()
    {
        if (_migracionRevisada) return;
        _migracionRevisada = true;

        var yaMigrado = await _js.InvokeAsync<string?>("localStorage.getItem", MigratedFlagKey);
        if (yaMigrado == "true") return;

        var json = await _js.InvokeAsync<string?>("localStorage.getItem", OldStorageKey);
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var itemsViejos = JsonSerializer.Deserialize<List<WatchlistItem>>(json) ?? new();
                if (itemsViejos.Count > 0)
                {
                    await _http.PostAsJsonAsync($"{ApiBase}/importar", itemsViejos);
                }
            }
            catch
            {
                // Si el JSON viejo estuviera corrupto, no bloqueamos la app; se puede revisar manualmente despues.
            }
        }

        // No borramos el localStorage viejo: queda como respaldo por si algo salio mal.
        await _js.InvokeVoidAsync("localStorage.setItem", MigratedFlagKey, "true");
    }

    public async Task<List<WatchlistItem>> GetAllAsync()
    {
        await AsegurarMigracionAsync();
        var items = await _http.GetFromJsonAsync<List<WatchlistItem>>(ApiBase);
        return items ?? new List<WatchlistItem>();
    }

    public async Task AddAsync(WatchlistItem item)
    {
        await AsegurarMigracionAsync();
        await _http.PostAsJsonAsync(ApiBase, item);
    }

    public async Task UpdateAsync(WatchlistItem item)
    {
        await AsegurarMigracionAsync();
        await _http.PutAsJsonAsync($"{ApiBase}/{item.Id}", item);
    }

    public async Task DeleteAsync(Guid id)
    {
        await AsegurarMigracionAsync();
        await _http.DeleteAsync($"{ApiBase}/{id}");
    }
}
