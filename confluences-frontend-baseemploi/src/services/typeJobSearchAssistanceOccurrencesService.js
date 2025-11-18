/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 07.10.2022
 * Description : Configurations des requêtes API pour les types d'ARE
 * Fichier : typeJobSearchAssistanceOccurrencesService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/typeJobSearchAssistanceOccurrences'

export default {
  // Récupère toutes les occurrences d'Assistance à la Recherche d'Emploi
  getTypeJobSearchAssistanceOccurrences() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère une occurrence spécifique par ID
  getTypeJobSearchAssistanceOccurrence(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée une nouvelle occurrence (POST)
  postTypeJobSearchAssistanceOccurrence(typeJobSearchAssistanceOccurrence) {
    return axios.post(API_URL + CONTROLLER, typeJobSearchAssistanceOccurrence)
  },
  
  // Met à jour une occurrence existante (PUT)
  putTypeJobSearchAssistanceOccurrence(typeJobSearchAssistanceOccurrence) {
    return axios.put(
      API_URL +
        CONTROLLER +
        '/' +
        typeJobSearchAssistanceOccurrence.typeJobSearchAssistanceOccurrenceId,
      typeJobSearchAssistanceOccurrence
    )
  },
  
  // Supprime une occurrence par ID (DELETE)
  deleteTypeJobSearchAssistanceOccurrence(typeJobSearchAssistanceOccurrenceId) {
    return axios.delete(
      API_URL + CONTROLLER + '/' + typeJobSearchAssistanceOccurrenceId
    )
  }
}