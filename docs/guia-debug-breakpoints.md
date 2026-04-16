# Guía de Debugging — EcommerceNet

## Clase: Cómo usar VS Code como desarrollador profesional

### Teclas rápidas esenciales para debugging

| Tecla | Qué hace | Cuándo usarla |
|-------|----------|---------------|
| F5 | Iniciar debugging (o continuar hasta el siguiente breakpoint) | Para arrancar en modo debug o continuar |
| Shift+F5 | Detener debugging | Para parar la ejecución |
| Ctrl+Shift+F5 | Reiniciar debugging | Cuando cambias código y quieres re-lanzar |
| F9 | Poner/quitar breakpoint en la línea actual | Para marcar dónde quieres que se pause |
| F10 | Step Over — ejecutar la línea y pasar a la siguiente | Cuando NO quieres entrar dentro de una función |
| F11 | Step Into — entrar dentro de la función en la línea actual | Cuando quieres ver qué pasa DENTRO de un método |
| Shift+F11 | Step Out — salir de la función actual | Cuando ya viste lo que necesitabas dentro del método |
| F12 | Go to Definition | Para navegar a la definición de una clase/método |
| Alt+F12 | Peek Definition | Ver definición sin cambiar de archivo |
| Ctrl+Shift+P | Command Palette | Para todo: cambiar tema, ejecutar tareas, buscar |
| Ctrl+P | Quick Open | Abrir cualquier archivo por nombre |
| Ctrl+Shift+F | Buscar en todo el proyecto | Para encontrar dónde se usa un método o variable |
| Ctrl+` | Abrir/cerrar terminal integrada | Para ejecutar comandos sin salir de VS Code |
| Ctrl+B | Mostrar/ocultar barra lateral | Para tener más espacio de código |
| Ctrl+J | Mostrar/ocultar panel inferior | Para ver logs y errores |
| Ctrl+Shift+E | Explorador de archivos | Para navegar la estructura del proyecto |
| Ctrl+Shift+D | Panel de Debug (Run and Debug) | Para ver breakpoints, variables, call stack |
| Ctrl+Shift+M | Panel de Problems | Para ver errores de compilación |
| Ctrl+K Ctrl+C | Comentar línea(s) seleccionada(s) | Para desactivar código temporalmente |
| Ctrl+K Ctrl+U | Descomentar línea(s) | Para reactivar código |
| Ctrl+D | Seleccionar siguiente ocurrencia de la palabra | Para renombrar rápido |
| Ctrl+Shift+L | Seleccionar TODAS las ocurrencias | Para cambiar todas a la vez |
| Alt+↑ / Alt+↓ | Mover línea arriba/abajo | Para reorganizar código |
| Ctrl+Shift+K | Eliminar línea completa | Más rápido que seleccionar + delete |

---

## Configuración inicial

### Extensiones necesarias
Abrir VS Code → Ctrl+Shift+P → "Show Recommended Extensions" (ya configuradas en `.vscode/extensions.json`):

| Extensión | Para qué |
|-----------|---------|
| `ms-dotnettools.csharp` | IntelliSense y debug para C# |
| `ms-dotnettools.csdevkit` | Kit completo .NET en VS Code |
| `Vue.volar` | Soporte para Vue.js 3 |
| `esbenp.prettier-vscode` | Formateo de JS/JSON |
| `humao.rest-client` | Ejecutar requests HTTP desde `.http` |
| `ms-azuretools.vscode-docker` | Docker |
| `mongodb.mongodb-vscode` | Explorar colecciones MongoDB |

### Cómo abrir el proyecto
- VS Code → **File → Open Folder** → `C:\Users\ramir\Source\repos\EcommerceNet`
- Esperar a que cargue (barra inferior muestra progreso de OmniSharp)

---

## Cómo ejecutar en modo debug

### Solo backend (API)
1. **Ctrl+Shift+D** para abrir el panel Run and Debug
2. Seleccionar **"Debug API (.NET)"** en el dropdown
3. **F5** — esperar `Now listening on http://localhost:5152`
4. La barra inferior cambia a **NARANJA** = estás en modo debug
5. Abrir `.vscode/requests.http` y enviar peticiones con "Send Request"

### Solo frontend (Vue.js)
1. Abrir terminal: **Ctrl+`**
2. `cd src/EcommerceNet.Web && npm run dev`
3. Abrir Chrome → `http://localhost:5173`
4. **F12** → DevTools → **Sources** → buscar archivos `.vue`

### Fullstack (ambos)
1. Seleccionar **"Fullstack (API + Frontend)"** en el dropdown
2. **F5** — lanza API + Chrome simultáneamente

### Swagger (alternativa a requests.http)
- Con la API corriendo → abrir `http://localhost:5152/swagger`
- Botón "Authorize" → pegar el token JWT → probar endpoints directamente

---

## Paso a paso: Tu primer debug completo

1. **F5** → seleccionar "Debug API (.NET)"
2. Abrir `AuthController.cs` (**Ctrl+P** → escribir "Auth")
3. Click en el margen junto a la línea con `// 🔴 BP-06` → aparece punto rojo
4. Abrir `.vscode/requests.http` → "1.3 Login" → click **"Send Request"**
5. VS Code se **pausa** — inspeccionar `dto.Email`, `dto.Password` en el panel Variables
6. **F11** para entrar a `LoginAsync` en AuthServicio
7. **F10** para avanzar línea a línea
8. **F5** para continuar hasta el final

---

## Mapa de breakpoints por flujo

### 🔐 Flujo 1: Autenticación (BP-01 a BP-10)

| # | Archivo | Línea (aprox.) | Qué inspeccionar |
|---|---------|----------------|-----------------|
| BP-01 | `ManejadorErroresMiddleware.cs` | 40 | `contexto.Request.Path`, `Method` — toda petición pasa aquí primero |
| BP-02 | `AuthController.cs` | 30 | `dto.Nombre`, `dto.Email`, `dto.Password` |
| BP-03 | `AuthServicio.cs` | 36 | `existe` (bool) — ¿el email ya está registrado? |
| BP-04 | `AuthServicio.cs` | 45 | `PasswordHash` — comprobar que es distinto al `dto.Password` |
| BP-05 | `AuthServicio.cs` | 50 | `usuario.Id` — debe ser 0 antes, >0 después de SaveChanges |
| BP-06 | `AuthController.cs` | 44 | `dto.Email`, `dto.Password` |
| BP-07 | `AuthServicio.cs` | 59 | `usuario` — null = no existe en BD |
| BP-08 | `AuthServicio.cs` | 68 | resultado de `BCrypt.Verify` (true/false) |
| BP-09 | `AuthServicio.cs` | 82 | `claims[]` — NameIdentifier, Name, Email, Role |
| BP-10 | `AuthServicio.cs` | 107 | token string (pegar en jwt.io), `expira` DateTime |

**Flujo:** `POST /api/auth/login` → BP-01 → BP-06 → BP-07 → BP-08 → BP-09 → BP-10

---

### 🛍️ Flujo 2: Admin CRUD productos (BP-11 a BP-18)

| # | Archivo | Línea (aprox.) | Qué inspeccionar |
|---|---------|----------------|-----------------|
| BP-11 | `ProductosController.cs` | 25 | `uow` — verificar que no sea null (DI funcionó) |
| BP-12 | `ProductosController.cs` | 38 | `productos.Count()`, `productos[0].Categoria` (cargada?) |
| BP-13 | `ProductoRepositorio.cs` | 71 | SQL en Output window, count de productos activos |
| BP-14 | `ProductosController.cs` | 93 | `User.Claims`, `User.IsInRole("Admin")`, `dto` |
| BP-15 | `ProductosController.cs` | 94 | `errores` list — ¿qué validaciones fallaron? |
| BP-16 | `RepositorioBase.cs` | 45 | `entidad.Id` — debe ser 0 antes del save |
| BP-17 | `UnidadDeTrabajo.cs` | 57 | retorno — número de entidades afectadas en BD |
| BP-18 | `ProductosController.cs` | 178 | `p` (entidad completa) vs campos expuestos en el DTO |

**Flujo:** `POST /api/productos` (con JWT Admin) → BP-14 → BP-15 → BP-16 → BP-17 → BP-18

---

### 🛒 Flujo 3: Compra completa (BP-19 a BP-35) — **El más importante**

> Si algo falla en BP-35 (SaveChanges), NADA de lo que ocurrió antes se guarda en BD.

| # | Archivo | Línea (aprox.) | Qué inspeccionar |
|---|---------|----------------|-----------------|
| BP-19 | `CarritoController.cs` | 33 | `claim.Value` (userId), `User.Claims` completo |
| BP-20 | `CarritoController.cs` | 56 | `dto.ProductoId`, `dto.Cantidad`, `usuarioId` |
| BP-21 | `CarritoServicio.cs` | 31 | `usuarioId`, `dto.ProductoId`, `dto.Cantidad` |
| BP-22 | `CarritoServicio.cs` | 33 | `producto` (null=no existe), `producto.Stock`, `producto.Activo` |
| BP-23 | `CarritoServicio.cs` | 39 | `carrito` (null=primera compra), `carrito?.Items.Count` |
| BP-24 | `Carrito.cs` | 31 | `producto.Nombre`, `cantidad`, `Items.Count` actual |
| BP-25 | `Carrito.cs` | 40 | `existente` (null=nuevo item, no null=incrementar cantidad) |
| BP-26 | `CarritoController.cs` | 46 | `resultado.Datos.Items`, `TotalProductos`, `Total` |
| BP-27 | `CarritoRepositorio.cs` | 36 | `carrito.Items[0].Producto.Categoria` — ¿todo cargado? |
| BP-28 | `CarritoController.cs` | 109 | `dto.DireccionEnvio`, `usuarioId` |
| BP-29 | `CarritoServicio.cs` | 125 | `carrito.Items.Count` (>0?), `carrito.CalcularTotal()` |
| BP-30 | `CarritoServicio.cs` | 141 | `orden.NumeroOrden` (vacío), `orden.Total` (0), `orden.Estado` |
| BP-31 | `CarritoServicio.cs` | 150 | `item.Producto.Nombre`, `item.Cantidad`, `item.Producto.Stock` ANTES |
| BP-32 | `Producto.cs` | 33 | `Stock` ANTES, `cantidad`, `Stock` DESPUÉS (=Stock-cantidad) |
| BP-33 | `CarritoServicio.cs` | 154 | `detalle.PrecioUnitario`, `detalle.Cantidad`, `detalle.Subtotal` |
| BP-34 | `CarritoServicio.cs` | 172 | `carrito.Items.Count` ANTES (>0) y DESPUÉS (0) |
| BP-35 | `CarritoServicio.cs` | 175 | retorno — número de entidades afectadas. Si falla → nada se guarda |

**Flujo:** `POST /api/carrito/checkout` → BP-28 → BP-29 → BP-30 → [loop: BP-31 → BP-32 → BP-33] → BP-34 → BP-35

---

### ❌ Flujo 4: Cancelación de orden (BP-36 a BP-40)

| # | Archivo | Línea (aprox.) | Qué inspeccionar |
|---|---------|----------------|-----------------|
| BP-36 | `OrdenesController.cs` | 95 | `id`, `ObtenerUsuarioId()`, `orden.Estado` |
| BP-37 | `OrdenesController.cs` | 103 | `orden.UsuarioId` vs `ObtenerUsuarioId()` — ¿coinciden? |
| BP-38 | `Orden.cs` | 41 | `Estado` ANTES, `SePuedeCancelar()` (solo Pendiente/Pagada) |
| BP-39 | `Orden.cs` | 49 | `detalle.Producto.Stock` ANTES y DESPUÉS de `AumentarStock()` |
| BP-40 | `OrdenesController.cs` | 112 | `orden.Estado` DESPUÉS (debe ser `Cancelada`) |

**Flujo:** `PUT /api/ordenes/1/cancelar` → BP-36 → BP-37 → BP-38 → BP-39 → BP-40

---

### 💥 Flujo 5: Errores y middleware (BP-41 a BP-45)

| # | Archivo | Línea (aprox.) | Qué inspeccionar |
|---|---------|----------------|-----------------|
| BP-41 | `ManejadorErroresMiddleware.cs` | 47 | `ex.GetType().Name`, `ex.Message`, `ex.StackTrace` |
| BP-42 | `ManejadorErroresMiddleware.cs` | 62 | `codigo` (401/400/404/500), `mensaje` resultante |
| BP-43 | `Producto.cs` | 34 | `Stock` actual, `cantidad` pedida — cuándo lanza excepción |
| BP-44 | `Orden.cs` | 43 | `Estado` actual — por qué no se puede cancelar |
| BP-45 | `Program.cs` | 156 | JWT inválido/expirado → 401 sin llegar al controlador |

**Para activar BP-41/42:** enviar request "8.1" (sin token) o "8.6" (carrito vacío)

---

## Variables clave para el panel Watch

Agregar en **WATCH** (Ctrl+Shift+D → sección Watch → click `+`):

```
_uow                       → verificar que no sea null (DI funcionó)
carrito.Items.Count        → cuántos productos en el carrito
producto.Stock             → antes y después de ReducirStock
orden.Total                → debe coincidir con suma de detalles
orden.Estado.ToString()    → estado legible (Pendiente/Pagada/Cancelada)
User.Claims                → verificar que el JWT se decodificó correctamente
resultado.Exito            → true/false de la operación
dto                        → ver todos los campos del DTO recibido
```

---

## Ejercicio guiado: Debug del checkout completo

1. **F5** para iniciar la API en debug
2. Activar breakpoints: BP-28, BP-29, BP-30, BP-31, BP-32, BP-35
3. Abrir `.vscode/requests.http`
4. Ejecutar **"1.3 Login"** → copiar el token de la respuesta
5. Ejecutar **"6.1 Agregar productos"**
6. Ejecutar **"6.2 Checkout"** → VS Code pausa en **BP-28**
   - Inspeccionar: `dto.DireccionEnvio`, `usuarioId`
   - **F11** para entrar a `CheckoutAsync`
7. Pausa en **BP-29** → `carrito.Items.Count`, `carrito.CalcularTotal()`
8. Pausa en **BP-30** → `orden.NumeroOrden` (vacío), `orden.Total` (0 aún)
9. Pausa en **BP-31** (loop) → `item.Producto.Stock` (ej: 15)
   - **F11** para entrar a `ReducirStock` → **BP-32**
   - Inspeccionar `Stock` = 15, `cantidad` = 2
   - **F10** → `Stock` ahora = 13 ← el stock cambió en memoria
   - **Shift+F11** para salir
10. Pausa en **BP-35** — ANTES del F10: nada está en BD aún
    - **F10** → SaveChanges ejecutado
    - Inspeccionar retorno (>0 = éxito)
11. Verificar: ejecutar **"7.1 Mis órdenes"** → la orden aparece
12. Ejecutar **"2.2 Obtener producto 1"** → `stock` se redujo

---

## Tips de debugging avanzado

### Ver el SQL que genera EF Core
En `Program.cs`, modificar el `AddDbContext`:
```csharp
opciones.UseSqlServer(connectionString)
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, LogLevel.Information)
```
Cada query aparece en la consola de Debug al ejecutar.

### Breakpoints condicionales
Click derecho en el punto rojo → **"Edit Condition"**:
```
item.Cantidad > 3
```
Solo pausa cuando la condición es verdadera — ideal para loops.

### Logpoints (sin pausar)
Click derecho en el margen → **"Add Logpoint"**:
```
Producto: {producto.Nombre}, Stock: {producto.Stock}
```
Imprime en la consola sin detener la ejecución.

### Ver un JWT decodificado
1. Copiar el token de la respuesta de login
2. Abrir `https://jwt.io` → pegar el token
3. Ver claims: `nameid` (userId), `unique_name` (nombre), `email`, `role`

### Debug de Vue.js en Chrome
1. `cd src/EcommerceNet.Web && npm run dev`
2. Chrome → `http://localhost:5173` → **F12** → Sources
3. Navegar a `webpack:///src/stores/carritoStore.js`
4. Click en número de línea → poner breakpoint

---

## Errores comunes y soluciones

| Problema | Causa | Solución |
|----------|-------|---------|
| Breakpoint aparece hueco (no activa) | Código compilado no coincide con fuente | `Ctrl+Shift+F5` (rebuild + restart) |
| "No se puede encontrar el archivo" | Entrando a código de .NET/NuGet | `Shift+F11` para salir |
| Variables muestran "optimized away" | Build en Release mode | Verificar que esté seleccionado `Debug` |
| El breakpoint se salta | La línea no se ejecuta en ese flujo | Verificar el if/else con F10 |
| No para en breakpoint del frontend | Chrome DevTools no conectó | Refresh de la página con DevTools abierto |
| "Port already in use" | Otra instancia corriendo | `Ctrl+C` en terminal anterior |
| 401 en todos los requests | Token expirado o mal copiado | Re-ejecutar "1.3 Login" y copiar nuevo token |

---

## Atajos de VS Code para productividad diaria

| Atajo | Qué hace |
|-------|----------|
| Ctrl+Space | IntelliSense — autocompletado |
| Ctrl+. | Quick Fix — sugerencias de corrección |
| F2 | Renombrar símbolo en todo el proyecto |
| Ctrl+Shift+O | Ir a símbolo en el archivo actual |
| Ctrl+T | Ir a símbolo en todo el proyecto |
| Ctrl+G | Ir a línea específica |
| Ctrl+/ | Toggle comentario |
| Ctrl+W | Cerrar tab actual |
| Ctrl+Tab | Cambiar entre tabs abiertos |
| Ctrl+\ | Dividir editor (ver 2 archivos lado a lado) |
