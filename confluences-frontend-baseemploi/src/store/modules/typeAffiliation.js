/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des affilisations
 * Fichier : typeAffiliation.js
 **/

import TypeAffiliationService from '@/services/typeAffiliationService.js'

export const namespaced = true

export const state = {
  typeAffiliations: [],
  typeAffiliation: {}
}

export const mutations = {
  SET_TYPEAFFILIATIONS(state, typeAffiliations) {
    state.typeAffiliations = typeAffiliations
  },
  SET_TYPEAFFILIATION(state, typeAffiliation) {
    state.typeAffiliation = typeAffiliation
  },
  ADD_TYPEAFFILIATION(state, typeAffiliationNew) {
    state.typeAffiliations.unshift({
      ...typeAffiliationNew
    })
  },
  EDIT_TYPEAFFILIATION(state, typeAffiliationNew) {
    state.typeAffiliations = state.typeAffiliations.filter(
      typeAffiliation =>
        typeAffiliation.typeAffiliationId !==
        typeAffiliationNew.typeAffiliationId
    )
    state.typeAffiliations.unshift({
      ...typeAffiliationNew
    })
  },
  DELETE_TYPEAFFILIATION(state) {
    state.typeAffiliation = null
  }
}

export const actions = {
  // Récupère les affiliations et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeAffiliations({ commit, dispatch }, notify) {
    return TypeAffiliationService.getTypeAffiliations()
      .then(response => {
        commit('SET_TYPEAFFILIATIONS', response.data)
        if (notify == true) {
          
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Problème au chargement des types d'affiliations : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type d'affiliation spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeAffiliation({ commit, dispatch }, id) {
    return TypeAffiliationService.getTypeAffiliation(id)
      .then(response => {
        commit('SET_TYPEAFFILIATION', response.data)
        const notification = {
          type: 'success',
          message: 'Affiliation chargée'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger une affiliation ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type d'affiliation et notifie l'utilisateur en cas de succès ou erreur
  createTypeAffiliation({ commit, dispatch }, typeAffiliation) {
    return TypeAffiliationService.postTypeAffiliation(typeAffiliation)
      .then(response => {
        commit('ADD_TYPEAFFILIATION', typeAffiliation)
        commit('SET_TYPEAFFILIATION', response.data)
        const notification = {
          type: 'success',
          message: 'Une affiliation a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'affiliation existe déjà"
        } else {
          message = "Erreur à l'ajout d'une affiliation : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type d'affiliation et notifie l'utilisateur en cas de succès ou erreur
  editTypeAffiliation({ commit, dispatch }, typeAffiliation) {
    return TypeAffiliationService.putTypeAffiliation(typeAffiliation)
      .then(() => {
        commit('EDIT_TYPEAFFILIATION', typeAffiliation)
        const notification = {
          type: 'success',
          message: 'Une affiliation a été modifiée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'affiliation existe déjà"
        } else {
          message = "Erreur à l'ajout d'une affiliation : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type d'affiliation et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeAffiliation({ commit, dispatch }, typeAffiliationId) {
    return TypeAffiliationService.deleteTypeAffiliation(typeAffiliationId)
      .then(() => {
        commit('DELETE_TYPEAFFILIATION')
        const notification = {
          type: 'success',
          message: 'Une affiliation a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Problème de suppression d'une affiliation ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
