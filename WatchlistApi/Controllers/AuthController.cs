using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WatchlistApi.Data;
using WatchlistApi.Models;

namespace WatchlistApi.Controllers;

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly AppDbContext _db;
  private readonly IConfiguration _config;

  public AuthController(UserManager<ApplicationUser> userManager, AppDbContext db, IConfiguration config)
  {
    _userManager = userManager;
    _db = db;
    _config = config;
  }

  [HttpPost("register")]
  public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
  {
    var usuarioExistente = await _userManager.FindByEmailAsync(request.Email);
    if (usuarioExistente is not null)
      return BadRequest("Ya existe una cuenta con ese correo.");

    var usuario = new ApplicationUser { UserName = request.Email, Email = request.Email };
    var resultado = await _userManager.CreateAsync(usuario, request.Password);

    if (!resultado.Succeeded)
      return BadRequest(string.Join(" ", resultado.Errors.Select(e => e.Description)));

    // Le asigna a esta cuenta nueva cualquier dato que hubiera quedado
    // "sin dueño" (de antes de que existiera el login).
    await ReclamarDatosHuerfanosAsync(usuario.Id);

    var token = GenerarToken(usuario);
    return Ok(new AuthResponse(token, usuario.Email!));
  }

  [HttpPost("login")]
  public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
  {
    var usuario = await _userManager.FindByEmailAsync(request.Email);
    if (usuario is null)
      return Unauthorized("Correo o contraseña incorrectos.");

    var passwordValido = await _userManager.CheckPasswordAsync(usuario, request.Password);
    if (!passwordValido)
      return Unauthorized("Correo o contraseña incorrectos.");

    var token = GenerarToken(usuario);
    return Ok(new AuthResponse(token, usuario.Email!));
  }

  private async Task ReclamarDatosHuerfanosAsync(string userId)
  {
    // Series: las filas viejas sin dueño ya tienen catalogo (lo creo la
    // migracion del paso 1, con el mismo Id); falta crearles el vinculo.
    var series = await _db.WatchlistItems.Where(i => i.UserId == null).ToListAsync();
    foreach (var s in series)
    {
      s.UserId = userId;

      var catalogo = await _db.CatalogoSeries.FindAsync(s.Id);
      if (catalogo is not null) catalogo.CreadoPorUserId = userId;

      if (catalogo is not null)
        _db.UsuarioSeries.Add(new UsuarioSerieEntity
        {
          Id = s.Id,
          UserId = userId,
          CatalogoSerieId = s.Id,
          Visto = s.Visto,
          EpisodiosVistos = s.EpisodiosVistos,
          FechaAgregado = s.FechaAgregado
        });
    }

    var libros = await _db.LibroItems.Where(i => i.UserId == null).ToListAsync();
    foreach (var l in libros)
    {
      l.UserId = userId;

      var catalogo = await _db.CatalogoLibros.FindAsync(l.Id);
      if (catalogo is not null) catalogo.CreadoPorUserId = userId;

      if (catalogo is not null)
        _db.UsuarioLibros.Add(new UsuarioLibroEntity
        {
          Id = l.Id,
          UserId = userId,
          CatalogoLibroId = l.Id,
          CapituloActual = l.CapituloActual,
          FechaAgregado = l.FechaAgregado
        });
    }

    var historial = await _db.HistorialVistos.Where(h => h.UserId == null).ToListAsync();
    foreach (var h in historial) h.UserId = userId;

    await _db.SaveChangesAsync();
  }

  private string GenerarToken(ApplicationUser usuario)
  {
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, usuario.Id),
      new Claim(ClaimTypes.Email, usuario.Email ?? "")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _config["Jwt:Issuer"],
      audience: _config["Jwt:Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddDays(30),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}