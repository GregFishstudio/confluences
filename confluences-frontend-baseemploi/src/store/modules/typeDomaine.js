/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des domaines
 * Fichier : typeDomaine.js
 **/

import TypeDomaineService from '@/services/typeDomaineService.js'

export const namespaced = true

export const state = {
  typeDomaines: [],
  typeDomaine: {}
}

export const mutations = {
  SET_TYPEDOMAINES(state, typeDomaines) {
    state.typeDomaines = typeDomaines
  },
  SET_TYPEDOMAINE(state, typeDomaine) {
    state.typeDomaine = typeDomaine
  },
  ADD_TYPEDOMAINE(state, typeDomaineNew) {
    state.typeDomaines.unshift({
      ...typeDomaineNew
    })
  },
  EDIT_TYPEDOMAINE(state, typeDomaineNew) {
    state.typeDomaines = state.typeDomaines.filter(
      typeDomaine => typeDomaine.typeDomaineId !== typeDomaineNew.typeDomaineId
    )
    state.typeDomaines.unshift({
      ...typeDomaineNew
    })
  },
  DELETE_TYPEDOMAINE(state) {
    state.typeDomaine = null
  }
}

export const actions = {
  // Récupère les domaines et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeDomaines({ commit, dispatch }, notify) {
    return TypeDomaineService.getTypeDomaines()
      .then(response => {
        commit('SET_TYPEDOMAINES', response.data)
        if (notify == true) {
          //const notification = {
          //  type: 'success',
          //  message: 'Domaines chargés'
          //}
          //dispatch('notification/add', notification, { root: true })
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            'Problème au chargement des types domaines : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type de domaine spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeDomaine({ commit, dispatch }, id) {
    return TypeDomaineService.getTypeDomaine(id)
      .then(response => {
        commit('SET_TYPEDOMAINE', response.data)
        const notification = {
          type: 'success',
          message: 'Domaine chargé'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger un domaine ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type de domaine et notifie l'utilisateur en cas de succès ou erreur
  createTypeDomaine({ commit, dispatch }, typeDomaine) {
    return TypeDomaineService.postTypeDomaine(typeDomaine)
      .then(response => {
        commit('ADD_TYPEDOMAINE', response.data)
        commit('SET_TYPEDOMAINE', response.data)
        const notification = {
          type: 'success',
          message: 'Un domaine a été ajouté !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le domaine existe déjà'
        } else {
          message = "Erreur à l'ajout d'un domaine : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type de domaine et notifie l'utilisateur en cas de succès ou erreur
  editTypeDomaine({ commit, dispatch }, typeDomaine) {
    return TypeDomaineService.putTypeDomaine(typeDomaine)
      .then(() => {
        commit('EDIT_TYPEDOMAINE', typeDomaine)
        const notification = {
          type: 'success',
          message: 'Un domaine a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le domaine existe déjà'
        } else {
          message = "Erreur à l'ajout d'un domaine : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type de domaine et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeDomaine({ commit, dispatch }, typeDomaineId) {
    return TypeDomaineService.deleteTypeDomaine(typeDomaineId)
      .then(() => {
        commit('DELETE_TYPEDOMAINE')
        const notification = {
          type: 'success',
          message: 'Un domaine a été supprimé !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un domaine ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
