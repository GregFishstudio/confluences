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
    loggedIn: (state) =>
      !!(state.token && (!state.expiresAt || state.expiresAt > Date.now())),
    username: (state) => state.user?.profile?.email || state.user?.email || 'Utilisateur'
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
                  .map(([k, v]) => encodeURIComponent(k) + '=' + encodeURIComponent(v))
                  .join('&')
            ]
          }
        )

        this.token = response.data.access_token
        this.refreshToken = response.data.refresh_token
        this.expiresAt = Date.now() + (response.data.expires_in || 3600) * 1000

        const decoded = parseJwt(this.token)
        this.user = {
          email: decoded.email || decoded.sub,
          roles: decoded.role || decoded.roles || []
        }

        localStorage.setItem('access_token', this.token)
        localStorage.setItem('refresh_token', this.refreshToken)
        localStorage.setItem('expires_at', this.expiresAt)
        localStorage.setItem('user', JSON.stringify(this.user))

        console.log('Login reussi:', this.user)

        const returnUrl = router.currentRoute.value.query.returnUrl || '/stagiaires'
        router.push(returnUrl)
      } catch (error) {
        console.error('Erreur de connexion', error)
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
        const storedUser = JSON.parse(localStorage.getItem('user') || '{}')
        this.$patch({
          token,
          user: storedUser,
          refreshToken: localStorage.getItem('refresh_token'),
          expiresAt
        })
        const email = storedUser?.profile?.email || storedUser?.email || '(inconnu)'
        console.log('Session restauree pour', email)
      } else {
        console.warn('Aucun token valide trouve')
      }
    }
  }
})

function parseJwt(token) {
  try {
    return JSON.parse(atob(token.split('.')[1]))
  } catch (e) {
    return {}
  }
}
