using System.Text.Json;
using Microsoft.JSInterop;
using WatchlistApp_Proyect.Models;

namespace WatchlistApp_Proyect.Services;

public class LocalStorageService
{
  private const string StorageKey = "watchlist-items";
  private readonly IJSRuntime _js;
  public LocalStorageService(IJSRuntime js) => _js = js;
  public async Task<List<WatchlistItem>> GetAllAsync()
  {
    var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
    if (string.IsNullOrWhiteSpace(json)) return new List<WatchlistItem>();
    try { return JsonSerializer.Deserialize<List<WatchlistItem>>(json) ?? new(); }
    catch { return new List<WatchlistItem>(); }
  }
  public async Task SaveAllAsync(List<WatchlistItem> items)
  {
    var json = JsonSerializer.Serialize(items);
    await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
  }
  public async Task AddAsync(WatchlistItem item)
  {
    var items = await GetAllAsync();
    items.Add(item);
    await SaveAllAsync(items);
  }
  public async Task UpdateAsync(WatchlistItem item)
  {
    var items = await GetAllAsync();
    var i = items.FindIndex(x => x.Id == item.Id);
    if (i >= 0) { items[i] = item; await SaveAllAsync(items); }
  }
  public async Task DeleteAsync(Guid id)
  {
    var items = await GetAllAsync();
    var i = items.FindIndex(x => x.Id == id);
    await SaveAllAsync(items);
  }
}