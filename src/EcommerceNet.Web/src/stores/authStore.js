import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  // --- Estado reactivo ---
  const usuario = ref(JSON.parse(localStorage.getItem('usuario') || 'null'))
  const token = ref(localStorage.getItem('jwt_token') || '')
  const cargando = ref(false)
  const error = ref('')

  // --- Getters (propiedades computadas) ---
  const estaLogueado = computed(() => !!token.value)
  const esAdmin = computed(() => usuario.value?.rol === 'Admin')
  const nombreUsuario = computed(() => usuario.value?.nombre || '')

  // --- Acciones ---

  async function login(email, password) {
    cargando.value = true
    error.value = ''
    try {
      const { data } = await api.post('/auth/login', { email, password })
      if (data.exito) {
        token.value = data.datos.token
        usuario.value = {
          nombre: data.datos.nombre,
          email: data.datos.email,
          rol: data.datos.rol
        }
        localStorage.setItem('jwt_token', data.datos.token)
        localStorage.setItem('usuario', JSON.stringify(usuario.value))
        return true
      } else {
        error.value = data.mensaje || 'Error al iniciar sesión'
        return false
      }
    } catch (err) {
      error.value = err.response?.data?.mensaje || 'Error de conexión'
      return false
    } finally {
      cargando.value = false
    }
  }

  async function registrar(nombre, email, password) {
    cargando.value = true
    error.value = ''
    try {
      const { data } = await api.post('/auth/registrar', { nombre, email, password })
      if (data.exito) {
        token.value = data.datos.token
        usuario.value = {
          nombre: data.datos.nombre,
          email: data.datos.email,
          rol: data.datos.rol
        }
        localStorage.setItem('jwt_token', data.datos.token)
        localStorage.setItem('usuario', JSON.stringify(usuario.value))
        return true
      } else {
        error.value = data.mensaje || 'Error al registrarse'
        return false
      }
    } catch (err) {
      error.value = err.response?.data?.mensaje || 'Error de conexión'
      return false
    } finally {
      cargando.value = false
    }
  }

  function logout() {
    token.value = ''
    usuario.value = null
    localStorage.removeItem('jwt_token')
    localStorage.removeItem('usuario')
  }

  return {
    usuario,
    token,
    cargando,
    error,
    estaLogueado,
    esAdmin,
    nombreUsuario,
    login,
    registrar,
    logout
  }
})
