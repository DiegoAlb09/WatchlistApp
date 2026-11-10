namespace WatchlistApi.Models;

// Importante: el ORDEN de los valores es lo que esta guardado en la base de datos
// (SQLITE guardael numero, no el nombre). Si se cambia el orden, todos los titulos
// cambian de tipo sin avisar. Antes de borrar WatchlistItemEntity.cs y LibroItemEntity.cs,
// corre scripts/verificar-enums.ps1: confirma que estos dos enums son IDENTICOS a los originales.

public enum TipoContenido
{
  Serie,
  Pelicula
}

public enum TipoLibro
{
  Libro,
  Manga, 
  Comic
}