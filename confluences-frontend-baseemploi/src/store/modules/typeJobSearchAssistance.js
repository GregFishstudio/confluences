/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 07.10.2022
 * Description : Gestion et stockage des ARE
 * Fichier : typeSearchAssistance.js
 **/

import TypeJobSearchAssistanceService from '@/services/typeJobSearchAssistancesService.js'

export const namespaced = true

export const state = {
  typeSearchAssistances: [],
  typeSearchAssistance: {}
}

export const mutations = {
  SET_TYPESEARCHASSISTANCES(state, typeSearchAssistances) {
    state.typeSearchAssistances = typeSearchAssistances
  },
  SET_TYPESEARCHASSISTANCE(state, typeSearchAssistance) {
    state.typeSearchAssistance = typeSearchAssistance
  },
  ADD_TYPESEARCHASSISTANCE(state, typeSearchAssistance) {
    state.typeSearchAssistance = typeSearchAssistance
    state.typeSearchAssistances.unshift({
      ...typeSearchAssistance
    })
  },
  EDIT_TYPESEARCHASSISTANCE(state, typeSearchAssistance) {
    state.typeSearchAssistance = typeSearchAssistance
  },
  DELETE_TYPESEARCHASSISTANCE(state) {
    state.typeSearchAssistance = null
  }
}

export const actions = {
  // Récupère les typeSearchAssistances et notifie l'utilisateur en cas de succès ou erreur
  // ✨ Recommended Improvement
fetchTypeJobSearchAssistances({ commit, dispatch }) {
  return TypeJobSearchAssistanceService.getTypeJobSearchAssistances()
    .then(response => {
      // 1. Success: Commit the fetched data
      commit('SET_TYPESEARCHASSISTANCES', response.data)
    })
    .catch(error => {
      // 2. Error: Commit an empty array to stabilize the state
      commit('SET_TYPESEARCHASSISTANCES', []) // <-- CRUCIAL FIX
      
      const notification = {
        type: 'error',
        message: 'Problème au chargement des type ARE : ' + error.message
      }
      dispatch('notification/add', notification, { root: true })
      // Optionally, you could use `throw error` here if components calling this action need to catch the error.
    })
},
  // Récupère un typeSearchAssistance spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeJobSearchAssistance({ commit, dispatch }, id) {
    return TypeJobSearchAssistanceService.getTypeJobSearchAssistance(id)
      .then(response => {
        commit('SET_TYPESEARCHASSISTANCE', response.data)
        const notification = {
          type: 'success',
          message: 'Type ARE chargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème pour charger un type ARE ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un typeSearchAssistance et notifie l'utilisateur en cas de succès ou erreur
  createTypeJobSearchAssistance({ commit, dispatch }, typeSearchAssistance) {
    return TypeJobSearchAssistanceService.postTypeJobSearchAssistance(
      typeSearchAssistance
    )
      .then(response => {
        commit('ADD_TYPESEARCHASSISTANCE', response.data)
        const notification = {
          type: 'success',
          message: 'Un type ARE a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le type ARE existe déjà'
        } else {
          message = "Erreur à l'ajout d'un type ARE : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Met à jour un typeSearchAssistance et notifie l'utilisateur en cas de succès ou erreur
  editTypeJobSearchAssistance({ commit, dispatch }, typeSearchAssistance) {
    return TypeJobSearchAssistanceService.putTypeJobSearchAssistance(
      typeSearchAssistance
    )
      .then(() => {
        commit('EDIT_TYPESEARCHASSISTANCE', typeSearchAssistance)
        const notification = {
          type: 'success',
          message: 'Un type ARE a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le type ARE existe déjà'
        } else {
          message = "Erreur à l'ajout d'un type ARE : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un typeSearchAssistance et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeJobSearchAssistance({ commit, dispatch }, typeSearchAssistanceId) {
    return TypeJobSearchAssistanceService.deleteTypeJobSearchAssistance(
      typeSearchAssistanceId
    )
      .then(() => {
        commit('DELETE_TYPESEARCHASSISTANCE')
        const notification = {
          type: 'success',
          message: 'Un type ARE a été supprimé !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un type ARE ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  }
}
