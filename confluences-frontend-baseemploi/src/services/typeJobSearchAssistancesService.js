/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 07.10.2022
 * Description : Configurations des requêtes API pour les types d'ARE
 * Fichier : typeJobSearchAssistanceServices.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/typeJobSearchAssistances'

export default {
  // Récupère tous les types d'Assistance à la Recherche d'Emploi (ARE)
  getTypeJobSearchAssistances() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type d'ARE spécifique par ID
  getTypeJobSearchAssistance(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type d'ARE (POST)
  postTypeJobSearchAssistance(typeJobSearchAssistance) {
    return axios.post(API_URL + CONTROLLER, typeJobSearchAssistance)
  },
  
  // Met à jour un type d'ARE existant (PUT)
  putTypeJobSearchAssistance(typeJobSearchAssistance) {
    return axios.put(
      API_URL +
        CONTROLLER +
        '/' +
        typeJobSearchAssistance.typeJobSearchAssistanceId,
      typeJobSearchAssistance
    )
  },
  
  // Supprime un type d'ARE par ID (DELETE)
  deleteTypeJobSearchAssistance(typeJobSearchAssistanceId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeJobSearchAssistanceId)
  }
}