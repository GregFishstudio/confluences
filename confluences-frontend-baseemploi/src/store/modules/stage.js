/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des stages
 * Fichier : stage.js
 **/

import StageService from '@/services/stageService.js'

export const namespaced = true

export const state = {
  stages: [],
  stage: {},
  filter: {
    nom: null,
    typeMetierId: null,
    entrepriseId: null,
    stagiaireId: null,
    year: null,
    dateDebut: null,
    dateFin: null,
    typeStageId: null,
    typeAnnonceId: null,
    typeIntershipActivityId: null,
    sessionId: null
  }
}

export const mutations = {
  SET_STAGES(state, stages) {
    state.stages = stages
  },
  SET_STAGE(state, stage) {
    state.stage = stage
  },
  ADD_STAGE(state, stage) {
    state.stages.push({
      ...stage
    })
  },
  EDIT_STAGE(state, stageNew) {
    state.stages = state.stages.filter(
      stage => stage.stageId !== stageNew.stageId
    )
    state.stages.push({
      ...stageNew
    })
  },
  DELETE_STAGE(state) {
    state.stage = null
  },
  SAVE_FILTER_STAGE(state, filter) {
    state.filter = filter
  },
  DELETE_FILTER_STAGE() {
    state.filter = {
      nom: null,
      typeMetierId: null,
      entrepriseId: null,
      stagiaireId: null,
      year: null,
      dateDebut: null,
      dateFin: null,
      typeStageId: null,
      typeAnnonceId: null,
      typeIntershipActivityId: null,
      sessionId: null
    }
  },
  ADD_STAGEFILE(state, file) {
    state.stage.stageFiles.push(file)
  },
  DELETE_STAGEFILE(state, id) {
    state.stage.stageFiles = state.stage.stageFiles.filter(
      file => file.stageFileId !== id
    )
  }
}

export const actions = {
  // Récupère les stages et notifie l'utilisateur en cas de succès ou erreur
  fetchStages({ commit, dispatch }) {
    return StageService.getStages()
      .then(response => {
        commit('SET_STAGES', response.data)
       
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème stages : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Export des stages
  fetchStagesExport({ dispatch }, filter) {
    return StageService.getStagesExport(filter)
      .then(response => {
        return response
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème export des stages : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un stage spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchStage({ commit, dispatch }, id) {
    return StageService.getStage(id)
      .then(response => {
        commit('SET_STAGE', response.data)
        const notification = {
          type: 'success',
          message: 'Stage chargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger un stage ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un stage spécifique et notifie l'utilisateur en cas de succès ou erreur
  createStage({ commit, dispatch }, stage) {
    return StageService.postStage(stage)
      .then(response => {
        commit('ADD_STAGE', stage)
        commit('SET_STAGE', response.data)
        const notification = {
          type: 'success',
          message: 'Un stage a été ajouté !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le stage existe déjà'
        } else {
          message = "Erreur à l'ajout d'un stage : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un stage spécifique et notifie l'utilisateur en cas de succès ou erreur
  editStage({ commit, dispatch }, stage) {
    return StageService.putStage(stage)
      .then(() => {
        commit('EDIT_STAGE', stage)
        const notification = {
          type: 'success',
          message: 'Un stage a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le stage existe déjà'
        } else {
          message = "Erreur à l'ajout d'un stage : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un stage spécifique et notifie l'utilisateur en cas de succès ou erreur
  deleteStage({ commit, dispatch }, stageId) {
    return StageService.deleteStage(stageId)
      .then(() => {
        commit('DELETE_STAGE')
        const notification = {
          type: 'success',
          message: 'Un stage a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un stage ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Sauvegarde le filtre du stage et l'applique
  saveFilterStage({ commit, dispatch }, filter) {
    commit('SAVE_FILTER_STAGE', filter)
    return StageService.getStagesWithFilter(filter)
      .then(response => {
        commit('SET_STAGES', response.data)
        
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème stages avec filtre : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Supprime le filtre du stage
  deleteFilterStage({ commit }) {
    commit('DELETE_FILTER_STAGE')
  },
  // Ajoute un fichier au stage
  addStageFile({ commit }, file) {
    commit('ADD_STAGEFILE', file)
  },
  deleteStageFile({ commit }, id) {
    commit('DELETE_STAGEFILE', id)
  }
}
