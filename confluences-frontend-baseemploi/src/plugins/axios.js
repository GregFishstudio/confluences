import axios from 'axios'

// --- Crée une instance partagée ---
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5003/api'
})

// --- Intercepteur pour ajouter le token automatiquement ---
api.interceptors.request.use(config => {
  const token = localStorage.getItem('access_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// --- Gestion d’erreurs (optionnelle) ---
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      console.warn('🔒 Session expirée — redirection login')
      localStorage.removeItem('access_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
