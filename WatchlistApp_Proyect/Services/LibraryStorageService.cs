using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

public class LibraryStorageService
{
    private const string OldStorageKey = "library-items";
    private const string MigratedFlagKey = "library-items-migrado";
    private const string ApiBase = "api/libros";

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private bool _migracionRevisada = false;

    public LibraryStorageService(HttpClient http, IJSRuntime js)
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
                var itemsViejos = JsonSerializer.Deserialize<List<LibroItem>>(json) ?? new();
                if (itemsViejos.Count > 0)
                {
                    await _http.PostAsJsonAsync($"{ApiBase}/importar", itemsViejos);
                }
            }
            catch
            {
                // JSON viejo corrupto: no bloqueamos la app.
            }
        }

        await _js.InvokeVoidAsync("localStorage.setItem", MigratedFlagKey, "true");
    }

    public async Task<List<LibroItem>> GetAllAsync()
    {
        await AsegurarMigracionAsync();
        var items = await _http.GetFromJsonAsync<List<LibroItem>>(ApiBase);
        return items ?? new List<LibroItem>();
    }

    public async Task AddAsync(LibroItem item)
    {
        await AsegurarMigracionAsync();
        await _http.PostAsJsonAsync(ApiBase, item);
    }

    public async Task UpdateAsync(LibroItem item)
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
