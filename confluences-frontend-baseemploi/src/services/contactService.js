/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les contacts
 * Fichier : contactService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/contacts'

export default {
  // Récupère tous les contacts
  getContacts() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère un contact spécifique par ID
  getContact(id) {
    return axios.get(API_URL + `${CONTROLLER}/${id}`)
  },
  
  // Crée un nouveau contact (POST)
  postContact(contact) {
    return axios.post(API_URL + CONTROLLER, contact)
  },
  
  // Met à jour un contact existant (PUT)
  putContact(contact) {
    return axios.put(API_URL + `${CONTROLLER}/${contact.contactId}`, contact)
  },
  
  // Supprime un contact par ID (DELETE)
  deleteContact(contactId) {
    return axios.delete(API_URL + `${CONTROLLER}/${contactId}`)
  }
}