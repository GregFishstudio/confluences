/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types de stage
 * Fichier : typeStageService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeStages'

export default {
  // Récupère tous les types de stage
  getTypeStages() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type de stage spécifique par ID
  getTypeStage(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type de stage (POST)
  postTypeStage(typeStage) {
    return axios.post(API_URL + CONTROLLER, typeStage)
  },
  
  // Met à jour un type de stage existant (PUT)
  putTypeStage(typeStage) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeStage.typeStageId,
      typeStage
    )
  },
  
  // Supprime un type de stage par ID (DELETE)
  deleteTypeStage(typeStageId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeStageId)
  }
}