# Guia de Debug Local - EcommerceNet

> **El 404 en localhost:5152/ es NORMAL.** La API no tiene pagina de inicio.
> La URL correcta es: **http://localhost:5152/swagger**

---

## INICIO RAPIDO

### 1. Arrancar la API

En VS Code:
- Presionar **Ctrl+Shift+D** (panel Run and Debug)
- En el dropdown elegir **Debug API (.NET)**
- Presionar **F5**

Esperar en la terminal hasta ver:

```
Now listening on: http://localhost:5152
```

La barra inferior de VS Code se pone **naranja** = debug activo.

### 2. Abrir Swagger (verificar que funciona)

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

## FLUJOS DE DEBUG CON BREAKPOINTS

### Flujo A - Login (el mas facil)

**Objetivo:** ver como el servidor recibe el email/password y devuelve un JWT.

1. Abrir `src/EcommerceNet.API/Controllers/AuthController.cs`
2. Buscar el metodo `Login` - poner breakpoint (F9) en la primera linea
3. Abrir `src/EcommerceNet.Data/Servicios/AuthServicio.cs`
4. Poner breakpoint (F9) en la primera linea de `LoginAsync`
5. En el navegador ir a http://localhost:5173 y hacer click en "Iniciar sesion"
6. Escribir: email `admin@ecommercenet.com` / password `Admin123!`
7. Click en "Iniciar sesion"

**VS Code se pausa automaticamente.**

- Pasar el mouse sobre `dto` para ver el email y password que llego
- F11 = entrar al metodo siguiente (vas a AuthServicio.LoginAsync)
- F10 = ejecutar linea por linea (ves como busca el usuario en BD y verifica el password)
- F5 = continuar hasta el proximo breakpoint (o hasta que termine)

---

### Flujo B - Ver productos

**Objetivo:** ver como el repositorio consulta la BD y devuelve los productos.

1. Abrir `src/EcommerceNet.API/Controllers/ProductosController.cs`
2. Breakpoint en el metodo `ObtenerTodos`
3. Abrir `src/EcommerceNet.Data/Repositorios/ProductoRepositorio.cs`
4. Breakpoint en `ObtenerActivosAsync`
5. Abrir http://localhost:5173 en el navegador (la pagina principal carga los productos)

**VS Code se pausa en ProductosController.**

- F11 = entrar al repositorio
- Inspeccionar la lista `productos` despues del await
- F5 = los productos aparecen en el frontend

---

### Flujo C - Agregar al carrito

**Objetivo:** ver como el JWT del usuario se valida y como se actualiza el carrito.

Primero hacer login sin breakpoints (F5 para continuar rapido en cada pausa).

Luego poner breakpoints en:
- `CarritoController.cs` -> metodo `Agregar` (primera linea)
- `CarritoServicio.cs` -> metodo `AgregarProductoAsync` (primera linea)

En el frontend: click en "Agregar al carrito" en cualquier producto.

**VS Code se pausa.**

Inspeccionar:
- `userId` = el GUID del usuario extraido del JWT (viene del token en el header)
- `productoId` = el producto que se quiere agregar
- F11 para entrar al servicio y ver si ya existe ese item en el carrito

---

### Flujo D - Checkout completo

**Objetivo:** ver la transaccion completa: crear orden, reducir stock, vaciar carrito.

Poner breakpoints en `CarritoServicio.cs` -> metodo `CheckoutAsync`, en estas lineas:
- Donde crea la `Orden`
- El `foreach` que crea cada `OrdenDetalle`
- La llamada a `ReducirStock`
- La llamada a `GuardarCambiosAsync` al final

En el frontend: agregar productos al carrito -> ir al carrito -> "Proceder al pago" -> confirmar.

**Punto clave:** hasta que no llega a `GuardarCambiosAsync`, nada esta guardado en la BD.
Despues de ese F10, ya todo existe en la base de datos.

---

### Flujo E - Probar errores (middleware)

**Objetivo:** ver como el middleware captura excepciones y devuelve JSON de error.

1. Abrir `src/EcommerceNet.API/Middleware/ManejadorErroresMiddleware.cs`
2. Breakpoint en el bloque `catch`
3. Abrir Swagger (http://localhost:5152/swagger)
4. Intentar `GET /api/carrito` sin estar autenticado -> devuelve 401
5. Intentar `POST /api/productos` con datos invalidos -> devuelve 400

---

## PROBAR LA API SIN FRONTEND (requests.http)

1. Abrir `.vscode/requests.http` en VS Code
2. La extension REST Client muestra **Send Request** encima de cada peticion
3. Ejecutar en orden:
   - `POST /auth/login` -> copiar el token de la respuesta
   - Pegar el token donde dice `@token = ...`
   - Los demas requests usan ese token automaticamente

---

## TECLAS DE DEBUG

| Tecla | Accion |
|-------|--------|
| F5 | Iniciar debug / Continuar al siguiente breakpoint |
| F9 | Poner o quitar breakpoint |
| F10 | Step Over (ejecuta la linea sin entrar a la funcion) |
| F11 | Step Into (entra dentro de la funcion) |
| Shift+F11 | Step Out (sale de la funcion actual) |
| Ctrl+Shift+F5 | Reiniciar debug |
| Shift+F5 | Detener debug |

---

## PROBLEMAS COMUNES

| Problema | Solucion |
|----------|----------|
| 404 en localhost:5152/ | Normal - usar /swagger o /api/productos |
| La pagina no carga nada (ERR_CONNECTION_REFUSED) | La API no esta corriendo - presionar F5 en VS Code |
| CORS error en consola del navegador (F12) | Program.cs ya tiene configurado localhost:5173 |
| Breakpoint no para (circulo hueco gris) | Ejecutar `dotnet build` en terminal, luego reiniciar debug |
| "Port already in use" | Cerrar la terminal anterior o reiniciar VS Code |
| Frontend no carga productos | Verificar que api.js tiene baseURL = http://localhost:5152/api |
| Login devuelve 401 | La BD tiene el admin: admin@ecommercenet.com / Admin123! |
| "Failed to determine https port" | Warning normal - ignorar, la API corre en HTTP |

---

## PUERTOS

| Que | Puerto | URL |
|-----|--------|-----|
| API (backend) | 5152 | http://localhost:5152/swagger |
| Frontend Vue.js | 5173 | http://localhost:5173 |
| jQuery legacy | 5173 | http://localhost:5173/legacy.html |
