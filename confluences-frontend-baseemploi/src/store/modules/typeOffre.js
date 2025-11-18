/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des offres
 * Fichier : typeOffre.js
 **/

import TypeOffreService from '@/services/typeOffreService.js'

export const namespaced = true

export const state = {
  typeOffres: [],
  typeOffre: {}
}

export const mutations = {
  SET_TYPEOFFRES(state, typeOffres) {
    state.typeOffres = typeOffres
  },
  SET_TYPEOFFRE(state, typeOffre) {
    state.typeOffre = typeOffre
  },
  ADD_TYPEOFFRE(state, typeOffreNew) {
    state.typeOffres.unshift({
      ...typeOffreNew
    })
  },
  EDIT_TYPEOFFRE(state, typeOffreNew) {
    state.typeOffres = state.typeOffres.filter(
      typeOffre => typeOffre.typeOffreId !== typeOffreNew.typeOffreId
    )
    state.typeOffres.unshift({
      ...typeOffreNew
    })
  },
  DELETE_TYPEOFFRE(state) {
    state.typeOffre = null
  }
}

export const actions = {
  // Récupère les types d'offres et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeOffres({ commit, dispatch }, notify) {
    return TypeOffreService.getTypeOffres()
      .then(response => {
        commit('SET_TYPEOFFRES', response.data)
        if (notify == true) {
        
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème au chargement des types offres : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type d'offre spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeOffre({ commit, dispatch }, id) {
    return TypeOffreService.getTypeOffre(id)
      .then(response => {
        commit('SET_TYPEOFFRE', response.data)
        const notification = {
          type: 'success',
          message: 'Offre chargée'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Il y un problème charger un type d'offre " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type d'offre et notifie l'utilisateur en cas de succès ou erreur
  createTypeOffre({ commit, dispatch }, typeOffre) {
    return TypeOffreService.postTypeOffre(typeOffre)
      .then(response => {
        commit('ADD_TYPEOFFRE', response.data)
        commit('SET_TYPEOFFRE', response.data)
        const notification = {
          type: 'success',
          message: 'Une offre a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'offre existe déjà"
        } else {
          message = "Erreur à l'ajout d'une offre : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type d'offre et notifie l'utilisateur en cas de succès ou erreur
  editTypeOffre({ commit, dispatch }, typeOffre) {
    return TypeOffreService.putTypeOffre(typeOffre)
      .then(() => {
        commit('EDIT_TYPEOFFRE', typeOffre)
        const notification = {
          type: 'success',
          message: 'Une offre a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'offre existe déjà"
        } else {
          message = "Erreur à l'ajout d'une offre : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type d'offre et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeOffre({ commit, dispatch }, typeOffreId) {
    return TypeOffreService.deleteTypeOffre(typeOffreId)
      .then(() => {
        commit('DELETE_TYPEOFFRE')
        const notification = {
          type: 'success',
          message: 'Une offre a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'une offre ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
