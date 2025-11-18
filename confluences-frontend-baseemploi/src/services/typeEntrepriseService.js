/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types d'entreprise
 * Fichier : typeEntrepriseService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeEntreprise'

export default {
  // Récupère tous les types d'entreprise
  getTypeEntreprises() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type d'entreprise spécifique par ID
  getTypeEntreprise(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type d'entreprise (POST)
  postTypeEntreprise(typeEntreprise) {
    return axios.post(API_URL + CONTROLLER, typeEntreprise)
  },
  
  // Met à jour un type d'entreprise existant (PUT)
  putTypeEntreprise(typeEntreprise) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeEntreprise.typeEntrepriseId,
      typeEntreprise
    )
  },
  
  // Supprime un type d'entreprise par ID (DELETE)
  deleteTypeEntreprise(typeEntrepriseId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeEntrepriseId)
  }
}