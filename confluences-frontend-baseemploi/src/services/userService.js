/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les utilisateurs
 * Fichier : userService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL

export default {
  // Récupère tous les utilisateurs (id, nom, niveau...)
  getUsers() {
    return axios.get(API_URL + '/api/Users')
  },

  // Récupère les stagiaires (dateDebutParticipation, prenom, typeAffiliation...)
  getStagiaires() {
    return axios.get(API_URL + '/api/Stagiaires')
  }
}