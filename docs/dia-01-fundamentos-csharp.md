# Día 01 — Fundamentos C#, Clean Architecture y Estructura

> **Rama Git:** `dia-01/fundamentos-csharp`  
> **Método:** 16 Pomodoros de 25 min (5 min descanso entre cada uno)  
> **Objetivo:** Crear la solución completa: entidades, interfaces, DTOs, servicio del carrito y 20 pruebas unitarias.

---

## Cronograma Pomodoro

| # | Bloque | Qué hacer |
|---|--------|-----------|
| 1 | Entorno | Instalar .NET SDK, configurar VS Code, verificar Git |
| 2 | Estructura | Crear solución, 4 proyectos, referencias, abrir en VS Code |
| 3 | Git | Iniciar repo, crear en GitHub, ramas `main` → `desarrollo` → `dia-01` |
| 4 | Enums | Crear `EstadoOrden.cs` y `RolUsuario.cs` — entender enums en C# |
| 5 | Entidad | Escribir `Categoria.cs` y `Producto.cs` con lógica de stock |
| 6 | Entidad | Escribir `Usuario.cs` y `CarritoItem.cs` |
| 7 | Entidad | Escribir `Carrito.cs` con toda la lógica de negocio |
| 8 | Entidad | Escribir `Orden.cs` y `OrdenDetalle.cs` + commit |
| 9 | Interfaces | Crear `IRepositorio<T>` genérico |
| 10 | Interfaces | Crear repositorios especializados + `IUnidadDeTrabajo` |
| 11 | DTOs | Crear DTOs de Producto, Carrito y Orden + `Resultado<T>` |
| 12 | Servicio | Escribir `ICarritoServicio` y empezar `CarritoServicio` |
| 13 | Servicio | Terminar `CarritoServicio` incluyendo `CheckoutAsync` |
| 14 | Tests | Escribir `ProductoTests.cs` (7 pruebas) |
| 15 | Tests | Escribir `CarritoTests.cs` (10 pruebas) + `OrdenTests.cs` (5 pruebas) |
| 16 | Merge | Ejecutar `dotnet test`, merge a `desarrollo`, push a GitHub |

---

## Pomodoro 1 — Entorno (25 min)

### Instalar herramientas

1. **.NET SDK 8**: descargar desde [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) — elegir "SDK", no "Runtime"
2. **VS Code** con extensiones:
   - **C# Dev Kit** (obligatoria — incluye IntelliSense, depuración, Solution Explorer)
   - **IntelliCode for C# Dev Kit** (autocompletado con IA)
   - **NuGet Gallery** (instalar paquetes sin terminal)
3. **Git**: descargar desde [git-scm.com](https://git-scm.com/) si no lo tienes

### Verificar en PowerShell

```powershell
dotnet --version   # Debe mostrar 8.0.x
git --version      # Debe mostrar 2.x.x
```

### Configuración recomendada de VS Code

Abre `Ctrl+Shift+P` → "Open User Settings JSON" y agrega:

```json
{
  "editor.formatOnSave": true,
  "editor.bracketPairColorization.enabled": true,
  "csharp.experimental.debug.hotReload": true,
  "files.exclude": { "**/bin": true, "**/obj": true }
}
```

### Atajos que usarás todo el día

| Atajo | Acción |
|-------|--------|
| `Ctrl+`` | Terminal integrada |
| `F5` | Depurar |
| `Ctrl+F5` | Ejecutar sin depurar |
| `F9` | Poner breakpoint |
| `Ctrl+.` | Quick fix / refactoring |
| `Ctrl+Shift+B` | Build |

---

## Pomodoro 2 — Crear la solución (25 min)

Abre la terminal integrada de VS Code (`Ctrl+``) y ejecuta bloque por bloque:

```powershell
# Crear carpeta y entrar
cd C:\Users\ramir\Source\repos
mkdir EcommerceNet; cd EcommerceNet

# Crear la solución .NET
dotnet new sln -n EcommerceNet

# Crear los 4 proyectos
dotnet new classlib -n EcommerceNet.Core -o src/EcommerceNet.Core
dotnet new classlib -n EcommerceNet.Data -o src/EcommerceNet.Data
dotnet new webapi -n EcommerceNet.API -o src/EcommerceNet.API
dotnet new xunit -n EcommerceNet.Tests -o tests/EcommerceNet.Tests

# Agregar proyectos a la solución
dotnet sln add src/EcommerceNet.Core/EcommerceNet.Core.csproj
dotnet sln add src/EcommerceNet.Data/EcommerceNet.Data.csproj
dotnet sln add src/EcommerceNet.API/EcommerceNet.API.csproj
dotnet sln add tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj

# Referencias entre proyectos
dotnet add src/EcommerceNet.Data/EcommerceNet.Data.csproj reference src/EcommerceNet.Core/EcommerceNet.Core.csproj
dotnet add src/EcommerceNet.API/EcommerceNet.API.csproj reference src/EcommerceNet.Data/EcommerceNet.Data.csproj
dotnet add tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj reference src/EcommerceNet.Core/EcommerceNet.Core.csproj

# Crear carpeta docs
mkdir docs

# Verificar que compila
dotnet build
```

> **Concepto Senior: Clean Architecture**
> Las capas tienen una dirección de dependencia estricta: Core ← Data ← API.
> Core NO sabe que existe EF Core, SQL Server ni ASP.NET. Solo contiene lógica pura de negocio.
> Esto permite cambiar la base de datos o el framework web sin tocar la lógica.

### Eliminar archivos por defecto

Borra `Class1.cs` de `EcommerceNet.Core` y `EcommerceNet.Data` (VS Code → click derecho → Delete).

---

## Pomodoro 3 — Git y GitHub (25 min)

```powershell
# Inicializar
git init
dotnet new gitignore

# Primer commit
git add .
git commit -m "feat: estructura inicial con Clean Architecture (Core, Data, API, Tests)"

# Crear repo en GitHub: ve a github.com → New repository → "EcommerceNet" → Create
# NO marques "Add README" (ya lo tenemos)

# Conectar
git remote add origin https://github.com/TU-USUARIO/EcommerceNet.git
git branch -M main
git push -u origin main

# Crear rama de pruebas
git checkout -b desarrollo
git push -u origin desarrollo

# Crear rama del día 1
git checkout -b dia-01/fundamentos-csharp
```

> **Concepto Git del día: Ramas de feature**
> Cada funcionalidad se desarrolla en su propia rama. Nunca se hace commit directo a `main`.
> El flujo es: feature branch → merge a desarrollo (pruebas) → merge a main (producción).
> En DaCodes, esto es parte del modelo "Launch Pod" donde QA revisa en `desarrollo` antes del merge a `main`.

---

## Pomodoros 4 a 8 — Entidades del dominio

### Estructura de carpetas en Core

Crea esta estructura dentro de `src/EcommerceNet.Core/`:

```
EcommerceNet.Core/
├── Entidades/
│   ├── Categoria.cs
│   ├── Producto.cs
│   ├── Usuario.cs
│   ├── Carrito.cs
│   ├── CarritoItem.cs
│   ├── Orden.cs
│   └── OrdenDetalle.cs
├── Enums/
│   ├── EstadoOrden.cs
│   └── RolUsuario.cs
├── Interfaces/
│   ├── IRepositorio.cs
│   ├── IProductoRepositorio.cs
│   ├── ICarritoRepositorio.cs
│   ├── IOrdenRepositorio.cs
│   └── IUnidadDeTrabajo.cs
├── DTOs/
│   ├── ProductoDto.cs
│   ├── CarritoDto.cs
│   ├── OrdenDto.cs
│   └── Resultado.cs
└── Servicios/
    ├── ICarritoServicio.cs
    └── CarritoServicio.cs
```

### Pomodoro 4 — Enums

**Archivo: `Enums/EstadoOrden.cs`**

```csharp
namespace EcommerceNet.Core.Enums;

/// <summary>
/// Estados posibles de una orden de compra
/// </summary>
public enum EstadoOrden
{
    Pendiente = 0,      // recién creada
    Pagada = 1,         // pago confirmado
    EnPreparacion = 2,  // preparando envío
    Enviada = 3,        // en camino
    Entregada = 4,      // cliente recibió
    Cancelada = 5       // cancelada
}
```

**Archivo: `Enums/RolUsuario.cs`**

```csharp
namespace EcommerceNet.Core.Enums;

/// <summary>
/// Roles del sistema — Cliente compra, Admin administra productos
/// </summary>
public enum RolUsuario
{
    Cliente = 0,
    Admin = 1
}
```

> **Concepto C#: Enums**
> Un enum es un tipo de valor que define un conjunto fijo de constantes con nombre.
> `EstadoOrden.Pendiente` es más legible que el número `0`.
> En la BD se guarda como `int`, pero en el código siempre usas el nombre.

### Pomodoro 5 — Categoria y Producto

**Archivo: `Entidades/Categoria.cs`**

```csharp
namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Categoría de productos (ej: Electrónica, Ropa, Hogar)
/// </summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;

    // Relación: una categoría tiene muchos productos
    public List<Producto> Productos { get; set; } = new();

    /// <summary>Cuenta productos activos en esta categoría</summary>
    public int TotalProductosActivos()
    {
        return Productos.Count(p => p.Activo);
    }
}
```

**Archivo: `Entidades/Producto.cs`**

```csharp
namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Producto a la venta. Entidad central del sistema.
/// Contiene lógica de validación de stock.
/// </summary>
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relación con categoría
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // --- Lógica de negocio ---

    /// <summary>¿Hay stock suficiente para esta cantidad?</summary>
    public bool TieneStockSuficiente(int cantidad)
    {
        return Activo && Stock >= cantidad;
    }

    /// <summary>Reduce stock tras una compra</summary>
    public void ReducirStock(int cantidad)
    {
        if (!TieneStockSuficiente(cantidad))
            throw new InvalidOperationException(
                $"Stock insuficiente para '{Nombre}'. Disponible: {Stock}, solicitado: {cantidad}");
        Stock -= cantidad;
    }

    /// <summary>Aumenta stock (reabastecimiento o cancelación)</summary>
    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero");
        Stock += cantidad;
    }
}
```

> **Concepto C#: Lógica en las entidades**
> Las entidades no son solo "bolsas de datos". Un desarrollador Senior pone validaciones
> dentro de la entidad (como `ReducirStock`). Esto se llama **Domain-Driven Design (DDD)**.
> La regla es: si la lógica depende solo de los datos de la propia entidad, va en la entidad.

### Pomodoro 6 — Usuario y CarritoItem

**Archivo: `Entidades/Usuario.cs`**

```csharp
using EcommerceNet.Core.Enums;

namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Usuario de la tienda. Puede ser Cliente o Admin.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // NUNCA texto plano
    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Carrito? Carrito { get; set; }
    public List<Orden> Ordenes { get; set; } = new();

    public bool EsAdmin() => Rol == RolUsuario.Admin;
}
```

**Archivo: `Entidades/CarritoItem.cs`**

```csharp
namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Un item dentro del carrito.
/// Guarda el precio al momento de agregarlo (puede cambiar después).
/// </summary>
public class CarritoItem
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    // Relaciones
    public int CarritoId { get; set; }
    public Carrito? Carrito { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    /// <summary>Precio × cantidad</summary>
    public decimal CalcularSubtotal() => PrecioUnitario * Cantidad;
}
```

### Pomodoro 7 — Carrito (la entidad más compleja)

**Archivo: `Entidades/Carrito.cs`**

```csharp
namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Carrito de compras. Cada usuario tiene uno solo.
/// Contiene TODA la lógica de agregar, quitar y actualizar items.
/// </summary>
public class Carrito
{
    public int Id { get; set; }
    public DateTime UltimaModificacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public List<CarritoItem> Items { get; set; } = new();

    // --- Cálculos ---

    public decimal CalcularTotal() => Items.Sum(i => i.CalcularSubtotal());
    public int TotalProductos() => Items.Sum(i => i.Cantidad);
    public bool EstaVacio() => Items.Count == 0;

    // --- Operaciones ---

    /// <summary>
    /// Agrega un producto. Si ya existe, incrementa la cantidad.
    /// </summary>
    public void AgregarProducto(Producto producto, int cantidad = 1)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero");

        if (!producto.TieneStockSuficiente(cantidad))
            throw new InvalidOperationException(
                $"Stock insuficiente para '{producto.Nombre}'");

        var existente = Items.FirstOrDefault(i => i.ProductoId == producto.Id);

        if (existente != null)
        {
            existente.Cantidad += cantidad;
        }
        else
        {
            Items.Add(new CarritoItem
            {
                ProductoId = producto.Id,
                Producto = producto,
                Cantidad = cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        UltimaModificacion = DateTime.UtcNow;
    }

    /// <summary>Actualiza cantidad. Si es 0, elimina el item.</summary>
    public void ActualizarCantidad(int productoId, int nuevaCantidad)
    {
        var item = Items.FirstOrDefault(i => i.ProductoId == productoId)
            ?? throw new InvalidOperationException("Producto no encontrado en el carrito");

        if (nuevaCantidad <= 0)
            Items.Remove(item);
        else
            item.Cantidad = nuevaCantidad;

        UltimaModificacion = DateTime.UtcNow;
    }

    /// <summary>Elimina un producto del carrito</summary>
    public void EliminarProducto(int productoId)
    {
        var item = Items.FirstOrDefault(i => i.ProductoId == productoId)
            ?? throw new InvalidOperationException("Producto no encontrado en el carrito");

        Items.Remove(item);
        UltimaModificacion = DateTime.UtcNow;
    }

    /// <summary>Vacía el carrito completamente</summary>
    public void Vaciar()
    {
        Items.Clear();
        UltimaModificacion = DateTime.UtcNow;
    }
}
```

### Pomodoro 8 — Orden y OrdenDetalle + commit

**Archivo: `Entidades/Orden.cs`**

```csharp
using EcommerceNet.Core.Enums;

namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Orden de compra — registro permanente de una compra completada.
/// </summary>
public class Orden
{
    public int Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;
    public decimal Total { get; set; }
    public string DireccionEnvio { get; set; } = string.Empty;

    // Relaciones
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public List<OrdenDetalle> Detalles { get; set; } = new();

    public void GenerarNumeroOrden()
    {
        NumeroOrden = $"ORD-{FechaCreacion:yyyyMMdd}-{Id:D4}";
    }

    public void RecalcularTotal()
    {
        Total = Detalles.Sum(d => d.Subtotal);
    }

    public bool SePuedeCancelar()
    {
        return Estado == EstadoOrden.Pendiente || Estado == EstadoOrden.Pagada;
    }

    /// <summary>Cancela la orden y devuelve stock</summary>
    public void Cancelar()
    {
        if (!SePuedeCancelar())
            throw new InvalidOperationException(
                $"No se puede cancelar una orden en estado '{Estado}'");

        Estado = EstadoOrden.Cancelada;
        foreach (var detalle in Detalles)
            detalle.Producto?.AumentarStock(detalle.Cantidad);
    }
}
```

**Archivo: `Entidades/OrdenDetalle.cs`**

```csharp
namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Línea de una orden — foto del momento de la compra.
/// </summary>
public class OrdenDetalle
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public int OrdenId { get; set; }
    public Orden? Orden { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public void CalcularSubtotal()
    {
        Subtotal = PrecioUnitario * Cantidad;
    }
}
```

### Commit de entidades

```powershell
git add .
git commit -m "feat: 7 entidades del dominio con lógica de negocio (Producto, Carrito, Orden)"
```

---

## Pomodoros 9-10 — Interfaces

**Archivo: `Interfaces/IRepositorio.cs`**

```csharp
namespace EcommerceNet.Core.Interfaces;

/// <summary>Contrato genérico CRUD para cualquier entidad</summary>
public interface IRepositorio<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}
```

**Archivo: `Interfaces/IProductoRepositorio.cs`**

```csharp
using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Core.Interfaces;

public interface IProductoRepositorio : IRepositorio<Producto>
{
    Task<IEnumerable<Producto>> BuscarPorNombreAsync(string termino);
    Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(int categoriaId);
    Task<IEnumerable<Producto>> ObtenerConStockBajoAsync(int minimo = 5);
    Task<IEnumerable<Producto>> ObtenerActivosAsync();
}
```

**Archivo: `Interfaces/ICarritoRepositorio.cs`**

```csharp
using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Core.Interfaces;

public interface ICarritoRepositorio
{
    Task<Carrito?> ObtenerPorUsuarioAsync(int usuarioId);
    Task AgregarAsync(Carrito carrito);
    void Actualizar(Carrito carrito);
}
```

**Archivo: `Interfaces/IOrdenRepositorio.cs`**

```csharp
using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Core.Interfaces;

public interface IOrdenRepositorio : IRepositorio<Orden>
{
    Task<IEnumerable<Orden>> ObtenerPorUsuarioAsync(int usuarioId);
    Task<Orden?> ObtenerConDetallesAsync(int ordenId);
}
```

**Archivo: `Interfaces/IUnidadDeTrabajo.cs`**

```csharp
namespace EcommerceNet.Core.Interfaces;

/// <summary>Agrupa repositorios en una sola transacción</summary>
public interface IUnidadDeTrabajo : IDisposable
{
    IProductoRepositorio Productos { get; }
    ICarritoRepositorio Carritos { get; }
    IOrdenRepositorio Ordenes { get; }
    Task<int> GuardarCambiosAsync();
}
```

> **Concepto Senior: Patrón Repository + Unit of Work**
> El repositorio abstrae el acceso a datos. La unidad de trabajo agrupa múltiples operaciones
> en una sola transacción. Si el checkout falla al reducir stock, la orden NO se crea.
> En la entrevista de DaCodes te pueden preguntar: "¿Por qué no llamas a SaveChanges desde el repositorio?"
> Respuesta: porque la unidad de trabajo garantiza atomicidad — todo se guarda o nada se guarda.

```powershell
git add .
git commit -m "feat: interfaces de repositorio genérico, especializados y unidad de trabajo"
```

---

## Pomodoro 11 — DTOs y Resultado

**Archivo: `DTOs/ProductoDto.cs`**

```csharp
namespace EcommerceNet.Core.DTOs;

public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public bool Disponible { get; set; }
}

public class CrearProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
}
```

**Archivo: `DTOs/CarritoDto.cs`**

```csharp
namespace EcommerceNet.Core.DTOs;

public class CarritoDto
{
    public int Id { get; set; }
    public List<CarritoItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public int TotalProductos { get; set; }
}

public class CarritoItemDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

public class AgregarAlCarritoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; } = 1;
}
```

**Archivo: `DTOs/OrdenDto.cs`**

```csharp
namespace EcommerceNet.Core.DTOs;

public class OrdenDto
{
    public int Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrdenDetalleDto> Detalles { get; set; } = new();
}

public class OrdenDetalleDto
{
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CrearOrdenDto
{
    public string DireccionEnvio { get; set; } = string.Empty;
}
```

**Archivo: `DTOs/Resultado.cs`**

```csharp
namespace EcommerceNet.Core.DTOs;

/// <summary>Envuelve toda respuesta de la API en un formato estándar</summary>
public class Resultado<T>
{
    public bool Exito { get; set; }
    public T? Datos { get; set; }
    public string? Mensaje { get; set; }
    public List<string> Errores { get; set; } = new();

    public static Resultado<T> Ok(T datos, string? mensaje = null)
        => new() { Exito = true, Datos = datos, Mensaje = mensaje };

    public static Resultado<T> Error(string mensaje)
        => new() { Exito = false, Mensaje = mensaje };

    public static Resultado<T> ErrorValidacion(List<string> errores)
        => new() { Exito = false, Errores = errores, Mensaje = "Error de validación" };
}
```

```powershell
git add .
git commit -m "feat: DTOs de Producto, Carrito, Orden y clase Resultado genérica"
```

---

## Pomodoros 12-13 — Servicio del carrito

Este es el archivo más importante del día. Contiene la lógica de negocio del carrito y el checkout.

**Archivo: `Servicios/ICarritoServicio.cs`**

```csharp
using EcommerceNet.Core.DTOs;

namespace EcommerceNet.Core.Servicios;

public interface ICarritoServicio
{
    Task<Resultado<CarritoDto>> ObtenerCarritoAsync(int usuarioId);
    Task<Resultado<CarritoDto>> AgregarProductoAsync(int usuarioId, AgregarAlCarritoDto dto);
    Task<Resultado<CarritoDto>> ActualizarCantidadAsync(int usuarioId, int productoId, int cantidad);
    Task<Resultado<CarritoDto>> EliminarProductoAsync(int usuarioId, int productoId);
    Task<Resultado<CarritoDto>> VaciarCarritoAsync(int usuarioId);
    Task<Resultado<OrdenDto>> CheckoutAsync(int usuarioId, CrearOrdenDto dto);
}
```

**Archivo: `Servicios/CarritoServicio.cs`**

```csharp
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Enums;
using EcommerceNet.Core.Interfaces;

namespace EcommerceNet.Core.Servicios;

/// <summary>
/// Servicio de carrito — toda la lógica de negocio:
/// agregar, quitar, actualizar y el proceso de checkout completo.
/// </summary>
public class CarritoServicio : ICarritoServicio
{
    private readonly IUnidadDeTrabajo _uow;

    public CarritoServicio(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    public async Task<Resultado<CarritoDto>> ObtenerCarritoAsync(int usuarioId)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Ok(new CarritoDto());
        return Resultado<CarritoDto>.Ok(MapearCarrito(carrito));
    }

    public async Task<Resultado<CarritoDto>> AgregarProductoAsync(
        int usuarioId, AgregarAlCarritoDto dto)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(dto.ProductoId);
        if (producto == null)
            return Resultado<CarritoDto>.Error("Producto no encontrado");
        if (!producto.TieneStockSuficiente(dto.Cantidad))
            return Resultado<CarritoDto>.Error($"Stock insuficiente. Disponible: {producto.Stock}");

        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null)
        {
            carrito = new Carrito { UsuarioId = usuarioId };
            await _uow.Carritos.AgregarAsync(carrito);
        }

        carrito.AgregarProducto(producto, dto.Cantidad);
        _uow.Carritos.Actualizar(carrito);
        await _uow.GuardarCambiosAsync();

        return Resultado<CarritoDto>.Ok(MapearCarrito(carrito), $"'{producto.Nombre}' agregado");
    }

    public async Task<Resultado<CarritoDto>> ActualizarCantidadAsync(
        int usuarioId, int productoId, int cantidad)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Error("Carrito no encontrado");

        try
        {
            carrito.ActualizarCantidad(productoId, cantidad);
            _uow.Carritos.Actualizar(carrito);
            await _uow.GuardarCambiosAsync();
            return Resultado<CarritoDto>.Ok(MapearCarrito(carrito));
        }
        catch (InvalidOperationException ex)
        {
            return Resultado<CarritoDto>.Error(ex.Message);
        }
    }

    public async Task<Resultado<CarritoDto>> EliminarProductoAsync(
        int usuarioId, int productoId)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Error("Carrito no encontrado");

        try
        {
            carrito.EliminarProducto(productoId);
            _uow.Carritos.Actualizar(carrito);
            await _uow.GuardarCambiosAsync();
            return Resultado<CarritoDto>.Ok(MapearCarrito(carrito), "Producto eliminado");
        }
        catch (InvalidOperationException ex)
        {
            return Resultado<CarritoDto>.Error(ex.Message);
        }
    }

    public async Task<Resultado<CarritoDto>> VaciarCarritoAsync(int usuarioId)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Error("Carrito no encontrado");

        carrito.Vaciar();
        _uow.Carritos.Actualizar(carrito);
        await _uow.GuardarCambiosAsync();
        return Resultado<CarritoDto>.Ok(new CarritoDto(), "Carrito vaciado");
    }

    /// <summary>
    /// CHECKOUT — el proceso más crítico:
    /// 1. Validar carrito no vacío
    /// 2. Verificar stock de cada producto
    /// 3. Crear orden con detalles
    /// 4. Reducir stock
    /// 5. Vaciar carrito
    /// Todo en UNA transacción (si falla uno, falla todo)
    /// </summary>
    public async Task<Resultado<OrdenDto>> CheckoutAsync(
        int usuarioId, CrearOrdenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DireccionEnvio))
            return Resultado<OrdenDto>.Error("La dirección de envío es obligatoria");

        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null || carrito.EstaVacio())
            return Resultado<OrdenDto>.Error("El carrito está vacío");

        // Verificar stock de cada producto
        var errores = new List<string>();
        foreach (var item in carrito.Items)
        {
            var prod = await _uow.Productos.ObtenerPorIdAsync(item.ProductoId);
            if (prod == null || !prod.TieneStockSuficiente(item.Cantidad))
                errores.Add($"'{item.Producto?.Nombre}': stock insuficiente");
        }
        if (errores.Count > 0)
            return Resultado<OrdenDto>.ErrorValidacion(errores);

        // Crear la orden
        var orden = new Orden
        {
            UsuarioId = usuarioId,
            DireccionEnvio = dto.DireccionEnvio,
            Estado = EstadoOrden.Pendiente
        };

        foreach (var item in carrito.Items)
        {
            var prod = await _uow.Productos.ObtenerPorIdAsync(item.ProductoId);
            if (prod == null) continue;

            var detalle = new OrdenDetalle
            {
                ProductoId = item.ProductoId,
                Producto = prod,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            };
            detalle.CalcularSubtotal();
            orden.Detalles.Add(detalle);

            prod.ReducirStock(item.Cantidad);
            _uow.Productos.Actualizar(prod);
        }

        orden.RecalcularTotal();
        await _uow.Ordenes.AgregarAsync(orden);

        carrito.Vaciar();
        _uow.Carritos.Actualizar(carrito);

        await _uow.GuardarCambiosAsync();

        orden.GenerarNumeroOrden();
        _uow.Ordenes.Actualizar(orden);
        await _uow.GuardarCambiosAsync();

        return Resultado<OrdenDto>.Ok(MapearOrden(orden), $"Orden {orden.NumeroOrden} creada");
    }

    // --- Mapeos privados ---

    private static CarritoDto MapearCarrito(Carrito c) => new()
    {
        Id = c.Id,
        Total = c.CalcularTotal(),
        TotalProductos = c.TotalProductos(),
        Items = c.Items.Select(i => new CarritoItemDto
        {
            ProductoId = i.ProductoId,
            ProductoNombre = i.Producto?.Nombre ?? "",
            ImagenUrl = i.Producto?.ImagenUrl ?? "",
            PrecioUnitario = i.PrecioUnitario,
            Cantidad = i.Cantidad,
            Subtotal = i.CalcularSubtotal()
        }).ToList()
    };

    private static OrdenDto MapearOrden(Orden o) => new()
    {
        Id = o.Id,
        NumeroOrden = o.NumeroOrden,
        FechaCreacion = o.FechaCreacion,
        Estado = o.Estado.ToString(),
        Total = o.Total,
        Detalles = o.Detalles.Select(d => new OrdenDetalleDto
        {
            ProductoNombre = d.Producto?.Nombre ?? "",
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList()
    };
}
```

```powershell
git add .
git commit -m "feat: servicio de carrito con lógica completa de checkout"
```

---

## Pomodoros 14-15 — Pruebas unitarias

**Archivo: `tests/EcommerceNet.Tests/Entidades/ProductoTests.cs`**

```csharp
using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Tests.Entidades;

public class ProductoTests
{
    [Fact]
    public void TieneStockSuficiente_ConStock_Verdadero()
    {
        var p = new Producto { Stock = 10, Activo = true };
        Assert.True(p.TieneStockSuficiente(5));
    }

    [Fact]
    public void TieneStockSuficiente_SinStock_Falso()
    {
        var p = new Producto { Stock = 2, Activo = true };
        Assert.False(p.TieneStockSuficiente(5));
    }

    [Fact]
    public void TieneStockSuficiente_Inactivo_Falso()
    {
        var p = new Producto { Stock = 100, Activo = false };
        Assert.False(p.TieneStockSuficiente(1));
    }

    [Fact]
    public void ReducirStock_Suficiente_Reduce()
    {
        var p = new Producto { Stock = 10, Activo = true };
        p.ReducirStock(3);
        Assert.Equal(7, p.Stock);
    }

    [Fact]
    public void ReducirStock_Insuficiente_LanzaExcepcion()
    {
        var p = new Producto { Stock = 2, Activo = true, Nombre = "Test" };
        Assert.Throws<InvalidOperationException>(() => p.ReducirStock(5));
    }

    [Fact]
    public void AumentarStock_Positivo_Aumenta()
    {
        var p = new Producto { Stock = 5 };
        p.AumentarStock(10);
        Assert.Equal(15, p.Stock);
    }

    [Fact]
    public void AumentarStock_Cero_LanzaExcepcion()
    {
        var p = new Producto { Stock = 5 };
        Assert.Throws<ArgumentException>(() => p.AumentarStock(0));
    }
}
```

**Archivo: `tests/EcommerceNet.Tests/Entidades/CarritoTests.cs`**

```csharp
using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Tests.Entidades;

public class CarritoTests
{
    private Producto CrearProducto(int id = 1, decimal precio = 100m, int stock = 10)
        => new() { Id = id, Nombre = $"Producto {id}", Precio = precio, Stock = stock, Activo = true };

    [Fact]
    public void AgregarProducto_Nuevo_SeAgrega()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(), 2);
        Assert.Single(c.Items);
        Assert.Equal(2, c.Items[0].Cantidad);
    }

    [Fact]
    public void AgregarProducto_Existente_IncrementaCantidad()
    {
        var c = new Carrito();
        var p = CrearProducto();
        c.AgregarProducto(p, 2);
        c.AgregarProducto(p, 3);
        Assert.Single(c.Items);
        Assert.Equal(5, c.Items[0].Cantidad);
    }

    [Fact]
    public void AgregarProducto_SinStock_LanzaExcepcion()
    {
        var c = new Carrito();
        Assert.Throws<InvalidOperationException>(() =>
            c.AgregarProducto(CrearProducto(stock: 2), 5));
    }

    [Fact]
    public void CalcularTotal_VariosItems_SumaCorrecto()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1, 100m), 2);  // 200
        c.AgregarProducto(CrearProducto(2, 50m), 3);   // 150
        Assert.Equal(350m, c.CalcularTotal());
    }

    [Fact]
    public void TotalProductos_VariosItems_Correcto()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1), 2);
        c.AgregarProducto(CrearProducto(2), 3);
        Assert.Equal(5, c.TotalProductos());
    }

    [Fact]
    public void ActualizarCantidad_Existente_Actualiza()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(), 2);
        c.ActualizarCantidad(1, 5);
        Assert.Equal(5, c.Items[0].Cantidad);
    }

    [Fact]
    public void ActualizarCantidad_Cero_EliminaItem()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(), 2);
        c.ActualizarCantidad(1, 0);
        Assert.Empty(c.Items);
    }

    [Fact]
    public void EliminarProducto_Existente_Elimina()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1), 2);
        c.AgregarProducto(CrearProducto(2), 3);
        c.EliminarProducto(1);
        Assert.Single(c.Items);
    }

    [Fact]
    public void EliminarProducto_NoExiste_LanzaExcepcion()
    {
        var c = new Carrito();
        Assert.Throws<InvalidOperationException>(() => c.EliminarProducto(999));
    }

    [Fact]
    public void Vaciar_ConItems_QuedaVacio()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1), 2);
        c.AgregarProducto(CrearProducto(2), 3);
        c.Vaciar();
        Assert.True(c.EstaVacio());
    }
}
```

**Archivo: `tests/EcommerceNet.Tests/Entidades/OrdenTests.cs`**

```csharp
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Enums;

namespace EcommerceNet.Tests.Entidades;

public class OrdenTests
{
    [Fact]
    public void RecalcularTotal_SumaDetalles()
    {
        var o = new Orden { Detalles = new()
        {
            new() { Subtotal = 200m },
            new() { Subtotal = 150m }
        }};
        o.RecalcularTotal();
        Assert.Equal(350m, o.Total);
    }

    [Fact]
    public void SePuedeCancelar_Pendiente_Verdadero()
    {
        Assert.True(new Orden { Estado = EstadoOrden.Pendiente }.SePuedeCancelar());
    }

    [Fact]
    public void SePuedeCancelar_Enviada_Falso()
    {
        Assert.False(new Orden { Estado = EstadoOrden.Enviada }.SePuedeCancelar());
    }

    [Fact]
    public void Cancelar_Pendiente_DevuelveStock()
    {
        var prod = new Producto { Stock = 5, Activo = true };
        var o = new Orden
        {
            Estado = EstadoOrden.Pendiente,
            Detalles = new() { new() { Cantidad = 3, Producto = prod } }
        };
        o.Cancelar();
        Assert.Equal(EstadoOrden.Cancelada, o.Estado);
        Assert.Equal(8, prod.Stock);
    }

    [Fact]
    public void Cancelar_Enviada_LanzaExcepcion()
    {
        var o = new Orden { Estado = EstadoOrden.Enviada };
        Assert.Throws<InvalidOperationException>(() => o.Cancelar());
    }
}
```

### Ejecutar pruebas

```powershell
dotnet test
# Esperado: Passed! - Failed: 0, Passed: 22, Skipped: 0
```

```powershell
git add .
git commit -m "test: 22 pruebas unitarias para Producto, Carrito y Orden"
```

---

## Pomodoro 16 — Merge y cierre

```powershell
# Verificar todo
dotnet build
dotnet test

# Merge a desarrollo
git checkout desarrollo
git merge dia-01/fundamentos-csharp
git push origin desarrollo

# Merge a main
git checkout main
git merge desarrollo
git push origin main
```

---

## Simulador de entrevista DaCodes — Día 1

Basado en entrevistas reales reportadas en Glassdoor:

**Pregunta 1:** "Cuéntame sobre tu perfil. ¿Qué lenguajes y herramientas manejas?"
> Respuesta preparada: "Mi stack principal es .NET con C# para backend (ASP.NET Core y experiencia con MVC), Vue.js para frontend, y SQL Server para datos. Uso VS Code como IDE, Git para control de versiones, y tengo experiencia consumiendo y diseñando APIs REST. También he trabajado con jQuery en proyectos legacy."

**Pregunta 2:** "¿Qué es Clean Architecture y por qué la usarías?"
> Respuesta: "Es un patrón que organiza el código en capas con una dirección estricta de dependencias. El núcleo de negocio (Core) no depende de frameworks ni bases de datos. Esto permite cambiar la infraestructura sin tocar la lógica, facilita las pruebas unitarias, y hace el código más mantenible en equipos grandes como los de DaCodes."

**Pregunta 3:** "¿Qué es la inyección de dependencias y los 3 ciclos de vida en .NET?"
> Respuesta: "Es un patrón donde las dependencias se pasan por constructor en lugar de crear instancias directamente. .NET tiene 3 ciclos: `AddTransient` crea una instancia nueva cada vez que se solicita, `AddScoped` crea una por request HTTP (ideal para repositorios y DbContext), y `AddSingleton` crea una sola instancia para toda la aplicación (ideal para configuración o caché)."

---

## Mañana: Día 2

Crearemos los controladores de la API REST con ASP.NET Core: endpoints de productos, carrito,
checkout, autenticación JWT y Swagger. Todo conectado a los servicios que construimos hoy.

Rama: `dia-02/aspnet-api`
