/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les stagiaires
 * Fichier : stagiaireService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/stagiaires'

export default {
  // Récupère tous les stagiaires
  getStagiaires() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un stagiaire spécifique par ID
  getStagiaire(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Met à jour un stagiaire existant (PUT)
  putStagiaire(stagiaire) {
    return axios.put(
      API_URL + CONTROLLER + '/' + stagiaire.stagiaireId,
      stagiaire
    )
  },
  
  // Supprime un stagiaire par ID (DELETE)
  deleteStagiaire(stagiaireId) {
    return axios.delete(API_URL + CONTROLLER + '/' + stagiaireId)
  }
}