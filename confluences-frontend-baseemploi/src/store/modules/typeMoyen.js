/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des moyens
 * Fichier : typeMoyen.js
 **/

import TypeMoyenService from '@/services/typeMoyenService.js'

export const namespaced = true

export const state = {
  typeMoyens: [],
  typeMoyen: {}
}

export const mutations = {
  SET_TYPEMOYENS(state, typeMoyens) {
    state.typeMoyens = typeMoyens
  },
  SET_TYPEMOYEN(state, typeMoyen) {
    state.typeMoyen = typeMoyen
  },
  ADD_TYPEMOYEN(state, typeMoyenNew) {
    state.typeMoyens.unshift({
      ...typeMoyenNew
    })
  },
  EDIT_TYPEMOYEN(state, typeMoyenNew) {
    state.typeMoyens = state.typeMoyens.filter(
      typeMoyen => typeMoyen.typeMoyenId !== typeMoyenNew.typeMoyenId
    )
    state.typeMoyens.unshift({
      ...typeMoyenNew
    })
  },
  DELETE_TYPEMOYEN(state) {
    state.typeMoyen = null
  }
}

export const actions = {
  // Récupère les types de moyens et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeMoyens({ commit, dispatch }, notify) {
    return TypeMoyenService.getTypeMoyens()
      .then(response => {
        commit('SET_TYPEMOYENS', response.data)
        if (notify == true) {
          
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème au chargement des types moyens : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type de moyen spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeMoyen({ commit, dispatch }, id) {
    return TypeMoyenService.getTypeMoyen(id)
      .then(response => {
        commit('SET_TYPEMOYEN', response.data)
        const notification = {
          type: 'success',
          message: 'Moyen chargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger un type de moyen ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type de moyen et notifie l'utilisateur en cas de succès ou erreur
  createTypeMoyen({ commit, dispatch }, typeMoyen) {
    return TypeMoyenService.postTypeMoyen(typeMoyen)
      .then(response => {
        commit('ADD_TYPEMOYEN', response.data)
        commit('SET_TYPEMOYEN', response.data)
        const notification = {
          type: 'success',
          message: 'Un moyen a été ajouté !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le moyen existe déjà'
        } else {
          message = "Erreur à l'ajout d'un moyen : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type de moyen et notifie l'utilisateur en cas de succès ou erreur
  editTypeMoyen({ commit, dispatch }, typeMoyen) {
    return TypeMoyenService.putTypeMoyen(typeMoyen)
      .then(() => {
        commit('EDIT_TYPEMOYEN', typeMoyen)
        const notification = {
          type: 'success',
          message: 'Un moyen a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le moyen existe déjà'
        } else {
          message = "Erreur à l'ajout d'un moyen : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type de moyen et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeMoyen({ commit, dispatch }, typeMoyenId) {
    return TypeMoyenService.deleteTypeMoyen(typeMoyenId)
      .then(() => {
        commit('DELETE_TYPEMOYEN')
        const notification = {
          type: 'success',
          message: 'Un moyen a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un moyen ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
