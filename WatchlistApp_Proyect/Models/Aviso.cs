using System;

namespace WatchlistApp_Proyect.Models;

public class Aviso
{
  public Guid Id { get; set; }
  public bool EsLibro { get; set; }
  public string TituloAnterior { get; set; } = string.Empty;
  public string TituloNuevo { get; set; } = string.Empty;
  public string Resumen { get; set; } = string.Empty;
  public DateTime FechaCreacion { get; set; }
}
