# Guia de Debug Local - EcommerceNet

> **El 404 en localhost:5152/ es NORMAL.** La API no tiene pagina de inicio.
> La URL correcta es: **http://localhost:5152/swagger**

---

## CONCEPTO CLAVE: Breakpoints de Startup vs Breakpoints de Request

Hay dos momentos en que un breakpoint puede activarse:

**Al arrancar la app (Startup)** — cuando presionas F5 en VS Code.
ASP.NET Core ejecuta Program.cs de arriba a abajo para construir la app.
Las lineas `app.UseMiddleware(...)`, `app.UseAuthentication()`, etc.
se ejecutan UNA SOLA VEZ mientras la app inicia. Si pones un breakpoint
en esas lineas, se activan ANTES de que llegue cualquier peticion HTTP.

Ejemplo: BP-45 en `app.UseAuthentication()` se activa al iniciar.
Presiona F5 para continuar. La app termina de iniciar y queda escuchando.

**Al recibir una peticion HTTP (Request)** — cuando el navegador o Swagger
manda una peticion. Los breakpoints en controladores, servicios y repositorios
SOLO se activan cuando llega una peticion. Puedes tener 40 breakpoints en
controladores: ninguno se activara hasta que hagas click en el navegador.

**Flujo de una peticion HTTP POST /api/auth/login:**

```
[Vue.js hace POST]
       |
       v
ManejadorErroresMiddleware.InvokeAsync()  <-- BP-01 (toda peticion pasa aqui)
       |
       v
JWT Bearer Middleware (valida token si hay uno)
       |
       v
AuthController.Login()                    <-- BP-06
       |
       v
AuthServicio.LoginAsync()                 <-- BP-07, BP-08, BP-09, BP-10
       |
       v
AppDbContext query a SQL Server
       |
       v
[Respuesta JSON con token JWT]
```

---

## INICIO RAPIDO

### 1. Arrancar la API

En VS Code:
- Presionar **Ctrl+Shift+D** (panel Run and Debug)
- En el dropdown elegir **Debug API (.NET)**
- Presionar **F5**

**SI BP-45 se activa primero:** es normal. Estas en Program.cs linea 157
(`app.UseAuthentication()`). Presiona F5 para continuar.
La app termina de iniciar cuando ves en la terminal:

```
Now listening on: http://localhost:5152
```

La barra inferior de VS Code se pone **naranja** = debug activo.

### 2. Verificar que funciona

Abrir en el navegador: **http://localhost:5152/swagger**

Si ves la interfaz de Swagger = API funcionando correctamente.

**Por que 404 en localhost:5152/ ?**
La API es solo un backend REST. No sirve paginas HTML.
El unico HTML es el frontend en Vue.js (puerto 5173).

### 3. Arrancar el frontend

Abrir una segunda terminal en VS Code (Ctrl+Shift+`):

```bash
cd src/EcommerceNet.Web
npm install
npm run dev
```

Esperar hasta ver:

```
Local: http://localhost:5173/
```

Abrir: **http://localhost:5173**

---

## MAPA COMPLETO DE BREAKPOINTS

Todos los breakpoints estan puestos en el codigo como comentarios `// BP-XX`.
Esta tabla explica que hace cada uno y por que importa.

### Middleware (ManejadorErroresMiddleware.cs)

| BP | Linea | Se activa cuando | Que inspeccionar | Por que importa |
|----|-------|-----------------|------------------|-----------------|
| BP-01 | 41 | TODA peticion HTTP | `contexto.Request.Path`, `contexto.Request.Method` | Punto de entrada de la app. Toda peticion pasa aqui primero. Puedes ver la URL y el metodo (GET/POST/etc) antes de que llegue al controlador. |
| BP-41 | 45 | Solo si hay una excepcion no manejada | `ex.GetType().Name`, `ex.Message`, `ex.StackTrace` | Si algo falla en cualquier parte del sistema, el error llega aqui. Desde aqui se genera la respuesta JSON de error en vez de que el servidor devuelva HTML. |
| BP-42 | 62 | Despues de BP-41 | `codigo` (HttpStatusCode), `mensaje` (string) | Ver como se mapea cada tipo de excepcion C# a un codigo HTTP: UnauthorizedAccessException = 401, KeyNotFoundException = 404, etc. |

### Program.cs (Startup — se activa al presionar F5)

| BP | Linea | Se activa cuando | Que inspeccionar | Por que importa |
|----|-------|-----------------|------------------|-----------------|
| BP-45 | 157 | Al iniciar la app | El orden de los Use* en Program.cs | Los middlewares se configuran en orden: error handler primero, CORS antes de auth, auth antes de authorization. El ORDEN define quien intercepta que peticion. |

### Autenticacion — AuthController.cs + AuthServicio.cs

| BP | Archivo | Se activa cuando | Que inspeccionar | Por que importa |
|----|---------|-----------------|------------------|-----------------|
| BP-06 | AuthController.cs:46 | POST /api/auth/login | `dto.Email`, `dto.Password` | El DTO llego deserializado del JSON que mando Vue.js. Ver exactamente que datos mando el cliente. |
| BP-07 | AuthServicio.cs:62 | Dentro de LoginAsync | `usuario` (null o no) | EF Core ejecuto SELECT en SQL Server. Si usuario es null = ese email no existe en BD. |
| BP-08 | AuthServicio.cs:70 | Verificacion de password | resultado del BCrypt.Verify (true/false) | BCrypt compara el password escrito contra el hash guardado en BD. true = correcto, false = incorrecto. |
| BP-09 | AuthServicio.cs:86 | Generando el JWT | `claims[]` (array) | Los claims son los datos dentro del token: Id, Nombre, Email, Rol. El cliente los lee sin consultar la BD. |
| BP-10 | AuthServicio.cs:111 | Token generado | `token` (string JWT), `expira` (DateTime) | El token tiene 3 partes separadas por puntos. Pegar en jwt.io para ver header, payload y signature. |
| BP-02 | AuthController.cs:30 | POST /api/auth/registrar | `dto.Nombre`, `dto.Email`, `dto.Password` | Igual que BP-06 pero para registro. |
| BP-03 | AuthServicio.cs:36 | Dentro de RegistrarAsync | `existe` (bool) | Si `existe` es true = ese email ya tiene cuenta. Devuelve 400. |
| BP-04 | AuthServicio.cs:46 | Hash de password | `usuario.PasswordHash` vs `dto.Password` | El hash es completamente distinto al password original. Asi se guarda en BD. NUNCA se puede revertir. |
| BP-05 | AuthServicio.cs:51 | SaveChanges de registro | `usuario.Id` antes y despues | ANTES del await: Id = 0. DESPUES: Id tiene el valor asignado por SQL Server (autoincrement). |

### Productos — ProductosController.cs + ProductoRepositorio.cs

| BP | Archivo | Se activa cuando | Que inspeccionar | Por que importa |
|----|---------|-----------------|------------------|-----------------|
| BP-12 | ProductosController.cs:39 | GET /api/productos | `productos` (IEnumerable), `.Count()` | Cuantos productos activos hay en BD. |
| BP-13 | ProductoRepositorio.cs:72 | Dentro de ObtenerActivosAsync | la expresion LINQ | EF Core traduce este LINQ a SQL con JOIN a Categorias. El Include carga la categoria en el mismo query (no N+1). |
| BP-18 | ProductosController.cs:180 | Mapeando entidad a DTO | `p` (entidad) vs resultado (DTO) | La entidad tiene todos los campos de BD. El DTO solo expone lo que el cliente necesita. PasswordHash nunca llega al DTO. |
| BP-14 | ProductosController.cs:96 | POST /api/productos (Admin) | `User.Claims`, `User.IsInRole("Admin")` | Solo llega aqui si el JWT tiene rol Admin. Ver los claims que trajo el token. |

### Carrito — CarritoController.cs + CarritoServicio.cs + CarritoRepositorio.cs

| BP | Archivo | Se activa cuando | Que inspeccionar | Por que importa |
|----|---------|-----------------|------------------|-----------------|
| BP-19 | CarritoController.cs:34 | Cualquier endpoint del carrito | `claim.Value` (el ID del usuario) | El ID del usuario NO se manda en el body. Se extrae del JWT. El claim NameIdentifier tiene el Id que se puso al generar el token en BP-09. |
| BP-20 | CarritoController.cs:58 | POST /api/carrito/agregar | `dto.ProductoId`, `dto.Cantidad`, `usuarioId` | Los datos del body mas el userId del token. |
| BP-21 | CarritoServicio.cs:31 | Inicio de AgregarProductoAsync | `usuarioId`, `dto.ProductoId` | Entrada al servicio de negocio. |
| BP-22 | CarritoServicio.cs:33 | Buscando producto | `producto` (null o no), `producto.Stock`, `producto.Activo` | Si producto es null = ese ID no existe. TieneStockSuficiente verifica que haya stock disponible. |
| BP-23 | CarritoServicio.cs:40 | Buscando carrito del usuario | `carrito` (null=primera compra), `carrito?.Items.Count` | Si es null = es la primera vez que este usuario agrega algo. Se crea un carrito nuevo. |
| BP-27 | CarritoRepositorio.cs:37 | Cargando carrito con Include | `carrito.Items[0].Producto.Categoria` | EF Core hace JOIN a 4 tablas: Carritos, CarritoItems, Productos, Categorias. Sin Include serian null. |
| BP-26 | CarritoController.cs:48 | GET /api/carrito | `resultado.Datos.Items`, `TotalProductos`, `Total` | El carrito completo con todos los productos y precios calculados. |

### Checkout — CarritoServicio.cs (flujo mas complejo)

| BP | Archivo | Se activa cuando | Que inspeccionar | Por que importa |
|----|---------|-----------------|------------------|-----------------|
| BP-28 | CarritoController.cs:112 | POST /api/carrito/checkout | `dto.DireccionEnvio`, `usuarioId` | Entrada al checkout. |
| BP-29 | CarritoServicio.cs:127 | Buscando carrito para checkout | `carrito.Items.Count`, `carrito.CalcularTotal()` | Si el carrito esta vacio o es null, se devuelve error inmediatamente. |
| BP-30 | CarritoServicio.cs:143 | Creando la orden | `orden.NumeroOrden` (vacio), `orden.Total` (0), `orden.Estado` (Pendiente) | La orden existe en C# pero NO en BD todavia. Id = 0, NumeroOrden = "". |
| BP-31 | CarritoServicio.cs:154 | Iterando items del carrito | `item.Producto.Nombre`, `item.Cantidad`, `item.Producto.Stock` | Ver el stock ANTES de reducirlo. Si stock era 5 y se compran 2, despues de esta linea sera 3. |
| BP-33 | CarritoServicio.cs:158 | Creando OrdenDetalle | `detalle.PrecioUnitario`, `detalle.Cantidad`, `detalle.Subtotal` | El detalle captura el precio EN EL MOMENTO de la compra. Historico permanente. |
| BP-34 | CarritoServicio.cs:176 | Vaciando el carrito | `carrito.Items.Count` antes (>0) y despues (0) | El carrito se vacia en memoria. Aun no esta en BD. |
| BP-35 | CarritoServicio.cs:180 | SaveChanges final | retorno (numero de entidades guardadas) | PUNTO CRITICO: hasta aqui nada esta en BD. En este await se guardan en una sola transaccion la orden, detalles, stock reducido y carrito vaciado. Si falla: NADA se guarda. |

### Unidad de Trabajo — UnidadDeTrabajo.cs

| BP | Archivo | Se activa cuando | Que inspeccionar | Por que importa |
|----|---------|-----------------|------------------|-----------------|
| BP-17 | UnidadDeTrabajo.cs:57 | Cualquier GuardarCambiosAsync | retorno (int) | EF Core traduce todos los cambios en memoria a SQL INSERT/UPDATE/DELETE en una transaccion. El retorno es cuantas filas afecto. |

---

## FLUJO COMPLETO: LOGIN (con todos los breakpoints en secuencia)

### Antes de empezar: poner los breakpoints

Abrir cada archivo y presionar F9 en la linea indicada:

1. `src/EcommerceNet.API/Middleware/ManejadorErroresMiddleware.cs` linea 41 (BP-01)
2. `src/EcommerceNet.API/Controllers/AuthController.cs` linea 46 (BP-06)
3. `src/EcommerceNet.Data/Servicios/AuthServicio.cs` linea 62 (BP-07)
4. `src/EcommerceNet.Data/Servicios/AuthServicio.cs` linea 70 (BP-08)
5. `src/EcommerceNet.Data/Servicios/AuthServicio.cs` linea 86 (BP-09)
6. `src/EcommerceNet.Data/Servicios/AuthServicio.cs` linea 111 (BP-10)

### Paso 1: Arrancar la API

Ctrl+Shift+D → seleccionar "Debug API (.NET)" → F5.

**Si se pausa en Program.cs (BP-45):** presionar F5 para continuar.
Esperar hasta ver "Now listening on: http://localhost:5152".

### Paso 2: Arrancar el frontend

Segunda terminal:

```bash
cd src/EcommerceNet.Web
npm run dev
```

Esperar: "Local: http://localhost:5173/"

### Paso 3: Ejecutar el login

Abrir http://localhost:5173 en el navegador.
Hacer click en "Iniciar sesion".
Escribir: email `admin@ecommercenet.com` / password `Admin123!`
Click en "Iniciar sesion".

**VS Code se activa automaticamente.**

### Paso 4: Recorrer los breakpoints

**BP-01 — ManejadorErroresMiddleware.InvokeAsync (linea 41)**

Toda peticion HTTP entra aqui primero.
Pasar el mouse sobre `contexto.Request`:
- `.Path` = "/api/auth/login"
- `.Method` = "POST"

Este middleware envuelve todo el sistema. Si algo falla mas adelante,
el catch lo atrapara y devolvera JSON en vez de un error HTML del servidor.
Sin este middleware, una excepcion en el controlador mostraria la pagina
de error de ASP.NET, que no es JSON y romperia el frontend.

Presionar F5 → salta a BP-06.

---

**BP-06 — AuthController.Login (linea 46)**

El DTO llego deserializado desde el JSON que mando Vue.js.
Pasar el mouse sobre `dto`:
- `.Email` = "admin@ecommercenet.com"
- `.Password` = "Admin123!"

El controlador NO hace logica de negocio. Solo recibe el DTO y lo
pasa al servicio. Esta es la separacion de responsabilidades de Clean Architecture:
el controlador traduce HTTP, el servicio implementa la logica.

Presionar F11 para entrar a LoginAsync.

---

**BP-07 — AuthServicio.LoginAsync (linea 62)**

EF Core va a ejecutar:
```sql
SELECT TOP(1) * FROM Usuarios WHERE Email = 'admin@ecommercenet.com'
```

Presionar F10 para ejecutar la linea (va a la BD y vuelve).
Pasar el mouse sobre `usuario`:
- Si `usuario` = null → ese email no existe en BD → retorna 401
- Si `usuario` tiene datos → el email existe → continuar a BP-08

---

**BP-08 — AuthServicio.LoginAsync (linea 70)**

BCrypt toma el password escrito ("Admin123!") y lo compara
contra el hash guardado en BD (empieza con "$2a$...").

Presionar F10. El resultado es un bool:
- `true` = password correcto → continua a BP-09
- `false` = password incorrecto → retorna 401

BCrypt es un hash de una sola via: no se puede revertir. Por eso
para verificar, se re-hashea el password ingresado y se comparan los hashes.

---

**BP-09 — AuthServicio.GenerarJwt (linea 86)**

Se construye el array de claims que van DENTRO del token JWT.
En el panel Variables (lado izquierdo), expandir `claims[]`:
- claims[0] = NameIdentifier = "1" (Id del admin en BD)
- claims[1] = Name = "Administrador"
- claims[2] = Email = "admin@ecommercenet.com"
- claims[3] = Role = "Admin"

Estos datos viajan dentro del token. El cliente los puede leer sin
consultar la BD. Asi el sistema es stateless: no hay sesiones en el servidor.
Cada peticion lleva su propio contexto de usuario en el token.

Presionar F10 varias veces hasta llegar a BP-10.

---

**BP-10 — AuthServicio.GenerarJwt (linea 111)**

El token JWT esta generado. En Variables ver el objeto que se va a retornar:
- `Token` = string largo con 3 partes separadas por puntos
- `Nombre` = "Administrador"
- `Rol` = "Admin"
- `Expira` = la fecha/hora en que vence

Copiar el valor de `Token` y pegarlo en https://jwt.io para decodificarlo
y ver los claims en texto plano (header, payload, signature).

Presionar F5 → la respuesta JSON llega al navegador → Vue.js guarda
el token en localStorage y redirige al inicio.

**Login completado.**

---

## FLUJO: VER PRODUCTOS (sin login)

Breakpoints recomendados:

1. `src/EcommerceNet.API/Controllers/ProductosController.cs` linea 39 (BP-12)
2. `src/EcommerceNet.Data/Repositorios/ProductoRepositorio.cs` linea 72 (BP-13)

Abrir http://localhost:5173 (la pagina principal carga productos al abrir).

**BP-12 — ProductosController.ObtenerTodos:**
La peticion llego al controlador. Todavia no hay datos cargados.
Presionar F11 para entrar al repositorio.

**BP-13 — ProductoRepositorio.ObtenerActivosAsync:**
Ver la expresion LINQ. EF Core va a generar algo como:
```sql
SELECT p.Id, p.Nombre, p.Precio, p.Stock, c.Nombre as CategoriaNombre
FROM Productos p
JOIN Categorias c ON p.CategoriaId = c.Id
WHERE p.Activo = 1
ORDER BY p.FechaCreacion DESC
```

El `Include(p => p.Categoria)` es lo que genera el JOIN.
Sin Include, cada producto tendria `Categoria = null` y el mapeo
en BP-18 devolveria "Sin categoria" para todos.

Presionar F10. Despues del await, pasar el mouse sobre el resultado.
Presionar F5 → los productos aparecen en el frontend.

---

## FLUJO: AGREGAR AL CARRITO (requiere login previo)

Primero hacer login sin breakpoints (F5 rapido en cada pausa).

Luego poner breakpoints:
1. `CarritoController.cs` linea 34 (BP-19) — extrae userId del JWT
2. `CarritoController.cs` linea 58 (BP-20) — datos del request
3. `CarritoServicio.cs` linea 40 (BP-23) — busca carrito existente

En el frontend: click en "Agregar al carrito" en cualquier producto.

**BP-19:** el `claim.Value` debe tener el Id del usuario (ej: "1").
NO viene en el body de la peticion. Vue.js lo manda en el header:
`Authorization: Bearer eyJhbGci...`
El JWT Bearer middleware lo valida y llena `User.Claims` automaticamente.

**BP-20:** ver `dto.ProductoId` y `dto.Cantidad`. El `usuarioId` ya fue
extraido del token en BP-19.

**BP-23:** `carrito` = null si es la primera vez que este usuario agrega.
Si ya habia agregado algo antes, `carrito.Items.Count` tendra items.
Si es null, el servicio crea un carrito nuevo con `new Carrito { UsuarioId = usuarioId }`.

---

## FLUJO: CHECKOUT (requiere carrito con productos)

Breakpoints clave:
1. `CarritoServicio.cs` linea 127 (BP-29) — valida carrito no vacio
2. `CarritoServicio.cs` linea 143 (BP-30) — crea orden en memoria
3. `CarritoServicio.cs` linea 180 (BP-35) — guarda TODO en BD

En el frontend: agregar productos → ir al carrito → "Proceder al pago".

**BP-29:** `carrito.Items.Count` debe ser > 0.
`carrito.CalcularTotal()` = suma de (precio * cantidad) de cada item.

**BP-30:** en este punto la orden existe en C# pero NO en la BD.
`orden.Id` = 0. `orden.Total` = 0. `orden.NumeroOrden` = "".
Todo es temporal en memoria.

**BP-35:** PUNTO CRITICO.
Antes del await — todo en memoria:
- Orden con detalles creada
- Stock de productos reducido
- Carrito vaciado

Despues del await — todo en SQL Server en una transaccion:
- Si inspeccionas la BD ahora, la orden existe con sus detalles
- El stock de los productos fue actualizado
- El carrito esta vacio

Si cualquier cosa falla en el await (constraint, timeout, etc.),
la transaccion hace rollback y NADA queda guardado. Atomicidad garantizada.

---

## TECLAS DE DEBUG

| Tecla | Accion |
|-------|--------|
| F5 | Iniciar debug / Continuar al siguiente breakpoint |
| F9 | Poner o quitar breakpoint |
| F10 | Step Over — ejecuta la linea sin entrar a la funcion |
| F11 | Step Into — entra dentro de la funcion llamada |
| Shift+F11 | Step Out — sale de la funcion y vuelve a quien la llamo |
| Ctrl+Shift+F5 | Reiniciar debug |
| Shift+F5 | Detener debug |

### Como leer variables en el panel de debug

- **Variables** (panel izquierdo): muestra todas las variables del scope actual
- **Watch**: agregar expresiones propias — ej: `carrito.Items.Count`
- **Call Stack**: muestra la pila de llamadas — de donde venimos
- **Hover**: pasar el mouse sobre cualquier variable en el codigo para ver su valor

---

## PROBLEMAS COMUNES

| Problema | Solucion |
|----------|----------|
| 404 en localhost:5152/ | Normal — usar /swagger o /api/productos |
| ERR_CONNECTION_REFUSED | La API no esta corriendo — presionar F5 en VS Code |
| BP-45 se activa al arrancar | Normal — presionar F5 para continuar el startup |
| Breakpoint no para (circulo hueco gris) | Ejecutar `dotnet build` en terminal, luego reiniciar debug |
| Breakpoint en controlador no se activa | La peticion no llego — verificar URL y metodo HTTP |
| "Port already in use" | Cerrar la terminal anterior o reiniciar VS Code |
| Login devuelve 401 | Credenciales: admin@ecommercenet.com / Admin123! |
| Carrito devuelve 401 | El token no se mando — hacer login primero |
| BP-07: usuario es null | El email no existe en BD — usar admin@ecommercenet.com |
| BP-08: Verify da false | Password incorrecto — usar Admin123! (mayuscula y !) |

---

## PUERTOS

| Que | Puerto | URL |
|-----|--------|-----|
| API (backend) | 5152 | http://localhost:5152/swagger |
| Frontend Vue.js | 5173 | http://localhost:5173 |
| jQuery legacy | 5173 | http://localhost:5173/legacy.html |
