// @/services/api.js

const API_BASE = import.meta.env.VITE_API_URL

export async function apiFetch(url, options = {}) {
  const res = await fetch(url, {
    ...options,
    headers: {
      Authorization: `Bearer ${localStorage.getItem('access_token')}`,
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  // 401 : déjà traité par l'intercepteur fetch (services/session.js) — jeton renouvelé ou
  // reconnexion sur place, puis nouvel essai. Un 401 restant ici est un vrai refus.
  if (res.status === 401) return null

  return res
}

export default API_BASE