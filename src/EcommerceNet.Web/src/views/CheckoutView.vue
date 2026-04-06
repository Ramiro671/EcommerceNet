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

          <button @click="confirmarCompra" :disabled="procesando" class="btn-confirmar">
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
.checkout-page {
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem;
}
.checkout-page h1 {
  font-size: 1.8rem;
  color: #1a1a2e;
  margin-bottom: 2rem;
}
.checkout-grid {
  display: grid;
  grid-template-columns: 1fr 350px;
  gap: 2rem;
}
.checkout-form h2,
.checkout-resumen h2 {
  font-size: 1.1rem;
  margin-bottom: 1rem;
  color: #1a1a2e;
}
.input-direccion {
  width: 100%;
  padding: 0.8rem;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.95rem;
  resize: vertical;
  font-family: inherit;
}
.error-msg {
  color: #e24b4a;
  margin: 0.5rem 0;
  font-size: 0.9rem;
}
.btn-confirmar {
  margin-top: 1rem;
  background: #1d9e75;
  color: white;
  border: none;
  padding: 0.8rem 2rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 600;
  width: 100%;
}
.btn-confirmar:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}
.checkout-resumen {
  background: #f9fafb;
  padding: 1.5rem;
  border-radius: 12px;
}
.resumen-item {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  font-size: 0.9rem;
  border-bottom: 1px solid #e5e7eb;
}
.resumen-total {
  display: flex;
  justify-content: space-between;
  padding-top: 1rem;
  font-size: 1.2rem;
  font-weight: 700;
  color: #1d9e75;
}
.orden-confirmada {
  text-align: center;
  padding: 4rem 2rem;
}
.icono-exito {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: #e1f5ee;
  color: #1d9e75;
  font-size: 2.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1.5rem;
}
.btn-ver-ordenes {
  background: #1a1a2e;
  color: white;
  border: none;
  padding: 0.6rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
  margin: 1rem 0.5rem;
}
.btn-seguir {
  background: none;
  border: 1px solid #e5e7eb;
  padding: 0.6rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
}
</style>
