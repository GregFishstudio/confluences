/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des ARE
 * Fichier : jobSearchAssistance.js
 **/

import JobSearchAssistancesService from '@/services/jobSearchAssistancesService.js'

export const namespaced = true

export const state = {
  jobSearchAssistances: [],
  jobSearchAssistance: {}
}

export const mutations = {
  SET_JOBSEARCHASSISTANCES(state, jobSearchAssistances) {
    state.jobSearchAssistances = jobSearchAssistances
  },
  SET_JOBSEARCHASSISTANCE(state, jobSearchAssistance) {
    state.jobSearchAssistance = jobSearchAssistance
  },
  ADD_JOBSEARCHASSISTANCE(state, jobSearchAssistance) {
    state.jobSearchAssistance = jobSearchAssistance
    state.jobSearchAssistances.unshift({
      ...jobSearchAssistance
    })
  },
  EDIT_JOBSEARCHASSISTANCE(state, jobSearchAssistance) {
    state.jobSearchAssistance = jobSearchAssistance
  },
  DELETE_JOBSEARCHASSISTANCE(state) {
    state.jobSearchAssistance = null
  }
}

export const actions = {
  // Récupère les jobSearchAssistances et notifie l'utilisateur en cas de succès ou erreur
  fetchJobSearchAssistances({ commit, dispatch }) {
    return JobSearchAssistancesService.getJobSearchAssistances()
      .then(response => {
        commit('SET_JOBSEARCHASSISTANCES', response.data)
        
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème au chargement des ARE : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un jobSearchAssistance spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchJobSearchAssistance({ commit, dispatch }, id) {
    return JobSearchAssistancesService.getJobSearchAssistance(id)
      .then(response => {
        commit('SET_JOBSEARCHASSISTANCE', response.data)
        
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème pour charger un ARE ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un jobSearchAssistance et notifie l'utilisateur en cas de succès ou erreur
  createJobSearchAssistance({ commit, dispatch }, jobSearchAssistance) {
    return JobSearchAssistancesService.postJobSearchAssistance(
      jobSearchAssistance
    )
      .then(response => {
        commit('ADD_JOBSEARCHASSISTANCE', response.data)
        const notification = {
          type: 'success',
          message: 'Un ARE a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le ARE existe déjà'
        } else {
          message = "Erreur à l'ajout d'un ARE : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Met à jour un jobSearchAssistance et notifie l'utilisateur en cas de succès ou erreur
  editJobSearchAssistance({ commit, dispatch }, jobSearchAssistance) {
    return JobSearchAssistancesService.putJobSearchAssistance(
      jobSearchAssistance
    )
      .then(() => {
        commit('EDIT_JOBSEARCHASSISTANCE', jobSearchAssistance)
        const notification = {
          type: 'success',
          message: 'Un ARE a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le ARE existe déjà'
        } else {
          message = "Erreur à l'ajout d'un ARE : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un jobSearchAssistance et notifie l'utilisateur en cas de succès ou erreur
  deleteJobSearchAssistance({ commit, dispatch }, jobSearchAssistanceId) {
    return JobSearchAssistancesService.deleteJobSearchAssistance(
      jobSearchAssistanceId.jobSearchAssistanceId
    )
      .then(() => {
        commit('DELETE_JOBSEARCHASSISTANCE')
        const notification = {
          type: 'success',
          message: 'Un ARE a été supprimé !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un ARE ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  }
}
