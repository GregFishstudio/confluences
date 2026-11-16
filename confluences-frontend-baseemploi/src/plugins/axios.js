import axios from 'axios'

// --- Crée une instance partagée ---
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5003/api'
})

// Jeton, renouvellement et reconnexion sur place : intercepteurs posés dans main.js
// (services/session.js installAxiosInterceptors).

export default api
