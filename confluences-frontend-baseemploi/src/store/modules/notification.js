/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des notifications
 * Fichier : notification.js
 **/

export const namespaced = true

export const state = {
  notifications: []
}

let nextId = 1
export const mutations = {
  ADD_NOTIFICATION(state, notifications) {
    state.notifications.push({
      ...notifications,
      id: nextId++
    })
  },
  DELETE_NOTIFICATION(state, notificationToRemove) {
    state.notifications = state.notifications.filter(
      notification => notification.id !== notificationToRemove.id
    )
  }
}

export const actions = {
  add({ commit }, notification) {
    commit('ADD_NOTIFICATION', notification)
  },
  remove({ commit }, notificationToRemove) {
    commit('DELETE_NOTIFICATION', notificationToRemove)
  }
}
