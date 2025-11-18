/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les entrepriseOffres
 * Fichier : entrepriseOffreService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/EntrepriseOffres'

export default {
  // Création d'une nouvelle EntrepriseOffre (POST)
  postEntrepriseOffre(entrepriseOffre) {
    return axios.post(API_URL + CONTROLLER, entrepriseOffre)
  },
  
  // Suppression d'une EntrepriseOffre (DELETE par clés composites)
  deleteEntrepriseOffre(payload) {
    // Construction de l'URL: /api/EntrepriseOffres/{entrepriseId}/{offreId}
    return axios.delete(
      API_URL + CONTROLLER + '/' + payload.entrepriseId + '/' + payload.offreId
    )
  }
}