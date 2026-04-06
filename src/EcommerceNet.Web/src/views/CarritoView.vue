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
              <img
                :src="item.imagenUrl || 'https://placehold.co/60x60'"
                :alt="item.productoNombre"
              />
              <span>{{ item.productoNombre }}</span>
            </td>
            <td>{{ formatearPrecio(item.precioUnitario) }}</td>
            <td>
              <div class="cantidad-control">
                <button @click="carrito.actualizarCantidad(item.productoId, item.cantidad - 1)">
                  -
                </button>
                <span>{{ item.cantidad }}</span>
                <button @click="carrito.actualizarCantidad(item.productoId, item.cantidad + 1)">
                  +
                </button>
              </div>
            </td>
            <td class="subtotal">{{ formatearPrecio(item.subtotal) }}</td>
            <td>
              <button @click="carrito.eliminarProducto(item.productoId)" class="btn-eliminar">
                ✕
              </button>
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
.carrito-page {
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem;
}
.carrito-page h1 {
  font-size: 1.8rem;
  color: #1a1a2e;
  margin-bottom: 2rem;
}
.carrito-vacio {
  text-align: center;
  padding: 4rem;
  color: #6b7280;
}
.btn-seguir {
  margin-top: 1rem;
  background: #1a1a2e;
  color: white;
  border: none;
  padding: 0.6rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
}
.carrito-tabla {
  width: 100%;
  border-collapse: collapse;
}
.carrito-tabla th {
  text-align: left;
  padding: 0.8rem;
  border-bottom: 2px solid #e5e7eb;
  color: #6b7280;
  font-size: 0.85rem;
  text-transform: uppercase;
}
.carrito-tabla td {
  padding: 1rem 0.8rem;
  border-bottom: 1px solid #f3f4f6;
}
.producto-cell {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.producto-cell img {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 8px;
}
.cantidad-control {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.cantidad-control button {
  width: 30px;
  height: 30px;
  border: 1px solid #e5e7eb;
  background: #f9fafb;
  border-radius: 6px;
  cursor: pointer;
}
.cantidad-control span {
  font-weight: 600;
  min-width: 30px;
  text-align: center;
}
.subtotal {
  font-weight: 600;
  color: #1a1a2e;
}
.btn-eliminar {
  background: none;
  border: none;
  color: #e24b4a;
  cursor: pointer;
  font-size: 1.2rem;
}
.carrito-resumen {
  margin-top: 2rem;
  padding: 1.5rem;
  background: #f9fafb;
  border-radius: 12px;
}
.resumen-linea {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
.resumen-total {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1d9e75;
}
.resumen-acciones {
  display: flex;
  justify-content: space-between;
}
.btn-vaciar {
  background: none;
  border: 1px solid #e5e7eb;
  padding: 0.6rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
  color: #6b7280;
}
.btn-checkout {
  background: #1d9e75;
  color: white;
  border: none;
  padding: 0.8rem 2rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 600;
}
</style>
