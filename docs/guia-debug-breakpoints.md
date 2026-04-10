# Guía de Breakpoints — Debug de los 3 Flujos Completos

> Líneas exactas extraídas del código real del proyecto. Usar junto con VS Code o Visual Studio.

---

## Configuración inicial del entorno de debug

### 1. Debuggear el backend .NET en VS Code

Crear o verificar `.vscode/launch.json` en la raíz del repositorio:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "EcommerceNet API",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/EcommerceNet.API/bin/Debug/net10.0/EcommerceNet.API.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/EcommerceNet.API",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "preLaunchTask": "build"
    },
    {
      "name": "Adjuntar a proceso .NET",
      "type": "coreclr",
      "request": "attach"
    }
  ]
}
```

**Poner breakpoints en VS Code:** hacer clic en el margen izquierdo (antes del número de línea). Aparece un círculo rojo. Presionar `F5` para iniciar la sesión de debug.

**Panel de debug (izquierda):**
- **Variables** — ver el estado actual de todas las variables en scope
- **Watch** — agregar expresiones a monitorear (`carrito.Items.Count`, `orden.Total`)
- **Call Stack** — ver toda la cadena de llamadas que llegó a este punto
- **Breakpoints** — lista de todos los breakpoints activos

### 2. Debuggear el frontend Vue.js en el navegador

1. Abrir DevTools (`F12` o `Ctrl+Shift+I`)
2. Ir a la pestaña **Sources** (Chrome) o **Debugger** (Firefox)
3. Buscar los archivos: `src/stores/authStore.js`, `src/views/TiendaView.vue`, etc.
4. Hacer clic en el número de línea para poner un breakpoint
5. Al ejecutar la acción en la página, el navegador pausa en esa línea

**Console.log estratégicos para stores:**
```js
// En carritoStore.js, antes de api.post:
console.log('[DEBUG] Enviando al carrito:', { productoId, cantidad })
console.log('[DEBUG] Respuesta:', data)

// En authStore.js, después del login:
console.log('[DEBUG] Token recibido:', data.datos.token)
console.log('[DEBUG] Usuario:', data.datos)
```

### 3. Debuggear ambos simultáneamente

```bash
# Terminal 1 — Backend (con debug activo desde VS Code F5)
# O manualmente:
cd src/EcommerceNet.API
dotnet run

# Terminal 2 — Frontend
cd src/EcommerceNet.Web
npm run dev
```

**Flujo completo:** poner breakpoint en el backend → ejecutar acción en el navegador → VS Code pausa en el backend → inspeccionar variables → presionar F5 para continuar → ver respuesta en DevTools del navegador.

---

## Flujo 1: Registro y Login

> Traza completa: usuario llena el formulario → Vue llama a la API → JWT generado → guardado en localStorage

### 1.1 Frontend — Vista de Registro

📍 `src/EcommerceNet.Web/src/views/RegistroView.vue:28`
```js
const exito = await auth.registrar(nombre.value, email.value, password.value)
```
**Qué verificar:** `nombre.value`, `email.value`, `password.value` — ¿los datos del formulario llegaron correctamente? ¿La validación local (contraseñas coinciden, mínimo 6 chars) ya pasó?

📍 `src/EcommerceNet.Web/src/views/RegistroView.vue:29`
```js
if (exito) {
```
**Qué verificar:** el valor de `exito` — si es `false`, revisar `auth.error` para saber qué falló.

### 1.2 Frontend — Vista de Login

📍 `src/EcommerceNet.Web/src/views/LoginView.vue:14`
```js
const exito = await auth.login(email.value, password.value)
```
**Qué verificar:** `email.value`, `password.value` antes de la llamada. Si falla, `auth.error` tendrá el mensaje.

📍 `src/EcommerceNet.Web/src/views/LoginView.vue:16`
```js
const redirect = route.query.redirect || '/'
```
**Qué verificar:** si el usuario vino de una ruta protegida, `route.query.redirect` tendrá la URL original (ej: `/carrito`).

### 1.3 Frontend — authStore (llamada real a la API)

📍 `src/EcommerceNet.Web/src/stores/authStore.js:23`
```js
const { data } = await api.post('/auth/login', { email, password })
```
**Qué verificar:** que `email` y `password` estén correctos. Si hay un error de red, el `catch` en línea 38 lo atrapará.

📍 `src/EcommerceNet.Web/src/stores/authStore.js:26`
```js
token.value = data.datos.token
```
**Qué verificar:** `data.exito === true`, `data.datos.token` (el JWT — debe empezar con `eyJ`), `data.datos.rol` (debe ser `"Cliente"` o `"Admin"`).

📍 `src/EcommerceNet.Web/src/stores/authStore.js:31`
```js
localStorage.setItem('jwt_token', data.datos.token)
```
**Qué verificar:** en DevTools → Application → Local Storage → `jwt_token` debe aparecer con el token. `localStorage.getItem('usuario')` debe tener el objeto JSON del usuario.

📍 `src/EcommerceNet.Web/src/stores/authStore.js:50` (equivalente en función registrar)
```js
const { data } = await api.post('/auth/registrar', { nombre, email, password })
```
**Qué verificar:** mismos campos que login, más `nombre`. Si el email ya existe, `data.exito` será `false`.

### 1.4 Frontend — Interceptor de request (api.js)

📍 `src/EcommerceNet.Web/src/services/api.js:17`
```js
const token = localStorage.getItem('jwt_token')
```
**Qué verificar:** ¿el token está en localStorage? Si es `null`, la petición saldrá sin header Authorization.

📍 `src/EcommerceNet.Web/src/services/api.js:19`
```js
config.headers.Authorization = `Bearer ${token}`
```
**Qué verificar:** `config.headers.Authorization` debe ser `"Bearer eyJhbGci..."`. Cualquier petición autenticada pasa por aquí antes de salir.

### 1.5 Backend — Middleware (PRIMER PUNTO en toda petición)

📍 `src/EcommerceNet.API/Middleware/ManejadorErroresMiddleware.cs:36`
```csharp
await _next(contexto);
```
**Qué verificar:** `contexto.Request.Path` (ej: `/api/auth/login`), `contexto.Request.Method` (`POST`). Si se lanza cualquier excepción dentro del `_next`, caerá en el `catch` de la línea 43.

### 1.6 Backend — AuthController

📍 `src/EcommerceNet.API/Controllers/AuthController.cs:29`
```csharp
var resultado = await _authServicio.RegistrarAsync(dto);
```
**Qué verificar:** `dto.Nombre`, `dto.Email`, `dto.Password` — ¿llegaron correctamente del JSON? ASP.NET Core hace el binding automático del body JSON al DTO.

📍 `src/EcommerceNet.API/Controllers/AuthController.cs:44`
```csharp
var resultado = await _authServicio.LoginAsync(dto);
```
**Qué verificar:** `dto.Email`, `dto.Password`. Si `resultado.Exito == false`, la línea 47 devuelve `401 Unauthorized`.

### 1.7 Backend — AuthServicio (lógica de negocio)

📍 `src/EcommerceNet.Data/Servicios/AuthServicio.cs:35`
```csharp
var existe = await _contexto.Usuarios.AnyAsync(u => u.Email == dto.Email);
```
**Qué verificar:** el resultado de `existe` — si es `true`, se retorna error "Ya existe un usuario con ese email" en la línea 37.

📍 `src/EcommerceNet.Data/Servicios/AuthServicio.cs:44`
```csharp
PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
```
**Qué verificar:** `dto.Password` antes del hash. El hash generado empieza con `$2a$11$` (work factor 11). NUNCA guardar `dto.Password` directamente.

📍 `src/EcommerceNet.Data/Servicios/AuthServicio.cs:65`
```csharp
if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
```
**Qué verificar:** `dto.Password` (lo que escribió el usuario), `usuario.PasswordHash` (el hash en BD). Si retorna `false`, se devuelve `"Credenciales incorrectas"`. Notar que el mensaje es genérico (no dice si el email no existe) — intencional para evitar enumeración de usuarios.

📍 `src/EcommerceNet.Data/Servicios/AuthServicio.cs:80`
```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
    new Claim(ClaimTypes.Name, usuario.Nombre),
    new Claim(ClaimTypes.Email, usuario.Email),
    new Claim(ClaimTypes.Role, usuario.Rol.ToString())
};
```
**Qué verificar:** los 4 claims que se incrustan en el token. El claim `ClaimTypes.Role` es el que usa `[Authorize(Roles = "Admin")]`. El `NameIdentifier` es el que extrae `ObtenerUsuarioId()` en los controladores.

📍 `src/EcommerceNet.Data/Servicios/AuthServicio.cs:91`
```csharp
var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
```
**Qué verificar:** `key` — es la clave secreta leída de `appsettings.json` (`Jwt:Key`). En producción se inyecta via `eb setenv`.

📍 `src/EcommerceNet.Data/Servicios/AuthServicio.cs:97`
```csharp
var token = new JwtSecurityToken(
    issuer: _config["Jwt:Issuer"],
    audience: _config["Jwt:Audience"],
    claims: claims,
    expires: expira,
    signingCredentials: credenciales);
```
**Qué verificar:** `expira` (cuándo vence el token), el `issuer` (`EcommerceNet.API`), el `audience` (`EcommerceNet.Web`). El token resultante en línea 106 es el JWT que va al cliente.

---

## Flujo 2: Administrador — CRUD de productos

> Traza: admin abre el panel → crea/edita/elimina producto → cambio visible en el catálogo

### 2.1 Frontend — AdminView (panel de administración)

📍 `src/EcommerceNet.Web/src/views/AdminView.vue:37`
```js
onMounted(() => {
  cargarProductos()
  cargarCategorias()
})
```
**Qué verificar:** ¿el componente monta correctamente? Si el usuario no es admin, el router guard de `router/index.js:71` lo redirige a tienda antes de llegar aquí.

📍 `src/EcommerceNet.Web/src/views/AdminView.vue:114`
```js
const { data } = await api.get('/productos')
```
**Qué verificar:** `data.exito`, `data.datos.length` (cuántos productos cargaron). El interceptor de api.js:19 ya agregó el Bearer token.

📍 `src/EcommerceNet.Web/src/views/AdminView.vue:167`
```js
await api.post('/productos', payload)
```
**Qué verificar:** `payload` — el objeto completo (`nombre`, `precio`, `stock`, `categoriaId`). Si la categoría no está seleccionada, `categoriaId` puede ser incorrecto.

📍 `src/EcommerceNet.Web/src/views/AdminView.vue:164`
```js
await api.put(`/productos/${formProducto.value.id}`, payload)
```
**Qué verificar:** `formProducto.value.id` — ¿tiene el ID correcto del producto a editar? Lo asigna la función `editarProducto()` en línea 134.

📍 `src/EcommerceNet.Web/src/views/AdminView.vue:181`
```js
await api.delete(`/productos/${id}`)
```
**Qué verificar:** `id` — el ID del producto a eliminar. Verifica que el usuario haya confirmado el `confirm()` en línea 178.

### 2.2 Backend — ProductosController

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:91`
```csharp
public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
```
**Qué verificar:** que el atributo `[Authorize(Roles = "Admin")]` en línea 90 haya pasado — si el claim `Role` no es `"Admin"`, ASP.NET Core retorna 403 automáticamente sin llegar aquí. Inspeccionar `User.Claims` para ver todos los claims del token.

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:116`
```csharp
await _uow.Productos.AgregarAsync(producto);
```
**Qué verificar:** el objeto `producto` completo antes de persistir. Aún no está en BD — solo está tracked por EF Core en estado `Added`.

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:117`
```csharp
await _uow.GuardarCambiosAsync();
```
**Qué verificar:** el valor retornado (número de filas afectadas — debe ser `1`). Si lanza excepción, significa violación de constraint en BD (ej: `CategoriaId` no existe).

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:37`
```csharp
var productos = await _uow.Productos.ObtenerActivosAsync();
```
**Qué verificar:** `productos.Count()` — cuántos productos activos hay. Este es el endpoint público que usa la TiendaView.

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:128`
```csharp
public async Task<IActionResult> Actualizar(int id, [FromBody] CrearProductoDto dto)
```
**Qué verificar:** `id` (de la URL), `dto` (del body JSON). La línea 131 busca el producto en BD — si no existe, retorna 404.

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:151`
```csharp
public async Task<IActionResult> Eliminar(int id)
```
**Qué verificar:** `id` — el producto a eliminar. La línea 153 lo busca en BD. Si tiene OrderDetalles asociados, `DeleteBehavior.Restrict` en AppDbContext causará una excepción de integridad referencial.

### 2.3 Backend — ProductoRepositorio (queries a la BD)

📍 `src/EcommerceNet.Data/Repositorios/ProductoRepositorio.cs:71`
```csharp
return await _contexto.Productos
    .Include(p => p.Categoria)
    .Where(p => p.Activo)
    .OrderByDescending(p => p.FechaCreacion)
    .ToListAsync();
```
**Qué verificar:** ¿el `Include` cargó la categoría? Sin `Include`, `p.Categoria` sería `null` (EF Core no hace lazy loading por defecto). El SQL generado incluirá un INNER JOIN con Categorias.

📍 `src/EcommerceNet.Data/Repositorios/ProductoRepositorio.cs:33`
```csharp
return await _contexto.Productos
    .Include(p => p.Categoria)
    .Where(p => p.Activo && p.Nombre.Contains(termino))
    .OrderBy(p => p.Nombre)
    .ToListAsync();
```
**Qué verificar:** `termino` — el string de búsqueda. `Contains()` se traduce a SQL `LIKE '%termino%'`. El índice en `Nombre` (AppDbContext.cs:72) hace esta query más rápida.

### 2.4 Backend — RepositorioBase (CRUD genérico)

📍 `src/EcommerceNet.Data/Repositorios/RepositorioBase.cs:45`
```csharp
await _dbSet.AddAsync(entidad);
```
**Qué verificar:** la entidad en estado `Added` — EF Core la trackea pero NO ejecuta INSERT hasta que se llame `SaveChanges`. Inspeccionando `_contexto.Entry(entidad).State` se ve `EntityState.Added`.

📍 `src/EcommerceNet.Data/Repositorios/RepositorioBase.cs:50`
```csharp
_dbSet.Update(entidad);
```
**Qué verificar:** la entidad en estado `Modified` — EF Core generará un UPDATE con TODOS los campos (no solo los cambiados). Si se quiere UPDATE parcial, usar `_dbSet.Attach()` + marcar propiedades individualmente.

📍 `src/EcommerceNet.Data/Repositorios/RepositorioBase.cs:54`
```csharp
_dbSet.Remove(entidad);
```
**Qué verificar:** la entidad en estado `Deleted` — EF Core generará un DELETE en el próximo `SaveChanges`. Si hay relaciones con `Restrict`, lanzará una excepción de integridad referencial.

### 2.5 Backend — UnidadDeTrabajo (lazy init + transacción)

📍 `src/EcommerceNet.Data/UnidadDeTrabajo.cs:34`
```csharp
public IProductoRepositorio Productos =>
    _productos ??= new ProductoRepositorio(_contexto);
```
**Qué verificar:** `_productos` — la primera vez será `null` y se creará. Las siguientes veces retornará la instancia existente. Todos los repositorios comparten el mismo `_contexto` (mismo AppDbContext, misma transacción).

📍 `src/EcommerceNet.Data/UnidadDeTrabajo.cs:54`
```csharp
return await _contexto.SaveChangesAsync();
```
**Qué verificar:** el número retornado (filas afectadas). Si es `0`, nada se guardó. Si lanza `DbUpdateException`, hay un error de constraint en la BD.

---

## Flujo 3: Compra completa — el flujo más importante

> Traza: usuario agrega producto → ve carrito → hace checkout → orden creada → stock reducido

### 3.1 Frontend — Agregar al carrito (TiendaView)

📍 `src/EcommerceNet.Web/src/views/TiendaView.vue:21`
```js
async function agregarAlCarrito(productoId) {
```
**Qué verificar:** `auth.estaLogueado` en línea 22 — si es `false`, se muestra un `alert` y no continúa. El `productoId` viene del evento `@agregar` emitido por `ProductoCard.vue`.

📍 `src/EcommerceNet.Web/src/views/TiendaView.vue:26`
```js
await carrito.agregarProducto(productoId)
```
**Qué verificar:** `productoId` — ¿es el ID correcto del producto? Aquí se delega al carritoStore.

📍 `src/EcommerceNet.Web/src/stores/carritoStore.js:35`
```js
const { data } = await api.post('/carrito/agregar', { productoId, cantidad })
```
**Qué verificar:** el body enviado `{ productoId, cantidad }`. La cantidad por defecto es `1`. El interceptor de api.js agrega el Bearer token automáticamente.

📍 `src/EcommerceNet.Web/src/stores/carritoStore.js:37`
```js
items.value = data.datos.items || []
```
**Qué verificar:** `data.datos.items` — el array actualizado del carrito. `data.datos.total` — el precio total. `data.mensaje` — el mensaje de confirmación.

### 3.2 Frontend — CarritoView (ver y modificar carrito)

📍 `src/EcommerceNet.Web/src/views/CarritoView.vue:13`
```js
onMounted(() => carrito.cargarCarrito())
```
**Qué verificar:** ¿el carrito se carga al entrar a la vista? `carrito.items` debería poblarse con los items del backend. Si está vacío, `carrito.estaVacio` es `true`.

📍 `src/EcommerceNet.Web/src/views/CarritoView.vue:48`
```js
@click="carrito.actualizarCantidad(item.productoId, item.cantidad - 1)"
```
**Qué verificar (en el store, carritoStore.js:51):** si `cantidad <= 0`, llama `eliminarProducto()` en vez de actualizar — esto es intencional para quitar el item cuando llega a cero.

### 3.3 Frontend — Checkout (el paso más crítico)

📍 `src/EcommerceNet.Web/src/views/CheckoutView.vue:18`
```js
async function confirmarCompra() {
```
**Qué verificar:** `direccion.value.trim()` en línea 19 — ¿está vacío? Si sí, `errorCheckout` se muestra y no continúa.

📍 `src/EcommerceNet.Web/src/views/CheckoutView.vue:27`
```js
const resultado = await carrito.checkout(direccion.value)
```
**Qué verificar:** `carrito.items` antes de la llamada — ¿tiene items? El backend validará el stock de cada uno.

📍 `src/EcommerceNet.Web/src/stores/carritoStore.js:89`
```js
const { data } = await api.post('/carrito/checkout', { direccionEnvio })
```
**Qué verificar:** el body enviado `{ direccionEnvio }`. Si la respuesta es exitosa, línea 91 vacía el carrito local (`items.value = []`).

📍 `src/EcommerceNet.Web/src/views/CheckoutView.vue:29`
```js
if (resultado.exito) {
  ordenCreada.value = resultado.orden
```
**Qué verificar:** `resultado.orden.numeroOrden` (ej: `ORD-20260410-0001`), `resultado.orden.total` — son los datos de la orden recién creada.

### 3.4 Frontend — MisOrdenes (historial de compras)

📍 `src/EcommerceNet.Web/src/views/MisOrdenesView.vue:22`
```js
onMounted(async () => {
```
**Qué verificar:** la llamada `api.get('/ordenes')` en línea 24 — `data.datos` debe ser el array de órdenes del usuario autenticado.

📍 `src/EcommerceNet.Web/src/views/MisOrdenesView.vue:33`
```js
async function cancelarOrden(id) {
```
**Qué verificar:** el `confirm()` en línea 34 — si el usuario cancela el diálogo, la función retorna inmediatamente.

📍 `src/EcommerceNet.Web/src/views/MisOrdenesView.vue:37`
```js
const { data } = await api.put(`/ordenes/${id}/cancelar`)
```
**Qué verificar:** la respuesta — si es exitosa, línea 39 actualiza el estado localmente (`orden.estado = 'Cancelada'`) sin recargar toda la lista.

### 3.5 Backend — CarritoController (extracción del JWT)

📍 `src/EcommerceNet.API/Controllers/CarritoController.cs:32`
```csharp
private int ObtenerUsuarioId()
```
**Qué verificar:** `User.Claims` — la colección completa de claims del token JWT. ASP.NET Core los carga automáticamente después de `UseAuthentication()` en Program.cs.

📍 `src/EcommerceNet.API/Controllers/CarritoController.cs:34`
```csharp
var claim = User.FindFirst(ClaimTypes.NameIdentifier)
    ?? User.FindFirst("sub");
```
**Qué verificar:** `claim.Value` — debe ser el ID del usuario como string (ej: `"1"`). Si `claim` es `null`, significa que el token no tiene el claim `NameIdentifier` — lanzará `UnauthorizedAccessException` en línea 38.

📍 `src/EcommerceNet.API/Controllers/CarritoController.cs:54`
```csharp
public async Task<IActionResult> Agregar([FromBody] AgregarAlCarritoDto dto)
```
**Qué verificar:** `dto.ProductoId`, `dto.Cantidad` — binding automático del JSON.

📍 `src/EcommerceNet.API/Controllers/CarritoController.cs:57`
```csharp
var resultado = await _carritoServicio.AgregarProductoAsync(usuarioId, dto);
```
**Qué verificar:** `usuarioId` — ¿es el ID correcto? Toda la lógica de negocio está en el servicio, no en el controlador.

📍 `src/EcommerceNet.API/Controllers/CarritoController.cs:110`
```csharp
public async Task<IActionResult> Checkout([FromBody] CrearOrdenDto dto)
```
**Qué verificar:** `dto.DireccionEnvio` — el campo obligatorio. El servicio también lo valida.

### 3.6 Backend — CarritoServicio (LÓGICA MÁS IMPORTANTE — breakpoints críticos)

📍 `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:31`
```csharp
var producto = await _uow.Productos.ObtenerPorIdAsync(dto.ProductoId);
```
**Qué verificar:** `producto` — si es `null`, el producto no existe. `producto.Stock` — stock disponible antes de agregar.

📍 `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:34`
```csharp
if (!producto.TieneStockSuficiente(dto.Cantidad))
```
**Qué verificar:** `dto.Cantidad` vs `producto.Stock` — ¿hay suficiente stock? `producto.Activo` — ¿está activo el producto?

📍 `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:37`
```csharp
var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
```
**Qué verificar:** `carrito` — si es `null`, es la primera vez que el usuario agrega algo. La línea 38 (`bool esNuevo = carrito == null`) maneja este caso.

📍 `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:46`
```csharp
carrito.AgregarProducto(producto, dto.Cantidad);
```
**Qué verificar:** `carrito.Items.Count` antes y después. Si el producto ya estaba en el carrito, la cantidad se incrementa (no se duplica el item).

#### CHECKOUT — los 8 pasos críticos

📍 **Paso 1** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:123`
```csharp
var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
```
**Qué verificar:** `carrito?.Items.Count` — cuántos items tiene. `carrito == null` significaría que el usuario nunca agregó nada.

📍 **Paso 2** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:124`
```csharp
if (carrito == null || carrito.EstaVacio())
```
**Qué verificar:** `carrito.EstaVacio()` llama a `Items.Count == 0`. Si el frontend envió checkout con carrito vacío, aquí se detiene.

📍 **Paso 3** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:129`
```csharp
foreach (var item in carrito.Items)
{
    var prod = await _uow.Productos.ObtenerPorIdAsync(item.ProductoId);
    if (prod == null || !prod.TieneStockSuficiente(item.Cantidad))
        errores.Add($"'{item.Producto?.Nombre}': stock insuficiente");
}
```
**Qué verificar:** `prod.Stock` vs `item.Cantidad` — puede haber cambiado desde que se agregó al carrito. Si hay errores, línea 135 retorna `ErrorValidacion` con la lista completa.

📍 **Paso 4** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:139`
```csharp
var orden = new Orden
{
    UsuarioId = usuarioId,
    DireccionEnvio = dto.DireccionEnvio,
    Estado = EstadoOrden.Pendiente
};
```
**Qué verificar:** `orden.Id` será `0` hasta que se llame `SaveChanges`. `orden.Estado` = `Pendiente` (valor inicial del enum).

📍 **Paso 5** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:158`
```csharp
detalle.CalcularSubtotal();
orden.Detalles.Add(detalle);
```
**Qué verificar:** `detalle.PrecioUnitario` — precio al momento de la compra (puede diferir del precio actual del producto). `detalle.Subtotal = PrecioUnitario * Cantidad`.

📍 **Paso 6 — EL MÁS CRÍTICO** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:161`
```csharp
prod.ReducirStock(item.Cantidad);
_uow.Productos.Actualizar(prod);
```
**Qué verificar:** `prod.Stock` ANTES de esta línea (ej: `15`) y DESPUÉS (ej: `12` si se compraron 3). Si el stock es insuficiente aquí, `ReducirStock()` lanza `InvalidOperationException`. El `_uow.Productos.Actualizar(prod)` marca el producto como modificado en EF Core.

📍 **Paso 7 — TRANSACCIÓN PRINCIPAL** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:171`
```csharp
await _uow.GuardarCambiosAsync();
```
**Qué verificar:** el valor retornado — número de filas afectadas (orden + detalles + productos + carrito = varios registros). Si lanza excepción, NINGÚN cambio se guarda (atomicidad). Este es el punto de no retorno de la compra.

📍 **Paso 8** — `src/EcommerceNet.Core/Servicios/CarritoServicio.cs:173`
```csharp
orden.GenerarNumeroOrden();
_uow.Ordenes.Actualizar(orden);
await _uow.GuardarCambiosAsync();
```
**Qué verificar:** `orden.NumeroOrden` después de la generación (formato: `ORD-20260410-0001`). Se necesita el `Id` de la orden (asignado en el `SaveChanges` anterior) para generar el número.

### 3.7 Backend — Entidades (lógica de dominio)

📍 `src/EcommerceNet.Core/Entidades/Producto.cs:32`
```csharp
public void ReducirStock(int cantidad)
```
**Qué verificar:** `Stock` antes de reducir, `cantidad` solicitada. La línea 34 lanza excepción si `!TieneStockSuficiente(cantidad)`.

📍 `src/EcommerceNet.Core/Entidades/Producto.cs:35`
```csharp
Stock -= cantidad;
```
**Qué verificar:** `Stock` después de la reducción — debe ser `Stock_antes - cantidad`. Nunca debe quedar negativo.

📍 `src/EcommerceNet.Core/Entidades/Carrito.cs:37`
```csharp
var existente = Items.FirstOrDefault(i => i.ProductoId == producto.Id);
```
**Qué verificar:** `existente` — si no es `null`, el producto ya está en el carrito y se incrementa la cantidad (línea 41). Si es `null`, se crea un nuevo `CarritoItem` (línea 45).

📍 `src/EcommerceNet.Core/Entidades/Carrito.cs:19`
```csharp
public decimal CalcularTotal() => Items.Sum(i => i.CalcularSubtotal());
```
**Qué verificar:** `Items` — que todos los items tengan `PrecioUnitario` y `Cantidad` correctos. Verificar que suma todos los subtotales.

📍 `src/EcommerceNet.Core/Entidades/Orden.cs:38`
```csharp
public void Cancelar()
```
**Qué verificar:** `Estado` actual de la orden — solo se puede cancelar si es `Pendiente` o `Pagada`. La línea 40 lanza excepción si está `Enviada`, `Entregada` o `Cancelada`.

📍 `src/EcommerceNet.Core/Entidades/Orden.cs:45`
```csharp
foreach (var detalle in Detalles)
    detalle.Producto?.AumentarStock(detalle.Cantidad);
```
**Qué verificar:** `detalle.Producto` — puede ser `null` si el producto fue eliminado de la BD (soft delete no implementado en productos). El operador `?.` evita NullReferenceException.

📍 `src/EcommerceNet.Core/Entidades/OrdenDetalle.cs:20`
```csharp
Subtotal = PrecioUnitario * Cantidad;
```
**Qué verificar:** `PrecioUnitario * Cantidad` — es el cálculo de negocio más simple del proyecto. El `PrecioUnitario` se captura al momento de la compra, no el precio actual del producto.

### 3.8 Backend — Repositorios (queries con Include + ThenInclude)

📍 `src/EcommerceNet.Data/Repositorios/CarritoRepositorio.cs:36`
```csharp
return await _contexto.Carritos
    .Include(c => c.Items)
        .ThenInclude(i => i.Producto)
            .ThenInclude(p => p!.Categoria)
    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
```
**Qué verificar:** el carrito retornado — `carrito.Items[0].Producto.Nombre` debe estar disponible (no null). Sin este Include+ThenInclude anidado, `Producto` sería null y el checkout fallaría con NullReferenceException.

📍 `src/EcommerceNet.Data/Repositorios/OrdenRepositorio.cs:30`
```csharp
return await _contexto.Ordenes
    .Include(o => o.Detalles)
        .ThenInclude(d => d.Producto)
    .Include(o => o.Usuario)
    .FirstOrDefaultAsync(o => o.Id == ordenId);
```
**Qué verificar:** `orden.Detalles.Count` — cuántos productos tiene la orden. `orden.Detalles[0].Producto.Nombre` — nombre del producto en cada línea. Se usa para la pantalla de detalle y para la cancelación (necesita acceder a `Producto` para devolver el stock).

---

## Flujo Bonus: Búsqueda con MongoDB

> Traza: usuario busca → SQL devuelve resultados → búsqueda se registra en MongoDB en background

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:56`
```csharp
public async Task<IActionResult> Buscar([FromQuery] string termino)
```
**Qué verificar:** `termino` — el parámetro de query string (ej: `/api/productos/buscar?termino=laptop`).

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:62`
```csharp
var productos = await _uow.Productos.BuscarPorNombreAsync(termino);
var lista = productos.ToList();
```
**Qué verificar:** `lista.Count` — cuántos productos se encontraron. Esta es la query a SQL Server.

📍 `src/EcommerceNet.API/Controllers/ProductosController.cs:69`
```csharp
_ = _historial.RegistrarBusquedaAsync(termino, usuarioId, lista.Count);
```
**Qué verificar:** el `_ =` significa que el resultado (Task) se descarta — es el patrón "fire-and-forget". Si MongoDB falla, la respuesta al usuario no se ve afectada. `usuarioId` puede ser `null` si el usuario no está autenticado.

📍 `src/EcommerceNet.Data/MongoDB/HistorialBusquedaServicio.cs:40`
```csharp
var busqueda = new BusquedaHistorial
{
    Termino = termino.ToLower().Trim(),
    UsuarioId = usuarioId,
    ResultadosEncontrados = resultados,
    Fecha = DateTime.UtcNow
};
```
**Qué verificar:** `busqueda.Termino` — normalizado a minúsculas para agrupar correctamente (`"Laptop"` y `"laptop"` cuentan como lo mismo).

📍 `src/EcommerceNet.Data/MongoDB/HistorialBusquedaServicio.cs:48`
```csharp
await _coleccion.InsertOneAsync(busqueda);
```
**Qué verificar:** `busqueda.Id` después de la inserción — MongoDB asigna un `ObjectId` automáticamente (ej: `"507f1f77bcf86cd799439011"`). Si MongoDB no está disponible, esta línea lanzará excepción (atrapada silenciosamente por el fire-and-forget).

📍 `src/EcommerceNet.Data/MongoDB/HistorialBusquedaServicio.cs:63`
```csharp
return await _coleccion.Aggregate()
    .Group(b => b.Termino, g => new TerminoPopular
    {
        Termino = g.Key,
        TotalBusquedas = g.Count()
    })
    .SortByDescending(t => t.TotalBusquedas)
    .Limit(top)
    .ToListAsync();
```
**Qué verificar:** el pipeline de agregación — equivale a `GROUP BY Termino ORDER BY COUNT(*) DESC LIMIT 10`. `top` por defecto es `10` (parámetro del método en línea 62).

---

## Referencia rápida — mapa de archivos

| Archivo | Responsabilidad |
|---------|----------------|
| `RegistroView.vue` | Formulario de registro → llama `authStore.registrar()` |
| `LoginView.vue` | Formulario de login → llama `authStore.login()` |
| `authStore.js` | Estado de autenticación, JWT en localStorage |
| `api.js` | Instancia de Axios con interceptores de token y error 401 |
| `TiendaView.vue` | Catálogo público, llama `carrito.agregarProducto()` |
| `CarritoView.vue` | Vista del carrito, actualizar/eliminar items |
| `CheckoutView.vue` | Formulario de dirección → llama `carrito.checkout()` |
| `MisOrdenesView.vue` | Historial y cancelación de órdenes |
| `AdminView.vue` | CRUD de productos y categorías (requiere rol Admin) |
| `carritoStore.js` | Operaciones del carrito, checkout |
| `productoStore.js` | Lista de productos, filtros locales |
| `AuthController.cs` | Endpoints `/auth/registrar` y `/auth/login` |
| `ProductosController.cs` | CRUD de productos, búsqueda con MongoDB |
| `CarritoController.cs` | Endpoints del carrito, extrae ID del JWT |
| `OrdenesController.cs` | Historial y cancelación de órdenes |
| `CategoriasController.cs` | CRUD de categorías, soft delete |
| `AuthServicio.cs` | BCrypt hash/verify, generación del JWT |
| `CarritoServicio.cs` | **LÓGICA PRINCIPAL:** agregar, checkout (reduce stock, crea orden, vacía carrito) |
| `RepositorioBase.cs` | CRUD genérico con EF Core |
| `ProductoRepositorio.cs` | Queries especializados con Include(Categoria) |
| `CarritoRepositorio.cs` | ObtenerPorUsuario con Include+ThenInclude anidado |
| `OrdenRepositorio.cs` | ObtenerConDetalles con Include(Detalles→Producto) |
| `UnidadDeTrabajo.cs` | Agrupa repositorios, controla SaveChanges (transacción) |
| `AppDbContext.cs` | Configuración Fluent API, seed data, OnModelCreating |
| `ManejadorErroresMiddleware.cs` | Atrapa todas las excepciones, devuelve JSON estandarizado |
| `HistorialBusquedaServicio.cs` | MongoDB: registrar búsquedas, pipeline de agregación |
| `Producto.cs` | `ReducirStock()`, `AumentarStock()`, `TieneStockSuficiente()` |
| `Carrito.cs` | `AgregarProducto()`, `CalcularTotal()`, `Vaciar()` |
| `Orden.cs` | `Cancelar()` (devuelve stock), `GenerarNumeroOrden()` |
| `OrdenDetalle.cs` | `CalcularSubtotal()` = `PrecioUnitario * Cantidad` |
