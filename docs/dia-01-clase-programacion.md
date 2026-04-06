# Clase de Programación — Día 1: C# desde cero con EcommerceNet

> **A quién va dirigido:** desarrollador que está aprendiendo C# desde cero.
> Cada línea de código del Día 1 está explicada en detalle: qué hace, por qué existe y
> cómo funciona por dentro. No se asume conocimiento previo de C#.

---

## Índice

1. [Antes de leer código — conceptos base de C#](#1-antes-de-leer-código--conceptos-base-de-c)
2. [Namespaces — cómo se organiza el código](#2-namespaces--cómo-se-organiza-el-código)
3. [Clases — el molde de los objetos](#3-clases--el-molde-de-los-objetos)
4. [Tipos de datos primitivos](#4-tipos-de-datos-primitivos)
5. [Propiedades y la sintaxis `{ get; set; }`](#5-propiedades-y-la-sintaxis--get-set-)
6. [Modificadores de acceso: `public`, `private`, `protected`](#6-modificadores-de-acceso-public-private-protected)
7. [Enums — conjuntos de constantes con nombre](#7-enums--conjuntos-de-constantes-con-nombre)
8. [Entidades del dominio — análisis línea a línea](#8-entidades-del-dominio--análisis-línea-a-línea)
9. [Interfaces — contratos sin implementación](#9-interfaces--contratos-sin-implementación)
10. [Genéricos — código que funciona para cualquier tipo](#10-genéricos--código-que-funciona-para-cualquier-tipo)
11. [DTOs — objetos de transferencia de datos](#11-dtos--objetos-de-transferencia-de-datos)
12. [Async/Await — programación asíncrona](#12-asyncawait--programación-asíncrona)
13. [Servicios y la inyección de dependencias](#13-servicios-y-la-inyección-de-dependencias)
14. [LINQ — consultas sobre colecciones](#14-linq--consultas-sobre-colecciones)
15. [Expresiones lambda](#15-expresiones-lambda)
16. [Manejo de errores — excepciones](#16-manejo-de-errores--excepciones)
17. [Operadores de C# usados en el proyecto](#17-operadores-de-c-usados-en-el-proyecto)
18. [Pruebas unitarias con xUnit](#18-pruebas-unitarias-con-xunit)
19. [Glosario rápido de palabras reservadas](#19-glosario-rápido-de-palabras-reservadas)

---

## 1. Antes de leer código — conceptos base de C#

### ¿Qué es C#?

C# (se lee "C sharp") es un lenguaje de programación orientado a objetos creado por Microsoft.
"Orientado a objetos" significa que todo el programa se organiza alrededor de **objetos**
que tienen **datos** (propiedades) y **comportamientos** (métodos).

### ¿Qué es .NET?

.NET es la plataforma que ejecuta tu código C#. Es como el motor de un auto:
tu código C# son las instrucciones, .NET es el motor que las hace funcionar.
Cuando escribes `dotnet run`, .NET compila tu código y lo ejecuta.

### ¿Cómo se ejecuta el código?

```
Tu archivo .cs  →  Compilador de C#  →  .dll (código intermedio)  →  .NET Runtime  →  Ejecución
```

El compilador detecta errores antes de que el programa corra. Por eso C# se llama
**lenguaje tipado**: si dices que una variable guarda un número y luego intentas guardar
texto, el compilador lo rechaza antes de ejecutar.

---

## 2. Namespaces — cómo se organiza el código

### El problema que resuelven

Imagina que en tu proyecto hay dos clases llamadas `Orden`: una de la tienda y otra de
una librería externa. ¿Cómo distingue el compilador cuál es cuál?
Los **namespaces** son carpetas lógicas para el código.

### Sintaxis clásica (con llaves)

```csharp
namespace EcommerceNet.Core.Entidades
{
    public class Producto
    {
        // código aquí
    }
}
```

### Sintaxis file-scoped (la que usamos — sin llaves)

```csharp
namespace EcommerceNet.Core.Entidades;  // punto y coma al final

public class Producto
{
    // todo el archivo pertenece a este namespace
}
```

Ambas son equivalentes. La file-scoped es más limpia porque evita un nivel de indentación.
Se introdujo en C# 10. **En este proyecto usamos siempre file-scoped.**

### Cómo se usan los namespaces

Para usar una clase de otro namespace, usas `using`:

```csharp
using EcommerceNet.Core.Enums;   // importa los enums

namespace EcommerceNet.Core.Entidades;

public class Usuario
{
    public RolUsuario Rol { get; set; }  // RolUsuario viene del namespace importado
}
```

Sin el `using`, tendrías que escribir:
```csharp
public EcommerceNet.Core.Enums.RolUsuario Rol { get; set; }  // muy largo
```

### Los namespaces en este proyecto

```
EcommerceNet.Core.Enums        → los dos enums
EcommerceNet.Core.Entidades    → las 7 entidades del dominio
EcommerceNet.Core.Interfaces   → los 5 contratos
EcommerceNet.Core.DTOs         → los 4 archivos de DTOs
EcommerceNet.Core.Servicios    → la interfaz y la implementación del servicio
EcommerceNet.Tests.Entidades   → las 3 clases de prueba
```

El punto (`.`) en el nombre es solo una convención visual — para el compilador
`EcommerceNet.Core.Enums` es un solo nombre, no tres niveles separados.

---

## 3. Clases — el molde de los objetos

### Qué es una clase

Una **clase** es un molde. Define qué datos y qué comportamientos tendrá cada objeto
creado a partir de ella.

```csharp
public class Producto           // la clase = el molde
{
    public string Nombre { get; set; } = string.Empty;  // dato
    public void ReducirStock(int cantidad) { ... }       // comportamiento
}
```

```csharp
var producto = new Producto();  // el objeto = una instancia del molde
producto.Nombre = "Laptop";     // asignamos datos al objeto concreto
```

### `new` — crear una instancia

`new Producto()` crea un **objeto nuevo** en memoria basado en el molde `Producto`.
Cada `new` crea un objeto independiente:

```csharp
var p1 = new Producto();
var p2 = new Producto();
p1.Nombre = "Laptop";
p2.Nombre = "Mouse";
// p1 y p2 son objetos distintos en memoria
```

### Inicialización con propiedades (object initializer)

En lugar de asignar línea por línea:

```csharp
var carrito = new Carrito();
carrito.UsuarioId = 5;
carrito.UltimaModificacion = DateTime.UtcNow;
```

Puedes usar la sintaxis de objeto inicializador con `{ }`:

```csharp
var carrito = new Carrito { UsuarioId = 5 };
```

Esto hace exactamente lo mismo pero en una línea. Lo vemos mucho en el servicio:

```csharp
var orden = new Orden
{
    UsuarioId = usuarioId,
    DireccionEnvio = dto.DireccionEnvio,
    Estado = EstadoOrden.Pendiente
};
```

### Métodos — los comportamientos de una clase

Un **método** es una función que pertenece a una clase:

```csharp
public bool TieneStockSuficiente(int cantidad)
//     ↑        ↑                    ↑
//  retorna   nombre del         parámetro de entrada
//  bool      método
{
    return Activo && Stock >= cantidad;
}
```

- **`public`** — quién puede llamarlo (ver sección 6)
- **`bool`** — tipo de dato que retorna (true o false)
- **`TieneStockSuficiente`** — nombre
- **`(int cantidad)`** — recibe un número entero como entrada
- **`return`** — devuelve el resultado al que llamó el método

Un método `void` no retorna nada:

```csharp
public void Vaciar()     // void = no retorna nada
{
    Items.Clear();
    UltimaModificacion = DateTime.UtcNow;
}
```

---

## 4. Tipos de datos primitivos

C# es **fuertemente tipado**: cada variable tiene un tipo y no puedes mezclarlos.

| Tipo | Qué guarda | Ejemplo en el proyecto |
|------|-----------|----------------------|
| `int` | Números enteros (sin decimales) | `Stock`, `Cantidad`, `Id` |
| `decimal` | Números con decimales, alta precisión | `Precio`, `Total`, `Subtotal` |
| `bool` | Verdadero (`true`) o Falso (`false`) | `Activo`, `Activa`, `Exito` |
| `string` | Texto | `Nombre`, `Email`, `DireccionEnvio` |
| `DateTime` | Fecha y hora | `FechaCreacion`, `FechaRegistro` |

### ¿Por qué `decimal` y no `double` para precios?

`double` guarda números con decimales, pero con errores de redondeo:
```csharp
double a = 0.1 + 0.2;   // resultado: 0.30000000000000004  ← ERROR
decimal b = 0.1m + 0.2m; // resultado: 0.3                 ← CORRECTO
```

Para dinero siempre usa `decimal`. La `m` al final (`100m`, `50m`) indica que el literal
es de tipo `decimal`.

### `string.Empty` — la cadena vacía

```csharp
public string Nombre { get; set; } = string.Empty;
```

`string.Empty` es equivalente a `""` (texto sin caracteres). Se prefiere porque
`string.Empty` es más explícito en la intención: "este campo empieza vacío, no es null".

### `DateTime.UtcNow` — la fecha y hora actual en UTC

```csharp
public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
```

- `DateTime` es el tipo que representa fecha y hora
- `.UtcNow` es una **propiedad estática** que devuelve la fecha/hora actual en UTC
  (UTC = Coordinated Universal Time — hora universal, sin zona horaria)

Usamos UTC y no la hora local porque los servidores pueden estar en distintos países.

---

## 5. Propiedades y la sintaxis `{ get; set; }`

### Qué es una propiedad

Una **propiedad** es un campo con control de acceso. En lugar de exponer directamente
la variable interna, defines cómo se lee y cómo se escribe.

### Versión completa (manual)

```csharp
private int _stock;           // campo privado (lo "real")

public int Stock              // propiedad pública (el acceso)
{
    get { return _stock; }    // leer: devuelve _stock
    set { _stock = value; }   // escribir: value es el nuevo valor
}
```

### Versión auto-implementada (la que usamos)

```csharp
public int Stock { get; set; }
```

El compilador genera automáticamente el campo privado oculto. Es equivalente a la versión
completa. **Siempre usamos esta forma en este proyecto.**

### Valor inicial de la propiedad

```csharp
public bool Activo { get; set; } = true;
```

El `= true` es el **valor por defecto**: cuando creas un `new Producto()`, `Activo`
ya es `true` sin que tengas que asignarlo. Si no pones valor inicial, los tipos tienen
sus propios defaults:

| Tipo | Valor por defecto |
|------|------------------|
| `int` | `0` |
| `decimal` | `0` |
| `bool` | `false` |
| `string` | `null` (¡peligroso! por eso usamos `= string.Empty`) |
| Clases | `null` |

### Propiedades de solo lectura (sin `set`)

```csharp
IProductoRepositorio Productos { get; }   // en IUnidadDeTrabajo
```

Solo tiene `get`, no `set`. Significa que desde fuera de la clase solo puedes
**leer** el valor, no modificarlo.

---

## 6. Modificadores de acceso: `public`, `private`, `protected`

Los modificadores controlan **desde dónde** se puede acceder a una clase, propiedad o método.

| Modificador | Quién puede acceder |
|------------|---------------------|
| `public` | Cualquiera desde cualquier parte |
| `private` | Solo la propia clase |
| `protected` | La clase y sus clases hijas (herencia) |
| `internal` | Solo dentro del mismo proyecto/ensamblado |

### En el proyecto

```csharp
public class Carrito              // la clase es pública — cualquiera puede usarla
{
    public int Id { get; set; }   // propiedad pública — cualquiera puede leer y escribir

    private readonly IUnidadDeTrabajo _uow;  // campo privado — solo CarritoServicio lo ve

    private static CarritoDto MapearCarrito(Carrito c) => ...  // método privado — interno
}
```

### `readonly` — solo escritura en el constructor

```csharp
private readonly IUnidadDeTrabajo _uow;
```

`readonly` significa que `_uow` solo puede asignarse **en el constructor** de la clase.
Después de eso, no puede cambiar. Esto garantiza que el servicio siempre tenga la misma
unidad de trabajo durante toda su vida.

### `static` — pertenece a la clase, no al objeto

```csharp
public static Resultado<T> Ok(T datos, string? mensaje = null) => ...
```

Un método o propiedad `static` pertenece a la **clase** en sí, no a cada instancia.
Lo llamas con el nombre de la clase:

```csharp
Resultado<CarritoDto>.Ok(carrito)   // ← nombre de la clase, no de un objeto
```

En cambio un método no-static se llama sobre un objeto:

```csharp
producto.ReducirStock(3)   // ← objeto producto
```

---

## 7. Enums — conjuntos de constantes con nombre

### El problema que resuelven

Sin enums, tendrías que usar números para representar estados:

```csharp
if (orden.Estado == 3)   // ¿qué significa 3? nadie lo sabe sin ver la documentación
```

Con enums:

```csharp
if (orden.Estado == EstadoOrden.Enviada)   // claro, legible, el compilador lo verifica
```

### `EstadoOrden.cs` — línea a línea

```csharp
namespace EcommerceNet.Core.Enums;
```
Este enum vive en el namespace de los Enums. Para usarlo en otro archivo hay que importarlo
con `using EcommerceNet.Core.Enums;`.

```csharp
/// <summary>
/// Estados posibles de una orden de compra
/// </summary>
```
Las tres barras `///` son **documentación XML**. Visual Studio y VS Code la muestran
como tooltip cuando usas el enum. No afecta la compilación.

```csharp
public enum EstadoOrden
```
- `public` — cualquiera puede usarlo
- `enum` — palabra reservada que indica que esto es un enumerador
- `EstadoOrden` — nombre del tipo

```csharp
{
    Pendiente = 0,      // recién creada
    Pagada = 1,         // pago confirmado
    EnPreparacion = 2,  // preparando envío
    Enviada = 3,        // en camino
    Entregada = 4,      // cliente recibió
    Cancelada = 5       // cancelada
}
```

Cada línea es un **miembro del enum** con su valor numérico. Los valores son opcionales
(si no los pones, C# empieza en 0 y suma 1 automáticamente), pero es buena práctica
ponerlos explícitamente para que sean estables al agregar nuevos valores.

En la base de datos se guardará como `INT`: `Pendiente = 0`, `Pagada = 1`, etc.
En el código siempre usas el nombre, nunca el número.

### `RolUsuario.cs` — análisis

```csharp
public enum RolUsuario
{
    Cliente = 0,
    Admin = 1
}
```

Solo dos valores. Este enum se usa en dos lugares:
1. En la entidad `Usuario.Rol` para saber qué tipo de usuario es
2. En el JWT (Día 2) para los claims de autorización `[Authorize(Roles = "Admin")]`

### Cómo se usa un enum

```csharp
// Asignar
usuario.Rol = RolUsuario.Admin;

// Comparar
if (usuario.Rol == RolUsuario.Admin) { ... }

// Convertir a string (para respuestas de la API)
string texto = orden.Estado.ToString();  // retorna "Pendiente", "Pagada", etc.

// Convertir de string a enum
EstadoOrden estado = Enum.Parse<EstadoOrden>("Pagada");
```

---

## 8. Entidades del dominio — análisis línea a línea

Las **entidades** son las clases que representan los objetos del negocio.
Son el corazón del proyecto — definen qué existe en la tienda.

---

### `Categoria.cs`

```csharp
namespace EcommerceNet.Core.Entidades;
```
Declara el namespace. No necesita `using` porque no usa clases de otros namespaces.

```csharp
public class Categoria
{
    public int Id { get; set; }
```
`Id` es el identificador único en la base de datos. Por convención en EF Core,
una propiedad llamada `Id` (o `ClaseId`) se convierte automáticamente en PRIMARY KEY.

```csharp
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
```
Dos campos de texto. El `= string.Empty` evita que sean `null` al crear un objeto.

```csharp
    public bool Activa { get; set; } = true;
```
Las categorías nacen activas. Si se desactiva una categoría, sus productos no aparecerán
en el listado público (esto se filtra en las consultas del repositorio).

```csharp
    // Relación: una categoría tiene muchos productos
    public List<Producto> Productos { get; set; } = new();
```
Esta es la **propiedad de navegación** de la relación 1:N.
- `List<Producto>` — una lista de objetos de tipo `Producto`
- `= new()` — equivale a `= new List<Producto>()` — empieza como lista vacía

`new()` sin el tipo es **target-typed new** de C# 9: el compilador infiere el tipo
(`List<Producto>`) del lado izquierdo. Es azúcar sintáctica (syntactic sugar).

EF Core usa esta propiedad para saber que `Categoria` tiene muchos `Producto`.
Cuando haces `.Include(c => c.Productos)` en una consulta, EF Core llena esta lista.

```csharp
    public int TotalProductosActivos()
    {
        return Productos.Count(p => p.Activo);
    }
```
- **`int`** — el método retorna un número entero
- **`TotalProductosActivos()`** — sin parámetros (los paréntesis vacíos lo indican)
- **`Productos.Count(p => p.Activo)`** — LINQ: cuenta los productos donde `Activo == true`
  (ver sección 14 para LINQ y sección 15 para las lambdas `p => p.Activo`)

---

### `Producto.cs`

```csharp
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
```
Ocho propiedades de datos. `FechaCreacion = DateTime.UtcNow` significa que al crear
un `new Producto()`, la fecha ya queda registrada automáticamente.

```csharp
    // Relación con categoría
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
```
Dos formas de representar la misma relación:
- `CategoriaId` — la **clave foránea** (FK): el número que se guarda en la columna de BD
- `Categoria?` — la **propiedad de navegación**: el objeto completo cuando está cargado

El `?` después de `Categoria` significa que puede ser `null` (nullable). No siempre
se carga el objeto de categoría — depende de si hiciste `.Include()` en la consulta.

**Regla:** si una propiedad de navegación puede no estar cargada, siempre ponle `?`.

---

#### El bloque de lógica de negocio de `Producto`

```csharp
    public bool TieneStockSuficiente(int cantidad)
    {
        return Activo && Stock >= cantidad;
    }
```

**Análisis del `return Activo && Stock >= cantidad`:**

- `Activo` — es un `bool`, así que ya es `true` o `false` directamente
- `&&` — operador lógico AND: ambas condiciones deben ser `true`
- `Stock >= cantidad` — operador de comparación "mayor o igual que"
- El método retorna `true` solo si el producto está activo Y tiene suficiente stock

Tabla de verdad del `&&`:

| Activo | Stock >= cantidad | Resultado |
|--------|-----------------|-----------|
| true | true | **true** |
| true | false | false |
| false | true | false |
| false | false | false |

```csharp
    public void ReducirStock(int cantidad)
    {
        if (!TieneStockSuficiente(cantidad))
            throw new InvalidOperationException(
                $"Stock insuficiente para '{Nombre}'. Disponible: {Stock}, solicitado: {cantidad}");
        Stock -= cantidad;
    }
```

- **`if (!TieneStockSuficiente(cantidad))`** — el `!` niega el resultado.
  Si `TieneStockSuficiente` devuelve `false`, `!false` es `true`, y entra al `if`.
- **`throw new InvalidOperationException(...)`** — lanza una excepción (ver sección 16).
  Cuando se lanza una excepción, el método termina inmediatamente.
- **`$"Stock insuficiente para '{Nombre}'. Disponible: {Stock}"`** — **string interpolado**:
  el `$` al inicio indica que las expresiones entre `{ }` se evalúan e insertan en el texto.
  `{Nombre}` se reemplaza con el valor actual de `Nombre`. Es equivalente a:
  ```csharp
  "Stock insuficiente para '" + Nombre + "'. Disponible: " + Stock + ...
  ```
- **`Stock -= cantidad`** — equivale a `Stock = Stock - cantidad`. El operador `-=`
  resta y asigna en una sola operación.

```csharp
    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero");
        Stock += cantidad;
    }
```

- **`cantidad <= 0`** — "menor o igual a cero": atrapa tanto negativos como cero.
- **`ArgumentException`** — excepción específica para argumentos inválidos.
  Se diferencia de `InvalidOperationException` en semántica:
  - `ArgumentException` → el argumento que te pasaron está mal
  - `InvalidOperationException` → la operación no es válida en el estado actual
- **`Stock += cantidad`** — equivale a `Stock = Stock + cantidad`.

---

### `Usuario.cs`

```csharp
using EcommerceNet.Core.Enums;   // necesario para usar RolUsuario

namespace EcommerceNet.Core.Entidades;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
```
El campo se llama `PasswordHash`, no `Password`. Esto es un **contrato implícito**:
nunca se almacena la contraseña en texto plano. Siempre un hash (BCrypt, Argon2, etc.)
En el Día 2 implementaremos el hashing real.

```csharp
    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
```
El tipo de esta propiedad es el enum `RolUsuario`. Por defecto, todo usuario nuevo
es `Cliente`. Un administrador tendría que cambiar explícitamente este valor.

```csharp
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Carrito? Carrito { get; set; }          // 1:1 — cada usuario tiene un carrito
    public List<Orden> Ordenes { get; set; } = new(); // 1:N — un usuario tiene muchas órdenes
```

```csharp
    public bool EsAdmin() => Rol == RolUsuario.Admin;
}
```

Este método usa la sintaxis **expression body** (`=>`). Es equivalente a:
```csharp
public bool EsAdmin()
{
    return Rol == RolUsuario.Admin;
}
```

Cuando el cuerpo del método es una sola expresión, puedes usar `=>` para acortarlo.
El `==` compara el valor de `Rol` con `RolUsuario.Admin`.

---

### `CarritoItem.cs`

```csharp
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
```

Tiene dos pares de FK + navegación:
- `CarritoId` / `Carrito?` — a qué carrito pertenece este item
- `ProductoId` / `Producto?` — qué producto es este item

Nota que `PrecioUnitario` se guarda aquí, en el item. Si el precio del producto cambia
mañana, el carrito sigue mostrando el precio de cuando se agregó.

```csharp
    public decimal CalcularSubtotal() => PrecioUnitario * Cantidad;
}
```

Expression body de una línea. El operador `*` es multiplicación.
`100m * 3 = 300m`.

---

### `Carrito.cs` — la entidad más compleja

```csharp
public class Carrito
{
    public int Id { get; set; }
    public DateTime UltimaModificacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public List<CarritoItem> Items { get; set; } = new();
```

`Items` es la lista de productos en el carrito. Empieza vacía (`= new()`).

#### Los métodos de cálculo (expression body de una línea)

```csharp
    public decimal CalcularTotal() => Items.Sum(i => i.CalcularSubtotal());
    public int TotalProductos() => Items.Sum(i => i.Cantidad);
    public bool EstaVacio() => Items.Count == 0;
```

- `Items.Sum(i => i.CalcularSubtotal())` — LINQ: suma el subtotal de cada item
- `Items.Sum(i => i.Cantidad)` — LINQ: suma las cantidades de todos los items
- `Items.Count == 0` — `.Count` es la cantidad de elementos en la lista; si es 0, está vacío

#### `AgregarProducto` — el método más interesante

```csharp
    public void AgregarProducto(Producto producto, int cantidad = 1)
```

El `= 1` en `int cantidad = 1` es un **parámetro opcional con valor por defecto**.
Si llamas `carrito.AgregarProducto(producto)` sin el segundo argumento, `cantidad` será 1.
Si llamas `carrito.AgregarProducto(producto, 3)`, `cantidad` será 3.

```csharp
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero");

        if (!producto.TieneStockSuficiente(cantidad))
            throw new InvalidOperationException(
                $"Stock insuficiente para '{producto.Nombre}'");
```

Dos validaciones rápidas. Si alguna falla, el método termina con una excepción.
Nota que el `if` sin llaves `{ }` solo aplica a la siguiente línea (`throw`).
Es válido pero solo cuando el if tiene una sola instrucción.

```csharp
        var existente = Items.FirstOrDefault(i => i.ProductoId == producto.Id);
```

- **`var`** — el compilador infiere el tipo. Aquí `existente` será `CarritoItem?`
  (puede ser null si no encuentra el item).
- **`Items.FirstOrDefault(...)`** — LINQ: busca el primer elemento que cumpla la condición.
  Retorna `null` si no encuentra ninguno.
- **`i => i.ProductoId == producto.Id`** — lambda: para cada item `i`, verifica si
  su `ProductoId` es igual al `Id` del producto que estamos agregando.

```csharp
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
```

- **`!= null`** — "distinto de null": el item ya existe en el carrito
- **`existente.Cantidad += cantidad`** — incrementa la cantidad del item existente
- **`else`** — si no existe, crea un nuevo `CarritoItem` con el inicializador de objetos

```csharp
        UltimaModificacion = DateTime.UtcNow;
    }
```

Actualiza la marca de tiempo al final de cualquier operación que modifica el carrito.

#### `ActualizarCantidad` — el operador `??`

```csharp
    public void ActualizarCantidad(int productoId, int nuevaCantidad)
    {
        var item = Items.FirstOrDefault(i => i.ProductoId == productoId)
            ?? throw new InvalidOperationException("Producto no encontrado en el carrito");
```

El operador **`??`** es el **null-coalescing operator** (operador de fusión de nulos).
Significa: "si lo de la izquierda es null, usa lo de la derecha".

```csharp
var item = A ?? B;
// Si A no es null: item = A
// Si A es null:    item = B
```

En este caso, si `FirstOrDefault` retorna `null`, lanza la excepción inmediatamente.
Esta es una forma compacta de escribir:

```csharp
var item = Items.FirstOrDefault(i => i.ProductoId == productoId);
if (item == null)
    throw new InvalidOperationException("Producto no encontrado en el carrito");
```

```csharp
        if (nuevaCantidad <= 0)
            Items.Remove(item);
        else
            item.Cantidad = nuevaCantidad;

        UltimaModificacion = DateTime.UtcNow;
    }
```

`Items.Remove(item)` elimina el objeto `item` de la lista. La lista sabe cuál es
porque compara referencias de objeto (son el mismo objeto en memoria).

---

### `Orden.cs`

```csharp
using EcommerceNet.Core.Enums;

namespace EcommerceNet.Core.Entidades;

public class Orden
{
    public int Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;
```

`Estado` es del tipo enum `EstadoOrden`. El valor inicial es `EstadoOrden.Pendiente`.

#### `GenerarNumeroOrden` — formato de fecha y relleno

```csharp
    public void GenerarNumeroOrden()
    {
        NumeroOrden = $"ORD-{FechaCreacion:yyyyMMdd}-{Id:D4}";
    }
```

La interpolación de strings admite **formatos**:
- `{FechaCreacion:yyyyMMdd}` — formatea la fecha como año-mes-día sin separadores: `20260331`
- `{Id:D4}` — formatea el número con mínimo 4 dígitos, rellenando con ceros: `Id = 7` → `"0007"`

Resultado final: `"ORD-20260331-0007"`

**¿Por qué se llama DESPUÉS del primer SaveChanges?**
El `Id` lo asigna la base de datos al insertar. Antes de guardar, `Id` es `0`.
Si llamaras antes, el número quedaría `"ORD-20260331-0000"` para todos.

#### `SePuedeCancelar` — el operador `||`

```csharp
    public bool SePuedeCancelar()
    {
        return Estado == EstadoOrden.Pendiente || Estado == EstadoOrden.Pagada;
    }
```

**`||`** es el operador lógico OR: retorna `true` si **al menos una** condición es `true`.

| Estado | Es Pendiente | Es Pagada | Resultado |
|--------|------------|----------|-----------|
| Pendiente | true | false | **true** |
| Pagada | false | true | **true** |
| Enviada | false | false | false |
| Entregada | false | false | false |

#### `Cancelar` — el bucle `foreach`

```csharp
    public void Cancelar()
    {
        if (!SePuedeCancelar())
            throw new InvalidOperationException(
                $"No se puede cancelar una orden en estado '{Estado}'");

        Estado = EstadoOrden.Cancelada;
        foreach (var detalle in Detalles)
            detalle.Producto?.AumentarStock(detalle.Cantidad);
    }
```

**`foreach`** itera sobre cada elemento de una colección:
```csharp
foreach (var detalle in Detalles)
//         ↑ variable temporal    ↑ la colección
```

En cada iteración, `detalle` es el siguiente `OrdenDetalle` de la lista.

**`detalle.Producto?.AumentarStock(detalle.Cantidad)`**

El operador **`?.`** es el **null-conditional operator** (operador condicional nulo).
- Si `detalle.Producto` es `null` → no hace nada (no lanza NullReferenceException)
- Si `detalle.Producto` no es `null` → llama a `AumentarStock(detalle.Cantidad)`

Sin el `?.` habría que escribir:
```csharp
if (detalle.Producto != null)
    detalle.Producto.AumentarStock(detalle.Cantidad);
```

---

### `OrdenDetalle.cs`

```csharp
public class OrdenDetalle
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }       // ← se persiste en BD

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

`Subtotal` es una propiedad que se guarda en la base de datos (no es calculada en tiempo
real). Se calcula una sola vez al crear el detalle con `CalcularSubtotal()` y luego se
persiste. Esto facilita reportes históricos: aunque el precio cambie, el subtotal es inmutable.

---

## 9. Interfaces — contratos sin implementación

### Qué es una interfaz

Una **interfaz** es un contrato: define QUÉ métodos y propiedades debe tener quien la
implemente, pero NO CÓMO los implementa.

Analogía: una interfaz es como un formulario en blanco. Dice "debes rellenar estos campos",
pero no dice cómo los consigues.

```csharp
public interface IProductoRepositorio   // el contrato
{
    Task<IEnumerable<Producto>> ObtenerActivosAsync();  // "debes tener este método"
    // no hay código, solo la firma
}

public class ProductoRepositorio : IProductoRepositorio  // quien firma el contrato
{
    public async Task<IEnumerable<Producto>> ObtenerActivosAsync()
    {
        // aquí sí hay código (Día 2 con EF Core)
        return await _context.Productos.Where(p => p.Activo).ToListAsync();
    }
}
```

### ¿Por qué usar interfaces en lugar de clases directamente?

**Sin interfaces (acoplamiento fuerte):**
```csharp
public class CarritoServicio
{
    private ProductoRepositorio _repo = new ProductoRepositorio();
    // CarritoServicio DEPENDE de la implementación concreta
    // No puedes probar sin BD, no puedes cambiar la implementación fácilmente
}
```

**Con interfaces (acoplamiento débil):**
```csharp
public class CarritoServicio
{
    private readonly IUnidadDeTrabajo _uow;
    // CarritoServicio DEPENDE de la interfaz, no de la implementación
    // Puedes pasar un mock en las pruebas, o cambiar de SQL Server a MongoDB
    // sin tocar CarritoServicio
}
```

### La convención `I` al inicio

Por convención en C# y .NET, todas las interfaces empiezan con la letra `I`:
`IRepositorio`, `ICarritoServicio`, `IDisposable`. Cuando ves la `I`, sabes que
es una interfaz, no una clase.

---

### `IRepositorio.cs` — la interfaz genérica

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

Hay dos cosas nuevas importantes aquí: **genéricos** y **`where T : class`**.
Ver sección 10 para el detalle completo.

### `IProductoRepositorio.cs` — herencia de interfaz

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

**`: IRepositorio<Producto>`** — herencia de interfaz.
`IProductoRepositorio` extiende (hereda) `IRepositorio<Producto>`.
Cualquier clase que implemente `IProductoRepositorio` debe implementar:
- Los 5 métodos de `IRepositorio<Producto>` (CRUD básico)
- Los 4 métodos especializados de `IProductoRepositorio`

En total: **9 métodos obligatorios**.

`int minimo = 5` — parámetro opcional con default en una firma de interfaz.
La implementación puede o no respetar el default, pero la firma es la misma.

### `ICarritoRepositorio.cs` — interfaz sin herencia

```csharp
public interface ICarritoRepositorio
{
    Task<Carrito?> ObtenerPorUsuarioAsync(int usuarioId);
    Task AgregarAsync(Carrito carrito);
    void Actualizar(Carrito carrito);
}
```

No hereda de `IRepositorio<T>` porque el carrito no se consulta por `Id` sino por
`UsuarioId`. Exponer `ObtenerPorIdAsync(int id)` sería una API engañosa desde el dominio.

### `IUnidadDeTrabajo.cs` — composición e `IDisposable`

```csharp
public interface IUnidadDeTrabajo : IDisposable
{
    IProductoRepositorio Productos { get; }
    ICarritoRepositorio Carritos { get; }
    IOrdenRepositorio Ordenes { get; }
    Task<int> GuardarCambiosAsync();
}
```

**`: IDisposable`** — herencia de la interfaz del sistema `IDisposable`.
`IDisposable` tiene un solo método: `void Dispose()`. Obliga a la implementación
a liberar recursos (como la conexión a la base de datos) cuando ya no se necesitan.
El framework llama a `Dispose()` automáticamente al final de cada request HTTP.

Las tres propiedades `{ get; }` son de solo lectura: la unidad de trabajo te da
acceso a los repositorios, pero no puedes reemplazarlos desde fuera.

`Task<int> GuardarCambiosAsync()` retorna el número de filas afectadas en la BD.
Es el único punto donde se persisten todos los cambios pendientes.

---

## 10. Genéricos — código que funciona para cualquier tipo

### El problema

Sin genéricos, necesitarías una interfaz de repositorio para cada entidad:

```csharp
interface IRepositorioProducto { Task<Producto?> ObtenerPorIdAsync(int id); }
interface IRepositorioCarrito  { Task<Carrito?>  ObtenerPorIdAsync(int id); }
interface IRepositorioOrden    { Task<Orden?>    ObtenerPorIdAsync(int id); }
// duplicación masiva...
```

### La solución: genéricos con `<T>`

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

`T` es un **parámetro de tipo** — un marcador de posición para cualquier tipo.
Cuando usas la interfaz, reemplazas `T` con el tipo concreto:

```csharp
IRepositorio<Producto>   // T = Producto
IRepositorio<Orden>      // T = Orden
IRepositorio<Usuario>    // T = Usuario
```

El compilador genera una versión especializada para cada tipo.

### `where T : class` — la restricción de tipo

```csharp
public interface IRepositorio<T> where T : class
```

**`where T : class`** es una **restricción genérica**: dice que `T` debe ser
una clase (un tipo por referencia), no un tipo primitivo como `int` o `bool`.

Sin esta restricción, alguien podría escribir `IRepositorio<int>`, que no tiene sentido.
Con la restricción, el compilador rechaza esos usos incorrectos.

### `IEnumerable<T>` — la interfaz de colección

```csharp
Task<IEnumerable<T>> ObtenerTodosAsync();
```

`IEnumerable<T>` es una interfaz del sistema que representa **cualquier colección iterable**.
Puedes usarla con `foreach`. `List<T>` implementa `IEnumerable<T>`, al igual que
arrays y otras colecciones. Retornar `IEnumerable<T>` en lugar de `List<T>` es más
flexible: la implementación puede decidir cómo materializar la colección.

### `Resultado<T>` — genérico propio

```csharp
public class Resultado<T>
{
    public bool Exito { get; set; }
    public T? Datos { get; set; }       // T puede ser CarritoDto, OrdenDto, lo que sea
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

`Resultado<T>` es una clase genérica. Al usarla:
```csharp
Resultado<CarritoDto>   // T = CarritoDto, los Datos son de tipo CarritoDto
Resultado<OrdenDto>     // T = OrdenDto, los Datos son de tipo OrdenDto
```

El campo `T? Datos` tiene el `?` porque puede ser `null` (cuando hay error, no hay datos).

---

## 11. DTOs — objetos de transferencia de datos

### Qué son y por qué existen

DTO = Data Transfer Object. Son clases cuyo único propósito es transportar datos
entre capas del sistema. No tienen lógica de negocio — solo propiedades.

**Las entidades NO se exponen directamente en la API** por tres razones:

1. **Seguridad**: `Usuario` tiene `PasswordHash`. Si serializaras la entidad directamente,
   ese campo aparecería en la respuesta JSON.
2. **Referencias circulares**: `Producto` → `Categoria` → `List<Producto>` → `Categoria` → ...
   El serializador JSON entraría en un bucle infinito.
3. **Forma del contrato**: El cliente puede necesitar campos calculados (`Disponible`,
   `Subtotal`) o nombres diferentes (`CategoriaNombre` en lugar de `CategoriaId`).

### `ProductoDto.cs` — dos clases en un archivo

```csharp
namespace EcommerceNet.Core.DTOs;

public class ProductoDto         // para LEER productos
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;  // string, no int
    public bool Disponible { get; set; }
}

public class CrearProductoDto    // para CREAR productos
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public int CategoriaId { get; set; }  // int FK, porque al crear necesitas el ID
}
```

En C# se pueden tener múltiples clases en un solo archivo. La convención es una clase
por archivo, pero DTOs relacionados se agrupan juntos.

Diferencias entre `ProductoDto` y la entidad `Producto`:
- `CategoriaNombre` (string) en lugar de `Categoria?` (objeto) — el cliente ve el nombre
- `Disponible` (bool calculado) en lugar de `Activo` (bool crudo) — mejor semántica
- No tiene `FechaCreacion` — el cliente no necesita ese dato en el listado

### `CarritoDto.cs` — tres clases relacionadas

```csharp
public class CarritoDto
{
    public int Id { get; set; }
    public List<CarritoItemDto> Items { get; set; } = new();  // lista de DTOs, no entidades
    public decimal Total { get; set; }
    public int TotalProductos { get; set; }
}

public class CarritoItemDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;  // nombre, no el objeto
    public string ImagenUrl { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }   // calculado, listo para el frontend
}

public class AgregarAlCarritoDto    // lo que el cliente ENVÍA al servidor
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; } = 1;   // default 1
}
```

### `Resultado.cs` — los métodos estáticos de fábrica

Los tres métodos de `Resultado<T>` son **factory methods** (métodos de fábrica):
crean y configuran un objeto en una sola llamada.

```csharp
// En lugar de:
var resultado = new Resultado<CarritoDto>();
resultado.Exito = true;
resultado.Datos = miCarrito;
resultado.Mensaje = "Producto agregado";

// Escribimos:
return Resultado<CarritoDto>.Ok(miCarrito, "Producto agregado");
```

La sintaxis `=> new() { ... }` es expression body + object initializer:

```csharp
public static Resultado<T> Ok(T datos, string? mensaje = null)
    => new() { Exito = true, Datos = datos, Mensaje = mensaje };
//  ↑ new Resultado<T>() con las propiedades inicializadas
```

`new()` sin argumentos usa el **constructor sin parámetros** (todos los DTOs y entidades
tienen uno generado automáticamente por el compilador cuando no defines ninguno).

---

## 12. Async/Await — programación asíncrona

### El problema: operaciones lentas bloquean el servidor

Cuando tu servidor hace una consulta a la base de datos, tarda millisegundos.
Sin async, el thread (hilo de ejecución) se queda esperando, bloqueado, sin poder
atender otras peticiones.

```
Thread 1: [procesando request 1] [ESPERANDO BD...........] [respondiendo]
Thread 2: [procesando request 2] [ESPERANDO BD...........] [respondiendo]
Thread 3: libre pero no hace nada
```

Con async, el thread queda libre mientras espera:

```
Thread 1: [procesando request 1] [--libre--] [respondiendo cuando BD responde]
Thread 1: [procesando request 3 mientras espera!]
```

Un solo thread puede manejar miles de requests concurrentes.

### La sintaxis

```csharp
// Firma del método async:
public async Task<Resultado<CarritoDto>> ObtenerCarritoAsync(int usuarioId)
//             ↑ async  ↑ Task<T> en lugar del tipo directo

// Llamar a otro método async:
var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
//            ↑ await suspende este método hasta que el otro termine

// Guardar en BD:
await _uow.GuardarCambiosAsync();
```

### `Task<T>` — el envoltorio del resultado asíncrono

`Task<T>` es una promesa: "eventualmente habrá un valor de tipo `T`".
- `Task<Resultado<CarritoDto>>` — promete que habrá un `Resultado<CarritoDto>`
- `Task` sin tipo — promete que la operación terminará (como `void` asíncrono)
- `Task<T?>` — puede ser `T` o `null`

### Regla de oro

Si un método llama a otro que retorna `Task`, ese método DEBE ser `async` y usar `await`.
Es un "virus" que sube por toda la cadena de llamadas hasta el controlador.

```
Controlador (async) → llama con await a →
CarritoServicio (async) → llama con await a →
IUnidadDeTrabajo.GuardarCambiosAsync() (async)
```

---

## 13. Servicios y la inyección de dependencias

### `ICarritoServicio.cs` — el contrato del servicio

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

Todos los métodos son `async` (retornan `Task<...>`). Los controladores del Día 2
dependerán de esta interfaz para llamar al servicio.

### `CarritoServicio.cs` — la implementación

#### La declaración de clase y herencia

```csharp
public class CarritoServicio : ICarritoServicio
```

El `:` indica que `CarritoServicio` **implementa** la interfaz `ICarritoServicio`.
El compilador verifica que todos los métodos de la interfaz estén implementados.
Si falta uno, error de compilación.

#### El constructor y la inyección de dependencias

```csharp
public class CarritoServicio : ICarritoServicio
{
    private readonly IUnidadDeTrabajo _uow;

    public CarritoServicio(IUnidadDeTrabajo uow)  // ← constructor
    {
        _uow = uow;
    }
```

El **constructor** es un método especial con el mismo nombre de la clase, sin tipo
de retorno. Se ejecuta automáticamente al hacer `new CarritoServicio(...)`.

**Inyección de dependencias:** en lugar de que `CarritoServicio` cree su propia
instancia de `IUnidadDeTrabajo`, la recibe desde afuera:

```csharp
// SIN inyección (malo):
public CarritoServicio()
{
    _uow = new UnidadDeTrabajo(new AppDbContext(...)); // acoplamiento fuerte
}

// CON inyección (bueno):
public CarritoServicio(IUnidadDeTrabajo uow)
{
    _uow = uow;  // quien crea el servicio decide qué implementación usar
}
```

En el Día 2, el contenedor de IoC (Inversion of Control) de ASP.NET Core creará
automáticamente las instancias y las inyectará:
```csharp
services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
services.AddScoped<ICarritoServicio, CarritoServicio>();
```

La convención de nombrado:
- `_uow` — el underscore `_` indica campo privado de instancia
- `uow` — abreviatura de Unit of Work

#### `ObtenerCarritoAsync` — el método más simple

```csharp
public async Task<Resultado<CarritoDto>> ObtenerCarritoAsync(int usuarioId)
{
    var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
    if (carrito == null) return Resultado<CarritoDto>.Ok(new CarritoDto());
    return Resultado<CarritoDto>.Ok(MapearCarrito(carrito));
}
```

- `await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId)` — busca el carrito en BD
- Si no existe (`null`), retorna un `CarritoDto` vacío con éxito — no es un error que
  no haya carrito todavía
- Si existe, lo convierte a DTO con `MapearCarrito(carrito)` (ver más abajo)
- En ambos casos retorna `Resultado<CarritoDto>.Ok(...)` — la operación fue exitosa

#### `AgregarProductoAsync` — múltiples validaciones

```csharp
public async Task<Resultado<CarritoDto>> AgregarProductoAsync(
    int usuarioId, AgregarAlCarritoDto dto)
{
    var producto = await _uow.Productos.ObtenerPorIdAsync(dto.ProductoId);
    if (producto == null)
        return Resultado<CarritoDto>.Error("Producto no encontrado");
    if (!producto.TieneStockSuficiente(dto.Cantidad))
        return Resultado<CarritoDto>.Error($"Stock insuficiente. Disponible: {producto.Stock}");
```

El patrón de **retorno temprano** (early return): si algo está mal, retornamos
inmediatamente con error. Esto evita el "triángulo de la muerte" de ifs anidados:

```csharp
// Mal (anidado):
if (producto != null) {
    if (producto.TieneStockSuficiente(dto.Cantidad)) {
        // código principal enterrado muy adentro
    }
}

// Bien (retorno temprano):
if (producto == null) return Error(...);
if (!TieneStockSuficiente) return Error(...);
// código principal en el primer nivel de indentación
```

```csharp
    var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
    if (carrito == null)
    {
        carrito = new Carrito { UsuarioId = usuarioId };
        await _uow.Carritos.AgregarAsync(carrito);
    }
```

**Lazy initialization**: el carrito se crea solo cuando se necesita por primera vez.
Un usuario recién registrado no tiene carrito — se crea en su primer `AgregarProducto`.

#### El método `CheckoutAsync` — la operación más compleja

```csharp
public async Task<Resultado<OrdenDto>> CheckoutAsync(int usuarioId, CrearOrdenDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.DireccionEnvio))
        return Resultado<OrdenDto>.Error("La dirección de envío es obligatoria");
```

**`string.IsNullOrWhiteSpace(texto)`** — método estático del sistema que retorna `true` si:
- `texto` es `null`, O
- `texto` es `""` (vacío), O
- `texto` es `"   "` (solo espacios en blanco)

Es más robusto que solo verificar `== null` o `== ""`.

```csharp
    var errores = new List<string>();
    foreach (var item in carrito.Items)
    {
        var prod = await _uow.Productos.ObtenerPorIdAsync(item.ProductoId);
        if (prod == null || !prod.TieneStockSuficiente(item.Cantidad))
            errores.Add($"'{item.Producto?.Nombre}': stock insuficiente");
    }
    if (errores.Count > 0)
        return Resultado<OrdenDto>.ErrorValidacion(errores);
```

- `new List<string>()` — lista vacía de strings para acumular errores
- `errores.Add(...)` — agrega un mensaje de error a la lista
- Si hay errores, los retorna todos juntos (no uno por uno)
- `item.Producto?.Nombre` — el `?.` porque `Producto` puede ser null (eager loading)

```csharp
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
```

**`continue`** — salta al siguiente elemento del `foreach` sin ejecutar el resto del
cuerpo. Es como un "skip" para este item.

```csharp
        var detalle = new OrdenDetalle
        {
            ProductoId = item.ProductoId,
            Producto = prod,
            Cantidad = item.Cantidad,
            PrecioUnitario = item.PrecioUnitario  // precio snapshot del momento
        };
        detalle.CalcularSubtotal();
        orden.Detalles.Add(detalle);

        prod.ReducirStock(item.Cantidad);  // lógica de dominio en la entidad
        _uow.Productos.Actualizar(prod);
    }

    orden.RecalcularTotal();
    await _uow.Ordenes.AgregarAsync(orden);
    carrito.Vaciar();
    _uow.Carritos.Actualizar(carrito);

    await _uow.GuardarCambiosAsync();  // ← UN SOLO SaveChanges para todo

    orden.GenerarNumeroOrden();
    _uow.Ordenes.Actualizar(orden);
    await _uow.GuardarCambiosAsync();  // ← segundo save solo para el número
```

#### Los métodos de mapeo privados

```csharp
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
```

- **`private static`** — privado (solo para esta clase) y estático (no necesita instancia)
- **`c.Items.Select(...)`** — LINQ: transforma cada `CarritoItem` en `CarritoItemDto`
- **`?? ""`** — operador null-coalescing: si el nombre es null, usa string vacío
- **`.ToList()`** — materializa el resultado de `Select` en una `List<CarritoItemDto>`

---

## 14. LINQ — consultas sobre colecciones

LINQ (Language Integrated Query) permite consultar colecciones con una sintaxis
similar a SQL pero dentro de C#.

### Los métodos LINQ usados en el proyecto

#### `Count(condición)` — contar elementos que cumplen una condición

```csharp
// En Categoria.cs
return Productos.Count(p => p.Activo);
// Cuenta cuántos productos tienen Activo == true
```

#### `Sum(selector)` — sumar un campo de cada elemento

```csharp
// En Carrito.cs
Items.Sum(i => i.CalcularSubtotal())  // suma los subtotales
Items.Sum(i => i.Cantidad)            // suma las cantidades
Detalles.Sum(d => d.Subtotal)         // suma los subtotales de la orden
```

#### `FirstOrDefault(condición)` — el primero que cumple, o null

```csharp
// En Carrito.cs
var existente = Items.FirstOrDefault(i => i.ProductoId == producto.Id);
// Retorna el CarritoItem cuyo ProductoId coincide, o null si no hay ninguno
```

#### `Select(transformación)` — transformar cada elemento

```csharp
// En CarritoServicio.cs
c.Items.Select(i => new CarritoItemDto { ... })
// Transforma cada CarritoItem en un CarritoItemDto
```

#### `Where(condición)` — filtrar

```csharp
// Ejemplo del repositorio (Día 2):
context.Productos.Where(p => p.Activo)
// Retorna solo los productos activos
```

#### `.ToList()` — materializar en lista

`Select`, `Where`, etc. retornan `IEnumerable<T>` que es "lazy" (no se evalúa hasta
que se necesita). `.ToList()` lo evalúa inmediatamente y crea una `List<T>`.

---

## 15. Expresiones lambda

Una **lambda** es una función anónima (sin nombre). Se escribe con `=>` (se lee "va a" o "arrow"):

```csharp
i => i.Cantidad
// "dado un i, retorna i.Cantidad"
```

Es como un método inline:

```csharp
// Lambda equivalente a este método:
int ObtenerCantidad(CarritoItem i)
{
    return i.Cantidad;
}
```

### Anatomía de una lambda

```csharp
(CarritoItem i) => i.Cantidad
//     ↑              ↑
// parámetro       expresión de retorno
```

El tipo del parámetro se infiere del contexto. En `Items.Sum(i => i.Cantidad)`,
LINQ sabe que `i` es `CarritoItem` porque `Items` es `List<CarritoItem>`.

### Lambdas con cuerpo de bloque

Cuando la lambda tiene múltiples líneas:

```csharp
Items.Sum(i => {
    var precio = i.PrecioUnitario * 1.16m; // con IVA
    return precio * i.Cantidad;
})
```

### Lambdas en el proyecto

| Uso | Lambda | Lectura |
|-----|--------|---------|
| `Productos.Count(p => p.Activo)` | `p => p.Activo` | "para cada p, toma su Activo" |
| `Items.Sum(i => i.CalcularSubtotal())` | `i => i.CalcularSubtotal()` | "para cada i, llama CalcularSubtotal" |
| `Items.FirstOrDefault(i => i.ProductoId == producto.Id)` | `i => i.ProductoId == producto.Id` | "para cada i, verifica si su ProductoId es igual" |
| `c.Items.Select(i => new CarritoItemDto { ... })` | `i => new CarritoItemDto {...}` | "para cada i, crea un nuevo CarritoItemDto" |

---

## 16. Manejo de errores — excepciones

### Qué es una excepción

Una excepción es un error que ocurre en tiempo de ejecución. Cuando se lanza,
el flujo normal del programa se interrumpe y "sube" por la cadena de llamadas
hasta que alguien la atrapa con `try/catch`, o hasta el nivel más alto (que la reporta al usuario).

### `throw` — lanzar una excepción

```csharp
throw new InvalidOperationException("No se puede cancelar");
```

- `throw` — palabra reservada que lanza la excepción
- `new InvalidOperationException(...)` — crea una instancia de la excepción

Cuando se ejecuta `throw`, el método termina inmediatamente. Las líneas siguientes
no se ejecutan.

### Jerarquía de excepciones en el proyecto

```
Exception                          ← base de todas
├── ArgumentException              ← argumento inválido del llamador
│   └── ArgumentNullException      ← argumento null
└── InvalidOperationException      ← operación inválida en el estado actual
```

| Excepción | Cuándo usarla | Ejemplo en el proyecto |
|-----------|--------------|----------------------|
| `ArgumentException` | El valor del argumento es inválido | `AumentarStock(0)` |
| `InvalidOperationException` | La operación no es válida ahora | `ReducirStock` sin stock, `Cancelar` orden enviada |

### `try/catch` — atrapar excepciones

```csharp
// En CarritoServicio.cs
try
{
    carrito.ActualizarCantidad(productoId, cantidad);  // puede lanzar excepción
    _uow.Carritos.Actualizar(carrito);
    await _uow.GuardarCambiosAsync();
    return Resultado<CarritoDto>.Ok(MapearCarrito(carrito));
}
catch (InvalidOperationException ex)
{
    return Resultado<CarritoDto>.Error(ex.Message);   // convierte la excepción en error del resultado
}
```

- `try { }` — el bloque que puede fallar
- `catch (TipoDeExcepcion ex) { }` — qué hacer si falla
- `ex.Message` — el mensaje de texto de la excepción

El servicio **convierte** las excepciones de dominio en `Resultado.Error(...)`.
Así el controlador recibe siempre un `Resultado<T>` y nunca una excepción sin atrapar.

---

## 17. Operadores de C# usados en el proyecto

### Operadores aritméticos

| Operador | Significado | Ejemplo |
|----------|------------|---------|
| `+` | Suma | `Stock + cantidad` |
| `-` | Resta | `Stock - cantidad` |
| `*` | Multiplicación | `PrecioUnitario * Cantidad` |
| `/` | División | `total / 2` |
| `+=` | Suma y asigna | `Stock += 10` = `Stock = Stock + 10` |
| `-=` | Resta y asigna | `Stock -= 3` = `Stock = Stock - 3` |

### Operadores de comparación

| Operador | Significado | Ejemplo |
|----------|------------|---------|
| `==` | Igual a | `Estado == EstadoOrden.Pagada` |
| `!=` | Distinto de | `carrito != null` |
| `>` | Mayor que | `Stock > 0` |
| `>=` | Mayor o igual | `Stock >= cantidad` |
| `<` | Menor que | `cantidad < 0` |
| `<=` | Menor o igual | `cantidad <= 0` |

### Operadores lógicos

| Operador | Significado | Ejemplo |
|----------|------------|---------|
| `&&` | AND lógico (ambos true) | `Activo && Stock >= cantidad` |
| `\|\|` | OR lógico (al menos uno true) | `Estado == Pendiente \|\| Estado == Pagada` |
| `!` | NOT lógico (niega) | `!TieneStockSuficiente(cantidad)` |

### Operadores especiales de C#

| Operador | Nombre | Ejemplo | Significado |
|----------|--------|---------|-------------|
| `?.` | Null-conditional | `detalle.Producto?.AumentarStock(n)` | Solo llama si no es null |
| `??` | Null-coalescing | `nombre ?? ""` | Si null, usa el lado derecho |
| `??=` | Null-coalescing assign | `carrito ??= new Carrito()` | Asigna solo si es null |
| `=>` | Lambda / Expression body | `x => x.Activo` | Define función inline |
| `$""` | String interpolado | `$"Hola {nombre}"` | Inserta expresiones en texto |

### El operador `??` en detalle

```csharp
var item = Items.FirstOrDefault(i => i.ProductoId == id)
    ?? throw new InvalidOperationException("No encontrado");
```

Se lee: "busca el item, y si el resultado es null, lanza la excepción".

```csharp
string nombre = producto?.Nombre ?? "";
```

Se lee: "si producto no es null, toma su Nombre; si ese Nombre es null, usa string vacío".

Se pueden encadenar: `a?.b?.c ?? defaultValue`.

---

## 18. Pruebas unitarias con xUnit

### Qué son las pruebas unitarias

Una **prueba unitaria** es un método que verifica que otro método funciona correctamente.
"Unitaria" porque prueba la unidad más pequeña de código (un método) de forma aislada.

### El patrón Arrange-Act-Assert (AAA)

```csharp
[Fact]
public void ReducirStock_Suficiente_Reduce()
{
    // Arrange — preparar el escenario
    var p = new Producto { Stock = 10, Activo = true };

    // Act — ejecutar lo que se prueba
    p.ReducirStock(3);

    // Assert — verificar el resultado
    Assert.Equal(7, p.Stock);
}
```

- **Arrange**: crear objetos con los datos necesarios
- **Act**: llamar al método que se quiere probar
- **Assert**: verificar que el resultado es el esperado

### El atributo `[Fact]`

```csharp
[Fact]
public void NombreDeLaPrueba()
```

`[Fact]` es un **atributo** de xUnit. Le dice al runner de pruebas "este método es
una prueba, ejecútalo". Sin `[Fact]`, el runner no sabe que el método es una prueba.

Los atributos en C# van entre corchetes `[ ]` encima del elemento que anotan.

### Convención de nombres

```
Método_Escenario_Resultado
ReducirStock_Suficiente_Reduce
```

- `ReducirStock` — qué método se prueba
- `Suficiente` — en qué escenario
- `Reduce` — qué resultado esperamos

### Los métodos `Assert` de xUnit

| Método | Qué verifica | Ejemplo |
|--------|-------------|---------|
| `Assert.True(valor)` | `valor == true` | `Assert.True(p.TieneStockSuficiente(5))` |
| `Assert.False(valor)` | `valor == false` | `Assert.False(p.TieneStockSuficiente(20))` |
| `Assert.Equal(esperado, real)` | `esperado == real` | `Assert.Equal(7, p.Stock)` |
| `Assert.Null(valor)` | `valor == null` | `Assert.Null(resultado)` |
| `Assert.NotNull(valor)` | `valor != null` | `Assert.NotNull(carrito)` |
| `Assert.Single(colección)` | la colección tiene exactamente 1 elemento | `Assert.Single(c.Items)` |
| `Assert.Empty(colección)` | la colección está vacía | `Assert.Empty(c.Items)` |
| `Assert.Throws<T>(acción)` | la acción lanza la excepción T | ver abajo |

### `Assert.Throws` — verificar excepciones

```csharp
[Fact]
public void ReducirStock_Insuficiente_LanzaExcepcion()
{
    var p = new Producto { Stock = 2, Activo = true, Nombre = "Test" };

    Assert.Throws<InvalidOperationException>(() => p.ReducirStock(5));
}
```

`() => p.ReducirStock(5)` es una lambda sin parámetros (los paréntesis vacíos `()`).
`Assert.Throws<InvalidOperationException>` ejecuta la lambda y verifica que lanza
ese tipo específico de excepción. Si no lanza, o lanza otro tipo, la prueba falla.

### Métodos auxiliares en las pruebas — el helper `CrearProducto`

```csharp
public class CarritoTests
{
    private Producto CrearProducto(int id = 1, decimal precio = 100m, int stock = 10)
        => new() { Id = id, Nombre = $"Producto {id}", Precio = precio, Stock = stock, Activo = true };
```

`CrearProducto` es un método privado de la clase de pruebas que no es una prueba en sí.
Sirve para no repetir la creación del producto en cada prueba. Es un **helper method**.

```csharp
    [Fact]
    public void CalcularTotal_VariosItems_SumaCorrecto()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1, 100m), 2);  // producto id=1, precio=100, cantidad=2 → 200
        c.AgregarProducto(CrearProducto(2, 50m), 3);   // producto id=2, precio=50, cantidad=3 → 150
        Assert.Equal(350m, c.CalcularTotal());           // 200 + 150 = 350
    }
```

Los comentarios `// 200` y `// 150` documentan el razonamiento matemático de la prueba.

### `OrdenTests.cs` — prueba que verifica múltiples cosas

```csharp
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
    Assert.Equal(EstadoOrden.Cancelada, o.Estado);   // verifica el estado
    Assert.Equal(8, prod.Stock);                      // verifica el stock (5 + 3 = 8)
}
```

Una prueba puede tener múltiples `Assert`. Si el primero falla, los siguientes no se ejecutan.
Esta prueba verifica dos cosas: el estado cambió a Cancelada y el stock fue devuelto.

`new() { new() { Cantidad = 3, Producto = prod } }` — inicialización de lista con elementos:
- `new()` = `new List<OrdenDetalle>()`
- `new() { Cantidad = 3, Producto = prod }` = `new OrdenDetalle { Cantidad = 3, Producto = prod }`

---

## 19. Glosario rápido de palabras reservadas

Todas las palabras clave de C# usadas en este día, ordenadas alfabéticamente:

| Palabra | Qué hace |
|---------|---------|
| `async` | Marca un método como asíncrono |
| `await` | Espera el resultado de una operación async |
| `bool` | Tipo booleano: `true` o `false` |
| `catch` | Atrapa una excepción en un bloque try/catch |
| `class` | Define una clase |
| `continue` | Salta a la siguiente iteración de un bucle |
| `decimal` | Tipo numérico de alta precisión para decimales |
| `else` | Rama alternativa de un `if` |
| `enum` | Define un enumerador |
| `false` | Valor booleano falso |
| `foreach` | Itera sobre cada elemento de una colección |
| `if` | Estructura de control condicional |
| `int` | Tipo entero (sin decimales) |
| `interface` | Define una interfaz |
| `namespace` | Declara el espacio de nombres del archivo |
| `new` | Crea una nueva instancia de una clase |
| `null` | Ausencia de valor (referencia nula) |
| `private` | Solo accesible dentro de la misma clase |
| `public` | Accesible desde cualquier parte |
| `readonly` | Solo se puede asignar en el constructor |
| `return` | Devuelve un valor y termina el método |
| `static` | Pertenece a la clase, no a la instancia |
| `string` | Tipo de texto |
| `throw` | Lanza una excepción |
| `true` | Valor booleano verdadero |
| `try` | Bloque que puede lanzar excepciones |
| `using` | Importa un namespace |
| `var` | El compilador infiere el tipo automáticamente |
| `void` | El método no retorna ningún valor |
| `where` | Restricción en un genérico (`where T : class`) |

---

## Resumen del día

Después de leer esta clase, el desarrollador debería entender:

| Concepto | Dónde lo viste |
|----------|---------------|
| Namespaces y `using` | Todos los archivos |
| Clases y objetos | 9 entidades, 4 DTOs, 2 servicios |
| Tipos de datos y propiedades | Todas las clases |
| Modificadores `public`/`private`/`readonly`/`static` | Servicio, interfaces |
| Enums | `EstadoOrden`, `RolUsuario` |
| Lógica en entidades (DDD) | `Producto`, `Carrito`, `Orden` |
| Nullable (`?`) y null-safety (`?.`, `??`) | Propiedades de navegación, servicio |
| Interfaces y contratos | 5 interfaces en `Interfaces/` |
| Genéricos (`<T>`) | `IRepositorio<T>`, `Resultado<T>` |
| Herencia de interfaces (`:`) | `IProductoRepositorio : IRepositorio<Producto>` |
| DTOs vs Entidades | 4 archivos de DTOs |
| Async/Await y `Task<T>` | Todo el servicio |
| Inyección de dependencias | Constructor de `CarritoServicio` |
| LINQ (`Sum`, `Count`, `FirstOrDefault`, `Select`) | Entidades y servicio |
| Expresiones lambda (`=>`) | Métodos cortos, LINQ |
| Excepciones (`throw`, `try/catch`) | Entidades y servicio |
| Todos los operadores | Todo el código |
| Pruebas unitarias con xUnit | 3 archivos de pruebas |
