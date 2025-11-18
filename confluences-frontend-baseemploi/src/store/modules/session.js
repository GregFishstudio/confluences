/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 11.11.2022
 * Description : Gestion et stockage des sessions
 * Fichier : session.js
 **/

import SessionService from '@/services/sessionService.js'

export const namespaced = true

export const state = {
  sessions: [],
  session: {}
}

export const mutations = {
  SET_SESSIONS(state, sessions) {
    state.sessions = sessions
  },
  SET_SESSION(state, session) {
    state.session = session
  },
  ADD_SESSION(state, session) {
    state.session = session
    state.sessions.unshift({
      ...session
    })
  },
  EDIT_SESSION(state, session) {
    state.session = session
  },
  DELETE_SESSION(state) {
    state.session = null
  }
}

export const actions = {
  // Récupère les sessions et notifie l'utilisateur en cas de succès ou erreur
  fetchSessions({ commit, dispatch }) {
    return SessionService.getSessions()
      .then(response => {
        commit('SET_SESSIONS', response.data)
       
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème au chargement des sessions : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère une session spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchSession({ commit, dispatch }, id) {
    return SessionService.getSession(id)
      .then(response => {
        commit('SET_SESSION', response.data)
        const notification = {
          type: 'success',
          message: 'Session chargée'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème pour charger une session ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un session et notifie l'utilisateur en cas de succès ou erreur
  createSession({ commit, dispatch }, session) {
    return SessionService.postSession(session)
      .then(response => {
        commit('ADD_SESSION', response.data)
        const notification = {
          type: 'success',
          message: 'Une session a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'La session existe déjà'
        } else {
          message = "Erreur à l'ajout d'une session : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Met à jour une session et notifie l'utilisateur en cas de succès ou erreur
  editSession({ commit, dispatch }, session) {
    return SessionService.putSession(session)
      .then(() => {
        commit('EDIT_SESSION', session)
        const notification = {
          type: 'success',
          message: 'Une session a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'La session existe déjà'
        } else {
          message = "Erreur à l'ajout d'une session : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime une session et notifie l'utilisateur en cas de succès ou erreur
  deleteSession({ commit, dispatch }, sessionId) {
    return SessionService.deleteSession(sessionId.sessionId)
      .then(() => {
        commit('DELETE_SESSION')
        const notification = {
          type: 'success',
          message: 'Une session a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'une session ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  }
}
