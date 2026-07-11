using Microsoft.EntityFrameworkCore;
using WatchlistApi.Data;

var builder = WebApplication.CreateBuilder(args);

//Puerto fijo para que el cliente Blazor sepa siempre a donde apuntar
builder.WebHost.UseUrls("http://localhost:5250");

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=watchlist.db"));

// CORS abierto: valido para desarrollo local. Si algun dia se publica el backend
// a internet, esto deberia restringirse a los origenes reales del frontend
builder.Services.AddCors(opt => 
    opt.AddPolicy("DevCors", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("DevCors");
app.MapControllers();

app.Run();