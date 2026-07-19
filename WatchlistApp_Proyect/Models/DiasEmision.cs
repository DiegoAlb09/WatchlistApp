namespace WatchlistApp_Proyect.Models;

/// <summary>
/// Lista de dias de la semana en espanol, en orden Lunes-Domingo (a diferencia
/// del enum DayOfWeek de .NET, que empieza en Domingo=0). Se usa tanto en el 
/// formulario de agregar/editar como en el modal del calendario de estrenos.
/// </summary>
public static class DiasEmision
{
  public static readonly (DayOfWeek Dia, string Nombre)[] Todos = new[]
  {
    (DayOfWeek.Monday, "Lunes"),
    (DayOfWeek.Tuesday, "Martes"),
    (DayOfWeek.Wednesday, "Miércoles"),
    (DayOfWeek.Thursday, "Jueves"),
    (DayOfWeek.Friday, "Viernes"),
    (DayOfWeek.Saturday, "Sábado"),
    (DayOfWeek.Sunday, "Domingo")
  };
}