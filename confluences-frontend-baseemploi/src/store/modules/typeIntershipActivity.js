/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des types d'activités
 * Fichier : typeIntershipActivity.js
 **/

import TypeInternshipActivityService from '@/services/typeInternshipActivityService.js'

export const namespaced = true

export const state = {
  typeIntershipActivities: [],
  typeIntershipActivity: {}
}

export const mutations = {
  SET_TYPEINTERSHIPACTIVITIES(state, typeIntershipActivities) {
    state.typeIntershipActivities = typeIntershipActivities
  },
  SET_TYPEINTERNSHIPACTIVITY(state, typeIntershipActivity) {
    state.typeIntershipActivity = typeIntershipActivity
  },
  ADD_TYPEINTERNSHIPACTIVITY(state, typeIntershipActivityNew) {
    state.typeIntershipActivities.unshift({
      ...typeIntershipActivityNew
    })
  },
  EDIT_TYPEINTERNSHIPACTIVITY(state, typeIntershipActivityNew) {
    state.typeIntershipActivities = state.typeIntershipActivities.filter(
      typeIntershipActivity =>
        typeIntershipActivity.typeIntershipActivityId !==
        typeIntershipActivityNew.typeIntershipActivityId
    )
    state.typeIntershipActivities.unshift({
      ...typeIntershipActivityNew
    })
  },
  DELETE_TYPEINTERNSHIPACTIVITY(state) {
    state.typeIntershipActivity = null
  }
}

export const actions = {
  // Récupère les types d'activité pour un stage et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeIntershipActivities({ commit, dispatch }, notify) {
    return TypeInternshipActivityService.getTypeIntershipActivities()
      .then(response => {
        commit('SET_TYPEINTERSHIPACTIVITIES', response.data)
        if (notify == true) {
       
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Problème au chargement des types d'activités : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type d'activité spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeIntershipActivity({ commit, dispatch }, id) {
    return TypeInternshipActivityService.getTypeIntershipActivity(id)
      .then(response => {
        commit('SET_TYPEINTERNSHIPACTIVITY', response.data)
        const notification = {
          type: 'success',
          message: 'Activité chargée'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème pour charger une activité ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type d'activité et notifie l'utilisateur en cas de succès ou erreur
  createTypeIntershipActivity({ commit, dispatch }, typeIntershipActivity) {
    return TypeInternshipActivityService.postTypeIntershipActivity(
      typeIntershipActivity
    )
      .then(response => {
        commit('ADD_TYPEINTERNSHIPACTIVITY', response.data)
        commit('SET_TYPEINTERNSHIPACTIVITY', response.data)
        const notification = {
          type: 'success',
          message: 'Une activité a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'activité existe déjà"
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
  // Modifie un type d'activité et notifie l'utilisateur en cas de succès ou erreur
  editTypeIntershipActivity({ commit, dispatch }, typeIntershipActivity) {
    return TypeInternshipActivityService.putTypeIntershipActivity(
      typeIntershipActivity
    )
      .then(() => {
        commit('EDIT_TYPEINTERNSHIPACTIVITY', typeIntershipActivity)
        const notification = {
          type: 'success',
          message: 'Une activité a été modifiée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'activité existe déjà"
        } else {
          message = "Erreur à l'ajout d'une activité : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type d'activité et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeIntershipActivity({ commit, dispatch }, typeIntershipActivityId) {
    return TypeInternshipActivityService.deleteTypeIntershipActivity(
      typeIntershipActivityId
    )
      .then(() => {
        commit('DELETE_TYPEINTERNSHIPACTIVITY')
        const notification = {
          type: 'success',
          message: 'Une activité a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'une activité ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
