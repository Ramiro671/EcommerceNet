# Manual Técnico — Día 3: EF Core + SQL Server + MongoDB

> **Fecha de ejecución:** 2026-04-05

> **Entorno:** Windows 11, .NET SDK 10.0.103, EF Core Tools 10.0.5, Git 2.51.1
> **Resultado final:** Build exitoso, 23/23 pruebas pasando, BD creada, 12 productos en seed data, API respondiendo

---

## Índice

1. [Revisión previa al desarrollo](#1-revisión-previa-al-desarrollo)
2. [Fase 1 — Instalación de paquetes NuGet](#2-fase-1--instalación-de-paquetes-nuget)
3. [Fase 2 — AppDbContext: Fluent API y seed data](#3-fase-2--appdbcontext-fluent-api-y-seed-data)
4. [Fase 3 — Repositorios concretos con EF Core](#4-fase-3--repositorios-concretos-con-ef-core)
5. [Fase 4 — UnidadDeTrabajo y AuthServicio en Data](#5-fase-4--unidaddetrabajo-y-authservicio-en-data)
6. [Fase 5 — MongoDB: historial de búsquedas](#6-fase-5--mongodb-historial-de-búsquedas)
7. [Fase 6 — Actualización de Program.cs y controladores](#7-fase-6--actualización-de-programcs-y-controladores)
8. [Fase 7 — Migraciones y base de datos](#8-fase-7--migraciones-y-base-de-datos)
9. [Inventario completo de archivos creados/modificados](#9-inventario-completo-de-archivos-creadosmodificados)
10. [Grafo de dependencias actualizado](#10-grafo-de-dependencias-actualizado)
11. [Decisiones técnicas y por qué](#11-decisiones-técnicas-y-por-qué)
12. [SQL generado por las migraciones](#12-sql-generado-por-las-migraciones)
13. [Error que SÍ ocurrió y cómo se resolvió](#13-error-que-sí-ocurrió-y-cómo-se-resolvió)
14. [Errores que NO ocurrieron y cómo se evitaron](#14-errores-que-no-ocurrieron-y-cómo-se-evitaron)
15. [Estado del proyecto al cierre del Día 3](#15-estado-del-proyecto-al-cierre-del-día-3)
16. [Pendientes para el Día 4](#16-pendientes-para-el-día-4)

---

## 1. Revisión previa al desarrollo

Se revisaron los siguientes archivos antes de comenzar el desarrollo:

### 1.1 Convenciones de arquitectura del proyecto

| Regla | Efecto en el código del Día 3 |
|-------|-------------------------------|
| `Core` no depende de nada externo | EF Core, MongoDB y BCrypt van en `Data`, nunca en `Core` |
| `Data` depende de `Core` solamente | Los repositorios implementan interfaces de Core |
| Namespaces file-scoped | Todos los archivos nuevos usan `namespace X;` |
| Patrón Repository + Unit of Work | Se implementan 5 repositorios concretos + UnidadDeTrabajo |
| Inyección de dependencias por constructor | AppDbContext llega por constructor a cada repositorio |
| Comentarios en español | Todos los `/// <summary>` y comentarios inline en español |

### 1.2 `docs/dia-03-datos.md` (plan del día)

Leyó el archivo en 7 bloques (supera el límite de ~10.000 tokens) y extrajo:
- Los 6 comandos de paquetes NuGet (4 en Data, 1 en API, 1 adicional BCrypt en Data)
- El código completo de AppDbContext con Fluent API
- Los 5 repositorios concretos con sus queries LINQ
- UnidadDeTrabajo con operador `??=`
- AuthServicio en `Data/Servicios/` (no en `API/Servicios/` como en Día 2)
- Los dos archivos de MongoDB
- Los cambios a Program.cs y ProductosController

### 1.3 `docs/dia-02-manual-tecnico.md` (estado del proyecto post-Día 2)

Confirmó qué existía: `API/Servicios/AuthServicio.cs` (versión sin BD real), el `Program.cs` con los TODO comentados para el Día 3, y la connection string sin `TrustServerCertificate=True`.

### 1.4 `docs/dia-01-clase-programacion.md` y `docs/dia-01-manual-tecnico.md`

Leídos para replicar el formato y nivel de detalle de la documentación.

---

## 2. Fase 1 — Instalación de paquetes NuGet

> El usuario confirmó el bloque completo antes de ejecutar.

### 2.1 Comandos ejecutados

```bash
# Proyecto Data — donde vive toda la infraestructura de datos
dotnet add src/EcommerceNet.Data/EcommerceNet.Data.csproj package Microsoft.EntityFrameworkCore
dotnet add src/EcommerceNet.Data/EcommerceNet.Data.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/EcommerceNet.Data/EcommerceNet.Data.csproj package Microsoft.EntityFrameworkCore.Tools
dotnet add src/EcommerceNet.Data/EcommerceNet.Data.csproj package MongoDB.Driver
dotnet add src/EcommerceNet.Data/EcommerceNet.Data.csproj package BCrypt.Net-Next

# Proyecto API — herramienta de scaffolding de migraciones
dotnet add src/EcommerceNet.API/EcommerceNet.API.csproj package Microsoft.EntityFrameworkCore.Design
```

### 2.2 Paquetes instalados

| Paquete | Versión | Proyecto | Propósito |
|---------|---------|----------|-----------|
| `Microsoft.EntityFrameworkCore` | 10.0.5 | Data | Motor principal del ORM |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.5 | Data | Proveedor SQL Server / LocalDB |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.5 | Data | Comandos `ef migrations` |
| `MongoDB.Driver` | 3.7.1 | Data | Driver oficial de MongoDB para .NET |
| `BCrypt.Net-Next` | 4.1.0 | Data | Hash de contraseñas (movido de API a Data) |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.5 | API | Necesario para `dotnet ef` desde el proyecto de startup |

### 2.3 Por qué BCrypt está tanto en Data como en API

El Día 2 se instaló `BCrypt.Net-Next` en API porque `AuthServicio` vivía ahí. En el Día 3, `AuthServicio` se mueve a `Data/Servicios/`, por lo que BCrypt se instala en Data. El paquete en API queda como dependencia transitiva y no genera conflicto.

---

## 3. Fase 2 — AppDbContext: Fluent API y seed data

### 3.1 Archivo creado: `src/EcommerceNet.Data/AppDbContext.cs`

**Responsabilidades:**
- Define los 7 `DbSet<T>` (uno por entidad = una tabla)
- Configura relaciones, restricciones y tipos de columna vía Fluent API
- Inserta datos iniciales (seed data) con `HasData()`

### 3.2 Configuraciones Fluent API relevantes

| Entidad | Configuraciones clave |
|---------|----------------------|
| `Producto` | `decimal(18,2)` para Precio, índice en Nombre, `DeleteBehavior.Restrict` hacia Categoria |
| `Usuario` | Índice UNIQUE en Email |
| `Carrito` | Relación 1:1 con Usuario vía `HasOne.WithOne`, `DeleteBehavior.Cascade` |
| `CarritoItem` | `decimal(18,2)` para PrecioUnitario, Cascade hacia Carrito, Restrict hacia Producto |
| `Orden` | `decimal(18,2)` para Total, índice en NumeroOrden, Restrict hacia Usuario |
| `OrdenDetalle` | `decimal(18,2)` para PrecioUnitario y Subtotal, Cascade hacia Orden, Restrict hacia Producto |

### 3.3 Seed data

| Entidad | Cantidad | Detalle |
|---------|----------|---------|
| `Categoria` | 5 | Electrónica, Ropa, Hogar, Deportes, Libros |
| `Producto` | 12 | 4 en Electrónica, 2 en Ropa, 2 en Hogar, 2 en Deportes, 2 en Libros |
| `Usuario` | 1 | Admin Tienda — admin@ecommercenet.com / Admin123! |

### 3.4 Decisión crítica: hash constante para seed data

**Problema:** El plan original usaba `BCrypt.Net.BCrypt.HashPassword("Admin123!")` dentro de `AgregarDatosIniciales()`. BCrypt genera un salt aleatorio en cada llamada → cada vez que EF Tools ejecuta `OnModelCreating()` (en un nuevo proceso) obtenía un hash diferente → EF Core detectaba "pending model changes" y rechazaba `database update`.

**Solución:** Usar `private const string HashAdminFijo = "hash-pre-computado"`. El hash se generó una sola vez ejecutando un programa de consola temporal:

```csharp
// Programa temporal para generar el hash
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin123!", 11));
// Output: $2a$11$6UzHJUoBbxgCefygc7iWkO3B9TgR5j28FMElzcMhOG3tDHqYZaMLu
```

Este hash es estable entre ejecuciones y `BCrypt.Verify("Admin123!", hash)` lo valida correctamente.

---

## 4. Fase 3 — Repositorios concretos con EF Core

### 4.1 Archivos creados

```
src/EcommerceNet.Data/Repositorios/
├── RepositorioBase.cs        — CRUD genérico con DbSet<T>
├── ProductoRepositorio.cs    — queries especializados con Include
├── CarritoRepositorio.cs     — Include + ThenInclude anidado
├── OrdenRepositorio.cs       — historial de órdenes con detalles
└── UsuarioRepositorio.cs     — búsqueda por email
```

### 4.2 Jerarquía de herencia

```
IRepositorio<T>  (Core)
    └── RepositorioBase<T>  (Data)
            ├── ProductoRepositorio : IProductoRepositorio
            ├── OrdenRepositorio    : IOrdenRepositorio
            └── UsuarioRepositorio  : IUsuarioRepositorio

ICarritoRepositorio  (Core — métodos propios, no hereda IRepositorio<T>)
    └── CarritoRepositorio  (Data)
```

`CarritoRepositorio` no hereda `RepositorioBase<Carrito>` porque su API pública es diferente: solo expone 3 métodos (`ObtenerPorUsuarioAsync`, `AgregarAsync`, `Actualizar`) en lugar del CRUD completo.

### 4.3 Queries con Include — impacto en SQL

| Método | Include | SQL generado (simplificado) |
|--------|---------|----------------------------|
| `ObtenerActivosAsync()` | `p.Categoria` | `SELECT ... FROM Productos JOIN Categorias` |
| `ObtenerPorUsuarioAsync()` | `Items → Producto → Categoria` | Triple JOIN |
| `ObtenerConDetallesAsync()` | `Detalles → Producto` + `Usuario` | Dos JOINs |

### 4.4 Override de `ObtenerPorIdAsync` en `ProductoRepositorio`

La versión base usa `FindAsync(id)` que NO carga relaciones (devuelve Producto sin Categoria). El override:

```csharp
public override async Task<Producto?> ObtenerPorIdAsync(int id)
{
    return await _contexto.Productos
        .Include(p => p.Categoria)
        .FirstOrDefaultAsync(p => p.Id == id);
}
```

Usa `virtual/override` para mantener el polimorfismo: el controlador llama `_uow.Productos.ObtenerPorIdAsync(id)` y siempre obtiene el producto con su categoría, sin saber qué implementación se usa.

---

## 5. Fase 4 — UnidadDeTrabajo y AuthServicio en Data

### 5.1 Archivo creado: `src/EcommerceNet.Data/UnidadDeTrabajo.cs`

Implementa `IUnidadDeTrabajo` con lazy initialization usando `??=`:

```csharp
public IProductoRepositorio Productos =>
    _productos ??= new ProductoRepositorio(_contexto);
```

**Por qué lazy:** Los controladores solo usan el repositorio que necesitan. Un `CarritoController` no debería instanciar `ProductoRepositorio` ni `OrdenRepositorio`. Con `??=`, el repositorio se crea solo cuando se accede por primera vez.

**Por qué todos comparten el mismo `_contexto`:** EF Core rastrea los cambios en el contexto. Si `Productos` y `Carritos` usaran contextos diferentes, los cambios de uno no serían visibles para el otro en la misma transacción.

### 5.2 Archivos movidos/creados: `AuthServicio`

| Día 2 | Día 3 |
|-------|-------|
| `API/Servicios/AuthServicio.cs` | **Eliminado** |
| — | `Data/Servicios/AuthServicio.cs` (nuevo) |

**Razón del movimiento:** El `AuthServicio` del Día 2 dependía de `IUnidadDeTrabajo` para buscar usuarios. El nuevo `AuthServicio` usa `AppDbContext` directamente, que es más eficiente y apropiado para una capa de datos. Además, el Día 3 registra `IAuthServicio → AuthServicio` de `Data`, lo que hace innecesaria la versión de `API`.

---

## 6. Fase 5 — MongoDB: historial de búsquedas

### 6.1 Archivos creados

```
src/EcommerceNet.Data/MongoDB/
├── BusquedaHistorial.cs          — documento (clase C# mapeada a colección)
└── HistorialBusquedaServicio.cs  — operaciones CRUD + agregación
```

### 6.2 Estructura del documento

```json
{
  "_id": ObjectId("..."),
  "termino": "laptop",
  "usuarioId": 5,
  "resultadosEncontrados": 3,
  "fecha": ISODate("2026-04-05T05:15:00Z")
}
```

### 6.3 Pipeline de agregación para términos populares

```csharp
_coleccion.Aggregate()
    .Group(b => b.Termino, g => new TerminoPopular {
        Termino = g.Key,
        TotalBusquedas = g.Count()
    })
    .SortByDescending(t => t.TotalBusquedas)
    .Limit(top)
    .ToListAsync()
```

Equivalente MongoDB shell:
```js
db.busquedas.aggregate([
  { $group: { _id: "$termino", total: { $sum: 1 } } },
  { $sort: { total: -1 } },
  { $limit: 10 }
])
```

### 6.4 Registro como Singleton

```csharp
builder.Services.AddSingleton<HistorialBusquedaServicio>();
```

`MongoClient` internamente maneja un pool de conexiones y está diseñado para ser reutilizado entre requests. Si se registrara como `Scoped`, se crearía una nueva instancia (y un nuevo pool) por request — ineficiente y potencialmente problemático bajo carga.

---

## 7. Fase 6 — Actualización de Program.cs y controladores

### 7.1 Cambios en `Program.cs`

Líneas añadidas en la sección de servicios:

```csharp
// Paquetes nuevos necesarios
using EcommerceNet.Core.Interfaces;
using EcommerceNet.Data;
using EcommerceNet.Data.MongoDB;
using EcommerceNet.Data.Servicios;
using Microsoft.EntityFrameworkCore;

// DbContext — Scoped (una conexión por request)
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios registrados
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();      // ← apunta a Data.Servicios ahora
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();
builder.Services.AddSingleton<HistorialBusquedaServicio>();    // ← Singleton por MongoClient
```

Los `using` de `EcommerceNet.API.Servicios` fueron reemplazados por `EcommerceNet.Data.Servicios`.

### 7.2 Cambios en `ProductosController.cs`

- Nuevo parámetro en constructor: `HistorialBusquedaServicio historial`
- Nuevo método privado `ObtenerUsuarioId()` — lee `ClaimTypes.NameIdentifier` del JWT (null si no autenticado)
- En el endpoint `Buscar`: se registra la búsqueda en MongoDB después de obtener resultados

```csharp
// Fire-and-forget intencional — no bloqueamos la respuesta si MongoDB falla
_ = _historial.RegistrarBusquedaAsync(termino, usuarioId, lista.Count);
```

### 7.3 Cambios en `appsettings.json`

```json
// Añadido TrustServerCertificate=True a la connection string
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;...;TrustServerCertificate=True;..."

// Añadida sección MongoDB
"MongoDB": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "EcommerceNetDB"
}
```

`TrustServerCertificate=True` es necesario con .NET 10 + LocalDB porque LocalDB no tiene un certificado SSL válido de CA. Sin esta flag, la conexión se rechaza con un error de certificado.

---

## 8. Fase 7 — Migraciones y base de datos

### 8.1 Comandos ejecutados

```bash
# Desde src/EcommerceNet.API (proyecto de startup con Design)
cd src/EcommerceNet.API

# Intento 1 — falló (ver sección 13)
dotnet ef migrations add CreacionInicial --project ../EcommerceNet.Data

# Corrección del hash (ver sección 13)
dotnet ef migrations remove --project ../EcommerceNet.Data --force

# Intento 2 — exitoso
dotnet ef migrations add CreacionInicial --project ../EcommerceNet.Data

# Aplicar a SQL Server LocalDB
dotnet ef database update --project ../EcommerceNet.Data
```

### 8.2 Archivos generados por EF Core en `EcommerceNet.Data/Migrations/`

```
Migrations/
├── 20260405051022_CreacionInicial.cs        — snapshot del esquema
├── 20260405051022_CreacionInicial.Designer.cs — metadata de EF Core
└── AppDbContextModelSnapshot.cs              — estado actual del modelo
```

### 8.3 Resultado del `database update`

- Base de datos `EcommerceNetDB` creada en `(localdb)\MSSQLLocalDB`
- 7 tablas creadas: Categorias, Usuarios, Productos, Carritos, CarritoItems, Ordenes, OrdenDetalles
- Tabla `__EFMigrationsHistory` creada (registro de migraciones aplicadas)
- Seed data insertado: 5 categorías + 12 productos + 1 usuario admin
- 10 índices creados

---

## 9. Inventario completo de archivos creados/modificados

### 9.1 Archivos NUEVOS

| Archivo | Capa | Descripción |
|---------|------|-------------|
| `src/EcommerceNet.Data/AppDbContext.cs` | Data | DbContext con Fluent API y seed data |
| `src/EcommerceNet.Data/UnidadDeTrabajo.cs` | Data | Implementación de IUnidadDeTrabajo |
| `src/EcommerceNet.Data/Repositorios/RepositorioBase.cs` | Data | CRUD genérico |
| `src/EcommerceNet.Data/Repositorios/ProductoRepositorio.cs` | Data | Queries especializados de productos |
| `src/EcommerceNet.Data/Repositorios/CarritoRepositorio.cs` | Data | Include anidado para carrito |
| `src/EcommerceNet.Data/Repositorios/OrdenRepositorio.cs` | Data | Historial de órdenes |
| `src/EcommerceNet.Data/Repositorios/UsuarioRepositorio.cs` | Data | Búsqueda por email |
| `src/EcommerceNet.Data/Servicios/AuthServicio.cs` | Data | Auth con BD real (BCrypt + JWT) |
| `src/EcommerceNet.Data/MongoDB/BusquedaHistorial.cs` | Data | Documento MongoDB |
| `src/EcommerceNet.Data/MongoDB/HistorialBusquedaServicio.cs` | Data | Servicio MongoDB |
| `src/EcommerceNet.Data/Migrations/20260405051022_CreacionInicial.cs` | Data | Migración inicial |
| `src/EcommerceNet.Data/Migrations/AppDbContextModelSnapshot.cs` | Data | Snapshot del modelo |

### 9.2 Archivos MODIFICADOS

| Archivo | Cambio |
|---------|--------|
| `src/EcommerceNet.Data/EcommerceNet.Data.csproj` | +5 PackageReference |
| `src/EcommerceNet.API/EcommerceNet.API.csproj` | +1 PackageReference (Design) |
| `src/EcommerceNet.API/Program.cs` | AddDbContext + UoW + MongoDB Singleton |
| `src/EcommerceNet.API/Controllers/ProductosController.cs` | +HistorialBusquedaServicio en Buscar |
| `src/EcommerceNet.API/appsettings.json` | +TrustServerCertificate + sección MongoDB |

### 9.3 Archivos ELIMINADOS

| Archivo | Razón |
|---------|-------|
| `src/EcommerceNet.API/Servicios/AuthServicio.cs` | Reemplazado por `Data/Servicios/AuthServicio.cs` |

---

## 10. Grafo de dependencias actualizado

```
┌─────────────────────────────────────────────────────┐
│  EcommerceNet.Tests                                  │
│  (xUnit, 23 pruebas)                                 │
│  Depende de: Core solamente ✓                        │
└──────────────────────┬──────────────────────────────┘
                       │ referencia
                       ▼
┌─────────────────────────────────────────────────────┐
│  EcommerceNet.Core                                   │
│  Entidades, Interfaces, DTOs, Servicios              │
│  Paquetes externos: NINGUNO ✓                        │
└──────────────────────┬──────────────────────────────┘
                       │ referencia
                       ▼
┌─────────────────────────────────────────────────────┐
│  EcommerceNet.Data                          [DÍA 3]  │
│  AppDbContext, Repositorios, UnidadDeTrabajo         │
│  AuthServicio (Data/Servicios/)                      │
│  HistorialBusquedaServicio (MongoDB/)                │
│  Paquetes: EF Core 10.0.5, SqlServer, Tools          │
│            MongoDB.Driver 3.7.1, BCrypt 4.1.0        │
└──────────────────────┬──────────────────────────────┘
                       │ referencia
                       ▼
┌─────────────────────────────────────────────────────┐
│  EcommerceNet.API                                    │
│  Controllers, Middleware                             │
│  Paquetes: JwtBearer, Swashbuckle, EFCore.Design     │
└─────────────────────────────────────────────────────┘
```

**Cambio respecto al Día 2:** Data ya no es un proyecto vacío. Ahora contiene toda la infraestructura: EF Core, repositorios, MongoDB y el `AuthServicio` real.

---

## 11. Decisiones técnicas y por qué

### 11.1 ¿Por qué `AddDbContext` es Scoped?

```csharp
builder.Services.AddDbContext<AppDbContext>(opciones => ...);
// AddDbContext registra como Scoped por defecto
```

`DbContext` mantiene un **caché de entidades** (change tracker). Si fuera Singleton, todos los requests compartirían el mismo contexto → problemas de concurrencia (dos requests modificando la misma entidad simultáneamente). Si fuera Transient, se perdería el change tracker entre operaciones del mismo request.

**Scoped = una instancia por request HTTP** → el contexto vive mientras dura la petición, luego se destruye.

### 11.2 ¿Por qué `HistorialBusquedaServicio` es Singleton?

`MongoClient` es thread-safe por diseño y gestiona un pool de conexiones internamente. La documentación oficial de MongoDB recomienda explícitamente instanciarlo una sola vez por aplicación. Con `Singleton`, el pool se inicializa en el primer request y se reutiliza en todos los siguientes.

### 11.3 ¿Por qué `DeleteBehavior.Restrict` en Producto → Categoría?

```csharp
e.HasOne(p => p.Categoria)
 .WithMany(c => c.Productos)
 .OnDelete(DeleteBehavior.Restrict);
```

Si una categoría tiene productos y se intentara borrar, SQL Server generaría un error. Esto es intencional: **no queremos borrar categorías que tienen productos activos**. La alternativa (Cascade) borraría todos los productos al borrar la categoría — demasiado destructivo para un e-commerce.

`Cascade` sí se usa donde tiene sentido: borrar un usuario elimina su carrito automáticamente, y borrar una orden elimina sus detalles.

### 11.4 ¿Por qué `decimal(18,2)` para precios?

SQL Server tiene tres tipos para decimales: `float` (impreciso), `money` (propietario de SQL Server) y `decimal(p,s)` (estándar ANSI). `decimal(18,2)` significa:
- 18 dígitos significativos en total
- 2 dígitos después del punto decimal
- Rango: hasta $9,999,999,999,999,999.99

`float` NO se usa para dinero porque es un tipo de punto flotante — puede generar errores de redondeo (`0.1 + 0.2 = 0.30000000000000004`). Para precios, la precisión exacta es obligatoria.

### 11.5 ¿Por qué `AuthServicio` pasó de `API` a `Data`?

En el Día 2, `AuthServicio` vivía en `API/Servicios/` y dependía de `IUnidadDeTrabajo`. El problema: `IUnidadDeTrabajo` no tenía implementación real, así que el servicio compilaba pero fallaba en runtime.

En el Día 3, `AuthServicio` usa `AppDbContext` directamente — acceso más eficiente (evita la capa extra del UoW para operaciones simples de auth) y correcto arquitectónicamente (acceder a la BD es responsabilidad de la capa Data).

### 11.6 ¿Por qué `??=` en lugar de verificación manual?

```csharp
// Con ??= (conciso)
public IProductoRepositorio Productos => _productos ??= new ProductoRepositorio(_contexto);

// Sin ??= (equivalente pero verboso)
public IProductoRepositorio Productos
{
    get
    {
        if (_productos == null)
            _productos = new ProductoRepositorio(_contexto);
        return _productos;
    }
}
```

Ambos son idénticos en comportamiento. `??=` (null-coalescing assignment) es azúcar sintáctica de C# 8+ que expresa la intención más claramente.

---

## 12. SQL generado por las migraciones

EF Core generó estas tablas (extracto del log de `database update`):

```sql
-- Tabla Categorias
CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,       -- HasMaxLength(100)
    [Descripcion] nvarchar(500) NOT NULL,  -- HasMaxLength(500)
    [Activa] bit NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
);

-- Tabla Productos (con FK hacia Categorias)
CREATE TABLE [Productos] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(200) NOT NULL,
    [Descripcion] nvarchar(1000) NOT NULL,
    [Precio] decimal(18,2) NOT NULL,       -- HasColumnType("decimal(18,2)")
    [Stock] int NOT NULL,
    [ImagenUrl] nvarchar(500) NOT NULL,
    [Activo] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [CategoriaId] int NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Productos_Categorias_CategoriaId]
        FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id])
        ON DELETE NO ACTION   -- DeleteBehavior.Restrict
);

-- Tabla Usuarios (con índice UNIQUE en Email)
CREATE TABLE [Usuarios] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Rol] int NOT NULL,
    [FechaRegistro] datetime2 NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);

-- Tabla Carritos (1:1 con Usuarios, Cascade)
CREATE TABLE [Carritos] (
    [Id] int NOT NULL IDENTITY,
    [UltimaModificacion] datetime2 NOT NULL,
    [UsuarioId] int NOT NULL,
    CONSTRAINT [PK_Carritos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Carritos_Usuarios_UsuarioId]
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id])
        ON DELETE CASCADE     -- DeleteBehavior.Cascade
);
CREATE UNIQUE INDEX [IX_Carritos_UsuarioId] ON [Carritos] ([UsuarioId]);
```

**Total: 7 tablas, 10 índices, 8 foreign keys, seed data de 18 filas.**

---

## 13. Error que SÍ ocurrió y cómo se resolvió

### Error: `PendingModelChangesWarning` al aplicar la migración

**Comando que falló:**
```
dotnet ef database update --project ../EcommerceNet.Data
```

**Mensaje de error:**
```
System.InvalidOperationException: An error was generated for warning
'Microsoft.EntityFrameworkCore.Migrations.PendingModelChangesWarning':
The model for context 'AppDbContext' has pending changes.
Add a new migration before updating the database.
```

**Causa raíz:**

En `AppDbContext.cs` se usó inicialmente:

```csharp
// PROBLEMÁTICO — BCrypt genera un salt aleatorio en cada llamada
private static readonly string HashAdminFijo =
    BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 11);
```

EF Core ejecuta `OnModelCreating()` dos veces:
1. Al crear la migración (`migrations add`) — genera hash A
2. Al aplicar la migración (`database update`) — genera hash B

Como A ≠ B, EF Core detecta que el modelo en el código no coincide con el snapshot de la migración → error.

**Solución en 3 pasos:**

1. Generar el hash una sola vez desde un programa temporal:
```bash
# Hash generado: $2a$11$6UzHJUoBbxgCefygc7iWkO3B9TgR5j28FMElzcMhOG3tDHqYZaMLu
```

2. Cambiar `static readonly` → `const` con el hash fijo:
```csharp
private const string HashAdminFijo =
    "$2a$11$6UzHJUoBbxgCefygc7iWkO3B9TgR5j28FMElzcMhOG3tDHqYZaMLu";
```

3. Eliminar la migración corrupta y recrearla:
```bash
dotnet ef migrations remove --project ../EcommerceNet.Data --force
dotnet ef migrations add CreacionInicial --project ../EcommerceNet.Data
dotnet ef database update --project ../EcommerceNet.Data  # ✓ exitoso
```

---

## 14. Errores que NO ocurrieron y cómo se evitaron

| Error potencial | Cómo se evitó |
|----------------|---------------|
| `TrustServerCertificate` error al conectar LocalDB | Se añadió `TrustServerCertificate=True` al connection string antes de las migraciones |
| Conflicto de múltiples DELETE CASCADE en SQL Server | Se usó `Restrict` en Producto→Categoría y Producto→CarritoItem/OrdenDetalle para evitar múltiples rutas de cascada |
| `using` faltante de `IUnidadDeTrabajo` en Program.cs | El hook de IDE detectó el error CS0234 inmediatamente y se corrigió agregando `using EcommerceNet.Core.Interfaces;` |
| Fechas dinámicas en seed data | Se usó `var fechaSeed = new DateTime(2026, 1, 1, ...)` en lugar de `DateTime.UtcNow` para evitar que EF detecte cambios en migraciones subsecuentes |
| AuthServicio registrado dos veces (API y Data) | Se eliminó `API/Servicios/AuthServicio.cs` antes de actualizar Program.cs |

---

## 15. Estado del proyecto al cierre del Día 3

```
dotnet build   → Build succeeded. 0 Warning(s). 0 Error(s).
dotnet test    → Passed! 23/23. Failed: 0. Skipped: 0.
dotnet run     → API escuchando en http://localhost:5152
GET /api/productos → 12 productos con categorías ✓
```

**Base de datos creada:** `EcommerceNetDB` en `(localdb)\MSSQLLocalDB`
- 7 tablas con datos
- 10 índices
- Migración registrada en `__EFMigrationsHistory`

**Estructura final del proyecto:**

```
EcommerceNet/
├── src/
│   ├── EcommerceNet.Core/         — Sin cambios del Día 3
│   ├── EcommerceNet.Data/         — COMPLETADO: AppDbContext, 5 repos, UoW, AuthServicio, MongoDB
│   │   ├── AppDbContext.cs
│   │   ├── UnidadDeTrabajo.cs
│   │   ├── Repositorios/ (5 archivos)
│   │   ├── Servicios/AuthServicio.cs
│   │   ├── MongoDB/ (2 archivos)
│   │   └── Migrations/ (3 archivos generados)
│   └── EcommerceNet.API/          — Program.cs y ProductosController actualizados
├── tests/
│   └── EcommerceNet.Tests/        — 23/23 pasando (sin cambios)
└── docs/
    ├── dia-01-*, dia-02-*         — Documentación días anteriores
    ├── dia-03-datos.md            — Plan del Día 3 (fuente)
    └── dia-03-manual-tecnico.md   — Este archivo
```

---

## 16. Pendientes para el Día 4

**Día 4: Frontend Vue.js 3**

- Crear proyecto Vue.js 3 en `src/EcommerceNet.Web/`
- Instalar Pinia, Vue Router, Axios
- Implementar `authStore` con JWT (login, registro, logout, persistencia en localStorage)
- Implementar `productoStore` y `carritoStore`
- Componentes: CatalogoProdutos, DetallProducto, CarritoLateral, Checkout, Login/Registro
- Interceptores Axios: adjuntar token JWT en cada request, manejar 401 → redirect login
- Página jQuery legacy: una página standalone `jquery-catalogo.html`
- Configurar CORS en API si se necesita puerto diferente al 5173
- Probar flujo completo: login → agregar al carrito → checkout → ver órdenes
