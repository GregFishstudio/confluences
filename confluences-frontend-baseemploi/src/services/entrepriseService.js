/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configurations des requêtes API pour les entreprises
 * Fichier : entrepriseService.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

import axios from 'axios'

// CORRECTION: Remplacer process.env par import.meta.env
const API_URL = import.meta.env.VITE_API_URL
const CONTROLLER = '/api/entreprise'

export default {
  // Méthode helper optimisée pour générer la query string
  getQuery(filter) {
    const params = new URLSearchParams()

    // 1. Ajouter les filtres simples (nom, ville, code postal, etc.)
    if (filter.city) {
      params.append('city', filter.city)
    }
    if (filter.codePostal) {
      params.append('codePostal', filter.codePostal)
    }
    if (filter.nom) {
      params.append('nom', filter.nom)
    }
    if (filter.dateModification) {
      params.append('dateModification', filter.dateModification)
    }
    if (filter.createur) {
      params.append('createur', filter.createur)
    }

    // 2. Ajouter les filtres de tableaux (metiers, offres, domaines)
    // On itère sur les tableaux et on ajoute chaque ID sous la même clé (ex: ?metiers=1&metiers=2)
    if (filter.metiers && filter.metiers.length > 0) {
      filter.metiers.forEach(metier => params.append('metiers', metier.typeMetierId))
    }
    if (filter.offres && filter.offres.length > 0) {
      filter.offres.forEach(offre => params.append('offres', offre.typeOffreId))
    }
    if (filter.domaines && filter.domaines.length > 0) {
      filter.domaines.forEach(domaine => params.append('domaines', domaine.typeDomaineId))
    }
    
    // URLSearchParams génère automatiquement la chaîne, y compris le '?' initial si des paramètres existent
    const queryString = params.toString()
    return queryString ? `?${queryString}` : ''
  },
  
  // --- Méthodes API ---

  getEntreprises() {
    return axios.get(API_URL + CONTROLLER)
  },
  
  getEntreprisesWithFilter(filter) {
    let query = this.getQuery(filter)
    return axios.get(API_URL + CONTROLLER + query)
  },
  
  getEntreprise(id) {
    return axios.get(API_URL + CONTROLLER + '/' + id)
  },
  
  postEntreprise(entreprise) {
    return axios.post(API_URL + CONTROLLER, entreprise)
  },
  
  putEntreprise(entreprise) {
    return axios.put(
      API_URL + CONTROLLER + '/' + entreprise.entrepriseId,
      entreprise
    )
  },
  
  deleteEntreprise(entrepriseId) {
    return axios.delete(API_URL + CONTROLLER + '/' + entrepriseId)
  }
}