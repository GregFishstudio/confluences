/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des type de stages
 * Fichier : typeStage.js
 **/

import TypeStageService from '@/services/typeStageService.js'

export const namespaced = true

export const state = {
  typeStages: [],
  typeStage: {}
}

export const mutations = {
  SET_TYPESTAGES(state, typeStages) {
    state.typeStages = typeStages
  },
  SET_TYPESTAGE(state, typeStage) {
    state.typeStage = typeStage
  },
  ADD_TYPESTAGE(state, typeStageNew) {
    state.typeStages.unshift({
      ...typeStageNew
    })
  },
  EDIT_TYPESTAGE(state, typeStageNew) {
    state.typeStages = state.typeStages.filter(
      typeStage => typeStage.typeStageId !== typeStageNew.typeStageId
    )
    state.typeStages.unshift({
      ...typeStageNew
    })
  },
  DELETE_TYPESTAGE(state) {
    state.typeStage = null
  }
}

export const actions = {
  // Récupère les types de stage et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeStages({ commit, dispatch }, notify) {
    return TypeStageService.getTypeStages()
      .then(response => {
        commit('SET_TYPESTAGES', response.data)
        if (notify == true) {
          
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème au chargement des types stages : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type de stage spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeStage({ commit, dispatch }, id) {
    return TypeStageService.getTypeStage(id)
      .then(response => {
        commit('SET_TYPESTAGE', response.data)
        const notification = {
          type: 'success',
          message: 'Type de stage chargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger un type de stage' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type de stage et notifie l'utilisateur en cas de succès ou erreur
  createTypeStage({ commit, dispatch }, typeStage) {
    return TypeStageService.postTypeStage(typeStage)
      .then(response => {
        commit('ADD_TYPESTAGE', response.data)
        commit('SET_TYPESTAGE', response.data)
        const notification = {
          type: 'success',
          message: 'Un type stage a été ajouté !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le type de stage existe déjà'
        } else {
          message = "Erreur à l'ajout d'un type de stage : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type de stage et notifie l'utilisateur en cas de succès ou erreur
  editTypeStage({ commit, dispatch }, typeStage) {
    return TypeStageService.putTypeStage(typeStage)
      .then(() => {
        commit('EDIT_TYPESTAGE', typeStage)
        const notification = {
          type: 'success',
          message: 'Un type de stage a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le type de stage existe déjà'
        } else {
          message = "Erreur à l'ajout d'un type de stage : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type de stage et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeStage({ commit, dispatch }, typeStageId) {
    return TypeStageService.deleteTypeStage(typeStageId)
      .then(() => {
        commit('DELETE_TYPESTAGE')
        const notification = {
          type: 'success',
          message: 'Un type de stage a été supprimé !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Problème de suppression d'un type de stage ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
