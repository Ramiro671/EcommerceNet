# Clase de Programación — Día 4: Vue.js 3 + jQuery + JavaScript ES6+

> **A quién va dirigido:** desarrollador que ya sabe C# y .NET (Días 1-3) y ahora aprende el frontend.
> Cada concepto nuevo del Día 4 está explicado desde cero: qué es, por qué existe, cómo funciona por dentro.
> Se asume conocimiento básico de HTML y CSS, pero no de frameworks JavaScript.

---

## Índice

1. [Vue.js 3: ¿Qué es una SPA y un framework reactivo?](#1-vuejs-3-qué-es-una-spa-y-un-framework-reactivo)
2. [Composition API: script setup y las funciones fundamentales](#2-composition-api-script-setup-y-las-funciones-fundamentales)
3. [Componentes: defineProps, defineEmits y slots](#3-componentes-defineprops-defineemits-y-slots)
4. [Directivas de template Vue.js](#4-directivas-de-template-vuejs)
5. [Pinia: estado global reactivo](#5-pinia-estado-global-reactivo)
6. [Vue Router: navegación entre páginas](#6-vue-router-navegación-entre-páginas)
7. [Axios: llamadas HTTP con interceptores](#7-axios-llamadas-http-con-interceptores)
8. [localStorage: persistencia en el navegador](#8-localstorage-persistencia-en-el-navegador)
9. [JavaScript ES6+ usado en el proyecto](#9-javascript-es6-usado-en-el-proyecto)
10. [jQuery: manipulación directa del DOM](#10-jquery-manipulación-directa-del-dom)
11. [Comparación jQuery vs Vue.js con código real](#11-comparación-jquery-vs-vuejs-con-código-real)
12. [Análisis línea por línea: api.js](#12-análisis-línea-por-línea-apijs)
13. [Análisis línea por línea: carritoStore.js](#13-análisis-línea-por-línea-carritoStorejs)
14. [Análisis línea por línea: TiendaView.vue](#14-análisis-línea-por-línea-tiendaviewvue)
15. [Análisis línea por línea: ProductoCard.vue](#15-análisis-línea-por-línea-productocardvue)
16. [Análisis línea por línea: CarritoView.vue](#16-análisis-línea-por-línea-carritoviewvue)
17. [Análisis línea por línea: CheckoutView.vue](#17-análisis-línea-por-línea-checkoutviewvue)
18. [Análisis línea por línea: LoginView.vue](#18-análisis-línea-por-línea-loginviewvue)
19. [Análisis línea por línea: router/index.js](#19-análisis-línea-por-línea-routerindexjs)
20. [Análisis línea por línea: legacy.html (jQuery)](#20-análisis-línea-por-línea-legacyhtml-jquery)
21. [Glosario de palabras clave del Día 4](#21-glosario-de-palabras-clave-del-día-4)

---

## 1. Vue.js 3: ¿Qué es una SPA y un framework reactivo?

### ¿Qué es una SPA?

**SPA** = Single Page Application (Aplicación de Una Sola Página).

El navegador descarga **un único archivo HTML** una sola vez. Después, JavaScript actualiza la página sin recargarla completa. Cuando navegas de `/carrito` a `/mis-ordenes`, el navegador **no hace una nueva petición al servidor** — solo JavaScript cambia lo que se muestra.

```
Sitio tradicional (Multi-Page):
Click en "Carrito" → Navegador pide carrito.html → Servidor responde → Página se recarga

SPA (Vue.js):
Click en "Carrito" → Vue Router cambia la URL y muestra CarritoView.vue → NO hay recarga
```

### ¿Qué es la reactividad?

Cuando Vue dice que es "reactivo", significa que el DOM (lo que ves en pantalla) **se actualiza automáticamente** cuando cambia el estado JavaScript.

Sin framework (jQuery/vanilla JS):
```javascript
// Tú manualmente actualizas el DOM cada vez que algo cambia
let cantidad = 0
function agregar() {
  cantidad++
  document.getElementById('contador').textContent = cantidad  // manual
}
```

Con Vue (reactivo):
```javascript
// Solo cambias el dato. Vue actualiza el DOM por ti
const cantidad = ref(0)
function agregar() {
  cantidad.value++  // Vue detecta el cambio y actualiza el DOM automáticamente
}
```

En el template:
```html
<span>{{ cantidad }}</span>  <!-- Se actualiza solo cuando cantidad.value cambia -->
```

### ¿Cómo funciona Vue por dentro?

Vue usa un **Virtual DOM**: una copia en memoria del DOM real. Cuando el estado cambia, Vue recalcula el Virtual DOM, lo compara con el anterior ("diffing"), y solo actualiza **los nodos que cambiaron** — no repinta toda la página. Esto es mucho más rápido que reemplazar todo el HTML.

---

## 2. Composition API: script setup y las funciones fundamentales

### `<script setup>` — El corazón del Día 4

En cada componente `.vue` usamos:
```html
<script setup>
// Todo lo que escribas aquí está disponible en el template automáticamente
const nombre = ref('EcommerceNet')
</script>

<template>
  <h1>{{ nombre }}</h1>
</template>
```

`<script setup>` es un "syntactic sugar" (azúcar sintáctico): Vue lo transforma en el `setup()` function tradicional internamente. La ventaja: menos código y todas las variables son accesibles en el template sin necesidad de `return {}`.

### `ref()` — Estado reactivo para valores simples

```javascript
import { ref } from 'vue'

const contador = ref(0)          // número
const nombre = ref('Ramiro')    // string
const cargando = ref(false)     // boolean
const usuario = ref(null)       // null o un objeto

// Para leer o modificar el valor, usas .value
contador.value++
console.log(nombre.value)       // "Ramiro"

// En el template, Vue desenvuelve .value automáticamente
// <p>{{ contador }}</p>  ← NO necesitas .value en el template
```

**¿Por qué `.value`?** Porque `ref(0)` no guarda el número directamente — crea un objeto `{ value: 0 }`. Esto permite a Vue detectar cuándo cambia.

### `computed()` — Valores derivados

Un `computed` es como una propiedad que se **calcula automáticamente** basándose en otros `ref`s. Se recalcula solo cuando sus dependencias cambian.

```javascript
import { ref, computed } from 'vue'

const items = ref([
  { nombre: 'Laptop', cantidad: 2, precio: 1500 },
  { nombre: 'Mouse', cantidad: 1, precio: 150 }
])

// Se recalcula cada vez que items.value cambia
const total = computed(() =>
  items.value.reduce((sum, item) => sum + item.cantidad * item.precio, 0)
)

console.log(total.value)  // 3150
```

En el proyecto: `productosFiltrados`, `totalItems`, `totalPrecio`, `estaLogueado`, `esAdmin` son todos `computed()`.

### `ref()` vs `reactive()` — La diferencia

```javascript
// ref() → para primitivos y objetos; siempre accedes con .value
const precio = ref(100)
precio.value = 200

// reactive() → solo para objetos; accedes directamente (sin .value)
const usuario = reactive({ nombre: 'Ana', edad: 30 })
usuario.nombre = 'María'  // sin .value
```

**Regla práctica:** En el proyecto usamos siempre `ref()` porque funciona para todo y es más consistente.

### `onMounted()` — Ciclo de vida

```javascript
import { onMounted } from 'vue'

onMounted(() => {
  // Este código se ejecuta DESPUÉS de que el componente se montó en el DOM
  // Es el lugar correcto para cargar datos desde la API
  productoStore.cargarProductos()
})
```

**¿Por qué no cargar datos directamente en `<script setup>`?** Porque el DOM aún no existe cuando `<script setup>` se ejecuta. `onMounted` garantiza que el componente ya está en pantalla.

### `watch()` — Reaccionar a cambios

```javascript
import { ref, watch } from 'vue'

const busqueda = ref('')

watch(busqueda, (valorNuevo, valorAnterior) => {
  console.log(`Cambió de "${valorAnterior}" a "${valorNuevo}"`)
  // Aquí podrías llamar a la API al escribir
})
```

En el proyecto no usamos `watch()` explícitamente porque los filtros se hacen con `computed()` (más eficiente). Pero `watch()` es útil cuando necesitas ejecutar un **efecto secundario** (llamada API, etc.) cuando algo cambia.

---

## 3. Componentes: defineProps, defineEmits y slots

### ¿Qué es un componente?

Un componente es un bloque reutilizable con su propio HTML, JavaScript y CSS. Es como una función pero para la UI.

```
TiendaView.vue
├── NavBar.vue (componente)
├── CategoriaFiltro.vue (componente)
└── ProductoCard.vue (componente, repetido 12 veces con v-for)
```

### `defineProps()` — Recibir datos del padre

El padre pasa datos al hijo con `:prop="valor"`. El hijo los recibe con `defineProps`:

```javascript
// ProductoCard.vue (hijo)
const props = defineProps({
  producto: { type: Object, required: true }
})

// Ahora puedes usar props.producto o simplemente producto en el template
```

```html
<!-- TiendaView.vue (padre) -->
<ProductoCard
  v-for="producto in lista"
  :key="producto.id"
  :producto="producto"        ← aquí "producto" es la prop
/>
```

**Los props son de solo lectura.** El hijo no debe modificarlos directamente — si necesita comunicar algo al padre, usa emits.

### `defineEmits()` — Enviar eventos al padre

```javascript
// ProductoCard.vue (hijo)
const emit = defineEmits(['agregar'])

function agregarAlCarrito() {
  emit('agregar', producto.id)  // le dice al padre qué ID se quiere agregar
}
```

```html
<!-- TiendaView.vue (padre) -->
<ProductoCard
  :producto="p"
  @agregar="agregarAlCarrito"   ← escucha el evento
/>
```

**El flujo completo:**
```
TiendaView → pasa producto como prop → ProductoCard
ProductoCard → emite 'agregar' con productoId → TiendaView
TiendaView → llama carritoStore.agregarProducto(productoId)
```

### Slots — Contenido flexible

Los slots permiten que el padre inyecte HTML dentro del hijo. No se usaron en este proyecto, pero es importante conocerlos:

```html
<!-- MiBoton.vue (hijo) -->
<button class="btn">
  <slot></slot>   <!-- aquí irá el contenido del padre -->
</button>

<!-- Uso en el padre -->
<MiBoton>Agregar al carrito</MiBoton>
<MiBoton>Ver más</MiBoton>
```

---

## 4. Directivas de template Vue.js

Las directivas son atributos especiales de HTML que empiezan con `v-` o `@` o `:`.

### `v-model` — Enlace bidireccional

```html
<input v-model="busqueda" type="text" />
```

Equivale a:
```html
<input :value="busqueda" @input="busqueda = $event.target.value" />
```

Cuando el usuario escribe → `busqueda` se actualiza.
Cuando `busqueda` cambia en JS → el input muestra el nuevo valor.

En el proyecto: `v-model="productoStore.terminoBusqueda"` en TiendaView conecta el input con el store directamente.

### `v-for` con `:key`

```html
<ProductoCard
  v-for="producto in productoStore.productosFiltrados"
  :key="producto.id"
  :producto="producto"
/>
```

**`:key` es obligatorio.** Permite a Vue identificar cada elemento único para actualizaciones eficientes. Si un producto se elimina del array, Vue sabe cuál nodo del DOM remover sin repintar todos.

### `v-if` / `v-else-if` / `v-else`

```html
<div v-if="carrito.estaVacio">Tu carrito está vacío</div>
<div v-else>
  <tabla>...</tabla>
</div>
```

`v-if="false"` **elimina el elemento del DOM** completamente. Es diferente de `v-show="false"` que solo lo oculta con `display: none`.

**Cuándo usar cada uno:**
- `v-if`: cuando el elemento no existe o existe (menos renders)
- `v-show`: cuando el elemento cambia frecuentemente entre visible/oculto (evita recrearlo)

### `@click`, `@input`, `@keyup.enter` — Manejo de eventos

```html
<button @click="cerrarSesion">Salir</button>
<input @keyup.enter="iniciarSesion" />
<input @input="buscar($event.target.value)" />
```

`@click` es shorthand de `v-on:click`.
`.enter` es un **modificador de tecla** — solo dispara cuando se presiona Enter.

### `:class` — Clases dinámicas

```html
<!-- Sintaxis con array y ternario -->
<button :class="['filtro-btn', seleccionada === cat ? 'activo' : '']">
  {{ cat }}
</button>

<!-- Sintaxis con objeto -->
<span :class="{ 'en-stock': producto.stock > 0, 'sin-stock': producto.stock <= 0 }">
```

`:class` es shorthand de `v-bind:class`. El `:` al inicio significa "esta es una expresión JavaScript, no un string literal".

### `:src`, `:alt` — Binding de atributos

```html
<img :src="producto.imagenUrl || 'https://placehold.co/400x300'" :alt="producto.nombre" />
```

Sin `:`, Vue trataría el valor como texto literal. Con `:`, lo evalúa como JavaScript.

---

## 5. Pinia: estado global reactivo

### ¿Qué problema resuelve Pinia?

Sin Pinia, compartir datos entre componentes es complicado:
```
TiendaView necesita saber cuántos items hay en el carrito (para el NavBar)
NavBar necesita el total del carrito
CheckoutView necesita la lista de items
→ ¿Cómo los tres acceden al mismo dato?
```

Con Pinia, el estado vive en un **store global** que cualquier componente puede usar:
```javascript
// En cualquier componente:
const carrito = useCarritoStore()
console.log(carrito.totalItems)  // siempre el mismo dato
```

### `defineStore()` — Crear un store

```javascript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useCarritoStore = defineStore('carrito', () => {
  // Estado (como ref en un componente normal)
  const items = ref([])

  // Getter (computed que calcula algo del estado)
  const totalItems = computed(() =>
    items.value.reduce((sum, item) => sum + item.cantidad, 0)
  )

  // Acción (función que modifica el estado)
  async function agregarProducto(productoId) {
    const { data } = await api.post('/carrito/agregar', { productoId })
    items.value = data.datos.items
  }

  // Todo lo que quieras exponer:
  return { items, totalItems, agregarProducto }
})
```

### Usar el store en un componente

```javascript
// En cualquier componente:
import { useCarritoStore } from '@/stores/carritoStore'

const carrito = useCarritoStore()

// Leer estado:
console.log(carrito.totalItems)
console.log(carrito.items)

// Llamar acciones:
await carrito.agregarProducto(123)
await carrito.vaciar()
```

### Pinia vs Vuex — Por qué Pinia ganó

| Aspecto | Vuex 4 | Pinia 2 |
|---------|--------|---------|
| Modificar estado | Solo con mutations | Directamente en acciones |
| Boilerplate | Alto (state, mutations, actions, getters) | Bajo (todo junto) |
| TypeScript | Difícil de tipar | Excelente inferencia |
| Soporte oficial | Mantenimiento mínimo | Librería oficial Vue 3 |

---

## 6. Vue Router: navegación entre páginas

### `createRouter()` y `createWebHistory()`

```javascript
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),  // URLs limpias: /carrito (no /#/carrito)
  routes: [...]
})
```

`createWebHistory()` usa la **History API** del navegador para URLs reales. La alternativa es `createWebHashHistory()` que usa `/#/ruta` (más simple pero feo).

### Rutas con parámetros dinámicos (`:id`)

```javascript
{
  path: '/producto/:id',
  name: 'producto-detalle',
  component: ProductoDetalleView
}
```

El `:id` es un **parámetro dinámico**. Si navegas a `/producto/7`, `route.params.id` será `"7"`.

```javascript
// En ProductoDetalleView.vue:
import { useRoute } from 'vue-router'
const route = useRoute()
console.log(route.params.id)  // "7"
```

### `useRoute()` y `useRouter()`

```javascript
// useRoute → datos de la ruta actual (parámetros, query, meta)
const route = useRoute()
console.log(route.params.id)          // parámetro dinámico
console.log(route.query.redirect)     // query string: ?redirect=/carrito
console.log(route.meta.requiereAuth)  // meta field

// useRouter → para navegar programáticamente
const router = useRouter()
router.push('/mis-ordenes')          // navegar
router.push({ name: 'login' })      // por nombre de ruta
router.back()                        // volver atrás
```

### `meta` fields — Datos por ruta

```javascript
{
  path: '/carrito',
  component: CarritoView,
  meta: { requiereAuth: true }  // dato personalizado
}
```

Los meta fields permiten agregar cualquier dato a una ruta y leerlo en los guards.

### `router.beforeEach()` — Navigation Guard

Se ejecuta **antes de cada cambio de ruta**:

```javascript
router.beforeEach((to, from, next) => {
  // to: ruta a donde va el usuario
  // from: ruta de donde viene
  // next: función para continuar o redirigir

  const auth = useAuthStore()

  if (to.meta.requiereAuth && !auth.estaLogueado) {
    next({ name: 'login', query: { redirect: to.fullPath } })
    // Redirige a /login?redirect=/carrito
  } else {
    next()  // continúa normalmente
  }
})
```

Es el equivalente frontend del `[Authorize]` attribute del backend en C#.

---

## 7. Axios: llamadas HTTP con interceptores

### `axios.create()` — Instancia personalizada

En lugar de usar `axios` directamente, creamos una instancia con configuración compartida:

```javascript
const api = axios.create({
  baseURL: 'http://localhost:5152/api',
  headers: { 'Content-Type': 'application/json' }
})
```

Esto permite que `api.get('/productos')` sea equivalente a `axios.get('http://localhost:5152/api/productos')`.

### Interceptores — Middleware de Axios

Los interceptores son funciones que se ejecutan para **todas las peticiones**:

```
Componente llama api.post('/login')
          ↓
[request interceptor] agrega JWT al header
          ↓
Petición HTTP sale hacia la API
          ↓
Respuesta HTTP llega
          ↓
[response interceptor] si 401, redirige a login
          ↓
Componente recibe la respuesta
```

**Interceptor de request** (antes de enviar):
```javascript
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwt_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config  // siempre debe retornar config
  },
  (error) => Promise.reject(error)
)
```

**Interceptor de response** (después de recibir):
```javascript
api.interceptors.response.use(
  (response) => response,  // si OK, pasa tal cual
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('jwt_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)
```

### Métodos de Axios

```javascript
// GET — leer datos
const { data } = await api.get('/productos')
const { data } = await api.get(`/productos/${id}`)
const { data } = await api.get('/productos/buscar', { params: { termino: 'laptop' } })

// POST — crear/enviar datos
const { data } = await api.post('/auth/login', { email, password })
const { data } = await api.post('/carrito/agregar', { productoId, cantidad })

// PUT — actualizar
const { data } = await api.put(`/carrito/${productoId}`, { cantidad })

// DELETE — eliminar
await api.delete(`/carrito/${productoId}`)
await api.delete('/carrito')
```

La **desestructuración** `const { data } = await api.get(...)` extrae la propiedad `data` de la respuesta de Axios. La respuesta completa tiene `{ data, status, headers, config, request }`.

---

## 8. localStorage: persistencia en el navegador

### ¿Qué es localStorage?

Es un almacenamiento en el navegador que **persiste aunque se recargue la página**. Es un diccionario de clave-valor donde todo se guarda como string.

```
localStorage ← → EcommerceNet
  "jwt_token" → "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  "usuario"   → '{"nombre":"Ramiro","email":"r@mail.com","rol":"Cliente"}'
```

### Métodos

```javascript
// Guardar
localStorage.setItem('jwt_token', token)
localStorage.setItem('usuario', JSON.stringify({ nombre: 'Ana', rol: 'Admin' }))

// Leer
const token = localStorage.getItem('jwt_token')    // null si no existe
const usuario = JSON.parse(localStorage.getItem('usuario') || 'null')

// Eliminar
localStorage.removeItem('jwt_token')
localStorage.removeItem('usuario')

// Eliminar todo
localStorage.clear()
```

### Por qué `JSON.stringify` y `JSON.parse`

localStorage solo guarda strings. Un objeto JavaScript no es un string. La conversión es:

```javascript
// JavaScript → String (para guardar)
JSON.stringify({ nombre: 'Ana' })  // → '{"nombre":"Ana"}'

// String → JavaScript (para leer)
JSON.parse('{"nombre":"Ana"}')     // → { nombre: 'Ana' }

// Caso especial: si el valor es null
JSON.parse(null)        // → null (válido)
JSON.parse('null')      // → null (válido)
JSON.parse(undefined)   // ← ERROR. Por eso usamos el fallback: || 'null'
localStorage.getItem('usuario') || 'null'  // garantiza que siempre hay un string válido
```

### Por qué usamos localStorage para el JWT

Cuando el usuario hace login, el token llega como respuesta de la API. Si lo guardamos solo en un `ref`, desaparece al recargar la página. localStorage mantiene el token entre sesiones.

**Limitación de seguridad:** localStorage es vulnerable a ataques XSS. En producción real se usa una **cookie HttpOnly** que JavaScript no puede leer. Para este proyecto de demo, localStorage es suficiente.

---

## 9. JavaScript ES6+ usado en el proyecto

### Arrow functions (`=>`)

```javascript
// Función tradicional
function formatearPrecio(precio) {
  return precio.toFixed(2)
}

// Arrow function
const formatearPrecio = (precio) => precio.toFixed(2)

// En arrays
const nombres = productos.map(p => p.nombre)
const activos = productos.filter(p => p.activo)
const total = items.reduce((sum, item) => sum + item.subtotal, 0)
```

### Destructuring (desestructuración)

```javascript
// Objeto
const { data } = await api.get('/productos')
// equivale a:
const respuesta = await api.get('/productos')
const data = respuesta.data

// Array
const [primero, segundo] = [1, 2, 3]

// Con renombre
const { data: respuesta } = await api.get('/productos')
```

### Template literals (backticks)

```javascript
const id = 7
const url = `/productos/${id}`            // → "/productos/7"
const mensaje = `Hola, ${usuario.nombre}` // → "Hola, Ramiro"

// Multilinea
const html = `
  <div class="card">
    <h2>${producto.nombre}</h2>
  </div>
`
```

### Spread operator (`...`)

```javascript
// Expandir un array
const nums = [1, 2, 3]
const mas = [...nums, 4, 5]  // [1, 2, 3, 4, 5]

// Extraer valores únicos con Set:
const categorias = [...new Set(productos.map(p => p.categoriaNombre))]
// new Set() elimina duplicados, [...] lo convierte de vuelta a array
```

### Optional chaining (`?.`)

```javascript
// Sin optional chaining (puede lanzar TypeError)
const nombre = error.response.data.mensaje   // ERROR si response es undefined

// Con optional chaining (retorna undefined si algún eslabón falta)
const nombre = error.response?.data?.mensaje  // nunca lanza error
```

En el proyecto:
```javascript
error.value = err.response?.data?.mensaje || 'Error de conexión'
const esAdmin = computed(() => usuario.value?.rol === 'Admin')
```

### Async/await en el contexto de Vue

```javascript
// En un store de Pinia:
async function login(email, password) {
  cargando.value = true
  try {
    const { data } = await api.post('/auth/login', { email, password })
    if (data.exito) {
      token.value = data.datos.token
      return true
    }
    error.value = data.mensaje
    return false
  } catch (err) {
    error.value = err.response?.data?.mensaje || 'Error de conexión'
    return false
  } finally {
    cargando.value = false  // se ejecuta siempre, haya error o no
  }
}
```

El `try/catch/finally` es el equivalente JavaScript del `try/catch` de C#. El bloque `finally` siempre se ejecuta — perfecto para apagar el estado de carga.

---

## 10. jQuery: manipulación directa del DOM

### ¿Qué es jQuery?

jQuery es una librería JavaScript creada en 2006 que simplifica la manipulación del DOM, las llamadas AJAX y el manejo de eventos. Era el estándar de facto hasta 2015-2016 cuando llegaron los frameworks modernos.

Se carga desde CDN:
```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js"></script>
```

La función global `$` es el punto de entrada de jQuery.

### `$(document).ready()` — Esperar al DOM

```javascript
$(document).ready(function() {
  // Este código corre cuando el HTML está completamente cargado
  cargarProductos()
})

// Shorthand equivalente:
$(function() {
  cargarProductos()
})
```

Equivalente Vue: `onMounted(() => { cargarProductos() })`.

### `$()` — Seleccionar elementos

```javascript
$('#productosGrid')        // por ID (como document.getElementById)
$('.producto-card')        // por clase (como getElementsByClassName)
$('button.btn-agregar')    // selector CSS combinado
$(this)                    // el elemento que disparó el evento
```

### `$.get()` — Llamadas AJAX

```javascript
$.get('http://localhost:5152/api/productos')
  .done(function(response) {
    // éxito: response es el objeto JSON parseado automáticamente
    renderizarProductos(response.datos)
  })
  .fail(function(jqXHR, textStatus) {
    // error
    console.error('Error:', textStatus)
  })
```

Equivalente moderno: `await api.get('/productos')`.

### `$.each()` — Iterar arrays

```javascript
$.each(productos, function(index, producto) {
  console.log(index)      // 0, 1, 2...
  console.log(producto)   // { id: 1, nombre: 'Laptop', ... }
})
```

Equivalente moderno: `productos.forEach((p, i) => ...)`.

### `.html()`, `.empty()`, `.text()`

```javascript
$('#productosGrid').empty()           // vacía el contenido
$('#productosGrid').html('<p>Hola</p>') // reemplaza el contenido
$('#mensaje').text('Producto agregado') // inserta texto (escapa HTML)
```

### `.hide()`, `.fadeIn()`, `.fadeOut()`

```javascript
$card.hide()                    // oculta instantáneamente
$card.appendTo($grid)          // agrega al DOM (oculto)
$card.fadeIn(200 + index * 50) // aparece con animación escalonada
$('#mensaje').fadeOut(300)      // desaparece con animación
```

### `.on()`, `.off()` — Manejo de eventos y delegación

```javascript
// Evento directo (el elemento debe existir cuando se registra)
$('#boton').on('click', function() { ... })

// Delegación de eventos (funciona con elementos creados dinámicamente)
$('#productosGrid').on('click', '.btn-agregar', function() {
  const id = $(this).data('id')  // lee el atributo data-id
  alert('Agregar: ' + id)
})

// El '.btn-agregar' es el selector de delegación
// jQuery captura el click en #productosGrid y verifica si viene de .btn-agregar
```

**Por qué delegación:** Los botones de agregar se crean dinámicamente con jQuery. Si usas `$('.btn-agregar').on('click', ...)`, los botones que aún no existen no tendrán el evento. Con delegación, el evento se pone en el contenedor padre (que siempre existe) y se filtra por el selector del hijo.

### `.off()` — Evitar eventos duplicados

```javascript
// PROBLEMA: si esto se llama 3 veces, cada botón dispara el evento 3 veces
$('#productosGrid').on('click', '.btn-agregar', fn)

// SOLUCIÓN: primero desregistra, luego registra de nuevo
$('#productosGrid').off('click', '.btn-agregar').on('click', '.btn-agregar', fn)
```

### `.data()` — Leer atributos `data-*`

```html
<button class="btn-agregar" data-id="7">Agregar</button>
```

```javascript
$(this).data('id')   // → 7 (número, no string)
// equivalente a $(this).attr('data-id') pero con tipo correcto
```

### Debounce manual con `setTimeout`/`clearTimeout`

```javascript
let timer  // variable fuera para recordar el temporizador anterior

$('#inputBusqueda').on('input', function() {
  clearTimeout(timer)  // cancela la búsqueda anterior si el usuario sigue escribiendo
  const termino = $(this).val().trim()

  timer = setTimeout(function() {
    // Solo ejecuta la búsqueda si el usuario dejó de escribir 300ms
    if (termino.length >= 2) {
      buscarProductos(termino)
    }
  }, 300)
})
```

Esto evita llamar a la API en cada pulsación de tecla. Con 300ms de retraso, solo busca cuando el usuario hace una pausa.

---

## 11. Comparación jQuery vs Vue.js con código real

### Caso 1: Mostrar lista de productos

**jQuery (imperativo):**
```javascript
function renderizarProductos(productos) {
  const $grid = $('#productosGrid')
  $grid.empty()                         // 1. vaciar el contenedor

  $.each(productos, function(i, p) {    // 2. iterar
    const $card = $('<div>').html(`...`) // 3. crear elemento
    $card.hide().appendTo($grid).fadeIn() // 4. agregar al DOM
  })
}
// Tú controlas cada paso: vaciar, crear, insertar, animar
```

**Vue.js (declarativo):**
```html
<ProductoCard
  v-for="producto in productoStore.productosFiltrados"
  :key="producto.id"
  :producto="producto"
/>
```
```javascript
// Cuando productoStore.productosFiltrados cambia, Vue actualiza el DOM automáticamente
// No hay empty(), no hay forEach, no hay appendTo
```

### Caso 2: Buscar en tiempo real

**jQuery:**
```javascript
let timer
$('#busqueda').on('input', function() {
  clearTimeout(timer)
  const termino = $(this).val()
  timer = setTimeout(() => buscar(termino), 300)
})
```

**Vue.js:**
```html
<input v-model="productoStore.terminoBusqueda" />
```
```javascript
// productosFiltrados es un computed() que observa terminoBusqueda automáticamente
// No necesitas event listener, no necesitas debounce (el filtrado es local e instantáneo)
```

### Caso 3: Mostrar/ocultar un mensaje

**jQuery:**
```javascript
// Para mostrar:
$('#mensajeCarrito').text('Producto agregado').show()
// Para ocultar después de 3 segundos:
setTimeout(() => $('#mensajeCarrito').hide(), 3000)
```

**Vue.js:**
```html
<div v-if="carrito.mensaje" class="mensaje-carrito">{{ carrito.mensaje }}</div>
```
```javascript
// En el store:
mensaje.value = 'Producto agregado'
setTimeout(() => (mensaje.value = ''), 3000)
// Vue muestra/oculta el div automáticamente cuando mensaje cambia
```

### Resumen de la filosofía

| Paradigma | jQuery | Vue.js |
|-----------|--------|--------|
| Estilo | **Imperativo**: le dices a la computadora qué hacer paso a paso | **Declarativo**: describes cómo debe verse el resultado |
| DOM | Tú lo manipulas manualmente | Vue lo actualiza automáticamente |
| Estado | Disperso en variables globales o el DOM mismo | Centralizado en stores de Pinia |
| Legibilidad | Difícil de seguir el flujo de datos | Clara separación: estado → template |
| Ideal para | Páginas simples, proyectos legacy | SPAs complejas con mucho estado |

---

## 12. Análisis línea por línea: api.js

```javascript
import axios from 'axios'
// Importa la librería axios instalada con npm

const api = axios.create({
// axios.create() devuelve una nueva instancia de axios con su propia configuración
// Todas las llamadas que hagamos con "api" usarán esta configuración base

  baseURL: 'http://localhost:5152/api',
  // Todas las URLs se concatenan con esta base:
  // api.get('/productos') → GET http://localhost:5152/api/productos

  headers: {
    'Content-Type': 'application/json'
  }
  // Indica al servidor que enviamos JSON en el body (requerido para POST/PUT)
})

api.interceptors.request.use(
// .interceptors.request.use(fn_exito, fn_error) registra funciones que
// se ejecutan ANTES de cada petición saliente

  (config) => {
  // config es el objeto de configuración de la petición (url, method, headers, data...)

    const token = localStorage.getItem('jwt_token')
    // Lee el JWT guardado. Si no hay sesión, devuelve null

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
      // Agrega el header: Authorization: Bearer eyJhbGci...
      // El backend valida este header en el middleware de autenticación
    }
    return config
    // CRÍTICO: siempre debe retornar config, sino la petición se cancela
  },
  (error) => {
    return Promise.reject(error)
    // Si hay error al configurar la petición, lo propagamos
  }
)

api.interceptors.response.use(
// Se ejecuta DESPUÉS de recibir cada respuesta

  (response) => response,
  // Si el status es 2xx (éxito), pasa la respuesta tal cual sin modificar

  (error) => {
    if (error.response?.status === 401) {
    // Optional chaining (?.) porque error.response puede ser undefined
    // si la petición falló antes de llegar al servidor (sin internet, etc.)
    // 401 = Unauthorized: el token expiró o no es válido

      localStorage.removeItem('jwt_token')
      localStorage.removeItem('usuario')
      // Limpia la sesión local

      window.location.href = '/login'
      // Redirige al login. Usamos window.location (no router.push) porque
      // este archivo no está dentro de un componente Vue y no tiene acceso al router
    }
    return Promise.reject(error)
    // Propaga el error para que el componente pueda manejarlo en su try/catch
  }
)

export default api
// Exporta la instancia para usarla en stores y vistas
```

---

## 13. Análisis línea por línea: carritoStore.js

```javascript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'
// @/ es el alias de src/, configurado en vite.config.js

export const useCarritoStore = defineStore('carrito', () => {
// defineStore('carrito', ...) — 'carrito' es el ID del store
// La función de fábrica (setup function) retorna el estado público

  const items = ref([])
  // Array vacío al inicio. Se sincroniza con la BD al llamar cargarCarrito()

  const cargando = ref(false)
  // true mientras hay una petición en vuelo (para mostrar spinners)

  const mensaje = ref('')
  // Texto de notificación temporal (se limpia después de 3 segundos)

  const totalItems = computed(() =>
    items.value.reduce((sum, item) => sum + item.cantidad, 0)
  )
  // reduce: acumula la suma de todas las cantidades
  // Se recalcula automáticamente cada vez que items.value cambia
  // NavBar.vue lee este computed → el badge se actualiza reactivamente

  const totalPrecio = computed(() =>
    items.value.reduce((sum, item) => sum + item.subtotal, 0)
  )
  // item.subtotal viene de la API: precio * cantidad (calculado en el servidor)

  const estaVacio = computed(() => items.value.length === 0)
  // CarritoView.vue usa v-if="carrito.estaVacio" para mostrar mensaje o tabla

  async function agregarProducto(productoId, cantidad = 1) {
  // cantidad = 1 es el valor por defecto si no se pasa
    try {
      const { data } = await api.post('/carrito/agregar', { productoId, cantidad })
      // POST al endpoint del CarritoController
      // El interceptor agrega automáticamente: Authorization: Bearer ...

      if (data.exito) {
        items.value = data.datos.items || []
        // Actualiza el estado local con la respuesta del servidor
        // Usamos la respuesta del servidor (no modificamos localmente) para garantizar
        // que el estado refleja exactamente lo que tiene la BD

        mensaje.value = data.mensaje || 'Producto agregado'
        setTimeout(() => (mensaje.value = ''), 3000)
        // Muestra el mensaje y lo limpia en 3 segundos
        return true
      }
      mensaje.value = data.mensaje || 'Error al agregar'
      return false
    } catch (err) {
      mensaje.value = err.response?.data?.mensaje || 'Error de conexión'
      // Si la API respondió con error, usa su mensaje. Si no (sin internet), mensaje genérico
      return false
    }
  }

  async function actualizarCantidad(productoId, cantidad) {
    if (cantidad <= 0) {
      return eliminarProducto(productoId)
      // Si la cantidad llega a 0 o menos, elimina el producto del carrito
      // Esto maneja el caso cuando el usuario hace clic en "-" cuando ya hay 1
    }
    try {
      const { data } = await api.put(`/carrito/${productoId}`, { cantidad })
      if (data.exito) {
        items.value = data.datos.items || []
      }
    } catch (err) {
      console.error('Error actualizando:', err)
    }
  }

  // ... (resto de acciones: eliminarProducto, vaciar, checkout)

  return {
    items, cargando, mensaje,
    totalItems, totalPrecio, estaVacio,
    cargarCarrito, agregarProducto, actualizarCantidad,
    eliminarProducto, vaciar, checkout
  }
  // Todo lo que no está en el return es privado (no accesible desde componentes)
})
```

---

## 14. Análisis línea por línea: TiendaView.vue

```html
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
// Los stores son singletons: todos los componentes que los usan
// comparten la misma instancia → el mismo estado

onMounted(() => {
  productoStore.cargarProductos()
  // Al montar el componente, carga los 12 productos desde la API
  // La respuesta actualiza productoStore.productos
  // productoStore.productosFiltrados (computed) se recalcula automáticamente
  // ProductoCard se renderiza para cada producto

  if (auth.estaLogueado) {
    carrito.cargarCarrito()
    // Solo carga el carrito si hay sesión iniciada
    // Evita un error 401 si el usuario no está logueado
  }
})

async function agregarAlCarrito(productoId) {
  if (!auth.estaLogueado) {
    alert('Inicia sesión para agregar productos al carrito')
    return
  }
  await carrito.agregarProducto(productoId)
  // Si el store retorna false (error), el mensaje de error ya se puso en carrito.mensaje
  // Si retorna true, el badge del NavBar se actualiza automáticamente (reactividad)
}
</script>

<template>
  <div class="tienda">
    <div class="tienda-header">
      <h1>Catálogo de productos</h1>
      <div class="busqueda">
        <input
          v-model="productoStore.terminoBusqueda"
          <!-- v-model conecta el input directamente con el store -->
          <!-- Cualquier cambio recalcula productoStore.productosFiltrados (computed) -->
          type="text"
          placeholder="Buscar productos..."
        />
      </div>
    </div>

    <div v-if="carrito.mensaje" class="mensaje-carrito">
      {{ carrito.mensaje }}
      <!-- Se muestra cuando carritoStore.mensaje no es vacío -->
      <!-- Vue oculta el div automáticamente cuando mensaje vuelve a '' -->
    </div>

    <div class="tienda-contenido">
      <aside class="sidebar">
        <CategoriaFiltro
          :categorias="productoStore.categorias"
          <!-- Pasa el array de categorías únicas como prop -->
          :seleccionada="productoStore.categoriaSeleccionada"
          <!-- Pasa la categoría actualmente seleccionada -->
          @filtrar="productoStore.filtrarPorCategoria"
          <!-- Cuando el hijo emite 'filtrar', llama al método del store -->
          @limpiar="productoStore.limpiarFiltros"
          <!-- Cuando el hijo emite 'limpiar', limpia los filtros -->
        />
      </aside>

      <main class="productos-grid">
        <p v-if="productoStore.cargando">Cargando productos...</p>
        <!-- Muestra mientras la petición está en vuelo -->

        <p v-else-if="productoStore.productosFiltrados.length === 0">
          No se encontraron productos.
          <!-- Muestra si el filtro no tiene resultados -->
        </p>

        <ProductoCard
          v-for="producto in productoStore.productosFiltrados"
          <!-- Renderiza una tarjeta por cada producto filtrado -->
          :key="producto.id"
          <!-- :key permite a Vue identificar cada tarjeta; importante para rendimiento -->
          :producto="producto"
          <!-- Pasa el objeto completo como prop -->
          @agregar="agregarAlCarrito"
          <!-- Escucha el evento 'agregar' del hijo -->
        />
      </main>
    </div>
  </div>
</template>
```

---

## 15. Análisis línea por línea: ProductoCard.vue

```html
<script setup>
import { RouterLink } from 'vue-router'

const props = defineProps({
  producto: { type: Object, required: true }
  // Vue validará que "producto" siempre se pase y sea un Object
  // Si no se pasa o es el tipo equivocado, Vue muestra una advertencia en consola
})

const emit = defineEmits(['agregar'])
// Declara los eventos que este componente puede emitir
// 'agregar' es el nombre del evento

function formatearPrecio(precio) {
  return new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN'
  }).format(precio)
  // Intl.NumberFormat es una API nativa de JS para formateo internacionalizado
  // 'es-MX' → formato mexicano: $ 1,299.00
}
</script>

<template>
  <div class="producto-card">
    <RouterLink :to="`/producto/${producto.id}`" class="producto-imagen">
    <!-- RouterLink es el <a> de Vue Router (sin recarga de página) -->
    <!-- :to — binding dinámico: crea la URL con el ID del producto -->
      <img
        :src="producto.imagenUrl || 'https://placehold.co/400x300?text=Producto'"
        <!-- || 'placeholder' → fallback si imagenUrl es null o vacío -->
        :alt="producto.nombre"
      />
    </RouterLink>

    <div class="producto-info">
      <span class="producto-categoria">{{ producto.categoriaNombre }}</span>

      <span :class="['stock', producto.stock > 0 ? 'en-stock' : 'sin-stock']">
      <!-- Array binding de :class: siempre agrega 'stock', condicionalmente 'en-stock' o 'sin-stock' -->
        {{ producto.stock > 0 ? `${producto.stock} disponibles` : 'Agotado' }}
        <!-- Operador ternario en el template -->
      </span>

      <button
        @click="emit('agregar', producto.id)"
        <!-- Emite el evento 'agregar' con el ID como argumento -->
        <!-- El padre escucha: @agregar="agregarAlCarrito" → recibe el ID -->
        :disabled="producto.stock <= 0"
        <!-- Si no hay stock, el botón se deshabilita -->
        class="btn-agregar"
      >
        Agregar al carrito
      </button>
    </div>
  </div>
</template>
```

---

## 16. Análisis línea por línea: CarritoView.vue

```html
<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCarritoStore } from '@/stores/carritoStore'

const carrito = useCarritoStore()
const router = useRouter()

onMounted(() => carrito.cargarCarrito())
// Carga el carrito desde la API al entrar a la página
// Garantiza que el estado local coincide con la BD
// (el usuario podría haber agregado algo desde otro dispositivo)
</script>

<template>
  <table class="carrito-tabla">
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
            <!-- Si item.cantidad es 1 y se hace clic, item.cantidad - 1 = 0 -->
            <!-- carritoStore.actualizarCantidad detecta que cantidad ≤ 0 y llama eliminarProducto -->
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
    <span>Total ({{ carrito.totalItems }} productos):</span>
    <!-- carrito.totalItems es un computed() → se actualiza automáticamente -->
    <span class="resumen-total">{{ formatearPrecio(carrito.totalPrecio) }}</span>
    <!-- Mismo: computed() reactivo -->

    <button @click="carrito.vaciar()">Vaciar carrito</button>
    <button @click="router.push('/checkout')">Proceder al pago</button>
    <!-- router.push navega sin recargar la página -->
  </div>
</template>
```

---

## 17. Análisis línea por línea: CheckoutView.vue

```html
<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useCarritoStore } from '@/stores/carritoStore'

const carrito = useCarritoStore()
const router = useRouter()

const direccion = ref('')
// Estado local de este componente (no necesita ser global)

const procesando = ref(false)
// Deshabilita el botón mientras se procesa el pago

const ordenCreada = ref(null)
// null = no hay orden aún; objeto = orden confirmada

const errorCheckout = ref('')
// Mensaje de error del formulario

async function confirmarCompra() {
  if (!direccion.value.trim()) {
    errorCheckout.value = 'Ingresa una dirección de envío'
    return
    // Validación frontend: si el campo está vacío, muestra error y no hace la petición
  }

  procesando.value = true
  // Deshabilita el botón (v-bind:disabled en el template)

  const resultado = await carrito.checkout(direccion.value)
  // El store hace: POST /api/carrito/checkout
  // Si OK: vacía items, retorna { exito: true, orden: {...} }
  // Si error: retorna { exito: false, mensaje: '...' }

  if (resultado.exito) {
    ordenCreada.value = resultado.orden
    // Guardar la orden creada → el template muestra la pantalla de éxito
  } else {
    errorCheckout.value = resultado.mensaje || 'Error al procesar la compra'
  }

  procesando.value = false
}
</script>

<template>
  <div v-if="ordenCreada" class="orden-confirmada">
  <!-- Si ordenCreada tiene valor, muestra la pantalla de éxito -->
    <div class="icono-exito">✓</div>
    <h1>¡Compra realizada!</h1>
    <p>Número de orden: <strong>{{ ordenCreada.numeroOrden }}</strong></p>
    <button @click="router.push('/mis-ordenes')">Ver mis órdenes</button>
  </div>

  <div v-else>
  <!-- Si no hay orden creada, muestra el formulario -->
    <textarea v-model="direccion" placeholder="Dirección completa..."></textarea>
    <!-- v-model en un textarea funciona igual que en input -->

    <button @click="confirmarCompra" :disabled="procesando">
      {{ procesando ? 'Procesando...' : 'Confirmar compra' }}
      <!-- El texto del botón cambia según el estado -->
    </button>
  </div>
</template>
```

---

## 18. Análisis línea por línea: LoginView.vue

```html
<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
// useRoute → lee datos de la URL actual (query params, params, meta)

const email = ref('')
const password = ref('')
// Estado local del formulario

async function iniciarSesion() {
  const exito = await auth.login(email.value, password.value)
  // El store hace: POST /api/auth/login
  // Si OK: guarda token en localStorage y en el store, retorna true
  // Si error: pone el mensaje en auth.error, retorna false

  if (exito) {
    const redirect = route.query.redirect || '/'
    // Si el usuario fue redirigido desde /carrito, route.query.redirect = '/carrito'
    // Si llegó directamente a /login, redirect = '/'
    router.push(redirect)
    // Navega a donde el usuario quería ir originalmente
  }
  // Si !exito, auth.error tiene el mensaje → se muestra en el template
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <div v-if="auth.error" class="error-msg">{{ auth.error }}</div>
      <!-- auth.error es reactivo → aparece automáticamente si el login falla -->

      <input v-model="email" type="email" placeholder="tu@email.com" />
      <input
        v-model="password"
        type="password"
        @keyup.enter="iniciarSesion"
        <!-- El usuario puede hacer login con Enter en lugar de hacer clic -->
      />

      <button @click="iniciarSesion" :disabled="auth.cargando">
        {{ auth.cargando ? 'Cargando...' : 'Iniciar sesión' }}
        <!-- Feedback visual mientras se procesa el login -->
      </button>

      <RouterLink to="/registro">Regístrate</RouterLink>
      <!-- RouterLink genera un <a> que navega sin recargar -->
    </div>
  </div>
</template>
```

---

## 19. Análisis línea por línea: router/index.js

```javascript
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

// Importar todos los componentes de vista
import TiendaView from '@/views/TiendaView.vue'
// ... (resto de imports)

const rutas = [
  {
    path: '/',           // URL
    name: 'tienda',      // Nombre interno (para router.push({ name: 'tienda' }))
    component: TiendaView
  },
  {
    path: '/producto/:id',
    // :id es un parámetro dinámico. Acepta cualquier valor:
    // /producto/1, /producto/7, /producto/99
    name: 'producto-detalle',
    component: ProductoDetalleView
  },
  {
    path: '/carrito',
    name: 'carrito',
    component: CarritoView,
    meta: { requiereAuth: true }
    // meta es un objeto de datos personalizados para esta ruta
    // El guard beforeEach lo leerá para decidir si bloquear el acceso
  }
  // ...
]

const router = createRouter({
  history: createWebHistory(),
  // createWebHistory() → URLs limpias: /carrito
  // createWebHashHistory() → URLs con #: /#/carrito
  // createWebHistory requiere configurar el servidor para redirigir todo a index.html
  // (en Vite dev server esto es automático)

  routes: rutas
})

router.beforeEach((to, from, next) => {
// Se ejecuta antes de CADA cambio de ruta
// to: objeto con la ruta de destino (path, name, meta, params, query)
// from: objeto con la ruta de origen
// next: función para continuar

  const auth = useAuthStore()
  // IMPORTANTE: useAuthStore() aquí funciona porque Pinia ya fue inicializado
  // en main.js (app.use(createPinia())) antes de que el router navegue

  if (to.meta.requiereAuth && !auth.estaLogueado) {
    next({
      name: 'login',
      query: { redirect: to.fullPath }
      // to.fullPath incluye path + query string: "/carrito" o "/checkout?promo=5"
    })
  } else {
    next()
    // Continuar con la navegación normalmente
  }
})

export default router
```

---

## 20. Análisis línea por línea: legacy.html (jQuery)

```javascript
const API_URL = 'http://localhost:5152/api'
// Puerto de la API .NET. En jQuery no hay "interceptores",
// hay que poner la URL base manualmente en cada función

$(document).ready(function() {
// Espera a que el DOM esté completamente cargado
// Equivalente Vue: onMounted(() => { ... })

  cargarProductos()
  // Carga inicial: llama a la función al inicio

  let timer
  // Variable para el debounce: recordar el timeout anterior

  $('#inputBusqueda').on('input', function() {
  // Registra el evento 'input' en el campo de búsqueda
  // Se dispara en cada pulsación de tecla

    clearTimeout(timer)
    // Cancela el timeout anterior (si el usuario sigue escribiendo)

    const termino = $(this).val().trim()
    // $(this) = el elemento que disparó el evento (el input)
    // .val() = obtiene el valor del input
    // .trim() = elimina espacios al inicio y al final

    timer = setTimeout(function() {
    // Programa una función para ejecutar en 300ms
      if (termino.length >= 2) {
        buscarProductos(termino)
        // Solo busca si el término tiene al menos 2 caracteres
        // Evita búsquedas con una sola letra ("a", "e"...)
      } else {
        cargarProductos()
        // Si el campo está casi vacío, carga todos los productos
      }
    }, 300)
  })
})

function renderizarProductos(productos) {
  const $grid = $('#productosGrid')
  // Guarda la referencia con $ al inicio por convención (variable jQuery)

  $grid.empty()
  // Vacía el contenido del grid
  // Sin esto, cada búsqueda acumularía los resultados anteriores

  if (productos.length === 0) {
    $grid.html('<p class="cargando">No se encontraron productos.</p>')
    return
  }

  $.each(productos, function(index, p) {
  // Itera el array. index: posición (0, 1, 2...), p: el producto actual
  // Equivalente moderno: productos.forEach((p, index) => { ... })

    const $card = $('<div>').addClass('producto-card').html(`
    // $('<div>') crea un nuevo elemento div (NO conectado al DOM todavía)
    // .addClass() agrega la clase CSS
    // .html() inserta el template literal como HTML interno

      <button class="btn-agregar" data-id="${p.id}">
      // data-id es un atributo HTML5 personalizado
      // jQuery puede leerlo con $(btn).data('id')
      // El valor se interpola con template literals de JS (${p.id})
    `)

    $card.hide().appendTo($grid).fadeIn(200 + index * 50)
    // .hide()          → oculta el elemento (display: none)
    // .appendTo($grid) → agrega el elemento al DOM (dentro del grid)
    // .fadeIn(miliseg) → animación de aparición. 200 + index*50:
    //   primer producto: fadeIn(200ms)
    //   segundo producto: fadeIn(250ms)
    //   tercer producto: fadeIn(300ms)
    //   → efecto "cascada" escalonado
  })

  $('#productosGrid').off('click', '.btn-agregar').on('click', '.btn-agregar', function() {
  // DELEGACIÓN DE EVENTOS:
  // El listener está en #productosGrid (el contenedor, siempre existe)
  // pero filtra por '.btn-agregar' (los hijos, que se crean dinámicamente)
  // .off() primero para evitar que el evento se duplique si esta función
  // se llama varias veces (por ejemplo, después de una búsqueda)

    const productoId = $(this).data('id')
    // $(this) = el botón que se hizo clic
    // .data('id') lee el atributo data-id y retorna el tipo correcto (número, no string)
  })
}
```

---

## 21. Glosario de palabras clave del Día 4

| Término | Tipo | Definición breve |
|---------|------|-----------------|
| `SPA` | Concepto | Single Page Application — una sola carga HTML, navegación sin recargar |
| `Reactividad` | Concepto | El DOM se actualiza automáticamente cuando el estado cambia |
| `Virtual DOM` | Concepto | Copia en memoria del DOM; Vue la compara para actualizar solo lo necesario |
| `<script setup>` | Sintaxis Vue | Shorthand de la Composition API; las variables se exponen al template automáticamente |
| `ref()` | Función Vue | Crea un valor reactivo; se accede con `.value` en JS, sin `.value` en template |
| `reactive()` | Función Vue | Crea un objeto reactivo; se accede directamente sin `.value` |
| `computed()` | Función Vue | Valor derivado que se recalcula automáticamente cuando sus dependencias cambian |
| `onMounted()` | Función Vue | Hook de ciclo de vida; se ejecuta después de montar el componente |
| `watch()` | Función Vue | Reacciona a cambios de un ref ejecutando una función |
| `defineProps()` | Función Vue | Declara las props que acepta un componente |
| `defineEmits()` | Función Vue | Declara los eventos que puede emitir un componente |
| `v-model` | Directiva | Enlace bidireccional: input ↔ variable |
| `v-for` | Directiva | Renderiza una lista de elementos |
| `v-if` / `v-else` | Directiva | Renderización condicional (agrega/quita del DOM) |
| `v-show` | Directiva | Muestra/oculta con CSS (el elemento siempre existe en el DOM) |
| `@click` | Shorthand | `v-on:click` — escucha un evento del DOM |
| `@keyup.enter` | Directiva + modificador | Escucha la tecla Enter |
| `:class` | Shorthand | `v-bind:class` — aplica clases CSS dinámicamente |
| `:src`, `:href` | Shorthand | `v-bind:src/href` — binding de atributos HTML |
| `RouterLink` | Componente | Equivalente de `<a>` para Vue Router (sin recarga) |
| `RouterView` | Componente | Placeholder donde Vue Router renderiza la vista actual |
| `useRoute()` | Composable | Accede a datos de la ruta actual (params, query, meta) |
| `useRouter()` | Composable | Permite navegar programáticamente (`push`, `back`) |
| `route.params.id` | Propiedad | Parámetro dinámico de la URL (`/producto/:id`) |
| `route.query.redirect` | Propiedad | Query string (`?redirect=/carrito`) |
| `meta.requiereAuth` | Campo meta | Dato personalizado de la ruta, leído por el navigation guard |
| `router.beforeEach()` | Guard | Se ejecuta antes de cada cambio de ruta |
| `next()` | Función guard | Continúa la navegación (o redirige si recibe argumentos) |
| `defineStore()` | Pinia | Crea un store global |
| `axios.create()` | Axios | Crea una instancia de Axios con configuración base |
| `interceptors.request` | Axios | Funciones que modifican peticiones antes de enviarlas |
| `interceptors.response` | Axios | Funciones que procesan respuestas antes de entregarlas |
| `localStorage.setItem()` | Web API | Guarda un par clave-valor en el navegador |
| `localStorage.getItem()` | Web API | Lee un valor guardado (null si no existe) |
| `localStorage.removeItem()` | Web API | Elimina un valor |
| `JSON.stringify()` | JavaScript | Convierte objeto JS → string JSON |
| `JSON.parse()` | JavaScript | Convierte string JSON → objeto JS |
| `?.` | Operador JS | Optional chaining: retorna undefined en vez de lanzar error |
| `||` | Operador JS | OR: retorna el primer valor "truthy" |
| `...` (spread) | Operador JS | Expande un iterable en sus elementos |
| `new Set()` | JavaScript | Colección de valores únicos (sin duplicados) |
| `$()` | jQuery | Selecciona elementos del DOM o crea nuevos |
| `$.get()` | jQuery | Petición AJAX GET |
| `$.each()` | jQuery | Itera un array u objeto |
| `.on()` | jQuery | Registra un evento |
| `.off()` | jQuery | Elimina un evento |
| `.html()` | jQuery | Lee o escribe el HTML interno de un elemento |
| `.empty()` | jQuery | Elimina todo el contenido de un elemento |
| `.fadeIn()` | jQuery | Animación de aparición con opacidad |
| `.fadeOut()` | jQuery | Animación de desaparición con opacidad |
| `.data()` | jQuery | Lee un atributo `data-*` con el tipo correcto |
| `$(this)` | jQuery | El elemento que disparó el evento actual |
| Delegación de eventos | Patrón | Poner el listener en el padre para capturar eventos de hijos dinámicos |
| Debounce | Patrón | Esperar N milisegundos antes de ejecutar una función (evita llamadas excesivas) |
| `Intl.NumberFormat` | JavaScript | API nativa para formateo de números con locale |
