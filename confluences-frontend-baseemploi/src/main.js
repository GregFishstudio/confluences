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
import { installFetchInterceptor, installAxiosInterceptors, startTokenKeepAlive } from '@/services/session'

// Session et enregistrements (services/session.js) : jeton renouvelé automatiquement, reconnexion
// sur place si la session a expiré (la requête repart ensuite, rien n'est perdu), et état des
// enregistrements affiché en permanence par SaveStatusIndicator.
installFetchInterceptor()
installAxiosInterceptors(axios)
installAxiosInterceptors(api)
startTokenKeepAlive()

// Lecture qui échoue de façon passagère (réseau, serveur) : une seconde tentative automatique,
// sinon les listes déroulantes des anciens écrans restaient vides jusqu'à un rafraîchissement.
axios.interceptors.response.use(undefined, async (error) => {
  const config = error?.config
  const status = error?.response?.status
  const transient = !error?.response || status >= 500
  if (config && !config.__retried && (config.method || 'get').toLowerCase() === 'get' && transient) {
    config.__retried = true
    await new Promise(resolve => setTimeout(resolve, 800))
    return axios(config)
  }
  return Promise.reject(error)
})

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
