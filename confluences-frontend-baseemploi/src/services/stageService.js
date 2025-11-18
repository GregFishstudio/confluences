/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les stages
 * Fichier : stageService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/stages'

export default {
  // --- Méthodes API ---

  getStages() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  getStagesWithFilter(filter) {
    let query = this.getQuery(filter)
    return axios.get(API_URL + CONTROLLER + query)
  },
  
  getStagesExport(filter) {
    let query = this.getQuery(filter)
    // NOTE: L'URL d'export utilise une structure légèrement différente de la requête de liste
    return axios.get(API_URL + CONTROLLER + '/export' + query, {
      responseType: 'blob'
    })
  },
  
  getStage(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  postStage(stage) {
    return axios.post(API_URL + CONTROLLER, stage)
  },
  
  putStage(stage) {
    return axios.put(API_URL + CONTROLLER + '/' + stage.stageId, stage)
  },
  
  deleteStage(stageId) {
    return axios.delete(API_URL + CONTROLLER + '/' + stageId)
  },

  // --- Helper pour la Query String (Optimisé) ---
  
  // Permet de retourner les paramètres pour la requête permettant de filtrer les stages
  getQuery(filter) {
    const params = new URLSearchParams()
    
    // Fonction pour ajouter un paramètre seulement s'il existe et n'est pas vide
    const addParam = (key, value) => {
      if (value !== null && value !== undefined && value !== '') {
        params.append(key, value)
      }
    }

    addParam('typeIntershipActivityId', filter.typeIntershipActivityId)
    addParam('nom', filter.nom)
    addParam('typeMetierId', filter.typeMetierId)
    addParam('entrepriseId', filter.entrepriseId)
    addParam('stagiaireId', filter.stagiaireId)
    addParam('year', filter.year)
    addParam('dateDebut', filter.dateDebut)
    addParam('dateFin', filter.dateFin)
    addParam('typeStageId', filter.typeStageId)
    addParam('typeAnnonceId', filter.typeAnnonceId)
    addParam('sessionId', filter.sessionId)
    
    const queryString = params.toString()
    
    // Ajoute le '?' seulement si des paramètres existent
    return queryString ? `?${queryString}` : ''
  }
}