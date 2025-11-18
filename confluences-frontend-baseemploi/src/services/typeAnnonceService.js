/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types d'annonce
 * Fichier : typeAnnonceService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeAnnonces'

export default {
  // Récupère tous les types d'annonce
  getTypeAnnonces() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type d'annonce spécifique par ID
  getTypeAnnonce(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type d'annonce (POST)
  postTypeAnnonce(typeAnnonce) {
    return axios.post(API_URL + CONTROLLER, typeAnnonce)
  },
  
  // Met à jour un type d'annonce existant (PUT)
  putTypeAnnonce(typeAnnonce) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeAnnonce.typeAnnonceId,
      typeAnnonce
    )
  },
  
  // Supprime un type d'annonce par ID (DELETE)
  deleteTypeAnnonce(typeAnnonceId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeAnnonceId)
  }
}