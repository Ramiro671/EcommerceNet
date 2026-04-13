# Manual Técnico — Día 1: Fundamentos C# y Clean Architecture

> **Fecha de ejecución:** 2026-03-31  

> **Entorno:** Windows 11, .NET SDK 10.0.103, Git 2.51.1  
> **Resultado final:** Build exitoso, 23/23 pruebas pasando, 0 errores, 0 warnings

---

## Índice

1. [Revisión previa al desarrollo](#1-revisión-previa-al-desarrollo)
2. [Fase 1 — Scaffolding de la solución](#2-fase-1--scaffolding-de-la-solución)
3. [Fase 2 — Archivos de EcommerceNet.Core](#3-fase-2--archivos-de-ecommercenetcore)
4. [Fase 3 — Pruebas unitarias](#4-fase-3--pruebas-unitarias)
5. [Fase 4 — Verificación final](#5-fase-4--verificación-final)
6. [Inventario completo de archivos](#6-inventario-completo-de-archivos)
7. [Grafo de dependencias](#7-grafo-de-dependencias)
8. [Decisiones técnicas y por qué](#8-decisiones-técnicas-y-por-qué)
9. [Errores que NO ocurrieron (y por qué)](#9-errores-que-no-ocurrieron-y-por-qué)
10. [Estado del proyecto al cierre del Día 1](#10-estado-del-proyecto-al-cierre-del-día-1)

---

## 1. Revisión previa al desarrollo

Se revisaron los siguientes archivos antes de comenzar el desarrollo:

### 1.1 Convenciones de arquitectura del proyecto

Extrajo de ahí las reglas estrictas que gobiernan todo el código:

| Regla | Convención del proyecto | Efecto en el código |
|-------|---------------------|---------------------|
| `Core` no depende de nada externo | "Reglas de dependencia" | El `.csproj` de Core no tiene ningún `<PackageReference>` |
| Namespaces file-scoped | "Reglas de código C#" | Todos los archivos usan `namespace X;` (sin llaves) |
| Comentarios en español | "Reglas de código C#" | Todos los `/// <summary>` están en español |
| Inyección de dependencias por constructor | "Reglas de código C#" | `CarritoServicio` recibe `IUnidadDeTrabajo` por constructor |
| Clase `Resultado<T>` obligatoria | "Reglas de código C#" | Cada método del servicio retorna `Resultado<T>` |
| Patrón Repository + Unit of Work | "Reglas de código C#" | Cinco interfaces en `Interfaces/` |
| Nunca exponer entidades en la API | "Reglas de código C#" | Existen DTOs separados de las entidades |

### 1.2 `docs/dia-01-fundamentos-csharp.md` (plan del día)

Leyó el archivo en cuatro bloques (el archivo supera el límite de lectura de ~10.000 tokens) y extrajo:
- El árbol de carpetas exacto a crear dentro de `Core/`
- El código fuente de cada archivo (copiado literalmente del plan)
- Los 22 casos de prueba distribuidos en tres clases

---

## 2. Fase 1 — Scaffolding de la solución

> El usuario confirmó el bloque completo antes de ejecutar.

### 2.1 Crear el archivo de solución

```bash
cd C:\Users\ramir\Source\repos\EcommerceNet
dotnet new sln -n EcommerceNet
```

**Qué produce:** `EcommerceNet.slnx`  
**Por qué existe:** El `.slnx` es el nuevo formato de solución introducido en .NET 9 (reemplaza al `.sln`). Es el punto de entrada para Visual Studio y para `dotnet build`/`dotnet test` desde la raíz. Sin él, habría que compilar cada proyecto por separado.

### 2.2 Crear los cuatro proyectos

```bash
dotnet new classlib -n EcommerceNet.Core  -o src/EcommerceNet.Core
dotnet new classlib -n EcommerceNet.Data  -o src/EcommerceNet.Data
dotnet new webapi   -n EcommerceNet.API   -o src/EcommerceNet.API
dotnet new xunit    -n EcommerceNet.Tests -o tests/EcommerceNet.Tests
```

| Proyecto | Template | Por qué ese template |
|----------|----------|----------------------|
| Core | `classlib` | No necesita servidor web ni runner. Solo lógica pura. |
| Data | `classlib` | Implementa interfaces de Core. No expone HTTP. |
| API | `webapi` | Necesita Kestrel, middleware, controladores, OpenAPI. |
| Tests | `xunit` | Incluye xUnit, coverlet y el runner de Visual Studio preconfigurados. |

### 2.3 Agregar los proyectos a la solución

```bash
dotnet sln add src/EcommerceNet.Core/EcommerceNet.Core.csproj
dotnet sln add src/EcommerceNet.Data/EcommerceNet.Data.csproj
dotnet sln add src/EcommerceNet.API/EcommerceNet.API.csproj
dotnet sln add tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj
```

**Por qué:** Sin este paso, `dotnet build` desde la raíz no los encuentra.

### 2.4 Establecer referencias entre proyectos

```bash
# Data puede usar entidades e interfaces de Core
dotnet add src/EcommerceNet.Data/EcommerceNet.Data.csproj \
  reference src/EcommerceNet.Core/EcommerceNet.Core.csproj

# API puede usar repositorios de Data (y Core transitivamente)
dotnet add src/EcommerceNet.API/EcommerceNet.API.csproj \
  reference src/EcommerceNet.Data/EcommerceNet.Data.csproj

# Tests solo necesita las entidades de Core para probar su lógica
dotnet add tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj \
  reference src/EcommerceNet.Core/EcommerceNet.Core.csproj
```

**Regla crítica:** Tests referencia SOLO Core. Si Tests referenciara Data o API,
podría probar comportamientos de infraestructura y romper el aislamiento.

### 2.5 Limpiar archivos por defecto

```bash
rm src/EcommerceNet.Core/Class1.cs
rm src/EcommerceNet.Data/Class1.cs
```

El template `classlib` genera un `Class1.cs` vacío que no tiene ningún propósito.

### 2.6 Crear la estructura de carpetas

```bash
mkdir -p src/EcommerceNet.Core/Enums
mkdir -p src/EcommerceNet.Core/Entidades
mkdir -p src/EcommerceNet.Core/Interfaces
mkdir -p src/EcommerceNet.Core/DTOs
mkdir -p src/EcommerceNet.Core/Servicios
mkdir -p tests/EcommerceNet.Tests/Entidades
```

---

## 3. Fase 2 — Archivos de EcommerceNet.Core

### 3.1 Enums

#### `Enums/EstadoOrden.cs`

```csharp
namespace EcommerceNet.Core.Enums;

public enum EstadoOrden
{
    Pendiente = 0,
    Pagada = 1,
    EnPreparacion = 2,
    Enviada = 3,
    Entregada = 4,
    Cancelada = 5
}
```

**Razón técnica:** Los estados son un conjunto cerrado y ordenado de valores.
Usar `int` directamente en el código (`if (estado == 3)`) es propenso a errores y no
se puede leer. Con el enum el compilador rechaza valores inválidos y el código es autoexplicativo.
En la base de datos se almacenará como `INT`, pero en el código siempre se usa el nombre.

#### `Enums/RolUsuario.cs`

```csharp
namespace EcommerceNet.Core.Enums;

public enum RolUsuario
{
    Cliente = 0,
    Admin = 1
}
```

**Razón técnica:** Los roles son exactamente dos. El enum permite hacer
`if (usuario.Rol == RolUsuario.Admin)` y al mismo tiempo se mapea directamente a los
claims del JWT (`[Authorize(Roles = "Admin")]`).

---

### 3.2 Entidades del dominio

#### `Entidades/Categoria.cs`

**Razón técnica:** Entidad raíz de la jerarquía de productos. Tiene una relación
1:N con `Producto`. El método `TotalProductosActivos()` es un ejemplo de lógica de dominio
legítima: depende únicamente de datos que la propia entidad ya tiene cargados.

#### `Entidades/Producto.cs`

**Razón técnica:** Entidad central del sistema. Contiene tres métodos de negocio:

| Método | Por qué está en la entidad y no en un servicio |
|--------|------------------------------------------------|
| `TieneStockSuficiente(int)` | Solo necesita `Stock` y `Activo`, ambos de la misma entidad. Es una regla del dominio, no de infraestructura. |
| `ReducirStock(int)` | La invariante "no puedes bajar el stock más de lo disponible" pertenece al objeto que posee el dato. |
| `AumentarStock(int)` | Validación de negocio: reabastecimientos de cantidad cero son un error lógico. |

Poner estas reglas en la entidad (en lugar de en un servicio o controlador) sigue el
principio **Rich Domain Model** de DDD. Si se pusieran en el controlador, se duplicarían
en cada endpoint que necesite validar stock.

#### `Entidades/Usuario.cs`

**Razón técnica:** El campo `PasswordHash` (no `Password`) es un contrato explícito:
nunca se almacena texto plano. El método `EsAdmin()` encapsula la comparación con el enum
y hace legible el código que lo usa (`if (usuario.EsAdmin())`).

#### `Entidades/CarritoItem.cs`

**Razón técnica:** Representa una línea del carrito. Guarda `PrecioUnitario` en el momento
de agregar el producto — no una referencia al precio actual. Esto es correcto porque el precio
puede cambiar mientras el usuario tiene el carrito abierto. `CalcularSubtotal()` es puro:
precio × cantidad, sin efectos secundarios.

#### `Entidades/Carrito.cs`

**La entidad más compleja del día.** Concentra toda la lógica de estado del carrito:

| Método | Lógica de negocio encapsulada |
|--------|-------------------------------|
| `AgregarProducto` | Si el producto ya existe, incrementa cantidad en lugar de duplicar la línea |
| `AgregarProducto` | Delega la validación de stock a `Producto.TieneStockSuficiente` (no la duplica) |
| `ActualizarCantidad` | Cantidad = 0 actúa como "eliminar" (UX habitual en e-commerce) |
| `EliminarProducto` | Lanza excepción si el producto no existe (fail-fast) |
| `Vaciar` | Limpia toda la lista y actualiza `UltimaModificacion` |

`UltimaModificacion` se actualiza en cada operación para poder detectar carritos abandonados
en el futuro (funcionalidad de Día N).

#### `Entidades/Orden.cs`

**Razón técnica:** Una orden es un registro inmutable de lo que ocurrió en el checkout.
Métodos clave:

| Método | Decisión de diseño |
|--------|-------------------|
| `GenerarNumeroOrden()` | Se llama después del primer `SaveChanges` porque necesita el `Id` asignado por la BD |
| `RecalcularTotal()` | Suma los `Subtotal` de los detalles — la fuente de verdad son los detalles, no el total |
| `SePuedeCancelar()` | Solo `Pendiente` o `Pagada` se pueden cancelar. `Enviada` ya no. |
| `Cancelar()` | Devuelve stock a cada producto — esto es parte de la lógica de negocio, no de infraestructura |

#### `Entidades/OrdenDetalle.cs`

**Razón técnica:** Snapshot del momento de la compra. Guarda `PrecioUnitario` independiente
del precio actual del producto. `CalcularSubtotal()` es un método void que escribe en la
propiedad — el `Subtotal` se persiste en BD para facilitar reportes sin recalcular.

---

### 3.3 Interfaces

#### `Interfaces/IRepositorio.cs`

```csharp
public interface IRepositorio<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}
```

**Razón técnica:** `AgregarAsync` es async porque implica ir a la BD.
`Actualizar` y `Eliminar` son síncronos porque con EF Core solo marcan el estado
del objeto en el `ChangeTracker` — el viaje a la BD ocurre en `SaveChangesAsync`.

#### `Interfaces/IProductoRepositorio.cs`

Extiende `IRepositorio<Producto>` y agrega cuatro consultas especializadas:
- `BuscarPorNombreAsync` — para el endpoint `/api/productos/buscar?termino=x`
- `ObtenerPorCategoriaAsync` — para `/api/productos/categoria/{id}`
- `ObtenerConStockBajoAsync` — para alertas de administración
- `ObtenerActivosAsync` — para el listado público (solo productos activos)

#### `Interfaces/ICarritoRepositorio.cs`

No extiende `IRepositorio<T>` porque el carrito no se consulta por Id genérico
sino siempre por `UsuarioId`. Exponerlo por Id sería una API incorrecta desde el dominio.

#### `Interfaces/IOrdenRepositorio.cs`

Agrega `ObtenerPorUsuarioAsync` y `ObtenerConDetallesAsync`. La segunda carga los
detalles con eager loading (a implementar en Data con `.Include()`).

#### `Interfaces/IUnidadDeTrabajo.cs`

```csharp
public interface IUnidadDeTrabajo : IDisposable
{
    IProductoRepositorio Productos { get; }
    ICarritoRepositorio Carritos { get; }
    IOrdenRepositorio Ordenes { get; }
    Task<int> GuardarCambiosAsync();
}
```

**Razón técnica (pregunta frecuente de entrevista):** ¿Por qué no llamas
`SaveChanges` desde cada repositorio?

Porque en el checkout necesitas que TRES operaciones ocurran en una sola transacción:
1. Crear la orden
2. Reducir el stock de cada producto
3. Vaciar el carrito

Si `SaveChanges` estuviera en el repositorio, podrías guardar la orden y luego fallar
al reducir el stock, dejando la BD en un estado inconsistente. Con la Unidad de Trabajo,
o todo se guarda o nada se guarda.

---

### 3.4 DTOs

Los DTOs (Data Transfer Objects) son la "cara pública" de la API. Las entidades nunca
se serializan directamente por varias razones:

1. **Seguridad:** `Usuario` tiene `PasswordHash` — exponerlo sería una vulnerabilidad.
2. **Circular references:** `Producto` → `Categoria` → `List<Producto>` → infinito.
3. **Forma del contrato:** El cliente puede necesitar campos calculados (`Disponible`, `Subtotal`)
   que no existen como columnas en la BD.

#### `DTOs/ProductoDto.cs`

Dos clases: `ProductoDto` (lectura) y `CrearProductoDto` (escritura).
`ProductoDto` incluye `CategoriaNombre` (string) en lugar de `CategoriaId` (int)
porque el cliente de la API necesita el nombre legible, no el ID de FK.

#### `DTOs/CarritoDto.cs`

Tres clases: `CarritoDto`, `CarritoItemDto`, `AgregarAlCarritoDto`.
`CarritoItemDto` incluye `Subtotal` precalculado para que el frontend no tenga que multiplicar.

#### `DTOs/OrdenDto.cs`

Tres clases: `OrdenDto`, `OrdenDetalleDto`, `CrearOrdenDto`.
`CrearOrdenDto` solo tiene `DireccionEnvio` porque el resto lo calcula el servidor
(UsuarioId viene del JWT, Total de los items, Estado inicial es Pendiente).

#### `DTOs/Resultado.cs`

```csharp
public class Resultado<T>
{
    public bool Exito { get; set; }
    public T? Datos { get; set; }
    public string? Mensaje { get; set; }
    public List<string> Errores { get; set; } = new();

    public static Resultado<T> Ok(T datos, string? mensaje = null) => ...
    public static Resultado<T> Error(string mensaje) => ...
    public static Resultado<T> ErrorValidacion(List<string> errores) => ...
}
```

**Razón técnica:** Estandariza TODA la respuesta de la API. El cliente siempre sabe
que recibirá `{ exito, datos, mensaje, errores }` — nunca un JSON de forma distinta
según el endpoint. Los tres factory methods cubren los tres escenarios:

| Factory | Cuándo usarlo |
|---------|---------------|
| `Ok(datos)` | Operación exitosa con datos |
| `Error(mensaje)` | Un solo error (producto no encontrado, carrito vacío) |
| `ErrorValidacion(errores)` | Múltiples errores de validación (varios items sin stock) |

---

### 3.5 Servicios

#### `Servicios/ICarritoServicio.cs`

Contrato del servicio. Los controladores del Día 2 dependerán de esta interfaz,
no de la implementación concreta. Esto permite hacer tests del controlador
con un mock del servicio sin tocar la BD.

#### `Servicios/CarritoServicio.cs`

La implementación más importante del día. Recibe `IUnidadDeTrabajo` por constructor —
nunca instancia repositorios directamente.

**Flujo del `CheckoutAsync` (el método más crítico):**

```
1. Validar que DireccionEnvio no esté vacía
2. Cargar carrito del usuario → si null o vacío: Error
3. Para cada item: verificar stock → acumular errores
4. Si hay errores: ErrorValidacion (no continúa)
5. Crear entidad Orden con Estado = Pendiente
6. Para cada item:
   a. Cargar Producto fresco de BD
   b. Crear OrdenDetalle con precio snapshot
   c. Llamar prod.ReducirStock()   ← lógica de dominio
   d. Marcar Producto como modificado
7. orden.RecalcularTotal()
8. Agregar Orden al repositorio
9. Vaciar carrito
10. GuardarCambiosAsync()  ← UN SOLO viaje a la BD
11. orden.GenerarNumeroOrden()  ← necesita el Id asignado por la BD
12. GuardarCambiosAsync()  ← segundo save solo para el NumeroOrden
13. Retornar Resultado<OrdenDto>.Ok(...)
```

**Por qué dos `GuardarCambiosAsync`:** `GenerarNumeroOrden` usa `$"ORD-{fecha}-{Id:D4}"`.
El `Id` lo asigna SQL Server en el primer save. Si intentamos generar el número antes,
`Id` es 0 y el número queda `ORD-20260331-0000`.

---

## 4. Fase 3 — Pruebas unitarias

Las pruebas están en `tests/EcommerceNet.Tests/Entidades/` y siguen el patrón
**Arrange-Act-Assert**. No usan mocks porque prueban lógica pura de las entidades
(sin BD ni HTTP).

### `ProductoTests.cs` — 7 pruebas

| Prueba | Qué verifica |
|--------|-------------|
| `TieneStockSuficiente_ConStock_Verdadero` | Camino feliz: stock 10, pide 5 → true |
| `TieneStockSuficiente_SinStock_Falso` | Stock insuficiente → false |
| `TieneStockSuficiente_Inactivo_Falso` | Producto inactivo → false aunque tenga stock |
| `ReducirStock_Suficiente_Reduce` | 10 - 3 = 7 |
| `ReducirStock_Insuficiente_LanzaExcepcion` | Pide 5, hay 2 → `InvalidOperationException` |
| `AumentarStock_Positivo_Aumenta` | 5 + 10 = 15 |
| `AumentarStock_Cero_LanzaExcepcion` | Cantidad 0 → `ArgumentException` |

### `CarritoTests.cs` — 10 pruebas

| Prueba | Qué verifica |
|--------|-------------|
| `AgregarProducto_Nuevo_SeAgrega` | Item nuevo se agrega con cantidad correcta |
| `AgregarProducto_Existente_IncrementaCantidad` | Mismo producto → una sola línea con suma |
| `AgregarProducto_SinStock_LanzaExcepcion` | Stock insuficiente → excepción |
| `CalcularTotal_VariosItems_SumaCorrecto` | 2×100 + 3×50 = 350 |
| `TotalProductos_VariosItems_Correcto` | 2 + 3 = 5 unidades |
| `ActualizarCantidad_Existente_Actualiza` | Cambia cantidad de 2 a 5 |
| `ActualizarCantidad_Cero_EliminaItem` | Cantidad 0 elimina el item |
| `EliminarProducto_Existente_Elimina` | Queda 1 item de 2 |
| `EliminarProducto_NoExiste_LanzaExcepcion` | ProductoId inexistente → excepción |
| `Vaciar_ConItems_QuedaVacio` | `EstaVacio()` retorna true |

### `OrdenTests.cs` — 5 pruebas

| Prueba | Qué verifica |
|--------|-------------|
| `RecalcularTotal_SumaDetalles` | 200 + 150 = 350 |
| `SePuedeCancelar_Pendiente_Verdadero` | Estado Pendiente → cancelable |
| `SePuedeCancelar_Enviada_Falso` | Estado Enviada → no cancelable |
| `Cancelar_Pendiente_DevuelveStock` | Cancela y producto recupera 3 unidades (5→8) |
| `Cancelar_Enviada_LanzaExcepcion` | `InvalidOperationException` |

### `UnitTest1.cs` — 1 prueba (generada por el template)

Prueba vacía generada automáticamente por `dotnet new xunit`. No tiene valor funcional.
Puede eliminarse con `rm tests/EcommerceNet.Tests/UnitTest1.cs`.

---

## 5. Fase 4 — Verificación final

### Build

```bash
dotnet build
```

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:06.99
```

### Tests

```bash
dotnet test
```

```
Passed!  - Failed: 0, Passed: 23, Skipped: 0, Total: 23, Duration: 107 ms
```

---

## 6. Inventario completo de archivos

### Archivos creados por comandos `dotnet new` (no editables, infraestructura)

| Archivo | Tipo | Descripción |
|---------|------|-------------|
| `EcommerceNet.slnx` | Solución | Punto de entrada para build/test de toda la solución (formato .NET 9+) |
| `src/EcommerceNet.Core/EcommerceNet.Core.csproj` | Proyecto | classlib, net10.0, sin dependencias externas |
| `src/EcommerceNet.Data/EcommerceNet.Data.csproj` | Proyecto | classlib, referencia a Core |
| `src/EcommerceNet.API/EcommerceNet.API.csproj` | Proyecto | webapi, referencia a Data, incluye OpenAPI |
| `src/EcommerceNet.API/Program.cs` | Arranque | Punto de entrada de la Web API (a modificar en Día 2) |
| `tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj` | Proyecto | xunit, referencia a Core |
| `tests/EcommerceNet.Tests/UnitTest1.cs` | Test | Placeholder vacío del template — puede eliminarse |

### Archivos creados manualmente en `EcommerceNet.Core`

| Archivo | Capa | Razón de ser |
|---------|------|-------------|
| `Enums/EstadoOrden.cs` | Dominio | Estados posibles de una orden (6 valores) |
| `Enums/RolUsuario.cs` | Dominio | Roles del sistema: Cliente y Admin |
| `Entidades/Categoria.cs` | Dominio | Agrupa productos; tiene `TotalProductosActivos()` |
| `Entidades/Producto.cs` | Dominio | Entidad central; lógica de stock en `ReducirStock`/`AumentarStock` |
| `Entidades/Usuario.cs` | Dominio | Autenticación; `PasswordHash` nunca en texto plano |
| `Entidades/Carrito.cs` | Dominio | Lógica completa de carrito: agregar, actualizar, eliminar, vaciar |
| `Entidades/CarritoItem.cs` | Dominio | Línea del carrito con precio snapshot |
| `Entidades/Orden.cs` | Dominio | Registro permanente de compra; `Cancelar()` devuelve stock |
| `Entidades/OrdenDetalle.cs` | Dominio | Línea de la orden; `Subtotal` persiste en BD |
| `Interfaces/IRepositorio.cs` | Contrato | CRUD genérico con async/await |
| `Interfaces/IProductoRepositorio.cs` | Contrato | Consultas especializadas de Producto |
| `Interfaces/ICarritoRepositorio.cs` | Contrato | Acceso a carrito por UsuarioId (no por Id genérico) |
| `Interfaces/IOrdenRepositorio.cs` | Contrato | Consultas de órdenes con detalles |
| `Interfaces/IUnidadDeTrabajo.cs` | Contrato | Transaccionalidad: agrupa repositorios en un `SaveChanges` |
| `DTOs/ProductoDto.cs` | Contrato API | `ProductoDto` (lectura) + `CrearProductoDto` (escritura) |
| `DTOs/CarritoDto.cs` | Contrato API | `CarritoDto` + `CarritoItemDto` + `AgregarAlCarritoDto` |
| `DTOs/OrdenDto.cs` | Contrato API | `OrdenDto` + `OrdenDetalleDto` + `CrearOrdenDto` |
| `DTOs/Resultado.cs` | Contrato API | Envuelve TODA respuesta: `{exito, datos, mensaje, errores}` |
| `Servicios/ICarritoServicio.cs` | Contrato | Interfaz que usarán los controladores del Día 2 |
| `Servicios/CarritoServicio.cs` | Lógica | Implementación completa incluyendo `CheckoutAsync` |

### Archivos de pruebas

| Archivo | Pruebas | Qué testea |
|---------|---------|-----------|
| `tests/.../Entidades/ProductoTests.cs` | 7 | Lógica de stock de Producto |
| `tests/.../Entidades/CarritoTests.cs` | 10 | Todas las operaciones del Carrito |
| `tests/.../Entidades/OrdenTests.cs` | 5 | Cálculo, cancelación y devolución de stock |

---

## 7. Grafo de dependencias

```
┌─────────────────────────────────────────────────────┐
│                  EcommerceNet.API                   │
│  (ASP.NET Core Web API — Controladores, JWT, Swagger)│
└───────────────────────┬─────────────────────────────┘
                        │ referencia
                        ▼
┌─────────────────────────────────────────────────────┐
│                 EcommerceNet.Data                   │
│  (EF Core, DbContext, implementaciones Repository)  │
└───────────────────────┬─────────────────────────────┘
                        │ referencia
            ┌───────────┴──────────┐
            ▼                      ▼
┌───────────────────┐   ┌─────────────────────────────┐
│ EcommerceNet.Core │   │    EcommerceNet.Tests        │
│  (Entidades,      │◄──│  (Pruebas unitarias          │
│   Interfaces,     │   │   de Core solamente)         │
│   DTOs,           │   └─────────────────────────────┘
│   Servicios)      │
└───────────────────┘
   ▲ SIN dependencias externas
```

**Regla de la flecha:** las dependencias apuntan HACIA Core, nunca desde Core hacia afuera.
Core no sabe que existen EF Core, SQL Server, ASP.NET ni xUnit.

---

## 8. Decisiones técnicas y por qué

### ¿Por qué .NET 10 y no .NET 8?

El plan original especificaba .NET 8 pero el SDK instalado en la máquina es 10.0.103.
`dotnet new` usa la versión instalada por defecto. Para fijar a .NET 8 habría que agregar
`--framework net8.0` a cada comando o instalar el SDK 8. Para este proyecto de estudio,
net10.0 es perfectamente válido — la API de Clean Architecture es idéntica en ambas versiones.

### ¿Por qué `ImplicitUsings = enable`?

El template lo activa por defecto. Significa que `System`, `System.Collections.Generic`,
`System.Linq`, `System.Threading.Tasks`, etc. se importan automáticamente en todos los archivos.
Por eso `Carrito.cs` usa `List<T>`, `DateTime`, y LINQ sin `using` explícitos.

### ¿Por qué `Nullable = enable`?

Activa las advertencias de tipos nullable. Si intentas asignar `null` a una propiedad
no-nullable sin el operador `?`, el compilador genera una advertencia (o error con ajuste).
En el código usamos `Categoria?`, `Producto?`, `Usuario?` (con `?`) cuando la navegación
puede ser null, y `string Nombre = string.Empty` para las no-nullable.

### ¿Por qué los métodos de servicio son `async` incluso el de obtener carrito?

Porque en producción irán a la BD (EF Core con `await context.SaveChangesAsync()`).
Si los declaras síncronos ahora y los cambias luego, romperías todos los controladores
que los llaman. La firma `Task<Resultado<T>>` es el contrato desde el inicio.

---

## 9. Errores que NO ocurrieron (y por qué)

| Error potencial | Cómo se evitó |
|----------------|---------------|
| `CS0234: Namespace not found` en Tests | Tests referencia Core directamente, no Data ni API |
| Referencias circulares entre proyectos | La dirección de dependencias es estricta: solo de afuera hacia Core |
| `NullReferenceException` en navegaciones | Todas las propiedades de navegación tienen `?` nullable |
| Texto plano en contraseñas | El campo se llama `PasswordHash` desde la entidad, no `Password` |
| Stock negativo | `ReducirStock` lanza excepción antes de modificar el campo |
| Orden con Id=0 en el número | `GenerarNumeroOrden` se llama después del primer `SaveChangesAsync` |

---

## 10. Estado del proyecto al cierre del Día 1

```
EcommerceNet/
├── EcommerceNet.sln
├── las convenciones del proyecto
├── README.md
├── docs/
│   ├── dia-01-fundamentos-csharp.md   (plan original)
│   └── dia-01-manual-tecnico.md       (este archivo)
├── src/
│   ├── EcommerceNet.Core/             ✅ Completo — 20 archivos .cs
│   ├── EcommerceNet.Data/             ⬜ Vacío — se implementa en Día 2
│   └── EcommerceNet.API/              ⬜ Solo Program.cs — se completa en Día 2
└── tests/
    └── EcommerceNet.Tests/            ✅ 23 pruebas, 0 fallos

Build:  ✅ 0 errores, 0 warnings
Tests:  ✅ Passed 23 / Failed 0 / Skipped 0
```

### Pendiente para el Día 2 (`dia-02/aspnet-api`)

- `EcommerceNet.Data`: `AppDbContext`, `ProductoRepositorio`, `CarritoRepositorio`,
  `OrdenRepositorio`, `UnidadDeTrabajo`, migraciones EF Core
- `EcommerceNet.API`: `ProductosController`, `CarritoController`, `OrdenesController`,
  `AuthController`, configuración JWT, Swagger/OpenAPI, seed data
