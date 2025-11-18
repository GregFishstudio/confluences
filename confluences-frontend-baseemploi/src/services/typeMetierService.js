/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types de métier
 * Fichier : typeMetierService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeMetiers'

export default {
  // Récupère tous les types de métier
  getTypeMetiers() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type de métier spécifique par ID
  getTypeMetier(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type de métier (POST)
  postTypeMetier(typeMetier) {
    return axios.post(API_URL + CONTROLLER, typeMetier)
  },
  
  // Met à jour un type de métier existant (PUT)
  putTypeMetier(typeMetier) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeMetier.typeMetierId,
      typeMetier
    )
  },
  
  // Supprime un type de métier par ID (DELETE)
  deleteTypeMetier(typeMetierId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeMetierId)
  }
}