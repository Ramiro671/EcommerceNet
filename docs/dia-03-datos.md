# Día 03 — SQL Server + Entity Framework Core + MongoDB

> **Rama Git:** `dia-03/datos`  
> **Método:** 16 Pomodoros de 25 min (5 min descanso entre cada uno)  
> **Objetivo:** Conectar la API a SQL Server con EF Core, implementar los repositorios reales, seed data, queries avanzados y MongoDB para historial de búsquedas.

---

## Cronograma Pomodoro

| # | Bloque | Qué hacer |
|---|--------|-----------|
| 1 | Teoría | EF Core: qué es, Code First vs Database First, DbContext |
| 2 | Paquetes | Instalar NuGet en Data y API, configurar connection string |
| 3 | DbContext | Crear AppDbContext con DbSets y OnModelCreating |
| 4 | Fluent API | Configurar relaciones, índices, restricciones y tipos de columna |
| 5 | Seed data | Agregar datos iniciales: categorías, productos, usuario admin |
| 6 | Repositorio | Crear RepositorioBase\<T\> genérico con EF Core |
| 7 | Repositorio | Crear ProductoRepositorio con queries LINQ + Include |
| 8 | Repositorio | Crear CarritoRepositorio y OrdenRepositorio |
| 9 | UoW | Crear UnidadDeTrabajo y registrar todo en Program.cs |
| 10 | Migración | Crear primera migración y aplicarla a la BD |
| 11 | AuthServicio | Implementar AuthServicio real con BD (registro + login + JWT) |
| 12 | SQL puro | Ejercicios SQL: JOINs, GROUP BY, subconsultas |
| 13 | SQL avanzado | CTEs, Stored Procedures, índices y planes de ejecución |
| 14 | MongoDB | Instalar driver, crear HistorialBusquedaServicio |
| 15 | Integrar | Conectar MongoDB al endpoint de búsqueda de productos |
| 16 | Merge | dotnet build, dotnet test, probar API con Swagger, merge |

---

## Pomodoro 1 — Teoría: Entity Framework Core (25 min)

### ¿Qué es Entity Framework Core?

Es un **ORM** (Object-Relational Mapper) — traduce entre tus clases C# y las tablas de SQL Server. En lugar de escribir SQL a mano, escribes LINQ en C# y EF Core lo convierte a SQL.

```
Tu código C#:    _contexto.Productos.Where(p => p.Precio > 100).ToListAsync()
EF Core genera:  SELECT * FROM Productos WHERE Precio > 100
```

### Code First vs Database First

| Enfoque | Qué haces primero | Cuándo usarlo |
|---------|-------------------|---------------|
| **Code First** (el nuestro) | Escribes las clases C# → EF crea las tablas | Proyectos nuevos |
| Database First | Tienes la BD → EF genera las clases | Proyectos legacy con BD existente |

Nosotros usamos Code First: las entidades del Día 1 (Producto, Carrito, Orden) se convierten en tablas automáticamente.

### ¿Qué es el DbContext?

Es la clase central de EF Core. Representa una sesión con la base de datos. Tiene:
- `DbSet<T>` — cada uno es una tabla
- `OnModelCreating` — donde configuras relaciones y restricciones
- `SaveChangesAsync()` — guarda todos los cambios pendientes en una transacción

> **Para la entrevista:** "El DbContext se registra como Scoped en DI porque cada request HTTP necesita su propia conexión a la BD. Si fuera Singleton, todos los requests compartirían la misma conexión y habría problemas de concurrencia."

---

## Pomodoro 2 — Instalar paquetes y configurar (25 min)

### Paquetes NuGet

```powershell
# En el proyecto Data (donde vive EF Core)
cd src/EcommerceNet.Data
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# En el proyecto API (para usar migraciones desde ahí)
cd ../EcommerceNet.API
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Connection string en `appsettings.json` (ya debería existir del Día 2)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EcommerceNetDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> **¿Qué es LocalDB?** Es una versión ligera de SQL Server que viene con Visual Studio. No necesitas instalar SQL Server completo. Se ejecuta automáticamente cuando la necesitas.

---

## Pomodoro 3 — Crear AppDbContext (25 min)

### Archivo: `src/EcommerceNet.Data/AppDbContext.cs`

```csharp
using EcommerceNet.Core.Entidades;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data;

/// <summary>
/// Contexto de base de datos — la puerta de entrada a SQL Server.
/// Cada DbSet<T> se convierte en una tabla.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Cada DbSet = una tabla en la BD
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<CarritoItem> CarritoItems => Set<CarritoItem>();
    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<OrdenDetalle> OrdenDetalles => Set<OrdenDetalle>();

    /// <summary>
    /// Configuración de las tablas: relaciones, índices, tipos de columna, seed data.
    /// EF Core lee esto al crear las migraciones.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar cada entidad (se detalla en Pomodoro 4)
        ConfigurarCategoria(modelBuilder);
        ConfigurarProducto(modelBuilder);
        ConfigurarUsuario(modelBuilder);
        ConfigurarCarrito(modelBuilder);
        ConfigurarCarritoItem(modelBuilder);
        ConfigurarOrden(modelBuilder);
        ConfigurarOrdenDetalle(modelBuilder);

        // Seed data (se detalla en Pomodoro 5)
        AgregarDatosIniciales(modelBuilder);
    }

    // Los métodos privados se implementan en los Pomodoros 4 y 5
    private void ConfigurarCategoria(ModelBuilder mb) { /* Pomodoro 4 */ }
    private void ConfigurarProducto(ModelBuilder mb) { /* Pomodoro 4 */ }
    private void ConfigurarUsuario(ModelBuilder mb) { /* Pomodoro 4 */ }
    private void ConfigurarCarrito(ModelBuilder mb) { /* Pomodoro 4 */ }
    private void ConfigurarCarritoItem(ModelBuilder mb) { /* Pomodoro 4 */ }
    private void ConfigurarOrden(ModelBuilder mb) { /* Pomodoro 4 */ }
    private void ConfigurarOrdenDetalle(ModelBuilder mb) { /* Pomodoro 4 */ }
    private void AgregarDatosIniciales(ModelBuilder mb) { /* Pomodoro 5 */ }
}
```

---

## Pomodoro 4 — Fluent API: relaciones y restricciones (25 min)

Reemplaza los métodos vacíos del Pomodoro 3 con estas implementaciones:

```csharp
private void ConfigurarCategoria(ModelBuilder mb)
{
    mb.Entity<Categoria>(e =>
    {
        e.HasKey(c => c.Id);
        e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        e.Property(c => c.Descripcion).HasMaxLength(500);
    });
}

private void ConfigurarProducto(ModelBuilder mb)
{
    mb.Entity<Producto>(e =>
    {
        e.HasKey(p => p.Id);
        e.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        e.Property(p => p.Descripcion).HasMaxLength(1000);
        e.Property(p => p.Precio).HasColumnType("decimal(18,2)");  // precisión para dinero
        e.Property(p => p.ImagenUrl).HasMaxLength(500);

        // Índice para búsquedas rápidas por nombre
        e.HasIndex(p => p.Nombre);

        // Relación: Producto pertenece a Categoria
        e.HasOne(p => p.Categoria)
         .WithMany(c => c.Productos)
         .HasForeignKey(p => p.CategoriaId)
         .OnDelete(DeleteBehavior.Restrict);  // no borrar categoría si tiene productos
    });
}

private void ConfigurarUsuario(ModelBuilder mb)
{
    mb.Entity<Usuario>(e =>
    {
        e.HasKey(u => u.Id);
        e.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        e.Property(u => u.Email).IsRequired().HasMaxLength(200);
        e.Property(u => u.PasswordHash).IsRequired();

        // Email único — no puede haber dos usuarios con el mismo email
        e.HasIndex(u => u.Email).IsUnique();
    });
}

private void ConfigurarCarrito(ModelBuilder mb)
{
    mb.Entity<Carrito>(e =>
    {
        e.HasKey(c => c.Id);

        // Relación 1:1 — un usuario tiene un solo carrito
        e.HasOne(c => c.Usuario)
         .WithOne(u => u.Carrito)
         .HasForeignKey<Carrito>(c => c.UsuarioId)
         .OnDelete(DeleteBehavior.Cascade);  // borrar carrito si se borra usuario
    });
}

private void ConfigurarCarritoItem(ModelBuilder mb)
{
    mb.Entity<CarritoItem>(e =>
    {
        e.HasKey(ci => ci.Id);
        e.Property(ci => ci.PrecioUnitario).HasColumnType("decimal(18,2)");

        // Relación: CarritoItem pertenece a Carrito
        e.HasOne(ci => ci.Carrito)
         .WithMany(c => c.Items)
         .HasForeignKey(ci => ci.CarritoId)
         .OnDelete(DeleteBehavior.Cascade);

        // Relación: CarritoItem referencia un Producto
        e.HasOne(ci => ci.Producto)
         .WithMany()
         .HasForeignKey(ci => ci.ProductoId)
         .OnDelete(DeleteBehavior.Restrict);
    });
}

private void ConfigurarOrden(ModelBuilder mb)
{
    mb.Entity<Orden>(e =>
    {
        e.HasKey(o => o.Id);
        e.Property(o => o.NumeroOrden).HasMaxLength(50);
        e.Property(o => o.Total).HasColumnType("decimal(18,2)");
        e.Property(o => o.DireccionEnvio).IsRequired().HasMaxLength(500);

        // Índice para buscar órdenes por número
        e.HasIndex(o => o.NumeroOrden);

        // Relación: Orden pertenece a Usuario
        e.HasOne(o => o.Usuario)
         .WithMany(u => u.Ordenes)
         .HasForeignKey(o => o.UsuarioId)
         .OnDelete(DeleteBehavior.Restrict);
    });
}

private void ConfigurarOrdenDetalle(ModelBuilder mb)
{
    mb.Entity<OrdenDetalle>(e =>
    {
        e.HasKey(od => od.Id);
        e.Property(od => od.PrecioUnitario).HasColumnType("decimal(18,2)");
        e.Property(od => od.Subtotal).HasColumnType("decimal(18,2)");

        e.HasOne(od => od.Orden)
         .WithMany(o => o.Detalles)
         .HasForeignKey(od => od.OrdenId)
         .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(od => od.Producto)
         .WithMany()
         .HasForeignKey(od => od.ProductoId)
         .OnDelete(DeleteBehavior.Restrict);
    });
}
```

> **Concepto Senior: Fluent API vs Data Annotations**
> Hay dos formas de configurar EF Core:
> - **Data Annotations**: `[Required]`, `[MaxLength(100)]` — se ponen encima de la propiedad
> - **Fluent API**: `.IsRequired()`, `.HasMaxLength(100)` — se configura en OnModelCreating
>
> Usamos Fluent API porque mantiene las entidades limpias (sin atributos de BD)
> y permite configuraciones más avanzadas (índices compuestos, DeleteBehavior).

---

## Pomodoro 5 — Seed data (25 min)

```csharp
private void AgregarDatosIniciales(ModelBuilder mb)
{
    // Categorías
    mb.Entity<Categoria>().HasData(
        new Categoria { Id = 1, Nombre = "Electrónica", Descripcion = "Gadgets, dispositivos y accesorios tecnológicos" },
        new Categoria { Id = 2, Nombre = "Ropa", Descripcion = "Moda casual, formal y deportiva" },
        new Categoria { Id = 3, Nombre = "Hogar", Descripcion = "Muebles, decoración y electrodomésticos" },
        new Categoria { Id = 4, Nombre = "Deportes", Descripcion = "Equipamiento y ropa deportiva" },
        new Categoria { Id = 5, Nombre = "Libros", Descripcion = "Libros físicos y digitales" }
    );

    // Productos
    mb.Entity<Producto>().HasData(
        // Electrónica
        new Producto { Id = 1, Nombre = "Laptop Gaming Pro", Descripcion = "Laptop 16GB RAM, SSD 512GB, RTX 4060", Precio = 25999.99m, Stock = 15, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Laptop" },
        new Producto { Id = 2, Nombre = "Audífonos Bluetooth", Descripcion = "Cancelación de ruido, 30 hrs batería", Precio = 1899.50m, Stock = 50, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Audifonos" },
        new Producto { Id = 3, Nombre = "Monitor 4K 27 pulgadas", Descripcion = "IPS, 144Hz, HDR10", Precio = 8499.00m, Stock = 20, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Monitor" },
        new Producto { Id = 4, Nombre = "Teclado Mecánico RGB", Descripcion = "Switches Cherry MX, retroiluminado", Precio = 2199.00m, Stock = 35, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Teclado" },
        // Ropa
        new Producto { Id = 5, Nombre = "Camiseta Algodón Premium", Descripcion = "100% algodón, corte regular", Precio = 399.00m, Stock = 100, CategoriaId = 2, ImagenUrl = "https://placehold.co/400x300?text=Camiseta" },
        new Producto { Id = 6, Nombre = "Jeans Slim Fit", Descripcion = "Mezclilla stretch, azul oscuro", Precio = 899.00m, Stock = 60, CategoriaId = 2, ImagenUrl = "https://placehold.co/400x300?text=Jeans" },
        // Hogar
        new Producto { Id = 7, Nombre = "Silla Ergonómica de Oficina", Descripcion = "Soporte lumbar, reposabrazos ajustable", Precio = 5999.00m, Stock = 10, CategoriaId = 3, ImagenUrl = "https://placehold.co/400x300?text=Silla" },
        new Producto { Id = 8, Nombre = "Lámpara LED de Escritorio", Descripcion = "3 tonos de luz, dimmer táctil", Precio = 699.00m, Stock = 40, CategoriaId = 3, ImagenUrl = "https://placehold.co/400x300?text=Lampara" },
        // Deportes
        new Producto { Id = 9, Nombre = "Mancuernas Ajustables 20kg", Descripcion = "Par de mancuernas con discos intercambiables", Precio = 1599.00m, Stock = 25, CategoriaId = 4, ImagenUrl = "https://placehold.co/400x300?text=Mancuernas" },
        new Producto { Id = 10, Nombre = "Tapete de Yoga Premium", Descripcion = "6mm grosor, antideslizante", Precio = 599.00m, Stock = 45, CategoriaId = 4, ImagenUrl = "https://placehold.co/400x300?text=Tapete" },
        // Libros
        new Producto { Id = 11, Nombre = "Clean Code — Robert C. Martin", Descripcion = "Guía para escribir código limpio y mantenible", Precio = 450.00m, Stock = 30, CategoriaId = 5, ImagenUrl = "https://placehold.co/400x300?text=CleanCode" },
        new Producto { Id = 12, Nombre = "Design Patterns — Gang of Four", Descripcion = "Los 23 patrones de diseño clásicos", Precio = 520.00m, Stock = 20, CategoriaId = 5, ImagenUrl = "https://placehold.co/400x300?text=DesignPatterns" }
    );

    // Usuario Admin (password: Admin123! — hasheado con BCrypt)
    mb.Entity<Usuario>().HasData(
        new Usuario
        {
            Id = 1,
            Nombre = "Admin Tienda",
            Email = "admin@ecommercenet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Rol = Core.Enums.RolUsuario.Admin
        }
    );
}
```

> **Nota sobre BCrypt en seed data:** Necesitarás agregar el paquete BCrypt al proyecto Data:
> `dotnet add package BCrypt.Net-Next`
> O puedes pegar un hash pre-generado como string para evitar la dependencia.

---

## Pomodoros 6-8 — Repositorios con EF Core (75 min)

### Archivo: `src/EcommerceNet.Data/Repositorios/RepositorioBase.cs`

```csharp
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Implementación genérica del CRUD con EF Core.
/// Cualquier entidad puede usar estos métodos base.
/// </summary>
public class RepositorioBase<T> : IRepositorio<T> where T : class
{
    protected readonly AppDbContext _contexto;
    protected readonly DbSet<T> _dbSet;

    public RepositorioBase(AppDbContext contexto)
    {
        _contexto = contexto;
        _dbSet = contexto.Set<T>();
    }

    public virtual async Task<T?> ObtenerPorIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> ObtenerTodosAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AgregarAsync(T entidad)
    {
        await _dbSet.AddAsync(entidad);
    }

    public void Actualizar(T entidad)
    {
        _dbSet.Update(entidad);
    }

    public void Eliminar(T entidad)
    {
        _dbSet.Remove(entidad);
    }
}
```

### Archivo: `src/EcommerceNet.Data/Repositorios/ProductoRepositorio.cs`

```csharp
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Repositorio de productos — queries especializados con LINQ + EF Core.
/// Cada método se traduce a SQL automáticamente.
/// </summary>
public class ProductoRepositorio : RepositorioBase<Producto>, IProductoRepositorio
{
    public ProductoRepositorio(AppDbContext contexto) : base(contexto) { }

    /// <summary>
    /// Buscar por nombre — EF genera: WHERE Nombre LIKE '%termino%'
    /// Include carga la categoría en la misma consulta (JOIN)
    /// </summary>
    public async Task<IEnumerable<Producto>> BuscarPorNombreAsync(string termino)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.Nombre.Contains(termino))
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Productos de una categoría específica
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(int categoriaId)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.CategoriaId == categoriaId)
            .OrderBy(p => p.Precio)
            .ToListAsync();
    }

    /// <summary>
    /// Productos con stock bajo — útil para el panel de admin
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerConStockBajoAsync(int minimo = 5)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.Stock < minimo)
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }

    /// <summary>
    /// Todos los productos activos con su categoría
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerActivosAsync()
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();
    }

    /// <summary>
    /// Override del base para incluir la categoría
    /// </summary>
    public override async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### Archivo: `src/EcommerceNet.Data/Repositorios/CarritoRepositorio.cs`

```csharp
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Repositorio del carrito — siempre carga Items y Productos (eager loading)
/// </summary>
public class CarritoRepositorio : ICarritoRepositorio
{
    private readonly AppDbContext _contexto;

    public CarritoRepositorio(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <summary>
    /// Obtener carrito con items y productos — necesita Include anidado
    /// EF genera: SELECT + JOIN CarritoItems + JOIN Productos
    /// </summary>
    public async Task<Carrito?> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _contexto.Carritos
            .Include(c => c.Items)
                .ThenInclude(i => i.Producto)      // Include anidado
                    .ThenInclude(p => p!.Categoria) // y la categoría del producto
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
    }

    public async Task AgregarAsync(Carrito carrito)
    {
        await _contexto.Carritos.AddAsync(carrito);
    }

    public void Actualizar(Carrito carrito)
    {
        _contexto.Carritos.Update(carrito);
    }
}
```

### Archivo: `src/EcommerceNet.Data/Repositorios/OrdenRepositorio.cs`

```csharp
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

public class OrdenRepositorio : RepositorioBase<Orden>, IOrdenRepositorio
{
    public OrdenRepositorio(AppDbContext contexto) : base(contexto) { }

    public async Task<IEnumerable<Orden>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _contexto.Ordenes
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Orden?> ObtenerConDetallesAsync(int ordenId)
    {
        return await _contexto.Ordenes
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(o => o.Usuario)
            .FirstOrDefaultAsync(o => o.Id == ordenId);
    }
}
```

---

## Pomodoro 9 — Unidad de Trabajo y registrar en DI (25 min)

### Archivo: `src/EcommerceNet.Data/UnidadDeTrabajo.cs`

```csharp
using EcommerceNet.Core.Interfaces;
using EcommerceNet.Data.Repositorios;

namespace EcommerceNet.Data;

/// <summary>
/// Agrupa todos los repositorios y controla cuándo se guardan los cambios.
/// Garantiza que múltiples operaciones se ejecuten en una sola transacción.
/// </summary>
public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly AppDbContext _contexto;

    // Repositorios — se crean lazy (solo cuando se necesitan)
    private IProductoRepositorio? _productos;
    private ICarritoRepositorio? _carritos;
    private IOrdenRepositorio? _ordenes;

    public UnidadDeTrabajo(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    // Si el repositorio no existe, créalo. Si ya existe, reutilízalo.
    public IProductoRepositorio Productos =>
        _productos ??= new ProductoRepositorio(_contexto);

    public ICarritoRepositorio Carritos =>
        _carritos ??= new CarritoRepositorio(_contexto);

    public IOrdenRepositorio Ordenes =>
        _ordenes ??= new OrdenRepositorio(_contexto);

    /// <summary>
    /// Guarda TODOS los cambios pendientes en una sola transacción.
    /// Si algo falla, nada se guarda.
    /// </summary>
    public async Task<int> GuardarCambiosAsync()
    {
        return await _contexto.SaveChangesAsync();
    }

    public void Dispose()
    {
        _contexto.Dispose();
    }
}
```

### Registrar en Program.cs

Agregar estas líneas en la sección de servicios de `Program.cs`:

```csharp
using EcommerceNet.Core.Interfaces;
using EcommerceNet.Core.Servicios;
using EcommerceNet.Data;
using Microsoft.EntityFrameworkCore;

// DbContext — Scoped (una conexión por request)
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Unidad de trabajo y servicios
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();
```

---

## Pomodoro 10 — Migraciones (25 min)

```powershell
# Desde la raíz del proyecto
cd src/EcommerceNet.API

# Crear la primera migración
dotnet ef migrations add CreacionInicial --project ../EcommerceNet.Data

# Ver qué SQL generaría (sin ejecutar)
dotnet ef migrations script --project ../EcommerceNet.Data

# Aplicar la migración (crear las tablas + seed data)
dotnet ef database update --project ../EcommerceNet.Data
```

> **¿Qué hacen las migraciones?**
> 1. EF Core compara tus entidades C# con el estado actual de la BD
> 2. Genera un archivo C# con los cambios necesarios (CREATE TABLE, ALTER, etc.)
> 3. `database update` ejecuta esos cambios en SQL Server
>
> Si luego agregas una propiedad a Producto (ej: `public string Marca`), haces:
> `dotnet ef migrations add AgregarMarcaAProducto` y EF genera el `ALTER TABLE`.

### Verificar que la BD se creó

```powershell
# Ejecutar la API
dotnet run

# Abrir en el navegador: https://localhost:5001/swagger
# Probar GET /api/productos — debería devolver los 12 productos del seed
```

---

## Pomodoro 11 — Implementar AuthServicio real (25 min)

### Archivo: `src/EcommerceNet.Data/Servicios/AuthServicio.cs`

> Nota: este servicio va en Data porque necesita acceso directo al DbContext para buscar usuarios.

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceNet.Data.Servicios;

/// <summary>
/// Servicio de autenticación con BD real.
/// Registro: hashea password con BCrypt y guarda en SQL Server.
/// Login: verifica password y genera token JWT.
/// </summary>
public class AuthServicio : IAuthServicio
{
    private readonly AppDbContext _contexto;
    private readonly IConfiguration _config;

    public AuthServicio(AppDbContext contexto, IConfiguration config)
    {
        _contexto = contexto;
        _config = config;
    }

    public async Task<Resultado<AuthRespuestaDto>> RegistrarAsync(RegistroDto dto)
    {
        // Validar que el email no exista
        var existe = await _contexto.Usuarios.AnyAsync(u => u.Email == dto.Email);
        if (existe)
            return Resultado<AuthRespuestaDto>.Error("Ya existe un usuario con ese email");

        // Crear usuario con password hasheado
        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _contexto.Usuarios.AddAsync(usuario);
        await _contexto.SaveChangesAsync();

        // Generar token y retornar
        var token = GenerarJwt(usuario);
        return Resultado<AuthRespuestaDto>.Ok(token, "Usuario registrado exitosamente");
    }

    public async Task<Resultado<AuthRespuestaDto>> LoginAsync(LoginDto dto)
    {
        // Buscar usuario por email
        var usuario = await _contexto.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null)
            return Resultado<AuthRespuestaDto>.Error("Credenciales incorrectas");

        // Verificar password con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return Resultado<AuthRespuestaDto>.Error("Credenciales incorrectas");

        // Generar token y retornar
        var token = GenerarJwt(usuario);
        return Resultado<AuthRespuestaDto>.Ok(token, "Login exitoso");
    }

    /// <summary>
    /// Genera un token JWT con los claims del usuario.
    /// Claims = datos dentro del token (ID, email, rol).
    /// </summary>
    private AuthRespuestaDto GenerarJwt(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiraMinutos = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");
        var expira = DateTime.UtcNow.AddMinutes(expiraMinutos);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return new AuthRespuestaDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            Expira = expira
        };
    }
}
```

---

## Pomodoros 12-13 — SQL puro: queries avanzados (50 min)

Practica estos queries directamente en Azure Data Studio o la extensión mssql de VS Code. Conéctate a `(localdb)\MSSQLLocalDB` y selecciona la BD `EcommerceNetDB`.

### Query 1: JOIN — productos con categoría

```sql
-- INNER JOIN: solo productos que TIENEN categoría
SELECT p.Nombre AS Producto, p.Precio, p.Stock,
       c.Nombre AS Categoria
FROM Productos p
INNER JOIN Categorias c ON p.CategoriaId = c.Id
WHERE p.Activo = 1
ORDER BY c.Nombre, p.Precio DESC;
```

### Query 2: GROUP BY — resumen por categoría

```sql
SELECT c.Nombre AS Categoria,
       COUNT(p.Id) AS TotalProductos,
       AVG(p.Precio) AS PrecioPromedio,
       SUM(p.Stock) AS StockTotal,
       MIN(p.Precio) AS PrecioMinimo,
       MAX(p.Precio) AS PrecioMaximo
FROM Categorias c
LEFT JOIN Productos p ON p.CategoriaId = c.Id AND p.Activo = 1
GROUP BY c.Nombre
ORDER BY TotalProductos DESC;
```

### Query 3: Subconsulta — productos más caros que el promedio

```sql
SELECT Nombre, Precio
FROM Productos
WHERE Activo = 1
  AND Precio > (SELECT AVG(Precio) FROM Productos WHERE Activo = 1)
ORDER BY Precio DESC;
```

### Query 4: CTE — ranking de productos por categoría

```sql
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

### Query 5: Stored Procedure — reporte de la tienda

```sql
CREATE PROCEDURE sp_ResumenTienda
AS
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Productos WHERE Activo = 1) AS ProductosActivos,
        (SELECT COUNT(*) FROM Usuarios) AS TotalUsuarios,
        (SELECT COUNT(*) FROM Ordenes WHERE Estado != 5) AS OrdenesActivas,
        (SELECT ISNULL(SUM(Total), 0) FROM Ordenes WHERE Estado != 5) AS IngresoTotal,
        (SELECT COUNT(*) FROM Productos WHERE Stock < 5 AND Activo = 1) AS StockBajo;
END;

-- Ejecutar
EXEC sp_ResumenTienda;
```

### Concepto: Índices

```sql
-- Ver los índices existentes en la tabla Productos
SELECT name, type_desc FROM sys.indexes WHERE object_id = OBJECT_ID('Productos');

-- Crear un índice compuesto para búsquedas frecuentes
CREATE NONCLUSTERED INDEX IX_Productos_Categoria_Precio
ON Productos (CategoriaId, Precio DESC)
WHERE Activo = 1;  -- índice filtrado

-- Ver el plan de ejecución de un query
SET STATISTICS IO ON;
SELECT * FROM Productos WHERE CategoriaId = 1 AND Precio > 1000;
SET STATISTICS IO OFF;
```

> **Para la entrevista:**
> - **¿Qué es un índice?** Es como el índice de un libro — permite encontrar filas sin recorrer toda la tabla.
> - **¿Cuándo lo crearías?** En columnas que se usan frecuentemente en WHERE, JOIN y ORDER BY.
> - **¿Cuándo NO?** En tablas pequeñas o columnas que cambian constantemente (cada INSERT recalcula el índice).
> - **¿Qué es un plan de ejecución?** Es el "mapa" que SQL Server crea para decidir cómo ejecutar tu query. Te dice si usa un índice o hace un table scan (recorrer todo).

---

## Pomodoros 14-15 — MongoDB para historial de búsquedas (50 min)

### Instalar el driver

```powershell
cd src/EcommerceNet.Data
dotnet add package MongoDB.Driver
```

### Configurar en appsettings.json

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "EcommerceNetDB"
  }
}
```

> Si no tienes MongoDB local, puedes usar [MongoDB Atlas](https://www.mongodb.com/atlas) (gratis, en la nube). O usar un contenedor Docker: `docker run -d -p 27017:27017 mongo:7`

### Archivo: `src/EcommerceNet.Data/MongoDB/BusquedaHistorial.cs`

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceNet.Data.MongoDB;

/// <summary>
/// Documento MongoDB — representa una búsqueda que hizo un usuario.
/// No es una entidad relacional — es un documento flexible.
/// </summary>
public class BusquedaHistorial
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Termino { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public int ResultadosEncontrados { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
```

### Archivo: `src/EcommerceNet.Data/MongoDB/HistorialBusquedaServicio.cs`

```csharp
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace EcommerceNet.Data.MongoDB;

/// <summary>
/// Servicio que guarda y consulta el historial de búsquedas en MongoDB.
/// Cada vez que alguien busca un producto, se registra aquí.
/// </summary>
public class HistorialBusquedaServicio
{
    private readonly IMongoCollection<BusquedaHistorial> _coleccion;

    public HistorialBusquedaServicio(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
        var databaseName = config["MongoDB:DatabaseName"] ?? "EcommerceNetDB";

        var cliente = new MongoClient(connectionString);
        var database = cliente.GetDatabase(databaseName);
        _coleccion = database.GetCollection<BusquedaHistorial>("busquedas");
    }

    /// <summary>Registrar una búsqueda</summary>
    public async Task RegistrarBusquedaAsync(string termino, int? usuarioId, int resultados)
    {
        var busqueda = new BusquedaHistorial
        {
            Termino = termino.ToLower().Trim(),
            UsuarioId = usuarioId,
            ResultadosEncontrados = resultados,
            Fecha = DateTime.UtcNow
        };

        await _coleccion.InsertOneAsync(busqueda);
    }

    /// <summary>Obtener los términos más buscados (top N)</summary>
    public async Task<List<TerminoPopular>> ObtenerMasBuscadosAsync(int top = 10)
    {
        var resultado = await _coleccion.Aggregate()
            .Group(b => b.Termino, g => new TerminoPopular
            {
                Termino = g.Key,
                TotalBusquedas = g.Count()
            })
            .SortByDescending(t => t.TotalBusquedas)
            .Limit(top)
            .ToListAsync();

        return resultado;
    }

    /// <summary>Historial de un usuario específico</summary>
    public async Task<List<BusquedaHistorial>> ObtenerPorUsuarioAsync(int usuarioId, int limite = 20)
    {
        return await _coleccion
            .Find(b => b.UsuarioId == usuarioId)
            .SortByDescending(b => b.Fecha)
            .Limit(limite)
            .ToListAsync();
    }
}

/// <summary>DTO para términos populares</summary>
public class TerminoPopular
{
    public string Termino { get; set; } = string.Empty;
    public int TotalBusquedas { get; set; }
}
```

### Registrar en Program.cs

```csharp
builder.Services.AddSingleton<HistorialBusquedaServicio>();
```

> **¿Por qué Singleton y no Scoped?**
> El `MongoClient` de MongoDB está diseñado para ser reutilizado — crear uno por request
> sería ineficiente. Una sola instancia maneja el pool de conexiones internamente.
> Esto es diferente de EF Core donde cada request necesita su propio DbContext.

### Integrar en ProductosController (endpoint de búsqueda)

```csharp
// En el constructor agregar:
private readonly HistorialBusquedaServicio _historial;

// En el método Buscar, después de obtener resultados:
await _historial.RegistrarBusquedaAsync(termino, usuarioId, productos.Count());
```

---

## Pomodoro 16 — Verificar, probar y merge (25 min)

```powershell
# Compilar todo
dotnet build

# Ejecutar pruebas (las del Día 1 deben seguir pasando)
dotnet test

# Ejecutar la API
cd src/EcommerceNet.API
dotnet run

# Abrir Swagger: https://localhost:5001/swagger
# Probar:
# 1. GET /api/productos → debe devolver 12 productos con categorías
# 2. POST /api/auth/login con admin@ecommercenet.com / Admin123!
# 3. Usar el JWT para probar endpoints protegidos
# 4. POST /api/carrito/agregar → verificar que funciona con BD real
```

```powershell
git add .
git commit -m "feat: EF Core con SQL Server, repositorios, migraciones, seed data y MongoDB"
git checkout desarrollo
git merge dia-03/datos
git push origin desarrollo
git checkout main
git merge desarrollo
git push origin main
```

---

## Módulo Git del día

### Comandos usados

```powershell
git checkout -b dia-03/datos
git add .
git commit -m "feat: ..."
git checkout desarrollo
git merge dia-03/datos
```

### Concepto avanzado: `git diff` y `git log`

```powershell
# Ver qué cambió antes de hacer commit
git diff

# Ver solo los nombres de archivos modificados
git diff --name-only

# Ver historial de commits con gráfico
git log --oneline --graph --all
```

---

## Simulador de entrevista técnica — Día 3

**Pregunta 1:** "¿Qué es Entity Framework Core y cuál es la diferencia entre Code First y Database First?"
> "EF Core es el ORM de .NET que traduce entre clases C# y tablas SQL. Con Code First escribo las entidades en C# y EF genera las tablas mediante migraciones. Con Database First parto de una BD existente y EF genera las clases. Uso Code First para proyectos nuevos como EcommerceNet, y Database First cuando mantengo sistemas legacy que ya tienen esquema definido."

**Pregunta 2:** "¿Cuál es la diferencia entre INNER JOIN y LEFT JOIN?"
> "INNER JOIN solo retorna filas donde hay coincidencia en ambas tablas. LEFT JOIN retorna TODAS las filas de la tabla izquierda, y NULL donde no hay coincidencia en la derecha. En mi proyecto, uso Include de EF Core que genera JOINs automáticamente, pero si necesito un LEFT JOIN uso GroupJoin o SQL directo. Por ejemplo, para listar categorías con conteo de productos uso LEFT JOIN porque quiero ver categorías aunque no tengan productos."

**Pregunta 3:** "¿Por qué usas SQL Server y MongoDB en el mismo proyecto?"
> "SQL Server para datos transaccionales que requieren integridad referencial: productos, órdenes, usuarios, carritos. Las relaciones y transacciones ACID son críticas aquí — no quiero vender un producto sin reducir el stock. MongoDB para datos no-transaccionales como el historial de búsquedas: es flexible, no necesita esquema fijo, y es más eficiente para escrituras masivas de logs. En empresa tech, la vacante pide SQL Server como obligatorio y MongoDB como deseable, así que demuestro que puedo usar ambos donde tiene sentido."

---

## Mañana: Día 4

Construiremos el frontend con Vue.js 3: catálogo de productos, carrito interactivo, checkout, login/registro y una página legacy con jQuery.

Rama: `dia-04/frontend`
