/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 23.09.2022
 * Description : Configurations des requêtes API pour les types d'activité pour un stage
 * Fichier : typeInternshipActivityService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeIntershipActivities'

export default {
  // Récupère tous les types d'activité de stage
  getTypeIntershipActivities() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type d'activité spécifique par ID
  getTypeIntershipActivity(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type d'activité de stage (POST)
  postTypeIntershipActivity(typeIntershipActivity) {
    return axios.post(API_URL + CONTROLLER, typeIntershipActivity)
  },
  
  // Met à jour un type d'activité de stage existant (PUT)
  putTypeIntershipActivity(typeIntershipActivity) {
    return axios.put(
      API_URL +
        CONTROLLER +
        '/' +
        typeIntershipActivity.typeIntershipActivityId,
      typeIntershipActivity
    )
  },
  
  // Supprime un type d'activité de stage par ID (DELETE)
  deleteTypeIntershipActivity(typeIntershipActivityId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeIntershipActivityId)
  }
}