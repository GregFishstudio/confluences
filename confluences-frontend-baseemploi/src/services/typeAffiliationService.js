/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types d'affiliation
 * Fichier : typeAffiliationService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeAffiliations'

export default {
  // Récupère tous les types d'affiliation
  getTypeAffiliations() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type d'affiliation spécifique par ID
  getTypeAffiliation(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type d'affiliation (POST)
  postTypeAffiliation(typeAffiliation) {
    return axios.post(API_URL + CONTROLLER, typeAffiliation)
  },
  
  // Met à jour un type d'affiliation existant (PUT)
  putTypeAffiliation(typeAffiliation) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeAffiliation.typeAffiliationId,
      typeAffiliation
    )
  },
  
  // Supprime un type d'affiliation par ID (DELETE)
  deleteTypeAffiliation(typeAffiliationId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeAffiliationId)
  }
}