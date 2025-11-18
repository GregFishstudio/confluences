<template>
  <div class="login-container">
    <!-- Background avec éléments décoratifs -->
    <div class="background-elements">
      <div class="circle circle-1"></div>
      <div class="circle circle-2"></div>
      <div class="circle circle-3"></div>
    </div>

    <div class="login-content">
      <!-- Carte de connexion principale -->
      <div class="login-card">
        <!-- En-tête avec logo --><div class="logo-container">
                  <div class="logo">
                      <img :src="logoImage" alt="Logo Confluences" class="logo-img"/>
                  </div></div>
        <div class="login-header">
        
          <div class="text-center mb-2">
            <h2 class="welcome-title">Content de vous revoir</h2>
            <p class="welcome-subtitle">Connectez-vous à votre compte</p>
          </div>
        
      </div>
        <!-- Formulaire -->
        <form @submit.prevent="handleLogin" class="login-form">
          <!-- Champ Nom d'utilisateur -->
          <div class="input-group">
            <div class="input-icon">
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M10 10C12.3012 10 14.1667 8.13452 14.1667 5.83333C14.1667 3.53215 12.3012 1.66667 10 1.66667C7.69882 1.66667 5.83334 3.53215 5.83334 5.83333C5.83334 8.13452 7.69882 10 10 10Z" stroke="#6B7280" stroke-width="1.5"/>
                <path d="M17.1583 18.3333C17.1583 15.1083 13.95 12.5 10 12.5C6.05001 12.5 2.84167 15.1083 2.84167 18.3333" stroke="#6B7280" stroke-width="1.5" stroke-linecap="round"/>
              </svg>
            </div>
            <input
              v-model="username"
              type="text"
              placeholder="Nom d'utilisateur"
              required
              class="form-input"
              :disabled="loading"
            />
          </div>

          <!-- Champ Mot de passe -->
          <div class="input-group">
            <div class="input-icon">
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M15.8333 9.16667H4.16667C3.24619 9.16667 2.5 9.91286 2.5 10.8333V16.6667C2.5 17.5871 3.24619 18.3333 4.16667 18.3333H15.8333C16.7538 18.3333 17.5 17.5871 17.5 16.6667V10.8333C17.5 9.91286 16.7538 9.16667 15.8333 9.16667Z" stroke="#6B7280" stroke-width="1.5"/>
                <path d="M5.83334 9.16667V5.83333C5.83334 4.72827 6.27233 3.66846 7.05373 2.88706C7.83514 2.10565 8.89495 1.66667 10 1.66667C11.1051 1.66667 12.1649 2.10565 12.9463 2.88706C13.7277 3.66846 14.1667 4.72827 14.1667 5.83333V9.16667" stroke="#6B7280" stroke-width="1.5"/>
              </svg>
            </div>
            <input
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Mot de passe"
              required
              class="form-input"
              :disabled="loading"
            />
            <button
              type="button"
              class="password-toggle"
              @click="showPassword = !showPassword"
            >
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M1.66666 10C1.66666 10 4.16666 4.16667 10 4.16667C15.8333 4.16667 18.3333 10 18.3333 10C18.3333 10 15.8333 15.8333 10 15.8333C4.16666 15.8333 1.66666 10 1.66666 10Z" stroke="#6B7280" stroke-width="1.5"/>
                <path d="M10 12.5C11.3807 12.5 12.5 11.3807 12.5 10C12.5 8.61929 11.3807 7.5 10 7.5C8.61929 7.5 7.5 8.61929 7.5 10C7.5 11.3807 8.61929 12.5 10 12.5Z" stroke="#6B7280" stroke-width="1.5"/>
              </svg>
            </button>
          </div>

          <!-- Options supplémentaires -->
          <div class="form-options">
            <label class="remember-me">
              <input type="checkbox" v-model="rememberMe" />
              <span class="checkmark"></span>
              Se souvenir de moi
            </label>
            <a href="#" class="forgot-password">Mot de passe oublié ?</a>
          </div>

          <!-- Bouton de connexion -->
          <button
            :disabled="loading"
            type="submit"
            class="login-button"
            :class="{ 'loading': loading }"
          >
            <span v-if="!loading">Se connecter</span>
            <div v-else class="button-loader">
              <div class="spinner"></div>
              <span>Connexion...</span>
            </div>
          </button>

          <!-- Message d'erreur -->
          <div v-if="error" class="error-message">
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M10 6.66667V10M10 13.3333H10.0083M18.3333 10C18.3333 14.6024 14.6024 18.3333 10 18.3333C5.39763 18.3333 1.66667 14.6024 1.66667 10C1.66667 5.39763 5.39763 1.66667 10 1.66667C14.6024 1.66667 18.3333 5.39763 18.3333 10Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
            <span>{{ error }}</span>
          </div>
        </form>

        <!-- Séparateur 
        <div class="separator">
          <span>Ou continuer avec</span>
        </div>-->

        <!-- Options de connexion alternatives 
        <div class="social-login">
          <button type="button" class="social-button google">
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M18.1712 10.2041C18.1712 9.56647 18.1167 9.11247 18.0001 8.64087H10.1631V11.3909H14.8431C14.7554 12.1125 14.2629 13.3209 13.1104 14.1125L13.0938 14.2241L15.5079 16.1165L15.6738 16.1341C17.2854 14.6665 18.1712 12.5875 18.1712 10.2041Z" fill="#4285F4"/>
              <path d="M10.1625 18.0001C12.3875 18.0001 14.2583 17.2784 15.6742 16.1342L13.1108 14.1127C12.4112 14.5877 11.4458 14.9034 10.1625 14.9034C7.9875 14.9034 6.15417 13.5127 5.52417 11.5784L5.41625 11.5877L2.90375 13.5459L2.87083 13.6477C4.27708 16.4459 7.02917 18.0001 10.1625 18.0001Z" fill="#34A853"/>
              <path d="M5.52333 11.5783C5.34833 11.0883 5.24583 10.5633 5.24583 10.0199C5.24583 9.47655 5.34833 8.95155 5.5125 8.46155L5.5075 8.34272L2.95333 6.34506L2.87083 6.39222C2.33625 7.46389 2.03125 8.65889 2.03125 9.99989C2.03125 11.3409 2.33625 12.5359 2.87083 13.6076L5.52333 11.5783Z" fill="#FBBC05"/>
              <path d="M10.1625 5.09663C11.7208 5.09663 12.8083 5.78896 13.4458 6.37829L15.7271 4.18163C14.2492 2.83329 12.3875 2 10.1625 2C7.02917 2 4.27708 3.55413 2.87083 6.39246L5.5125 8.46163C6.15417 6.52746 7.9875 5.09663 10.1625 5.09663Z" fill="#EB4335"/>
            </svg>
            Google
          </button>
          <button type="button" class="social-button microsoft">
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M9.16667 2.5H2.5V9.16667H9.16667V2.5Z" fill="#F1511B"/>
              <path d="M17.5 2.5H10.8333V9.16667H17.5V2.5Z" fill="#80CC28"/>
              <path d="M9.16667 10.8333H2.5V17.5H9.16667V10.8333Z" fill="#00ADEF"/>
              <path d="M17.5 10.8333H10.8333V17.5H17.5V10.8333Z" fill="#FBBC09"/>
            </svg>
            Microsoft
          </button>
        </div>-->

   
      </div>

      <!-- Footer -->
      <div class="login-footer">
        <p>© 2025 Confluences. Tous droits réservés.</p>
        <div class="footer-links">
          <a href="#">Confidentialité</a>
          <a href="#">Conditions</a>
          <a href="#">Support</a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { UserManager, WebStorageStateStore } from 'oidc-client-ts'
import { useAuthStore } from '@/store/auth'
import logoImage from '@/assets/logo.png'

const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const showPassword = ref(false)
const rememberMe = ref(false)

const router = useRouter()
const route = useRoute()
const loading = ref(false)
const error = ref('')

// 🔧 Configuration OIDC CENTRALISÉE et COHÉRENTE
const getOidcConfig = () => {
  const authority = import.meta.env.VITE_AUTHORITY_URL || 'http://localhost:5000'
  const clientId = import.meta.env.VITE_CLIENT_ID || 'gestion-stagiaire'
  
  return {
    authority,
    client_id: clientId,
    redirect_uri: `${window.location.origin}/callback`,
    response_type: 'code',
    scope: 'openid profile api1 roles',
    userStore: new WebStorageStateStore({ store: window.localStorage }),
    loadUserInfo: true,
    // Important pour éviter les mismatches
    filterProtocolClaims: true,
    monitorSession: false
  }
}

// Instance unique du UserManager
let mgr = null

const getUserManager = () => {
  if (!mgr) {
    mgr = new UserManager(getOidcConfig())
  }
  return mgr
}

onMounted(async () => {
  const returnUrl = route.query.returnUrl || '/entreprises'
  console.log('📍 Page Login - returnUrl:', returnUrl)

  // Si une session existe déjà → passe directement à la page interne
  const sessionExists = await checkExistingSession()
  if (sessionExists) return

  // Si on a été redirigé par IdentityServer (avec returnUrl fourni)
  if (route.query.returnUrl) {
    console.log('🔗 Redirection automatique vers IdentityServer pour authentification...')
    await handleLogin()
  }
})


function checkExistingSession() {
  const user = JSON.parse(localStorage.getItem('user'))
  if (user && user.expires_at > Date.now()) {
    authStore.$patch({
      token: user.access_token,
      refreshToken: user.refresh_token,
      user,
      expiresAt: user.expires_at
    })
    router.push(route.query.returnUrl || '/entreprises')
    return true
  }
  return false
}


async function handleLogin() {
  loading.value = true
  error.value = ''
  try {
    const authority = import.meta.env.VITE_AUTHORITY_URL || 'http://localhost:5000'
    const response = await fetch(`${authority}/connect/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({
        grant_type: 'password',
        username: username.value,
        password: password.value,
        client_id: 'gestion-stagiaire',
        scope: 'openid profile api1 roles offline_access'
      })
    })

    if (!response.ok) throw new Error('Identifiants invalides')
    const data = await response.json()
    console.log('✅ Token reçu:', data)

    const user = {
      access_token: data.access_token,
      refresh_token: data.refresh_token,
      expires_at: Date.now() + data.expires_in * 1000,
      profile: { role: 'Teacher' }
    }

    localStorage.setItem('user', JSON.stringify(user))
    localStorage.setItem('access_token', data.access_token)
    localStorage.setItem('refresh_token', data.refresh_token)
    localStorage.setItem('expires_at', user.expires_at)

    // 🧠 Mets à jour directement le store Pinia
    authStore.$patch({
      token: data.access_token,
      refreshToken: data.refresh_token,
      user,
      expiresAt: user.expires_at
    })

    router.push('/entreprises')
  } catch (err) {
    console.error('❌ Erreur login:', err)
    error.value = 'Nom d’utilisateur ou mot de passe incorrect'
  } finally {
    loading.value = false
  }
}


</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #e5f1ff 0%, #f0f7ff 100%);
  padding: 20px;
  position: relative;
  overflow: hidden;
}

.background-elements {
  position: absolute;
  width: 100%;
  height: 100%;
  pointer-events: none;
}

.circle {
  position: absolute;
  border-radius: 50%;
  background: rgba(59, 130, 246, 0.05);
}

.circle-1 {
  width: 300px;
  height: 300px;
  top: -150px;
  right: -150px;
}

.circle-2 {
  width: 200px;
  height: 200px;
  bottom: -100px;
  left: -100px;
}

.circle-3 {
  width: 150px;
  height: 150px;
  top: 50%;
  left: 10%;
}

.login-content {
  width: 100%;
  max-width: 440px;
  z-index: 10;
}

.login-card {
  background: white;
  padding: 40px 32px;
  border-radius: 24px;
  box-shadow: 
    0 20px 60px rgba(0, 0, 0, 0.08),
    0 0 0 1px rgba(0, 0, 0, 0.02);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.8);
}

.login-header {
  margin-bottom: 32px;
}

.logo-container {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-bottom: 24px;
}

.company-name {
  font-size: 24px;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.welcome-title {
  font-size: 28px;
  font-weight: 700;
  color: #111827;
  margin: 0 0 8px 0;
}

.welcome-subtitle {
  color: #6b7280;
  font-size: 16px;
  margin: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.input-group {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 16px;
  z-index: 10;
}

.form-input {
  width: 100%;
  padding: 16px 16px 16px 48px;
  border: 1.5px solid #e5e7eb;
  border-radius: 12px;
  font-size: 16px;
  transition: all 0.2s ease;
  background: #fafafa;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  background: white;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.password-toggle {
  position: absolute;
  right: 16px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.password-toggle:hover {
  background-color: #f3f4f6;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
}

.remember-me {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  color: #6b7280;
}

.remember-me input {
  display: none;
}

.checkmark {
  width: 18px;
  height: 18px;
  border: 2px solid #d1d5db;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.remember-me input:checked + .checkmark {
  background-color: #3b82f6;
  border-color: #3b82f6;
}

.remember-me input:checked + .checkmark::after {
  content: '✓';
  color: white;
  font-size: 12px;
}

.forgot-password {
  color: #3b82f6;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.forgot-password:hover {
  color: #2563eb;
}

.login-button {
  width: 100%;
  padding: 16px;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.login-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(37, 99, 235, 0.3);
}

.login-button:disabled {
  cursor: not-allowed;
  opacity: 0.8;
}

.login-button.loading {
  pointer-events: none;
}

.button-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid transparent;
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-message {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 12px;
  color: #dc2626;
  font-size: 14px;
  font-weight: 500;
}

.separator {
  display: flex;
  align-items: center;
  margin: 24px 0;
  color: #6b7280;
  font-size: 14px;
}

.separator::before,
.separator::after {
  content: '';
  flex: 1;
  height: 1px;
  background-color: #e5e7eb;
}

.separator::before {
  margin-right: 16px;
}

.separator::after {
  margin-left: 16px;
}

.social-login {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 24px;
}

.social-button {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 16px;
  border: 1.5px solid #e5e7eb;
  border-radius: 12px;
  background: white;
  color: #374151;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.social-button:hover {
  border-color: #d1d5db;
  background-color: #f9fafb;
  transform: translateY(-1px);
}

.signup-link {
  text-align: center;
  color: #6b7280;
  font-size: 14px;
}

.signup-link a {
  color: #3b82f6;
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s;
}

.signup-link a:hover {
  color: #2563eb;
}

.login-footer {
  margin-top: 32px;
  text-align: center;
  color: #9ca3af;
  font-size: 14px;
}

.footer-links {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin-top: 8px;
}

.footer-links a {
  color: #9ca3af;
  text-decoration: none;
  transition: color 0.2s;
}

.footer-links a:hover {
  color: #6b7280;
}
.logo {
width: 500px;
}

/* 🚨 Nouveau style pour l'image PNG */
.logo-img {
 width: 100%; /* L'image remplit son conteneur .logo */
 height: 100%;
object-fit: contain; /* S'assurer qu'elle s'affiche correctement sans déformation */
}
/* Responsive */
@media (max-width: 480px) {
  .login-card {
    padding: 32px 24px;
    border-radius: 20px;
  }
  
  .welcome-title {
    font-size: 24px;
  }
  
  .social-login {
    grid-template-columns: 1fr;
  }
  
  .form-options {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }
  
  .footer-links {
    flex-direction: column;
    gap: 8px;
  }
}

@media (max-width: 360px) {
  .login-card {
    padding: 24px 20px;
  }
  
  .welcome-title {
    font-size: 22px;
  }
}
</style>