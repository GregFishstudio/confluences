// src/services/authService.js
import axios from 'axios'

export async function login(email, password) {
  return axios.post(
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
              ([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`
            )
            .join('&')
      ]
    }
  )
}
