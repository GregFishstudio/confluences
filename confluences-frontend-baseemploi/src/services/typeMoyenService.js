/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les types de moyens
 * Fichier : typeMoyenService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/TypeMoyens'

export default {
  // Récupère tous les types de moyens
  getTypeMoyens() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un type de moyen spécifique par ID
  getTypeMoyen(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  // Crée un nouveau type de moyen (POST)
  postTypeMoyen(typeMoyen) {
    return axios.post(API_URL + CONTROLLER, typeMoyen)
  },
  
  // Met à jour un type de moyen existant (PUT)
  putTypeMoyen(typeMoyen) {
    return axios.put(
      API_URL + CONTROLLER + '/' + typeMoyen.typeMoyenId,
      typeMoyen
    )
  },
  
  // Supprime un type de moyen par ID (DELETE)
  deleteTypeMoyen(typeMoyenId) {
    return axios.delete(API_URL + CONTROLLER + '/' + typeMoyenId)
  }
}