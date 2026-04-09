# Día 02 — ASP.NET Core Web API + ASP.NET MVC (Legacy vs Moderno)

> **Rama Git:** `dia-02/aspnet-api`  
> **Método:** 16 Pomodoros de 25 min (5 min descanso entre cada uno)  
> **Objetivo:** Crear la API REST completa: controladores, JWT, Swagger, middleware de errores. Entender la diferencia entre MVC legacy y Core.

---

## Cronograma Pomodoro

| # | Bloque | Qué hacer |
|---|--------|-----------|
| 1 | Teoría | MVC Legacy vs ASP.NET Core — diferencias clave |
| 2 | Teoría | Pipeline de middleware y Program.cs |
| 3 | Config | Instalar paquetes, configurar Program.cs con DI, CORS y Swagger |
| 4 | Config | Configurar autenticación JWT en Program.cs |
| 5 | Auth | Crear AuthController: registro y login |
| 6 | Auth | Crear servicio de autenticación con hash de contraseña |
| 7 | Productos | Crear ProductosController: GET endpoints públicos |
| 8 | Productos | Agregar POST/PUT/DELETE protegidos con [Authorize] |
| 9 | Categorías | Crear CategoriasController completo |
| 10 | Carrito | Crear CarritoController: agregar, quitar, actualizar |
| 11 | Carrito | Agregar endpoint de checkout y conectar con CarritoServicio |
| 12 | Órdenes | Crear OrdenesController: listar, detalle, cancelar |
| 13 | Middleware | Crear ManejadorErroresMiddleware para errores globales |
| 14 | Swagger | Configurar Swagger con JWT y documentación XML |
| 15 | Pruebas | Crear archivo requests.http y probar todos los endpoints |
| 16 | Merge | Build, test, merge a desarrollo, push a GitHub |

---

## Pomodoro 1 — ASP.NET MVC Legacy vs ASP.NET Core (25 min)

Esta es la sección más importante para la entrevista. La vacante pide experiencia en ambos.

### ¿Qué es ASP.NET MVC (.NET Framework)?

Es el framework web que Microsoft creó en 2009 sobre .NET Framework. Genera HTML en el servidor usando vistas Razor (archivos `.cshtml`). jQuery y JavaScript se agregan dentro de esas vistas para interactividad.

### ¿Qué es ASP.NET Core?

Es la reescritura completa que Microsoft lanzó en 2016. Es multiplataforma, modular y mucho más rápido. Puede generar HTML (con Razor Pages) o funcionar como API pura que devuelve JSON.

### Tabla comparativa (memorízala para la entrevista)

| Concepto | MVC Legacy (.NET Framework) | ASP.NET Core (.NET 10) |
|----------|---------------------------|----------------------|
| **Plataforma** | Solo Windows | Windows, Linux, macOS |
| **Punto de entrada** | `Global.asax` + `Startup.cs` | `Program.cs` (unificado) |
| **Configuración** | `Web.config` (XML) | `appsettings.json` (JSON) |
| **Servidor** | IIS obligatorio | Kestrel (propio) + cualquier reverse proxy |
| **Dependencias** | `System.Web` (monolítico) | Paquetes NuGet modulares |
| **Inyección de dependencias** | Manual o con librería externa (Ninject, Unity) | Nativa en el framework |
| **Middleware** | Filtros + módulos HTTP | Pipeline de middleware (`app.Use...`) |
| **Vistas** | Razor Views (`.cshtml`) obligatorias | Opcional: Razor, o solo JSON (API) |
| **Frontend** | jQuery embebido en Razor | SPA separada (Vue, React, Angular) |
| **Rendimiento** | ~50k req/seg | ~7M req/seg (Kestrel) |
| **Estado** | Mantenimiento (no se actualiza) | Activo (nuevas versiones cada año) |

### Ejemplo: Cómo se veía un controlador MVC Legacy

```csharp
// ASP.NET MVC (.NET Framework 4.5)
// El controlador GENERA HTML — devuelve una vista Razor

public class ProductosController : Controller  // hereda de Controller, no ApiController
{
    private readonly ProductoRepository _repo;  // sin inyección de dependencias nativa

    public ProductosController()
    {
        // Crear dependencias manualmente (anti-patrón)
        _repo = new ProductoRepository(new SqlConnection("..."));
    }

    // GET /Productos
    // Retorna una VISTA HTML, no JSON
    public ActionResult Index()
    {
        var productos = _repo.ObtenerTodos();
        return View(productos);  // busca Views/Productos/Index.cshtml
    }

    // GET /Productos/Detalle/5
    public ActionResult Detalle(int id)
    {
        var producto = _repo.ObtenerPorId(id);
        if (producto == null)
            return HttpNotFound();  // no existe NotFound()
        return View(producto);
    }

    // POST /Productos/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]  // protección CSRF para formularios
    public ActionResult Crear(ProductoViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);  // re-renderiza el formulario con errores

        _repo.Guardar(modelo);
        return RedirectToAction("Index");  // redirige a la lista
    }
}
```

```html
<!-- Views/Productos/Index.cshtml (vista Razor) -->
@model IEnumerable<Producto>

<h2>Catálogo de productos</h2>
<table class="table">
    @foreach (var p in Model)
    {
        <tr>
            <td>@p.Nombre</td>
            <td>@p.Precio.ToString("C")</td>
            <td>
                <!-- jQuery vive AQUÍ, dentro de la vista -->
                <button class="btn-agregar" data-id="@p.Id">
                    Agregar al carrito
                </button>
            </td>
        </tr>
    }
</table>

<!-- jQuery al final de la vista -->
<script src="~/Scripts/jquery-3.x.min.js"></script>
<script>
    $('.btn-agregar').click(function() {
        var id = $(this).data('id');
        $.post('/Carrito/Agregar', { productoId: id }, function(response) {
            alert('Producto agregado');
        });
    });
</script>
```

### Ejemplo: El mismo controlador en ASP.NET Core

```csharp
// ASP.NET Core 8
// El controlador devuelve JSON — NO genera HTML

[ApiController]                          // marca como API (validación automática)
[Route("api/[controller]")]              // ruta base: /api/productos
public class ProductosController : ControllerBase  // ControllerBase, no Controller
{
    private readonly IUnidadDeTrabajo _uow;

    // Inyección de dependencias por constructor (nativa)
    public ProductosController(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    // GET /api/productos
    // Retorna JSON, no HTML
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var productos = await _uow.Productos.ObtenerActivosAsync();
        return Ok(productos);  // 200 + JSON
    }

    // GET /api/productos/5
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);
        if (producto == null)
            return NotFound();  // 404
        return Ok(producto);
    }

    // POST /api/productos (solo admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]  // JWT, no cookies
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        // [ApiController] valida automáticamente — no necesitas ModelState.IsValid
        // ...crear producto...
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, dto);
    }
}
```

### Diferencias clave que te preguntarán

| Pregunta | Respuesta corta |
|----------|----------------|
| ¿`Controller` vs `ControllerBase`? | `Controller` tiene soporte para vistas Razor. `ControllerBase` es solo para APIs (sin vistas). |
| ¿`ActionResult` vs `IActionResult`? | Son equivalentes en Core. `IActionResult` es la interfaz, `ActionResult` es la clase base. En MVC legacy solo existía `ActionResult`. |
| ¿`[ValidateAntiForgeryToken]` vs JWT? | CSRF tokens protegen formularios HTML. JWT protege APIs — el token va en el header `Authorization`, no en un cookie. |
| ¿Por qué Core no necesita `Web.config`? | Porque la configuración es modular: `appsettings.json` para settings, `Program.cs` para servicios, paquetes NuGet para cada funcionalidad. |
| ¿Cómo migrarías un proyecto MVC a Core? | Gradualmente: primero extraer la lógica a una capa Core compartida, luego crear la API en Core que expone la misma funcionalidad como JSON, y finalmente reemplazar las vistas Razor por un frontend SPA (Vue.js). |

> **Para la entrevista:** No digas "no sé MVC legacy". Di: "He trabajado con ambos.
> En proyectos legacy, el controlador retornaba vistas Razor con jQuery embebido.
> En proyectos modernos, uso ASP.NET Core como API que devuelve JSON y el frontend
> es una SPA independiente en Vue.js. Puedo mantener código legacy y también construir
> desde cero con el stack moderno."

---

## Pomodoro 2 — Pipeline de middleware y Program.cs (25 min)

### ¿Qué es el middleware?

Cada petición HTTP pasa por una cadena de componentes (middleware) antes de llegar a tu controlador. Cada uno puede procesar, modificar o rechazar la petición.

### Orden del pipeline (importa el orden)

```
Petición HTTP entrante
    │
    ▼
┌─────────────────────────┐
│ 1. Manejo de errores    │  ← atrapa excepciones de todo lo que sigue
├─────────────────────────┤
│ 2. CORS                 │  ← permite peticiones desde Vue.js
├─────────────────────────┤
│ 3. Autenticación        │  ← lee y valida el token JWT
├─────────────────────────┤
│ 4. Autorización         │  ← verifica roles y permisos
├─────────────────────────┤
│ 5. Routing              │  ← encuentra qué controlador manejar
├─────────────────────────┤
│ 6. Tu Controlador       │  ← tu código se ejecuta aquí
└─────────────────────────┘
    │
    ▼
Respuesta HTTP al cliente
```

> **Concepto Senior:** Si pones `UseAuthentication()` después de `UseAuthorization()`,
> la autorización se ejecuta antes de leer el token — y todo endpoint protegido falla.
> El orden del pipeline es la causa #1 de bugs misteriosos en ASP.NET Core.

---

## Pomodoro 3 — Configurar Program.cs (25 min)

### Paquetes necesarios

```powershell
cd src/EcommerceNet.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
dotnet add package BCrypt.Net-Next
```

### Archivo: `Program.cs`

```csharp
using System.Text;
using EcommerceNet.Core.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// === SERVICIOS (Inyección de Dependencias) ===

// Controladores
builder.Services.AddControllers();

// Swagger para documentación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — permitir peticiones desde Vue.js (puerto 5173)
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirVue", politica =>
    {
        politica.WithOrigins("http://localhost:5173")  // Vue.js dev server
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Autenticación JWT (se configura en Pomodoro 4)
// builder.Services.AddAuthentication(...)

// Registrar servicios de negocio
// Scoped = una instancia por cada petición HTTP
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();

// Registrar repositorios y UoW (Día 3 con EF Core)
// builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

var app = builder.Build();

// === PIPELINE DE MIDDLEWARE (el orden importa) ===

// 1. Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Redirigir HTTP a HTTPS
app.UseHttpsRedirection();

// 3. CORS — antes de auth
app.UseCors("PermitirVue");

// 4. Autenticación — lee el token JWT
app.UseAuthentication();

// 5. Autorización — verifica roles
app.UseAuthorization();

// 6. Mapear controladores
app.MapControllers();

app.Run();
```

> **Concepto Senior: Los 3 ciclos de vida de DI**
>
> `AddTransient<IServicio, Servicio>()` — nueva instancia CADA VEZ que alguien la pide.
> Ejemplo: un generador de PDFs.
>
> `AddScoped<IServicio, Servicio>()` — una instancia POR REQUEST HTTP.
> Ejemplo: repositorios, DbContext, servicios de negocio.
> Es el más usado porque cada petición tiene su propia transacción.
>
> `AddSingleton<IServicio, Servicio>()` — UNA instancia para toda la vida de la app.
> Ejemplo: configuración, caché en memoria.
>
> ¿Por qué importa? Si registras el DbContext como Singleton, todos los requests
> comparten la misma conexión a BD — bug de concurrencia garantizado.

### Commit

```powershell
git add .
git commit -m "feat: configurar Program.cs con DI, CORS y pipeline de middleware"
```

---

## Pomodoro 4 — Configurar autenticación JWT (25 min)

### ¿Qué es JWT?

Un JSON Web Token es una cadena codificada que contiene información del usuario (ID, email, rol). El servidor lo genera al hacer login y el cliente lo envía en cada petición en el header `Authorization: Bearer {token}`.

### Agregar configuración en `appsettings.json`

```json
{
  "Jwt": {
    "Key": "EstaEsMiClaveSecretaSuperSeguraDe256Bits!!",
    "Issuer": "EcommerceNet.API",
    "Audience": "EcommerceNet.Web",
    "ExpireMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EcommerceNetDB;Trusted_Connection=True;"
  }
}
```

### Agregar JWT en Program.cs (después de AddCors)

```csharp
// Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(opciones =>
{
    opciones.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opciones.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opciones =>
{
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});
```

> **¿Qué valida cada parámetro?**
> - `ValidateIssuer` — ¿quién generó el token? (nuestra API)
> - `ValidateAudience` — ¿para quién es? (nuestro frontend)
> - `ValidateLifetime` — ¿el token expiró?
> - `ValidateIssuerSigningKey` — ¿la firma es válida? (no fue alterado)

---

## Pomodoros 5-6 — Autenticación: controlador y servicio (50 min)

### Archivo: `DTOs/AuthDtos.cs` (crear en EcommerceNet.Core/DTOs/)

```csharp
namespace EcommerceNet.Core.DTOs;

/// <summary>DTO para registro de usuario</summary>
public class RegistroDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>DTO para login</summary>
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Respuesta del login con el token JWT</summary>
public class AuthRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
}
```

### Archivo: `Servicios/IAuthServicio.cs` (en Core)

```csharp
using EcommerceNet.Core.DTOs;

namespace EcommerceNet.Core.Servicios;

public interface IAuthServicio
{
    Task<Resultado<AuthRespuestaDto>> RegistrarAsync(RegistroDto dto);
    Task<Resultado<AuthRespuestaDto>> LoginAsync(LoginDto dto);
}
```

### Archivo: `Controllers/AuthController.cs` (en API)

```csharp
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _authServicio;

    public AuthController(IAuthServicio authServicio)
    {
        _authServicio = authServicio;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        var resultado = await _authServicio.RegistrarAsync(dto);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>
    /// Iniciar sesión — devuelve un token JWT
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authServicio.LoginAsync(dto);

        if (!resultado.Exito)
            return Unauthorized(resultado);

        return Ok(resultado);
    }
}
```

> **¿Por qué Unauthorized (401) y no BadRequest (400)?**
> - `400 BadRequest` = los datos que enviaste están mal formados (ej: falta el email)
> - `401 Unauthorized` = tus credenciales son incorrectas
> - `403 Forbidden` = tus credenciales son correctas pero no tienes permiso (ej: cliente intentando acceder a endpoint de admin)

### Commit

```powershell
git add .
git commit -m "feat: AuthController con registro y login, DTOs de autenticación"
```

---

## Pomodoros 7-8 — ProductosController (50 min)

### Archivo: `Controllers/ProductosController.cs`

```csharp
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IUnidadDeTrabajo _uow;

    public ProductosController(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    // === ENDPOINTS PÚBLICOS ===

    /// <summary>
    /// Obtener todos los productos activos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var productos = await _uow.Productos.ObtenerActivosAsync();
        var dtos = productos.Select(MapearADto);
        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
    }

    /// <summary>
    /// Obtener un producto por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);

        if (producto == null)
            return NotFound(Resultado<ProductoDto>.Error("Producto no encontrado"));

        return Ok(Resultado<ProductoDto>.Ok(MapearADto(producto)));
    }

    /// <summary>
    /// Buscar productos por nombre
    /// </summary>
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return BadRequest(Resultado<string>.Error("El término de búsqueda es obligatorio"));

        var productos = await _uow.Productos.BuscarPorNombreAsync(termino);
        var dtos = productos.Select(MapearADto);
        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
    }

    /// <summary>
    /// Obtener productos por categoría
    /// </summary>
    [HttpGet("categoria/{categoriaId}")]
    public async Task<IActionResult> ObtenerPorCategoria(int categoriaId)
    {
        var productos = await _uow.Productos.ObtenerPorCategoriaAsync(categoriaId);
        var dtos = productos.Select(MapearADto);
        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
    }

    // === ENDPOINTS DE ADMIN (requieren JWT con rol Admin) ===

    /// <summary>
    /// Crear un producto nuevo (solo admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        // Validaciones
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            errores.Add("El nombre es obligatorio");
        if (dto.Precio <= 0)
            errores.Add("El precio debe ser mayor a cero");
        if (dto.Stock < 0)
            errores.Add("El stock no puede ser negativo");
        if (dto.CategoriaId <= 0)
            errores.Add("Debe especificar una categoría");

        if (errores.Count > 0)
            return BadRequest(Resultado<ProductoDto>.ErrorValidacion(errores));

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            ImagenUrl = dto.ImagenUrl,
            CategoriaId = dto.CategoriaId
        };

        await _uow.Productos.AgregarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = producto.Id },
            Resultado<ProductoDto>.Ok(MapearADto(producto), "Producto creado"));
    }

    /// <summary>
    /// Actualizar un producto existente (solo admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CrearProductoDto dto)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);

        if (producto == null)
            return NotFound(Resultado<ProductoDto>.Error("Producto no encontrado"));

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        producto.ImagenUrl = dto.ImagenUrl;
        producto.CategoriaId = dto.CategoriaId;

        _uow.Productos.Actualizar(producto);
        await _uow.GuardarCambiosAsync();

        return Ok(Resultado<ProductoDto>.Ok(MapearADto(producto), "Producto actualizado"));
    }

    /// <summary>
    /// Eliminar un producto (solo admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);

        if (producto == null)
            return NotFound(Resultado<bool>.Error("Producto no encontrado"));

        _uow.Productos.Eliminar(producto);
        await _uow.GuardarCambiosAsync();

        return Ok(Resultado<bool>.Ok(true, "Producto eliminado"));
    }

    // --- Mapeo privado ---

    private static ProductoDto MapearADto(Producto p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Precio = p.Precio,
        Stock = p.Stock,
        ImagenUrl = p.ImagenUrl,
        CategoriaNombre = p.Categoria?.Nombre ?? "Sin categoría",
        Disponible = p.Activo && p.Stock > 0
    };
}
```

> **Concepto Senior: Códigos HTTP que debes conocer**
>
> | Código | Cuándo usarlo | Método en Core |
> |--------|--------------|----------------|
> | 200 OK | Petición exitosa | `Ok(datos)` |
> | 201 Created | Recurso creado | `CreatedAtAction(...)` |
> | 400 Bad Request | Datos inválidos | `BadRequest(error)` |
> | 401 Unauthorized | Sin token o token inválido | `Unauthorized()` |
> | 403 Forbidden | Token válido pero sin permisos | `Forbid()` |
> | 404 Not Found | Recurso no existe | `NotFound(error)` |
> | 500 Internal Error | Error del servidor | Lo atrapa el middleware |

### Commit

```powershell
git add .
git commit -m "feat: ProductosController con CRUD completo y autorización por roles"
```

---

## Pomodoro 9 — CategoriasController (25 min)

### Archivo: `Controllers/CategoriasController.cs`

```csharp
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly IUnidadDeTrabajo _uow;

    public CategoriasController(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    /// <summary>Listar todas las categorías activas</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        // Por ahora usamos ObtenerTodosAsync genérico
        // En Día 3 agregaremos un repositorio específico de categorías
        var categorias = await _uow.Productos.ObtenerTodosAsync();
        // TODO: implementar repositorio de categorías
        return Ok(Resultado<string>.Ok("Endpoint pendiente de implementar con EF Core"));
    }

    /// <summary>Crear categoría (solo admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearCategoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(Resultado<string>.Error("El nombre es obligatorio"));

        // TODO: implementar con repositorio real en Día 3
        return Ok(Resultado<string>.Ok("Categoría creada (pendiente EF Core)"));
    }
}

// DTO temporal (mover a Core/DTOs/ después)
public class CrearCategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
```

---

## Pomodoros 10-11 — CarritoController (50 min)

### Archivo: `Controllers/CarritoController.cs`

```csharp
using System.Security.Claims;
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

/// <summary>
/// Controlador del carrito de compras.
/// TODOS los endpoints requieren autenticación.
/// La lógica está en CarritoServicio — el controlador solo traduce HTTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]  // todos los endpoints requieren JWT
public class CarritoController : ControllerBase
{
    private readonly ICarritoServicio _carritoServicio;

    public CarritoController(ICarritoServicio carritoServicio)
    {
        _carritoServicio = carritoServicio;
    }

    /// <summary>
    /// Obtener el ID del usuario desde el token JWT.
    /// El claim "sub" o "nameid" contiene el ID.
    /// </summary>
    private int ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new UnauthorizedAccessException("Token inválido");

        return id;
    }

    /// <summary>Ver mi carrito actual</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCarrito()
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.ObtenerCarritoAsync(usuarioId);
        return Ok(resultado);
    }

    /// <summary>Agregar un producto al carrito</summary>
    [HttpPost("agregar")]
    public async Task<IActionResult> Agregar([FromBody] AgregarAlCarritoDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.AgregarProductoAsync(usuarioId, dto);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>Actualizar la cantidad de un producto en el carrito</summary>
    [HttpPut("{productoId}")]
    public async Task<IActionResult> ActualizarCantidad(
        int productoId, [FromBody] ActualizarCantidadDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.ActualizarCantidadAsync(
            usuarioId, productoId, dto.Cantidad);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>Quitar un producto del carrito</summary>
    [HttpDelete("{productoId}")]
    public async Task<IActionResult> EliminarProducto(int productoId)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.EliminarProductoAsync(usuarioId, productoId);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>Vaciar el carrito completamente</summary>
    [HttpDelete]
    public async Task<IActionResult> Vaciar()
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.VaciarCarritoAsync(usuarioId);
        return Ok(resultado);
    }

    /// <summary>
    /// Checkout — procesar la compra.
    /// Crea la orden, reduce stock y vacía el carrito.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CrearOrdenDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.CheckoutAsync(usuarioId, dto);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }
}

// DTO auxiliar
public class ActualizarCantidadDto
{
    public int Cantidad { get; set; }
}
```

> **Concepto Senior: Separación controlador vs servicio**
> El controlador NO tiene lógica de negocio. Solo hace tres cosas:
> 1. Extraer datos del HTTP (body, query, headers, token)
> 2. Llamar al servicio correspondiente
> 3. Traducir el resultado a un código HTTP (200, 400, 404)
>
> Si ves un controlador con más de 15 líneas por método, algo está mal.

### Commit

```powershell
git add .
git commit -m "feat: CarritoController con agregar, quitar, actualizar y checkout"
```

---

## Pomodoro 12 — OrdenesController (25 min)

### Archivo: `Controllers/OrdenesController.cs`

```csharp
using System.Security.Claims;
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IUnidadDeTrabajo _uow;

    public OrdenesController(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    private int ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    /// <summary>Listar mis órdenes</summary>
    [HttpGet]
    public async Task<IActionResult> MisOrdenes()
    {
        var usuarioId = ObtenerUsuarioId();
        var ordenes = await _uow.Ordenes.ObtenerPorUsuarioAsync(usuarioId);

        var dtos = ordenes.Select(o => new OrdenDto
        {
            Id = o.Id,
            NumeroOrden = o.NumeroOrden,
            FechaCreacion = o.FechaCreacion,
            Estado = o.Estado.ToString(),
            Total = o.Total
        });

        return Ok(Resultado<IEnumerable<OrdenDto>>.Ok(dtos));
    }

    /// <summary>Ver detalle de una orden</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(id);

        if (orden == null)
            return NotFound(Resultado<OrdenDto>.Error("Orden no encontrada"));

        // Verificar que la orden pertenece al usuario
        if (orden.UsuarioId != ObtenerUsuarioId())
            return Forbid();

        var dto = new OrdenDto
        {
            Id = orden.Id,
            NumeroOrden = orden.NumeroOrden,
            FechaCreacion = orden.FechaCreacion,
            Estado = orden.Estado.ToString(),
            Total = orden.Total,
            Detalles = orden.Detalles.Select(d => new OrdenDetalleDto
            {
                ProductoNombre = d.Producto?.Nombre ?? "",
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };

        return Ok(Resultado<OrdenDto>.Ok(dto));
    }

    /// <summary>Cancelar una orden</summary>
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(id);

        if (orden == null)
            return NotFound(Resultado<bool>.Error("Orden no encontrada"));

        if (orden.UsuarioId != ObtenerUsuarioId())
            return Forbid();

        try
        {
            orden.Cancelar();  // la lógica está en la entidad
            _uow.Ordenes.Actualizar(orden);
            await _uow.GuardarCambiosAsync();

            return Ok(Resultado<bool>.Ok(true, "Orden cancelada. Stock restaurado."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(Resultado<bool>.Error(ex.Message));
        }
    }
}
```

---

## Pomodoro 13 — Middleware de errores globales (25 min)

### Archivo: `Middleware/ManejadorErroresMiddleware.cs`

```csharp
using System.Net;
using System.Text.Json;
using EcommerceNet.Core.DTOs;

namespace EcommerceNet.API.Middleware;

/// <summary>
/// Middleware que atrapa CUALQUIER excepción no manejada
/// y la convierte en una respuesta JSON estandarizada.
/// Sin esto, una excepción devuelve HTML de error del servidor.
/// </summary>
public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorErroresMiddleware> _logger;

    public ManejadorErroresMiddleware(RequestDelegate next, ILogger<ManejadorErroresMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            // Dejar que la petición pase al siguiente middleware/controlador
            await _next(contexto);
        }
        catch (Exception ex)
        {
            // Si algo falla, atrapar aquí
            _logger.LogError(ex, "Error no manejado: {Mensaje}", ex.Message);
            await ManejarExcepcionAsync(contexto, ex);
        }
    }

    private static async Task ManejarExcepcionAsync(HttpContext contexto, Exception ex)
    {
        contexto.Response.ContentType = "application/json";

        var (codigo, mensaje) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado"),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor")
        };

        contexto.Response.StatusCode = (int)codigo;

        var resultado = Resultado<string>.Error(mensaje);
        var json = JsonSerializer.Serialize(resultado, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await contexto.Response.WriteAsync(json);
    }
}
```

### Registrar en Program.cs (antes de UseHttpsRedirection)

```csharp
// Agregar al inicio del pipeline — ANTES de todo lo demás
app.UseMiddleware<ManejadorErroresMiddleware>();
```

> **¿Por qué usar un middleware y no try/catch en cada controlador?**
> Porque DRY (Don't Repeat Yourself). Sin middleware, necesitarías un try/catch
> en CADA método de CADA controlador. Con el middleware, lo escribes una vez y
> atrapa errores de toda la aplicación, incluyendo errores en otros middlewares.

### Commit

```powershell
git add .
git commit -m "feat: middleware global de manejo de errores"
```

---

## Pomodoro 14 — Swagger con JWT (25 min)

### Configurar Swagger en Program.cs

Reemplaza `builder.Services.AddSwaggerGen()` por:

```csharp
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EcommerceNet API",
        Version = "v1",
        Description = "API REST para tienda en línea — Proyecto DaCodes"
    });

    // Agregar botón "Authorize" en Swagger para JWT
    opciones.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa tu token JWT. Ejemplo: eyJhbGciOi..."
    });

    opciones.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

> Ahora al abrir `https://localhost:5001/swagger` verás todos tus endpoints
> documentados y un botón "Authorize" donde pegas el JWT para probar endpoints protegidos.

---

## Pomodoro 15 — Archivo requests.http (25 min)

### Archivo: `requests.http` (en la raíz de EcommerceNet.API)

Instala la extensión **REST Client** en VS Code para usar estos archivos.

```http
### ============================================
### AUTH
### ============================================

### Registrar usuario
POST http://localhost:5000/api/auth/registrar
Content-Type: application/json

{
  "nombre": "Ramiro Dev",
  "email": "ramiro@dacodes.com",
  "password": "MiPassword123!"
}

### Registrar admin
POST http://localhost:5000/api/auth/registrar
Content-Type: application/json

{
  "nombre": "Admin Tienda",
  "email": "admin@ecommerce.com",
  "password": "Admin123!"
}

### Login (copiar el token de la respuesta)
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "ramiro@dacodes.com",
  "password": "MiPassword123!"
}

### ============================================
### PRODUCTOS (públicos)
### ============================================

### Listar todos los productos
GET http://localhost:5000/api/productos

### Obtener producto por ID
GET http://localhost:5000/api/productos/1

### Buscar productos
GET http://localhost:5000/api/productos/buscar?termino=laptop

### Productos por categoría
GET http://localhost:5000/api/productos/categoria/1

### ============================================
### PRODUCTOS (admin — requiere JWT)
### ============================================

@token = PEGAR_TOKEN_AQUI

### Crear producto
POST http://localhost:5000/api/productos
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "nombre": "Laptop Gaming Pro",
  "descripcion": "Laptop para desarrollo y gaming, 16GB RAM",
  "precio": 25999.99,
  "stock": 15,
  "imagenUrl": "https://ejemplo.com/laptop.jpg",
  "categoriaId": 1
}

### Actualizar producto
PUT http://localhost:5000/api/productos/1
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "nombre": "Laptop Gaming Pro (Actualizada)",
  "descripcion": "Ahora con 32GB RAM",
  "precio": 29999.99,
  "stock": 10,
  "imagenUrl": "https://ejemplo.com/laptop-v2.jpg",
  "categoriaId": 1
}

### Eliminar producto
DELETE http://localhost:5000/api/productos/1
Authorization: Bearer {{token}}

### ============================================
### CARRITO (requiere JWT)
### ============================================

### Ver mi carrito
GET http://localhost:5000/api/carrito
Authorization: Bearer {{token}}

### Agregar producto al carrito
POST http://localhost:5000/api/carrito/agregar
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "productoId": 1,
  "cantidad": 2
}

### Agregar otro producto
POST http://localhost:5000/api/carrito/agregar
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "productoId": 2,
  "cantidad": 1
}

### Actualizar cantidad
PUT http://localhost:5000/api/carrito/1
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "cantidad": 5
}

### Quitar producto del carrito
DELETE http://localhost:5000/api/carrito/2
Authorization: Bearer {{token}}

### Vaciar carrito
DELETE http://localhost:5000/api/carrito
Authorization: Bearer {{token}}

### ============================================
### CHECKOUT
### ============================================

### Procesar compra
POST http://localhost:5000/api/carrito/checkout
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "direccionEnvio": "Calle 50 #200, Col. Centro, Merida, Yucatan"
}

### ============================================
### ÓRDENES (requiere JWT)
### ============================================

### Mis órdenes
GET http://localhost:5000/api/ordenes
Authorization: Bearer {{token}}

### Detalle de una orden
GET http://localhost:5000/api/ordenes/1
Authorization: Bearer {{token}}

### Cancelar orden
PUT http://localhost:5000/api/ordenes/1/cancelar
Authorization: Bearer {{token}}
```

---

## Pomodoro 16 — Build, test y merge (25 min)

```powershell
# Verificar que compila
dotnet build

# Ejecutar pruebas del Día 1 (deben seguir pasando)
dotnet test

# Commit final
git add .
git commit -m "feat: API REST completa - 5 controladores, JWT, Swagger, middleware de errores"

# Merge a desarrollo
git checkout desarrollo
git merge dia-02/aspnet-api
git push origin desarrollo

# Merge a main
git checkout main
git merge desarrollo
git push origin main
```

---

## Módulo Git del día

### Comandos usados

```powershell
git checkout -b dia-02/aspnet-api     # crear rama del día
git add .                              # agregar cambios
git commit -m "feat: ..."             # commit con prefijo
git checkout desarrollo                # cambiar a desarrollo
git merge dia-02/aspnet-api            # integrar la rama
git push origin desarrollo             # subir a GitHub
```

### Concepto avanzado: `git stash`

Si estás trabajando en algo y necesitas cambiar de rama urgentemente:

```powershell
git stash                  # guarda tus cambios temporalmente
git checkout otra-rama     # cambias de rama limpio
# ... haces lo que necesitas ...
git checkout dia-02/aspnet-api
git stash pop              # recuperas tus cambios
```

---

## Resumen del día

### Lo que construiste

| Archivo | Tecnología de la vacante |
|---------|-------------------------|
| `Program.cs` con DI, CORS, JWT, Swagger | ASP.NET Core |
| `AuthController` — registro y login | APIs REST, JWT |
| `ProductosController` — CRUD con roles | ASP.NET Core, [Authorize] |
| `CategoriasController` — estructura | ASP.NET Core |
| `CarritoController` — 6 endpoints | APIs REST |
| `OrdenesController` — listar, detalle, cancelar | APIs REST |
| `ManejadorErroresMiddleware` | .NET Core middleware |
| `requests.http` — pruebas de API | REST Client |
| Sección teórica MVC vs Core | ASP.NET MVC (.NET Framework) |

### Tabla de endpoints de la API

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/registrar` | No | Crear cuenta |
| POST | `/api/auth/login` | No | Obtener JWT |
| GET | `/api/productos` | No | Listar productos |
| GET | `/api/productos/{id}` | No | Detalle |
| GET | `/api/productos/buscar?termino=x` | No | Buscar |
| GET | `/api/productos/categoria/{id}` | No | Por categoría |
| POST | `/api/productos` | Admin | Crear |
| PUT | `/api/productos/{id}` | Admin | Actualizar |
| DELETE | `/api/productos/{id}` | Admin | Eliminar |
| GET | `/api/carrito` | Sí | Ver carrito |
| POST | `/api/carrito/agregar` | Sí | Agregar producto |
| PUT | `/api/carrito/{productoId}` | Sí | Actualizar cantidad |
| DELETE | `/api/carrito/{productoId}` | Sí | Quitar producto |
| DELETE | `/api/carrito` | Sí | Vaciar |
| POST | `/api/carrito/checkout` | Sí | Comprar |
| GET | `/api/ordenes` | Sí | Mis órdenes |
| GET | `/api/ordenes/{id}` | Sí | Detalle |
| PUT | `/api/ordenes/{id}/cancelar` | Sí | Cancelar |

---

## Simulador de entrevista DaCodes — Día 2

**Pregunta 1:** "¿Cuál es la diferencia entre ASP.NET MVC y ASP.NET Core?"
> "ASP.NET MVC corre sobre .NET Framework, solo en Windows, y genera HTML en el servidor usando vistas Razor con jQuery para interactividad. ASP.NET Core es multiplataforma, mucho más rápido, modular, y tiene inyección de dependencias nativa. En proyectos modernos lo uso como API que devuelve JSON, con el frontend en Vue.js como SPA separada. Puedo trabajar con ambos — mantener código legacy MVC y construir nuevo con Core."

**Pregunta 2:** "¿Cómo implementaste la autenticación en tu proyecto?"
> "Uso JWT Bearer tokens. El usuario hace POST a `/api/auth/login` con email y contraseña. El backend valida las credenciales, genera un token que incluye el ID del usuario y su rol como claims, y lo devuelve. El frontend guarda el token y lo envía en cada petición en el header `Authorization: Bearer {token}`. Los endpoints protegidos usan `[Authorize]` para verificar el token, y `[Authorize(Roles = "Admin")]` para verificar roles específicos."

**Pregunta 3:** "¿Qué pasa si un middleware está en el orden incorrecto en el pipeline?"
> "El pipeline es secuencial — cada middleware se ejecuta en el orden que lo registras. Si pongo `UseAuthorization()` antes de `UseAuthentication()`, la autorización intenta verificar roles sin haber leído el token primero, y todos los endpoints protegidos fallan con 401. El orden correcto es: manejo de errores → CORS → autenticación → autorización → controladores."

---

## Mañana: Día 3

Conectaremos la API a SQL Server con Entity Framework Core: DbContext, migraciones,
repositorios reales, seed data y queries SQL avanzados. También agregaremos MongoDB
para el historial de búsquedas.

Rama: `dia-03/datos`
