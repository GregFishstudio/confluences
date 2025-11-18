/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des utilisateurs
 * Fichier : user.js
 **/

import UserService from '@/services/userService.js'

export const namespaced = true

export const state = {
  users: []
}

export const mutations = {
  SET_USERS(state, users) {
    state.users = users
  }
}

export const actions = {
  // Récupère les utilisateurs et notifie l'utilisateur en cas de succès ou erreur
  fetchUsers({ commit, dispatch }) {
    return UserService.getUsers()
      .then(response => {
        commit('SET_USERS', response.data)
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème au chargement des utilisateurs : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
