// @/services/api.js
import router from '@/router'

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

  if (res.status === 401) {
    localStorage.removeItem('access_token')
    router.push({ name: 'login' })
    return null
  }

  return res
}

export default API_BASE