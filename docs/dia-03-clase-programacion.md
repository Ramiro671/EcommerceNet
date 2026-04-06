# Clase de Programación — Día 3: EF Core, SQL Server y MongoDB

> **A quién va dirigido:** desarrollador que ya conoce los conceptos del Día 1 (C#, clases, interfaces, async/await)
> y del Día 2 (controladores, JWT, middleware). Esta clase explica SOLO los conceptos NUEVOS del Día 3.
> Cada concepto parte de cero y progresa hasta el uso real en el proyecto.

---

## Índice

1. [Entity Framework Core — qué es un ORM](#1-entity-framework-core--qué-es-un-orm)
2. [DbContext — la sesión con la base de datos](#2-dbcontext--la-sesión-con-la-base-de-datos)
3. [DbSet\<T\> — la tabla en C#](#3-dbsett--la-tabla-en-c)
4. [OnModelCreating — configurar el esquema](#4-onmodelcreating--configurar-el-esquema)
5. [Fluent API — métodos de configuración](#5-fluent-api--métodos-de-configuración)
6. [DeleteBehavior — qué pasa al borrar](#6-deletebehavior--qué-pasa-al-borrar)
7. [Include y ThenInclude — eager loading](#7-include-y-theninclude--eager-loading)
8. [Migraciones — sincronizar código y BD](#8-migraciones--sincronizar-código-y-bd)
9. [Seed data con HasData](#9-seed-data-con-hasdata)
10. [Patrón Repository implementado con EF Core](#10-patrón-repository-implementado-con-ef-core)
11. [virtual y override — polimorfismo en repositorios](#11-virtual-y-override--polimorfismo-en-repositorios)
12. [Unidad de Trabajo implementada](#12-unidad-de-trabajo-implementada)
13. [El operador ??= (null-coalescing assignment)](#13-el-operador---null-coalescing-assignment)
14. [SQL Server — queries avanzados](#14-sql-server--queries-avanzados)
15. [Índices en SQL Server](#15-índices-en-sql-server)
16. [Connection strings — qué significa cada parte](#16-connection-strings--qué-significa-cada-parte)
17. [MongoDB — documentos vs tablas](#17-mongodb--documentos-vs-tablas)
18. [BsonId y BsonRepresentation — atributos de MongoDB](#18-bsonid-y-bsonrepresentation--atributos-de-mongodb)
19. [Análisis línea por línea de los archivos del Día 3](#19-análisis-línea-por-línea-de-los-archivos-del-día-3)
20. [Glosario de palabras reservadas y métodos nuevos del Día 3](#20-glosario-de-palabras-reservadas-y-métodos-nuevos-del-día-3)

---

## 1. Entity Framework Core — qué es un ORM

### El problema que resuelve

Imagina que tienes esta clase C#:

```csharp
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
}
```

Y quieres guardarla en SQL Server. Sin un ORM, tendrías que escribir:

```csharp
// Sin ORM — código manual, tedioso y propenso a errores
using var conexion = new SqlConnection("...");
conexion.Open();
var cmd = new SqlCommand(
    "INSERT INTO Productos (Nombre, Precio) VALUES (@nombre, @precio)", conexion);
cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
cmd.Parameters.AddWithValue("@precio", producto.Precio);
cmd.ExecuteNonQuery();
```

**Con EF Core (ORM):**

```csharp
// Con ORM — EF Core escribe el SQL por ti
await _contexto.Productos.AddAsync(producto);
await _contexto.SaveChangesAsync();
// EF genera: INSERT INTO Productos (Nombre, Precio) VALUES (@p0, @p1)
```

### ORM = Object-Relational Mapper

| Concepto | Mundo C# | Mundo SQL |
|----------|----------|-----------|
| Clase | `Producto` | Tabla `Productos` |
| Propiedad | `Nombre` | Columna `Nombre` |
| Objeto | `new Producto { ... }` | Fila en la tabla |
| Lista de objetos | `IEnumerable<Producto>` | Resultado de SELECT |

### Code First vs Database First

| Enfoque | Qué haces primero | Cuándo usarlo |
|---------|-------------------|---------------|
| **Code First** (el nuestro) | Escribes clases C# → EF crea las tablas | Proyectos nuevos |
| **Database First** | Tienes la BD → EF genera las clases | Proyectos legacy |

---

## 2. DbContext — la sesión con la base de datos

### Qué es

`DbContext` es la clase central de EF Core. Representa una **sesión** con la base de datos. Es como una conexión inteligente que:
- Sabe qué tablas existen (a través de `DbSet<T>`)
- Rastrea qué objetos cambiaron (change tracker)
- Sabe cuándo guardar todo en una transacción (`SaveChangesAsync`)

### Cómo se crea

```csharp
// Tu contexto hereda de DbContext
public class AppDbContext : DbContext
{
    // Recibe la configuración (connection string, proveedor, etc.) por constructor
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
```

### DbContextOptions — la configuración

`DbContextOptions<AppDbContext>` contiene toda la configuración:
- Qué proveedor usar (SQL Server, SQLite, PostgreSQL, en-memory para tests)
- El connection string
- Opciones de logging y comportamiento

Se configura en `Program.cs`:

```csharp
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer("Server=...;Database=..."));
```

### El ciclo de vida del DbContext

```
Request HTTP llega
    → DI crea AppDbContext (nueva instancia — Scoped)
    → El controlador usa el contexto para leer/escribir datos
    → Se llama SaveChangesAsync() — EF envía todos los cambios a la BD en una transacción
    → El request termina → DI destruye el contexto → conexión cerrada
```

### ¿Por qué Scoped y no Singleton?

```csharp
// CORRECTO — Scoped
builder.Services.AddDbContext<AppDbContext>(...);
// Equivale a: AddScoped<AppDbContext>

// MAL — Singleton
builder.Services.AddSingleton<AppDbContext>();
```

Si fuera Singleton, todos los requests HTTP compartirían el mismo contexto. Problema: el **change tracker** de EF Core recuerda qué entidades fueron modificadas. Con varios requests simultáneos, se mezclarían los cambios de diferentes usuarios — datos corruptos.

---

## 3. DbSet\<T\> — la tabla en C#

### Qué es

`DbSet<T>` representa una tabla en la base de datos. Cuando escribes:

```csharp
public DbSet<Producto> Productos => Set<Producto>();
```

Le estás diciendo a EF Core: "la clase `Producto` se mapea a la tabla `Productos`".

### Operaciones que puedes hacer con DbSet

```csharp
// Leer todos
var todos = await _contexto.Productos.ToListAsync();
// SQL: SELECT * FROM Productos

// Leer con filtro
var activos = await _contexto.Productos.Where(p => p.Activo).ToListAsync();
// SQL: SELECT * FROM Productos WHERE Activo = 1

// Buscar por PK
var producto = await _contexto.Productos.FindAsync(5);
// SQL: SELECT TOP 1 * FROM Productos WHERE Id = 5

// Agregar
await _contexto.Productos.AddAsync(nuevoProducto);
// (no ejecuta SQL hasta SaveChangesAsync)

// Actualizar
_contexto.Productos.Update(producto);
// (marca la entidad como modificada — SQL en SaveChangesAsync)

// Eliminar
_contexto.Productos.Remove(producto);
// (marca la entidad para borrar — SQL en SaveChangesAsync)
```

### LINQ sobre DbSet

LINQ (Language Integrated Query) es el lenguaje de consultas de C#. EF Core convierte LINQ a SQL:

```csharp
// LINQ en C#
var resultado = await _contexto.Productos
    .Where(p => p.Activo && p.CategoriaId == 1)
    .OrderBy(p => p.Precio)
    .ToListAsync();

// SQL que genera EF Core
// SELECT * FROM Productos
// WHERE Activo = 1 AND CategoriaId = 1
// ORDER BY Precio ASC
```

---

## 4. OnModelCreating — configurar el esquema

### Qué es

`OnModelCreating` es un método que EF Core llama al inicializar el modelo. Es donde le dices exactamente cómo deben ser las tablas: qué columnas son obligatorias, qué índices crear, cómo se relacionan las tablas.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);  // siempre llama al base primero

    // Aquí configuras tus entidades
    ConfigurarProducto(modelBuilder);
    ConfigurarUsuario(modelBuilder);
    // ...
}
```

### ModelBuilder — el constructor del esquema

`ModelBuilder` es el objeto que recibe tus instrucciones de configuración. Para cada entidad, accedes a ella con `.Entity<T>()`:

```csharp
modelBuilder.Entity<Producto>(entidad =>
{
    // Aquí van las configuraciones de Producto
});
```

### Data Annotations vs Fluent API

Hay dos formas de configurar EF Core:

```csharp
// Forma 1: Data Annotations — atributos encima de la propiedad
public class Producto
{
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; }
}

// Forma 2: Fluent API — en OnModelCreating (la que usamos)
modelBuilder.Entity<Producto>(e =>
{
    e.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
});
```

**Usamos Fluent API porque:**
- Las entidades en `Core` quedan limpias (sin atributos de infraestructura)
- Permite configuraciones más avanzadas (índices, DeleteBehavior)
- Toda la configuración está en un solo lugar (AppDbContext), fácil de auditar

---

## 5. Fluent API — métodos de configuración

### HasKey — definir la clave primaria

```csharp
e.HasKey(p => p.Id);
// SQL: PRIMARY KEY (Id)
// EF Core también infiere PKs por convención (propiedad llamada "Id" o "ProductoId")
// HasKey se usa cuando el nombre no sigue la convención
```

### IsRequired — columna NOT NULL

```csharp
e.Property(p => p.Nombre).IsRequired();
// SQL: Nombre nvarchar(200) NOT NULL
// Sin IsRequired: Nombre nvarchar(200) NULL (acepta nulls)
```

### HasMaxLength — longitud máxima

```csharp
e.Property(p => p.Nombre).HasMaxLength(200);
// SQL: Nombre nvarchar(200) NOT NULL
// Sin HasMaxLength: Nombre nvarchar(max) — sin límite, costoso en índices
```

### HasColumnType — tipo de columna personalizado

```csharp
e.Property(p => p.Precio).HasColumnType("decimal(18,2)");
// SQL: Precio decimal(18,2) NOT NULL
// decimal(18,2) = 16 dígitos enteros + 2 decimales
// Sin esto: EF usaría decimal(18,6) por defecto — más decimales de los necesarios
```

### HasIndex — crear un índice

```csharp
e.HasIndex(p => p.Nombre);
// SQL: CREATE INDEX IX_Productos_Nombre ON Productos (Nombre)

e.HasIndex(u => u.Email).IsUnique();
// SQL: CREATE UNIQUE INDEX IX_Usuarios_Email ON Usuarios (Email)
// UNIQUE = no puede haber dos filas con el mismo Email
```

### HasOne / WithMany / HasForeignKey — relaciones 1:N

```csharp
// Relación: Producto pertenece a Categoria (N:1)
e.HasOne(p => p.Categoria)   // "Producto tiene una Categoria"
 .WithMany(c => c.Productos)  // "Categoria tiene muchos Productos"
 .HasForeignKey(p => p.CategoriaId)  // la columna FK en Productos
 .OnDelete(DeleteBehavior.Restrict); // comportamiento al borrar
```

### HasOne / WithOne — relaciones 1:1

```csharp
// Relación: Carrito pertenece a Usuario (1:1)
e.HasOne(c => c.Usuario)
 .WithOne(u => u.Carrito)
 .HasForeignKey<Carrito>(c => c.UsuarioId); // <Carrito> = quién tiene la FK
```

### HasData — seed data

```csharp
e.HasData(
    new Categoria { Id = 1, Nombre = "Electrónica" },
    new Categoria { Id = 2, Nombre = "Ropa" }
);
// EF incluye estos datos en la migración como INSERT INTO
```

---

## 6. DeleteBehavior — qué pasa al borrar

Cuando defines una FK, EF Core necesita saber qué hacer si se borra el registro padre.

### Las tres opciones principales

```csharp
.OnDelete(DeleteBehavior.Restrict)
// SQL: ON DELETE NO ACTION
// Comportamiento: SQL Server lanza un error si intentas borrar el padre
// Uso en el proyecto: Categoria → Producto (no borrar categoría si tiene productos)
//                    Usuario → Orden (no borrar usuario con órdenes)

.OnDelete(DeleteBehavior.Cascade)
// SQL: ON DELETE CASCADE
// Comportamiento: al borrar el padre, SQL Server borra automáticamente los hijos
// Uso en el proyecto: Usuario → Carrito, Carrito → CarritoItems, Orden → OrdenDetalles

.OnDelete(DeleteBehavior.SetNull)
// SQL: ON DELETE SET NULL
// Comportamiento: al borrar el padre, la FK del hijo se pone en NULL
// Uso: cuando el hijo puede existir sin el padre (no se usa en este proyecto)
```

### Diagrama de DeleteBehavior en EcommerceNet

```
Usuario ──CASCADE──► Carrito ──CASCADE──► CarritoItem
   │                                           │
   │                                        RESTRICT
   │                                           ▼
   RESTRICT                               Producto ◄──RESTRICT── Categoria
   ▼
 Orden ──CASCADE──► OrdenDetalle
                        │
                     RESTRICT
                        ▼
                    Producto
```

**Regla de oro:** Usa `Cascade` cuando los hijos no tienen sentido sin el padre. Usa `Restrict` cuando quieres proteger datos importantes.

---

## 7. Include y ThenInclude — eager loading

### El problema: lazy loading desactivado

Por defecto, EF Core **no carga las relaciones** automáticamente. Si buscas un Producto, `producto.Categoria` será `null`:

```csharp
var producto = await _contexto.Productos.FindAsync(1);
Console.WriteLine(producto.Categoria.Nombre); // ← NullReferenceException!
```

### La solución: Include (eager loading)

`Include` le dice a EF Core que cargue la relación **en la misma consulta** (un solo SQL con JOIN):

```csharp
var producto = await _contexto.Productos
    .Include(p => p.Categoria)  // carga la categoría
    .FirstOrDefaultAsync(p => p.Id == 1);

Console.WriteLine(producto.Categoria.Nombre); // ✓ "Electrónica"
```

**SQL generado:**
```sql
SELECT p.*, c.*
FROM Productos p
INNER JOIN Categorias c ON p.CategoriaId = c.Id
WHERE p.Id = 1
```

### ThenInclude — relaciones anidadas

Cuando la relación tiene varias capas, usas `ThenInclude`:

```csharp
// Cargar: Carrito → Items → Producto → Categoria (3 niveles)
var carrito = await _contexto.Carritos
    .Include(c => c.Items)                    // nivel 1: Items
        .ThenInclude(i => i.Producto)         // nivel 2: Producto de cada Item
            .ThenInclude(p => p!.Categoria)   // nivel 3: Categoria del Producto
    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
```

**SQL generado (simplificado):**
```sql
SELECT c.*, ci.*, p.*, cat.*
FROM Carritos c
JOIN CarritoItems ci ON ci.CarritoId = c.Id
JOIN Productos p ON p.Id = ci.ProductoId
JOIN Categorias cat ON cat.Id = p.CategoriaId
WHERE c.UsuarioId = @usuarioId
```

### Eager vs Lazy loading

| Tipo | Cuándo carga | Ventaja | Desventaja |
|------|-------------|---------|------------|
| **Eager** (Include) | En la misma consulta | Una sola ida a la BD | Query más complejo |
| **Lazy** | Cuando accedes a la propiedad | Simple de usar | N+1 queries (problema de performance) |
| **Explicit** | Cuando llamas Load() manualmente | Control total | Más código |

**Usamos Eager loading (Include)** porque nos da control explícito y evita el problema N+1.

---

## 8. Migraciones — sincronizar código y BD

### ¿Qué es una migración?

Es un archivo C# que describe **cómo pasar de un estado de la BD al siguiente**. Tiene dos métodos:
- `Up()` — los cambios a aplicar (CREATE TABLE, ALTER TABLE, etc.)
- `Down()` — cómo revertirlos (DROP TABLE, etc.)

### El flujo completo

```
1. Modificas tus clases C# (ej: agregas propiedad Marca a Producto)
       ↓
2. dotnet ef migrations add AgregarMarcaAProducto
   EF compara tu código con el snapshot anterior y genera:
       Migrations/20260405_AgregarMarcaAProducto.cs
       (contiene: ALTER TABLE Productos ADD Marca nvarchar(max))
       ↓
3. dotnet ef database update
   EF ejecuta el Up() de migraciones pendientes en la BD real
       ↓
4. La tabla Productos ahora tiene la columna Marca
```

### Comandos esenciales

```bash
# Crear nueva migración
dotnet ef migrations add NombreDescriptivo --project ../EcommerceNet.Data

# Aplicar migraciones pendientes a la BD
dotnet ef database update --project ../EcommerceNet.Data

# Ver el SQL que generaría (sin ejecutar)
dotnet ef migrations script --project ../EcommerceNet.Data

# Eliminar la última migración (solo si no se aplicó a la BD)
dotnet ef migrations remove --project ../EcommerceNet.Data

# Revertir la BD al estado de una migración anterior
dotnet ef database update NombreMigracionAnterior --project ../EcommerceNet.Data
```

### Por qué se ejecutan desde el proyecto API

```bash
cd src/EcommerceNet.API   # ← desde aquí
dotnet ef migrations add ... --project ../EcommerceNet.Data
```

`dotnet ef` necesita dos proyectos:
- **Startup project** (donde está Program.cs con `AddDbContext`) → `API`
- **Project con el DbContext** (donde están las migraciones) → `Data`

EF Tools arranca la aplicación de `API` para descubrir la configuración de `AppDbContext` (connection string, opciones). Luego genera los archivos en `Data`.

---

## 9. Seed data con HasData

### Qué es

`HasData` le dice a EF Core que inserte filas iniciales cuando se crea la BD. Es para datos que siempre deben existir: categorías de productos, usuario admin, configuraciones base.

```csharp
mb.Entity<Categoria>().HasData(
    new Categoria { Id = 1, Nombre = "Electrónica", Activa = true },
    new Categoria { Id = 2, Nombre = "Ropa", Activa = true }
);
```

### Reglas importantes

**Regla 1: El Id debe ser explícito**

```csharp
// MAL — EF no puede rastrear filas sin Id
new Categoria { Nombre = "Electrónica" }

// BIEN — Id explícito para que EF sepa qué fila actualizar o borrar en migraciones futuras
new Categoria { Id = 1, Nombre = "Electrónica" }
```

**Regla 2: Usar fechas fijas, no `DateTime.UtcNow`**

```csharp
// MAL — fecha diferente en cada ejecución → EF detecta "pending changes" siempre
FechaCreacion = DateTime.UtcNow

// BIEN — fecha constante
var fechaSeed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
FechaCreacion = fechaSeed
```

**Regla 3: Hashes de contraseñas como constantes**

```csharp
// MAL — BCrypt genera salt aleatorio → hash diferente cada vez
PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")

// BIEN — hash pre-computado una sola vez
private const string HashAdmin = "$2a$11$6UzHJU...";
PasswordHash = HashAdmin
```

### SQL que genera HasData

```sql
-- En la migración (dentro de Up())
INSERT INTO [Categorias] ([Id], [Activa], [Descripcion], [Nombre])
VALUES (1, CAST(1 AS bit), N'Gadgets...', N'Electrónica'),
       (2, CAST(1 AS bit), N'Moda casual...', N'Ropa'),
       ...
```

---

## 10. Patrón Repository implementado con EF Core

### La interfaz (Core — sin cambios)

```csharp
// Core/Interfaces/IRepositorio.cs
public interface IRepositorio<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}
```

### La implementación (Data)

```csharp
// Data/Repositorios/RepositorioBase.cs
public class RepositorioBase<T> : IRepositorio<T> where T : class
{
    protected readonly AppDbContext _contexto;
    protected readonly DbSet<T> _dbSet;  // ← la tabla

    public RepositorioBase(AppDbContext contexto)
    {
        _contexto = contexto;
        _dbSet = contexto.Set<T>();  // Set<T>() obtiene el DbSet<T> correcto
    }

    public virtual async Task<T?> ObtenerPorIdAsync(int id)
        => await _dbSet.FindAsync(id);  // busca en caché primero, luego BD

    public virtual async Task<IEnumerable<T>> ObtenerTodosAsync()
        => await _dbSet.ToListAsync();  // SELECT * FROM tabla

    public async Task AgregarAsync(T entidad)
        => await _dbSet.AddAsync(entidad);  // marca para INSERT

    public void Actualizar(T entidad)
        => _dbSet.Update(entidad);  // marca para UPDATE

    public void Eliminar(T entidad)
        => _dbSet.Remove(entidad);  // marca para DELETE
}
```

**Nota:** `AgregarAsync`, `Actualizar` y `Eliminar` NO ejecutan SQL inmediatamente. Solo marcan la entidad. El SQL real se ejecuta cuando se llama `SaveChangesAsync()` en la UnidadDeTrabajo.

### `contexto.Set<T>()` — acceder a cualquier DbSet genéricamente

```csharp
_dbSet = contexto.Set<T>();
```

Si `T = Producto`, entonces `Set<Producto>()` devuelve el mismo `DbSet<Producto>` que `contexto.Productos`. La diferencia es que funciona de forma genérica sin conocer el tipo en tiempo de compilación.

---

## 11. virtual y override — polimorfismo en repositorios

### El problema que resuelven

`RepositorioBase.ObtenerPorIdAsync` usa `FindAsync` — eficiente pero **no carga relaciones**:

```csharp
public virtual async Task<T?> ObtenerPorIdAsync(int id)
    => await _dbSet.FindAsync(id);
// Devuelve Producto con Categoria = null
```

`ProductoRepositorio` necesita devolver el producto **con su categoría**. Solución: override.

### virtual — método que permite ser sobreescrito

```csharp
// En RepositorioBase — la implementación por defecto
public virtual async Task<T?> ObtenerPorIdAsync(int id)
    => await _dbSet.FindAsync(id);
```

`virtual` = "este método puede ser reemplazado por una subclase".

### override — reemplazar el comportamiento

```csharp
// En ProductoRepositorio — implementación especializada
public override async Task<Producto?> ObtenerPorIdAsync(int id)
{
    return await _contexto.Productos
        .Include(p => p.Categoria)  // ← la diferencia
        .FirstOrDefaultAsync(p => p.Id == id);
}
```

`override` = "reemplaza la implementación del padre con esta".

### Por qué importa (polimorfismo)

```csharp
// El controlador llama:
var producto = await _uow.Productos.ObtenerPorIdAsync(5);

// IProductoRepositorio apunta a ProductoRepositorio
// ProductoRepositorio tiene override de ObtenerPorIdAsync
// → se ejecuta el override (con Include), no el base
// El controlador no sabe ni le importa cuál implementación se usa
```

---

## 12. Unidad de Trabajo implementada

### Recuerda: ¿qué problema resuelve?

Sin UnidadDeTrabajo, cada repositorio tendría su propio `SaveChanges`. Si la operación de checkout necesita:
1. Reducir el stock del producto
2. Vaciar el carrito
3. Crear la orden

...y el paso 2 falla, el paso 1 ya se ejecutó → stock reducido pero sin orden creada. **Inconsistencia de datos.**

Con UnidadDeTrabajo, todos los cambios se guardan en **una sola transacción**. O todo tiene éxito, o nada.

### Análisis del código

```csharp
public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly AppDbContext _contexto;

    // Repositorios — null hasta que se acceden
    private IProductoRepositorio? _productos;

    public UnidadDeTrabajo(AppDbContext contexto)
    {
        _contexto = contexto;
        // NO se crean los repositorios aquí — se crean cuando se necesitan
    }

    // La propiedad Productos:
    // Si _productos es null → créalo con el MISMO contexto
    // Si ya existe → devuélvelo (reutiliza el mismo objeto)
    public IProductoRepositorio Productos =>
        _productos ??= new ProductoRepositorio(_contexto);

    public async Task<int> GuardarCambiosAsync()
        => await _contexto.SaveChangesAsync();
    // SaveChangesAsync envuelve todos los cambios pendientes en una transacción

    public void Dispose()
        => _contexto.Dispose(); // libera la conexión a la BD
}
```

**Por qué todos los repositorios reciben el MISMO `_contexto`:**

```csharp
// Repositorios creados con el mismo contexto
_productos ??= new ProductoRepositorio(_contexto);
_carritos  ??= new CarritoRepositorio(_contexto);
_ordenes   ??= new OrdenRepositorio(_contexto);

// Cuando llamas GuardarCambiosAsync():
await _contexto.SaveChangesAsync();
// El contexto conoce TODOS los cambios: de Productos, de Carritos, de Ordenes
// Los guarda todos en UNA sola transacción
```

---

## 13. El operador ??= (null-coalescing assignment)

### ¿Qué hace?

`??=` asigna el valor del lado derecho **solo si la variable es null**. Si ya tiene un valor, no hace nada.

```csharp
// Con ??=
x ??= valor;

// Equivalente a:
if (x == null)
    x = valor;
```

### En el contexto de UnidadDeTrabajo

```csharp
// Primera vez que se accede a Productos:
// _productos es null → se crea y asigna → se devuelve
public IProductoRepositorio Productos =>
    _productos ??= new ProductoRepositorio(_contexto);

// Segunda vez que se accede a Productos:
// _productos ya NO es null → no se crea nada → se devuelve el mismo objeto
```

### Comparación de operadores `?`

```csharp
// ?? (null-coalescing) — devuelve el valor derecho si el izquierdo es null
string nombre = usuario?.Nombre ?? "Anónimo";

// ??= (null-coalescing assignment) — asigna el valor derecho si la variable es null
_productos ??= new ProductoRepositorio(_contexto);

// ?. (null-conditional) — accede al miembro solo si el objeto no es null
int? longitud = texto?.Length;
```

---

## 14. SQL Server — queries avanzados

Los siguientes queries funcionan en Azure Data Studio contra la BD `EcommerceNetDB`.

### INNER JOIN vs LEFT JOIN

```sql
-- INNER JOIN: solo filas con coincidencia en AMBAS tablas
-- Devuelve solo categorías que tienen al menos un producto
SELECT c.Nombre AS Categoria, COUNT(p.Id) AS Productos
FROM Categorias c
INNER JOIN Productos p ON p.CategoriaId = c.Id
GROUP BY c.Nombre;

-- LEFT JOIN: TODAS las filas de la tabla izquierda, aunque no tengan coincidencia
-- Devuelve TODAS las categorías, incluyendo las vacías (con 0 productos)
SELECT c.Nombre AS Categoria, COUNT(p.Id) AS Productos
FROM Categorias c
LEFT JOIN Productos p ON p.CategoriaId = c.Id AND p.Activo = 1
GROUP BY c.Nombre;
```

### GROUP BY — agrupar y agregar

```sql
SELECT c.Nombre AS Categoria,
       COUNT(p.Id) AS TotalProductos,
       AVG(p.Precio) AS PrecioPromedio,
       MIN(p.Precio) AS PrecioMinimo,
       MAX(p.Precio) AS PrecioMaximo,
       SUM(p.Stock) AS StockTotal
FROM Categorias c
LEFT JOIN Productos p ON p.CategoriaId = c.Id AND p.Activo = 1
GROUP BY c.Id, c.Nombre
ORDER BY TotalProductos DESC;
```

| Función | Qué hace |
|---------|----------|
| `COUNT(col)` | Cuenta filas no nulas |
| `AVG(col)` | Promedio |
| `MIN(col)` | Valor mínimo |
| `MAX(col)` | Valor máximo |
| `SUM(col)` | Suma total |

### Subconsulta — query dentro de un query

```sql
-- Productos más caros que el promedio general
SELECT Nombre, Precio
FROM Productos
WHERE Activo = 1
  AND Precio > (SELECT AVG(Precio) FROM Productos WHERE Activo = 1)
ORDER BY Precio DESC;
```

La subconsulta `(SELECT AVG(Precio)...)` se ejecuta primero y devuelve un número que se usa en el `WHERE` del query principal.

### CTE — Common Table Expression

Una CTE es como una "vista temporal" que solo existe dentro del query. Mejora la legibilidad de queries complejos:

```sql
-- Sin CTE (difícil de leer)
SELECT * FROM (
    SELECT p.Nombre, p.Precio, c.Nombre AS Categoria,
           ROW_NUMBER() OVER (PARTITION BY c.Id ORDER BY p.Precio DESC) AS Ranking
    FROM Productos p
    INNER JOIN Categorias c ON p.CategoriaId = c.Id
    WHERE p.Activo = 1
) AS sub
WHERE sub.Ranking <= 3;

-- Con CTE (fácil de leer)
WITH ProductosRankeados AS (
    SELECT p.Nombre, p.Precio, c.Nombre AS Categoria,
           ROW_NUMBER() OVER (PARTITION BY c.Id ORDER BY p.Precio DESC) AS Ranking
    FROM Productos p
    INNER JOIN Categorias c ON p.CategoriaId = c.Id
    WHERE p.Activo = 1
)
SELECT Nombre, Precio, Categoria, Ranking
FROM ProductosRankeados
WHERE Ranking <= 3;  -- top 3 más caros por categoría
```

### ROW_NUMBER() OVER (PARTITION BY)

`ROW_NUMBER()` asigna un número a cada fila. `PARTITION BY` reinicia el contador para cada grupo.

```sql
-- Numera los productos dentro de cada categoría (por precio descendente)
ROW_NUMBER() OVER (PARTITION BY c.Id ORDER BY p.Precio DESC) AS Ranking
```

| Producto | Categoria | Precio | Ranking |
|---------|-----------|--------|---------|
| Laptop | Electrónica | 25999 | 1 |
| Monitor | Electrónica | 8499 | 2 |
| Teclado | Electrónica | 2199 | 3 |
| Silla | Hogar | 5999 | 1 |
| Lámpara | Hogar | 699 | 2 |

### Stored Procedures

Un Stored Procedure es un bloque de SQL guardado en la BD con nombre, que se ejecuta con `EXEC`:

```sql
CREATE PROCEDURE sp_ResumenTienda
AS
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Productos WHERE Activo = 1) AS ProductosActivos,
        (SELECT COUNT(*) FROM Usuarios) AS TotalUsuarios,
        (SELECT ISNULL(SUM(Total), 0) FROM Ordenes) AS IngresoTotal;
END;

-- Ejecutar
EXEC sp_ResumenTienda;
```

**Cuándo usar SPs:** reportes complejos, lógica que no cambia frecuentemente. En proyectos con EF Core se usan poco porque LINQ es más mantenible.

---

## 15. Índices en SQL Server

### El problema sin índices

```sql
-- Sin índice en Email:
SELECT * FROM Usuarios WHERE Email = 'juan@ejemplo.com';
-- SQL Server lee TODA la tabla hasta encontrar la fila → Table Scan
-- Con 1 millón de usuarios: lento
```

### Qué es un índice

Un índice es una **estructura de datos adicional** que SQL Server mantiene, ordenada por una o más columnas. Es como el índice al final de un libro: en lugar de leer todo el libro para encontrar "ArrayList", vas al índice y encuentras directamente la página.

```sql
-- Con índice único en Email:
SELECT * FROM Usuarios WHERE Email = 'juan@ejemplo.com';
-- SQL Server va directamente a la fila → Index Seek
-- Con 1 millón de usuarios: milisegundos
```

### CLUSTERED vs NONCLUSTERED

| Tipo | Qué hace | Cuántos puede haber |
|------|---------|---------------------|
| **CLUSTERED** | Ordena físicamente las filas de la tabla | Solo 1 (la PK por defecto) |
| **NONCLUSTERED** | Estructura separada con punteros | Hasta 999 |

```sql
-- La PK crea automáticamente un índice CLUSTERED:
CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
-- → filas de Productos ordenadas físicamente por Id

-- EF Core crea NONCLUSTERED para índices adicionales:
CREATE INDEX [IX_Productos_Nombre] ON [Productos] ([Nombre]);
```

### Índices en EcommerceNet (generados por EF Core)

```sql
CREATE UNIQUE INDEX [IX_Usuarios_Email]      -- búsqueda de usuario por email (login)
CREATE UNIQUE INDEX [IX_Carritos_UsuarioId]  -- carrito de un usuario específico
CREATE INDEX [IX_Productos_Nombre]           -- búsqueda LIKE '%termino%'
CREATE INDEX [IX_Ordenes_NumeroOrden]        -- buscar orden por número
```

### ¿Cuándo NO crear índices?

- Tablas muy pequeñas (< 1000 filas): el overhead no vale la pena
- Columnas que se modifican constantemente: cada UPDATE recalcula el índice
- Columnas con muy poca selectividad (ej: columna booleana `Activo` — solo 2 valores posibles)

---

## 16. Connection strings — qué significa cada parte

```
Server=(localdb)\MSSQLLocalDB;Database=EcommerceNetDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

| Parte | Significado |
|-------|-------------|
| `Server=(localdb)\MSSQLLocalDB` | Servidor SQL. `(localdb)` = SQL Server LocalDB (versión ligera). `\MSSQLLocalDB` = nombre de la instancia |
| `Database=EcommerceNetDB` | Nombre de la base de datos a usar (se crea si no existe) |
| `Trusted_Connection=True` | Autenticarse con las credenciales de Windows (no usuario/contraseña) |
| `TrustServerCertificate=True` | Confiar en el certificado SSL del servidor sin validarlo. **Necesario para LocalDB con .NET 10** porque LocalDB no tiene certificado CA válido |
| `MultipleActiveResultSets=true` | Permite múltiples queries activos simultáneamente en la misma conexión (necesario para algunas operaciones de EF Core) |

### Para producción (AWS RDS):

```
Server=mi-instancia.rds.amazonaws.com,1433;
Database=EcommerceNetDB;
User Id=sa;
Password=contraseña-segura;
Encrypt=True;
TrustServerCertificate=False;  ← False en prod (validar el certificado real)
```

---

## 17. MongoDB — documentos vs tablas

### La diferencia conceptual

| SQL Server | MongoDB |
|-----------|---------|
| Tablas con esquema fijo | Colecciones de documentos (JSON/BSON) |
| Filas con columnas definidas | Documentos con campos variables |
| Relaciones por FK | Documentos embebidos o referencias |
| ACID transactions | Transactions (desde MongoDB 4.0) |
| SQL estándar | Pipeline de agregación |
| Fuerte para datos transaccionales | Fuerte para datos flexibles/logs |

### El documento de búsqueda en MongoDB

```json
// Documento en la colección "busquedas"
{
  "_id": ObjectId("507f1f77bcf86cd799439011"),
  "termino": "laptop",
  "usuarioId": 5,
  "resultadosEncontrados": 3,
  "fecha": ISODate("2026-04-05T05:15:00Z")
}
```

No hay "NULL" para campos opcionales: si `usuarioId` no existe, el campo simplemente no está en el documento. Esto es imposible en SQL Server (una columna existe o no existe — no puede "no estar" en una fila).

### MongoClient — el punto de entrada

```csharp
// MongoClient = conexión al servidor MongoDB (puede manejar múltiples bases de datos)
var cliente = new MongoClient("mongodb://localhost:27017");

// GetDatabase = obtiene o crea la base de datos
var database = cliente.GetDatabase("EcommerceNetDB");

// GetCollection<T> = obtiene o crea la colección (equivalente a una tabla)
var coleccion = database.GetCollection<BusquedaHistorial>("busquedas");
```

### Operaciones CRUD en MongoDB

```csharp
// INSERT — agregar un documento
await coleccion.InsertOneAsync(new BusquedaHistorial { Termino = "laptop" });

// SELECT con filtro — Find devuelve un cursor, ToListAsync materializa
var busquedas = await coleccion
    .Find(b => b.UsuarioId == 5)
    .ToListAsync();

// UPDATE — UpdateOneAsync con filter + update
await coleccion.UpdateOneAsync(
    filter: b => b.Id == id,
    update: Builders<BusquedaHistorial>.Update.Set(b => b.Termino, "nuevo"));

// DELETE
await coleccion.DeleteOneAsync(b => b.Id == id);
```

### ¿Por qué MongoDB para historial de búsquedas?

1. **Escrituras masivas**: cada búsqueda genera un registro. MongoDB es más eficiente para escrituras frecuentes que SQL Server.
2. **Sin esquema fijo**: las búsquedas pueden tener campos opcionales (usuarioId puede ser null para usuarios anónimos).
3. **Agregaciones**: el pipeline de aggregation es natural para "términos más buscados".
4. **No transaccional**: si falla guardar una búsqueda, no afecta al usuario. En SQL Server con FK esto sería más rígido.

---

## 18. BsonId y BsonRepresentation — atributos de MongoDB

```csharp
public class BusquedaHistorial
{
    [BsonId]                                    // ← atributo 1
    [BsonRepresentation(BsonType.ObjectId)]     // ← atributo 2
    public string? Id { get; set; }

    public string Termino { get; set; } = string.Empty;
}
```

### [BsonId]

Marca la propiedad como el `_id` del documento MongoDB. Equivalente a `[Key]` en Entity Framework. Todo documento MongoDB tiene un `_id` obligatorio.

### [BsonRepresentation(BsonType.ObjectId)]

MongoDB usa `ObjectId` internamente para los `_id`. Es un tipo binario de 12 bytes que incluye timestamp, ID de máquina y un contador. Externamente lo representamos como un `string` de 24 caracteres hexadecimales.

`BsonRepresentation(BsonType.ObjectId)` le dice al driver que:
- Al leer: convierte el `ObjectId` binario de MongoDB a `string` en C#
- Al escribir: convierte el `string` de C# a `ObjectId` binario para MongoDB

```
MongoDB almacena: ObjectId("507f1f77bcf86cd799439011")  (12 bytes binarios)
C# ve:           "507f1f77bcf86cd799439011"             (string de 24 chars)
```

### ¿Por qué `string?` y no `string`?

```csharp
public string? Id { get; set; }
```

El `?` indica que `Id` puede ser `null`. Cuando creas un objeto nuevo en C# antes de insertarlo en MongoDB, `Id` es null. MongoDB asigna el ObjectId automáticamente al hacer `InsertOneAsync`.

---

## 19. Análisis línea por línea de los archivos del Día 3

### 19.1 AppDbContext.cs

```csharp
public class AppDbContext : DbContext          // hereda de DbContext (EF Core)
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    // DbContextOptions<AppDbContext> = configuración específica para este contexto
    // : base(options) = pasa la config al constructor de DbContext
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    // Set<Categoria>() = obtiene el DbSet<Categoria> del contexto
    // => = expression body (propiedad de solo lectura, sin backing field)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    // protected = solo accesible por esta clase y subclases
    // override = reemplaza la implementación del padre (DbContext)
    // ModelBuilder = el objeto que recibe las instrucciones de configuración
    {
        base.OnModelCreating(modelBuilder);
        // SIEMPRE llamar al base primero (EF Core necesita ejecutar su propio código)

        ConfigurarProducto(modelBuilder);
        // Delegamos la configuración a métodos privados para mejor organización
    }

    private static void ConfigurarProducto(ModelBuilder mb)
    // static = no necesita instancia de AppDbContext (no accede a this)
    {
        mb.Entity<Producto>(e =>
        // Lambda: e = la configuración de la entidad Producto
        {
            e.HasKey(p => p.Id);
            // Declara Id como clave primaria
            // EF Core lo infiere por convención, pero es buena práctica ser explícito

            e.Property(p => p.Precio).HasColumnType("decimal(18,2)");
            // Fuerza el tipo SQL específico (EF usaría decimal(18,6) por defecto)

            e.HasIndex(p => p.Nombre);
            // Crea NONCLUSTERED INDEX IX_Productos_Nombre ON Productos (Nombre)

            e.HasOne(p => p.Categoria)    // Producto tiene UNA Categoria
             .WithMany(c => c.Productos)  // Categoria tiene MUCHOS Productos
             .HasForeignKey(p => p.CategoriaId)  // columna FK en Productos
             .OnDelete(DeleteBehavior.Restrict);  // no borrar Categoria si tiene Productos
        });
    }

    private const string HashAdminFijo = "$2a$11$6UzHJU...";
    // const = valor fijo en tiempo de compilación (nunca cambia en runtime)
    // Pre-computado UNA vez para evitar el bug de "pending model changes"
}
```

### 19.2 ProductoRepositorio.cs

```csharp
public class ProductoRepositorio : RepositorioBase<Producto>, IProductoRepositorio
// Herencia múltiple de clase base + interfaz
// RepositorioBase<Producto> provee el CRUD genérico
// IProductoRepositorio define los métodos especializados
{
    public ProductoRepositorio(AppDbContext contexto) : base(contexto) { }
    // Constructor mínimo — pasa el contexto al base para que inicialice _dbSet

    public override async Task<Producto?> ObtenerPorIdAsync(int id)
    // override = reemplaza el virtual del RepositorioBase
    // Versión especializada que agrega Include(Categoria)
    {
        return await _contexto.Productos
        // _contexto viene de RepositorioBase (protected)
            .Include(p => p.Categoria)
            // JOIN a Categorias — necesario para que p.Categoria no sea null
            .FirstOrDefaultAsync(p => p.Id == id);
            // FirstOrDefault: devuelve la primera fila o null si no existe
    }

    public async Task<IEnumerable<Producto>> BuscarPorNombreAsync(string termino)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.Nombre.Contains(termino))
            // .Contains(termino) → SQL: WHERE Nombre LIKE '%termino%'
            .OrderBy(p => p.Nombre)
            .ToListAsync();
            // ToListAsync() ejecuta el query y materializa los resultados en memoria
    }
}
```

### 19.3 CarritoRepositorio.cs

```csharp
public async Task<Carrito?> ObtenerPorUsuarioAsync(int usuarioId)
{
    return await _contexto.Carritos
        .Include(c => c.Items)                  // JOIN a CarritoItems
            .ThenInclude(i => i.Producto)        // luego JOIN a Productos
                .ThenInclude(p => p!.Categoria)  // luego JOIN a Categorias
        // p! = "confío en que p no es null aquí" (null-forgiving operator)
        // EF Core garantiza que Producto está cargado por el Include anterior
        .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
}
```

### 19.4 UnidadDeTrabajo.cs

```csharp
public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly AppDbContext _contexto;
    // readonly = solo se asigna en el constructor (inmutabilidad)

    private IProductoRepositorio? _productos;
    // ? = puede ser null (tipo nullable)
    // Comienza como null — se crea en el primer acceso

    public IProductoRepositorio Productos =>
        _productos ??= new ProductoRepositorio(_contexto);
    // => = expression body (getter sin backing field explícito)
    // ??= = si _productos es null, créalo y asígnalo; si no, devuelve el existente
    // new ProductoRepositorio(_contexto) = repositorio comparte el MISMO contexto

    public async Task<int> GuardarCambiosAsync()
        => await _contexto.SaveChangesAsync();
    // SaveChangesAsync detecta todos los cambios pendientes y genera el SQL
    // Ejecuta TODO en una sola transacción atómica

    public void Dispose()
        => _contexto.Dispose();
    // Libera la conexión a SQL Server cuando el request HTTP termina
}
```

### 19.5 AuthServicio.cs (Data/Servicios)

```csharp
public async Task<Resultado<AuthRespuestaDto>> RegistrarAsync(RegistroDto dto)
{
    var existe = await _contexto.Usuarios.AnyAsync(u => u.Email == dto.Email);
    // AnyAsync = SELECT CASE WHEN EXISTS(...) THEN 1 ELSE 0 END
    // Más eficiente que FirstOrDefaultAsync cuando solo necesitas saber si existe

    if (existe)
        return Resultado<AuthRespuestaDto>.Error("Ya existe un usuario con ese email");

    var usuario = new Usuario
    {
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        // HashPassword genera un salt aleatorio y lo incluye en el hash
        // El hash resultante incluye el salt — BCrypt.Verify puede verificarlo después
    };

    await _contexto.Usuarios.AddAsync(usuario);  // marca para INSERT
    await _contexto.SaveChangesAsync();            // ejecuta el INSERT + asigna el Id

    var respuesta = GenerarJwt(usuario);
    // usuario.Id ya tiene valor después de SaveChangesAsync (SQL Server lo asigna)
    return Resultado<AuthRespuestaDto>.Ok(respuesta, "Usuario registrado exitosamente");
}
```

### 19.6 HistorialBusquedaServicio.cs

```csharp
public HistorialBusquedaServicio(IConfiguration config)
// IConfiguration = acceso a appsettings.json, variables de entorno, etc.
{
    var connectionString = config["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
    // config["MongoDB:ConnectionString"] = lee la sección MongoDB → ConnectionString

    var cliente = new MongoClient(connectionString);
    // MongoClient es thread-safe — puede usarse en múltiples threads simultáneamente
    // Gestiona internamente el pool de conexiones

    var database = cliente.GetDatabase(databaseName);
    // Si la base de datos no existe, MongoDB la crea en el primer insert

    _coleccion = database.GetCollection<BusquedaHistorial>("busquedas");
    // GetCollection = obtiene la colección "busquedas"
    // Si no existe, MongoDB la crea en el primer insert
}

public async Task<List<TerminoPopular>> ObtenerMasBuscadosAsync(int top = 10)
{
    return await _coleccion.Aggregate()
    // Aggregate() inicia un pipeline de agregación (como GROUP BY en SQL)
        .Group(
            b => b.Termino,           // clave de agrupación (GROUP BY Termino)
            g => new TerminoPopular
            {
                Termino = g.Key,      // el valor del GROUP BY
                TotalBusquedas = g.Count()  // COUNT(*) para cada grupo
            })
        .SortByDescending(t => t.TotalBusquedas)  // ORDER BY TotalBusquedas DESC
        .Limit(top)                                // LIMIT @top
        .ToListAsync();
}
```

---

## 20. Glosario de palabras reservadas y métodos nuevos del Día 3

### EF Core

| Término | Tipo | Definición |
|---------|------|-----------|
| `DbContext` | Clase | Sesión con la base de datos. Raíz de EF Core. |
| `DbSet<T>` | Propiedad | Representa una tabla. Permite queries LINQ. |
| `ModelBuilder` | Parámetro | Recibe la configuración Fluent API en OnModelCreating. |
| `OnModelCreating` | Método | Se ejecuta al inicializar el modelo. Aquí va Fluent API. |
| `HasKey` | Método fluent | Define la clave primaria. |
| `IsRequired` | Método fluent | Columna NOT NULL. |
| `HasMaxLength` | Método fluent | Longitud máxima → `nvarchar(N)`. |
| `HasColumnType` | Método fluent | Tipo SQL explícito (ej: `decimal(18,2)`). |
| `HasIndex` | Método fluent | Crea un índice en la columna. |
| `IsUnique` | Método fluent | Hace el índice UNIQUE. |
| `HasOne` | Método fluent | Define el lado "uno" de una relación. |
| `WithMany` | Método fluent | Define el lado "muchos" de una relación. |
| `WithOne` | Método fluent | Define el otro lado de una relación 1:1. |
| `HasForeignKey` | Método fluent | Especifica la columna FK. |
| `OnDelete` | Método fluent | Comportamiento al borrar el padre. |
| `HasData` | Método fluent | Seed data — datos iniciales en migraciones. |
| `DeleteBehavior.Restrict` | Enum | SQL: ON DELETE NO ACTION. Lanza error si hay hijos. |
| `DeleteBehavior.Cascade` | Enum | SQL: ON DELETE CASCADE. Borra hijos automáticamente. |
| `Include` | Extensión LINQ | Carga una relación (JOIN) en el mismo query. |
| `ThenInclude` | Extensión LINQ | Carga una relación anidada (JOIN adicional). |
| `FindAsync(id)` | Método DbSet | Busca por PK. Revisa caché primero. |
| `FirstOrDefaultAsync` | Extensión LINQ | Devuelve el primer resultado o null. |
| `AnyAsync` | Extensión LINQ | Devuelve true si existe al menos una fila. |
| `ToListAsync` | Extensión LINQ | Materializa el query en una lista. |
| `SaveChangesAsync` | Método DbContext | Persiste todos los cambios en una transacción. |
| `Set<T>()` | Método DbContext | Obtiene el DbSet<T> por tipo genérico. |

### SQL Server

| Término | Definición |
|---------|-----------|
| `INNER JOIN` | Retorna solo filas con coincidencia en ambas tablas. |
| `LEFT JOIN` | Retorna todas las filas de la izquierda, null si no hay coincidencia. |
| `GROUP BY` | Agrupa filas por columna para aplicar funciones de agregación. |
| `COUNT(*)` | Cuenta todas las filas del grupo. |
| `AVG(col)` | Promedio de los valores de la columna. |
| `SUM(col)` | Suma de los valores. |
| `ISNULL(expr, val)` | Si expr es NULL, devuelve val. |
| `CTE (WITH ... AS)` | Tabla temporal nombrada para usar dentro de un query. |
| `ROW_NUMBER()` | Asigna número de fila dentro de la partición. |
| `OVER (PARTITION BY)` | Define el alcance de la función de ventana. |
| `CREATE PROCEDURE` | Define un bloque de SQL reutilizable. |
| `EXEC sp_nombre` | Ejecuta un stored procedure. |
| `IDENTITY` | Autoincremento — SQL Server asigna el valor automáticamente. |
| `nvarchar(N)` | Cadena Unicode de longitud variable, máximo N caracteres. |
| `nvarchar(max)` | Cadena Unicode sin límite de longitud. |
| `decimal(p,s)` | Número decimal exacto. p = dígitos totales, s = decimales. |
| `bit` | Booleano SQL Server (0 = false, 1 = true). |
| `datetime2` | Fecha y hora con alta precisión. |

### MongoDB / Driver

| Término | Tipo | Definición |
|---------|------|-----------|
| `MongoClient` | Clase | Conexión al servidor MongoDB. Thread-safe, usar como Singleton. |
| `IMongoDatabase` | Interfaz | Referencia a una base de datos MongoDB. |
| `IMongoCollection<T>` | Interfaz | Referencia a una colección (≈ tabla). |
| `BsonId` | Atributo | Marca la propiedad como _id del documento. |
| `BsonRepresentation` | Atributo | Controla cómo se serializa/deserializa la propiedad. |
| `BsonType.ObjectId` | Enum | Tipo ObjectId de MongoDB (12 bytes). |
| `InsertOneAsync` | Método | Inserta un documento. |
| `Find(filtro)` | Método | Inicia una búsqueda con filtro. |
| `Aggregate()` | Método | Inicia un pipeline de agregación (GROUP BY, SORT, LIMIT). |
| `Group(key, proj)` | Pipeline | Agrupa documentos por clave. |
| `SortByDescending` | Pipeline | Ordena descendente. |
| `Limit(n)` | Pipeline | Limita el resultado a N documentos. |

### C# — Operadores y keywords nuevos

| Término | Definición |
|---------|-----------|
| `??=` | Null-coalescing assignment. Asigna solo si la variable es null. |
| `virtual` | El método puede ser sobreescrito en subclases. |
| `override` | Reemplaza la implementación del método virtual del padre. |
| `const` | Constante en tiempo de compilación. No puede cambiar en runtime. |
| `p!` | Null-forgiving operator. "Confío en que esto no es null". |
| `_ = expr` | Discard. Descarta el valor de retorno de la expresión. |
