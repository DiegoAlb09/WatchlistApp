# 🎬📚📊 Mi Watchlist

Aplicación web para llevar el control de series, películas, libros, manga y cómics pendientes por ver/leer. Permite agregar títulos, clasificarlos, llevar un rango o avance de capítulos, ver el progreso general y estadísticas con gráficas, elegir algo al azar para ver, y llevar un calendario (semanal o mensual) de qué series están en emisión — todo con modo oscuro/claro.

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
- Agregar/Editar: título, portada (URL), tipo, y para series un **rango de capítulos** pendientes.
- Checklist de capítulos; para películas, checkbox de "Vista".
- **Tag "EN EMISIÓN"** en series activas, con día de la semana configurable.
- **📅 Calendario de estrenos** (modal), con dos vistas:
  - **Semana**: lista de qué series salen cada día de la semana.
  - **Mes**: grilla de calendario mensual completa, con navegación entre meses (‹ ›). Como el día de emisión es fijo semanalmente (no fechas exactas por episodio), la vista de mes repite ese día en cada semana — no requiere datos adicionales en el backend.
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
- 1 gráfica de barras: vistas por mes (últimos 6 meses), basada en el historial de eventos.
- Construidas con SVG puro (`DonutChart.razor`, `BarChart.razor`, reutilizables), sin librerías externas.

### Historial de vistas
- Cada vez que marcas un episodio, una película, o avanzas un capítulo de libro/manga/cómic, se registra un evento con fecha.
- Es un log que solo crece: nunca se borra ni se edita, ni siquiera si después desmarcas el episodio o retrocedes el capítulo.
- Alimenta la gráfica de "vistas por mes" en Estadísticas.

### Interfaz
- Íconos SVG (Heroicons + algunos propios: `chart`, `dice`) en vez de emojis, en un componente `Icon.razor` reutilizable.
- Inputs con estilo propio, buen contraste en ambos temas.
- Patrón de modal reutilizable (`.modal-overlay`/`.modal-card`), usado por "Sorpréndeme" y el calendario de estrenos.
- **CSS organizado en dos niveles**: módulos globales compartidos (`wwwroot/css/*.css`, ej. botones, formularios, layout) y **CSS isolation de Blazor** (`ComponentName.razor.css`) para estilos exclusivos de una sola página o componente (tarjetas, checklists, calendario, estadísticas, gráficas) — ver estructura abajo.

### Persistencia
- **Backend real**: ASP.NET Core Web API + Entity Framework Core + SQLite, con **migraciones reales** (`dotnet ef migrations add ...`) en vez de recrear la base de datos a mano en cada cambio de modelo.
- La app migra automáticamente los datos viejos de `localStorage` al backend la primera vez que corre.
- Las preferencias de interfaz (filtro/orden/búsqueda/compactar) siguen viviendo en `localStorage`.

---

## 🛠️ Tecnología

**Frontend**
- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) (.NET 8)
- C# + Razor Components
- CSS modular con variables (design tokens) + CSS isolation por componente
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
Al arrancar imprime la ruta exacta de `watchlist.db` que está usando. Las migraciones pendientes se aplican solas — no hace falta borrar la base de datos al cambiar el modelo, solo generar la migración nueva:
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
│   │   ├── Home.razor                # tag emision, calendario semana/mes, sorprendeme
│   │   ├── Home.razor.css            # CSS aislado: tarjetas, checklist, calendario
│   │   ├── AddItem.razor
│   │   ├── Libros.razor
│   │   ├── Libros.razor.css          # CSS aislado: diseño de lista, stepper
│   │   ├── AgregarLibro.razor
│   │   ├── Estadisticas.razor        # tarjetas + donas + barras + vistas por mes
│   │   └── Estadisticas.razor.css    # CSS aislado: solo lo que renderiza esta pagina
│   ├── Shared/
│   │   ├── MainLayout.razor
│   │   ├── Icon.razor
│   │   ├── DonutChart.razor
│   │   ├── DonutChart.razor.css      # CSS aislado propio (componente hijo)
│   │   ├── BarChart.razor
│   │   └── BarChart.razor.css        # CSS aislado propio (componente hijo)
│   └── wwwroot/
│       ├── css/
│       │   ├── app.css               # solo @import de lo verdaderamente compartido
│       │   ├── tokens.css            # incluye .libros-theme (variables compartidas)
│       │   ├── base.css / layout.css / buttons.css / forms.css / modal.css
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
Diego Alberto Aranda Gonzalez — Proyecto Personal, Ingeniería en Computación Inteligente.