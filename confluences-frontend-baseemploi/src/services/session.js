// Session et enregistrements — un seul endroit pour :
//  1. garder la connexion valide : le jeton d'accès est renouvelé avec le refresh token (fourni
//     par le serveur d'identité mais jamais utilisé jusqu'ici), avant son expiration ;
//  2. ne jamais perdre une saisie quand la session a quand même expiré : la requête refusée (401)
//     attend que la personne se reconnecte dans une fenêtre sur place (SessionExpiredDialog),
//     puis repart automatiquement — au lieu de renvoyer vers l'écran de connexion ;
//  3. suivre l'état des enregistrements de toute l'application (SaveStatusIndicator) :
//     en cours, enregistré à HH:mm, ou échec avec une raison compréhensible.
import { reactive } from 'vue'
import axios from 'axios'

const AUTHORITY = import.meta.env.VITE_AUTHORITY_URL || 'http://localhost:5000'
const API_URL = import.meta.env.VITE_API_URL || ''
const CLIENT_ID = 'gestion-stagiaire'

// Renouvellement anticipé : quand il reste moins de 30 min, vérifié toutes les 5 min.
const REFRESH_MARGIN_MS = 30 * 60 * 1000
const KEEPALIVE_MS = 5 * 60 * 1000

const originalFetch = window.fetch.bind(window)

export const sessionState = reactive({
  expired: false,          // fenêtre de reconnexion ouverte
})

export const saveState = reactive({
  pending: 0,              // enregistrements en cours
  lastSavedAt: null,       // Date du dernier enregistrement réussi
  error: null,             // message du dernier échec (effacé au prochain succès)
})

// ─── Jetons ───────────────────────────────────────────────────────────────────
export function storeTokens(data) {
  const expiresAt = Date.now() + (data.expires_in || 3600) * 1000
  localStorage.setItem('access_token', data.access_token)
  if (data.refresh_token) localStorage.setItem('refresh_token', data.refresh_token)
  localStorage.setItem('expires_at', String(expiresAt))
  try {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    user.access_token = data.access_token
    if (data.refresh_token) user.refresh_token = data.refresh_token
    user.expires_at = expiresAt
    localStorage.setItem('user', JSON.stringify(user))
  } catch { /* fiche utilisateur illisible : les jetons suffisent */ }
  axios.defaults.headers.common.Authorization = `Bearer ${data.access_token}`
}

const expiresAt = () => Number(localStorage.getItem('expires_at')) || 0
const hasRefreshToken = () => {
  const rt = localStorage.getItem('refresh_token')
  return !!rt && rt !== 'undefined' && rt !== 'null'
}

let refreshing = null
// Un seul renouvellement à la fois : le refresh token est à usage unique.
export function refreshAccessToken() {
  if (refreshing) return refreshing
  if (!hasRefreshToken()) return Promise.resolve(false)
  refreshing = (async () => {
    try {
      const res = await originalFetch(`${AUTHORITY}/connect/token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({
          grant_type: 'refresh_token',
          refresh_token: localStorage.getItem('refresh_token'),
          client_id: CLIENT_ID,
        }),
      })
      if (!res.ok) return false
      storeTokens(await res.json())
      return true
    } catch {
      return false
    } finally {
      setTimeout(() => { refreshing = null }, 0)
    }
  })()
  return refreshing
}

// Connexion par mot de passe (fenêtre « Session expirée »).
export async function loginWithPassword(username, password) {
  const res = await originalFetch(`${AUTHORITY}/connect/token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      grant_type: 'password', username, password, client_id: CLIENT_ID,
      scope: 'openid profile api1 roles offline_access',
    }),
  })
  if (!res.ok) return false
  storeTokens(await res.json())
  return true
}

// ─── Reconnexion sur place ────────────────────────────────────────────────────
let waiters = []
function waitForRelogin() {
  sessionState.expired = true
  return new Promise(resolve => waiters.push(resolve))
}
export function reloginDone() {
  sessionState.expired = false
  waiters.forEach(resolve => resolve())
  waiters = []
}

// Après un 401 : renouvelle le jeton, sinon attend la reconnexion.
async function recover() {
  if (await refreshAccessToken()) return
  await waitForRelogin()
}

export function startTokenKeepAlive() {
  const tick = () => {
    if (hasRefreshToken() && expiresAt() && expiresAt() - Date.now() < REFRESH_MARGIN_MS) refreshAccessToken()
  }
  tick()
  setInterval(tick, KEEPALIVE_MS)
  // Ordinateur sorti de veille / onglet revenu au premier plan.
  document.addEventListener('visibilitychange', () => { if (!document.hidden) tick() })
}

// ─── Messages d'erreur compréhensibles ───────────────────────────────────────
async function readableError(res) {
  let detail = ''
  try {
    const text = await res.clone().text()
    try {
      const json = JSON.parse(text)
      detail = typeof json === 'string' ? json : (json.title || '')
    } catch { detail = text.length < 200 ? text : '' }
  } catch { /* corps illisible */ }
  if (res.status === 400) return detail || 'données refusées (vérifier les champs)'
  if (res.status === 403) return "vous n'avez pas les droits pour cette modification"
  if (res.status === 404) return "l'élément n'existe plus (supprimé entre-temps ?)"
  if (res.status === 409) return detail || 'conflit avec une donnée existante'
  if (res.status >= 500) return 'erreur du serveur'
  return detail || `erreur ${res.status}`
}

const SAVE_METHODS = new Set(['POST', 'PUT', 'PATCH', 'DELETE'])
// Recalculs et génération de PDF : pas des enregistrements.
const NOT_A_SAVE = [/\/taux(\?|$)/i, /\/api\/documents\//i]
const isApiUrl = url => !!API_URL && url.startsWith(API_URL)
const isSave = (url, method) => isApiUrl(url) && SAVE_METHODS.has(method) && !NOT_A_SAVE.some(r => r.test(url))

function saveStarted() { saveState.pending++ }
function saveSucceeded() {
  saveState.pending = Math.max(0, saveState.pending - 1)
  saveState.lastSavedAt = new Date()
  saveState.error = null
}
function saveFailed(message) {
  saveState.pending = Math.max(0, saveState.pending - 1)
  saveState.error = message
}

// ─── fetch ────────────────────────────────────────────────────────────────────
export function installFetchInterceptor() {
  window.fetch = async (input, init = {}) => {
    const url = typeof input === 'string' ? input : input?.url || ''
    const method = String(init.method || input?.method || 'GET').toUpperCase()
    const save = isSave(url, method)
    if (save) saveStarted()
    try {
      let res = await originalFetch(input, init)
      if (res.status === 401 && isApiUrl(url)) {
        await recover()
        const headers = new Headers(init.headers || (typeof input === 'string' ? undefined : input.headers))
        headers.set('Authorization', `Bearer ${localStorage.getItem('access_token')}`)
        res = await originalFetch(input, { ...init, headers })
      }
      if (save) res.ok ? saveSucceeded() : saveFailed(await readableError(res))
      return res
    } catch (e) {
      if (save) saveFailed('pas de connexion au serveur')
      throw e
    }
  }
}

// ─── axios (anciens écrans, modules Vuex) ─────────────────────────────────────
export function installAxiosInterceptors(instance) {
  instance.interceptors.request.use(config => {
    const url = `${config.baseURL || ''}${config.url || ''}`
    if (!url.includes('/connect/')) {
      const token = localStorage.getItem('access_token')
      if (token) config.headers.Authorization = `Bearer ${token}`
    }
    if (isSave(url, String(config.method || 'get').toUpperCase())) {
      config.__save = true
      saveStarted()
    }
    return config
  })
  instance.interceptors.response.use(
    response => {
      if (response.config?.__save) saveSucceeded()
      return response
    },
    async error => {
      const config = error?.config
      if (error?.response?.status === 401 && config && !config.__authRetried) {
        await recover()
        config.__authRetried = true
        if (config.__save) saveState.pending = Math.max(0, saveState.pending - 1)
        return instance(config)
      }
      if (config?.__save) {
        saveFailed(error?.response
          ? await readableError(new Response(JSON.stringify(error.response.data ?? ''), { status: error.response.status }))
          : 'pas de connexion au serveur')
      }
      return Promise.reject(error)
    },
  )
}
