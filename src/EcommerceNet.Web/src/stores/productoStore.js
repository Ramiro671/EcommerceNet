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
      resultado = resultado.filter((p) => p.categoriaNombre === categoriaSeleccionada.value)
    }

    // Filtrar por término de búsqueda (filtrado local, sin llamar a la API)
    if (terminoBusqueda.value.trim()) {
      const termino = terminoBusqueda.value.toLowerCase()
      resultado = resultado.filter(
        (p) =>
          p.nombre.toLowerCase().includes(termino) ||
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
        const cats = [...new Set(data.datos.map((p) => p.categoriaNombre))]
        categorias.value = cats.filter((c) => c && c !== 'Sin categoría')
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
    productos,
    categorias,
    categoriaSeleccionada,
    terminoBusqueda,
    cargando,
    productosFiltrados,
    totalProductos,
    cargarProductos,
    buscarProductos,
    filtrarPorCategoria,
    limpiarFiltros
  }
})
