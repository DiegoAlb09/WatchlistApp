using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WatchlistApi.Data;
using WatchlistApi.Models;

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

// Identity: solo para gestionar usuarios/contraseñas (hashing, etc.).
// El registro/login los maneja AuthController a mano, generando un JWT propio
// no usamos los endpoints integrados de Identity (MapIdentityApi) porque sus
// reglas de cookies/tokens para SPA's son mas dificiles de controlar
builder.Services
  .AddIdentityCore<ApplicationUser>(options =>
  {
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6; // razonable para desarrollo local
  })
  .AddEntityFrameworkStores<AppDbContext>();

// Autenticacion por JWT Bearer
var jwtKey = builder.Configuration["Jwt:Key"]
  ?? throw new InvalidOperationException("Falta configurar Jwt:Key en appsettings.json");

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuer = builder.Configuration["Jwt:Issuer"],
      ValidateAudience = true,
      ValidAudience = builder.Configuration["Jwt:Audience"],
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
      ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero      
    };
  });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();