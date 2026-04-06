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
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
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
      const orden = ordenes.value.find((o) => o.id === id)
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
.ordenes-page {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}
.ordenes-page h1 {
  font-size: 1.8rem;
  color: #1a1a2e;
  margin-bottom: 2rem;
}
.sin-ordenes {
  color: #6b7280;
  text-align: center;
  padding: 3rem;
}
.ordenes-lista {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.orden-card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.2rem 1.5rem;
}
.orden-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}
.orden-numero {
  font-weight: 600;
  color: #1a1a2e;
}
.orden-estado {
  padding: 0.2rem 0.8rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 500;
}
.estado-pendiente {
  background: #fef3c7;
  color: #92400e;
}
.estado-pagada {
  background: #e1f5ee;
  color: #0f6e56;
}
.estado-cancelada {
  background: #fef2f2;
  color: #991b1b;
}
.estado-enviada {
  background: #e6f1fb;
  color: #0c447c;
}
.orden-info {
  display: flex;
  justify-content: space-between;
  color: #6b7280;
  font-size: 0.9rem;
}
.orden-total {
  font-weight: 700;
  color: #1a1a2e;
  font-size: 1.1rem;
}
.btn-cancelar {
  margin-top: 0.8rem;
  background: none;
  border: 1px solid #e24b4a;
  color: #e24b4a;
  padding: 0.4rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.85rem;
}
</style>
