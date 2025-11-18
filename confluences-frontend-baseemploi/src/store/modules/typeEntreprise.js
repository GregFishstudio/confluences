/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des entreprises
 * Fichier : typeEntreprise.js
 **/

import TypeEntrepriseService from '@/services/typeEntrepriseService.js'

export const namespaced = true

export const state = {
  typeEntreprises: [],
  typeEntreprise: {}
}

export const mutations = {
  SET_TYPEENTREPRISES(state, typeEntreprises) {
    state.typeEntreprises = typeEntreprises
  },
  SET_TYPEENTREPRISE(state, typeEntreprise) {
    state.typeEntreprise = typeEntreprise
  },
  ADD_TYPEENTREPRISE(state, typeEntrepriseNew) {
    state.typeEntreprises.unshift({
      ...typeEntrepriseNew
    })
  },
  EDIT_TYPEENTREPRISE(state, typeEntrepriseNew) {
    state.typeEntreprises = state.typeEntreprises.filter(
      typeEntreprise =>
        typeEntreprise.typeEntrepriseId !== typeEntrepriseNew.typeEntrepriseId
    )
    state.typeEntreprises.unshift({
      ...typeEntrepriseNew
    })
  },
  DELETE_TYPEENTREPRISE(state) {
    state.typeEntreprise = null
  }
}

export const actions = {
  // Récupère les types d'entreprise et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeEntreprises({ commit, dispatch }, notify) {
    return TypeEntrepriseService.getTypeEntreprises()
      .then(response => {
        commit('SET_TYPEENTREPRISES', response.data)
        if (notify == true) {
          
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            'Problème au chargement des types entreprises : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un type d'entreprise spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchTypeEntreprise({ commit, dispatch }, id) {
    return TypeEntrepriseService.getTypeEntreprise(id)
      .then(response => {
        commit('SET_TYPEENTREPRISE', response.data)
        const notification = {
          type: 'success',
          message: "Type d'entreprise chargé"
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Il y un problème charger un type d'entreprise" + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un type d'entreprise et notifie l'utilisateur en cas de succès ou erreur
  createTypeEntreprise({ commit, dispatch }, typeEntreprise) {
    return TypeEntrepriseService.postTypeEntreprise(typeEntreprise)
      .then(response => {
        commit('ADD_TYPEENTREPRISE', response.data)
        commit('SET_TYPEENTREPRISE', response.data)
        const notification = {
          type: 'success',
          message: "Un type d'entreprise a été ajouté !"
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "Le type d'entreprise existe déjà"
        } else {
          message = "Erreur à l'ajout d'un type d'entreprise : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie un type d'entreprise et notifie l'utilisateur en cas de succès ou erreur
  editTypeEntreprise({ commit, dispatch }, typeEntreprise) {
    return TypeEntrepriseService.putTypeEntreprise(typeEntreprise)
      .then(() => {
        commit('EDIT_TYPEENTREPRISE', typeEntreprise)
        const notification = {
          type: 'success',
          message: "Un type d'entreprise a été modifié !"
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "Le type d'entreprise existe déjà"
        } else {
          message = "Erreur à l'ajout d'un type d'entreprise : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un type d'entreprise et notifie l'utilisateur en cas de succès ou erreur
  deleteTypeEntreprise({ commit, dispatch }, typeEntrepriseId) {
    return TypeEntrepriseService.deleteTypeEntreprise(typeEntrepriseId)
      .then(() => {
        commit('DELETE_TYPEENTREPRISE')
        const notification = {
          type: 'success',
          message: "Un type d'entreprise a été supprimé !"
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Problème de suppression d'un type d'entreprise ! : " +
            error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  }
}
