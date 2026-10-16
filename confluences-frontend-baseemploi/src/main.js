/**
 * Projet: Confluences (migration Vue 3)
 * Auteur : Greg + Tim (2025)
 * Description : Point d’entrée principal de l’application Vue 3
 */

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store' // Vuex 4
import { createPinia } from 'pinia'
import api from './plugins/axios'
// --- Vuetify 3 ---
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { fr } from 'vuetify/locale'
import { matchesSearch } from '@/utils/search'

// --- Icônes Material Design ---
import '@mdi/font/css/materialdesignicons.css'

// --- CSS globaux ---
import 'nprogress/nprogress.css'

// --- Utils globaux ---
import upperFirst from 'lodash/upperFirst'
import camelCase from 'lodash/camelCase'

import axios from 'axios'

axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Lecture qui échoue de façon passagère (réseau, serveur, jeton pas encore disponible juste
// après la connexion) : une seconde tentative automatique, sinon les listes déroulantes des
// anciens écrans restaient vides jusqu'à un rafraîchissement de la page.
axios.interceptors.response.use(undefined, async (error) => {
  const config = error?.config
  const status = error?.response?.status
  const transient = !error?.response || status >= 500 || status === 401
  if (config && !config.__retried && (config.method || 'get').toLowerCase() === 'get' && transient) {
    config.__retried = true
    await new Promise(resolve => setTimeout(resolve, 800))
    return axios(config)
  }
  return Promise.reject(error)
})

// --- Confirmation des enregistrements ---
// Les pages récentes appellent l'API avec fetch (apiFetch/authFetch) sans rien afficher en cas
// de succès : on confirme ici, en un seul endroit, chaque enregistrement réussi vers l'API.
// (Les anciens écrans passent par axios et leurs modules Vuex notifient déjà eux-mêmes.)
// Messages regroupés : une rafale de clics (ex. fiche de présence) ne donne qu'un message.
const API_URL = import.meta.env.VITE_API_URL || ''
const SAVE_METHODS = new Set(['POST', 'PUT', 'PATCH', 'DELETE'])
const NOT_A_SAVE = [/\/taux(\?|$)/i, /\/api\/documents\//i]   // recalculs et PDF, pas des modifications
let lastSaveToast = 0
const originalFetch = window.fetch.bind(window)
window.fetch = async (input, init = {}) => {
  const res = await originalFetch(input, init)
  try {
    const url = typeof input === 'string' ? input : input?.url || ''
    const method = String(init.method || input?.method || 'GET').toUpperCase()
    if (res.ok && API_URL && url.startsWith(API_URL) && SAVE_METHODS.has(method) && !NOT_A_SAVE.some(r => r.test(url))) {
      const now = Date.now()
      if (now - lastSaveToast > 2000) {
        lastSaveToast = now
        store.dispatch('notification/add', {
          type: 'success',
          message: method === 'DELETE' ? 'Suppression enregistrée' : 'Modification sauvegardée',
        })
      }
    }
  } catch { /* la confirmation ne doit jamais casser la requête */ }
  return res
}

// Création des instances
const app = createApp(App)
// Recherche tolérante dans toutes les listes (tableaux) et listes déroulantes avec recherche :
// ignore majuscules, accents, tirets/ponctuation et ordre des mots, accepte une petite faute
// ("Jean Marie" trouve "Jean-Marie", "Dupont Jean" trouve la ligne Jean | Dupont).
// Un composant qui fournit son propre custom-filter garde le sien.
const searchableValues = (raw, depth = 0) => {
  if (raw == null) return []
  if (typeof raw !== 'object') return [raw]
  if (depth > 1) return []
  return Object.values(raw).flatMap(v => searchableValues(v, depth + 1))
}
const tableFilter = (value, query, item) => matchesSearch(query, item?.raw ? searchableValues(item.raw) : value)
const pickerFilter = (value, query) => matchesSearch(query, value)

const vuetify = createVuetify({
  components,
  directives,
  defaults: {
    VDataTable: { customFilter: tableFilter },
    VDataTableVirtual: { customFilter: tableFilter },
    VAutocomplete: { customFilter: pickerFilter },
    VCombobox: { customFilter: pickerFilter },
  },
  icons: { defaultSet: 'mdi' },
  locale: {
    locale: 'fr',       // langue par défaut
    fallback: 'en',     // si quelque chose manque
    messages: { fr },   // les traductions françaises
  },
})
const pinia = createPinia()

// --- Enregistrement automatique des composants globaux BaseXxx ---
const requireComponent = import.meta.glob('./components/Base[A-Z]*.{vue,js}', { eager: true })
Object.entries(requireComponent).forEach(([path, definition]) => {
  const componentName = upperFirst(
    camelCase(path.split('/').pop().replace(/\.\w+$/, ''))
  )
  app.component(componentName, definition.default)
})

app.config.globalProperties.$api = api
// --- Injection des plugins ---
app.use(router)
app.use(store)   // Vuex 4
app.use(pinia)   // Pinia (optionnel mais compatible)
app.use(vuetify)

// --- Montage ---
app.mount('#app')
