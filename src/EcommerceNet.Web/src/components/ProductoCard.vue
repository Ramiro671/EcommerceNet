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
      <img
        :src="producto.imagenUrl || 'https://placehold.co/400x300?text=Producto'"
        :alt="producto.nombre"
      />
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
.producto-card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  overflow: hidden;
  transition:
    transform 0.2s,
    box-shadow 0.2s;
}
.producto-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.08);
}
.producto-imagen img {
  width: 100%;
  height: 200px;
  object-fit: cover;
}
.producto-info {
  padding: 1rem;
}
.producto-categoria {
  font-size: 0.75rem;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 1px;
}
.producto-nombre {
  display: block;
  font-size: 1rem;
  font-weight: 600;
  color: #1a1a2e;
  text-decoration: none;
  margin: 0.3rem 0;
}
.producto-precio {
  font-size: 1.2rem;
  font-weight: 700;
  color: #1d9e75;
  margin: 0.5rem 0;
}
.producto-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 0.8rem;
}
.stock {
  font-size: 0.8rem;
}
.en-stock {
  color: #1d9e75;
}
.sin-stock {
  color: #e24b4a;
}
.btn-agregar {
  background: #1a1a2e;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.85rem;
}
.btn-agregar:hover {
  background: #2d2d4e;
}
.btn-agregar:disabled {
  background: #d1d5db;
  cursor: not-allowed;
}
</style>
