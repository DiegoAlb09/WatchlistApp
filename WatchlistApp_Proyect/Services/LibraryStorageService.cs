using System.Text.Json;
using Microsoft.JSInterop;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

public class LibraryStorageService
{
    private const string StorageKey = "library-items";
    private readonly IJSRuntime _js;

    public LibraryStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<List<LibroItem>> GetAllAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrWhiteSpace(json))
            return new List<LibroItem>();

        try
        {
            return JsonSerializer.Deserialize<List<LibroItem>>(json) ?? new List<LibroItem>();
        }
        catch
        {
            return new List<LibroItem>();
        }
    }

    public async Task SaveAllAsync(List<LibroItem> items)
    {
        var json = JsonSerializer.Serialize(items);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task AddAsync(LibroItem item)
    {
        var items = await GetAllAsync();
        items.Add(item);
        await SaveAllAsync(items);
    }

    public async Task UpdateAsync(LibroItem item)
    {
        var items = await GetAllAsync();
        var index = items.FindIndex(i => i.Id == item.Id);
        if (index >= 0)
        {
            items[index] = item;
            await SaveAllAsync(items);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var items = await GetAllAsync();
        items.RemoveAll(i => i.Id == id);
        await SaveAllAsync(items);
    }
}
