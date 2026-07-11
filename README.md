# 🎬📚 Mi Watchlist

Aplicación web para llevar el control de series, películas, libros, manga y cómics pendientes por ver/leer. Permite agregar títulos, clasificarlos, llevar un rango o avance de capítulos, y ver el progreso general de tu lista — todo con modo oscuro/claro.

Los datos ya **no viven solo en el navegador**: hay un backend real (ASP.NET Core Web API + SQLite) que los guarda de forma persistente. La primera vez que corras la app, cualquier dato que tuvieras en `localStorage` se migra automáticamente al backend, sin borrarse (queda de respaldo).

---

## ✨ Funcionalidades actuales

### Navegación
- Navbar superior para cambiar entre la sección **Series/Películas** y **Libros/Manga/Cómics**, cada una con su propio diseño.
- Botón de **modo oscuro/claro** (con ícono), con la preferencia guardada en `localStorage`.

### 🎬 Series y Películas
- Barra de progreso general, ponderada por capítulos vistos y películas vistas.
- Filtros: Todo / Solo series / Solo películas.
- Toggle **"Compactar completos"**: oculta lo ya terminado de la grilla principal, mostrándolo como lista compacta.
- Agregar/Editar: título, portada (URL), tipo, y para series un **rango de capítulos** pendientes (ej. del 1169 al 1170) — ideal para series muy largas.
- Checklist de capítulos dentro del rango; para películas, checkbox de "Vista" con diseño propio (cuadro con check, no un pill de ancho completo).
- Eliminar con confirmación previa.

### 📚 Libros, Manga y Cómics
- Diseño de **lista** (distinto al de Series), con portada tipo lomo de libro.
- Seguimiento por **stepper** (−/+) del capítulo/tomo actual.
- Barra de progreso individual por título (si defines una meta de capítulo/tomo).
- Filtros por tipo: Libro / Manga / Cómic.

### Interfaz
- Íconos SVG (Heroicons) en vez de emojis, en un componente `Icon.razor` reutilizable.
- Inputs de formularios con estilo propio y estado de foco visible.

### Persistencia
- **Backend real**: ASP.NET Core Web API + Entity Framework Core + SQLite (`watchlist.db`).
- La app migra automáticamente los datos viejos de `localStorage` al backend la primera vez que corre (sin borrar el respaldo local).
- Series/Películas y Libros/Manga/Cómics tienen sus propias tablas y endpoints.

---

## 🛠️ Tecnología

**Frontend**
- [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) (.NET 8)
- C# + Razor Components
- CSS puro con variables (modo oscuro/claro)

**Backend**
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core + SQLite
- CORS abierto para desarrollo local (sin autenticación todavía)

---

## ▶️ Cómo correrlo localmente

Requiere el [.NET SDK 8.0](https://dotnet.microsoft.com/download). Se necesitan **dos terminales abiertas al mismo tiempo**.

**Terminal 1 — Backend:**
```bash
cd WatchlistApi
dotnet restore
dotnet run
```
Debe quedar escuchando en `http://localhost:5250`. La base de datos `watchlist.db` se crea sola la primera vez.

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
│   │   ├── WatchlistItem.cs
│   │   └── LibroItem.cs
│   ├── Services/
│   │   ├── LocalStorageService.cs    # ahora habla con el backend + migra localStorage
│   │   └── LibraryStorageService.cs
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── AddItem.razor
│   │   ├── Libros.razor
│   │   └── AgregarLibro.razor
│   ├── Shared/
│   │   ├── MainLayout.razor
│   │   └── Icon.razor
│   └── wwwroot/
│       ├── css/app.css
│       └── js/theme.js
│
└── WatchlistApi/                ← Backend (ASP.NET Core Web API)
    ├── Models/
    │   ├── WatchlistItemEntity.cs
    │   └── LibroItemEntity.cs
    ├── Data/
    │   └── AppDbContext.cs
    ├── Controllers/
    │   ├── WatchlistController.cs
    │   └── LibrosController.cs
    ├── Program.cs
    └── watchlist.db              # se genera solo, no se sube al repo (.gitignore)
```

---

## 🚀 Roadmap — próximos pasos

### Corto plazo
- [ ] Diseñar los `InputText`/`InputNumber` restantes con más detalle (ya se mejoró el foco, falta pulir bordes/sombras).
- [ ] Subir portada como archivo local (base64) en vez de solo URL.
- [ ] Ordenar/buscar dentro de la lista.
- [ ] Recordar el filtro activo y el estado de "compactar completos" entre sesiones.

### Publicar el backend y el frontend (acceso público)
Actualmente ambos proyectos solo corren en `localhost`. Para que otras personas puedan usar la app desde internet:
- [ ] Publicar el **backend** en un hosting que soporte ASP.NET Core (Azure App Service, Render, Railway, etc.) — ya no es un sitio estático, necesita servidor corriendo.
- [ ] Publicar el **frontend** (Blazor WASM sigue siendo estático) en GitHub Pages o Azure Static Web Apps, apuntando su `HttpClient` a la URL pública del backend.
- [ ] Restringir el CORS del backend a la URL real del frontend publicado (ahora mismo está abierto con `AllowAnyOrigin`, válido solo para desarrollo local).
- [ ] Cambiar la base de datos de SQLite a algo compatible con el hosting elegido si hace falta (SQLite funciona bien para un solo usuario/proyecto personal; para más tráfico conviene PostgreSQL o Azure SQL).

### Login y sincronización entre dispositivos
Con el backend ya en pie, este es el siguiente paso natural:
- [ ] Sistema de **autenticación** (login) — opciones a evaluar:
  - ASP.NET Core Identity (email + contraseña).
  - Login con Google/GitHub (OAuth).
- [ ] Relacionar cada `WatchlistItemEntity`/`LibroItemEntity` con un usuario (agregar `UserId` a las tablas).
- [ ] Cada usuario autenticado ve solo su propia watchlist.
- [ ] Proteger los endpoints del backend (hoy son públicos, cualquiera con la URL puede leer/escribir).

### Otras ideas a futuro
- [ ] Estadísticas (ej. cuántas series/libros completaste este mes).
- [ ] Notas personales por título.
- [ ] Reemplazar los emojis restantes (si quedara alguno) por íconos de Heroicons.

---

## 👤 Autor
Diego Alberto Aranda Gonzalez — Proyecto personal, Ingeniería en Computación Inteligente, UAA.