/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des stagiaires
 * Fichier : stagiaires.js
 **/

import StagiaireService from '@/services/stagiaireService.js'

export const namespaced = true

export const state = {
  stagiaires: [],
  stagiaire: {}
}

export const mutations = {
  SET_STAGIAIRES(state, stagiaires) {
    state.stagiaires = stagiaires
  },
  SET_STAGIAIRE(state, stagiaire) {
    state.stagiaire = stagiaire
  },
  EDIT_STAGIAIRE(state, stagiaireNew) {
    state.stagiaires = state.stagiaires.filter(
      stagiaire => stagiaire.stagiaireId !== stagiaireNew.stagiaireId
    )
    state.stagiaires.push({
      ...stagiaireNew
    })
  },
  DELETE_STAGIAIRE(state) {
    state.stagiaire = null
  }
}

export const actions = {
  // Récupère les stagiaires et notifie l'utilisateur en cas de succès ou erreur
  fetchStagiaires({ commit, dispatch }) {
    return StagiaireService.getStagiaires()
      .then(response => {
        commit('SET_STAGIAIRES', response.data)
        
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème stagiaires : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un stagiaire et notifie l'utilisateur en cas de succès ou erreur
  fetchStagiaire({ commit, dispatch }, id) {
    return StagiaireService.getStagiaire(id)
      .then(response => {
        commit('SET_STAGIAIRE', response.data)
        const notification = {
          type: 'success',
          message: 'Stagiaire chargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger un stagiaire ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Modifie un stagiaire et notifie l'utilisateur en cas de succès ou erreur
  editStagiaire({ commit, dispatch }, stagiaire) {
    return StagiaireService.putStagiaire(stagiaire)
      .then(() => {
        commit('EDIT_STAGIAIRE', stagiaire)
        const notification = {
          type: 'success',
          message: 'Un-e stagiaire a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le-la stagiaire existe déjà'
        } else {
          message = "Erreur à l'ajout d'un-e stagiaire : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un stagiaire et notifie l'utilisateur en cas de succès ou erreur
  deleteStagiaire({ commit, dispatch }, stagiaireId) {
    return StagiaireService.deleteStagiaire(stagiaireId)
      .then(() => {
        commit('DELETE_STAGIAIRE')
        const notification = {
          type: 'success',
          message: 'Un stagiaire a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un stagiaire ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
