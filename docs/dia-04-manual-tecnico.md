# Manual Técnico — Día 4: Frontend Vue.js 3 + jQuery Legacy

> **Fecha de ejecución:** 2026-04-06
> **Herramienta:** Claude Code (claude-sonnet-4-6) ejecutado dentro del VSCode Extension
> **Entorno:** Windows 11, Node.js v20.16.0, npm 10.8.1, Vite 5.4.21
> **Resultado final:** Build exitoso (103 módulos), 0 errores, SPA completa con 7 vistas, 3 stores Pinia, página jQuery legacy

---

## Índice

1. [Qué leyó Claude Code antes de escribir una sola línea](#1-qué-leyó-claude-code-antes-de-escribir-una-sola-línea)
2. [Fase 1 — Creación del proyecto Vue.js 3](#2-fase-1--creación-del-proyecto-vuejs-3)
3. [Fase 2 — Servicios y Stores de Pinia](#3-fase-2--servicios-y-stores-de-pinia)
4. [Fase 3 — Componentes reutilizables](#4-fase-3--componentes-reutilizables)
5. [Fase 4 — Router con navigation guards](#5-fase-4--router-con-navigation-guards)
6. [Fase 6 — Siete vistas de la aplicación](#6-fase-6--siete-vistas-de-la-aplicación)
7. [Fase 7 — App.vue y página jQuery legacy](#7-fase-7--appvue-y-página-jquery-legacy)
8. [Inventario completo de archivos creados](#8-inventario-completo-de-archivos-creados)
9. [Flujo de datos de extremo a extremo](#9-flujo-de-datos-de-extremo-a-extremo)
10. [Decisiones técnicas y por qué](#10-decisiones-técnicas-y-por-qué)
11. [Errores evitados y cómo](#11-errores-evitados-y-cómo)
12. [Estado del proyecto al cierre del Día 4](#12-estado-del-proyecto-al-cierre-del-día-4)
13. [Pendientes para el Día 5](#13-pendientes-para-el-día-5)

---

## 1. Qué leyó Claude Code antes de escribir una sola línea

Claude Code ejecutó cinco lecturas antes de crear cualquier archivo:

### 1.1 `CLAUDE.md` (reglas de arquitectura)

| Regla | Efecto en el código del Día 4 |
|-------|-------------------------------|
| Frontend es proyecto Node.js independiente | `EcommerceNet.Web/` no tiene referencias a los proyectos .NET |
| Composition API en todos los componentes | Todos los `.vue` usan `<script setup>` |
| Pinia para estado global | Tres stores: `authStore`, `productoStore`, `carritoStore` |
| Axios con interceptores para JWT | `services/api.js` con interceptor de request y response |
| Comentarios en español | Todos los comentarios inline en español |
| Frontend en `src/EcommerceNet.Web/` | Carpeta dentro del mono-repositorio |

### 1.2 `docs/dia-04-frontend.md` (plan del día)

Leído en 7 bloques (supera el límite de ~10.000 tokens). Se extrajeron:
- El código completo de los 16 archivos Vue.js
- La página `legacy.html` con jQuery 3.7.1
- La estructura exacta de carpetas
- Los comentarios conceptuales por Pomodoro

### 1.3 `docs/dia-03-manual-tecnico.md` (estado del proyecto tras Día 3)

Confirmó que la API ya tiene:
- 18 endpoints en 5 controladores (GET /api/productos ya retorna 12 productos con categorías)
- CORS configurado con `WithOrigins("http://localhost:5173")`
- JWT Bearer configurado con roles Admin y Cliente
- Puerto de escucha: 5152 (del `dotnet run` del Día 3)

### 1.4 `docs/dia-01-clase-programacion.md` y `docs/dia-01-manual-tecnico.md`

Leídos para replicar el formato y nivel de detalle de la documentación.

---

## 2. Fase 1 — Creación del proyecto Vue.js 3

### 2.1 Problema con `npm create vue@latest` interactivo

El CLI de `create-vue` es completamente interactivo y no acepta todos los flags `--no-ts`, `--no-jsx` en la versión instalada (3.22.2). El intento falló:

```
TypeError [ERR_PARSE_ARGS_UNKNOWN_OPTION]: Unknown option '--no-ts'
```

**Solución:** Crear el proyecto manualmente con `package.json` propio, lo cual da control total sobre las versiones exactas de las dependencias y evita que el scaffold incluya archivos innecesarios como `HelloWorld.vue`, `TheWelcome.vue`, `IconBase.vue`, etc.

### 2.2 Estructura creada manualmente

```bash
mkdir -p src/EcommerceNet.Web/src/{assets,components,views,stores,services,router}
mkdir -p src/EcommerceNet.Web/public
```

### 2.3 Dependencias instaladas

```bash
cd src/EcommerceNet.Web
npm install
```

**`package.json` creado:**

| Paquete | Versión | Rol |
|---------|---------|-----|
| `vue` | ^3.4.29 | Framework principal |
| `vue-router` | ^4.3.3 | Navegación SPA |
| `pinia` | ^2.1.7 | Estado global |
| `axios` | ^1.7.2 | Llamadas HTTP a la API |
| `vite` | ^5.3.1 | Dev server y bundler |
| `@vitejs/plugin-vue` | ^5.0.5 | Plugin para procesar `.vue` |
| `eslint` | ^8.57.0 | Linting JavaScript/Vue |
| `prettier` | ^3.2.5 | Formateo de código |

### 2.4 Configuración de Vite

`vite.config.js` con alias `@` apuntando a `./src`:

```javascript
resolve: {
  alias: {
    '@': fileURLToPath(new URL('./src', import.meta.url))
  }
}
```

Esto permite escribir `import api from '@/services/api'` en lugar de rutas relativas complejas como `../../services/api`.

---

## 3. Fase 2 — Servicios y Stores de Pinia

### 3.1 `src/services/api.js` — Instancia Axios con interceptores

El archivo más crítico del frontend. Centraliza toda la comunicación con la API:

**Interceptor de request:** Agrega el JWT automáticamente a cada petición:
```javascript
config.headers.Authorization = `Bearer ${token}`
```
Sin esto, habría que agregar el header manualmente en cada llamada.

**Interceptor de response:** Maneja globalmente el error 401 (token expirado):
```javascript
if (error.response?.status === 401) {
  localStorage.removeItem('jwt_token')
  window.location.href = '/login'
}
```

**URL base ajustada:** La API corrió en el puerto 5152 (detectado en el Día 3), no en el 5000 del plan original:
```javascript
baseURL: 'http://localhost:5152/api'
```

### 3.2 `src/stores/authStore.js`

Store de Pinia que maneja login, registro y logout:

| Ref | Tipo | Qué guarda |
|-----|------|-----------|
| `usuario` | `ref(Object\|null)` | Datos del usuario (nombre, email, rol) |
| `token` | `ref(String)` | JWT vigente |
| `cargando` | `ref(Boolean)` | Estado de la llamada async |
| `error` | `ref(String)` | Mensaje de error de la última operación |

Getters computados:
- `estaLogueado` → `!!token.value` (doble negación convierte a booleano)
- `esAdmin` → `usuario.value?.rol === 'Admin'`
- `nombreUsuario` → `usuario.value?.nombre || ''`

**Persistencia en localStorage:** Al hacer login/registro exitoso, guarda el token y datos del usuario. Al recargar la página, `ref()` se inicializa leyendo localStorage:
```javascript
const usuario = ref(JSON.parse(localStorage.getItem('usuario') || 'null'))
const token = ref(localStorage.getItem('jwt_token') || '')
```

### 3.3 `src/stores/productoStore.js`

Gestiona el catálogo de productos con filtrado reactivo local:

**Getter clave — `productosFiltrados`:** Es un `computed()` que aplica dos filtros encadenados sobre el array `productos.value`:
1. Filtro por categoría seleccionada (`p.categoriaNombre === categoriaSeleccionada.value`)
2. Filtro por término de búsqueda (incluye nombre y descripción)

**Categorías dinámicas:** Se extraen de los propios productos usando `Set` para eliminar duplicados:
```javascript
const cats = [...new Set(data.datos.map(p => p.categoriaNombre))]
```

### 3.4 `src/stores/carritoStore.js`

El store más complejo por la cantidad de operaciones:

| Acción | Endpoint | Efecto local |
|--------|----------|-------------|
| `cargarCarrito()` | GET /api/carrito | Sincroniza `items` con la BD |
| `agregarProducto()` | POST /api/carrito/agregar | Actualiza `items` con respuesta del servidor |
| `actualizarCantidad()` | PUT /api/carrito/{id} | Llama a `eliminarProducto` si cantidad ≤ 0 |
| `eliminarProducto()` | DELETE /api/carrito/{id} | Actualiza `items` |
| `vaciar()` | DELETE /api/carrito | Vacía `items` localmente |
| `checkout()` | POST /api/carrito/checkout | Vacía `items`, retorna la orden creada |

---

## 4. Fase 3 — Componentes reutilizables

### 4.1 `NavBar.vue`

Barra de navegación sticky con lógica condicional basada en el estado de auth:

```
v-if="auth.estaLogueado"  →  Mis Órdenes | Carrito (badge) | Nombre | Salir
v-else                    →  Iniciar Sesión | Registrarse
```

**Badge del carrito:** Usa `position: absolute` para superponer el contador sobre el link de carrito. Solo visible cuando `carrito.totalItems > 0`.

### 4.2 `ProductoCard.vue`

Tarjeta de producto con props y emits:
- `defineProps`: recibe `producto: Object`
- `defineEmits`: emite `agregar` con el `producto.id`
- El padre (`TiendaView`) escucha `@agregar="agregarAlCarrito"`

**Imagen fallback:** Si `producto.imagenUrl` es null o vacío, muestra un placeholder de Placehold.co:
```javascript
:src="producto.imagenUrl || 'https://placehold.co/400x300?text=Producto'"
```

### 4.3 `CategoriaFiltro.vue`

Sidebar de filtros con estado activo resaltado:
```html
:class="['filtro-btn', seleccionada === cat ? 'activo' : '']"
```

No tiene estado interno — es un componente "tonto" que recibe props y emite eventos. El estado real vive en `productoStore`.

---

## 5. Fase 4 — Router con navigation guards

### 5.1 `router/index.js`

Siete rutas definidas:

| Ruta | Vista | `meta.requiereAuth` |
|------|-------|---------------------|
| `/` | TiendaView | No |
| `/producto/:id` | ProductoDetalleView | No |
| `/carrito` | CarritoView | **Sí** |
| `/checkout` | CheckoutView | **Sí** |
| `/mis-ordenes` | MisOrdenesView | **Sí** |
| `/login` | LoginView | No |
| `/registro` | RegistroView | No |

### 5.2 Navigation Guard

```javascript
router.beforeEach((to, from, next) => {
  const auth = useAuthStore()
  if (to.meta.requiereAuth && !auth.estaLogueado) {
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else {
    next()
  }
})
```

Si el usuario intenta navegar a `/carrito` sin login, es redirigido a `/login?redirect=/carrito`. Después del login exitoso, `LoginView` lee `route.query.redirect` y lo lleva directo a donde quería ir.

---

## 6. Fase 6 — Siete vistas de la aplicación

| Vista | Complejidad | Características principales |
|-------|------------|----------------------------|
| `TiendaView` | Alta | Grid responsivo, barra de búsqueda con v-model, filtro lateral, mensaje del carrito |
| `ProductoDetalleView` | Media | Carga el producto por ID desde la API, selector +/- de cantidad |
| `CarritoView` | Alta | Tabla editable con controles de cantidad, total calculado en tiempo real |
| `CheckoutView` | Alta | Formulario + resumen + pantalla de confirmación post-compra |
| `LoginView` | Media | Formulario con manejo de errores, redirección post-login, `@keyup.enter` |
| `RegistroView` | Media | Validación de coincidencia de contraseñas en frontend |
| `MisOrdenesView` | Media | Lista de órdenes con badges de estado por color, botón cancelar |

---

## 7. Fase 7 — App.vue y página jQuery legacy

### 7.1 `App.vue`

Layout mínimo: NavBar + RouterView. Los estilos globales incluyen la fuente Inter importada de Google Fonts.

### 7.2 `public/legacy.html`

Página standalone que demuestra jQuery 3.7.1 consumiendo la misma API REST:

| Técnica jQuery | Dónde se usa |
|---------------|-------------|
| `$(document).ready()` | Inicialización al cargar la página |
| `$.get(url).done().fail()` | Llamadas AJAX a la API |
| `$.each(array, fn)` | Iterar el array de productos |
| `$('<div>').html(template)` | Crear elementos dinámicamente |
| `.hide().appendTo($grid).fadeIn()` | Animación de entrada escalonada |
| `.off('click').on('click', '.btn-agregar', fn)` | Delegación de eventos (evita duplicados) |
| `setTimeout/clearTimeout` | Debounce manual para búsqueda (300ms) |

---

## 8. Inventario completo de archivos creados

```
src/EcommerceNet.Web/
├── .eslintrc.cjs               — Configuración ESLint con plugin Vue 3 y Prettier
├── .prettierrc.json            — Formato: sin semicolons, comillas simples
├── index.html                  — Punto de entrada HTML con div#app
├── package.json                — Dependencias y scripts (dev, build, lint)
├── vite.config.js              — Configuración de Vite con alias @
├── public/
│   └── legacy.html             — Página standalone con jQuery 3.7.1 consumiendo la API
└── src/
    ├── main.js                 — Punto de entrada: createApp, use(pinia), use(router)
    ├── App.vue                 — Layout: NavBar + RouterView + estilos globales con fuente Inter
    ├── assets/
    │   └── main.css            — Variables CSS globales y reset mínimo
    ├── services/
    │   └── api.js              — Instancia Axios con baseURL y dos interceptores (request/response)
    ├── stores/
    │   ├── authStore.js        — Login, registro, logout; persistencia JWT en localStorage
    │   ├── productoStore.js    — Catálogo, búsqueda, filtrado por categoría (computed reactivo)
    │   └── carritoStore.js     — CRUD del carrito + checkout; totalItems y totalPrecio computados
    ├── router/
    │   └── index.js            — 7 rutas, meta.requiereAuth, guard beforeEach con redirección
    ├── components/
    │   ├── NavBar.vue          — Barra sticky con badge del carrito y menú condicional por auth
    │   ├── ProductoCard.vue    — Tarjeta de producto con props/emits, imagen fallback, precio formateado
    │   └── CategoriaFiltro.vue — Sidebar de categorías con botón activo resaltado
    └── views/
        ├── TiendaView.vue          — Catálogo completo: grid + buscador + sidebar
        ├── ProductoDetalleView.vue — Detalle individual: imagen grande, selector +/-, agregar
        ├── CarritoView.vue         — Tabla editable, controles de cantidad, resumen total
        ├── CheckoutView.vue        — Formulario dirección + resumen + pantalla éxito con número de orden
        ├── LoginView.vue           — Email/password, errores inline, redirige post-login
        ├── RegistroView.vue        — 4 campos, validación contraseñas iguales, mínimo 6 caracteres
        └── MisOrdenesView.vue      — Historial con badges por estado, botón cancelar orden
```

**Total: 19 archivos creados**

---

## 9. Flujo de datos de extremo a extremo

```
USUARIO hace clic en "Agregar al carrito"
         ↓
ProductoCard.vue  →  emit('agregar', producto.id)
         ↓
TiendaView.vue    →  agregarAlCarrito(productoId)
         ↓
carritoStore.js   →  agregarProducto(productoId, cantidad=1)
         ↓
api.js            →  axios.post('/carrito/agregar', { productoId, cantidad })
                      [interceptor agrega: Authorization: Bearer eyJhbG...]
         ↓
EcommerceNet.API  →  CarritoController.AgregarProducto()
                      → CarritoServicio.AgregarProductoAsync()
                      → CarritoRepositorio.ObtenerPorUsuarioAsync()
                      → UnidadDeTrabajo.GuardarCambiosAsync()
                      → SQL Server: UPDATE CarritoItems, INSERT si nuevo
         ↓
Respuesta HTTP 200:
{
  "exito": true,
  "datos": { "items": [...], "totalItems": 3, "total": 1299.97 },
  "mensaje": "Producto agregado al carrito"
}
         ↓
api.js            →  interceptor de response (si 200, pasa tal cual)
         ↓
carritoStore.js   →  items.value = data.datos.items
                      mensaje.value = "Producto agregado"  (se limpia en 3s)
         ↓
NavBar.vue        →  carrito.totalItems (reactivo → badge se actualiza automáticamente)
TiendaView.vue    →  carrito.mensaje (reactivo → banner verde aparece)
```

---

## 10. Decisiones técnicas y por qué

| Decisión | Alternativa rechazada | Razón |
|----------|----------------------|-------|
| **Pinia** en lugar de Vuex | Vuex 4 | Pinia es la librería oficial para Vue 3, sin mutations, más simple, mejor TypeScript. Vuex quedó en mantenimiento. |
| **Composition API** (`<script setup>`) | Options API | Mejor organización por funcionalidad, reutilización con composables, requisito explícito de la vacante. |
| **localStorage para JWT** | Cookie HttpOnly | Cookie requiere configuración CORS con credenciales en ambos lados. localStorage es suficiente para demo. En producción se usaría cookie HttpOnly. |
| **Axios con interceptores** | fetch nativo | Interceptores permiten inyectar el JWT una sola vez. fetch requeriría envolver todas las llamadas manualmente. |
| **Filtrado local** de productos | Llamar a la API en cada búsqueda | Con 12 productos es más rápido filtrar en el cliente. En producción con miles de productos se llamaría al endpoint `/buscar`. |
| **Fuente Inter** de Google Fonts | Sistema font-stack | Inter es legible, moderna, popular en SaaS. Demuestra atención al diseño. |
| **Proyecto manual** en vez de `create-vue` | Usar el scaffold | El CLI interactivo no se puede ejecutar sin interacción. El proyecto manual da más control y menos archivos innecesarios. |
| **Página jQuery standalone** en `public/` | Integrarlo en Vue | CLAUDE.md especifica "una página standalone que consume la API". Debe ser accesible sin router de Vue. |

---

## 11. Errores evitados y cómo

| Error potencial | Cómo se evitó |
|----------------|---------------|
| **CORS bloqueando la SPA** | La API ya tenía `WithOrigins("http://localhost:5173")` del Día 2. El Día 4 no necesitó cambios. |
| **401 Unauthorized al agregar al carrito** | El interceptor de request agrega automáticamente el token de localStorage. Sin interceptor, cada llamada necesitaría el header manual. |
| **Loop infinito de redirección a /login** | El guard `beforeEach` solo redirige si `to.meta.requiereAuth && !auth.estaLogueado`. Las rutas `/login` y `/registro` no tienen `meta.requiereAuth`. |
| **badge del carrito desactualizado** | `totalItems` es un `computed()` en el store — se recalcula automáticamente cuando cambia `items`. No hay que llamar a ninguna función para actualizarlo. |
| **localStorage undefined en SSR** | La app es CSR (Client-Side Rendering con Vite), no SSR. localStorage siempre está disponible. |
| **Fugas de eventos jQuery** | En `legacy.html`, antes de registrar el click en `.btn-agregar` se hace `.off('click')` para limpiar listeners anteriores. Evita que el evento se dispare múltiples veces al buscar. |
| **Imágenes rotas en tarjetas** | Fallback con `|| 'https://placehold.co/...'` en todos los `<img :src="...">`. |
| **node_modules en git** | Se detectó tras el primer commit. Se creó `.gitignore` y se removieron con `git rm -r --cached`. El segundo commit solo tiene el código fuente. |

---

## 12. Estado del proyecto al cierre del Día 4

### Comandos de verificación ejecutados

```bash
# Build de producción exitoso
cd src/EcommerceNet.Web
npm run build
# ✓ 103 módulos transformados
# dist/index.html: 0.44 kB
# dist/assets/index-B-DFAkwK.css: 12.50 kB
# dist/assets/index-gFAjy-So.js: 152.67 kB
# ✓ built in 1.43s
```

### Árbol completo de la solución

```
EcommerceNet/
├── .gitignore                              — Excluye bin/, obj/, node_modules/, dist/
├── CLAUDE.md                               — Reglas de arquitectura
├── README.md                               — Descripción del proyecto
├── EcommerceNet.slnx                       — Solución .NET (formato .NET 9+)
├── docs/
│   ├── dia-01-fundamentos-csharp.md
│   ├── dia-01-clase-programacion.md
│   ├── dia-01-manual-tecnico.md
│   ├── dia-02-aspnet-api.md
│   ├── dia-02-clase-programacion.md
│   ├── dia-02-manual-tecnico.md
│   ├── dia-03-datos.md
│   ├── dia-03-clase-programacion.md
│   ├── dia-03-manual-tecnico.md
│   ├── dia-04-frontend.md
│   ├── dia-04-clase-programacion.md       ← NUEVO (Día 4)
│   └── dia-04-manual-tecnico.md           ← NUEVO (Día 4)
├── src/
│   ├── EcommerceNet.Core/                  — Día 1: Entidades, interfaces, DTOs, servicios
│   ├── EcommerceNet.Data/                  — Día 3: EF Core, repositorios, MongoDB
│   ├── EcommerceNet.API/                   — Día 2: Controladores, JWT, Swagger
│   └── EcommerceNet.Web/                   — Día 4: Vue.js 3 SPA ← NUEVO
│       ├── package.json
│       ├── vite.config.js
│       ├── index.html
│       ├── public/
│       │   └── legacy.html                 ← jQuery 3.7.1
│       └── src/
│           ├── main.js
│           ├── App.vue
│           ├── assets/main.css
│           ├── services/api.js             ← Axios + interceptores JWT
│           ├── stores/authStore.js         ← Login/registro/logout + localStorage
│           ├── stores/productoStore.js     ← Catálogo con filtrado reactivo
│           ├── stores/carritoStore.js      ← CRUD carrito + checkout
│           ├── router/index.js             ← 7 rutas + guard beforeEach
│           ├── components/NavBar.vue       ← Sticky + badge carrito
│           ├── components/ProductoCard.vue ← Tarjeta con props/emits
│           ├── components/CategoriaFiltro.vue ← Sidebar de filtros
│           ├── views/TiendaView.vue
│           ├── views/ProductoDetalleView.vue
│           ├── views/CarritoView.vue
│           ├── views/CheckoutView.vue
│           ├── views/LoginView.vue
│           ├── views/RegistroView.vue
│           └── views/MisOrdenesView.vue
└── tests/
    └── EcommerceNet.Tests/                 — 23 pruebas unitarias (todas pasando)
```

### Commits del Día 4

```
555c5ae fix: agregar .gitignore y excluir node_modules y dist del repo
1ab0c64 feat: frontend Vue.js 3 con catálogo, carrito, checkout, auth y página jQuery legacy
80eb242 feat: proyecto inicial días 1-3 backend completo
```

### Estado de los builds

| Sistema | Resultado |
|---------|-----------|
| `dotnet build` | ✅ 0 errores, 0 warnings |
| `dotnet test` | ✅ 23/23 pasando |
| `npm run build` | ✅ 103 módulos, 0 errores |

---

## 13. Pendientes para el Día 5

### Funcionalidad

1. **CORS en la página jQuery:** El endpoint en `legacy.html` usa `localhost:5152`. Si la API cambia de puerto, hay que actualizarlo manualmente. Pendiente parametrizar.
2. **Protección de rutas Admin:** La UI no muestra un panel de administración. El backend acepta `POST /api/productos` solo para admins — el frontend no tiene formulario de creación de producto.

### DevOps (Día 5)

1. **Dockerfile** para `EcommerceNet.API` con multi-stage build
2. **docker-compose.yml** con API + SQL Server
3. **`.github/workflows/ci-cd.yml`** con jobs de backend y frontend
4. **README.md** con instrucciones de deploy, capturas y URLs de demo
5. **Despliegue en AWS:** API en Elastic Beanstalk, frontend estático en S3 + CloudFront
6. **Tag `v1.0.0`** marcando el proyecto como completo para la entrevista
