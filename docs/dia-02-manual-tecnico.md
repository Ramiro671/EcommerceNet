# Manual Técnico — Día 2: ASP.NET Core Web API

> **Fecha de ejecución:** 2026-04-03
> **Herramienta:** Claude Code (claude-sonnet-4-6) ejecutado dentro del VSCode Extension
> **Entorno:** Windows 11, .NET SDK 10.0.103, Git 2.51.1
> **Resultado final:** Build exitoso, 23/23 pruebas pasando, 0 errores, 0 warnings

---

## Índice

1. [Qué leyó Claude Code antes de escribir una sola línea](#1-qué-leyó-claude-code-antes-de-escribir-una-sola-línea)
2. [Fase 1 — Instalación de paquetes NuGet](#2-fase-1--instalación-de-paquetes-nuget)
3. [Fase 2 — Archivos nuevos en EcommerceNet.Core](#3-fase-2--archivos-nuevos-en-ecommercenetcore)
4. [Fase 3 — Archivos del proyecto EcommerceNet.API](#4-fase-3--archivos-del-proyecto-ecommercenetapi)
5. [Inventario completo de archivos creados/modificados](#5-inventario-completo-de-archivos-creadosmodificados)
6. [Grafo de dependencias actualizado](#6-grafo-de-dependencias-actualizado)
7. [Decisiones técnicas y por qué](#7-decisiones-técnicas-y-por-qué)
8. [Errores que NO ocurrieron (y cómo se evitaron)](#8-errores-que-no-ocurrieron-y-cómo-se-evitaron)
9. [Error que SÍ ocurrió y cómo se resolvió](#9-error-que-sí-ocurrió-y-cómo-se-resolvió)
10. [Estado del proyecto al cierre del Día 2](#10-estado-del-proyecto-al-cierre-del-día-2)
11. [Pendientes para el Día 3](#11-pendientes-para-el-día-3)

---

## 1. Qué leyó Claude Code antes de escribir una sola línea

Claude Code ejecutó cuatro lecturas antes de tocar cualquier archivo:

### 1.1 `CLAUDE.md` (reglas de arquitectura)

Extrajo las reglas que gobiernan el Día 2:

| Regla | Efecto en el código del Día 2 |
|-------|-------------------------------|
| `Core` no depende de nada externo | `AuthServicio.cs` fue colocado en `API/Servicios/`, NO en `Core/Servicios/` |
| Namespaces file-scoped | Todos los archivos usan `namespace X;` |
| Inyección de dependencias por constructor | Todos los controladores y servicios reciben sus dependencias por constructor |
| Clase `Resultado<T>` obligatoria | Cada endpoint envuelve su respuesta en `Resultado<T>` |
| Nunca exponer entidades en la API | Los controladores mapean entidades a DTOs antes de devolver |
| Comentarios en español | Todos los `/// <summary>` y comentarios inline están en español |

### 1.2 `docs/dia-02-aspnet-api.md` (plan del día)

Leyó el archivo en 8 bloques (supera el límite de lectura de ~10.000 tokens) y extrajo:
- Los comandos de instalación de paquetes NuGet
- El código fuente de todos los controladores
- La configuración completa de Program.cs y appsettings.json
- El archivo requests.http completo
- El código del middleware de errores

### 1.3 `docs/dia-01-manual-tecnico.md` (formato de referencia)

Leyó las primeras 150 líneas para extraer el formato: secciones, tablas de decisiones técnicas, bloques de código con contexto, y el nivel de detalle esperado.

### 1.4 `docs/dia-01-clase-programacion.md` (formato de referencia)

Leyó las primeras 80 líneas para entender el estilo: explicaciones desde cero, análisis línea por línea, glosarios al final.

---

## 2. Fase 1 — Instalación de paquetes NuGet

> Todos los paquetes se instalan en `EcommerceNet.API`, nunca en `Core` o `Data`.
> El usuario confirmó la ejecución antes de proceder.

### 2.1 Comandos ejecutados

```bash
cd C:\Users\ramir\Source\repos\EcommerceNet

dotnet add src/EcommerceNet.API/EcommerceNet.API.csproj \
  package Microsoft.AspNetCore.Authentication.JwtBearer

dotnet add src/EcommerceNet.API/EcommerceNet.API.csproj \
  package Swashbuckle.AspNetCore

dotnet add src/EcommerceNet.API/EcommerceNet.API.csproj \
  package BCrypt.Net-Next
```

### 2.2 Paquetes instalados

| Paquete | Versión instalada | Propósito |
|---------|------------------|-----------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | Valida tokens JWT en cada petición HTTP |
| `Swashbuckle.AspNetCore` | 6.9.0* | Genera la UI de Swagger y el JSON de OpenAPI |
| `BCrypt.Net-Next` | 4.1.0 | Hashea y verifica contraseñas con el algoritmo BCrypt |

> *La versión instalada inicialmente fue 10.1.7. Se bajó a 6.9.0 al corregir un error de compilación (ver sección 9).

### 2.3 Paquete eliminado del `.csproj`

El template `webapi` incluye `Microsoft.AspNetCore.OpenApi` versión 10.0.3 que trae `Microsoft.OpenApi` 2.x — una versión que eliminó el namespace `Microsoft.OpenApi.Models`. Para evitar el conflicto con Swashbuckle 6.9.0 (que usa `Microsoft.OpenApi` 1.x), se eliminó este paquete del `.csproj`.

```xml
<!-- ANTES (template por defecto) -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.3" />

<!-- DESPUÉS (eliminado — reemplazado por Swashbuckle) -->
<!-- ya no existe en el csproj -->
```

### 2.4 Estado final del `.csproj`

```xml
<ItemGroup>
  <PackageReference Include="BCrypt.Net-Next" Version="4.1.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.5" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
</ItemGroup>
```

---

## 3. Fase 2 — Archivos nuevos en EcommerceNet.Core

> El proyecto Core sigue sin paquetes NuGet externos. Solo recibió nuevas interfaces y DTOs.

### 3.1 `Core/DTOs/AuthDtos.cs` (nuevo)

Tres DTOs para el flujo de autenticación:

```csharp
namespace EcommerceNet.Core.DTOs;

public class RegistroDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
}
```

**Por qué existen:** `RegistroDto` y `LoginDto` encapsulan los datos que llegan del cliente sin exponer la entidad `Usuario`. `AuthRespuestaDto` devuelve el token JWT junto con datos básicos del usuario para que el frontend no necesite hacer una segunda petición.

### 3.2 `Core/Servicios/IAuthServicio.cs` (nuevo)

```csharp
public interface IAuthServicio
{
    Task<Resultado<AuthRespuestaDto>> RegistrarAsync(RegistroDto dto);
    Task<Resultado<AuthRespuestaDto>> LoginAsync(LoginDto dto);
}
```

**Por qué en Core:** La interfaz no tiene dependencias externas. Define el contrato que deben cumplir todas las implementaciones de autenticación, sin comprometerse con BCrypt o JWT. En Core solo existe el "qué", nunca el "cómo".

### 3.3 `Core/Interfaces/IUsuarioRepositorio.cs` (nuevo)

```csharp
public interface IUsuarioRepositorio : IRepositorio<Usuario>
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
}
```

**Por qué existe:** `AuthServicio` necesita buscar usuarios por email para verificar si ya están registrados (registro) y para obtener el hash de contraseña (login). Sin este repositorio, el servicio no puede acceder a la base de datos de usuarios.

**Por qué hereda de `IRepositorio<Usuario>`:** Para reutilizar las operaciones genéricas `AgregarAsync`, `ObtenerPorIdAsync`, etc. Solo se agrega `BuscarPorEmailAsync` que es específico de usuarios.

### 3.4 `Core/Interfaces/IUnidadDeTrabajo.cs` (modificado)

Se agregó la propiedad `IUsuarioRepositorio Usuarios`:

```csharp
// ANTES (Día 1)
public interface IUnidadDeTrabajo : IDisposable
{
    IProductoRepositorio Productos { get; }
    ICarritoRepositorio Carritos { get; }
    IOrdenRepositorio Ordenes { get; }
    Task<int> GuardarCambiosAsync();
}

// DESPUÉS (Día 2)
public interface IUnidadDeTrabajo : IDisposable
{
    IProductoRepositorio Productos { get; }
    ICarritoRepositorio Carritos { get; }
    IOrdenRepositorio Ordenes { get; }
    IUsuarioRepositorio Usuarios { get; }  // ← nuevo
    Task<int> GuardarCambiosAsync();
}
```

**Por qué este cambio no rompe las pruebas del Día 1:** Las 23 pruebas unitarias del Día 1 prueban entidades (`Producto`, `Carrito`, `Orden`) directamente sin usar `IUnidadDeTrabajo`. La interfaz actualizada no afecta ningún test existente.

---

## 4. Fase 3 — Archivos del proyecto EcommerceNet.API

### 4.1 `API/appsettings.json` (modificado)

Se agregó la sección `Jwt` y la cadena de conexión a SQL Server LocalDB:

```json
{
  "Jwt": {
    "Key": "EstaEsMiClaveSecretaSuperSeguraDe256BitsParaJWT!!",
    "Issuer": "EcommerceNet.API",
    "Audience": "EcommerceNet.Web",
    "ExpireMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EcommerceNetDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Notas de seguridad:**
- La clave JWT debe tener al menos 256 bits (32 caracteres) para HMAC-SHA256.
- En producción, esta clave debe salir de `appsettings.json` y vivir en variables de entorno o AWS Secrets Manager.
- `MultipleActiveResultSets=true` se necesita cuando EF Core ejecuta múltiples queries en la misma conexión (relacionadas con includes/lazy loading).

### 4.2 `API/Program.cs` (reescritura completa)

Pipeline completo en orden correcto:

```
builder.Services.AddControllers()
builder.Services.AddSwaggerGen(...)       ← Swagger con botón Authorize
builder.Services.AddCors(...)             ← permite Vue.js (puerto 5173)
builder.Services.AddAuthentication(...)   ← JWT Bearer
builder.Services.AddAuthorization()
builder.Services.AddScoped<IAuthServicio, AuthServicio>()
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>()

app.UseMiddleware<ManejadorErroresMiddleware>()  ← 1er middleware
app.UseSwagger() / app.UseSwaggerUI()            ← solo en Development
app.UseHttpsRedirection()
app.UseCors("PermitirVue")
app.UseAuthentication()                          ← ANTES de Authorization
app.UseAuthorization()
app.MapControllers()
```

**Por qué `IUnidadDeTrabajo` NO se registra en DI aquí:** No hay implementación concreta todavía. `UnidadDeTrabajo` con EF Core se crea en el Día 3. Los controllers que dependen de `IUnidadDeTrabajo` compilarán, pero lanzarán una excepción de DI en tiempo de ejecución hasta el Día 3.

### 4.3 `API/Servicios/AuthServicio.cs` (nuevo — decisión de arquitectura)

**Ubicación:** `API/Servicios/` (NO en `Core/Servicios/`)

**Por qué:** `AuthServicio` usa `BCrypt.Net-Next` y `Microsoft.IdentityModel.Tokens`. Ambos son paquetes NuGet externos. CLAUDE.md prohíbe paquetes externos en `Core`. Por lo tanto, la implementación vive en `API` (la capa de infraestructura), mientras que el contrato `IAuthServicio` permanece en `Core`.

Este patrón es estándar en Clean Architecture: Core define "qué debe hacer el sistema", las capas externas definen "cómo lo hace con tecnologías concretas".

Flujo de registro:
1. Verificar que el email no existe → `_uow.Usuarios.BuscarPorEmailAsync(dto.Email)`
2. Hashear la contraseña → `BCrypt.Net.BCrypt.HashPassword(dto.Password)`
3. Crear `Usuario` con `PasswordHash`, `Rol = Cliente`, `FechaRegistro = UtcNow`
4. Guardar → `AgregarAsync` + `GuardarCambiosAsync`
5. Generar token JWT → `GenerarToken(usuario)`
6. Retornar `Resultado<AuthRespuestaDto>.Ok(...)`

Flujo de login:
1. Buscar usuario por email
2. Verificar contraseña → `BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash)`
3. Mensaje de error genérico si falla (previene enumeración de usuarios)
4. Generar y retornar token JWT

Generación del token JWT:
```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
    new Claim(ClaimTypes.Email, usuario.Email),
    new Claim(ClaimTypes.Name, usuario.Nombre),
    new Claim(ClaimTypes.Role, usuario.Rol.ToString())
};

var token = new JwtSecurityToken(
    issuer: _configuracion["Jwt:Issuer"],
    audience: _configuracion["Jwt:Audience"],
    claims: claims,
    expires: expiracion,
    signingCredentials: credenciales  // HMAC-SHA256
);
```

### 4.4 `API/Controllers/AuthController.cs` (nuevo)

Endpoints públicos: `POST /api/auth/registrar` y `POST /api/auth/login`.

Código de respuesta diferenciado:
- Registro fallido → `400 BadRequest` (datos inválidos)
- Login fallido → `401 Unauthorized` (credenciales incorrectas)
- Éxito → `200 OK` con el token JWT

### 4.5 `API/Controllers/ProductosController.cs` (nuevo)

7 endpoints: 4 públicos (GET) + 3 protegidos (POST/PUT/DELETE con `[Authorize(Roles = "Admin")]`).

Patrón de validación manual en `Crear`:
```csharp
var errores = new List<string>();
if (string.IsNullOrWhiteSpace(dto.Nombre)) errores.Add("...");
if (dto.Precio <= 0) errores.Add("...");
if (errores.Count > 0)
    return BadRequest(Resultado<ProductoDto>.ErrorValidacion(errores));
```

Método `CreatedAtAction` para 201:
```csharp
return CreatedAtAction(
    nameof(ObtenerPorId),      // nombre del action que obtiene el recurso
    new { id = producto.Id },  // parámetros de la ruta
    Resultado<ProductoDto>.Ok(MapearADto(producto), "Producto creado"));
// Esto agrega el header Location: /api/productos/5
```

### 4.6 `API/Controllers/CategoriasController.cs` (nuevo)

Controlador con estructura completa pero implementación placeholder. El repositorio de categorías se implementará en el Día 3 con EF Core. La estructura ya existe para que el árbol de rutas sea correcto desde el inicio.

### 4.7 `API/Controllers/CarritoController.cs` (nuevo)

6 endpoints, todos con `[Authorize]` a nivel de clase. La lógica de negocio está **completamente en `CarritoServicio`** (Día 1). El controlador solo:
1. Extrae el `usuarioId` del token JWT via `ClaimTypes.NameIdentifier`
2. Llama al servicio
3. Devuelve `BadRequest` o `Ok` según el resultado

Método helper privado para extraer el ID del usuario:
```csharp
private int ObtenerUsuarioId()
{
    var claim = User.FindFirst(ClaimTypes.NameIdentifier)
        ?? User.FindFirst("sub");

    if (claim == null || !int.TryParse(claim.Value, out var id))
        throw new UnauthorizedAccessException("Token inválido");

    return id;
}
```

### 4.8 `API/Controllers/OrdenesController.cs` (nuevo)

3 endpoints: listar órdenes, detalle, cancelar. Incluye verificación de propiedad:
```csharp
if (orden.UsuarioId != ObtenerUsuarioId())
    return Forbid();  // 403 — la orden existe pero no es del usuario
```

La diferencia entre `NotFound` (404) y `Forbid` (403) es importante: si se devuelve 404 para órdenes de otros usuarios, el atacante no puede saber si existen. Si se devuelve 403, confirma que la orden existe. En este caso se devuelve 403 porque el endpoint ya requiere autenticación — un usuario anónimo jamás llega a esta lógica.

### 4.9 `API/Middleware/ManejadorErroresMiddleware.cs` (nuevo)

Captura cualquier excepción no manejada y la convierte en JSON con `Resultado<T>`:

```csharp
var (codigo, mensaje) = ex switch
{
    UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado"),
    InvalidOperationException   => (HttpStatusCode.BadRequest, ex.Message),
    ArgumentException           => (HttpStatusCode.BadRequest, ex.Message),
    KeyNotFoundException        => (HttpStatusCode.NotFound, "Recurso no encontrado"),
    _                           => (HttpStatusCode.InternalServerError, "Error interno")
};
```

Se registra en Program.cs como **primer middleware** (antes de todo) para que ningún error escape sin ser convertido a JSON.

### 4.10 `API/requests.http` (nuevo)

Archivo con 18 peticiones HTTP listas para ejecutar con la extensión **REST Client** de VS Code. Incluye variable `@token` para pegar el JWT copiado del login.

---

## 5. Inventario completo de archivos creados/modificados

### Archivos NUEVOS

| Archivo | Proyecto | Tipo |
|---------|---------|------|
| `Core/DTOs/AuthDtos.cs` | Core | DTO: RegistroDto, LoginDto, AuthRespuestaDto |
| `Core/Servicios/IAuthServicio.cs` | Core | Interfaz del servicio de auth |
| `Core/Interfaces/IUsuarioRepositorio.cs` | Core | Interfaz del repositorio de usuarios |
| `API/Servicios/AuthServicio.cs` | API | Implementación: BCrypt + JWT |
| `API/Controllers/AuthController.cs` | API | POST /api/auth/registrar, POST /api/auth/login |
| `API/Controllers/ProductosController.cs` | API | CRUD de productos (7 endpoints) |
| `API/Controllers/CategoriasController.cs` | API | Estructura placeholder (Día 3) |
| `API/Controllers/CarritoController.cs` | API | 6 endpoints del carrito |
| `API/Controllers/OrdenesController.cs` | API | 3 endpoints de órdenes |
| `API/Middleware/ManejadorErroresMiddleware.cs` | API | Manejador global de excepciones |
| `API/requests.http` | API | 18 peticiones HTTP para pruebas |

### Archivos MODIFICADOS

| Archivo | Proyecto | Qué cambió |
|---------|---------|-----------|
| `Core/Interfaces/IUnidadDeTrabajo.cs` | Core | Se agregó `IUsuarioRepositorio Usuarios { get; }` |
| `API/Program.cs` | API | Reescritura completa (DI, CORS, JWT, Swagger, pipeline) |
| `API/appsettings.json` | API | Se agregó sección `Jwt` y `ConnectionStrings` |
| `API/EcommerceNet.API.csproj` | API | Se cambiaron referencias de paquetes NuGet |

---

## 6. Grafo de dependencias actualizado

```
┌─────────────────────────────────────────────────────────┐
│                   EcommerceNet.Core                      │
│                                                          │
│  Enums/        Entidades/      Interfaces/               │
│  EstadoOrden   Categoria       IRepositorio<T>           │
│  RolUsuario    Producto        IProductoRepositorio       │
│                Usuario         ICarritoRepositorio        │
│                Carrito         IOrdenRepositorio          │
│                CarritoItem     IUsuarioRepositorio ← NEW  │
│                Orden           IUnidadDeTrabajo (+ Usuarios) ← MOD │
│                OrdenDetalle                              │
│                                                          │
│  DTOs/         Servicios/                                │
│  ProductoDto   ICarritoServicio                          │
│  CarritoDto    CarritoServicio                           │
│  OrdenDto      IAuthServicio ← NEW                       │
│  Resultado<T>  AuthDtos ← NEW                            │
└────────────────────────┬────────────────────────────────┘
                         │ (referencia de proyecto)
                         ▼
┌────────────────────────────────────────────────────────────────┐
│                   EcommerceNet.Data                             │
│                   (pendiente — Día 3)                          │
│                                                                 │
│  Implementará: AppDbContext, repositorios concretos,            │
│  UnidadDeTrabajo, migraciones EF Core, seed data               │
└────────────────────────┬───────────────────────────────────────┘
                         │ (referencia de proyecto)
                         ▼
┌──────────────────────────────────────────────────────────────────┐
│                   EcommerceNet.API                                │
│                                                                   │
│  Servicios/                Controllers/                           │
│  AuthServicio ← NEW        AuthController ← NEW                  │
│  (BCrypt + JWT)            ProductosController ← NEW             │
│                            CategoriasController ← NEW            │
│  Middleware/               CarritoController ← NEW               │
│  ManejadorErrores ← NEW    OrdenesController ← NEW               │
│                                                                   │
│  Program.cs ← REESCRITO    appsettings.json ← MODIFICADO         │
│  requests.http ← NEW                                             │
│                                                                   │
│  Paquetes NuGet:                                                  │
│  - JwtBearer 10.0.5                                               │
│  - Swashbuckle 6.9.0                                              │
│  - BCrypt.Net-Next 4.1.0                                          │
└──────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│                EcommerceNet.Tests                        │
│  (referencia solo Core — sin cambios)                   │
│  23/23 pruebas pasando                                  │
└────────────────────────────────────────────────────────┘
```

---

## 7. Decisiones técnicas y por qué

### 7.1 ¿Por qué BCrypt y no SHA256 o MD5?

| Algoritmo | Tipo | Seguridad | Por qué NO usarlo |
|-----------|------|-----------|------------------|
| MD5 | Hash rápido | ❌ Roto desde 2004 | Rainbow tables lo rompen en segundos |
| SHA-256 | Hash rápido | ❌ Inseguro para contraseñas | Es rápido — un atacante puede probar millones de hashes por segundo |
| BCrypt | Hash lento | ✅ Diseñado para contraseñas | Incorpora salt automático + work factor configurable |

BCrypt tiene un "work factor" (costo computacional). A mayor factor, más tiempo tarda en calcular el hash. Esto hace que un ataque de fuerza bruta sea impráctico incluso con hardware moderno. `BCrypt.Net-Next` usa factor 10 por defecto (≈100ms por hash — insignificante para usuarios, catastrófico para atacantes).

### 7.2 ¿Por qué `AddScoped` y no `AddTransient` o `AddSingleton`?

| Ciclo de vida | Cuándo crear instancia | Úsalo para |
|---------------|------------------------|-----------|
| `AddTransient` | Cada vez que se solicita | Servicios sin estado, generadores de PDFs |
| `AddScoped` | Una por petición HTTP | **Repositorios, servicios de negocio, DbContext** |
| `AddSingleton` | Una para toda la app | Configuración, caché en memoria |

Los servicios `IAuthServicio` y `ICarritoServicio` se registran como `Scoped` porque:
- Comparten la misma instancia durante toda una petición HTTP (coherencia de datos)
- Se destruyen al final de la petición (no hay memory leaks)
- Si se registraran como `Singleton` y se inyectara un `DbContext` (que es `Scoped`), habría una excepción de "captive dependency" en tiempo de ejecución

### 7.3 ¿Por qué `AuthServicio` en `API/Servicios/` y no en `Core/Servicios/`?

El plan `dia-02-aspnet-api.md` sugería poner `AuthServicio` en `Core/Servicios/`. Sin embargo, `CLAUDE.md` establece como regla estricta que **Core no puede tener paquetes NuGet externos**. `AuthServicio` necesita:
- `BCrypt.Net-Next` (para hash de contraseñas)
- `Microsoft.IdentityModel.Tokens` (para generar JWT)

Ambos son paquetes externos. La solución correcta de Clean Architecture es:
- `IAuthServicio` → **Core** (solo define el contrato, sin dependencias externas)
- `AuthServicio` → **API** (implementa el contrato usando tecnologías concretas)

`CLAUDE.md` tiene precedencia sobre el plan del día.

### 7.4 ¿Por qué el middleware de errores es el PRIMER middleware del pipeline?

```csharp
// CORRECTO — atrapa errores de TODOS los middlewares siguientes
app.UseMiddleware<ManejadorErroresMiddleware>();
app.UseHttpsRedirection();
app.UseCors("PermitirVue");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Si se colocara después de `UseAuthentication`, los errores lanzados en la capa de autenticación no serían atrapados por el middleware, y el cliente recibiría HTML de error del servidor en lugar de JSON.

### 7.5 ¿Por qué `UseAuthentication` va antes de `UseAuthorization`?

El pipeline de middleware es secuencial. `UseAuthentication` lee el header `Authorization: Bearer {token}`, lo valida y puebla `HttpContext.User` con los claims. `UseAuthorization` luego consulta `HttpContext.User` para verificar si tiene los permisos requeridos por `[Authorize]`.

Si el orden se invierte, `UseAuthorization` ejecuta antes de que `HttpContext.User` tenga datos — resultado: todos los endpoints protegidos devuelven 401.

### 7.6 ¿Por qué el mensaje de error de login es genérico?

```csharp
// MAL — permite enumeración de usuarios
if (usuario == null)
    return Resultado.Error("El email no está registrado");
if (!BCrypt.Verify(...))
    return Resultado.Error("Contraseña incorrecta");

// BIEN — mensaje genérico
if (usuario == null || !BCrypt.Verify(...))
    return Resultado.Error("Credenciales incorrectas");
```

Si el mensaje diferencia entre "email no encontrado" y "contraseña incorrecta", un atacante puede descubrir qué emails están registrados en el sistema simplemente probando emails. Este ataque se llama "enumeración de usuarios" y es una vulnerabilidad documentada en OWASP.

### 7.7 ¿Por qué `Forbid()` (403) en lugar de `NotFound()` (404) para órdenes de otros usuarios?

```csharp
if (orden.UsuarioId != ObtenerUsuarioId())
    return Forbid();  // 403
```

Ambos son válidos dependiendo del contexto de seguridad:
- **404**: Oculta que el recurso existe (más seguro contra enumeración)
- **403**: Reconoce que existe pero el usuario no tiene permiso

Se eligió 403 porque el endpoint ya requiere autenticación. Un atacante anónimo jamás llega a esta verificación. Para usuarios autenticados, saber que una orden existe (sin poder ver su contenido) no es una vulnerabilidad significativa.

---

## 8. Errores que NO ocurrieron (y cómo se evitaron)

### 8.1 Circular reference en JSON

**El error que no pasó:** Al serializar una entidad como `Producto` que tiene `Categoria`, y `Categoria` tiene `List<Producto>`, el serializador entraría en un loop infinito.

**Cómo se evitó:** Los controladores nunca devuelven entidades. Siempre mapean a DTOs (`ProductoDto`, `OrdenDto`, etc.) que no tienen referencias circulares.

### 8.2 `UseAuthorization` antes de `UseAuthentication`

**El error que no pasó:** Registrar `app.UseAuthorization()` antes de `app.UseAuthentication()` haría que todos los endpoints `[Authorize]` fallaran con 401.

**Cómo se evitó:** El orden en Program.cs sigue estrictamente la secuencia correcta documentada en el plan del día.

### 8.3 Token JWT con clave de menos de 256 bits

**El error que no pasó:** HMAC-SHA256 requiere una clave de mínimo 256 bits (32 caracteres). Una clave más corta causa una excepción `ArgumentOutOfRangeException` en tiempo de ejecución.

**Cómo se evitó:** La clave en `appsettings.json` tiene 47 caracteres = 376 bits.

### 8.4 Rompimiento de pruebas del Día 1

**El error que no pasó:** Modificar `IUnidadDeTrabajo` podría haber roto las 23 pruebas del Día 1 si alguna de ellas usara esa interfaz.

**Cómo se evitó:** Las pruebas del Día 1 prueban entidades puras (`Producto`, `Carrito`, `Orden`) sin instanciar `IUnidadDeTrabajo`. Se verificó con `dotnet test` después del cambio: 23/23 pruebas siguieron pasando.

---

## 9. Error que SÍ ocurrió y cómo se resolvió

### 9.1 `error CS0234: The type or namespace name 'Models' does not exist in the namespace 'Microsoft.OpenApi'`

**Causa:** Al instalar `Swashbuckle.AspNetCore` 10.1.7, se descargó con él `Microsoft.OpenApi` 2.x. En esta versión, Microsoft eliminó el sub-namespace `Microsoft.OpenApi.Models` — todos los tipos ahora viven directamente en `Microsoft.OpenApi`. Adicionalmente, el template `webapi` ya incluía `Microsoft.AspNetCore.OpenApi` 10.0.3 que también trae `Microsoft.OpenApi` 2.x.

**Diagnóstico:** El error apuntaba exactamente a la línea `using Microsoft.OpenApi.Models;` en `Program.cs`.

**Solución en dos pasos:**

1. Bajar `Swashbuckle.AspNetCore` de 10.1.7 a **6.9.0** (versión estable que usa `Microsoft.OpenApi` 1.x donde `Models` sí existe)
2. Eliminar `Microsoft.AspNetCore.OpenApi` 10.0.3 del `.csproj` (era el paquete del template que traía `Microsoft.OpenApi` 2.x y ganaba el conflicto de versiones)

**Resultado:** `dotnet build` con 0 errores, 0 warnings.

**Lección:** Cuando hay múltiples paquetes que dependen de la misma librería transitiva, el que pide la versión más alta gana. `Microsoft.AspNetCore.OpenApi` 10.0.3 pedía `Microsoft.OpenApi` 2.x y "ganaba" sobre `Swashbuckle` 6.9.0 que pedía 1.x. La solución fue eliminar el paquete que forzaba la versión incompatible.

---

## 10. Estado del proyecto al cierre del Día 2

### Árbol de archivos relevantes (src/)

```
src/
├── EcommerceNet.Core/
│   ├── DTOs/
│   │   ├── AuthDtos.cs          ← NUEVO
│   │   ├── CarritoDto.cs
│   │   ├── OrdenDto.cs
│   │   ├── ProductoDto.cs
│   │   └── Resultado.cs
│   ├── Entidades/
│   │   ├── Carrito.cs
│   │   ├── CarritoItem.cs
│   │   ├── Categoria.cs
│   │   ├── Orden.cs
│   │   ├── OrdenDetalle.cs
│   │   ├── Producto.cs
│   │   └── Usuario.cs
│   ├── Enums/
│   │   ├── EstadoOrden.cs
│   │   └── RolUsuario.cs
│   ├── Interfaces/
│   │   ├── ICarritoRepositorio.cs
│   │   ├── IOrdenRepositorio.cs
│   │   ├── IProductoRepositorio.cs
│   │   ├── IRepositorio.cs
│   │   ├── IUnidadDeTrabajo.cs  ← MODIFICADO (+ Usuarios)
│   │   └── IUsuarioRepositorio.cs  ← NUEVO
│   └── Servicios/
│       ├── CarritoServicio.cs
│       ├── IAuthServicio.cs     ← NUEVO
│       └── ICarritoServicio.cs
│
├── EcommerceNet.Data/           ← pendiente (Día 3)
│
└── EcommerceNet.API/
    ├── Controllers/
    │   ├── AuthController.cs        ← NUEVO
    │   ├── CarritoController.cs     ← NUEVO
    │   ├── CategoriasController.cs  ← NUEVO
    │   ├── OrdenesController.cs     ← NUEVO
    │   └── ProductosController.cs   ← NUEVO
    ├── Middleware/
    │   └── ManejadorErroresMiddleware.cs  ← NUEVO
    ├── Servicios/
    │   └── AuthServicio.cs         ← NUEVO
    ├── appsettings.json            ← MODIFICADO
    ├── EcommerceNet.API.csproj     ← MODIFICADO
    ├── Program.cs                  ← REESCRITO
    └── requests.http               ← NUEVO
```

### Resultado de `dotnet build`

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Resultado de `dotnet test`

```
Total tests: 23
     Passed: 23
```

### Endpoints disponibles al cierre del Día 2

| Método | Ruta | Auth | Estado |
|--------|------|------|--------|
| POST | `/api/auth/registrar` | No | ✅ Definido (requiere UoW en Día 3) |
| POST | `/api/auth/login` | No | ✅ Definido (requiere UoW en Día 3) |
| GET | `/api/productos` | No | ✅ Definido (requiere UoW en Día 3) |
| GET | `/api/productos/{id}` | No | ✅ Definido |
| GET | `/api/productos/buscar` | No | ✅ Definido |
| GET | `/api/productos/categoria/{id}` | No | ✅ Definido |
| POST | `/api/productos` | Admin | ✅ Definido |
| PUT | `/api/productos/{id}` | Admin | ✅ Definido |
| DELETE | `/api/productos/{id}` | Admin | ✅ Definido |
| GET | `/api/categorias` | No | ✅ Placeholder |
| POST | `/api/categorias` | Admin | ✅ Placeholder |
| GET | `/api/carrito` | Sí | ✅ Definido |
| POST | `/api/carrito/agregar` | Sí | ✅ Definido |
| PUT | `/api/carrito/{productoId}` | Sí | ✅ Definido |
| DELETE | `/api/carrito/{productoId}` | Sí | ✅ Definido |
| DELETE | `/api/carrito` | Sí | ✅ Definido |
| POST | `/api/carrito/checkout` | Sí | ✅ Definido |
| GET | `/api/ordenes` | Sí | ✅ Definido |
| GET | `/api/ordenes/{id}` | Sí | ✅ Definido |
| PUT | `/api/ordenes/{id}/cancelar` | Sí | ✅ Definido |

---

## 11. Pendientes para el Día 3

### Día 3: EF Core + SQL Server + MongoDB

| Tarea | Archivo/Capa | Prioridad |
|-------|-------------|-----------|
| Crear `AppDbContext` con Fluent API | `Data/AppDbContext.cs` | Alta |
| Implementar repositorios concretos (5) | `Data/Repositorios/` | Alta |
| Implementar `UnidadDeTrabajo` | `Data/UnidadDeTrabajo.cs` | Alta |
| Registrar `AppDbContext` y `IUnidadDeTrabajo` en DI | `API/Program.cs` | Alta |
| Crear primera migración EF Core | Terminal | Alta |
| Agregar seed data (categorías y productos) | `AppDbContext.cs` | Media |
| Implementar `CategoriasController` completo | `API/Controllers/` | Media |
| Agregar `ICategoriaRepositorio` a Core | `Core/Interfaces/` | Media |
| Integración MongoDB para búsqueda | `Data/MongoDB/` | Baja |
| Documentar todos los endpoints con XML | `.csproj` + comentarios | Baja |
