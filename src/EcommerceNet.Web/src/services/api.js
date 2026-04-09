import axios from 'axios'

// URL de la API — usa variable de entorno de Vite si existe, o localhost para desarrollo
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5152/api'

// Crear instancia de Axios con la URL base de la API
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// INTERCEPTOR DE REQUEST — agrega el JWT a cada petición automáticamente
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwt_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// INTERCEPTOR DE RESPONSE — maneja errores globalmente
api.interceptors.response.use(
  (response) => response, // si todo OK, pasar la respuesta tal cual
  (error) => {
    // Si el token expiró (401), redirigir a login
    if (error.response?.status === 401) {
      localStorage.removeItem('jwt_token')
      localStorage.removeItem('usuario')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
