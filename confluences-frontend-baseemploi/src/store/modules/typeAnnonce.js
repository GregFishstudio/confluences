/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des annonces
 * Fichier : typeAnnonce.js
 **/

import TypeAnnonceService from '@/services/typeAnnonceService.js'

export const namespaced = true

export const state = {
  typeAnnonces: [],
  typeAnnonce: {}
}

export const mutations = {
  SET_TYPEANNONCES(state, typeAnnonces) {
    state.typeAnnonces = typeAnnonces
  },
  SET_TYPEANNONCE(state, typeAnnonce) {
    state.typeAnnonce = typeAnnonce
  },
  ADD_TYPEANNONCE(state, typeAnnonceNew) {
    state.typeAnnonces.unshift({
      ...typeAnnonceNew
    })
  },
  EDIT_TYPEANNONCE(state, typeAnnonceNew) {
    state.typeAnnonces = state.typeAnnonces.filter(
      typeAnnonce => typeAnnonce.typeAnnonceId !== typeAnnonceNew.typeAnnonceId
    )
    state.typeAnnonces.unshift({
      ...typeAnnonceNew
    })
  },
  DELETE_TYPEANNONCE(state) {
    state.typeAnnonce = null
  }
}

export const actions = {
  // Récupère les annonces et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeAnnonces({ commit, dispatch }, notify) {
    return TypeAnnonceService.getTypeAnnonces()
      .then(response => {
        commit('SET_TYPEANNONCES', response.data)
        if (notify == true) {
       
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            'Problème au chargement des types annonces : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type d'annonce spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeAnnonce({ commit, dispatch }, id) {
    return TypeAnnonceService.getTypeAnnonce(id)
      .then(response => {
        commit('SET_TYPEANNONCE', response.data)
        const notification = {
          type: 'success',
          message: 'Annonce chargée'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger une annonce ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type d'annonce et notifie l'utilisateur en cas de succès ou erreur
  createTypeAnnonce({ commit, dispatch }, typeAnnonce) {
    return TypeAnnonceService.postTypeAnnonce(typeAnnonce)
      .then(response => {
        commit('ADD_TYPEANNONCE', response.data)
        commit('SET_TYPEANNONCE', response.data)
        const notification = {
          type: 'success',
          message: 'Une annonce a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'annonce existe déjà"
        } else {
          message = "Erreur à l'ajout d'une annonce : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type d'annonce et notifie l'utilisateur en cas de succès ou erreur
  editTypeAnnonce({ commit, dispatch }, typeAnnonce) {
    return TypeAnnonceService.putTypeAnnonce(typeAnnonce)
      .then(() => {
        commit('EDIT_TYPEANNONCE', typeAnnonce)
        const notification = {
          type: 'success',
          message: 'Une annonce a été modifiée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'annonce existe déjà"
        } else {
          message = "Erreur à l'ajout d'une annonce : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type d'annonce et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeAnnonce({ commit, dispatch }, typeAnnonceId) {
    return TypeAnnonceService.deleteTypeAnnonce(typeAnnonceId)
      .then(() => {
        commit('DELETE_TYPEANNONCE')
        const notification = {
          type: 'success',
          message: 'Une annonce a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'une annonce ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
