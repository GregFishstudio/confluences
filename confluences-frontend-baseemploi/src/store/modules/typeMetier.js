/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des métiers
 * Fichier : typeMetier.js
 **/

import TypeMetierService from '@/services/typeMetierService.js'

export const namespaced = true

export const state = {
  typeMetiers: [],
  typeMetier: {}
}

export const mutations = {
  SET_TYPEMETIERS(state, typeMetiers) {
    state.typeMetiers = typeMetiers
  },
  SET_TYPEMETIER(state, typeMetier) {
    state.typeMetier = typeMetier
  },
  ADD_TYPEMETIER(state, typeMetierNew) {
    state.typeMetiers.unshift({
      ...typeMetierNew
    })
  },
  EDIT_TYPEMETIER(state, typeMetierNew) {
    state.typeMetiers = state.typeMetiers.filter(
      typeMetier => typeMetier.typeMetierId !== typeMetierNew.typeMetierId
    )
    state.typeMetiers.unshift({
      ...typeMetierNew
    })
  },
  DELETE_TYPEMETIER(state) {
    state.typeMetier = null
  }
}

export const actions = {
  // Récupère les types de métier et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeMetiers({ commit, dispatch }, notify) {
    return TypeMetierService.getTypeMetiers()
      .then(response => {
        commit('SET_TYPEMETIERS', response.data)
        if (notify == true) {
          
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème au chargement des types métiers : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type de métier spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeMetier({ commit, dispatch }, id) {
    return TypeMetierService.getTypeMetier(id)
      .then(response => {
        commit('SET_TYPEMETIER', response.data)
        const notification = {
          type: 'success',
          message: 'Métier chargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger un métier ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type de métier et notifie l'utilisateur en cas de succès ou erreur
  createTypeMetier({ commit, dispatch }, typeMetier) {
    return TypeMetierService.postTypeMetier(typeMetier)
      .then(response => {
        commit('ADD_TYPEMETIER', response.data)
        commit('SET_TYPEMETIER', response.data)
        const notification = {
          type: 'success',
          message: 'Un métier a été ajouté !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le métier existe déjà'
        } else {
          message = "Erreur à l'ajout d'un métier : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type de métier et notifie l'utilisateur en cas de succès ou erreur
  editTypeMetier({ commit, dispatch }, typeMetier) {
    return TypeMetierService.putTypeMetier(typeMetier)
      .then(() => {
        commit('EDIT_TYPEMETIER', typeMetier)
        const notification = {
          type: 'success',
          message: 'Un métier a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le métier existe déjà'
        } else {
          message = "Erreur à l'ajout d'un métier : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type de métier et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeMetier({ commit, dispatch }, typeMetierId) {
    return TypeMetierService.deleteTypeMetier(typeMetierId)
      .then(() => {
        commit('DELETE_TYPEMETIER')
        const notification = {
          type: 'success',
          message: 'Un métier a été supprimé !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un métier ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
