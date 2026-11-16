/**
 * Description : Gestion et stockage du marrainage
 * Fichier : marrainage.js
 **/

import MarrainageService from '@/services/marrainageService.js'

export const namespaced = true

export const state = {
  marrainages: [],
  marrainage: {}
}

export const mutations = {
  SET_MARRAINAGES(state, marrainages) {
    state.marrainages = marrainages
  },
  SET_MARRAINAGE(state, marrainage) {
    state.marrainage = marrainage
  },
  DELETE_MARRAINAGE(state) {
    state.marrainage = null
  }
}

export const actions = {
  // `isLatest` (optionnel) : renvoie false si une requête plus récente a été lancée entre-temps,
  // auquel cas cette réponse périmée n'écrase pas la liste.
  fetchMarrainages({ commit, dispatch }, { isLatest, ...params } = {}) {
    return MarrainageService.getMarrainages(params)
      .then(response => {
        if (isLatest && !isLatest()) return response.data
        commit('SET_MARRAINAGES', response.data)
        return response.data
      })
      .catch(error => {
        dispatch('notification/add', { type: 'error', message: 'Problème marrainages : ' + error.message }, { root: true })
      })
  },

  fetchMarrainage({ commit, dispatch }, id) {
    return MarrainageService.getMarrainage(id)
      .then(response => {
        commit('SET_MARRAINAGE', response.data)
        return response.data
      })
      .catch(error => {
        dispatch('notification/add', { type: 'error', message: 'Il y a un problème pour charger le marrainage : ' + error.message }, { root: true })
      })
  },

  createMarrainage({ dispatch }, marrainage) {
    return MarrainageService.postMarrainage(marrainage)
      .then(response => {
        dispatch('notification/add', { type: 'success', message: 'Marrainage créé !' }, { root: true })
        return response.data
      })
      .catch(error => {
        dispatch('notification/add', { type: 'error', message: "Erreur à la création du marrainage : " + error.message }, { root: true })
        throw error
      })
  },

  editMarrainage(_context, marrainage) {
    // Succès et échec sont affichés par la barre d'enregistrement de la fiche (PageSaveBar).
    return MarrainageService.putMarrainage(marrainage)
  },

  importMarrainages({ dispatch }, marrainages) {
    return MarrainageService.importMarrainages(marrainages)
      .then(response => {
        dispatch('notification/add', { type: 'success', message: `${marrainages.length} marrainage(s) importé(s) !` }, { root: true })
        return response.data
      })
      .catch(error => {
        dispatch('notification/add', { type: 'error', message: "Erreur à l'import des marrainages : " + error.message }, { root: true })
        throw error
      })
  },

  deleteMarrainage({ commit, dispatch }, id) {
    return MarrainageService.deleteMarrainage(id)
      .then(() => {
        commit('DELETE_MARRAINAGE')
        dispatch('notification/add', { type: 'success', message: 'Marrainage supprimé !' }, { root: true })
      })
      .catch(error => {
        dispatch('notification/add', { type: 'error', message: "Problème de suppression du marrainage : " + error.message }, { root: true })
        throw error
      })
  },

  clearMarrainages({ commit, dispatch }) {
    return MarrainageService.deleteAllMarrainages()
      .then(() => {
        commit('SET_MARRAINAGES', [])
        dispatch('notification/add', { type: 'success', message: 'Tous les marrainages ont été supprimés !' }, { root: true })
      })
      .catch(error => {
        dispatch('notification/add', { type: 'error', message: "Problème lors du vidage des marrainages : " + error.message }, { root: true })
        throw error
      })
  }
}
