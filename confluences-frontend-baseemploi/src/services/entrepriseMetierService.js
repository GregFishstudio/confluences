/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les entrepriseMetiers
 * Fichier : entrepriseMetierService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/entrepriseMetiers'

export default {
  // Création d'une nouvelle association Entreprise-Métier (POST)
  postEntrepriseMetier(entrepriseMetier) {
    return axios.post(API_URL + CONTROLLER, entrepriseMetier)
  },
  
  // Suppression d'une association Entreprise-Métier (DELETE par clés composites)
  deleteEntrepriseMetier(payload) {
    // Construction de l'URL: /api/entrepriseMetiers/{entrepriseId}/{metierId}
    return axios.delete(
      API_URL + CONTROLLER + '/' + payload.entrepriseId + '/' + payload.metierId
    )
  }
}