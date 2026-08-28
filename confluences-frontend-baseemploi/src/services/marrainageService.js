/**
 * Description : Configurations des requêtes API pour le marrainage
 * Fichier : marrainageService.js
 **/

import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/marrainages'

export default {
  getMarrainages(params) {
    return axios.get(API_URL + CONTROLLER, { params })
  },

  getMarrainage(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },

  postMarrainage(marrainage) {
    return axios.post(API_URL + CONTROLLER, marrainage)
  },

  importMarrainages(marrainages) {
    return axios.post(API_URL + CONTROLLER + '/import', marrainages)
  },

  putMarrainage(marrainage) {
    return axios.put(API_URL + CONTROLLER + '/' + marrainage.marrainageId, marrainage)
  },

  deleteMarrainage(id) {
    return axios.delete(API_URL + CONTROLLER + '/' + id)
  },

  deleteAllMarrainages() {
    return axios.delete(API_URL + CONTROLLER + '/all')
  },

  mergeMarrainage(keepId, duplicateId) {
    return axios.post(`${API_URL}${CONTROLLER}/${keepId}/merge`, { duplicateId })
  }
}
