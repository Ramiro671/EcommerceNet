<script setup>
const props = defineProps({
  categorias: { type: Array, required: true },
  seleccionada: { type: String, default: null }
})

const emit = defineEmits(['filtrar', 'limpiar'])
</script>

<template>
  <div class="filtro">
    <h3>Categorías</h3>
    <button @click="emit('limpiar')" :class="['filtro-btn', !seleccionada ? 'activo' : '']">
      Todas
    </button>
    <button
      v-for="cat in categorias"
      :key="cat"
      @click="emit('filtrar', cat)"
      :class="['filtro-btn', seleccionada === cat ? 'activo' : '']"
    >
      {{ cat }}
    </button>
  </div>
</template>

<style scoped>
.filtro h3 {
  font-size: 1rem;
  margin-bottom: 1rem;
  color: #1a1a2e;
}
.filtro-btn {
  display: block;
  width: 100%;
  text-align: left;
  padding: 0.6rem 1rem;
  border: none;
  background: none;
  cursor: pointer;
  border-radius: 6px;
  margin-bottom: 0.3rem;
  font-size: 0.9rem;
  color: #4b5563;
}
.filtro-btn:hover {
  background: #f3f4f6;
}
.filtro-btn.activo {
  background: #1a1a2e;
  color: white;
}
</style>
