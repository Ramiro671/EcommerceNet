# Cuestionario de Entrevista Técnica — Senior Fullstack .NET & Vue.js
> 94 preguntas basadas en el código real de EcommerceNet. Cada respuesta es memorizable en 2 lecturas.
> Generado: 2026-04-09

---

## Sección 1 — C# y fundamentos (10 preguntas)

**P: ¿Qué es C# y en qué se diferencia .NET Framework de .NET 10?**
R: C# es el lenguaje de programación. .NET Framework era solo Windows. .NET Core/.NET 10 es multiplataforma, open source, más rápido y con Docker. Este proyecto usa .NET 10 (TargetFramework: `net10.0`, paquetes `10.0.5`).

**P: ¿Qué es un file-scoped namespace y por qué se usa en este proyecto?**
R: Elimina un nivel de indentación. En vez de `namespace X { class Y {} }` se escribe `namespace X;` al inicio del archivo. Todos los `.cs` del proyecto usan esta sintaxis: ver `Producto.cs`, `IRepositorio.cs`, etc.

**P: ¿Qué son las clases, objetos e instancias?**
R: Clase = molde (ej: `Producto`). Objeto = instancia en memoria (`new Producto()`). Una clase puede generar miles de instancias, cada una con sus propios valores de `Nombre`, `Precio`, `Stock`.

**P: ¿Por qué `decimal` para precios y no `double`?**
R: `double` tiene errores de punto flotante binario: `0.1 + 0.2 ≠ 0.3`. `decimal` es exacto para dinero. En `Producto.cs`: `public decimal Precio { get; set; }`. En `AppDbContext.cs`: `HasColumnType("decimal(18,2)")`.

**P: ¿Qué son `{ get; set; }`, `private set` e `init`?**
R: `{ get; set; }` = lectura y escritura pública. `private set` = solo la misma clase puede cambiar el valor. `init` = solo se puede asignar en el constructor o con `new X { Prop = val }`. En el proyecto solo se usa `{ get; set; }` con `= string.Empty` como valor por defecto.

**P: ¿Qué son los modificadores de acceso: public, private, protected, internal?**
R: `public` = todos pueden acceder. `private` = solo la clase misma (ej: `_uow` en `CarritoServicio`). `protected` = la clase y sus subclases (ej: `_contexto` en `RepositorioBase`). `internal` = solo dentro del mismo ensamblado.

**P: ¿Qué son los enums? Muestra un ejemplo real del proyecto.**
R: Son conjuntos de constantes con nombre entero. En `EstadoOrden.cs`: `Pendiente=0, Pagada=1, Enviada=3, Cancelada=5`. En `RolUsuario.cs`: `Cliente=0, Admin=1`. En `Orden.cs`: `public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;`.

**P: ¿Qué son los nullable types y los operadores `?.` y `??`?**
R: `Categoria?` = puede ser null. `?.` es null-conditional: `p.Categoria?.Nombre` no lanza NullRef si Categoria es null. `??` es null-coalescing: `p.Categoria?.Nombre ?? "Sin categoría"` — ver `MapearADto` en `ProductosController.cs`.

**P: ¿Qué son los genéricos? Muestra con código real del proyecto.**
R: Permiten escribir código que funciona con cualquier tipo. `IRepositorio<T>` funciona para Producto, Carrito, Orden, etc. sin repetir código. `Resultado<T>` envuelve cualquier tipo de dato en la respuesta: `Resultado<ProductoDto>`, `Resultado<CarritoDto>`, etc.

**P: ¿Qué son las expresiones lambda `=>`?**
R: Son funciones anónimas cortas. En `Carrito.cs`: `public decimal CalcularTotal() => Items.Sum(i => i.CalcularSubtotal());`. En LINQ: `.Where(p => p.Activo).OrderBy(p => p.Precio)`. En `UnidadDeTrabajo.cs`: `_productos ??= new ProductoRepositorio(_contexto)`.

---

## Sección 2 — Clean Architecture y patrones (10 preguntas)

**P: ¿Qué es Clean Architecture? ¿Cuáles son las 4 capas del proyecto?**
R: Arquitectura que separa responsabilidades en capas con dependencias unidireccionales hacia el centro. Las 4 capas: `Core` (entidades + interfaces), `Data` (EF Core + repositorios), `API` (controladores + middleware), `Web` (Vue.js). Core no depende de nada.

**P: ¿Por qué Core no depende de ningún paquete externo?**
R: Para que el dominio del negocio sea independiente de frameworks. Si mañana cambias EF Core por Dapper, Core no cambia. Las interfaces en `Core/Interfaces/` son contratos que `Data` implementa — Core nunca sabe que existe EF Core.

**P: ¿Qué es una interfaz y por qué se usa para inyección de dependencias?**
R: Contrato que define qué debe hacer una clase sin decir cómo. `IRepositorio<T>` define `ObtenerPorIdAsync`, `AgregarAsync`, etc. La DI registra `RepositorioBase` como implementación. Si cambias la implementación, el código que usa la interfaz no cambia.

**P: ¿Qué es el patrón Repository? Muestra la cadena real del proyecto.**
R: Abstrae el acceso a datos detrás de una interfaz. Cadena: `IRepositorio<T>` (Core) → `RepositorioBase<T>` (implementación genérica con EF) → `ProductoRepositorio` (override de `ObtenerPorIdAsync` con `Include(p => p.Categoria)`).

**P: ¿Qué es Unit of Work? ¿Por qué `SaveChanges` va ahí y no en los repositorios?**
R: Agrupa múltiples operaciones en una sola transacción. En `UnidadDeTrabajo.cs`, `GuardarCambiosAsync()` llama a `_contexto.SaveChangesAsync()` — si falla algo, nada se guarda. El repositorio solo marca entidades como `Added/Modified`; el UoW decide cuándo persistir.

**P: ¿Qué es DDD? Muestra con `Producto.ReducirStock()` y `Carrito.AgregarProducto()`.**
R: Domain-Driven Design: la lógica de negocio vive en las entidades, no en servicios ni controladores. `Producto.ReducirStock()` valida stock y lanza excepción. `Carrito.AgregarProducto()` verifica stock, incrementa si ya existe el item, actualiza `UltimaModificacion`. El servicio solo orquesta.

**P: ¿Qué es un DTO y por qué nunca se expone la entidad directamente en la API?**
R: Data Transfer Object = clase plana para transferir solo los datos necesarios. `ProductoDto` tiene `CategoriaNombre` (string) en vez de `Categoria` (objeto con todas sus relaciones). Evita serialización circular, exponer datos internos y acoplar el cliente al modelo de BD.

**P: ¿Qué principios SOLID se aplican en el proyecto?**
R: S: cada clase tiene una responsabilidad (`CarritoServicio` = lógica de carrito, `AuthServicio` = autenticación). O: `RepositorioBase` se extiende sin modificar. D: `CarritoServicio` depende de `IUnidadDeTrabajo`, no de `UnidadDeTrabajo` concreta.

**P: ¿Qué hace el operador `??=` en `UnidadDeTrabajo.cs`?**
R: Null-coalescing assignment: si el campo es null, asigna el nuevo valor; si ya tiene valor, lo devuelve. `public IProductoRepositorio Productos => _productos ??= new ProductoRepositorio(_contexto);` — los repositorios se crean solo cuando se necesitan (lazy initialization).

**P: ¿Qué son `virtual` y `override`? Muestra con `RepositorioBase` y `ProductoRepositorio`.**
R: `virtual` marca un método como sobreescribible. `override` reemplaza la implementación en una subclase. `RepositorioBase.ObtenerPorIdAsync` usa `FindAsync(id)`. `ProductoRepositorio` hace `override` para agregar `Include(p => p.Categoria)` y usar `FirstOrDefaultAsync`.

---

## Sección 3 — Async/Await y pruebas unitarias (8 preguntas)

**P: ¿Qué es async/await? ¿Qué pasa si usas `.Result` en ASP.NET Core?**
R: `async` marca que un método tiene operaciones asíncronas. `await` libera el hilo mientras espera I/O (BD, red). `.Result` bloquea el hilo sincrónicamente y puede causar deadlock en ASP.NET Core porque el contexto de sincronización espera el hilo que está bloqueado.

**P: ¿Qué es `Task<T>`? ¿Por qué los métodos devuelven `Task<Resultado<T>>`?**
R: `Task<T>` es la promesa de que habrá un valor T en el futuro. `Task<Resultado<T>>` combina asincronía con la envoltura estándar de respuesta. Por ejemplo, `ObtenerCarritoAsync` devuelve `Task<Resultado<CarritoDto>>` — puedes hacer `await` y siempre obtienes un `Resultado` con `Exito`, `Datos` y `Mensaje`.

**P: ¿Qué es LINQ? Nombra 5 operadores usados en el proyecto.**
R: Language Integrated Query — consultas sobre colecciones en C#. En el proyecto: `.Where(p => p.Activo)`, `.Include(p => p.Categoria)`, `.OrderBy(p => p.Precio)`, `.OrderByDescending(p => p.FechaCreacion)`, `.Select(MapearADto)`, `.Sum(i => i.CalcularSubtotal())`, `.FirstOrDefault(i => i.ProductoId == id)`.

**P: ¿Qué significa AAA en xUnit?**
R: Arrange-Act-Assert. Arrange: preparar el objeto (`var p = new Producto { Stock = 10, Activo = true }`). Act: ejecutar el método (`p.ReducirStock(3)`). Assert: verificar resultado (`Assert.Equal(7, p.Stock)`). Patrón usado en todos los tests de `ProductoTests.cs`, `CarritoTests.cs`, `OrdenTests.cs`.

**P: ¿Diferencia entre `[Fact]` y `[Theory]` con `[InlineData]`?**
R: `[Fact]` = test que siempre corre igual, sin parámetros. `[Theory]` + `[InlineData(valor)]` = mismo test con múltiples conjuntos de datos (evita duplicar código de test). El proyecto usa solo `[Fact]` — 7 en `ProductoTests`, 10 en `CarritoTests`, 5 en `OrdenTests` = 22 tests (más 1 en `UnitTest1`).

**P: ¿Qué son las excepciones? ¿`ArgumentException` vs `InvalidOperationException`?**
R: `ArgumentException` = el parámetro es inválido (`AumentarStock(0)` lanza "La cantidad debe ser mayor a cero"). `InvalidOperationException` = la operación no es válida en el estado actual (`ReducirStock(100)` cuando `Stock = 2`, o `Cancelar()` cuando la orden ya está `Enviada`).

**P: ¿Cuántas pruebas tiene el proyecto y qué cubren?**
R: 23 pruebas de 23 pasando. Cubren: `Producto` (stock suficiente, reducir, aumentar), `Carrito` (agregar nuevo/existente/sin stock, calcular total, actualizar, eliminar, vaciar), `Orden` (recalcular total, cancelar, devolver stock). Solo prueban lógica de dominio en `Core/` — sin BD, sin mocks.

**P: ¿Qué son `Assert.Equal`, `Assert.True`, `Assert.NotNull`, `Assert.Throws`?**
R: Métodos de xUnit para verificar resultados. `Assert.Equal(7, p.Stock)` — valores iguales. `Assert.True(p.TieneStockSuficiente(5))`. `Assert.Throws<InvalidOperationException>(() => p.ReducirStock(100))` — verifica que se lanza la excepción esperada. `Assert.Single(c.Items)` — la lista tiene exactamente 1 elemento.

---

## Sección 4 — ASP.NET MVC Legacy vs ASP.NET Core (8 preguntas)

**P: ¿5 diferencias principales entre ASP.NET MVC Framework y ASP.NET Core?**
R: 1) Core es multiplataforma (Linux/Mac/Windows), Framework solo Windows. 2) Core tiene Kestrel integrado, Framework necesita IIS. 3) Core usa `Program.cs` con minimal hosting, Framework usa `Global.asax` + `Web.config`. 4) Core tiene DI nativa, Framework necesita Unity/Ninject. 5) Core es open source, Framework es cerrado.

**P: ¿`Controller` vs `ControllerBase`?**
R: `Controller` hereda de `ControllerBase` y agrega soporte para vistas Razor (`.cshtml`) y `ViewBag`. `ControllerBase` es solo para APIs REST — devuelve JSON, sin vistas. Todos los controladores del proyecto usan `ControllerBase` porque es una API pura: `public class ProductosController : ControllerBase`.

**P: ¿`ActionResult` vs `IActionResult`?**
R: `IActionResult` es la interfaz — puede devolver cualquier tipo de acción HTTP (200, 201, 400, 404, etc.). `ActionResult<T>` es la versión genérica tipada — permite retornar `T` directamente o un `IActionResult`. El proyecto usa `IActionResult` porque los controladores devuelven `Resultado<T>` envuelto en `Ok()`, `BadRequest()`, etc.

**P: ¿`Global.asax` + `Web.config` vs `Program.cs` + `appsettings.json`?**
R: En MVC Framework, `Global.asax` manejaba eventos de aplicación y `Web.config` (XML) tenía configuración. En Core, `Program.cs` registra servicios y construye el pipeline, `appsettings.json` (JSON) tiene la configuración. En el proyecto, `Program.cs` registra DI, JWT, CORS, EF Core, MongoDB y middleware.

**P: ¿IIS obligatorio vs Kestrel?**
R: MVC Framework requería IIS en Windows. ASP.NET Core incluye Kestrel, un servidor HTTP multiplataforma embebido. En producción en Elastic Beanstalk, el contenedor Docker ejecuta Kestrel directamente en el puerto 80 (`ENV ASPNETCORE_URLS=http://+:80`), sin IIS.

**P: ¿`[ValidateAntiForgeryToken]` vs JWT?**
R: `[ValidateAntiForgeryToken]` protege formularios HTML contra ataques CSRF (Cross-Site Request Forgery) en apps con sesiones y cookies. JWT es stateless: el token firmado ya prueba la identidad. En APIs REST modernas (como este proyecto) no hay sesiones ni formularios, entonces JWT reemplaza completamente este mecanismo.

**P: ¿Cómo migrarías un proyecto MVC Framework a ASP.NET Core?**
R: 1) Crear proyecto nuevo en .NET 10. 2) Mover controllers a `ControllerBase` (quitar `ViewBag`, `Session`). 3) Reemplazar `Web.config` por `appsettings.json`. 4) Mover autenticación de Forms Auth a JWT. 5) Migrar BD con EF Core y Code First. 6) Mantener jQuery para páginas legadas mientras el equipo migra gradualmente a Vue.js.

**P: ¿Dónde vivía jQuery en proyectos MVC legacy y por qué tenía sentido?**
R: En `~/Scripts/jquery.min.js` (cargado por Bundling & Minification de MVC). Hacía sentido porque las vistas Razor del servidor generaban HTML y jQuery manipulaba el DOM del lado del cliente. En el proyecto, `legacy.html` en `public/` simula este escenario: consume `GET /api/productos` con `$.ajax()` y renderiza las tarjetas manipulando el DOM directamente.

---

## Sección 5 — API REST, JWT, Middleware, DI (10 preguntas)

**P: ¿Qué es el pipeline de middleware? ¿Cuál es el orden correcto en el proyecto?**
R: Cadena de componentes que procesa cada request HTTP. En `Program.cs` el orden es: 1) `ManejadorErroresMiddleware` (captura todo error), 2) Swagger, 3) HTTPS redirect (solo dev), 4) `UseCors`, 5) `UseAuthentication`, 6) `UseAuthorization`, 7) `MapControllers`. El orden importa: CORS antes de Auth para que los preflight OPTIONS pasen.

**P: ¿Por qué `UseAuthentication` debe ir antes que `UseAuthorization`?**
R: `UseAuthentication` lee y valida el token JWT del header `Authorization` y llena `HttpContext.User` con los claims. `UseAuthorization` luego verifica si ese usuario tiene permiso para el endpoint. Si van al revés, `User` está vacío cuando Authorization lo consulta y todos los `[Authorize]` fallan.

**P: ¿`AddTransient` vs `AddScoped` vs `AddSingleton`? ¿Cuál para DbContext?**
R: `Transient` = nueva instancia cada vez que se pide. `Scoped` = una instancia por request HTTP (correcto para `DbContext` — evita problemas de concurrencia entre requests). `Singleton` = una instancia toda la vida de la app (correcto para `HistorialBusquedaServicio` — `MongoClient` gestiona su propio pool). En `Program.cs`: `AddDbContext` es Scoped, `AddSingleton<HistorialBusquedaServicio>()`.

**P: ¿Cómo funciona JWT? ¿Cuáles son sus 3 partes? ¿Por qué es seguro si el payload es Base64?**
R: El token tiene 3 partes separadas por punto: Header (algoritmo) + Payload (claims: ID, nombre, email, rol) + Signature (HMAC-SHA256 del Header+Payload con la clave secreta). Es seguro porque sin la clave no puedes generar una firma válida — puedes leer el payload pero no falsificarlo. En `AuthServicio.cs`: `SecurityAlgorithms.HmacSha256`.

**P: ¿Por qué BCrypt y no MD5/SHA-256 para contraseñas?**
R: MD5 y SHA-256 son rápidos — permiten ataques de fuerza bruta con millones de intentos por segundo. BCrypt es deliberadamente lento (tiene un `work factor` — en el proyecto `$2a$11$...`) y genera un salt aleatorio. `BCrypt.Net.BCrypt.HashPassword(dto.Password)` en `AuthServicio.cs`. `BCrypt.Verify()` para validar.

**P: ¿`[Authorize]` vs `[Authorize(Roles)]` vs `[AllowAnonymous]`?**
R: `[Authorize]` = debe haber token JWT válido (cualquier rol). En `CarritoController.cs`: clase completa con `[Authorize]`. `[Authorize(Roles = "Admin")]` = solo usuarios con rol Admin. En `ProductosController.cs`: `[HttpPost][Authorize(Roles = "Admin")]`. `[AllowAnonymous]` sobreescribe `[Authorize]` — los endpoints GET de `ProductosController` lo tienen implícito al no tener `[Authorize]`.

**P: Códigos HTTP: 200, 201, 400, 401, 403, 404, 500 — ¿cuándo usar cada uno?**
R: 200 OK (GET/PUT exitoso), 201 Created (POST que crea recurso — `CreatedAtAction` en `Crear` de `ProductosController`), 400 Bad Request (validación fallida o datos incorrectos), 401 Unauthorized (sin token o token inválido — `AuthController.Login` devuelve 401 si credenciales incorrectas), 403 Forbidden (token válido pero sin permiso), 404 Not Found (producto no existe), 500 Internal Server Error (excepción no manejada — el middleware la captura).

**P: ¿Qué es CORS y por qué es necesario en este proyecto?**
R: Cross-Origin Resource Sharing — el navegador bloquea requests a un dominio diferente al de la página. El frontend Vue en `localhost:5173` o S3 (`http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com`) es un origen diferente al backend EB. En `Program.cs`: `WithOrigins("http://localhost:5173", "http://localhost:5000", "http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com")`.

**P: ¿Qué hace `[ApiController]` automáticamente?**
R: 1) Valida el modelo automáticamente y devuelve 400 si hay errores (sin necesitar `if (!ModelState.IsValid)`). 2) Infiere la fuente de los parámetros (`[FromBody]` en POST, `[FromQuery]` en GET). 3) Devuelve respuestas de error en formato `ProblemDetails`. Todos los controladores del proyecto tienen `[ApiController]`.

**P: ¿`[FromBody]` vs `[FromQuery]`?**
R: `[FromBody]` = lee el JSON del cuerpo del request HTTP (para POST/PUT). `[FromQuery]` = lee parámetros de la URL (`?termino=laptop`). En `ProductosController.cs`: `Crear([FromBody] CrearProductoDto dto)` para crear, `Buscar([FromQuery] string termino)` para búsqueda. En `CarritoController.cs`: `ActualizarCantidad(int productoId, [FromBody] ActualizarCantidadDto dto)` — el ID viene de la ruta, el body del cuerpo.

---

## Sección 6 — Entity Framework Core y SQL Server (10 preguntas)

**P: ¿Qué es un ORM? ¿Qué son `DbContext` y `DbSet`?**
R: ORM (Object-Relational Mapper) traduce entre objetos C# y tablas SQL. `DbContext` es la conexión a la BD y la unidad de trabajo de EF. `DbSet<T>` representa una tabla: `public DbSet<Producto> Productos => Set<Producto>();`. En `AppDbContext.cs` hay 7 DbSets, uno por cada entidad.

**P: ¿Code First vs Database First?**
R: Database First = tienes la BD primero, generas las clases. Code First = defines las clases C# primero, EF genera el SQL. Este proyecto usa Code First: defines `Producto.cs`, configuras con Fluent API en `AppDbContext.OnModelCreating()`, ejecutas `dotnet ef migrations add Init`, EF genera el SQL de creación de tablas.

**P: ¿Fluent API vs Data Annotations?**
R: Data Annotations son atributos en la clase: `[Required]`, `[MaxLength(200)]`. Fluent API es código en `OnModelCreating()`: `e.Property(p => p.Nombre).IsRequired().HasMaxLength(200)`. El proyecto usa Fluent API en `AppDbContext.cs` para mantener las entidades limpias sin dependencias de atributos de EF.

**P: ¿`IEnumerable<T>` vs `IQueryable<T>`?**
R: `IEnumerable` = datos ya en memoria — filtrar con LINQ ejecuta C#. `IQueryable` = consulta pendiente — filtrar con LINQ genera SQL. `_contexto.Productos.Where(p => p.Activo)` es `IQueryable`, el SQL se envía hasta `.ToListAsync()`. Si llamas `.ToList()` antes del `.Where()`, traes todos los productos a memoria y filtras en C# (más lento).

**P: ¿`Include`/`ThenInclude`? ¿Qué es el problema N+1?**
R: `Include(p => p.Categoria)` genera un JOIN para traer la categoría junto con el producto en una sola query SQL. Sin él, `p.Categoria` sería null. N+1: si cargas 100 productos sin Include y luego accedes a `producto.Categoria` en un bucle, EF hace 100 queries adicionales. `ProductoRepositorio.ObtenerActivosAsync()` usa `Include(p => p.Categoria)` para evitarlo.

**P: ¿Qué son las migraciones? Comandos para crear, aplicar y revertir.**
R: Archivos C# generados por EF que registran los cambios al esquema de BD. Crear: `dotnet ef migrations add NombreMigracion`. Aplicar: `dotnet ef database update`. Revertir: `dotnet ef database update NombreMigracionAnterior`. En producción AWS se usa `EnsureCreated()` (InMemory) en vez de migraciones.

**P: ¿`HasData` para seed data?**
R: Inserta datos iniciales con la migración. En `AppDbContext.AgregarDatosIniciales()`: `mb.Entity<Categoria>().HasData(new Categoria { Id = 1, Nombre = "Electrónica", ... })`. Se usan IDs fijos y `fechaSeed = new DateTime(2026, 1, 1...)` fija para que EF no detecte "pending changes" en cada migración.

**P: ¿`DeleteBehavior.Restrict` vs `DeleteBehavior.Cascade`?**
R: `Cascade` = si borras el padre, se borran automáticamente los hijos. En `AppDbContext.cs`: Carrito → CarritoItems es Cascade (borrar el carrito borra sus items). `Restrict` = no puedes borrar el padre si tiene hijos. Categoria → Productos es Restrict (no puedes borrar una categoría que tiene productos) para proteger la integridad histórica.

**P: ¿Por qué `HasColumnType("decimal(18,2)")`?**
R: Sin especificarlo, EF usa `decimal(18,0)` — sin decimales. `decimal(18,2)` permite hasta 16 dígitos enteros + 2 decimales (centavos). Usado en `Precio` de `Producto`, `PrecioUnitario` y `Subtotal` de `OrdenDetalle`, `Total` de `Orden`. Sin esto, `$25,999.99` se guardaría como `$26,000.00`.

**P: ¿`INNER JOIN` vs `LEFT JOIN`? ¿Cuándo EF genera cada uno?**
R: `INNER JOIN` solo retorna filas que tienen coincidencia en ambas tablas. `LEFT JOIN` retorna todas las filas de la tabla izquierda aunque no haya coincidencia. EF usa `INNER JOIN` cuando la relación es requerida (`CategoriaId` no nullable). Usa `LEFT JOIN` cuando la navegación es opcional (nullable). El `Include(p => p.Categoria)` en `ProductoRepositorio` genera `INNER JOIN` porque `CategoriaId` es requerido.

---

## Sección 7 — SQL avanzado (6 preguntas)

**P: ¿`GROUP BY` con `COUNT`, `SUM`, `AVG`?**
R: Agrupa filas y aplica funciones de agregación. Ejemplo equivalente al pipeline de MongoDB en el proyecto: `SELECT Termino, COUNT(*) AS TotalBusquedas FROM busquedas GROUP BY Termino ORDER BY TotalBusquedas DESC`. En LINQ: `.GroupBy(b => b.Termino).Select(g => new { Termino = g.Key, Total = g.Count() })`.

**P: ¿Qué es una subconsulta?**
R: Una SELECT dentro de otra SELECT. Por ejemplo, obtener productos con precio mayor al promedio: `SELECT * FROM Productos WHERE Precio > (SELECT AVG(Precio) FROM Productos)`. En EF LINQ: `var promedio = await _contexto.Productos.AverageAsync(p => p.Precio); var caros = await _contexto.Productos.Where(p => p.Precio > promedio).ToListAsync();`.

**P: ¿Qué es un CTE (`WITH`)? ¿`ROW_NUMBER OVER PARTITION BY`?**
R: CTE (Common Table Expression) es un resultado temporal con nombre usado en la misma query: `WITH TopProductos AS (SELECT ... FROM ...) SELECT * FROM TopProductos WHERE Rango <= 3`. `ROW_NUMBER() OVER (PARTITION BY CategoriaId ORDER BY Precio)` numera filas dentro de cada categoría — útil para "top 3 más baratos por categoría".

**P: ¿Qué es un Stored Procedure? ¿Cuándo usarlo vs LINQ?**
R: Código SQL almacenado en la BD con nombre, reutilizable. Usar SP cuando: la query es muy compleja (múltiples joins, CTEs), necesitas transacciones avanzadas, o el DBA del equipo lo exige. Usar LINQ cuando: la consulta es simple, quieres type-safety y refactoring automático. En este proyecto todo es LINQ con EF Core.

**P: ¿Qué es un índice? ¿`CLUSTERED` vs `NONCLUSTERED`?**
R: Estructura de datos que acelera las búsquedas. `CLUSTERED` = define el orden físico de los datos en disco (solo uno por tabla, normalmente el PK). `NONCLUSTERED` = índice separado con punteros a los datos (varios por tabla). En `AppDbContext.cs`: `e.HasIndex(p => p.Nombre)` crea un NONCLUSTERED index en `Nombre` para que `WHERE Nombre LIKE '%laptop%'` sea rápido. `HasIndex(u => u.Email).IsUnique()` garantiza emails únicos.

**P: ¿Qué es un plan de ejecución? ¿`Table Scan` vs `Index Seek`?**
R: El plan de ejecución es la estrategia que SQL Server elige para ejecutar una query. `Table Scan` = revisa cada fila de la tabla (lento en tablas grandes, sin índice). `Index Seek` = usa el índice para ir directo a las filas relevantes (rápido). Se ve en SSMS con `SET STATISTICS IO ON` o Ctrl+M. La diferencia entre buscar un producto por nombre sin y con `HasIndex(p => p.Nombre)`.

---

## Sección 8 — MongoDB (4 preguntas)

**P: ¿SQL Server vs MongoDB? ¿Cuándo usar cada uno?**
R: SQL Server: datos relacionales, transacciones ACID, esquema fijo, queries complejos con JOINs. MongoDB: documentos JSON flexibles, sin esquema rígido, escala horizontal, ideal para datos variables. En el proyecto: SQL Server para pedidos/usuarios/productos (integridad crítica), MongoDB para historial de búsquedas (`BusquedaHistorial.cs`) — no necesita joins, el esquema puede cambiar.

**P: ¿Por qué MongoDB es `Singleton` y `DbContext` es `Scoped`?**
R: `MongoClient` gestiona internamente un pool de conexiones y está diseñado para ser una instancia por aplicación — crearlo por request tiene overhead. `DbContext` de EF Core rastrea entidades en memoria y no es thread-safe — debe ser por request (Scoped) para evitar que dos requests simultáneos vean el estado del otro. Ver `Program.cs`: `AddSingleton<HistorialBusquedaServicio>()` vs `AddDbContext` (Scoped).

**P: ¿Qué son `[BsonId]` y `[BsonRepresentation]`?**
R: En `BusquedaHistorial.cs`: `[BsonId]` marca la propiedad como el campo `_id` de MongoDB (equivale a PK en SQL). `[BsonRepresentation(BsonType.ObjectId)]` indica que el `string` en C# se serializa como `ObjectId` de MongoDB (formato de 24 caracteres hexadecimales, auto-generado por Mongo). Sin esto, MongoDB no sabe cuál campo es el identificador.

**P: ¿`MongoClient`, `IMongoCollection<T>`, `Aggregate`?**
R: En `HistorialBusquedaServicio.cs`: `MongoClient` = conexión al servidor. `GetDatabase("EcommerceNetDB")` = selecciona la BD. `GetCollection<BusquedaHistorial>("busquedas")` = equivalente a `DbSet<T>`. `Aggregate().Group().SortByDescending().Limit()` = pipeline de agregación para obtener términos más buscados. Equivale a `GROUP BY ... ORDER BY ... LIMIT` en SQL.

---

## Sección 9 — Vue.js 3, Pinia y jQuery (10 preguntas)

**P: ¿Qué es una SPA? ¿Virtual DOM?**
R: SPA (Single Page Application) = una sola página HTML que Vue actualiza dinámicamente con JavaScript sin recargar completo. Virtual DOM = representación en memoria del DOM real; Vue compara el Virtual DOM anterior con el nuevo y solo actualiza las partes que cambiaron (diffing), haciendo la UI rápida. El frontend en `src/EcommerceNet.Web` es la SPA, servida desde S3.

**P: ¿Composition API vs Options API?**
R: Options API (Vue 2): se organiza el código en secciones `data()`, `methods`, `computed`, `mounted`. Composition API (Vue 3): todo junto en `<script setup>`, agrupado por funcionalidad. Todos los componentes del proyecto usan Composition API: `<script setup>` en `TiendaView.vue`, `CarritoView.vue`, `NavBar.vue`, etc. Es más legible y permite mejor reutilización de lógica.

**P: ¿`ref()` vs `reactive()`?**
R: `ref()` envuelve cualquier tipo (primitivos o objetos) en un objeto reactivo accesible con `.value`. `reactive()` hace reactivo un objeto completo pero no funciona con primitivos. En las stores del proyecto se usa `ref()` para todo: `const productos = ref([])`, `const cargando = ref(false)`, `const token = ref(localStorage.getItem('jwt_token') || '')`.

**P: ¿`computed()` — para qué? Muestra `productosFiltrados`.**
R: Propiedad derivada que se recalcula automáticamente cuando cambian sus dependencias reactivas — solo cuando es necesario (cacheada). En `productoStore.js`: `const productosFiltrados = computed(() => { let resultado = productos.value; if (categoriaSeleccionada.value) resultado = resultado.filter(...); if (terminoBusqueda.value.trim()) resultado = resultado.filter(...); return resultado; })`. Se recalcula solo cuando cambia `productos`, `categoriaSeleccionada` o `terminoBusqueda`.

**P: ¿`onMounted` — ciclo de vida de un componente?**
R: Los hooks de ciclo de vida permiten ejecutar código en momentos específicos. `onMounted` corre cuando el componente ya está en el DOM. En `TiendaView.vue` se usa para cargar productos al entrar a la página: `onMounted(() => productoStore.cargarProductos())`. Equivale a `mounted()` en Options API o `componentDidMount` en React.

**P: ¿`defineProps` y `defineEmits`?**
R: `defineProps` declara los datos que el padre pasa al hijo. `defineEmits` declara los eventos que el hijo puede emitir al padre. En `ProductoCard.vue`: `defineProps({ producto: Object })` para recibir el producto. `defineEmits(['agregar-al-carrito'])` para notificar al padre cuando el usuario hace clic en "Agregar". Comunicación: padre → hijo con props, hijo → padre con emits.

**P: ¿Pinia vs Vuex?**
R: Pinia es el estado global oficial para Vue 3. Es más simple (sin mutations, solo state + getters + actions), tiene soporte nativo para Composition API (`defineStore` con `ref/computed`), mejor TypeScript support y DevTools integration. Vuex requería mutations separadas para modificar el estado. El proyecto tiene 3 stores Pinia: `authStore.js`, `carritoStore.js`, `productoStore.js`.

**P: ¿Vue Router: navigation guards, `meta`, `beforeEach`?**
R: Navigation guards protegen rutas. En `router/index.js`: las rutas tienen `meta: { requiereAuth: true, requiereAdmin: true }`. El guard `router.beforeEach((to, from, next) => { if (to.meta.requiereAuth && !auth.estaLogueado) next({ name: 'login' }); else if (to.meta.requiereAdmin && !auth.esAdmin) next({ name: 'tienda' }); else next(); })`. Protege `/carrito`, `/checkout`, `/mis-ordenes`, `/admin`.

**P: ¿Axios interceptores: request y response?**
R: En `api.js`: el interceptor de request agrega el JWT automáticamente a cada llamada: `config.headers.Authorization = 'Bearer ' + token`. El interceptor de response maneja errores globalmente: si la respuesta es 401 (token expirado), limpia `localStorage` y redirige a `/login`. Sin interceptores, tendría que agregar el token y manejar 401 en cada llamada API.

**P: ¿jQuery vs Vue.js — imperativo vs declarativo?**
R: jQuery es imperativo: tú dices paso a paso cómo manipular el DOM (`$('#grid').html('')`, `$.ajax(...)`, `$(tarjeta).appendTo('#grid')`). Vue es declarativo: describes el estado y Vue actualiza el DOM automáticamente (`v-for="producto in productos"` — si cambia `productos`, el DOM se actualiza solo). `legacy.html` usa jQuery imperativo. La SPA usa Vue declarativo.

---

## Sección 10 — Docker, CI/CD y GitHub Actions (6 preguntas)

**P: ¿Contenedor vs VM?**
R: VM virtualiza hardware completo (SO + kernel) — usa varios GB, tarda minutos en arrancar. Contenedor comparte el kernel del host, solo empaqueta la app y sus dependencias — usa MB, arranca en segundos. El contenedor del proyecto pesa ~200 MB (solo el runtime `aspnet:10.0`). Una VM equivalente pesaría 5-10 GB.

**P: ¿`Dockerfile`: `FROM`, `WORKDIR`, `COPY`, `RUN`, `EXPOSE`, `ENTRYPOINT`?**
R: En el `Dockerfile` raíz: `FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build` — imagen base. `WORKDIR /app` — directorio de trabajo. `COPY EcommerceNet.slnx .` — copiar archivo. `RUN dotnet restore` — ejecutar comando. `EXPOSE 80` — declarar puerto (solo documentación). `ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]` — comando al iniciar el contenedor.

**P: ¿Multi-stage build — por qué?**
R: El `sdk:10.0` pesa ~800 MB (incluye compilador, herramientas). El `aspnet:10.0` pesa ~200 MB (solo runtime). Con multi-stage: etapa 1 usa `sdk` para compilar, etapa 2 usa `aspnet` para ejecutar y solo copia el binario compilado (`COPY --from=build /publish .`). El contenedor final pesa ~200 MB en vez de ~800 MB. Mejora seguridad (menos superficie de ataque) y velocidad de deploy.

**P: ¿`docker-compose`: `services`, `ports "5000:80"`, `volumes`, `depends_on`?**
R: En `docker-compose.yml`: `services` define los contenedores (api, sqlserver, mongo). `ports: "5000:80"` mapea puerto del host → puerto del contenedor. `volumes: sqlserver-data:/var/opt/mssql` persiste datos entre reinicios. `depends_on: sqlserver` espera que SQL Server esté corriendo antes de iniciar la API.

**P: ¿CI (Continuous Integration) vs CD (Continuous Delivery)?**
R: CI = integrar código frecuentemente y verificar que compila y los tests pasan en cada push. CD = desplegar automáticamente ese código validado a un ambiente. En el proyecto: CI = el job `backend` que hace `dotnet restore`, `build`, `test` en cada push. CD = el job publica el artefacto en `main`. No hay deploy automático a EB (el deploy se hace manual con `eb deploy`).

**P: ¿GitHub Actions: workflow, jobs, steps, triggers?**
R: En `.github/workflows/ci-cd.yml`: el `workflow` es el archivo completo. `on: push: branches: [main, desarrollo]` es el trigger. `jobs` son unidades paralelas: `backend` y `frontend` corren simultáneamente en runners `ubuntu-latest`. `steps` son los pasos secuenciales dentro de cada job: checkout → setup .NET → restore → build → test → publish → upload artifact.

---

## Sección 11 — AWS (6 preguntas)

**P: ¿EC2 (IaaS) vs Elastic Beanstalk (PaaS)?**
R: EC2 es IaaS: tú administras el servidor, instala dependencias, configuras nginx, actualizas el OS. EB es PaaS: tú subes la app (Docker o .zip), AWS administra la infraestructura, load balancer, health checks, auto-scaling. Para el proyecto, EB lee el `Dockerfile` raíz, construye la imagen y la despliega en una instancia `t3.micro` automáticamente.

**P: ¿Por qué S3 para el frontend y no Nginx en EC2?**
R: S3 static website hosting es serverless, escala infinitamente, cuesta centavos (o nada en Free Tier), y no necesita mantenimiento de servidor. Nginx en EC2 requiere configurar el servidor, aplicar parches, pagar por la instancia 24/7. Para una SPA (archivos HTML/CSS/JS estáticos), S3 es la opción más simple y económica.

**P: ¿IAM — por qué nunca usar el usuario root?**
R: Root tiene acceso ilimitado a todos los servicios y no puede ser restringido — si se comprometen las credenciales root, todo el account es vulnerable. Se crea un usuario IAM (`ecommercenet-deploy`) con solo los permisos necesarios (AWSElasticBeanstalkFullAccess + AmazonS3FullAccess + IAMUserChangePassword). Root solo se usa para configurar MFA y crear el primer usuario IAM.

**P: ¿Free Tier de AWS — qué incluye?**
R: Por 12 meses: EC2 750 horas/mes de `t2.micro` o `t3.micro`. S3: 5 GB de almacenamiento, 20,000 GET, 2,000 PUT. RDS: 750 horas `db.t2.micro`. El proyecto usa EB (una instancia `t3.micro`) + S3 (hosting del frontend) — ambos dentro del Free Tier. Total: $0 durante los primeros 12 meses.

**P: ¿Qué errores reales ocurrieron en el deploy y cómo se resolvieron?**
R: Error 1: `COPY EcommerceNet.sln` fallaba — solución: `.sln` no existía, .NET 10 crea `.slnx`. Cambio: `COPY EcommerceNet.slnx .`. Error 2: EB usaba `docker-compose.yml` en vez del Dockerfile — solución: crear `.ebignore` excluyendo `docker-compose.yml`. Error 3: `sdk:8.0` no soporta `.slnx` — solución: usar `sdk:10.0` y `aspnet:10.0`. Documentado en `docs/guia-deploy-aws.md`.

**P: ¿Cómo fue el flujo completo de deploy?**
R: Backend: `eb init` (configurar proyecto EB) → `eb create ecommercenet-api --single --instance-type t3.micro` (crear entorno) → `eb setenv Jwt__Key="..." UseInMemoryDatabase=true` (variables de entorno) → `eb deploy` (deploy de actualizaciones). Frontend: `npm run build` → `aws s3 mb s3://ecommercenet-ramiro671` → configurar acceso público → `aws s3 sync dist/ s3://ecommercenet-ramiro671`. CORS actualizado en `Program.cs` con la URL de S3.

---

## Sección 12 — Empresa y entrevista (6 preguntas)

**P: ¿Qué son los modelos de Studios y Pods en empresas tech?**
R: empresa tech organiza su trabajo en 4 Studios: Launch Pod (construir productos desde cero para startups), Growth Pod (escalar productos existentes), Enterprise Pod (soluciones para grandes empresas), y AWS Migration Pod (migrar empresas a la nube AWS — Las empresas con certificación AWS Partner). Cada Studio tiene equipos de fullstack, diseño y QA.

**P: ¿Qué es el Launch Pod y el AWS Migration Pod?**
R: Launch Pod: equipo que lleva una idea a producción rápidamente — MVP en semanas. AWS Migration Pod: ayuda a empresas a migrar sus sistemas on-premise a AWS con buenas prácticas de arquitectura cloud. Este proyecto demuestra habilidades para ambos: fullstack completo (Launch) + deploy en AWS con Docker y EB (Migration).

**P: "Cuéntame de tu proyecto" — respuesta de 60 segundos.**
R: "Construí EcommerceNet en 5 días para demostrar dominio del stack. Es una tienda online con backend ASP.NET Core .NET 10 siguiendo Clean Architecture — 4 capas, repositorios, Unit of Work, JWT. Frontend Vue.js 3 con Pinia, carrito completo y panel admin. 23 pruebas unitarias, CI/CD con GitHub Actions y deploy en producción en AWS: la API en Elastic Beanstalk con Docker multi-stage, el frontend en S3. El proyecto está live ahora mismo — puedo mostrártelo en Swagger o en el frontend."

**P: "¿Por qué esta empresa?" — respuesta.**
R: "empresa tech opera en los dos ámbitos que más me interesan: construir productos nuevos desde cero (Launch Pod) y cloud migration en AWS (Migration Pod). Ser AWS Partner significa que los proyectos tienen estándares reales de arquitectura cloud, no solo deployar en un servidor. Además, el stack .NET + Vue.js que usan es exactamente lo que construí en EcommerceNet — puedo ser productivo desde el primer día."

**P: "¿Cuál es tu expectativa salarial?" — respuesta.**
R: "Estoy buscando entre [X y Y] pesos mensuales brutos, acorde al rango de mercado para Senior Fullstack .NET en CDMX / remoto. Estoy abierto a conversar el paquete completo — esquema de trabajo, prestaciones, crecimiento. Lo más importante para mí es el tipo de proyectos y el equipo." *(Ajusta los números según tu investigación de mercado antes de la entrevista.)*

**P: "¿Cómo conectas tu proyecto con el modelo ágil de pods?"**
R: "EcommerceNet replica exactamente el tipo de trabajo de un equipo ágil: Clean Architecture como base para proyectos escalables (Launch Pod), Docker + Elastic Beanstalk como punto de entrada a AWS (Migration Pod), CI/CD con GitHub Actions para entregas continuas. La separación en capas con interfaces facilita agregar nuevos desarrolladores al equipo sin romper el código existente — exactamente lo que necesita un equipo de pods como el de empresas tech."

---

## Referencia rápida — Archivos clave del proyecto

| Concepto | Archivo |
|----------|---------|
| Entidad con lógica de dominio | `src/EcommerceNet.Core/Entidades/Carrito.cs` |
| Interfaz genérica repository | `src/EcommerceNet.Core/Interfaces/IRepositorio.cs` |
| Resultado<T> estándar | `src/EcommerceNet.Core/DTOs/Resultado.cs` |
| Repositorio con override + Include | `src/EcommerceNet.Data/Repositorios/ProductoRepositorio.cs` |
| Unit of Work con ??= | `src/EcommerceNet.Data/UnidadDeTrabajo.cs` |
| Fluent API + seed data | `src/EcommerceNet.Data/AppDbContext.cs` |
| BCrypt + JWT generation | `src/EcommerceNet.Data/Servicios/AuthServicio.cs` |
| Pipeline middleware + DI | `src/EcommerceNet.API/Program.cs` |
| Middleware de errores | `src/EcommerceNet.API/Middleware/ManejadorErroresMiddleware.cs` |
| CRUD admin + [Authorize(Roles)] | `src/EcommerceNet.API/Controllers/ProductosController.cs` |
| MongoDB Singleton + Aggregate | `src/EcommerceNet.Data/MongoDB/HistorialBusquedaServicio.cs` |
| Axios interceptors | `src/EcommerceNet.Web/src/services/api.js` |
| Pinia store + computed | `src/EcommerceNet.Web/src/stores/productoStore.js` |
| Router guards + meta | `src/EcommerceNet.Web/src/router/index.js` |
| jQuery imperativo | `src/EcommerceNet.Web/public/legacy.html` |
| Multi-stage Dockerfile | `Dockerfile` (raíz) |
| GitHub Actions CI/CD | `.github/workflows/ci-cd.yml` |
