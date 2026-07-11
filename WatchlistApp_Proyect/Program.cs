using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WatchlistApp_Proyect;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5250/") });

builder.Services.AddScoped<WatchlistApp_Proyect.Services.LocalStorageService>();
builder.Services.AddScoped<WatchlistApp_Proyect.Services.LibraryStorageService>();

await builder.Build().RunAsync();