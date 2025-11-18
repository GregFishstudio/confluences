/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 07.10.2022
 * Description : Configurations des requêtes API pour les ARE
 * Fichier : jobSearchAssistancesServices.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/JobSearchAssistances'

export default {
  // Récupère toutes les Assistances à la Recherche d'Emploi (ARE)
  getJobSearchAssistances() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère une ARE spécifique par ID
  getJobSearchAssistance(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée une nouvelle ARE (POST)
  postJobSearchAssistance(jobSearchAssistance) {
    return axios.post(API_URL + CONTROLLER, jobSearchAssistance)
  },
  
  // Met à jour une ARE existante (PUT)
  putJobSearchAssistance(jobSearchAssistance) {
    return axios.put(
      API_URL + CONTROLLER + '/' + jobSearchAssistance.jobSearchAssistanceId,
      jobSearchAssistance
    )
  },
  
  // Supprime une ARE par ID (DELETE)
  deleteJobSearchAssistance(jobSearchAssistanceId) {
    return axios.delete(API_URL + CONTROLLER + '/' + jobSearchAssistanceId)
  }
}