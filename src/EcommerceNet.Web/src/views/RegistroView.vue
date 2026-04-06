<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()

const nombre = ref('')
const email = ref('')
const password = ref('')
const confirmarPassword = ref('')
const errorLocal = ref('')

async function registrarse() {
  errorLocal.value = ''

  if (password.value !== confirmarPassword.value) {
    errorLocal.value = 'Las contraseñas no coinciden'
    return
  }

  if (password.value.length < 6) {
    errorLocal.value = 'La contraseña debe tener al menos 6 caracteres'
    return
  }

  const exito = await auth.registrar(nombre.value, email.value, password.value)
  if (exito) {
    router.push('/')
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <h1>Crear cuenta</h1>

      <div v-if="auth.error || errorLocal" class="error-msg">
        {{ errorLocal || auth.error }}
      </div>

      <div class="campo">
        <label>Nombre</label>
        <input v-model="nombre" type="text" placeholder="Tu nombre" />
      </div>

      <div class="campo">
        <label>Email</label>
        <input v-model="email" type="email" placeholder="tu@email.com" />
      </div>

      <div class="campo">
        <label>Contraseña</label>
        <input v-model="password" type="password" placeholder="Mínimo 6 caracteres" />
      </div>

      <div class="campo">
        <label>Confirmar contraseña</label>
        <input
          v-model="confirmarPassword"
          type="password"
          placeholder="Repetir contraseña"
          @keyup.enter="registrarse"
        />
      </div>

      <button @click="registrarse" :disabled="auth.cargando" class="btn-submit">
        {{ auth.cargando ? 'Cargando...' : 'Crear cuenta' }}
      </button>

      <p class="link-alterno">
        ¿Ya tienes cuenta? <RouterLink to="/login">Inicia sesión</RouterLink>
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 70vh;
  padding: 2rem;
}
.auth-card {
  background: #fff;
  padding: 2.5rem;
  border-radius: 16px;
  border: 1px solid #e5e7eb;
  width: 100%;
  max-width: 420px;
}
.auth-card h1 {
  font-size: 1.5rem;
  color: #1a1a2e;
  margin-bottom: 1.5rem;
  text-align: center;
}
.error-msg {
  background: #fef2f2;
  color: #e24b4a;
  padding: 0.6rem 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}
.campo {
  margin-bottom: 1rem;
}
.campo label {
  display: block;
  font-size: 0.85rem;
  color: #4b5563;
  margin-bottom: 0.3rem;
}
.campo input {
  width: 100%;
  padding: 0.7rem;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.95rem;
}
.btn-submit {
  width: 100%;
  padding: 0.8rem;
  background: #1a1a2e;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
  margin-top: 0.5rem;
}
.link-alterno {
  text-align: center;
  margin-top: 1rem;
  font-size: 0.9rem;
  color: #6b7280;
}
.link-alterno a {
  color: #1d9e75;
}
</style>
