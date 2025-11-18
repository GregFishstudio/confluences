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

// Création des instances
const app = createApp(App)
const vuetify = createVuetify({
  components,
  directives,
  icons: { defaultSet: 'mdi' },
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
