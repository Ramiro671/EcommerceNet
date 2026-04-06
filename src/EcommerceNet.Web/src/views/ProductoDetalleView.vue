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
      <img
        :src="producto.imagenUrl || 'https://placehold.co/600x400?text=Producto'"
        :alt="producto.nombre"
        class="detalle-img"
      />

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
          <button @click="agregar" class="btn-agregar-grande">Agregar al carrito</button>
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
.detalle {
  max-width: 1000px;
  margin: 0 auto;
  padding: 2rem;
}
.btn-volver {
  background: none;
  border: none;
  color: #6b7280;
  cursor: pointer;
  margin-bottom: 1rem;
  font-size: 0.95rem;
}
.detalle-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 3rem;
}
.detalle-img {
  width: 100%;
  border-radius: 12px;
}
.detalle-categoria {
  font-size: 0.8rem;
  color: #6b7280;
  text-transform: uppercase;
}
.detalle-info h1 {
  font-size: 1.8rem;
  color: #1a1a2e;
  margin: 0.5rem 0;
}
.detalle-descripcion {
  color: #4b5563;
  line-height: 1.6;
  margin: 1rem 0;
}
.detalle-precio {
  font-size: 2rem;
  font-weight: 700;
  color: #1d9e75;
}
.detalle-acciones {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-top: 1.5rem;
}
.cantidad-selector {
  display: flex;
  align-items: center;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  overflow: hidden;
}
.cantidad-selector button {
  width: 40px;
  height: 40px;
  border: none;
  background: #f9fafb;
  cursor: pointer;
  font-size: 1.2rem;
}
.cantidad-selector span {
  width: 50px;
  text-align: center;
  font-weight: 600;
}
.btn-agregar-grande {
  background: #1a1a2e;
  color: white;
  border: none;
  padding: 0.8rem 2rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
}
.mensaje-exito {
  margin-top: 1rem;
  padding: 0.8rem;
  background: #e1f5ee;
  color: #0f6e56;
  border-radius: 8px;
}
.en-stock {
  color: #1d9e75;
}
.sin-stock {
  color: #e24b4a;
}
.cargando-pagina,
.no-encontrado {
  text-align: center;
  padding: 4rem;
  color: #6b7280;
}
</style>
