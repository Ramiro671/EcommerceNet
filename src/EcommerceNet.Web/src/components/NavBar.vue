<script setup>
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useCarritoStore } from '@/stores/carritoStore'

const auth = useAuthStore()
const carrito = useCarritoStore()
const router = useRouter()

function cerrarSesion() {
  auth.logout()
  router.push('/')
}
</script>

<template>
  <nav class="navbar">
    <div class="navbar-marca">
      <RouterLink to="/" class="logo">EcommerceNet</RouterLink>
    </div>

    <div class="navbar-links">
      <RouterLink to="/">Tienda</RouterLink>

      <template v-if="auth.estaLogueado">
        <RouterLink to="/mis-ordenes">Mis Órdenes</RouterLink>
        <RouterLink to="/carrito" class="carrito-link">
          Carrito
          <span v-if="carrito.totalItems > 0" class="carrito-badge">
            {{ carrito.totalItems }}
          </span>
        </RouterLink>
        <span class="usuario-nombre">{{ auth.nombreUsuario }}</span>
        <button @click="cerrarSesion" class="btn-logout">Salir</button>
      </template>

      <template v-else>
        <RouterLink to="/login" class="btn-login">Iniciar Sesión</RouterLink>
        <RouterLink to="/registro" class="btn-registro">Registrarse</RouterLink>
      </template>
    </div>
  </nav>
</template>

<style scoped>
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background: #fff;
  border-bottom: 1px solid #e5e7eb;
  position: sticky;
  top: 0;
  z-index: 100;
}

.logo {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1a1a2e;
  text-decoration: none;
}

.navbar-links {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.navbar-links a {
  color: #4b5563;
  text-decoration: none;
  font-size: 0.95rem;
}

.navbar-links a:hover {
  color: #1a1a2e;
}

.carrito-link {
  position: relative;
}

.carrito-badge {
  position: absolute;
  top: -8px;
  right: -12px;
  background: #e24b4a;
  color: white;
  font-size: 0.7rem;
  padding: 2px 6px;
  border-radius: 10px;
  font-weight: 600;
}

.usuario-nombre {
  color: #6b7280;
  font-size: 0.9rem;
}

.btn-logout {
  background: none;
  border: 1px solid #e5e7eb;
  padding: 0.4rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  color: #4b5563;
}

.btn-login,
.btn-registro {
  padding: 0.4rem 1rem;
  border-radius: 6px;
}

.btn-registro {
  background: #1a1a2e;
  color: white !important;
}
</style>
