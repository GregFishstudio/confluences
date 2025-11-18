/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les sessions
 * Fichier : sessionService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/sessions'

export default {
  // Récupère toutes les sessions
  getSessions() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  // Récupère une session spécifique par ID
  getSession(id) {
    return axios.get(API_URL + `${CONTROLLER}/${id}`)
  },
  
  // Crée une nouvelle session (POST)
  postSession(session) {
    return axios.post(API_URL + CONTROLLER, session)
  },
  
  // Met à jour une session existante (PUT)
  putSession(session) {
    return axios.put(API_URL + `${CONTROLLER}/${session.sessionId}`, session)
  },
  
  // Supprime une session par ID (DELETE)
  deleteSession(sessionId) {
    return axios.delete(API_URL + `${CONTROLLER}/${sessionId}`)
  }
}