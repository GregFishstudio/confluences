/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 13.10.2020
 * Description : Configurations des requêtes API pour les entrepriseDomaines
 * Fichier : entrepriseDomaineService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/entrepriseDomaines'

export default {
  // Création d'une nouvelle association Entreprise-Domaine (POST)
  postEntrepriseDomaine(entrepriseDomaine) {
    return axios.post(API_URL + CONTROLLER, entrepriseDomaine)
  },
  
  // Suppression d'une association Entreprise-Domaine (DELETE par clés composites)
  deleteEntrepriseDomaine(payload) {
    // Construction de l'URL: /api/entrepriseDomaines/{entrepriseId}/{domaineId}
    return axios.delete(
      API_URL +
        CONTROLLER +
        '/' +
        payload.entrepriseId +
        '/' +
        payload.domaineId
    )
  }
}