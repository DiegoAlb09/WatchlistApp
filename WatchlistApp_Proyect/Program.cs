using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WatchlistApp_Proyect;
using WatchlistApp_Proyect.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Mismo puerto fijo configurado en WatchlistApi/Program.cs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5250/") });

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<LibraryStorageService>();
builder.Services.AddScoped<HistorialService>();

await builder.Build().RunAsync();
