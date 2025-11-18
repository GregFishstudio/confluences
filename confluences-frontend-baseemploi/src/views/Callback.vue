<template>
  <div class="callback-container">
    <div class="callback-content">
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <h2 class="title">Authentification en cours...</h2>
        <p class="subtitle">Validation de votre session et récupération des informations utilisateur.</p>
      </div>
      <div v-else-if="error" class="error-state">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="error-icon">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9 3.75h.008v.008H12v-.008Z" />
        </svg>
        <h2 class="title error-title">Erreur de Connexion</h2>
        <p class="subtitle error-text">{{ error }}</p>
        <button @click="goToLogin" class="error-button">Retour à la page de connexion</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { UserManager, WebStorageStateStore } from 'oidc-client-ts'
import { useAuthStore } from '@/store/auth'

const router = useRouter()
const authStore = useAuthStore()
const loading = ref(true)
const error = ref(null)

// 🔧 MÊME configuration que Login.vue
const getOidcConfig = () => {
  const authority = import.meta.env.VITE_AUTHORITY_URL || 'http://localhost:5000'
  const clientId = import.meta.env.VITE_CLIENT_ID || 'confluences-web'
  
  return {
    authority,
    client_id: clientId,
    redirect_uri: `${window.location.origin}/callback`,
    response_type: 'code',
    scope: 'openid profile api1 roles',
    userStore: new WebStorageStateStore({ store: window.localStorage }),
    loadUserInfo: true,
    filterProtocolClaims: true,
    monitorSession: false
  }
}

let mgr = null

const getUserManager = () => {
  if (!mgr) {
    mgr = new UserManager(getOidcConfig())
  }
  return mgr
}

function goToLogin() {
  router.replace('/login')
}

onMounted(async () => {
  console.log('🔁 Callback OIDC démarré...')
  loading.value = true
  error.value = null

  try {
    const userManager = getUserManager()
    
    // 1. Nettoie les états précédents
    await userManager.clearStaleState()
    
    // 2. Traite la réponse OIDC
    console.log('🔄 Traitement du callback OIDC...')
    const user = await userManager.signinRedirectCallback()
    
    console.log('✅ Callback traité, utilisateur:', user)

    if (user && user.access_token) {
      console.log('🎉 Authentification réussie !')
      
      // 3. Stocke dans Pinia
      await authStore.authenticate(user)

      // 4. Redirection sécurisée
      let target = '/stagiaires'
      if (user.state && user.state.returnUrl) {
        const cleanReturnUrl = cleanReturnUrl(user.state.returnUrl)
        if (cleanReturnUrl) {
          target = cleanReturnUrl
        }
      }
      
      console.log(`➡️ Redirection vers: ${target}`)
      router.replace(target)
      
    } else {
      throw new Error('Aucun token reçu après authentification')
    }

  } catch (err) {
    console.error('❌ Erreur callback OIDC:', err)
    
    // Messages d'erreur plus précis
    if (err.message.includes('authority mismatch')) {
      error.value = 'Problème de configuration. Veuillez rafraîchir la page.'
    } else if (err.message.includes('state')) {
      error.value = 'Session expirée. Veuillez vous reconnecter.'
    } else {
      error.value = 'Échec de la connexion. Veuillez réessayer.'
    }
    
    loading.value = false
    
    // Redirection après délai
    setTimeout(() => goToLogin(), 4000)
  }
})

function cleanReturnUrl(returnUrl) {
  if (!returnUrl) return null
  
  try {
    // URLs autorisées
    const allowedPaths = ['/stagiaires', '/entreprises', '/offres', '/dashboard', '/profile']
    const url = new URL(returnUrl, window.location.origin)
    
    if (allowedPaths.some(path => url.pathname.startsWith(path))) {
      return url.pathname + url.search
    }
    
    return '/stagiaires'
  } catch {
    return '/stagiaires'
  }
}
</script>

<style scoped>
/* Conteneur principal */
.callback-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f4f7f9;
}

.callback-content {
  text-align: center;
  padding: 40px;
  background: white;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
  max-width: 400px;
  width: 90%;
}

/* Style de chargement */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.spinner {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3b82f6;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
  margin-bottom: 20px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 8px;
}

.subtitle {
  color: #6b7280;
  font-size: 0.95rem;
  margin: 0;
}

/* Style d'erreur */
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.error-icon {
  width: 48px;
  height: 48px;
  color: #ef4444;
  margin-bottom: 15px;
}

.error-title {
  color: #ef4444;
}

.error-text {
  color: #dc2626;
  margin-bottom: 20px;
}

.error-button {
  padding: 10px 20px;
  background-color: #ef4444;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.error-button:hover {
  background-color: #dc2626;
}
</style>