/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 07.10.2022
 * Description : Gestion et stockage des ARE
 * Fichier : typeSearchAssistanceOccurrence.js
 **/

import TypeSearchAssistanceOccurrenceService from '@/services/typeJobSearchAssistanceOccurrencesService.js'

export const namespaced = true

export const state = {
  typeSearchAssistanceOccurrences: [],
  typeSearchAssistanceOccurrence: {}
}

export const mutations = {
  SET_TYPESEARCHASSISTANCEOCCURRENCEOCCURRENCES(
    state,
    typeSearchAssistanceOccurrences
  ) {
    state.typeSearchAssistanceOccurrences = typeSearchAssistanceOccurrences
  },
  SET_TYPESEARCHASSISTANCEOCCURRENCE(state, typeSearchAssistanceOccurrence) {
    state.typeSearchAssistanceOccurrence = typeSearchAssistanceOccurrence
  },
  ADD_TYPESEARCHASSISTANCEOCCURRENCE(state, typeSearchAssistanceOccurrence) {
    state.typeSearchAssistanceOccurrence = typeSearchAssistanceOccurrence
    state.typeSearchAssistanceOccurrences.unshift({
      ...typeSearchAssistanceOccurrence
    })
  },
  EDIT_TYPESEARCHASSISTANCEOCCURRENCE(state, typeSearchAssistanceOccurrence) {
    state.typeSearchAssistanceOccurrence = typeSearchAssistanceOccurrence
  },
  DELETE_TYPESEARCHASSISTANCEOCCURRENCE(state) {
    state.typeSearchAssistanceOccurrence = null
  }
}

export const actions = {
  // Récupère les typeSearchAssistanceOccurrences et notifie l'utilisateur en cas de succès ou erreur
  fetchtypeSearchAssistanceOccurrences({ commit, dispatch }) {
    return TypeSearchAssistanceOccurrenceService.getTypeJobSearchAssistanceOccurrences()
      .then(response => {
        commit('SET_TYPESEARCHASSISTANCEOCCURRENCEOCCURRENCES', response.data)
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            'Problème au chargement des occurences ARE : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un typeSearchAssistanceOccurrence spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchtypeSearchAssistanceOccurrence({ commit, dispatch }, id) {
    return TypeSearchAssistanceOccurrenceService.getTypeJobSearchAssistanceOccurrence(
      id
    )
      .then(response => {
        commit('SET_TYPESEARCHASSISTANCEOCCURRENCE', response.data)
        const notification = {
          type: 'success',
          message: 'Occurence ARE chargée'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            'Il y un problème pour charger une occurence ARE ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un typeSearchAssistanceOccurrence et notifie l'utilisateur en cas de succès ou erreur
  createtypeSearchAssistanceOccurrence(
    { commit, dispatch },
    typeSearchAssistanceOccurrence
  ) {
    return TypeSearchAssistanceOccurrenceService.postTypeJobSearchAssistanceOccurrence(
      typeSearchAssistanceOccurrence
    )
      .then(response => {
        commit('ADD_TYPESEARCHASSISTANCEOCCURRENCE', response.data)
        const notification = {
          type: 'success',
          message: 'Une occurence ARE a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'occurence ARE existe déjà"
        } else {
          message = "Erreur à l'ajout d'une occurence ARE : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Met à jour un typeSearchAssistanceOccurrence et notifie l'utilisateur en cas de succès ou erreur
  editTypeSearchAssistanceOccurrence(
    { commit, dispatch },
    typeSearchAssistanceOccurrence
  ) {
    return TypeSearchAssistanceOccurrenceService.putTypeJobSearchAssistanceOccurrence(
      typeSearchAssistanceOccurrence
    )
      .then(() => {
        commit(
          'EDIT_TYPESEARCHASSISTANCEOCCURRENCE',
          typeSearchAssistanceOccurrence
        )
        const notification = {
          type: 'success',
          message: 'Une occurence ARE a été modifiée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le type ARE existe déjà'
        } else {
          message = "Erreur à l'ajout d'une occurence ARE : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un typeSearchAssistanceOccurrence et notifie l'utilisateur en cas de succès ou erreur
  deletetypeSearchAssistanceOccurrence(
    { commit, dispatch },
    typeSearchAssistanceOccurrenceId
  ) {
    return TypeSearchAssistanceOccurrenceService.deleteTypeJobSearchAssistanceOccurrence(
      typeSearchAssistanceOccurrenceId
    )
      .then(() => {
        commit('DELETE_TYPESEARCHASSISTANCEOCCURRENCE')
        const notification = {
          type: 'success',
          message: 'Un occurence a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Problème de suppression d'une occurence ARE ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  }
}
