/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 30.11.2022
 * Description : Configurations des requêtes API pour les derniers contacts des entreprises
 * Fichier : lastContactService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/lastContacts'

export default {
  // Récupère tous les derniers contacts
  getLastContacts() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un dernier contact spécifique par ID
  getLastContact(id) {
    return axios.get(API_URL + `${CONTROLLER}/${id}`)
  },
  
  // Crée un nouveau dernier contact (POST)
  postLastContact(lastContact) {
    return axios.post(API_URL + CONTROLLER, lastContact)
  },
  
  // Met à jour un dernier contact existant (PUT)
  putLastContact(lastContact) {
    return axios.put(
      API_URL + `${CONTROLLER}/${lastContact.lastContactId}`,
      lastContact
    )
  },
  
  // Supprime un dernier contact par ID (DELETE)
  deleteLastContact(lastContactId) {
    return axios.delete(API_URL + `${CONTROLLER}/${lastContactId}`)
  }
}