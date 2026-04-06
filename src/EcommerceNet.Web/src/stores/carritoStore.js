import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export const useCarritoStore = defineStore('carrito', () => {
  // Estado
  const items = ref([])
  const cargando = ref(false)
  const mensaje = ref('')

  // Getters
  const totalItems = computed(() => items.value.reduce((sum, item) => sum + item.cantidad, 0))

  const totalPrecio = computed(() => items.value.reduce((sum, item) => sum + item.subtotal, 0))

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
        setTimeout(() => (mensaje.value = ''), 3000)
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
    if (cantidad <= 0) {
      return eliminarProducto(productoId)
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

  async function eliminarProducto(productoId) {
    try {
      const { data } = await api.delete(`/carrito/${productoId}`)
      if (data.exito) {
        items.value = data.datos.items || []
        mensaje.value = 'Producto eliminado del carrito'
        setTimeout(() => (mensaje.value = ''), 3000)
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
    items,
    cargando,
    mensaje,
    totalItems,
    totalPrecio,
    estaVacio,
    cargarCarrito,
    agregarProducto,
    actualizarCantidad,
    eliminarProducto,
    vaciar,
    checkout
  }
})
