/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types d'offre
 * Fichier : typeOffreService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeOffres'

export default {
  // Récupère tous les types d'offre
  getTypeOffres() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type d'offre spécifique par ID
  getTypeOffre(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type d'offre (POST)
  postTypeOffre(typeOffre) {
    return axios.post(API_URL + CONTROLLER, typeOffre)
  },
  
  // Met à jour un type d'offre existant (PUT)
  putTypeOffre(typeOffre) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeOffre.typeOffreId,
      typeOffre
    )
  },
  
  // Supprime un type d'offre par ID (DELETE)
  deleteTypeOffre(typeOffreId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeOffreId)
  }
}