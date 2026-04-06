<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const email = ref('')
const password = ref('')

async function iniciarSesion() {
  const exito = await auth.login(email.value, password.value)
  if (exito) {
    const redirect = route.query.redirect || '/'
    router.push(redirect)
  }
}
</script>

<template>
  <div class="auth-page">
    <div class="auth-card">
      <h1>Iniciar sesión</h1>

      <div v-if="auth.error" class="error-msg">{{ auth.error }}</div>

      <div class="campo">
        <label>Email</label>
        <input v-model="email" type="email" placeholder="tu@email.com" />
      </div>

      <div class="campo">
        <label>Contraseña</label>
        <input
          v-model="password"
          type="password"
          placeholder="••••••••"
          @keyup.enter="iniciarSesion"
        />
      </div>

      <button @click="iniciarSesion" :disabled="auth.cargando" class="btn-submit">
        {{ auth.cargando ? 'Cargando...' : 'Iniciar sesión' }}
      </button>

      <p class="link-alterno">
        ¿No tienes cuenta? <RouterLink to="/registro">Regístrate</RouterLink>
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
.btn-submit:disabled {
  background: #9ca3af;
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
