# 🎬📚📊 Mi Watchlist

Aplicación web para llevar el control de series, películas, libros, manga y cómics pendientes por ver/leer. Permite agregar títulos, clasificarlos, llevar un rango o avance de capítulos, ver el progreso general y estadísticas con gráficas, elegir algo al azar para ver, y llevar un calendario de qué series están en emisión — todo con modo oscuro/claro.

Los datos ya **no viven solo en el navegador**: hay un backend real (ASP.NET Core Web API + SQLite + migraciones de EF Core) que los guarda de forma persistente. La primera vez que corras la app, cualquier dato que tuvieras en `localStorage` se migra automáticamente al backend, sin borrarse (queda de respaldo).

---

## ✨ Funcionalidades actuales

### Navegación
- Navbar superior con tres pestañas: **Series/Películas**, **Libros/Manga/Cómics** y **Estadísticas**.
- Botón de **modo oscuro/claro**, con la preferencia guardada en `localStorage`.

### 🎬 Series y Películas
- Barra de progreso general, ponderada por capítulos vistos y películas vistas.
- Filtros: Todo / Solo series / Solo películas / En progreso.
- Búsqueda por título y orden (recientes, antiguos, más avanzados primero, título A-Z/Z-A).
- Toggle **"Compactar completos"**.
- El filtro, la búsqueda, el orden y "compactar completos" se recuerdan entre sesiones.
- Agregar/Editar: título, portada, tipo, y para series un **rango de capítulos** pendientes.
- Checklist de capítulos; para películas, checkbox de "Vista".
- **Tag "EN EMISIÓN"** en series activas, con día de la semana configurable.
- **📅 Calendario de estrenos** (modal): muestra qué series "en emisión" salen cada día de la semana, resaltando el día de hoy.
- **🎲 Botón "Sorpréndeme"** (modal): elige al azar algo pendiente por ver, sin repetir la última elección salvo que sea la única opción disponible.
- Eliminar con confirmación previa.

### 📚 Libros, Manga y Cómics
- Diseño de lista, con portada tipo lomo de libro y stepper (−/+) de capítulo/tomo.
- Barra de progreso individual (si defines una meta).
- Filtros, búsqueda y orden — mismo comportamiento que en Series, con sus propias preferencias guardadas.

### 📊 Estadísticas
- 4 tarjetas de resumen: títulos totales, completados, en progreso, sin empezar.
- 2 gráficas de dona: progreso general de cada sección.
- 1 gráfica de barras: distribución por tipo (Series, Películas, Libros, Manga, Cómics).
- **1 gráfica de barras: vistas por mes (últimos 6 meses)**, basada en el historial de eventos (ver abajo).
- Construidas con SVG puro (`DonutChart.razor`, `BarChart.razor`, reutilizables), sin librerías externas.

### Historial de vistas
- Cada vez que marcas un episodio, una película, o avanzas un capítulo de libro/manga/cómic, se registra un evento con fecha.
- **Es un log que solo crece**: nunca se borra ni se edita, ni siquiera si después desmarcas el episodio o retrocedes el capítulo — el historial refleja "esto pasó", no el estado actual.
- Alimenta la gráfica de "vistas por mes" en Estadísticas.

### Interfaz
- Íconos SVG (Heroicons + algunos propios: `chart`, `dice`) en vez de emojis, en un componente `Icon.razor` reutilizable.
- Inputs con estilo propio, buen contraste en ambos temas.
- Patrón de modal reutilizable (`.modal-overlay`/`.modal-card`), usado por "Sorpréndeme" y el calendario de estrenos.

### Persistencia
- **Backend real**: ASP.NET Core Web API + Entity Framework Core + SQLite, con **migraciones reales** (`dotnet ef migrations add ...`) en vez de recrear la base de datos a mano en cada cambio de modelo.
- La app migra automáticamente los datos viejos de `localStorage` al backend la primera vez que corre.
- Las preferencias de interfaz (filtro/orden/búsqueda/compactar) siguen viviendo en `localStorage`.

---

## 🛠️ Tecnología

**Frontend**
- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) (.NET 8)
- C# + Razor Components
- CSS modular con variables (design tokens)
- Gráficas SVG hechas a mano (sin dependencias de charting)

**Backend**
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core + SQLite, con migraciones (`dotnet ef`)
- CORS abierto para desarrollo local (sin autenticación todavía)

---

## ▶️ Cómo correrlo localmente

Requiere el [.NET SDK 8.0](https://dotnet.microsoft.com/download) y la herramienta `dotnet-ef` (`dotnet tool install --global dotnet-ef`). Se necesitan **dos terminales abiertas al mismo tiempo**.

**Terminal 1 — Backend:**
```bash
cd WatchlistApi
dotnet restore
dotnet run
```
Al arrancar imprime la ruta exacta de `watchlist.db` que está usando (útil para depurar si algo no cuadra). Las migraciones pendientes se aplican solas — no hace falta borrar la base de datos al cambiar el modelo, solo generar la migración nueva:
```bash
dotnet ef migrations add NombreDelCambio
```

**Terminal 2 — Frontend:**
```bash
cd WatchlistApp_Proyect
dotnet restore
dotnet watch run
```
Abre en el navegador la URL que muestre la terminal (ej. `http://localhost:5224`).

> ⚠️ Si el backend no está corriendo, el frontend va a fallar al cargar los datos (error de red/CORS en la consola del navegador).

---

## 📁 Estructura del proyecto (monorepo)

```
Watchlist/                       ← raíz del repo (.git aquí)
├── WatchlistApp_Proyect/        ← Frontend (Blazor WebAssembly)
│   ├── Models/
│   │   ├── WatchlistItem.cs          # incluye EnEmision, DiaEmision
│   │   ├── LibroItem.cs
│   │   ├── HistorialVisto.cs
│   │   └── DiasEmision.cs            # helper: dias de la semana en espanol
│   ├── Services/
│   │   ├── LocalStorageService.cs    # habla con el backend + migra localStorage
│   │   ├── LibraryStorageService.cs
│   │   └── HistorialService.cs       # solo GetAll y Registrar (append-only)
│   ├── Pages/
│   │   ├── Home.razor                # tag emision, calendario, sorprendeme
│   │   ├── AddItem.razor             # checkbox + selector de dia de emision
│   │   ├── Libros.razor
│   │   ├── AgregarLibro.razor
│   │   └── Estadisticas.razor        # tarjetas + donas + barras + vistas por mes
│   ├── Shared/
│   │   ├── MainLayout.razor
│   │   ├── Icon.razor
│   │   ├── DonutChart.razor
│   │   └── BarChart.razor
│   └── wwwroot/
│       ├── css/
│       │   ├── app.css               # solo @import de los modulos
│       │   ├── tokens.css / base.css / layout.css / buttons.css
│       │   ├── forms.css / cards.css / checklist.css / libros.css
│       │   ├── estadisticas.css / modal.css / calendario.css
│       └── js/theme.js
│
└── WatchlistApi/                ← Backend (ASP.NET Core Web API)
    ├── Models/
    │   ├── WatchlistItemEntity.cs    # incluye EnEmision, DiaEmision
    │   ├── LibroItemEntity.cs
    │   └── HistorialVistoEntity.cs
    ├── Data/
    │   └── AppDbContext.cs
    ├── Controllers/
    │   ├── WatchlistController.cs
    │   ├── LibrosController.cs
    │   └── HistorialController.cs    # solo GET y POST (append-only)
    ├── Migrations/                    # SI se sube a git (es codigo, no datos)
    ├── Program.cs
    └── watchlist.db                   # se genera solo, no se sube al repo (.gitignore)
```

---

## 🚀 Roadmap — próximos pasos

### Corto plazo
- [ ] Subir portada como archivo local (base64) en vez de solo URL.
- [ ] Migrar `cards.css`, `checklist.css`, `libros.css`, `estadisticas.css`, `calendario.css` a CSS isolation de Blazor.
- [ ] Vista de calendario mensual completo para "En emisión" (hoy es solo vista semanal por día fijo).

### Publicar el backend y el frontend (acceso público)
- [ ] Publicar el **backend** en un hosting que soporte ASP.NET Core (Azure App Service, Render, Railway, etc.).
- [ ] Publicar el **frontend** en GitHub Pages o Azure Static Web Apps, apuntando su `HttpClient` a la URL pública del backend.
- [ ] Restringir el CORS del backend a la URL real del frontend publicado.
- [ ] Evaluar si SQLite sigue siendo suficiente o conviene PostgreSQL/Azure SQL.

### Login y sincronización entre dispositivos
- [ ] Sistema de **autenticación** (ASP.NET Core Identity, o login con Google/GitHub vía OAuth).
- [ ] Relacionar cada entidad con un usuario (`UserId`).
- [ ] Cada usuario autenticado ve solo su propia watchlist.
- [ ] Proteger los endpoints del backend (hoy son públicos).

### Otras ideas a futuro
- [ ] Notas personales por título.
- [ ] Exportar/importar la watchlist como JSON (respaldo manual mientras no haya cuentas).

---

## 👤 Autor
Diego Alberto Aranda Gonzalez — Proyecto Personal, Ingeniería en Computación Inteligente