/**
 * Description : Configuration des requêtes API pour les questions du questionnaire de marrainage
 * Fichier : marrainageQuestionService.js
 **/

import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/MarrainageQuestions'

export default {
  getQuestions(activeOnly = false) {
    return axios.get(API_URL + CONTROLLER, { params: { activeOnly } })
  },
  postQuestion(question) {
    return axios.post(API_URL + CONTROLLER, question)
  },
  putQuestion(question) {
    return axios.put(API_URL + CONTROLLER + '/' + question.marrainageQuestionId, question)
  },
  // Nouvel ordre : identifiants des questions, de la première à la dernière.
  putOrdre(ids) {
    return axios.put(API_URL + CONTROLLER + '/ordre', ids)
  },
  deleteQuestion(id) {
    return axios.delete(API_URL + CONTROLLER + '/' + id)
  }
}
