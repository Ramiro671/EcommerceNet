# Guía de Debug Local — EcommerceNet

## Paso 1: Arrancar la API en modo debug

1. Abrir VS Code en la carpeta `C:\Users\ramir\Source\repos\EcommerceNet`
2. Presionar **Ctrl+Shift+D** (panel Run and Debug)
3. En el dropdown seleccionar **Debug API (.NET)**
4. Presionar **F5**
5. Esperar a que en la terminal aparezca: `Now listening on: http://localhost:5152`
6. La barra inferior de VS Code cambia a **naranja** = debug activo

## Paso 2: Verificar que la API responde

IMPORTANTE: La API NO tiene página en la raíz (/). Estos son los URLs correctos:

| URL | Qué muestra |
|-----|------------|
| http://localhost:5152/swagger | Documentación interactiva de la API |
| http://localhost:5152/api/productos | Lista de productos en JSON |
| http://localhost:5152/api/auth/login | Endpoint de login (solo POST) |

Abrir en el navegador: **http://localhost:5152/swagger**
Si ves la interfaz de Swagger, la API funciona correctamente.

El error "404 No se encuentra esta página" en http://localhost:5152/ es NORMAL — la API no sirve HTML en la raíz.

## Paso 3: Arrancar el frontend Vue.js

Abrir una SEGUNDA terminal en VS Code (Ctrl+Shift+`):
```bash
cd src/EcommerceNet.Web
npm install
npm run dev
```

Esperar a que muestre: `Local: http://localhost:5173/`
Abrir en el navegador: **http://localhost:5173**

IMPORTANTE: Verificar que en `src/EcommerceNet.Web/src/services/api.js` el baseURL apunte al puerto correcto de la API:
```javascript
baseURL: 'http://localhost:5152/api'
```

Si el puerto en api.js no coincide con el puerto real de la API, cambiar api.js al puerto correcto.

## Paso 4: Verificar CORS

Si el frontend muestra errores de CORS en la consola del navegador (F12 → Console), verificar que Program.cs tenga el origen del frontend:

```csharp
politica.WithOrigins("http://localhost:5173")
```

Ya está configurado. Si se cambia el puerto del frontend, actualizar Program.cs y reiniciar la API (Ctrl+Shift+F5).

## Paso 5: Probar flujo con breakpoints

### Flujo A — Login (el más simple para empezar)

1. En VS Code, abrir `src/EcommerceNet.API/Controllers/AuthController.cs`
2. Buscar el método `Login` y poner breakpoint en la primera línea (F9)
3. Abrir `src/EcommerceNet.Data/Servicios/AuthServicio.cs`
4. Poner breakpoint en la primera línea de `LoginAsync`
5. En el navegador (http://localhost:5173), ir a **Iniciar sesión**
6. Escribir: email: `admin@ecommercenet.com`, password: `Admin123!`
7. Click en Iniciar sesión
8. VS Code se PAUSA en el breakpoint del AuthController
9. Inspeccionar: pasar mouse sobre `dto` → ver dto.Email y dto.Password
10. Presionar **F11** (Step Into) → entras a AuthServicio.LoginAsync
11. Presionar **F10** (Step Over) línea por línea → ver cómo busca el usuario y verifica password
12. Presionar **F5** (Continue) → el login completa y el frontend recibe el JWT

### Flujo B — Ver catálogo de productos

1. Poner breakpoint en `ProductosController.ObtenerTodos()`
2. Poner breakpoint en `ProductoRepositorio.ObtenerActivosAsync()`
3. En el navegador, ir a la página principal (http://localhost:5173)
4. VS Code se pausa en ProductosController
5. F11 → entras al repositorio
6. Inspeccionar: productos (lista), verificar que Include cargó Categoria
7. F5 → los productos aparecen en el frontend

### Flujo C — Agregar al carrito (requiere estar logueado)

1. Primero hacer login (Flujo A sin breakpoints — solo F5 para continuar rápido)
2. Poner breakpoints en:
   - `CarritoController.Agregar()` — donde extrae el userId del JWT
   - `CarritoServicio.AgregarProductoAsync()` — donde busca el producto
   - `Carrito.AgregarProducto()` — la lógica de dominio
3. En el frontend, click en "Agregar al carrito" en cualquier producto
4. VS Code se pausa → inspeccionar paso a paso:
   - ¿userId se extrajo correctamente del JWT?
   - ¿El producto existe y tiene stock?
   - ¿El carrito ya tenía este producto? (incrementa cantidad vs crea nuevo item)

### Flujo D — Checkout completo (el más complejo)

1. Tener productos en el carrito (Flujo C)
2. Poner breakpoints en:
   - `CarritoController.Checkout()`
   - `CarritoServicio.CheckoutAsync()` — CADA paso interno:
     - a) Obtener carrito
     - b) Verificar que no esté vacío
     - c) Crear Orden
     - d) Loop: crear OrdenDetalle + ReducirStock
     - e) Vaciar carrito
     - f) GuardarCambiosAsync
3. Ir al carrito → Proceder al pago → Ingresar dirección → Confirmar
4. VS Code se pausa → seguir paso a paso:
   - En el loop de items: inspeccionar producto.Stock ANTES y DESPUÉS de ReducirStock
   - En GuardarCambiosAsync: este es el momento donde TODO se guarda en BD — antes de F10 nada está guardado, después ya sí
5. F5 → el frontend muestra "¡Compra realizada!"

### Flujo E — Probar errores (breakpoint en el middleware)

1. Poner breakpoint en `ManejadorErroresMiddleware` en el `catch`
2. En `.vscode/requests.http` o en Swagger, enviar request sin token a endpoint protegido:
   `POST http://localhost:5152/api/carrito/agregar` (sin header Authorization)
3. Esperar 401 → el middleware lo maneja
4. Enviar datos inválidos: POST http://localhost:5152/api/productos con precio negativo
5. Esperar 400 → ver cómo el controlador valida

## Paso 6: Probar con requests.http (sin frontend)

Si prefieres probar solo la API sin el frontend:

1. Abrir `.vscode/requests.http` en VS Code
2. La extensión REST Client muestra "Send Request" arriba de cada petición
3. Ejecutar en orden:
   - Primero: POST /auth/login → copiar el token de la respuesta
   - Pegar el token en la variable `@token` del archivo
   - Luego: los demás requests ya usan ese token automáticamente

## Resumen de teclas para debug

| Tecla | Qué hace |
|-------|----------|
| F5 | Iniciar debug / Continuar al siguiente breakpoint |
| F9 | Poner/quitar breakpoint |
| F10 | Step Over (ejecutar línea, no entrar a funciones) |
| F11 | Step Into (entrar dentro de la función) |
| Shift+F11 | Step Out (salir de la función actual) |
| Ctrl+Shift+F5 | Reiniciar debug |
| Shift+F5 | Detener debug |

## Troubleshooting

| Problema | Solución |
|----------|---------|
| 404 en localhost:5152/ | Normal — usar /swagger o /api/productos |
| CORS error en consola | Verificar WithOrigins en Program.cs incluye http://localhost:5173 |
| Breakpoint no se activa (círculo hueco) | Hacer dotnet build primero, luego F5 |
| "Port already in use" | Cerrar la terminal anterior o matar proceso |
| Frontend no carga productos | Verificar baseURL en api.js apunta a localhost:5152 |
| Login falla con 401 | Verificar que la BD tiene el usuario admin (seed data) |
| "Failed to determine https port" | Warning normal — la API corre en HTTP en desarrollo |
