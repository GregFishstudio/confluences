/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des fichiers des stages
 * Fichier : stageFileService.js
 **/

import StageFileService from '@/services/stageFileService.js'

export const namespaced = true

export const state = {
  stageFile: {}
}

export const mutations = {
  SET_STAGEFILE(state, stageFile) {
    state.stageFile = stageFile
  },
  ADD_STAGEFILE(state, stageFile) {
    state.stageFile = stageFile
  },
  DELETE_STAGEFILE(state) {
    state.stageFile = null
  }
}

export const actions = {
  // Récupère un fichier spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchStageFile({ dispatch }, id) {
    return StageFileService.getStageFile(id)
      .then(response => {
        const notification = {
          type: 'success',
          message: 'Fichier téléchargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème pour charger un fichier ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un fichier et notifie l'utilisateur en cas de succès ou erreur
  createStageFile({ commit, dispatch }, stageFile) {
    return StageFileService.postStageFile(stageFile)
      .then(response => {
        commit('ADD_STAGEFILE', response.data)
        const notification = {
          type: 'success',
          message: 'Un fichier a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
        return response
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le fichier existe déjà'
        } else {
          message = "Erreur à l'ajout d'un fichier : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un fichier et notifie l'utilisateur en cas de succès ou erreur
  deleteStageFile({ commit, dispatch }, stageFileId) {
    return StageFileService.deleteStageFile(stageFileId.stageFileId)
      .then(() => {
        commit('DELETE_STAGEFILE')
        const notification = {
          type: 'success',
          message: 'Un fichier a été supprimé !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un fichier ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  }
}
