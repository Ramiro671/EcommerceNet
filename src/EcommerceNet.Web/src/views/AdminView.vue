<script setup>
import { ref, onMounted } from 'vue'
import api from '@/services/api'

// --- Tabs ---
const tabActiva = ref('productos')

// =============================================
// PRODUCTOS
// =============================================
const productos = ref([])
const cargandoProductos = ref(false)
const mensajeProducto = ref('')
const errorProducto = ref('')
const mostrarFormProducto = ref(false)
const modoEdicionProducto = ref(false)

const formProducto = ref({
  id: null, nombre: '', descripcion: '', precio: '', stock: '', imagenUrl: '', categoriaId: 1
})

// =============================================
// CATEGORÍAS
// =============================================
const categorias = ref([])
const cargandoCategorias = ref(false)
const mensajeCategoria = ref('')
const errorCategoria = ref('')
const mostrarFormCategoria = ref(false)
const modoEdicionCategoria = ref(false)

const formCategoria = ref({ id: null, nombre: '', descripcion: '' })

// =============================================
// INICIALIZACIÓN
// =============================================
onMounted(() => {
  cargarProductos()
  cargarCategorias()
})

// =============================================
// ACCIONES — CATEGORÍAS
// =============================================
async function cargarCategorias() {
  cargandoCategorias.value = true
  try {
    const { data } = await api.get('/categorias/todas')
    categorias.value = data.datos || []
  } catch {
    errorCategoria.value = 'Error al cargar categorías'
  } finally {
    cargandoCategorias.value = false
  }
}

function abrirNuevaCategoria() {
  modoEdicionCategoria.value = false
  formCategoria.value = { id: null, nombre: '', descripcion: '' }
  mostrarFormCategoria.value = true
  mensajeCategoria.value = ''
  errorCategoria.value = ''
}

function editarCategoria(cat) {
  modoEdicionCategoria.value = true
  formCategoria.value = { id: cat.id, nombre: cat.nombre, descripcion: cat.descripcion }
  mostrarFormCategoria.value = true
  mensajeCategoria.value = ''
  errorCategoria.value = ''
}

function cancelarCategoria() {
  mostrarFormCategoria.value = false
}

async function guardarCategoria() {
  errorCategoria.value = ''
  mensajeCategoria.value = ''
  try {
    const payload = { nombre: formCategoria.value.nombre, descripcion: formCategoria.value.descripcion }
    if (modoEdicionCategoria.value) {
      await api.put(`/categorias/${formCategoria.value.id}`, payload)
      mensajeCategoria.value = 'Categoría actualizada'
    } else {
      await api.post('/categorias', payload)
      mensajeCategoria.value = 'Categoría creada'
    }
    mostrarFormCategoria.value = false
    await cargarCategorias()
  } catch (err) {
    errorCategoria.value = err.response?.data?.mensaje || 'Error al guardar'
  }
}

async function desactivarCategoria(id, nombre) {
  if (!confirm(`¿Desactivar la categoría "${nombre}"?\nLos productos de esta categoría seguirán visibles.`)) return
  errorCategoria.value = ''
  try {
    await api.delete(`/categorias/${id}`)
    mensajeCategoria.value = `"${nombre}" desactivada`
    await cargarCategorias()
  } catch (err) {
    errorCategoria.value = err.response?.data?.mensaje || 'Error al desactivar'
  }
}

// =============================================
// ACCIONES — PRODUCTOS
// =============================================
async function cargarProductos() {
  cargandoProductos.value = true
  try {
    const { data } = await api.get('/productos')
    productos.value = data.datos || []
  } catch {
    errorProducto.value = 'Error al cargar productos'
  } finally {
    cargandoProductos.value = false
  }
}

function abrirNuevoProducto() {
  modoEdicionProducto.value = false
  formProducto.value = {
    id: null, nombre: '', descripcion: '', precio: '', stock: '',
    imagenUrl: '', categoriaId: categorias.value[0]?.id || 1
  }
  mostrarFormProducto.value = true
  mensajeProducto.value = ''
  errorProducto.value = ''
}

function editarProducto(p) {
  modoEdicionProducto.value = true
  const cat = categorias.value.find(c => c.nombre === p.categoriaNombre)
  formProducto.value = {
    id: p.id, nombre: p.nombre, descripcion: p.descripcion || '',
    precio: p.precio, stock: p.stock, imagenUrl: p.imagenUrl || '',
    categoriaId: cat?.id || categorias.value[0]?.id || 1
  }
  mostrarFormProducto.value = true
  mensajeProducto.value = ''
  errorProducto.value = ''
}

function cancelarProducto() {
  mostrarFormProducto.value = false
}

async function guardarProducto() {
  errorProducto.value = ''
  mensajeProducto.value = ''
  const payload = {
    nombre: formProducto.value.nombre,
    descripcion: formProducto.value.descripcion,
    precio: parseFloat(formProducto.value.precio),
    stock: parseInt(formProducto.value.stock),
    imagenUrl: formProducto.value.imagenUrl,
    categoriaId: parseInt(formProducto.value.categoriaId)
  }
  try {
    if (modoEdicionProducto.value) {
      await api.put(`/productos/${formProducto.value.id}`, payload)
      mensajeProducto.value = 'Producto actualizado'
    } else {
      await api.post('/productos', payload)
      mensajeProducto.value = 'Producto creado'
    }
    mostrarFormProducto.value = false
    await cargarProductos()
  } catch (err) {
    errorProducto.value = err.response?.data?.mensaje || 'Error al guardar'
  }
}

async function eliminarProducto(id, nombre) {
  if (!confirm(`¿Eliminar "${nombre}"?`)) return
  errorProducto.value = ''
  try {
    await api.delete(`/productos/${id}`)
    mensajeProducto.value = `"${nombre}" eliminado`
    await cargarProductos()
  } catch (err) {
    errorProducto.value = err.response?.data?.mensaje || 'Error al eliminar'
  }
}
</script>

<template>
  <div class="admin-container">
    <div class="admin-header">
      <h1>Panel de Administración</h1>
    </div>

    <!-- Tabs -->
    <div class="tabs">
      <button :class="['tab', { activa: tabActiva === 'productos' }]" @click="tabActiva = 'productos'">
        Productos ({{ productos.length }})
      </button>
      <button :class="['tab', { activa: tabActiva === 'categorias' }]" @click="tabActiva = 'categorias'">
        Categorías ({{ categorias.length }})
      </button>
    </div>

    <!-- ======================== TAB PRODUCTOS ======================== -->
    <div v-if="tabActiva === 'productos'">
      <div class="seccion-header">
        <h2>Gestión de Productos</h2>
        <button class="btn-nuevo" @click="abrirNuevoProducto">+ Nuevo producto</button>
      </div>

      <div v-if="mensajeProducto" class="alerta-exito">{{ mensajeProducto }}</div>
      <div v-if="errorProducto" class="alerta-error">{{ errorProducto }}</div>

      <!-- Formulario producto -->
      <div v-if="mostrarFormProducto" class="formulario-card">
        <h3>{{ modoEdicionProducto ? 'Editar producto' : 'Nuevo producto' }}</h3>
        <form @submit.prevent="guardarProducto" class="formulario">
          <div class="campo">
            <label>Nombre *</label>
            <input v-model="formProducto.nombre" required placeholder="Nombre del producto" />
          </div>
          <div class="campo">
            <label>Descripción</label>
            <textarea v-model="formProducto.descripcion" rows="2" placeholder="Descripción"></textarea>
          </div>
          <div class="fila-dos">
            <div class="campo">
              <label>Precio * ($)</label>
              <input v-model="formProducto.precio" type="number" step="0.01" min="0.01" required placeholder="0.00" />
            </div>
            <div class="campo">
              <label>Stock *</label>
              <input v-model="formProducto.stock" type="number" min="0" required placeholder="0" />
            </div>
          </div>
          <div class="campo">
            <label>Categoría *</label>
            <select v-model="formProducto.categoriaId">
              <option v-for="cat in categorias.filter(c => c.activa)" :key="cat.id" :value="cat.id">
                {{ cat.nombre }}
              </option>
            </select>
          </div>
          <div class="campo">
            <label>URL de imagen</label>
            <input v-model="formProducto.imagenUrl" placeholder="https://..." />
          </div>
          <div class="formulario-acciones">
            <button type="button" class="btn-cancelar" @click="cancelarProducto">Cancelar</button>
            <button type="submit" class="btn-guardar">
              {{ modoEdicionProducto ? 'Actualizar' : 'Crear' }}
            </button>
          </div>
        </form>
      </div>

      <!-- Tabla productos -->
      <div v-if="cargandoProductos" class="cargando">Cargando productos...</div>
      <table v-else class="tabla">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Categoría</th>
            <th>Precio</th>
            <th>Stock</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in productos" :key="p.id">
            <td>{{ p.id }}</td>
            <td>{{ p.nombre }}</td>
            <td>{{ p.categoriaNombre }}</td>
            <td>${{ p.precio.toFixed(2) }}</td>
            <td>{{ p.stock }}</td>
            <td class="acciones">
              <button class="btn-editar" @click="editarProducto(p)">Editar</button>
              <button class="btn-eliminar" @click="eliminarProducto(p.id, p.nombre)">Eliminar</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ======================== TAB CATEGORÍAS ======================== -->
    <div v-if="tabActiva === 'categorias'">
      <div class="seccion-header">
        <h2>Gestión de Categorías</h2>
        <button class="btn-nuevo" @click="abrirNuevaCategoria">+ Nueva categoría</button>
      </div>

      <div v-if="mensajeCategoria" class="alerta-exito">{{ mensajeCategoria }}</div>
      <div v-if="errorCategoria" class="alerta-error">{{ errorCategoria }}</div>

      <!-- Formulario categoría -->
      <div v-if="mostrarFormCategoria" class="formulario-card">
        <h3>{{ modoEdicionCategoria ? 'Editar categoría' : 'Nueva categoría' }}</h3>
        <form @submit.prevent="guardarCategoria" class="formulario">
          <div class="campo">
            <label>Nombre *</label>
            <input v-model="formCategoria.nombre" required placeholder="Nombre de la categoría" />
          </div>
          <div class="campo">
            <label>Descripción</label>
            <textarea v-model="formCategoria.descripcion" rows="2" placeholder="Descripción"></textarea>
          </div>
          <div class="formulario-acciones">
            <button type="button" class="btn-cancelar" @click="cancelarCategoria">Cancelar</button>
            <button type="submit" class="btn-guardar">
              {{ modoEdicionCategoria ? 'Actualizar' : 'Crear' }}
            </button>
          </div>
        </form>
      </div>

      <!-- Tabla categorías -->
      <div v-if="cargandoCategorias" class="cargando">Cargando categorías...</div>
      <table v-else class="tabla">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Descripción</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="cat in categorias" :key="cat.id">
            <td>{{ cat.id }}</td>
            <td>{{ cat.nombre }}</td>
            <td class="descripcion-col">{{ cat.descripcion }}</td>
            <td>
              <span :class="['badge', cat.activa ? 'badge-activa' : 'badge-inactiva']">
                {{ cat.activa ? 'Activa' : 'Inactiva' }}
              </span>
            </td>
            <td class="acciones">
              <button class="btn-editar" @click="editarCategoria(cat)">Editar</button>
              <button v-if="cat.activa" class="btn-eliminar" @click="desactivarCategoria(cat.id, cat.nombre)">
                Desactivar
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.admin-container {
  max-width: 1100px;
  margin: 2rem auto;
  padding: 0 1.5rem;
}

.admin-header h1 {
  font-size: 1.6rem;
  font-weight: 700;
  color: #1a1a2e;
  margin-bottom: 1.5rem;
}

/* Tabs */
.tabs {
  display: flex;
  gap: 0;
  border-bottom: 2px solid #e5e7eb;
  margin-bottom: 1.5rem;
}

.tab {
  padding: 0.75rem 1.5rem;
  border: none;
  background: none;
  cursor: pointer;
  font-size: 0.95rem;
  color: #6b7280;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
}

.tab.activa {
  color: #1a1a2e;
  font-weight: 600;
  border-bottom-color: #1a1a2e;
}

/* Sección */
.seccion-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.seccion-header h2 {
  font-size: 1.1rem;
  color: #1a1a2e;
}

.btn-nuevo {
  background: #1a1a2e;
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
}

/* Alertas */
.alerta-exito {
  background: #d1fae5;
  color: #065f46;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
}

.alerta-error {
  background: #fee2e2;
  color: #991b1b;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
}

/* Formulario */
.formulario-card {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
}

.formulario-card h3 {
  font-size: 1rem;
  margin-bottom: 1rem;
  color: #1a1a2e;
}

.formulario { display: flex; flex-direction: column; gap: 0.75rem; }
.campo { display: flex; flex-direction: column; gap: 0.25rem; }
.campo label { font-size: 0.85rem; font-weight: 600; color: #374151; }

.campo input,
.campo textarea,
.campo select {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.95rem;
}

.fila-dos { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }

.formulario-acciones {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.btn-cancelar {
  background: white;
  border: 1px solid #d1d5db;
  padding: 0.5rem 1.2rem;
  border-radius: 6px;
  cursor: pointer;
}

.btn-guardar {
  background: #059669;
  color: white;
  border: none;
  padding: 0.5rem 1.2rem;
  border-radius: 6px;
  cursor: pointer;
}

/* Tabla */
.cargando { text-align: center; color: #6b7280; padding: 2rem; }

.tabla {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.tabla th {
  background: #1a1a2e;
  color: white;
  padding: 0.75rem 1rem;
  text-align: left;
}

.tabla td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #e5e7eb;
}

.tabla tr:hover td { background: #f9fafb; }

.descripcion-col {
  max-width: 280px;
  color: #6b7280;
  font-size: 0.85rem;
}

.acciones { display: flex; gap: 0.5rem; }

.btn-editar {
  background: #3b82f6;
  color: white;
  border: none;
  padding: 0.35rem 0.8rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
}

.btn-eliminar {
  background: #ef4444;
  color: white;
  border: none;
  padding: 0.35rem 0.8rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
}

.badge {
  padding: 0.2rem 0.7rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 500;
}

.badge-activa { background: #d1fae5; color: #065f46; }
.badge-inactiva { background: #f3f4f6; color: #6b7280; }
</style>
