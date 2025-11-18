// src/store/auth.js
import { defineStore } from 'pinia'
import axios from 'axios'
import router from '@/router'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
    token: localStorage.getItem('access_token') || null,
    refreshToken: localStorage.getItem('refresh_token') || null,
    expiresAt: parseInt(localStorage.getItem('expires_at')) || null
  }),

  getters: {
    // ✅ Considère connecté seulement si le token est présent ET non expiré
    loggedIn: (state) =>
      !!(state.token && (!state.expiresAt || state.expiresAt > Date.now())),
    username: (state) => state.user?.email || 'Utilisateur'
  },

  actions: {
    async login(email, password) {
      try {
        const response = await axios.post(
          'http://localhost:5000/connect/token',
          {
            client_id: 'gestion-stagiaire',
            grant_type: 'password',
            username: email,
            password: password
          },
          {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            transformRequest: [
              (data) =>
                Object.entries(data)
                  .map(
                    ([k, v]) =>
                      `${encodeURIComponent(k)}=${encodeURIComponent(v)}`
                  )
                  .join('&')
            ]
          }
        )

        // ✅ Sauvegarde des données utilisateur
        this.token = response.data.access_token
        this.refreshToken = response.data.refresh_token
        this.expiresAt = Date.now() + (response.data.expires_in || 3600) * 1000

        const decoded = parseJwt(this.token)
        this.user = {
          email: decoded.email || decoded.sub,
          roles: decoded.role || decoded.roles || []
        }

        // 🧠 Stockage local
        localStorage.setItem('access_token', this.token)
        localStorage.setItem('refresh_token', this.refreshToken)
        localStorage.setItem('expires_at', this.expiresAt)
        localStorage.setItem('user', JSON.stringify(this.user))

        console.log('✅ Login réussi:', this.user)

        // 🔁 Redirection après connexion
        const returnUrl =
          router.currentRoute.value.query.returnUrl || '/stagiaires'
        router.push(returnUrl)
      } catch (error) {
        console.error('❌ Erreur de connexion', error)
        throw error
      }
    },

    logout() {
      this.token = null
      this.refreshToken = null
      this.user = null
      this.expiresAt = null

      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
      localStorage.removeItem('expires_at')
      localStorage.removeItem('user')

      router.push('/login')
    },

    restoreSession() {
  const token = localStorage.getItem('access_token')
  const expiresAt = parseInt(localStorage.getItem('expires_at'))

  if (token && (!expiresAt || expiresAt > Date.now())) {
    this.$patch({
      token,
      user: JSON.parse(localStorage.getItem('user')),
      refreshToken: localStorage.getItem('refresh_token'),
      expiresAt
    })
    console.log('✅ Session restaurée pour', this.user?.email)
  } else {
    console.warn('⚠️ Aucun token valide trouvé (mais pas de logout forcé)')
    // ❌ ne pas faire this.logout() ici au montage
    // laisse l’utilisateur se connecter manuellement
  }
}

  }
})

function parseJwt(token) {
  try {
    return JSON.parse(atob(token.split('.')[1]))
  } catch {
    return {}
  }
}
