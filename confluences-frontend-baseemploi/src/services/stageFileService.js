/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les fichiers des stages
 * Fichier : stageFileService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/stageFiles'

export default {
  // Récupère un fichier spécifique par ID (téléchargement)
  getStageFile(id) {
    return axios.get(API_URL + `${CONTROLLER}/${id}`, {
      responseType: 'blob' // important pour recevoir des données binaires
    })
  },
  
  // Envoie un nouveau fichier (upload)
  postStageFile(formData) {
    // formData doit contenir le fichier et les métadonnées
    return axios.post(API_URL + CONTROLLER, formData)
  },
  
  // Supprime un fichier par ID
  deleteStageFile(id) {
    return axios.delete(API_URL + `${CONTROLLER}/${id}`)
  }
}