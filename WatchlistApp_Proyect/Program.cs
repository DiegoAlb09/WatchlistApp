using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WatchlistApp_Proyect;
using WatchlistApp_Proyect.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ===== HttpClient con el token adjuntado automaticamente =====
// Antes: builder.Services.AddScoped(sp => new HttpClient { BaseAddress = ... });
// Ahora: un HttpClient nombrado, con AuthTokenHandler enganchado via
// AddHttpMessageHandler, para que TODAS las peticiones (Series, Libros,
// Historial) lleven el header Authorization sin que cada servicio tenga que
// preocuparse por eso.
builder.Services.AddScoped<AuthTokenHandler>();

builder.Services
    .AddHttpClient("WatchlistApi", client => client.BaseAddress = new Uri("http://localhost:5250/"))
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("WatchlistApi"));

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<LibraryStorageService>();
builder.Services.AddScoped<HistorialService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ConfirmService>();

// ===== Autenticacion =====
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();