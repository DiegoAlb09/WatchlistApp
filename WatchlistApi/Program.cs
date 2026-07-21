using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Puerto fijo para que el cliente Blazor sepa siempre a donde apuntar
builder.WebHost.UseUrls("http://localhost:5250");

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null); // JSON en PascalCase, igual que el cliente

// Ruta ABSOLUTA y anclada a la carpeta del proyecto (donde vive este Program.cs),
// sin importar desde donde se ejecute "dotnet run" (terminal, IDE, etc.).
// Antes usabamos una ruta relativa ("watchlist.db"), que dependia del
// directorio de trabajo del proceso y podia terminar creando archivos
// .db distintos segun como se arrancara el backend.
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "watchlist.db");
Console.WriteLine($"[WatchlistApi] Usando base de datos en: {dbPath}");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// CORS abierto: valido para desarrollo local. Si algun dia se publica el backend
// a internet, esto deberia restringirse a los origenes reales del frontend.
builder.Services.AddCors(opt =>
    opt.AddPolicy("DevCors", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Aplica automaticamente cualquier migracion pendiente al arrancar.
using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  db.Database.Migrate();
}

app.UseCors("DevCors");
app.MapControllers();

app.Run();