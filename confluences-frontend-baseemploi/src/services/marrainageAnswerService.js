/**
 * Description : Configuration des requêtes API pour les réponses au questionnaire de marrainage
 * Fichier : marrainageAnswerService.js
 **/

import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/MarrainageAnswers'

export default {
  getAnswers(marrainageId) {
    return axios.get(API_URL + CONTROLLER, { params: { marrainageId } })
  },
  saveAnswers(marrainageId, answers) {
    return axios.post(API_URL + CONTROLLER + '/bulk', { marrainageId, answers })
  }
}
