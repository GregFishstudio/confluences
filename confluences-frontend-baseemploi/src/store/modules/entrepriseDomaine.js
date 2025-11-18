/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des domaines des entreprises
 * Fichier : entrepriseDomaine.js
 **/

import EntrepriseDomaineService from '@/services/entrepriseDomaineService.js'

export const namespaced = true

export const state = {
  entrepriseDomaine: {}
}

export const mutations = {
  ADD_ENTREPRISEDOMAINE(state, entrepriseDomaine) {
    state.entrepriseDomaine = entrepriseDomaine
  }
}

export const actions = {
  // Ajoute un domaine pour une entreprise et notifie l'utilisateur en cas de succès ou erreur
  createEntrepriseDomaine({ commit, dispatch }, entrepriseDomaine) {
    return EntrepriseDomaineService.postEntrepriseDomaine(entrepriseDomaine)
      .then(() => {
        commit('ADD_ENTREPRISEDOMAINE', entrepriseDomaine)
        const notification = {
          type: 'success',
          message: 'Un domaine a été ajoutée !'
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
  // Supprime un domaine pour une entreprise et notifie l'utilisateur en cas de succès ou erreur
  deleteEntrepriseDomaine({ dispatch }, parameters) {
    return EntrepriseDomaineService.deleteEntrepriseDomaine(parameters)
      .then(() => {
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
