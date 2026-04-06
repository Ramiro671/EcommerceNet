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
.tienda {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}
.tienda-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}
.tienda-header h1 {
  font-size: 1.8rem;
  color: #1a1a2e;
}
.input-busqueda {
  padding: 0.6rem 1rem;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  width: 300px;
  font-size: 0.95rem;
}
.mensaje-carrito {
  background: #e1f5ee;
  color: #0f6e56;
  padding: 0.8rem 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  text-align: center;
}
.tienda-contenido {
  display: grid;
  grid-template-columns: 220px 1fr;
  gap: 2rem;
}
.productos-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 1.5rem;
}
.cargando,
.sin-resultados {
  grid-column: 1 / -1;
  text-align: center;
  color: #6b7280;
  padding: 3rem;
}
</style>
