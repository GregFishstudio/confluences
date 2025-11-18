/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020 (Mis à jour pour Vue 3/Vuex 4)
 * Description : Gestion et stockage de l'authentification
 * Fichier : authentification.js
 **/

import SecurityService from '@/security/index'
import axios from 'axios'

export const namespaced = true

// IMPORTANT: Nous devons initialiser SecurityService ici pour qu'il soit accessible
// Note : SecurityService utilise les variables d'environnement (process.env)
// que vous avez corrigées dans le fichier précédent.
var mgr = SecurityService

export const state = {
  user: null
}

export const mutations = {
  SET_USER_DATA(state, userData) {
    state.user = userData
  },
  // La mutation n'a pas besoin de vider l'état si l'action le fait déjà
  // mais la laisser comme "placeholder" est acceptable.
  CLEAR_USER_DATA(state) {
    state.user = null
    // Optionnel : Nettoyer le header d'axios pour les appels suivants
    delete axios.defaults.headers.common['Authorization']
  }
}

export const actions = {
  // Déconnecte l'utilisateur et avertit le serveur d'authentification
  async logout({ commit, state }) { // 💡 Changement : L'état (state) est passé en argument ici
    // Utiliser l'id_token pour la déconnexion si disponible
    const idToken = state.user ? state.user.id_token : null

    // Appel à la méthode signOut du service de sécurité
    // L'état est accédé via 'state' du contexte de l'action.
    mgr.signOut(idToken)

    commit('CLEAR_USER_DATA')
  },
  // Authentifie l'utilisateur
  async authenticate({ commit }) {
    const user = await mgr.getUser()
    
    // Si l'utilisateur est valide, définir le header d'autorisation global
    if (user && user.access_token) {
        axios.defaults.headers.common['Authorization'] =
          'Bearer ' + user.access_token
        commit('SET_USER_DATA', user)
    }
  }
}

export const getters = {
  // Indique si l'utilisateur est connecté
  loggedIn(state) {
    // Vérification plus robuste de l'objet user
    return state.user !== null && state.user !== undefined
  }
}