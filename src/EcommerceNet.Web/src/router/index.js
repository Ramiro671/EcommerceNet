import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

import TiendaView from '@/views/TiendaView.vue'
import ProductoDetalleView from '@/views/ProductoDetalleView.vue'
import CarritoView from '@/views/CarritoView.vue'
import CheckoutView from '@/views/CheckoutView.vue'
import MisOrdenesView from '@/views/MisOrdenesView.vue'
import LoginView from '@/views/LoginView.vue'
import RegistroView from '@/views/RegistroView.vue'

const rutas = [
  {
    path: '/',
    name: 'tienda',
    component: TiendaView
  },
  {
    path: '/producto/:id',
    name: 'producto-detalle',
    component: ProductoDetalleView
  },
  {
    path: '/carrito',
    name: 'carrito',
    component: CarritoView,
    meta: { requiereAuth: true }
  },
  {
    path: '/checkout',
    name: 'checkout',
    component: CheckoutView,
    meta: { requiereAuth: true }
  },
  {
    path: '/mis-ordenes',
    name: 'mis-ordenes',
    component: MisOrdenesView,
    meta: { requiereAuth: true }
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView
  },
  {
    path: '/registro',
    name: 'registro',
    component: RegistroView
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes: rutas
})

// GUARD DE NAVEGACIÓN — protege rutas que requieren login
router.beforeEach((to, from, next) => {
  const auth = useAuthStore()

  if (to.meta.requiereAuth && !auth.estaLogueado) {
    // Guardar la ruta destino para redirigir después del login
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else {
    next()
  }
})

export default router
