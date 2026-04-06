# Clase de Programación — Día 2: ASP.NET Core Web API desde cero

> **A quién va dirigido:** desarrollador que ya entendió los conceptos del Día 1 (C#, clases,
> interfaces, genéricos, LINQ, async/await). Este documento explica ÚNICAMENTE los conceptos
> nuevos que aparecieron en el Día 2. Cada concepto se explica con el código real del proyecto.

---

## Índice

1. [La diferencia entre ASP.NET MVC y ASP.NET Core](#1-la-diferencia-entre-aspnet-mvc-y-aspnet-core)
2. [Controladores y ControllerBase](#2-controladores-y-controllerbase)
3. [Atributos HTTP: ApiController, Route, HttpGet, HttpPost...](#3-atributos-http-apicontroller-route-httpget-httppost)
4. [Model Binding: FromBody y FromQuery](#4-model-binding-frombody-y-fromquery)
5. [IActionResult y códigos HTTP](#5-iactionresult-y-códigos-http)
6. [El pipeline de middleware en ASP.NET Core](#6-el-pipeline-de-middleware-en-aspnet-core)
7. [Inyección de dependencias en Program.cs](#7-inyección-de-dependencias-en-programcs)
8. [Autenticación JWT: qué es y cómo funciona](#8-autenticación-jwt-qué-es-y-cómo-funciona)
9. [Claims y ClaimTypes](#9-claims-y-claimtypes)
10. [Autorización con roles: Authorize y AllowAnonymous](#10-autorización-con-roles-authorize-y-allowanonymous)
11. [CORS: por qué existe y cómo se configura](#11-cors-por-qué-existe-y-cómo-se-configura)
12. [Swagger y OpenAPI: documentación interactiva](#12-swagger-y-openapi-documentación-interactiva)
13. [Hashing de contraseñas con BCrypt](#13-hashing-de-contraseñas-con-bcrypt)
14. [Análisis línea a línea: Program.cs](#14-análisis-línea-a-línea-programcs)
15. [Análisis línea a línea: AuthController.cs](#15-análisis-línea-a-línea-authcontrollercs)
16. [Análisis línea a línea: ProductosController.cs](#16-análisis-línea-a-línea-productoscontrollercs)
17. [Análisis línea a línea: CarritoController.cs](#17-análisis-línea-a-línea-carritocontrollercs)
18. [Análisis línea a línea: ManejadorErroresMiddleware.cs](#18-análisis-línea-a-línea-manejadorerresmiddlewarecs)
19. [Patrón: separación entre controlador y servicio](#19-patrón-separación-entre-controlador-y-servicio)
20. [Glosario de atributos y palabras nuevas del Día 2](#20-glosario-de-atributos-y-palabras-nuevas-del-día-2)

---

## 1. La diferencia entre ASP.NET MVC y ASP.NET Core

Antes de escribir código, debes entender qué problema resuelve cada uno. La vacante pide experiencia en ambos.

### ¿Qué es ASP.NET MVC (el legacy)?

Es el framework web de Microsoft creado en 2009. Corre solo en Windows. Genera páginas HTML en el servidor usando **vistas Razor** (archivos `.cshtml`). El controlador recibe la petición y devuelve HTML.

```csharp
// ASP.NET MVC (.NET Framework 4.x) — el controlador devuelve HTML
public class ProductosController : Controller  // hereda de Controller (con vistas)
{
    public ActionResult Index()
    {
        var productos = db.Productos.ToList();
        return View(productos);  // busca Views/Productos/Index.cshtml y renderiza HTML
    }
}
```

```html
<!-- Views/Productos/Index.cshtml — HTML generado en el servidor -->
@model IEnumerable<Producto>
<table>
    @foreach (var p in Model)
    {
        <tr><td>@p.Nombre</td></tr>
        <!-- jQuery vive AQUÍ dentro de la vista HTML -->
    }
</table>
<script src="~/Scripts/jquery.min.js"></script>
```

### ¿Qué es ASP.NET Core (el moderno)?

Reescritura completa de 2016. Multiplataforma (Windows, Linux, Mac). Mucho más rápido. El controlador devuelve **JSON**, no HTML. El frontend (Vue.js) vive en un proyecto separado.

```csharp
// ASP.NET Core — el controlador devuelve JSON
public class ProductosController : ControllerBase  // ControllerBase (sin vistas)
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var productos = await _uow.Productos.ObtenerActivosAsync();
        return Ok(productos);  // devuelve JSON, no HTML
    }
}
```

### Tabla comparativa (memoriza esto para la entrevista)

| Concepto | MVC Legacy (.NET Framework) | ASP.NET Core |
|----------|-----------------------------|--------------|
| Plataforma | Solo Windows | Windows, Linux, macOS |
| Punto de entrada | `Global.asax` + `Startup.cs` | `Program.cs` (unificado) |
| Configuración | `Web.config` (XML) | `appsettings.json` (JSON) |
| Servidor | IIS obligatorio | Kestrel (propio) + cualquier reverse proxy |
| Inyección de dependencias | Manual o con Ninject/Unity | Nativa |
| Respuesta típica | Página HTML | JSON |
| Frontend | jQuery en vistas Razor | Vue.js / React / Angular (SPA separada) |
| Rendimiento | ~50k peticiones/segundo | ~7M peticiones/segundo |
| Estado | En mantenimiento | Activo (nueva versión cada año) |

---

## 2. Controladores y ControllerBase

### ¿Qué es un controlador?

Un controlador es una clase que recibe peticiones HTTP, las procesa y devuelve una respuesta. En nuestro proyecto, hay 5 controladores: `AuthController`, `ProductosController`, `CategoriasController`, `CarritoController` y `OrdenesController`.

### `ControllerBase` vs `Controller`

```csharp
// Para APIs que devuelven JSON — usa ControllerBase
public class ProductosController : ControllerBase { }

// Para apps web que devuelven HTML (vistas Razor) — usa Controller
public class ProductosController : Controller { }
```

`ControllerBase` es la clase base para APIs. Tiene todos los métodos para construir respuestas HTTP (`Ok()`, `NotFound()`, `BadRequest()`, etc.) pero **no** tiene soporte para vistas Razor.

`Controller` hereda de `ControllerBase` y agrega soporte para vistas. Si no usas vistas, `ControllerBase` es la opción correcta y más liviana.

### La propiedad `User`

Dentro de cualquier controlador que hereda de `ControllerBase`, tienes acceso a `User` (tipo `ClaimsPrincipal`). Esta propiedad contiene los datos del usuario autenticado extraídos del token JWT:

```csharp
// Dentro de un método del controlador
var nombre = User.FindFirst(ClaimTypes.Name)?.Value;  // "Ramiro"
var rol    = User.FindFirst(ClaimTypes.Role)?.Value;  // "Admin" o "Cliente"
var id     = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // "5"
```

---

## 3. Atributos HTTP: ApiController, Route, HttpGet, HttpPost...

### ¿Qué son los atributos?

Ya vimos atributos en el Día 1 (`[Fact]` en xUnit). En ASP.NET Core, los atributos configuran cómo se mapean las peticiones HTTP a los métodos del controlador.

### `[ApiController]`

```csharp
[ApiController]
public class ProductosController : ControllerBase { }
```

Este atributo activa comportamientos automáticos:
1. **Validación automática del modelo**: si el body del request no se puede convertir al tipo esperado, devuelve 400 automáticamente sin que escribas código de validación.
2. **Inferencia de origen de los parámetros**: deduce automáticamente si un parámetro viene del body, de la URL o del query string.
3. **Respuestas de error estandarizadas**: los errores de validación usan `ProblemDetails` (formato estándar).

### `[Route("api/[controller]")]`

```csharp
[Route("api/[controller]")]
public class ProductosController : ControllerBase { }
// Ruta base: /api/productos
// [controller] se reemplaza con el nombre de la clase sin "Controller"
```

`[controller]` es un placeholder que ASP.NET Core reemplaza con el nombre de la clase sin el sufijo "Controller". `ProductosController` → `productos`. La ruta base queda `/api/productos`.

```csharp
[Route("api/[controller]")]
public class AuthController : ControllerBase { }
// Ruta base: /api/auth
```

### `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`

Estos atributos indican el **verbo HTTP** que activa cada método:

```csharp
[HttpGet]               // GET /api/productos
public async Task<IActionResult> ObtenerTodos() { ... }

[HttpGet("{id}")]       // GET /api/productos/5
public async Task<IActionResult> ObtenerPorId(int id) { ... }

[HttpGet("buscar")]     // GET /api/productos/buscar
public async Task<IActionResult> Buscar([FromQuery] string termino) { ... }

[HttpGet("categoria/{categoriaId}")]  // GET /api/productos/categoria/3
public async Task<IActionResult> ObtenerPorCategoria(int categoriaId) { ... }

[HttpPost]              // POST /api/productos
public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto) { ... }

[HttpPut("{id}")]       // PUT /api/productos/5
public async Task<IActionResult> Actualizar(int id, ...) { ... }

[HttpDelete("{id}")]    // DELETE /api/productos/5
public async Task<IActionResult> Eliminar(int id) { ... }
```

Los parámetros entre llaves `{id}` se llaman **parámetros de ruta**. ASP.NET Core los extrae de la URL y los inyecta en el método automáticamente.

### Rutas con segmentos adicionales

```csharp
[HttpPost("registrar")]  // POST /api/auth/registrar
[HttpPost("login")]      // POST /api/auth/login

[HttpPost("agregar")]    // POST /api/carrito/agregar
[HttpPost("checkout")]   // POST /api/carrito/checkout

[HttpPut("{id}/cancelar")]  // PUT /api/ordenes/5/cancelar
```

---

## 4. Model Binding: FromBody y FromQuery

### El problema

Una petición HTTP puede traer datos en distintos lugares:
- En la **URL**: `/api/productos/5` (parámetro de ruta)
- En el **query string**: `/api/productos/buscar?termino=laptop` (parámetro de consulta)
- En el **body** de la petición: el JSON que envías en un POST (body)
- En los **headers**: `Authorization: Bearer eyJ...` (header)

ASP.NET Core necesita saber de dónde vienen los datos para cada parámetro del método.

### `[FromBody]` — datos del body JSON

```csharp
// El cliente envía esto en el body:
// { "nombre": "Laptop", "precio": 25999.99, "stock": 10, "categoriaId": 1 }

[HttpPost]
public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
{
    // dto.Nombre = "Laptop"
    // dto.Precio = 25999.99
    // ASP.NET Core deserializó el JSON automáticamente
}
```

`[FromBody]` le dice a ASP.NET Core: "lee el JSON del body de la petición y convierte ese JSON a este tipo de C#". Internamente usa `System.Text.Json` para la deserialización.

### `[FromQuery]` — datos del query string

```csharp
// El cliente envía: GET /api/productos/buscar?termino=laptop

[HttpGet("buscar")]
public async Task<IActionResult> Buscar([FromQuery] string termino)
{
    // termino = "laptop"
    // ASP.NET Core extrajo "laptop" del query string
}
```

`[FromQuery]` extrae el valor del query string de la URL. El nombre del parámetro del método (`termino`) debe coincidir con el nombre en la URL (`?termino=...`).

### Parámetros de ruta (sin atributo)

```csharp
// GET /api/productos/5
[HttpGet("{id}")]
public async Task<IActionResult> ObtenerPorId(int id)
{
    // id = 5
    // ASP.NET Core lo extrajo de la URL automáticamente
}
```

Los parámetros de ruta no necesitan atributo. Si el nombre del parámetro coincide con el placeholder en la ruta (`{id}` → `int id`), ASP.NET Core lo inyecta solo.

---

## 5. IActionResult y códigos HTTP

### ¿Qué es IActionResult?

`IActionResult` es la interfaz que representa una respuesta HTTP. Todos los métodos de los controladores retornan `IActionResult` (o `Task<IActionResult>` en versiones async).

```csharp
public async Task<IActionResult> ObtenerPorId(int id)
{
    // El tipo de retorno es IActionResult — puede ser Ok, NotFound, etc.
}
```

### Los métodos de respuesta (heredados de ControllerBase)

Estos métodos crean instancias de `IActionResult` con el código HTTP correcto:

```csharp
// 200 OK — petición exitosa
return Ok(datos);
return Ok(Resultado<ProductoDto>.Ok(dto));

// 201 Created — recurso creado (agrega header Location)
return CreatedAtAction(
    nameof(ObtenerPorId),    // acción que obtiene el recurso
    new { id = producto.Id },// parámetros de ruta
    dto                      // cuerpo de la respuesta
);

// 400 Bad Request — datos inválidos
return BadRequest(Resultado<string>.Error("El nombre es obligatorio"));

// 401 Unauthorized — no autenticado o credenciales inválidas
return Unauthorized(resultado);

// 403 Forbidden — autenticado pero sin permisos
return Forbid();

// 404 Not Found — recurso no existe
return NotFound(Resultado<ProductoDto>.Error("Producto no encontrado"));
```

### Tabla completa de códigos HTTP

| Código | Nombre | Cuándo usarlo en nuestra API | Método en ControllerBase |
|--------|--------|------------------------------|--------------------------|
| 200 | OK | Petición exitosa | `Ok(datos)` |
| 201 | Created | Nuevo recurso creado | `CreatedAtAction(...)` |
| 400 | Bad Request | Datos inválidos o falta información | `BadRequest(error)` |
| 401 | Unauthorized | Sin token o credenciales incorrectas | `Unauthorized(error)` |
| 403 | Forbidden | Token válido pero sin permiso | `Forbid()` |
| 404 | Not Found | El recurso no existe | `NotFound(error)` |
| 500 | Internal Server Error | Error inesperado del servidor | Lo maneja el middleware |

### ¿Por qué 401 para login fallido y no 400?

```csharp
// Login con credenciales incorrectas
if (!resultado.Exito)
    return Unauthorized(resultado);  // 401, no 400
```

- `400 Bad Request` significa que el formato de los datos está mal (falta el campo email, el JSON está malformado, etc.).
- `401 Unauthorized` significa que los datos tienen buen formato pero la autenticación falló.

La semántica importa: el cliente que recibe 401 sabe que debe pedir nuevas credenciales al usuario.

### `CreatedAtAction` — el más complejo

```csharp
return CreatedAtAction(
    nameof(ObtenerPorId),      // nombre del action (string)
    new { id = producto.Id },  // valores de los parámetros de ruta de ese action
    Resultado<ProductoDto>.Ok(MapearADto(producto))  // body de la respuesta
);
```

Esto hace tres cosas:
1. Devuelve el código HTTP **201 Created**
2. Agrega el header `Location: /api/productos/5` (la URL donde se puede obtener el recurso creado)
3. Incluye el DTO del producto creado en el body

`nameof(ObtenerPorId)` es una expresión de C# que devuelve el nombre del método como string: `"ObtenerPorId"`. Usar `nameof` en lugar de un string literal previene errores si renombras el método.

---

## 6. El pipeline de middleware en ASP.NET Core

### ¿Qué es el middleware?

Imagina que cada petición HTTP que llega a tu API es como una pelota que pasa por una serie de cajas antes de llegar al controlador. Cada caja es un middleware. Puede:
- Dejar pasar la pelota al siguiente (normal)
- Modificar la pelota (agregar headers, leer el token)
- Detener la pelota y devolver una respuesta (error, redirección)

```
Petición HTTP entrante
    │
    ▼
┌──────────────────────────────────┐
│  1. ManejadorErroresMiddleware   │  ← envuelve todo en try/catch
├──────────────────────────────────┤
│  2. Swagger (solo desarrollo)    │  ← sirve la UI de Swagger
├──────────────────────────────────┤
│  3. HttpsRedirection             │  ← redirige HTTP → HTTPS
├──────────────────────────────────┤
│  4. CORS                         │  ← permite o bloquea origins
├──────────────────────────────────┤
│  5. Authentication               │  ← lee y valida el JWT
├──────────────────────────────────┤
│  6. Authorization                │  ← verifica [Authorize] y roles
├──────────────────────────────────┤
│  7. MapControllers               │  ← enruta al controlador correcto
├──────────────────────────────────┤
│  Tu método del controlador       │  ← tu código
└──────────────────────────────────┘
    │
    ▼
Respuesta HTTP al cliente
```

### El orden importa — errores clásicos

**Error 1:** `UseAuthorization` antes de `UseAuthentication`
```csharp
// INCORRECTO — todos los [Authorize] fallan con 401
app.UseAuthorization();   // ejecuta ANTES — User está vacío
app.UseAuthentication();  // ejecuta DESPUÉS — ya es tarde
```

`UseAuthentication` lee el token JWT y puebla `HttpContext.User`. Si `UseAuthorization` se ejecuta primero, encuentra `User` vacío y rechaza todas las peticiones.

**Error 2:** El manejador de errores no está al inicio
```csharp
// INCORRECTO — errores en CORS o Authentication escapan sin convertirse a JSON
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<ManejadorErroresMiddleware>();  // demasiado tarde
```

### Cómo se escribe un middleware

Un middleware es una clase con un método `InvokeAsync(HttpContext contexto)`:

```csharp
public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;  // el siguiente middleware en la cadena

    public ManejadorErroresMiddleware(RequestDelegate next) { _next = next; }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _next(contexto);  // dejar pasar la petición al siguiente middleware
        }
        catch (Exception ex)
        {
            // interceptar el error y transformarlo en respuesta JSON
        }
    }
}
```

`RequestDelegate` es un delegado que representa el resto del pipeline. Al llamar `await _next(contexto)`, estás diciendo "pasa esta petición al siguiente middleware".

### Cómo se registra un middleware en Program.cs

```csharp
app.UseMiddleware<ManejadorErroresMiddleware>();
// o con extensión propia:
// app.UseManejadorErrores();
```

---

## 7. Inyección de dependencias en Program.cs

### ¿Qué es la inyección de dependencias (DI)?

En el Día 1, `CarritoServicio` recibía `IUnidadDeTrabajo` por constructor. Alguien tiene que crear esa instancia. En ASP.NET Core, ese "alguien" es el **contenedor de DI**, configurado en `Program.cs`.

```csharp
// ANTES (sin DI — anti-patrón)
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _auth = new AuthServicio(...);  // el controlador crea todo
}

// DESPUÉS (con DI — correcto)
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _auth;
    public AuthController(IAuthServicio auth) { _auth = auth; }  // el contenedor inyecta
}
```

### Los 3 ciclos de vida

```csharp
// 1. AddTransient — nueva instancia CADA VEZ que se solicita
builder.Services.AddTransient<IGeneradorPdf, GeneradorPdf>();
// Úsalo para: servicios sin estado que no acceden a BD

// 2. AddScoped — una instancia POR PETICIÓN HTTP
builder.Services.AddScoped<IAuthServicio, AuthServicio>();
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();
// Úsalo para: servicios de negocio, repositorios, DbContext
// Es el más común en aplicaciones web

// 3. AddSingleton — UNA instancia para toda la vida de la app
builder.Services.AddSingleton<IConfiguracion, Configuracion>();
// Úsalo para: configuración, caché en memoria
```

### ¿Por qué el ciclo de vida importa?

Escenario problemático:
```csharp
// AppDbContext (acceso a BD) se registra como Scoped por defecto en EF Core
builder.Services.AddDbContext<AppDbContext>(...);  // Scoped

// Si registras un servicio que usa DbContext como Singleton:
builder.Services.AddSingleton<IProductoRepositorio, ProductoRepositorio>();
// ERROR en tiempo de ejecución:
// "Cannot consume scoped service from singleton"
// Un Singleton no puede usar un Scoped porque el Scoped se destruye cada petición
// mientras el Singleton vive para siempre
```

### Cómo se registra la configuración JWT

```csharp
var jwtKey = builder.Configuration["Jwt:Key"]!;
// builder.Configuration lee appsettings.json
// ["Jwt:Key"] navega la jerarquía JSON: { "Jwt": { "Key": "..." } }
// El ! al final dice "confío en que no es null"

builder.Services.AddAuthentication(opciones =>
{
    // Esquema por defecto para autenticar peticiones
    opciones.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // Esquema por defecto cuando falla la autenticación
    opciones.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opciones =>
{
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           // verificar quién generó el token
        ValidateAudience = true,         // verificar para quién es
        ValidateLifetime = true,         // verificar si expiró
        ValidateIssuerSigningKey = true, // verificar la firma (no fue alterado)
        ValidIssuer = "EcommerceNet.API",
        ValidAudience = "EcommerceNet.Web",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
```

---

## 8. Autenticación JWT: qué es y cómo funciona

### El flujo completo

```
1. Cliente → POST /api/auth/login  { email, password }
2. API valida credenciales con BCrypt
3. API genera token JWT firmado
4. API → Cliente { token: "eyJhbGciOi..." }

Peticiones futuras:
5. Cliente → GET /api/carrito
           Authorization: Bearer eyJhbGciOi...
6. API valida el token (firma, expiración, issuer)
7. API lee los claims del token (quién es, qué rol tiene)
8. API ejecuta el controlador si todo está bien
```

### Anatomía de un token JWT

Un JWT tiene 3 partes separadas por puntos:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
.
eyJzdWIiOiI1IiwiZW1haWwiOiJyYW1pcm9AZGFjb2Rlcy5jb20iLCJyb2xlIjoiQ2xpZW50ZSIsImV4cCI6MTc0MzY5NTYwMH0
.
xK8mN3pQrL2vTjUyHfGdSoWbEcZaIqMnYgRhXwOtPk
```

| Parte | Nombre | Contenido | ¿Está encriptado? |
|-------|--------|-----------|-------------------|
| Primera | Header | Algoritmo usado (HS256) | No — codificado en Base64 |
| Segunda | Payload | Claims (datos del usuario) | No — codificado en Base64 |
| Tercera | Signature | Firma HMAC-SHA256 | Es la firma, no encriptación |

**Importante:** El payload es visible (cualquiera puede decodificarlo). La seguridad viene de la **firma**: si alguien modifica el payload, la firma no coincide y el token se rechaza.

Por esto nunca pongas contraseñas u información muy sensible en el payload.

### Decodificado del payload en nuestro token

```json
{
  "nameid": "5",
  "email": "ramiro@dacodes.com",
  "unique_name": "Ramiro Dev",
  "role": "Cliente",
  "nbf": 1743692000,
  "exp": 1743695600,
  "iss": "EcommerceNet.API",
  "aud": "EcommerceNet.Web"
}
```

Estos son los claims que `AuthServicio.GenerarToken` pone en el token.

---

## 9. Claims y ClaimTypes

### ¿Qué es un claim?

Un **claim** es una afirmación sobre el usuario. Literalmente significa "el portador de este token *afirma* ser el usuario con ID 5, con email ramiro@dacodes.com, con rol Cliente". La API confía en esta afirmación porque verifica la firma del token.

### Cómo se crean (en AuthServicio)

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
    // ↑ tipo del claim: "nameidentifier"   valor: "5"

    new Claim(ClaimTypes.Email, usuario.Email),
    // ↑ tipo del claim: "emailaddress"     valor: "ramiro@dacodes.com"

    new Claim(ClaimTypes.Name, usuario.Nombre),
    // ↑ tipo del claim: "name"             valor: "Ramiro Dev"

    new Claim(ClaimTypes.Role, usuario.Rol.ToString())
    // ↑ tipo del claim: "role"             valor: "Cliente" o "Admin"
};
```

`ClaimTypes` es una clase estática con constantes que representan tipos de claims estándar. El tipo es solo un string que identifica qué significa el valor.

### Cómo se leen (en los controladores)

```csharp
// En cualquier controlador, User es de tipo ClaimsPrincipal
// ClaimsPrincipal contiene los claims del token JWT

private int ObtenerUsuarioId()
{
    var claim = User.FindFirst(ClaimTypes.NameIdentifier)  // busca por tipo
        ?? User.FindFirst("sub");  // "sub" es la alternativa estándar de OAuth2

    // claim.Value es el string "5"
    if (claim == null || !int.TryParse(claim.Value, out var id))
        throw new UnauthorizedAccessException("Token inválido");

    return id;  // 5
}
```

`User.FindFirst(tipo)` devuelve el primer claim con ese tipo, o `null` si no existe.
`int.TryParse(valor, out var id)` convierte el string "5" al entero 5 sin lanzar excepción si falla.

### Tabla de ClaimTypes usados

| ClaimTypes | String real en JWT | Qué contiene en nuestro proyecto |
|------------|-------------------|----------------------------------|
| `ClaimTypes.NameIdentifier` | `"nameidentifier"` / `"sub"` | ID del usuario (int → string) |
| `ClaimTypes.Email` | `"emailaddress"` | Email del usuario |
| `ClaimTypes.Name` | `"name"` | Nombre completo |
| `ClaimTypes.Role` | `"role"` | `"Admin"` o `"Cliente"` |

---

## 10. Autorización con roles: Authorize y AllowAnonymous

### `[Authorize]` — cualquier usuario autenticado

```csharp
[Authorize]
public class CarritoController : ControllerBase
{
    // Todos los métodos de esta clase requieren un token JWT válido
    // Si no hay token → 401 Unauthorized
    // Si el token expiró → 401 Unauthorized
}
```

Aplicado a nivel de clase, `[Authorize]` protege TODOS los métodos. No necesitas repetirlo en cada método.

### `[Authorize(Roles = "Admin")]` — solo administradores

```csharp
[HttpPost]
[Authorize(Roles = "Admin")]  // el token debe tener ClaimTypes.Role == "Admin"
public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
{
    // Si el rol es "Cliente" → 403 Forbidden
    // Si no hay token → 401 Unauthorized
    // Si es "Admin" → entra al método
}
```

ASP.NET Core verifica que el claim `Role` del token sea `"Admin"`. Esto funciona porque en `AuthServicio.GenerarToken` pusimos `new Claim(ClaimTypes.Role, usuario.Rol.ToString())`.

### Diferencia entre 401 y 403

```
Sin token o token inválido → 401 Unauthorized (no autenticado)
Token válido + rol incorrecto → 403 Forbidden (no autorizado)
```

```csharp
// Un cliente intenta crear un producto (solo admin)
// 1. Tiene token → 401 no aplica
// 2. Su rol es "Cliente" → 403 Forbidden

// Un anónimo intenta crear un producto
// 1. No tiene token → 401 Unauthorized
// No llega al paso 2
```

### `[AllowAnonymous]` — excepciones en controladores protegidos

```csharp
[Authorize]  // por defecto, todo requiere autenticación
public class UsuariosController : ControllerBase
{
    [AllowAnonymous]  // excepciones a la regla del controlador
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        // Este método es accesible sin token aunque el controlador tenga [Authorize]
    }
}
```

En nuestro proyecto, `AuthController` no tiene `[Authorize]` porque todos sus endpoints son públicos. `[AllowAnonymous]` se usa cuando tienes un controlador mayormente protegido con uno o dos endpoints públicos.

---

## 11. CORS: por qué existe y cómo se configura

### ¿Qué es CORS?

CORS significa "Cross-Origin Resource Sharing" (Compartición de recursos entre orígenes). Los navegadores web tienen una política de seguridad llamada "Same-Origin Policy": por defecto, el código JavaScript en `http://localhost:5173` (Vue.js) NO puede hacer peticiones a `http://localhost:5000` (nuestra API) porque son orígenes diferentes.

CORS es el mecanismo que permite a la API decirle al navegador: "sí, confío en peticiones de `http://localhost:5173`".

**Importante:** CORS solo lo impone el navegador. Herramientas como Postman, REST Client, o el backend de otro servidor ignoran CORS completamente.

### Cómo se configura en Program.cs

```csharp
// Registro del servicio CORS
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirVue", politica =>
    {
        politica
            .WithOrigins("http://localhost:5173")  // solo permitir este origin
            .AllowAnyHeader()    // aceptar cualquier header (Content-Type, Authorization, etc.)
            .AllowAnyMethod();   // aceptar GET, POST, PUT, DELETE, etc.
    });
});

// Uso en el pipeline — ANTES de Authentication y Authorization
app.UseCors("PermitirVue");
```

### ¿Por qué CORS va ANTES de Authentication?

Antes de que el navegador haga una petición con método no-seguro (POST, PUT, DELETE), envía una "preflight request" (petición `OPTIONS`) para preguntar si tiene permiso. Si CORS va después de Authentication, esta preflight falla con 401 antes de que CORS pueda responder con los headers correctos.

El orden correcto:
```csharp
app.UseCors("PermitirVue");    // CORS primero — responde a las preflights
app.UseAuthentication();        // luego autenticación
app.UseAuthorization();         // luego autorización
```

---

## 12. Swagger y OpenAPI: documentación interactiva

### ¿Qué es Swagger?

Swagger (ahora llamado OpenAPI) es un estándar para documentar APIs REST. Genera automáticamente un archivo JSON (`/swagger/v1/swagger.json`) que describe todos los endpoints, sus parámetros y sus respuestas.

`Swashbuckle.AspNetCore` lee los atributos de tus controladores (`[HttpGet]`, `[HttpPost]`, `[ProducesResponseType]`, `/// <summary>`) y genera ese JSON automáticamente.

### La UI de Swagger

`Swashbuckle.AspNetCore` también incluye una interfaz web en `https://localhost:xxxx/swagger` donde puedes:
- Ver todos los endpoints documentados
- Hacer clic en un endpoint y ejecutarlo directamente
- Pegar tu token JWT en el botón "Authorize" para probar endpoints protegidos

### Configuración con botón Authorize (JWT)

```csharp
builder.Services.AddSwaggerGen(opciones =>
{
    // Información general de la API
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcommerceNet API",
        Version = "v1",
        Description = "API REST para tienda en línea — Proyecto DaCodes"
    });

    // Definir el esquema de seguridad JWT
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",          // nombre del header
        Type = SecuritySchemeType.Http,  // tipo: HTTP (no API Key)
        Scheme = "bearer",               // esquema: Bearer tokens
        BearerFormat = "JWT",            // formato del token
        In = ParameterLocation.Header,   // dónde va el token: en el header
        Description = "Ingresa tu token JWT"
    });

    // Indicar que TODOS los endpoints pueden necesitar el token
    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"  // referencia al esquema definido arriba
                }
            },
            Array.Empty<string>()  // sin scopes adicionales (JWT no usa scopes como OAuth2)
        }
    });
});
```

---

## 13. Hashing de contraseñas con BCrypt

### ¿Por qué no se guarda la contraseña directamente?

```csharp
// MAL — guardas la contraseña en texto plano
usuario.Password = dto.Password;  // "MiPassword123!"
// Si la base de datos es hackeada, todas las contraseñas quedan expuestas

// BIEN — guardas el hash irreversible
usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
// Guardas algo como: "$2b$10$X7Ld8k3Y2mN9pQ4jV6cWuO..."
// Incluso con acceso a la BD, el atacante no puede recuperar la contraseña original
```

### Cómo funciona BCrypt

BCrypt aplica el algoritmo de Blowfish múltiples veces con un "salt" (valor aleatorio). Cada vez que hasheas la misma contraseña, el resultado es diferente porque el salt cambia:

```csharp
// Hashear la contraseña al registrarse
var hash1 = BCrypt.Net.BCrypt.HashPassword("MiPassword123!");
// "$2b$10$8dQ3XjK2mR7nL4pV9wCyZu..."

var hash2 = BCrypt.Net.BCrypt.HashPassword("MiPassword123!");
// "$2b$10$3kF7YlM9nS2pQ8vW5bAxRe..."
// ¡El hash es diferente aunque la contraseña es la misma! (el salt cambia)

// Verificar la contraseña al hacer login
bool esCorrecta = BCrypt.Net.BCrypt.Verify("MiPassword123!", hash1);
// true — BCrypt extrae el salt del hash guardado y recalcula
bool esIncorrecta = BCrypt.Net.BCrypt.Verify("OtraContraseña", hash1);
// false
```

`Verify` extrae el salt del hash guardado, aplica BCrypt con ese mismo salt a la contraseña ingresada, y compara los resultados. Así puede verificar sin necesitar el texto plano original.

### ¿Por qué es "lento" intencionalmente?

```csharp
// factor 10 (por defecto) = 2^10 = 1.024 iteraciones
var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
// tarda ~100ms en una máquina moderna

// factor 12 = 2^12 = 4.096 iteraciones
var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
// tarda ~400ms — suficiente para frustrar ataques de fuerza bruta masivos
```

Para un usuario legítimo, 100ms es imperceptible. Para un atacante que prueba millones de contraseñas, 100ms por intento significa ~10 contraseñas por segundo en lugar de millones. BCrypt hace que los ataques de diccionario sean económicamente inviables.

---

## 14. Análisis línea a línea: Program.cs

```csharp
// ─── IMPORTACIONES ───────────────────────────────────────────────────────────

using System.Text;
// System.Text contiene Encoding.UTF8.GetBytes() — para convertir la clave JWT a bytes

using EcommerceNet.API.Middleware;
// Para usar ManejadorErroresMiddleware

using EcommerceNet.API.Servicios;
// Para referenciar AuthServicio (la clase concreta que se registra en DI)

using EcommerceNet.Core.Servicios;
// Para referenciar IAuthServicio e ICarritoServicio (las interfaces)

using Microsoft.AspNetCore.Authentication.JwtBearer;
// Trae JwtBearerDefaults.AuthenticationScheme = "Bearer"

using Microsoft.IdentityModel.Tokens;
// Trae TokenValidationParameters y SymmetricSecurityKey

using Microsoft.OpenApi.Models;
// Trae OpenApiInfo, OpenApiSecurityScheme, etc. (Swashbuckle)

// ─── CREACIÓN DEL BUILDER ────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
// WebApplication.CreateBuilder crea el objeto que configura la aplicación
// args son los argumentos de línea de comandos (ej: --urls http://localhost:5000)
// Internamente configura logging, configuration, inyección de dependencias

// ─── REGISTRO DE SERVICIOS (DI CONTAINER) ────────────────────────────────────

builder.Services.AddControllers();
// Registra el sistema de controladores MVC
// Habilita [ApiController], routing basado en atributos, model binding

builder.Services.AddEndpointsApiExplorer();
// Permite a Swashbuckle descubrir los endpoints para documentarlos

builder.Services.AddSwaggerGen(opciones =>
// AddSwaggerGen registra el generador de documentación OpenAPI
{
    opciones.SwaggerDoc("v1", new OpenApiInfo { ... });
    // Define la versión de la API y su descripción

    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
    // Agrega el botón "Authorize" en la UI de Swagger

    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement { ... });
    // Indica que los endpoints pueden requerir el Bearer token
});

builder.Services.AddCors(opciones =>
// Registra el sistema CORS con la política que definimos
{
    opciones.AddPolicy("PermitirVue", politica =>
    {
        politica
            .WithOrigins("http://localhost:5173")  // solo Vue.js en dev
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
// builder.Configuration accede a appsettings.json
// ["Jwt:Key"] navega: { "Jwt": { "Key": "valor" } }
// El ! dice "sé que no es null — fallo si es null"

builder.Services.AddAuthentication(opciones =>
{
    opciones.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // = "Bearer" — esquema para identificar al usuario en cada petición

    opciones.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    // = "Bearer" — esquema para redirigir cuando falla la autenticación
})
.AddJwtBearer(opciones =>
// .AddJwtBearer encadena la configuración de Bearer tokens sobre AddAuthentication
{
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        // Verificar que el claim "iss" del token sea "EcommerceNet.API"

        ValidateAudience = true,
        // Verificar que el claim "aud" del token sea "EcommerceNet.Web"

        ValidateLifetime = true,
        // Verificar que el token no haya expirado (claim "exp")

        ValidateIssuerSigningKey = true,
        // Verificar que la firma HMAC-SHA256 sea correcta con nuestra clave

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        // SymmetricSecurityKey: la misma clave firma y verifica (simétrico)
        // Encoding.UTF8.GetBytes: convierte el string a array de bytes
    };
});

builder.Services.AddAuthorization();
// Habilita el sistema de autorización (para [Authorize] y [Authorize(Roles="Admin")])

builder.Services.AddScoped<IAuthServicio, AuthServicio>();
// Registra: "cuando alguien pida IAuthServicio, crea una instancia de AuthServicio"
// AddScoped: una instancia por petición HTTP

builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();

// ─── CONSTRUIR LA APLICACIÓN ─────────────────────────────────────────────────

var app = builder.Build();
// Construye la app con todos los servicios registrados
// A partir de aquí, configuras el PIPELINE (el orden de middleware)

// ─── PIPELINE DE MIDDLEWARE ───────────────────────────────────────────────────

app.UseMiddleware<ManejadorErroresMiddleware>();
// PRIMERO: atrapa cualquier excepción que tire cualquier middleware siguiente
// UseMiddleware<T> crea y encadena el middleware en el pipeline

if (app.Environment.IsDevelopment())
// app.Environment.IsDevelopment() = true cuando ASPNETCORE_ENVIRONMENT = "Development"
// En producción, Swagger no se expone (no queremos documentación pública)
{
    app.UseSwagger();
    // Sirve el JSON de OpenAPI en /swagger/v1/swagger.json

    app.UseSwaggerUI(opciones =>
    // Sirve la interfaz web en /swagger
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceNet API v1");
        opciones.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
// Redirige peticiones HTTP a HTTPS (301 Moved Permanently)

app.UseCors("PermitirVue");
// Aplica la política CORS "PermitirVue" — agrega headers CORS a las respuestas

app.UseAuthentication();
// Lee el header Authorization: Bearer {token}, valida el JWT y puebla User

app.UseAuthorization();
// Verifica que el usuario tiene los permisos requeridos por [Authorize]

app.MapControllers();
// Registra las rutas de todos los controladores anotados con [ApiController]
// Conecta /api/productos → ProductosController, etc.

app.Run();
// Inicia el servidor Kestrel y comienza a aceptar peticiones HTTP
```

---

## 15. Análisis línea a línea: AuthController.cs

```csharp
using EcommerceNet.Core.DTOs;
// Para RegistroDto y LoginDto

using EcommerceNet.Core.Servicios;
// Para IAuthServicio

using Microsoft.AspNetCore.Mvc;
// Para ControllerBase, IActionResult, [ApiController], [Route], [HttpPost], [FromBody]

namespace EcommerceNet.API.Controllers;
// File-scoped namespace — todo el archivo pertenece a este namespace

[ApiController]
// Activa: validación automática, inferencia de binding, respuestas ProblemDetails

[Route("api/[controller]")]
// Ruta base: /api/auth
// [controller] → "auth" (de "Auth" + "Controller")

public class AuthController : ControllerBase
// ControllerBase: clase base para APIs REST
// Hereda: Ok(), BadRequest(), NotFound(), Unauthorized(), Forbid(), User, etc.
{
    private readonly IAuthServicio _authServicio;
    // private readonly: no puede modificarse después del constructor
    // Nombrado con guión bajo por convención de campos privados

    public AuthController(IAuthServicio authServicio)
    // Constructor con inyección de dependencias
    // El contenedor DI creará IAuthServicio (AuthServicio) y lo pasará aquí
    {
        _authServicio = authServicio;
    }

    [HttpPost("registrar")]
    // POST /api/auth/registrar
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    // async Task<IActionResult>: método asíncrono que devuelve una respuesta HTTP
    // [FromBody]: leer RegistroDto del body JSON de la petición
    {
        var resultado = await _authServicio.RegistrarAsync(dto);
        // await: espera que el servicio termine (puede ser una operación de BD)
        // resultado es Resultado<AuthRespuestaDto>

        if (!resultado.Exito)
            return BadRequest(resultado);
        // BadRequest() = código HTTP 400
        // resultado contiene el mensaje de error en resultado.Mensaje

        return Ok(resultado);
        // Ok() = código HTTP 200
        // resultado contiene el token JWT en resultado.Datos.Token
    }

    [HttpPost("login")]
    // POST /api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authServicio.LoginAsync(dto);

        if (!resultado.Exito)
            return Unauthorized(resultado);
        // Unauthorized() = código HTTP 401
        // Se usa 401 (no 400) porque las credenciales son incorrectas (no están mal formadas)

        return Ok(resultado);
    }
}
```

---

## 16. Análisis línea a línea: ProductosController.cs

Solo los fragmentos más importantes (el archivo completo tiene 130 líneas):

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IUnidadDeTrabajo _uow;
    // IUnidadDeTrabajo agrupa todos los repositorios
    // En lugar de inyectar IProductoRepositorio directamente, usamos el UoW

    public ProductosController(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    // ──── ENDPOINTS PÚBLICOS ─────────────────────────────────────────────────

    [HttpGet]
    // GET /api/productos — sin [Authorize], cualquiera puede acceder
    public async Task<IActionResult> ObtenerTodos()
    {
        var productos = await _uow.Productos.ObtenerActivosAsync();
        // _uow.Productos es IProductoRepositorio
        // ObtenerActivosAsync() viene de IProductoRepositorio (Día 1)

        var dtos = productos.Select(MapearADto);
        // Select con un método estático — convierte cada Producto a ProductoDto
        // productos es IEnumerable<Producto>
        // dtos es IEnumerable<ProductoDto>

        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
        // Anidado: Ok(IActionResult) contiene Resultado<T>.Ok(datos)
        // El primero es el código HTTP 200
        // El segundo es el envoltorio del cuerpo de la respuesta JSON
    }

    [HttpGet("{id}")]
    // GET /api/productos/5
    public async Task<IActionResult> ObtenerPorId(int id)
    // int id se extrae automáticamente de la URL (parámetro de ruta)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);

        if (producto == null)
            return NotFound(Resultado<ProductoDto>.Error("Producto no encontrado"));
        // 404 + mensaje de error en el body JSON

        return Ok(Resultado<ProductoDto>.Ok(MapearADto(producto)));
    }

    // ──── ENDPOINT CON QUERY STRING ─────────────────────────────────────────

    [HttpGet("buscar")]
    // GET /api/productos/buscar?termino=laptop
    public async Task<IActionResult> Buscar([FromQuery] string termino)
    // [FromQuery] extrae el valor del query string: ?termino=laptop → termino="laptop"
    {
        if (string.IsNullOrWhiteSpace(termino))
        // string.IsNullOrWhiteSpace: true si es null, "", " ", "\t", etc.
            return BadRequest(Resultado<string>.Error("El término de búsqueda es obligatorio"));

        var productos = await _uow.Productos.BuscarPorNombreAsync(termino);
        var dtos = productos.Select(MapearADto);
        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
    }

    // ──── ENDPOINTS PROTEGIDOS ───────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin")]
    // Dos atributos = dos verificaciones:
    // 1. ¿Tiene token JWT válido? (Authorize)
    // 2. ¿El token tiene Role = "Admin"? (Roles = "Admin")
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        var errores = new List<string>();
        // List<string>: lista de mensajes de error para validación

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            errores.Add("El nombre es obligatorio");
        if (dto.Precio <= 0)
            errores.Add("El precio debe ser mayor a cero");
        if (dto.Stock < 0)
            errores.Add("El stock no puede ser negativo");
        if (dto.CategoriaId <= 0)
            errores.Add("Debe especificar una categoría válida");

        if (errores.Count > 0)
            return BadRequest(Resultado<ProductoDto>.ErrorValidacion(errores));
        // ErrorValidacion(List<string>) devuelve Resultado con lista de errores

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            ImagenUrl = dto.ImagenUrl,
            CategoriaId = dto.CategoriaId
            // Activo = true por defecto (propiedad del Día 1)
            // Id = 0 — la BD lo asignará
        };

        await _uow.Productos.AgregarAsync(producto);
        // Agrega la entidad al contexto de EF Core (no a la BD todavía)

        await _uow.GuardarCambiosAsync();
        // Aquí EF Core ejecuta el INSERT en la BD
        // Después de esto, producto.Id tiene el ID asignado por la BD

        return CreatedAtAction(
            nameof(ObtenerPorId),     // "ObtenerPorId"
            new { id = producto.Id }, // parámetros de ruta: /api/productos/{id}
            Resultado<ProductoDto>.Ok(MapearADto(producto), "Producto creado exitosamente")
        );
        // HTTP 201 + header Location: /api/productos/5 + el DTO en el body
    }

    // ──── MÉTODO PRIVADO DE MAPEO ────────────────────────────────────────────

    private static ProductoDto MapearADto(Producto p) => new()
    // static: no necesita instancia del controlador — solo datos de entrada
    // El tipo de retorno se infiere: new() = new ProductoDto()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Precio = p.Precio,
        Stock = p.Stock,
        ImagenUrl = p.ImagenUrl,
        CategoriaNombre = p.Categoria?.Nombre ?? "Sin categoría",
        // p.Categoria?.Nombre: si Categoria es null → null
        // ?? "Sin categoría": si es null → "Sin categoría"
        Disponible = p.Activo && p.Stock > 0
        // Solo disponible si el producto está activo Y tiene stock
    };
}
```

---

## 17. Análisis línea a línea: CarritoController.cs

```csharp
using System.Security.Claims;
// Para ClaimTypes — necesario para leer claims del token JWT

[ApiController]
[Route("api/[controller]")]
[Authorize]
// [Authorize] a nivel de clase: TODOS los métodos requieren autenticación
// Equivale a poner [Authorize] en cada uno de los 6 métodos
public class CarritoController : ControllerBase
{
    private readonly ICarritoServicio _carritoServicio;
    // ICarritoServicio tiene la lógica de negocio del carrito
    // El controlador SOLO traduce entre HTTP y el servicio

    public CarritoController(ICarritoServicio carritoServicio)
    {
        _carritoServicio = carritoServicio;
    }

    private int ObtenerUsuarioId()
    // private: solo para uso interno del controlador
    // int: devuelve el ID del usuario como entero
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
        // User es ClaimsPrincipal — contiene los claims del token JWT
        // FindFirst busca el primer claim con ese tipo
        // ClaimTypes.NameIdentifier = "nameidentifier" (el claim con el ID)

            ?? User.FindFirst("sub");
        // ?? = si el primero es null, intenta con "sub"
        // "sub" es el nombre estándar de OAuth2 para el ID del sujeto

        if (claim == null || !int.TryParse(claim.Value, out var id))
        // claim == null: no se encontró el claim → token mal formado
        // !int.TryParse: el valor no se puede convertir a int → token malo
        // out var id: declara la variable 'id' aquí y recibe el valor convertido
            throw new UnauthorizedAccessException("Token inválido");
        // El ManejadorErroresMiddleware atrapa esta excepción y devuelve 401

        return id;
    }

    [HttpGet]
    // GET /api/carrito
    public async Task<IActionResult> ObtenerCarrito()
    {
        var usuarioId = ObtenerUsuarioId();
        // Obtenemos el ID del usuario autenticado del token JWT

        var resultado = await _carritoServicio.ObtenerCarritoAsync(usuarioId);
        // El servicio carga el carrito del usuario desde la BD
        // resultado es Resultado<CarritoDto>

        return Ok(resultado);
        // Siempre devuelve 200 — si el carrito no existe, el servicio lo crea
    }

    [HttpPost("agregar")]
    // POST /api/carrito/agregar
    public async Task<IActionResult> Agregar([FromBody] AgregarAlCarritoDto dto)
    // AgregarAlCarritoDto viene del Día 1: { productoId: 1, cantidad: 2 }
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.AgregarProductoAsync(usuarioId, dto);

        if (!resultado.Exito)
            return BadRequest(resultado);
        // Puede fallar si: el producto no existe, no tiene stock suficiente, etc.

        return Ok(resultado);
    }

    [HttpPut("{productoId}")]
    // PUT /api/carrito/5
    public async Task<IActionResult> ActualizarCantidad(
        int productoId,                   // de la URL: /api/carrito/5
        [FromBody] ActualizarCantidadDto dto)  // del body: { "cantidad": 3 }
    // Los dos parámetros vienen de lugares diferentes
    // productoId → URL → no necesita [FromRoute] (es automático por el placeholder)
    // dto → body → necesita [FromBody]
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.ActualizarCantidadAsync(
            usuarioId, productoId, dto.Cantidad);

        if (!resultado.Exito)
            return BadRequest(resultado);
        return Ok(resultado);
    }

    [HttpDelete("{productoId}")]
    // DELETE /api/carrito/5
    public async Task<IActionResult> EliminarProducto(int productoId)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.EliminarProductoAsync(usuarioId, productoId);
        if (!resultado.Exito) return BadRequest(resultado);
        return Ok(resultado);
    }

    [HttpDelete]
    // DELETE /api/carrito (sin {id} — vacía el carrito completo)
    public async Task<IActionResult> Vaciar()
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.VaciarCarritoAsync(usuarioId);
        return Ok(resultado);
        // Vaciar siempre tiene éxito — no tiene lógica de validación que falle
    }

    [HttpPost("checkout")]
    // POST /api/carrito/checkout
    public async Task<IActionResult> Checkout([FromBody] CrearOrdenDto dto)
    // CrearOrdenDto del Día 1: { "direccionEnvio": "Calle 50 #200..." }
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.CheckoutAsync(usuarioId, dto);
        // CheckoutAsync (Día 1) hace mucho trabajo:
        // 1. Carga el carrito
        // 2. Valida que tenga items
        // 3. Verifica stock de cada producto
        // 4. Crea la Orden
        // 5. Reduce stock de cada producto
        // 6. Vacía el carrito
        // 7. Genera el número de orden
        // El controlador no hace nada de eso — solo llama al servicio

        if (!resultado.Exito) return BadRequest(resultado);
        return Ok(resultado);
    }
}

// ─── DTO AUXILIAR ─────────────────────────────────────────────────────────────

public class ActualizarCantidadDto
// Clase simple — solo necesita la nueva cantidad
// Se pone aquí porque solo la usa este controlador
// En Día 3 se podría mover a Core/DTOs/ si se reutiliza
{
    public int Cantidad { get; set; }
}
```

---

## 18. Análisis línea a línea: ManejadorErroresMiddleware.cs

```csharp
using System.Net;
// Para HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest, etc.

using System.Text.Json;
// Para JsonSerializer.Serialize() — convierte objeto a string JSON

using EcommerceNet.Core.DTOs;
// Para Resultado<T> — envuelve el error en el formato estándar

namespace EcommerceNet.API.Middleware;

public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;
    // RequestDelegate es el tipo del "siguiente paso" en el pipeline
    // Es un delegado: Func<HttpContext, Task>
    // Llamar _next(contexto) pasa la petición al siguiente middleware

    private readonly ILogger<ManejadorErroresMiddleware> _logger;
    // ILogger es la interfaz de logging de .NET
    // <ManejadorErroresMiddleware> es el "category name" para filtrar logs

    public ManejadorErroresMiddleware(
        RequestDelegate next,
        ILogger<ManejadorErroresMiddleware> logger)
    // El contenedor DI inyecta RequestDelegate automáticamente
    // ILogger también lo inyecta el contenedor
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    // InvokeAsync es el método que ASP.NET Core llama por cada petición
    // El nombre debe ser exactamente "InvokeAsync" o "Invoke"
    // HttpContext contiene: Request (la petición), Response (la respuesta), User, etc.
    {
        try
        {
            await _next(contexto);
            // Pasamos la petición al siguiente middleware en el pipeline
            // Si todo va bien, la respuesta ya está escrita cuando esto termine
            // Si algo lanza una excepción, vamos al catch
        }
        catch (Exception ex)
        // Exception es la clase base de todas las excepciones en .NET
        // Aquí atrapamos CUALQUIER excepción no manejada de toda la aplicación
        {
            _logger.LogError(ex, "Error no manejado: {Mensaje}", ex.Message);
            // LogError registra en el sistema de logging con nivel Error
            // ex: la excepción completa (con stack trace)
            // "Error no manejado: {Mensaje}": plantilla de mensaje con placeholder
            // ex.Message: el mensaje específico de la excepción

            await ManejarExcepcionAsync(contexto, ex);
        }
    }

    private static async Task ManejarExcepcionAsync(HttpContext contexto, Exception ex)
    // static: no necesita estado de la instancia
    // async Task: puede hacer operaciones asíncronas (escribir la respuesta)
    {
        contexto.Response.ContentType = "application/json";
        // Indicar que la respuesta es JSON, no HTML

        var (codigo, mensaje) = ex switch
        // Switch expression (C# 8+) — más conciso que if/else if
        // (codigo, mensaje) = deconstrucción de una tupla (dos valores)
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado"),
            // UnauthorizedAccessException → 401

            InvalidOperationException   => (HttpStatusCode.BadRequest, ex.Message),
            // InvalidOperationException → 400 con el mensaje específico de la excepción

            ArgumentException           => (HttpStatusCode.BadRequest, ex.Message),
            // ArgumentException → 400 (argumento inválido en algún método)

            KeyNotFoundException        => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            // KeyNotFoundException → 404

            _                           => (HttpStatusCode.InternalServerError, "Error interno")
            // _ = wildcard — cualquier otra excepción → 500
        };

        contexto.Response.StatusCode = (int)codigo;
        // Asignar el código HTTP numérico
        // (int)HttpStatusCode.BadRequest = 400
        // (int)HttpStatusCode.InternalServerError = 500

        var resultado = Resultado<string>.Error(mensaje);
        // Crear el objeto de respuesta estándar
        // Resultado<string> porque el "dato" de un error es el mensaje (string)

        var opciones = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            // Serializar con camelCase: { "exito": false } no { "Exito": false }
            // Es la convención de JSON (el frontend espera camelCase)
        };

        var json = JsonSerializer.Serialize(resultado, opciones);
        // Convierte el objeto C# a string JSON
        // resultado = { Exito: false, Mensaje: "No autorizado" }
        // json = '{"exito":false,"datos":null,"mensaje":"No autorizado","errores":[]}'

        await contexto.Response.WriteAsync(json);
        // Escribe el JSON en el body de la respuesta HTTP
        // await porque es una operación de I/O (escritura de red)
    }
}
```

---

## 19. Patrón: separación entre controlador y servicio

### La regla de oro

Un controlador hace exactamente **tres cosas** por cada petición:

```
1. Extraer datos del HTTP (URL, body, headers, token)
2. Llamar al servicio correspondiente
3. Traducir el resultado a un código HTTP
```

**Nada más.** Si ves lógica de negocio en un controlador, es un problema de diseño.

### Ejemplo correcto vs incorrecto

```csharp
// ❌ INCORRECTO — lógica de negocio en el controlador
[HttpPost("agregar")]
public async Task<IActionResult> Agregar([FromBody] AgregarAlCarritoDto dto)
{
    var carrito = await _uow.Carritos.ObtenerPorUsuarioIdAsync(usuarioId);
    if (carrito == null) carrito = new Carrito { UsuarioId = usuarioId };

    var producto = await _uow.Productos.ObtenerPorIdAsync(dto.ProductoId);
    if (producto == null) return NotFound("Producto no encontrado");
    if (!producto.TieneStockSuficiente(dto.Cantidad))
        return BadRequest("No hay suficiente stock");

    carrito.AgregarProducto(producto, dto.Cantidad);
    await _uow.GuardarCambiosAsync();
    return Ok(carrito);
    // 30 líneas de lógica de negocio en el controlador — MALO
}

// ✅ CORRECTO — el controlador solo traduce
[HttpPost("agregar")]
public async Task<IActionResult> Agregar([FromBody] AgregarAlCarritoDto dto)
{
    var usuarioId = ObtenerUsuarioId();           // 1. Extraer del token
    var resultado = await _carritoServicio.AgregarProductoAsync(usuarioId, dto);  // 2. Llamar servicio
    if (!resultado.Exito) return BadRequest(resultado);  // 3. Traducir a HTTP
    return Ok(resultado);                          // 3. Traducir a HTTP
    // 5 líneas — el controlador no sabe qué hace el servicio internamente
}
```

### ¿Por qué importa esta separación?

1. **Testabilidad**: puedes probar `CarritoServicio` con un mock sin levantar HTTP
2. **Reutilización**: el mismo servicio puede usarse desde un controller, un background job, o una CLI
3. **Legibilidad**: el controlador es fácil de entender — es solo enrutamiento
4. **Mantenimiento**: cambiar la lógica de negocio no requiere tocar el controlador

---

## 20. Glosario de atributos y palabras nuevas del Día 2

### Atributos (decoradores)

| Atributo | Dónde se usa | Qué hace |
|----------|-------------|----------|
| `[ApiController]` | Clase | Activa validación automática, model binding inteligente |
| `[Route("api/[controller]")]` | Clase | Define la ruta base del controlador |
| `[HttpGet]` | Método | Responde a peticiones GET |
| `[HttpPost]` | Método | Responde a peticiones POST |
| `[HttpPut]` | Método | Responde a peticiones PUT |
| `[HttpDelete]` | Método | Responde a peticiones DELETE |
| `[FromBody]` | Parámetro | Leer del body JSON |
| `[FromQuery]` | Parámetro | Leer del query string (?clave=valor) |
| `[Authorize]` | Clase/Método | Requiere token JWT válido |
| `[Authorize(Roles="Admin")]` | Clase/Método | Requiere JWT con rol Admin |
| `[AllowAnonymous]` | Método | Excepciona la regla de [Authorize] |

### Tipos y clases nuevas

| Tipo | Namespace | Para qué |
|------|-----------|---------|
| `ControllerBase` | `Microsoft.AspNetCore.Mvc` | Clase base de controladores API |
| `IActionResult` | `Microsoft.AspNetCore.Mvc` | Tipo de retorno de los métodos del controlador |
| `HttpContext` | `Microsoft.AspNetCore.Http` | Contexto de la petición HTTP actual |
| `RequestDelegate` | `Microsoft.AspNetCore.Http` | El siguiente paso en el pipeline |
| `ClaimsPrincipal` | `System.Security.Claims` | El usuario autenticado (con sus claims) |
| `Claim` | `System.Security.Claims` | Un dato afirmado sobre el usuario |
| `ClaimTypes` | `System.Security.Claims` | Constantes para tipos de claims estándar |
| `JwtSecurityToken` | `System.IdentityModel.Tokens.Jwt` | Un token JWT |
| `JwtSecurityTokenHandler` | `System.IdentityModel.Tokens.Jwt` | Genera y valida tokens JWT |
| `SymmetricSecurityKey` | `Microsoft.IdentityModel.Tokens` | Clave criptográfica simétrica |
| `SigningCredentials` | `Microsoft.IdentityModel.Tokens` | Algoritmo + clave para firmar |
| `TokenValidationParameters` | `Microsoft.IdentityModel.Tokens` | Reglas de validación del JWT |
| `HttpStatusCode` | `System.Net` | Enum con códigos HTTP (400, 401, 404, 500) |
| `JsonSerializer` | `System.Text.Json` | Serializa/deserializa JSON |
| `ILogger<T>` | `Microsoft.Extensions.Logging` | Sistema de logging de .NET |

### Métodos de ControllerBase

| Método | Código HTTP | Cuándo usarlo |
|--------|-------------|---------------|
| `Ok(datos)` | 200 | Petición exitosa |
| `CreatedAtAction(accion, params, datos)` | 201 | Recurso creado |
| `BadRequest(error)` | 400 | Datos inválidos |
| `Unauthorized(error)` | 401 | Sin autenticación o credenciales incorrectas |
| `Forbid()` | 403 | Autenticado pero sin permiso |
| `NotFound(error)` | 404 | Recurso no encontrado |

### Extensiones de Program.cs

| Método | Registra / Usa |
|--------|----------------|
| `AddControllers()` | Sistema de controladores MVC |
| `AddSwaggerGen()` | Generador de documentación OpenAPI |
| `AddCors()` | Sistema de políticas CORS |
| `AddAuthentication()` | Sistema de autenticación |
| `AddJwtBearer()` | Validación de tokens JWT Bearer |
| `AddAuthorization()` | Sistema de autorización por roles |
| `AddScoped<I, C>()` | Registra C como implementación de I (una por request) |
| `UseMiddleware<T>()` | Agrega T al pipeline de middleware |
| `UseSwagger()` | Sirve el JSON de OpenAPI |
| `UseSwaggerUI()` | Sirve la interfaz web de Swagger |
| `UseHttpsRedirection()` | Redirige HTTP → HTTPS |
| `UseCors(política)` | Aplica la política CORS al pipeline |
| `UseAuthentication()` | Lee y valida tokens JWT |
| `UseAuthorization()` | Verifica permisos [Authorize] |
| `MapControllers()` | Conecta rutas con controladores |
