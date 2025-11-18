/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types de domaine
 * Fichier : typeDomaineService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeDomaines'

export default {
  // Récupère tous les types de domaine
  getTypeDomaines() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type de domaine spécifique par ID
  getTypeDomaine(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type de domaine (POST)
  postTypeDomaine(typeDomaine) {
    return axios.post(API_URL + CONTROLLER, typeDomaine)
  },
  
  // Met à jour un type de domaine existant (PUT)
  putTypeDomaine(typeDomaine) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeDomaine.typeDomaineId,
      typeDomaine
    )
  },
  
  // Supprime un type de domaine par ID (DELETE)
  deleteTypeDomaine(typeDomaineId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeDomaineId)
  }
}