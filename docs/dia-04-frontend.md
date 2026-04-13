# Día 04 — Vue.js 3 + JavaScript + jQuery

> **Rama Git:** `dia-04/frontend`  
> **Método:** 16 Pomodoros de 25 min (5 min descanso entre cada uno)  
> **Objetivo:** Crear la tienda online completa con Vue.js 3: catálogo, carrito, checkout, auth. Más una página legacy con jQuery.

---

## Cronograma Pomodoro

| # | Bloque | Qué hacer |
|---|--------|-----------|
| 1 | Setup | Crear proyecto Vue.js 3 con Vite, instalar dependencias |
| 2 | API | Crear servicio Axios con interceptores JWT |
| 3 | Store | Crear authStore con Pinia (login, registro, logout) |
| 4 | Store | Crear productoStore (listar, buscar, filtrar por categoría) |
| 5 | Store | Crear carritoStore (agregar, quitar, actualizar, checkout) |
| 6 | Layout | Crear NavBar.vue con contador del carrito y menú de usuario |
| 7 | Router | Configurar Vue Router con guards de autenticación |
| 8 | Vista | Crear TiendaView.vue — catálogo con grid de ProductoCard |
| 9 | Vista | Crear ProductoCard.vue y CategoriaFiltro.vue |
| 10 | Vista | Crear ProductoDetalleView.vue |
| 11 | Vista | Crear CarritoView.vue con tabla editable |
| 12 | Vista | Crear CheckoutView.vue con formulario y confirmación |
| 13 | Vista | Crear LoginView.vue y RegistroView.vue |
| 14 | Vista | Crear MisOrdenesView.vue |
| 15 | jQuery | Crear public/legacy.html — catálogo con jQuery puro |
| 16 | Merge | Probar todo end-to-end, git merge, push |

---

## Pomodoro 1 — Crear proyecto Vue.js 3 (25 min)

### Crear el proyecto con Vite

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet\src

# Crear proyecto Vue.js
npm create vue@latest EcommerceNet.Web

# Opciones a seleccionar:
# ✔ Add TypeScript? → No (la vacante pide JavaScript)
# ✔ Add JSX Support? → No
# ✔ Add Vue Router? → Yes
# ✔ Add Pinia? → Yes
# ✔ Add Vitest? → No
# ✔ Add ESLint? → Yes
# ✔ Add Prettier? → Yes

cd EcommerceNet.Web
npm install
npm install axios

# Verificar que funciona
npm run dev
# Abrir http://localhost:5173
```

### Estructura de carpetas a crear

```
src/EcommerceNet.Web/src/
├── assets/
│   └── main.css                    # Estilos globales
├── components/
│   ├── NavBar.vue                  # Navegación con contador del carrito
│   ├── ProductoCard.vue            # Tarjeta de producto en el catálogo
│   ├── CategoriaFiltro.vue         # Filtro lateral de categorías
│   └── CarritoItem.vue             # Fila editable de un item del carrito
├── views/
│   ├── TiendaView.vue              # Página principal — catálogo
│   ├── ProductoDetalleView.vue     # Detalle de un producto
│   ├── CarritoView.vue             # Carrito completo
│   ├── CheckoutView.vue            # Formulario de checkout
│   ├── MisOrdenesView.vue          # Historial de compras
│   ├── LoginView.vue               # Inicio de sesión
│   └── RegistroView.vue            # Crear cuenta
├── stores/
│   ├── authStore.js                # Estado de autenticación
│   ├── productoStore.js            # Estado de productos y categorías
│   └── carritoStore.js             # Estado del carrito
├── services/
│   └── api.js                      # Axios configurado con JWT
├── router/
│   └── index.js                    # Rutas y guards
├── App.vue                         # Layout principal
└── main.js                         # Punto de entrada
```

---

## Pomodoro 2 — Servicio API con Axios (25 min)

### Archivo: `src/services/api.js`

```javascript
import axios from 'axios'

// Crear instancia de Axios con la URL base de la API
const api = axios.create({
  baseURL: 'http://localhost:5000/api',  // ajustar al puerto de tu API
  headers: {
    'Content-Type': 'application/json'
  }
})

// INTERCEPTOR DE REQUEST — agrega el JWT a cada petición automáticamente
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwt_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// INTERCEPTOR DE RESPONSE — maneja errores globalmente
api.interceptors.response.use(
  (response) => response,  // si todo OK, pasar la respuesta tal cual
  (error) => {
    // Si el token expiró (401), redirigir a login
    if (error.response?.status === 401) {
      localStorage.removeItem('jwt_token')
      localStorage.removeItem('usuario')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
```

> **Concepto Vue.js: Interceptores de Axios**
> Son funciones que se ejecutan ANTES de enviar cada petición (request interceptor)
> y DESPUÉS de recibir cada respuesta (response interceptor).
> El de request agrega el JWT para que no tengas que hacerlo manualmente en cada llamada.
> El de response redirige a login si el token expiró.

---

## Pomodoro 3 — Auth Store con Pinia (25 min)

### Archivo: `src/stores/authStore.js`

```javascript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  // --- Estado reactivo ---
  const usuario = ref(JSON.parse(localStorage.getItem('usuario') || 'null'))
  const token = ref(localStorage.getItem('jwt_token') || '')
  const cargando = ref(false)
  const error = ref('')

  // --- Getters (propiedades computadas) ---
  const estaLogueado = computed(() => !!token.value)
  const esAdmin = computed(() => usuario.value?.rol === 'Admin')
  const nombreUsuario = computed(() => usuario.value?.nombre || '')

  // --- Acciones ---

  async function login(email, password) {
    cargando.value = true
    error.value = ''
    try {
      const { data } = await api.post('/auth/login', { email, password })
      if (data.exito) {
        token.value = data.datos.token
        usuario.value = {
          nombre: data.datos.nombre,
          email: data.datos.email,
          rol: data.datos.rol
        }
        localStorage.setItem('jwt_token', data.datos.token)
        localStorage.setItem('usuario', JSON.stringify(usuario.value))
        return true
      } else {
        error.value = data.mensaje || 'Error al iniciar sesión'
        return false
      }
    } catch (err) {
      error.value = err.response?.data?.mensaje || 'Error de conexión'
      return false
    } finally {
      cargando.value = false
    }
  }

  async function registrar(nombre, email, password) {
    cargando.value = true
    error.value = ''
    try {
      const { data } = await api.post('/auth/registrar', { nombre, email, password })
      if (data.exito) {
        token.value = data.datos.token
        usuario.value = {
          nombre: data.datos.nombre,
          email: data.datos.email,
          rol: data.datos.rol
        }
        localStorage.setItem('jwt_token', data.datos.token)
        localStorage.setItem('usuario', JSON.stringify(usuario.value))
        return true
      } else {
        error.value = data.mensaje || 'Error al registrarse'
        return false
      }
    } catch (err) {
      error.value = err.response?.data?.mensaje || 'Error de conexión'
      return false
    } finally {
      cargando.value = false
    }
  }

  function logout() {
    token.value = ''
    usuario.value = null
    localStorage.removeItem('jwt_token')
    localStorage.removeItem('usuario')
  }

  return {
    usuario, token, cargando, error,
    estaLogueado, esAdmin, nombreUsuario,
    login, registrar, logout
  }
})
```

> **Concepto Vue.js: Pinia con Composition API**
> `defineStore` crea un store global. Dentro usamos `ref()` para estado reactivo
> y `computed()` para getters. Las funciones son las acciones.
> A diferencia de Vuex, no hay mutations — modificas el estado directamente.
> `localStorage` persiste el token para que no se pierda al recargar la página.

---

## Pomodoro 4 — Producto Store (25 min)

### Archivo: `src/stores/productoStore.js`

```javascript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export const useProductoStore = defineStore('producto', () => {
  // Estado
  const productos = ref([])
  const categorias = ref([])
  const categoriaSeleccionada = ref(null)
  const terminoBusqueda = ref('')
  const cargando = ref(false)

  // Getters
  const productosFiltrados = computed(() => {
    let resultado = productos.value

    // Filtrar por categoría si hay una seleccionada
    if (categoriaSeleccionada.value) {
      resultado = resultado.filter(
        p => p.categoriaNombre === categoriaSeleccionada.value
      )
    }

    // Filtrar por término de búsqueda
    if (terminoBusqueda.value.trim()) {
      const termino = terminoBusqueda.value.toLowerCase()
      resultado = resultado.filter(
        p => p.nombre.toLowerCase().includes(termino) ||
             p.descripcion.toLowerCase().includes(termino)
      )
    }

    return resultado
  })

  const totalProductos = computed(() => productosFiltrados.value.length)

  // Acciones
  async function cargarProductos() {
    cargando.value = true
    try {
      const { data } = await api.get('/productos')
      if (data.exito) {
        productos.value = data.datos
        // Extraer categorías únicas de los productos
        const cats = [...new Set(data.datos.map(p => p.categoriaNombre))]
        categorias.value = cats.filter(c => c && c !== 'Sin categoría')
      }
    } catch (err) {
      console.error('Error cargando productos:', err)
    } finally {
      cargando.value = false
    }
  }

  async function buscarProductos(termino) {
    cargando.value = true
    try {
      const { data } = await api.get(`/productos/buscar?termino=${termino}`)
      if (data.exito) {
        productos.value = data.datos
      }
    } catch (err) {
      console.error('Error buscando:', err)
    } finally {
      cargando.value = false
    }
  }

  function filtrarPorCategoria(categoria) {
    categoriaSeleccionada.value =
      categoriaSeleccionada.value === categoria ? null : categoria
  }

  function limpiarFiltros() {
    categoriaSeleccionada.value = null
    terminoBusqueda.value = ''
  }

  return {
    productos, categorias, categoriaSeleccionada, terminoBusqueda, cargando,
    productosFiltrados, totalProductos,
    cargarProductos, buscarProductos, filtrarPorCategoria, limpiarFiltros
  }
})
```

---

## Pomodoro 5 — Carrito Store (25 min)

### Archivo: `src/stores/carritoStore.js`

```javascript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export const useCarritoStore = defineStore('carrito', () => {
  // Estado
  const items = ref([])
  const cargando = ref(false)
  const mensaje = ref('')

  // Getters
  const totalItems = computed(() =>
    items.value.reduce((sum, item) => sum + item.cantidad, 0)
  )

  const totalPrecio = computed(() =>
    items.value.reduce((sum, item) => sum + item.subtotal, 0)
  )

  const estaVacio = computed(() => items.value.length === 0)

  // Acciones
  async function cargarCarrito() {
    cargando.value = true
    try {
      const { data } = await api.get('/carrito')
      if (data.exito) {
        items.value = data.datos.items || []
      }
    } catch (err) {
      console.error('Error cargando carrito:', err)
    } finally {
      cargando.value = false
    }
  }

  async function agregarProducto(productoId, cantidad = 1) {
    try {
      const { data } = await api.post('/carrito/agregar', { productoId, cantidad })
      if (data.exito) {
        items.value = data.datos.items || []
        mensaje.value = data.mensaje || 'Producto agregado'
        setTimeout(() => mensaje.value = '', 3000)
        return true
      }
      mensaje.value = data.mensaje || 'Error al agregar'
      return false
    } catch (err) {
      mensaje.value = err.response?.data?.mensaje || 'Error de conexión'
      return false
    }
  }

  async function actualizarCantidad(productoId, cantidad) {
    try {
      const { data } = await api.put(`/carrito/${productoId}`, { cantidad })
      if (data.exito) {
        items.value = data.datos.items || []
      }
    } catch (err) {
      console.error('Error actualizando:', err)
    }
  }

  async function eliminarProducto(productoId) {
    try {
      const { data } = await api.delete(`/carrito/${productoId}`)
      if (data.exito) {
        items.value = data.datos.items || []
        mensaje.value = 'Producto eliminado del carrito'
        setTimeout(() => mensaje.value = '', 3000)
      }
    } catch (err) {
      console.error('Error eliminando:', err)
    }
  }

  async function vaciar() {
    try {
      await api.delete('/carrito')
      items.value = []
    } catch (err) {
      console.error('Error vaciando:', err)
    }
  }

  async function checkout(direccionEnvio) {
    cargando.value = true
    try {
      const { data } = await api.post('/carrito/checkout', { direccionEnvio })
      if (data.exito) {
        items.value = []
        return { exito: true, orden: data.datos, mensaje: data.mensaje }
      }
      return { exito: false, mensaje: data.mensaje, errores: data.errores }
    } catch (err) {
      return {
        exito: false,
        mensaje: err.response?.data?.mensaje || 'Error procesando la compra'
      }
    } finally {
      cargando.value = false
    }
  }

  return {
    items, cargando, mensaje,
    totalItems, totalPrecio, estaVacio,
    cargarCarrito, agregarProducto, actualizarCantidad,
    eliminarProducto, vaciar, checkout
  }
})
```

---

## Pomodoro 6 — NavBar con contador del carrito (25 min)

### Archivo: `src/components/NavBar.vue`

```vue
<script setup>
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useCarritoStore } from '@/stores/carritoStore'

const auth = useAuthStore()
const carrito = useCarritoStore()
const router = useRouter()

function cerrarSesion() {
  auth.logout()
  router.push('/')
}
</script>

<template>
  <nav class="navbar">
    <div class="navbar-marca">
      <RouterLink to="/" class="logo">EcommerceNet</RouterLink>
    </div>

    <div class="navbar-links">
      <RouterLink to="/">Tienda</RouterLink>

      <template v-if="auth.estaLogueado">
        <RouterLink to="/mis-ordenes">Mis Órdenes</RouterLink>
        <RouterLink to="/carrito" class="carrito-link">
          Carrito
          <span v-if="carrito.totalItems > 0" class="carrito-badge">
            {{ carrito.totalItems }}
          </span>
        </RouterLink>
        <span class="usuario-nombre">{{ auth.nombreUsuario }}</span>
        <button @click="cerrarSesion" class="btn-logout">Salir</button>
      </template>

      <template v-else>
        <RouterLink to="/login" class="btn-login">Iniciar Sesión</RouterLink>
        <RouterLink to="/registro" class="btn-registro">Registrarse</RouterLink>
      </template>
    </div>
  </nav>
</template>

<style scoped>
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background: #fff;
  border-bottom: 1px solid #e5e7eb;
  position: sticky;
  top: 0;
  z-index: 100;
}

.logo {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1a1a2e;
  text-decoration: none;
}

.navbar-links {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.navbar-links a {
  color: #4b5563;
  text-decoration: none;
  font-size: 0.95rem;
}

.navbar-links a:hover {
  color: #1a1a2e;
}

.carrito-link {
  position: relative;
}

.carrito-badge {
  position: absolute;
  top: -8px;
  right: -12px;
  background: #e24b4a;
  color: white;
  font-size: 0.7rem;
  padding: 2px 6px;
  border-radius: 10px;
  font-weight: 600;
}

.usuario-nombre {
  color: #6b7280;
  font-size: 0.9rem;
}

.btn-logout {
  background: none;
  border: 1px solid #e5e7eb;
  padding: 0.4rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  color: #4b5563;
}

.btn-login, .btn-registro {
  padding: 0.4rem 1rem;
  border-radius: 6px;
}

.btn-registro {
  background: #1a1a2e;
  color: white !important;
}
</style>
```

---

## Pomodoro 7 — Vue Router con guards (25 min)

### Archivo: `src/router/index.js`

```javascript
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

import TiendaView from '@/views/TiendaView.vue'
import ProductoDetalleView from '@/views/ProductoDetalleView.vue'
import CarritoView from '@/views/CarritoView.vue'
import CheckoutView from '@/views/CheckoutView.vue'
import MisOrdenesView from '@/views/MisOrdenesView.vue'
import LoginView from '@/views/LoginView.vue'
import RegistroView from '@/views/RegistroView.vue'

const rutas = [
  {
    path: '/',
    name: 'tienda',
    component: TiendaView
  },
  {
    path: '/producto/:id',
    name: 'producto-detalle',
    component: ProductoDetalleView
  },
  {
    path: '/carrito',
    name: 'carrito',
    component: CarritoView,
    meta: { requiereAuth: true }
  },
  {
    path: '/checkout',
    name: 'checkout',
    component: CheckoutView,
    meta: { requiereAuth: true }
  },
  {
    path: '/mis-ordenes',
    name: 'mis-ordenes',
    component: MisOrdenesView,
    meta: { requiereAuth: true }
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView
  },
  {
    path: '/registro',
    name: 'registro',
    component: RegistroView
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes: rutas
})

// GUARD DE NAVEGACIÓN — protege rutas que requieren login
router.beforeEach((to, from, next) => {
  const auth = useAuthStore()

  if (to.meta.requiereAuth && !auth.estaLogueado) {
    // Guardar la ruta destino para redirigir después del login
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else {
    next()
  }
})

export default router
```

> **Concepto Vue.js: Navigation Guards**
> `router.beforeEach` se ejecuta ANTES de cada cambio de ruta.
> Si la ruta tiene `meta: { requiereAuth: true }` y el usuario no está logueado,
> lo redirigimos a `/login`. Después del login, lo llevamos a donde quería ir.
> Es el equivalente frontend del `[Authorize]` del backend.

---

## Pomodoros 8-9 — TiendaView y componentes (50 min)

### Archivo: `src/views/TiendaView.vue`

```vue
<script setup>
import { onMounted } from 'vue'
import { useProductoStore } from '@/stores/productoStore'
import { useCarritoStore } from '@/stores/carritoStore'
import { useAuthStore } from '@/stores/authStore'
import ProductoCard from '@/components/ProductoCard.vue'
import CategoriaFiltro from '@/components/CategoriaFiltro.vue'

const productoStore = useProductoStore()
const carrito = useCarritoStore()
const auth = useAuthStore()

// Cargar productos al montar el componente
onMounted(() => {
  productoStore.cargarProductos()
  if (auth.estaLogueado) {
    carrito.cargarCarrito()
  }
})

async function agregarAlCarrito(productoId) {
  if (!auth.estaLogueado) {
    alert('Inicia sesión para agregar productos al carrito')
    return
  }
  await carrito.agregarProducto(productoId)
}
</script>

<template>
  <div class="tienda">
    <div class="tienda-header">
      <h1>Catálogo de productos</h1>
      <div class="busqueda">
        <input
          v-model="productoStore.terminoBusqueda"
          type="text"
          placeholder="Buscar productos..."
          class="input-busqueda"
        />
      </div>
    </div>

    <!-- Mensaje del carrito -->
    <div v-if="carrito.mensaje" class="mensaje-carrito">
      {{ carrito.mensaje }}
    </div>

    <div class="tienda-contenido">
      <!-- Filtro de categorías -->
      <aside class="sidebar">
        <CategoriaFiltro
          :categorias="productoStore.categorias"
          :seleccionada="productoStore.categoriaSeleccionada"
          @filtrar="productoStore.filtrarPorCategoria"
          @limpiar="productoStore.limpiarFiltros"
        />
      </aside>

      <!-- Grid de productos -->
      <main class="productos-grid">
        <p v-if="productoStore.cargando" class="cargando">Cargando productos...</p>

        <p v-else-if="productoStore.productosFiltrados.length === 0" class="sin-resultados">
          No se encontraron productos.
        </p>

        <ProductoCard
          v-for="producto in productoStore.productosFiltrados"
          :key="producto.id"
          :producto="producto"
          @agregar="agregarAlCarrito"
        />
      </main>
    </div>
  </div>
</template>

<style scoped>
.tienda { max-width: 1200px; margin: 0 auto; padding: 2rem; }
.tienda-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.tienda-header h1 { font-size: 1.8rem; color: #1a1a2e; }
.input-busqueda { padding: 0.6rem 1rem; border: 1px solid #e5e7eb; border-radius: 8px; width: 300px; font-size: 0.95rem; }
.mensaje-carrito { background: #e1f5ee; color: #0f6e56; padding: 0.8rem 1rem; border-radius: 8px; margin-bottom: 1rem; text-align: center; }
.tienda-contenido { display: grid; grid-template-columns: 220px 1fr; gap: 2rem; }
.productos-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 1.5rem; }
.cargando, .sin-resultados { grid-column: 1 / -1; text-align: center; color: #6b7280; padding: 3rem; }
</style>
```

### Archivo: `src/components/ProductoCard.vue`

```vue
<script setup>
import { RouterLink } from 'vue-router'

const props = defineProps({
  producto: { type: Object, required: true }
})

const emit = defineEmits(['agregar'])

function formatearPrecio(precio) {
  return new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN'
  }).format(precio)
}
</script>

<template>
  <div class="producto-card">
    <RouterLink :to="`/producto/${producto.id}`" class="producto-imagen">
      <img :src="producto.imagenUrl || 'https://placehold.co/400x300?text=Producto'" :alt="producto.nombre" />
    </RouterLink>

    <div class="producto-info">
      <span class="producto-categoria">{{ producto.categoriaNombre }}</span>
      <RouterLink :to="`/producto/${producto.id}`" class="producto-nombre">
        {{ producto.nombre }}
      </RouterLink>
      <p class="producto-precio">{{ formatearPrecio(producto.precio) }}</p>

      <div class="producto-footer">
        <span :class="['stock', producto.stock > 0 ? 'en-stock' : 'sin-stock']">
          {{ producto.stock > 0 ? `${producto.stock} disponibles` : 'Agotado' }}
        </span>
        <button
          @click="emit('agregar', producto.id)"
          :disabled="producto.stock <= 0"
          class="btn-agregar"
        >
          Agregar al carrito
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.producto-card { background: #fff; border: 1px solid #e5e7eb; border-radius: 12px; overflow: hidden; transition: transform 0.2s, box-shadow 0.2s; }
.producto-card:hover { transform: translateY(-4px); box-shadow: 0 8px 25px rgba(0,0,0,0.08); }
.producto-imagen img { width: 100%; height: 200px; object-fit: cover; }
.producto-info { padding: 1rem; }
.producto-categoria { font-size: 0.75rem; color: #6b7280; text-transform: uppercase; letter-spacing: 1px; }
.producto-nombre { display: block; font-size: 1rem; font-weight: 600; color: #1a1a2e; text-decoration: none; margin: 0.3rem 0; }
.producto-precio { font-size: 1.2rem; font-weight: 700; color: #1d9e75; margin: 0.5rem 0; }
.producto-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 0.8rem; }
.stock { font-size: 0.8rem; }
.en-stock { color: #1d9e75; }
.sin-stock { color: #e24b4a; }
.btn-agregar { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
.btn-agregar:hover { background: #2d2d4e; }
.btn-agregar:disabled { background: #d1d5db; cursor: not-allowed; }
</style>
```

### Archivo: `src/components/CategoriaFiltro.vue`

```vue
<script setup>
const props = defineProps({
  categorias: { type: Array, required: true },
  seleccionada: { type: String, default: null }
})

const emit = defineEmits(['filtrar', 'limpiar'])
</script>

<template>
  <div class="filtro">
    <h3>Categorías</h3>
    <button
      @click="emit('limpiar')"
      :class="['filtro-btn', !seleccionada ? 'activo' : '']"
    >
      Todas
    </button>
    <button
      v-for="cat in categorias"
      :key="cat"
      @click="emit('filtrar', cat)"
      :class="['filtro-btn', seleccionada === cat ? 'activo' : '']"
    >
      {{ cat }}
    </button>
  </div>
</template>

<style scoped>
.filtro h3 { font-size: 1rem; margin-bottom: 1rem; color: #1a1a2e; }
.filtro-btn { display: block; width: 100%; text-align: left; padding: 0.6rem 1rem; border: none; background: none; cursor: pointer; border-radius: 6px; margin-bottom: 0.3rem; font-size: 0.9rem; color: #4b5563; }
.filtro-btn:hover { background: #f3f4f6; }
.filtro-btn.activo { background: #1a1a2e; color: white; }
</style>
```

---

## Pomodoro 10 — ProductoDetalleView (25 min)

### Archivo: `src/views/ProductoDetalleView.vue`

```vue
<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useCarritoStore } from '@/stores/carritoStore'
import { useAuthStore } from '@/stores/authStore'
import api from '@/services/api'

const route = useRoute()
const router = useRouter()
const carrito = useCarritoStore()
const auth = useAuthStore()

const producto = ref(null)
const cantidad = ref(1)
const cargando = ref(true)

function formatearPrecio(precio) {
  return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(precio)
}

onMounted(async () => {
  try {
    const { data } = await api.get(`/productos/${route.params.id}`)
    if (data.exito) {
      producto.value = data.datos
    }
  } catch (err) {
    console.error('Error:', err)
  } finally {
    cargando.value = false
  }
})

async function agregar() {
  if (!auth.estaLogueado) {
    router.push('/login')
    return
  }
  const exito = await carrito.agregarProducto(producto.value.id, cantidad.value)
  if (exito) {
    cantidad.value = 1
  }
}
</script>

<template>
  <div class="detalle" v-if="producto">
    <button @click="router.back()" class="btn-volver">← Volver</button>

    <div class="detalle-grid">
      <img :src="producto.imagenUrl || 'https://placehold.co/600x400?text=Producto'" :alt="producto.nombre" class="detalle-img" />

      <div class="detalle-info">
        <span class="detalle-categoria">{{ producto.categoriaNombre }}</span>
        <h1>{{ producto.nombre }}</h1>
        <p class="detalle-descripcion">{{ producto.descripcion }}</p>
        <p class="detalle-precio">{{ formatearPrecio(producto.precio) }}</p>

        <div class="detalle-stock">
          <span :class="producto.stock > 0 ? 'en-stock' : 'sin-stock'">
            {{ producto.stock > 0 ? `${producto.stock} disponibles` : 'Agotado' }}
          </span>
        </div>

        <div class="detalle-acciones" v-if="producto.stock > 0">
          <div class="cantidad-selector">
            <button @click="cantidad > 1 && cantidad--">-</button>
            <span>{{ cantidad }}</span>
            <button @click="cantidad < producto.stock && cantidad++">+</button>
          </div>
          <button @click="agregar" class="btn-agregar-grande">
            Agregar al carrito
          </button>
        </div>

        <div v-if="carrito.mensaje" class="mensaje-exito">
          {{ carrito.mensaje }}
        </div>
      </div>
    </div>
  </div>

  <div v-else-if="cargando" class="cargando-pagina">Cargando...</div>
  <div v-else class="no-encontrado">Producto no encontrado</div>
</template>

<style scoped>
.detalle { max-width: 1000px; margin: 0 auto; padding: 2rem; }
.btn-volver { background: none; border: none; color: #6b7280; cursor: pointer; margin-bottom: 1rem; font-size: 0.95rem; }
.detalle-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 3rem; }
.detalle-img { width: 100%; border-radius: 12px; }
.detalle-categoria { font-size: 0.8rem; color: #6b7280; text-transform: uppercase; }
.detalle-info h1 { font-size: 1.8rem; color: #1a1a2e; margin: 0.5rem 0; }
.detalle-descripcion { color: #4b5563; line-height: 1.6; margin: 1rem 0; }
.detalle-precio { font-size: 2rem; font-weight: 700; color: #1d9e75; }
.detalle-acciones { display: flex; gap: 1rem; align-items: center; margin-top: 1.5rem; }
.cantidad-selector { display: flex; align-items: center; border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden; }
.cantidad-selector button { width: 40px; height: 40px; border: none; background: #f9fafb; cursor: pointer; font-size: 1.2rem; }
.cantidad-selector span { width: 50px; text-align: center; font-weight: 600; }
.btn-agregar-grande { background: #1a1a2e; color: white; border: none; padding: 0.8rem 2rem; border-radius: 8px; cursor: pointer; font-size: 1rem; }
.mensaje-exito { margin-top: 1rem; padding: 0.8rem; background: #e1f5ee; color: #0f6e56; border-radius: 8px; }
.en-stock { color: #1d9e75; } .sin-stock { color: #e24b4a; }
.cargando-pagina, .no-encontrado { text-align: center; padding: 4rem; color: #6b7280; }
</style>
```

---

## Pomodoros 11-12 — CarritoView y CheckoutView (50 min)

### Archivo: `src/views/CarritoView.vue`

```vue
<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCarritoStore } from '@/stores/carritoStore'

const carrito = useCarritoStore()
const router = useRouter()

function formatearPrecio(precio) {
  return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(precio)
}

onMounted(() => carrito.cargarCarrito())
</script>

<template>
  <div class="carrito-page">
    <h1>Mi carrito</h1>

    <div v-if="carrito.estaVacio" class="carrito-vacio">
      <p>Tu carrito está vacío</p>
      <button @click="router.push('/')" class="btn-seguir">Seguir comprando</button>
    </div>

    <div v-else class="carrito-contenido">
      <table class="carrito-tabla">
        <thead>
          <tr>
            <th>Producto</th>
            <th>Precio</th>
            <th>Cantidad</th>
            <th>Subtotal</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in carrito.items" :key="item.productoId">
            <td class="producto-cell">
              <img :src="item.imagenUrl || 'https://placehold.co/60x60'" :alt="item.productoNombre" />
              <span>{{ item.productoNombre }}</span>
            </td>
            <td>{{ formatearPrecio(item.precioUnitario) }}</td>
            <td>
              <div class="cantidad-control">
                <button @click="carrito.actualizarCantidad(item.productoId, item.cantidad - 1)">-</button>
                <span>{{ item.cantidad }}</span>
                <button @click="carrito.actualizarCantidad(item.productoId, item.cantidad + 1)">+</button>
              </div>
            </td>
            <td class="subtotal">{{ formatearPrecio(item.subtotal) }}</td>
            <td>
              <button @click="carrito.eliminarProducto(item.productoId)" class="btn-eliminar">✕</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="carrito-resumen">
        <div class="resumen-linea">
          <span>Total ({{ carrito.totalItems }} productos):</span>
          <span class="resumen-total">{{ formatearPrecio(carrito.totalPrecio) }}</span>
        </div>
        <div class="resumen-acciones">
          <button @click="carrito.vaciar()" class="btn-vaciar">Vaciar carrito</button>
          <button @click="router.push('/checkout')" class="btn-checkout">Proceder al pago</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.carrito-page { max-width: 900px; margin: 0 auto; padding: 2rem; }
.carrito-page h1 { font-size: 1.8rem; color: #1a1a2e; margin-bottom: 2rem; }
.carrito-vacio { text-align: center; padding: 4rem; color: #6b7280; }
.btn-seguir { margin-top: 1rem; background: #1a1a2e; color: white; border: none; padding: 0.6rem 1.5rem; border-radius: 8px; cursor: pointer; }
.carrito-tabla { width: 100%; border-collapse: collapse; }
.carrito-tabla th { text-align: left; padding: 0.8rem; border-bottom: 2px solid #e5e7eb; color: #6b7280; font-size: 0.85rem; text-transform: uppercase; }
.carrito-tabla td { padding: 1rem 0.8rem; border-bottom: 1px solid #f3f4f6; }
.producto-cell { display: flex; align-items: center; gap: 1rem; }
.producto-cell img { width: 60px; height: 60px; object-fit: cover; border-radius: 8px; }
.cantidad-control { display: flex; align-items: center; gap: 0.5rem; }
.cantidad-control button { width: 30px; height: 30px; border: 1px solid #e5e7eb; background: #f9fafb; border-radius: 6px; cursor: pointer; }
.cantidad-control span { font-weight: 600; min-width: 30px; text-align: center; }
.subtotal { font-weight: 600; color: #1a1a2e; }
.btn-eliminar { background: none; border: none; color: #e24b4a; cursor: pointer; font-size: 1.2rem; }
.carrito-resumen { margin-top: 2rem; padding: 1.5rem; background: #f9fafb; border-radius: 12px; }
.resumen-linea { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.resumen-total { font-size: 1.5rem; font-weight: 700; color: #1d9e75; }
.resumen-acciones { display: flex; justify-content: space-between; }
.btn-vaciar { background: none; border: 1px solid #e5e7eb; padding: 0.6rem 1.5rem; border-radius: 8px; cursor: pointer; color: #6b7280; }
.btn-checkout { background: #1d9e75; color: white; border: none; padding: 0.8rem 2rem; border-radius: 8px; cursor: pointer; font-size: 1rem; font-weight: 600; }
</style>
```

### Archivo: `src/views/CheckoutView.vue`

```vue
<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useCarritoStore } from '@/stores/carritoStore'

const carrito = useCarritoStore()
const router = useRouter()

const direccion = ref('')
const procesando = ref(false)
const ordenCreada = ref(null)
const errorCheckout = ref('')

function formatearPrecio(precio) {
  return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(precio)
}

async function confirmarCompra() {
  if (!direccion.value.trim()) {
    errorCheckout.value = 'Ingresa una dirección de envío'
    return
  }

  procesando.value = true
  errorCheckout.value = ''

  const resultado = await carrito.checkout(direccion.value)

  if (resultado.exito) {
    ordenCreada.value = resultado.orden
  } else {
    errorCheckout.value = resultado.mensaje || 'Error al procesar la compra'
  }

  procesando.value = false
}
</script>

<template>
  <div class="checkout-page">
    <!-- Orden confirmada -->
    <div v-if="ordenCreada" class="orden-confirmada">
      <div class="icono-exito">✓</div>
      <h1>¡Compra realizada!</h1>
      <p>Número de orden: <strong>{{ ordenCreada.numeroOrden }}</strong></p>
      <p>Total: <strong>{{ formatearPrecio(ordenCreada.total) }}</strong></p>
      <button @click="router.push('/mis-ordenes')" class="btn-ver-ordenes">Ver mis órdenes</button>
      <button @click="router.push('/')" class="btn-seguir">Seguir comprando</button>
    </div>

    <!-- Formulario de checkout -->
    <div v-else>
      <h1>Checkout</h1>

      <div class="checkout-grid">
        <div class="checkout-form">
          <h2>Dirección de envío</h2>
          <textarea
            v-model="direccion"
            placeholder="Ingresa tu dirección completa (calle, número, colonia, ciudad, estado, CP)"
            rows="4"
            class="input-direccion"
          ></textarea>

          <div v-if="errorCheckout" class="error-msg">{{ errorCheckout }}</div>

          <button
            @click="confirmarCompra"
            :disabled="procesando"
            class="btn-confirmar"
          >
            {{ procesando ? 'Procesando...' : 'Confirmar compra' }}
          </button>
        </div>

        <div class="checkout-resumen">
          <h2>Resumen del pedido</h2>
          <div v-for="item in carrito.items" :key="item.productoId" class="resumen-item">
            <span>{{ item.productoNombre }} × {{ item.cantidad }}</span>
            <span>{{ formatearPrecio(item.subtotal) }}</span>
          </div>
          <div class="resumen-total">
            <span>Total:</span>
            <span>{{ formatearPrecio(carrito.totalPrecio) }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.checkout-page { max-width: 900px; margin: 0 auto; padding: 2rem; }
.checkout-page h1 { font-size: 1.8rem; color: #1a1a2e; margin-bottom: 2rem; }
.checkout-grid { display: grid; grid-template-columns: 1fr 350px; gap: 2rem; }
.checkout-form h2, .checkout-resumen h2 { font-size: 1.1rem; margin-bottom: 1rem; color: #1a1a2e; }
.input-direccion { width: 100%; padding: 0.8rem; border: 1px solid #e5e7eb; border-radius: 8px; font-size: 0.95rem; resize: vertical; font-family: inherit; }
.error-msg { color: #e24b4a; margin: 0.5rem 0; font-size: 0.9rem; }
.btn-confirmar { margin-top: 1rem; background: #1d9e75; color: white; border: none; padding: 0.8rem 2rem; border-radius: 8px; cursor: pointer; font-size: 1rem; font-weight: 600; width: 100%; }
.btn-confirmar:disabled { background: #9ca3af; cursor: not-allowed; }
.checkout-resumen { background: #f9fafb; padding: 1.5rem; border-radius: 12px; }
.resumen-item { display: flex; justify-content: space-between; padding: 0.5rem 0; font-size: 0.9rem; border-bottom: 1px solid #e5e7eb; }
.resumen-total { display: flex; justify-content: space-between; padding-top: 1rem; font-size: 1.2rem; font-weight: 700; color: #1d9e75; }
.orden-confirmada { text-align: center; padding: 4rem 2rem; }
.icono-exito { width: 80px; height: 80px; border-radius: 50%; background: #e1f5ee; color: #1d9e75; font-size: 2.5rem; display: flex; align-items: center; justify-content: center; margin: 0 auto 1.5rem; }
.btn-ver-ordenes { background: #1a1a2e; color: white; border: none; padding: 0.6rem 1.5rem; border-radius: 8px; cursor: pointer; margin: 1rem 0.5rem; }
.btn-seguir { background: none; border: 1px solid #e5e7eb; padding: 0.6rem 1.5rem; border-radius: 8px; cursor: pointer; }
</style>
```

---

## Pomodoro 13 — Login y Registro (25 min)

### Archivo: `src/views/LoginView.vue`

```vue
<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const email = ref('')
const password = ref('')

async function iniciarSesion() {
  const exito = await auth.login(email.value, password.value)
  if (exito) {
    const redirect = route.query.redirect || '/'
    router.push(redirect)
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <h1>Iniciar sesión</h1>

      <div v-if="auth.error" class="error-msg">{{ auth.error }}</div>

      <div class="campo">
        <label>Email</label>
        <input v-model="email" type="email" placeholder="tu@email.com" />
      </div>

      <div class="campo">
        <label>Contraseña</label>
        <input v-model="password" type="password" placeholder="••••••••" @keyup.enter="iniciarSesion" />
      </div>

      <button @click="iniciarSesion" :disabled="auth.cargando" class="btn-submit">
        {{ auth.cargando ? 'Cargando...' : 'Iniciar sesión' }}
      </button>

      <p class="link-alterno">
        ¿No tienes cuenta? <RouterLink to="/registro">Regístrate</RouterLink>
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-page { display: flex; justify-content: center; align-items: center; min-height: 70vh; padding: 2rem; }
.auth-card { background: #fff; padding: 2.5rem; border-radius: 16px; border: 1px solid #e5e7eb; width: 100%; max-width: 420px; }
.auth-card h1 { font-size: 1.5rem; color: #1a1a2e; margin-bottom: 1.5rem; text-align: center; }
.error-msg { background: #fef2f2; color: #e24b4a; padding: 0.6rem 1rem; border-radius: 8px; margin-bottom: 1rem; font-size: 0.9rem; }
.campo { margin-bottom: 1rem; }
.campo label { display: block; font-size: 0.85rem; color: #4b5563; margin-bottom: 0.3rem; }
.campo input { width: 100%; padding: 0.7rem; border: 1px solid #e5e7eb; border-radius: 8px; font-size: 0.95rem; }
.btn-submit { width: 100%; padding: 0.8rem; background: #1a1a2e; color: white; border: none; border-radius: 8px; font-size: 1rem; cursor: pointer; margin-top: 0.5rem; }
.btn-submit:disabled { background: #9ca3af; }
.link-alterno { text-align: center; margin-top: 1rem; font-size: 0.9rem; color: #6b7280; }
.link-alterno a { color: #1d9e75; }
</style>
```

### Archivo: `src/views/RegistroView.vue`

```vue
<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()

const nombre = ref('')
const email = ref('')
const password = ref('')
const confirmarPassword = ref('')
const errorLocal = ref('')

async function registrarse() {
  errorLocal.value = ''

  if (password.value !== confirmarPassword.value) {
    errorLocal.value = 'Las contraseñas no coinciden'
    return
  }

  if (password.value.length < 6) {
    errorLocal.value = 'La contraseña debe tener al menos 6 caracteres'
    return
  }

  const exito = await auth.registrar(nombre.value, email.value, password.value)
  if (exito) {
    router.push('/')
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <h1>Crear cuenta</h1>

      <div v-if="auth.error || errorLocal" class="error-msg">
        {{ errorLocal || auth.error }}
      </div>

      <div class="campo">
        <label>Nombre</label>
        <input v-model="nombre" type="text" placeholder="Tu nombre" />
      </div>

      <div class="campo">
        <label>Email</label>
        <input v-model="email" type="email" placeholder="tu@email.com" />
      </div>

      <div class="campo">
        <label>Contraseña</label>
        <input v-model="password" type="password" placeholder="Mínimo 6 caracteres" />
      </div>

      <div class="campo">
        <label>Confirmar contraseña</label>
        <input v-model="confirmarPassword" type="password" placeholder="Repetir contraseña" @keyup.enter="registrarse" />
      </div>

      <button @click="registrarse" :disabled="auth.cargando" class="btn-submit">
        {{ auth.cargando ? 'Cargando...' : 'Crear cuenta' }}
      </button>

      <p class="link-alterno">
        ¿Ya tienes cuenta? <RouterLink to="/login">Inicia sesión</RouterLink>
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-page { display: flex; justify-content: center; align-items: center; min-height: 70vh; padding: 2rem; }
.auth-card { background: #fff; padding: 2.5rem; border-radius: 16px; border: 1px solid #e5e7eb; width: 100%; max-width: 420px; }
.auth-card h1 { font-size: 1.5rem; color: #1a1a2e; margin-bottom: 1.5rem; text-align: center; }
.error-msg { background: #fef2f2; color: #e24b4a; padding: 0.6rem 1rem; border-radius: 8px; margin-bottom: 1rem; font-size: 0.9rem; }
.campo { margin-bottom: 1rem; }
.campo label { display: block; font-size: 0.85rem; color: #4b5563; margin-bottom: 0.3rem; }
.campo input { width: 100%; padding: 0.7rem; border: 1px solid #e5e7eb; border-radius: 8px; font-size: 0.95rem; }
.btn-submit { width: 100%; padding: 0.8rem; background: #1a1a2e; color: white; border: none; border-radius: 8px; font-size: 1rem; cursor: pointer; margin-top: 0.5rem; }
.link-alterno { text-align: center; margin-top: 1rem; font-size: 0.9rem; color: #6b7280; }
.link-alterno a { color: #1d9e75; }
</style>
```

---

## Pomodoro 14 — MisOrdenesView (25 min)

### Archivo: `src/views/MisOrdenesView.vue`

```vue
<script setup>
import { ref, onMounted } from 'vue'
import api from '@/services/api'

const ordenes = ref([])
const cargando = ref(true)

function formatearPrecio(precio) {
  return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(precio)
}

function formatearFecha(fecha) {
  return new Date(fecha).toLocaleDateString('es-MX', {
    year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit'
  })
}

onMounted(async () => {
  try {
    const { data } = await api.get('/ordenes')
    if (data.exito) {
      ordenes.value = data.datos
    }
  } finally {
    cargando.value = false
  }
})

async function cancelarOrden(id) {
  if (!confirm('¿Seguro que deseas cancelar esta orden?')) return

  try {
    const { data } = await api.put(`/ordenes/${id}/cancelar`)
    if (data.exito) {
      const orden = ordenes.value.find(o => o.id === id)
      if (orden) orden.estado = 'Cancelada'
    }
  } catch (err) {
    alert(err.response?.data?.mensaje || 'Error al cancelar')
  }
}
</script>

<template>
  <div class="ordenes-page">
    <h1>Mis órdenes</h1>

    <p v-if="cargando">Cargando...</p>
    <p v-else-if="ordenes.length === 0" class="sin-ordenes">No tienes órdenes aún.</p>

    <div v-else class="ordenes-lista">
      <div v-for="orden in ordenes" :key="orden.id" class="orden-card">
        <div class="orden-header">
          <span class="orden-numero">{{ orden.numeroOrden || `#${orden.id}` }}</span>
          <span :class="['orden-estado', `estado-${orden.estado.toLowerCase()}`]">
            {{ orden.estado }}
          </span>
        </div>
        <div class="orden-info">
          <span>{{ formatearFecha(orden.fechaCreacion) }}</span>
          <span class="orden-total">{{ formatearPrecio(orden.total) }}</span>
        </div>
        <button
          v-if="orden.estado === 'Pendiente' || orden.estado === 'Pagada'"
          @click="cancelarOrden(orden.id)"
          class="btn-cancelar"
        >
          Cancelar orden
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ordenes-page { max-width: 800px; margin: 0 auto; padding: 2rem; }
.ordenes-page h1 { font-size: 1.8rem; color: #1a1a2e; margin-bottom: 2rem; }
.sin-ordenes { color: #6b7280; text-align: center; padding: 3rem; }
.ordenes-lista { display: flex; flex-direction: column; gap: 1rem; }
.orden-card { background: #fff; border: 1px solid #e5e7eb; border-radius: 12px; padding: 1.2rem 1.5rem; }
.orden-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
.orden-numero { font-weight: 600; color: #1a1a2e; }
.orden-estado { padding: 0.2rem 0.8rem; border-radius: 20px; font-size: 0.8rem; font-weight: 500; }
.estado-pendiente { background: #fef3c7; color: #92400e; }
.estado-pagada { background: #e1f5ee; color: #0f6e56; }
.estado-cancelada { background: #fef2f2; color: #991b1b; }
.estado-enviada { background: #e6f1fb; color: #0c447c; }
.orden-info { display: flex; justify-content: space-between; color: #6b7280; font-size: 0.9rem; }
.orden-total { font-weight: 700; color: #1a1a2e; font-size: 1.1rem; }
.btn-cancelar { margin-top: 0.8rem; background: none; border: 1px solid #e24b4a; color: #e24b4a; padding: 0.4rem 1rem; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
</style>
```

---

## Pomodoro 15 — Página legacy con jQuery (25 min)

### Archivo: `src/EcommerceNet.Web/public/legacy.html`

```html
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>EcommerceNet — Catálogo (jQuery Legacy)</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js"></script>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #f9fafb; color: #1a1a2e; }
        .header { background: #fff; padding: 1rem 2rem; border-bottom: 1px solid #e5e7eb; display: flex; justify-content: space-between; align-items: center; }
        .header h1 { font-size: 1.3rem; }
        .header span { color: #6b7280; font-size: 0.9rem; }
        .container { max-width: 1100px; margin: 0 auto; padding: 2rem; }
        .busqueda { margin-bottom: 2rem; }
        .busqueda input { width: 100%; padding: 0.8rem 1rem; border: 1px solid #e5e7eb; border-radius: 8px; font-size: 1rem; }
        .productos-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 1.5rem; }
        .producto-card { background: #fff; border: 1px solid #e5e7eb; border-radius: 12px; overflow: hidden; transition: box-shadow 0.2s; }
        .producto-card:hover { box-shadow: 0 4px 15px rgba(0,0,0,0.08); }
        .producto-card img { width: 100%; height: 180px; object-fit: cover; }
        .producto-card .info { padding: 1rem; }
        .producto-card .categoria { font-size: 0.75rem; color: #6b7280; text-transform: uppercase; }
        .producto-card .nombre { font-size: 1rem; font-weight: 600; margin: 0.3rem 0; }
        .producto-card .precio { font-size: 1.2rem; font-weight: 700; color: #1d9e75; }
        .producto-card .btn-agregar { width: 100%; margin-top: 0.8rem; padding: 0.5rem; background: #1a1a2e; color: white; border: none; border-radius: 6px; cursor: pointer; }
        .producto-card .btn-agregar:hover { background: #2d2d4e; }
        .cargando { text-align: center; padding: 3rem; color: #6b7280; }
        .error { text-align: center; padding: 2rem; color: #e24b4a; }
        .mensaje { position: fixed; top: 20px; right: 20px; background: #e1f5ee; color: #0f6e56; padding: 0.8rem 1.5rem; border-radius: 8px; display: none; z-index: 999; }
    </style>
</head>
<body>
    <div class="header">
        <h1>EcommerceNet — Catálogo (jQuery)</h1>
        <span>Esta página demuestra el uso de jQuery para consumir la API REST</span>
    </div>

    <div class="container">
        <div class="busqueda">
            <input type="text" id="inputBusqueda" placeholder="Buscar productos..." />
        </div>

        <div id="productosGrid" class="productos-grid">
            <p class="cargando">Cargando productos...</p>
        </div>
    </div>

    <div id="mensaje" class="mensaje"></div>

    <script>
        // URL base de la API
        const API_URL = 'http://localhost:5000/api';

        // Al cargar la página, obtener productos
        $(document).ready(function() {
            cargarProductos();

            // Búsqueda en tiempo real con debounce
            let timer;
            $('#inputBusqueda').on('input', function() {
                clearTimeout(timer);
                const termino = $(this).val().trim();
                timer = setTimeout(function() {
                    if (termino.length >= 2) {
                        buscarProductos(termino);
                    } else {
                        cargarProductos();
                    }
                }, 300);
            });
        });

        // Cargar todos los productos con $.get (AJAX)
        function cargarProductos() {
            $('#productosGrid').html('<p class="cargando">Cargando productos...</p>');

            $.get(API_URL + '/productos')
                .done(function(response) {
                    if (response.exito) {
                        renderizarProductos(response.datos);
                    }
                })
                .fail(function(err) {
                    $('#productosGrid').html(
                        '<p class="error">Error al cargar productos. ¿Está la API corriendo?</p>'
                    );
                });
        }

        // Buscar productos
        function buscarProductos(termino) {
            $('#productosGrid').html('<p class="cargando">Buscando...</p>');

            $.get(API_URL + '/productos/buscar', { termino: termino })
                .done(function(response) {
                    if (response.exito) {
                        renderizarProductos(response.datos);
                    }
                })
                .fail(function() {
                    $('#productosGrid').html('<p class="error">Error al buscar.</p>');
                });
        }

        // Renderizar productos dinámicamente con jQuery
        function renderizarProductos(productos) {
            const $grid = $('#productosGrid');
            $grid.empty();

            if (productos.length === 0) {
                $grid.html('<p class="cargando">No se encontraron productos.</p>');
                return;
            }

            $.each(productos, function(index, p) {
                const precio = new Intl.NumberFormat('es-MX', {
                    style: 'currency', currency: 'MXN'
                }).format(p.precio);

                const $card = $('<div>').addClass('producto-card').html(`
                    <img src="${p.imagenUrl || 'https://placehold.co/400x300?text=Producto'}"
                         alt="${p.nombre}" />
                    <div class="info">
                        <span class="categoria">${p.categoriaNombre || ''}</span>
                        <div class="nombre">${p.nombre}</div>
                        <div class="precio">${precio}</div>
                        <button class="btn-agregar" data-id="${p.id}">
                            Agregar al carrito
                        </button>
                    </div>
                `);

                // Animación de entrada
                $card.hide().appendTo($grid).fadeIn(200 + index * 50);
            });

            // Evento click con delegación de eventos
            $('.btn-agregar').off('click').on('click', function() {
                const productoId = $(this).data('id');
                mostrarMensaje('Para usar el carrito, ve a la versión Vue.js');
            });
        }

        // Mostrar mensaje temporal
        function mostrarMensaje(texto) {
            $('#mensaje').text(texto).fadeIn(300);
            setTimeout(function() {
                $('#mensaje').fadeOut(300);
            }, 3000);
        }
    </script>
</body>
</html>
```

> **Lo que demuestra esta página jQuery:**
> - `$.get()` para llamadas AJAX a la API
> - `$.each()` para iterar arrays
> - `$('<div>').html(...)` para crear elementos dinámicamente
> - `.fadeIn()`, `.fadeOut()` para animaciones
> - `.on('click')` con delegación de eventos
> - `.on('input')` con debounce para búsqueda en tiempo real
> - Manipulación directa del DOM (sin framework reactivo)

---

## Pomodoro 16 — App.vue, probar y merge (25 min)

### Archivo: `src/App.vue`

```vue
<script setup>
import NavBar from '@/components/NavBar.vue'
</script>

<template>
  <NavBar />
  <RouterView />
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

* { margin: 0; padding: 0; box-sizing: border-box; }

body {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  background: #f9fafb;
  color: #1a1a2e;
}
</style>
```

### Probar todo

```powershell
# Terminal 1 — API (ya debe tener la BD del Día 3)
cd C:\Users\ramir\Source\repos\EcommerceNet\src\EcommerceNet.API
dotnet run

# Terminal 2 — Frontend Vue.js
cd C:\Users\ramir\Source\repos\EcommerceNet\src\EcommerceNet.Web
npm run dev

# Abrir http://localhost:5173
# Probar: ver catálogo, buscar, filtrar, login, agregar al carrito, checkout

# jQuery legacy
# Abrir http://localhost:5173/legacy.html
```

### Merge

```powershell
git add .
git commit -m "feat: frontend Vue.js 3 con catálogo, carrito, checkout, auth y página jQuery"
git checkout desarrollo
git merge dia-04/frontend
git push origin desarrollo
git checkout main
git merge desarrollo
git push origin main
```

---

## Simulador de entrevista técnica — Día 4

**Pregunta 1:** "¿Cuál es la diferencia entre Options API y Composition API en Vue.js?"
> "Options API organiza el código por tipo (data, methods, computed, watch) — funciona pero mezcla lógica de diferentes funcionalidades en secciones separadas. Composition API con `<script setup>` organiza el código por funcionalidad — todo lo relacionado al carrito está junto (estado, getters, acciones). Uso Composition API porque es más legible, reutilizable (composables) y tiene mejor soporte de TypeScript. Es el estándar en Vue 3."

**Pregunta 2:** "¿Cómo manejas el estado global en tu aplicación?"
> "Uso Pinia, el store oficial de Vue.js 3. Tengo 3 stores: authStore para autenticación y JWT, productoStore para el catálogo con filtros, y carritoStore para el carrito de compras. Cada store tiene estado reactivo con `ref()`, getters con `computed()` y acciones que llaman a la API con Axios. Pinia reemplazó a Vuex porque es más simple — no tiene mutations, las acciones modifican el estado directamente."

**Pregunta 3:** "¿Tienes experiencia con jQuery? ¿Cuál es la diferencia con Vue.js?"
> "Sí, jQuery se usa en proyectos legacy con ASP.NET MVC donde vive dentro de las vistas Razor para agregar interactividad: AJAX con `$.get()`, manipulación del DOM con selectores, y animaciones. La diferencia fundamental es que jQuery manipula el DOM directamente — tú dices qué cambiar. Vue.js es reactivo — tú defines el estado y Vue actualiza el DOM automáticamente cuando el estado cambia. En mi proyecto EcommerceNet creé una página standalone con jQuery que consume la misma API REST, demostrando que puedo trabajar con ambos."

---

## Mañana: Día 5

Integración final, despliegue en AWS (Las empresas con certificación AWS Partner), CI/CD con GitHub Actions y simulacro completo de entrevista.

Rama: `dia-05/deploy-aws`
