/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des contacts
 * Fichier : contact.js
 **/

import LastContactService from '@/services/lastContactService.js'

export const namespaced = true

export const state = {
  lastContacts: [],
  lastContact: {}
}

export const mutations = {
  SET_LASTCONTACTS(state, lastContacts) {
    state.lastContacts = lastContacts
  },
  SET_LASTCONTACT(state, lastContact) {
    state.lastContact = lastContact
  },
  ADD_LASTCONTACT(state, lastContact) {
    state.lastContact = lastContact
    state.lastContacts.unshift({
      ...lastContact
    })
  },
  EDIT_LASTCONTACT(state, lastContact) {
    state.lastContact = lastContact
  },
  DELETE_LASTCONTACT(state) {
    state.lastContact = null
  }
}

export const actions = {
  // Récupère les contacts et notifie l'utilisateur en cas de succès ou erreur
  fetchLastContacts({ commit, dispatch }) {
    return LastContactService.getLastContacts()
      .then(response => {
        commit('SET_LASTCONTACTS', response.data)
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            'Problème au chargement des derniers contacts : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère un contact spécifique et notifie l'utilisateur en cas de succès ou erreur
  fetchLastContact({ commit, dispatch }, id) {
    return LastContactService.getLastContact(id)
      .then(response => {
        commit('SET_LASTCONTACT', response.data)
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème pour charger un contact ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé un contact et notifie l'utilisateur en cas de succès ou erreur
  createLastContact({ commit, dispatch }, contact) {
    return LastContactService.postLastContact(contact)
      .then(response => {
        commit('ADD_LASTCONTACT', response.data)
        const notification = {
          type: 'success',
          message: 'Un contact a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le contact existe déjà'
        } else {
          message = "Erreur à l'ajout d'un contact : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Met à jour un contact et notifie l'utilisateur en cas de succès ou erreur
  editLastContact({ commit, dispatch }, contact) {
    return LastContactService.putLastContact(contact)
      .then(() => {
        commit('EDIT_LASTCONTACT', contact)
        const notification = {
          type: 'success',
          message: 'Un contact a été modifié !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = 'Le contact existe déjà'
        } else {
          message = "Erreur à l'ajout d'un contact : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime un contact et notifie l'utilisateur en cas de succès ou erreur
  deleteLastContact({ commit, dispatch }, contactId) {
    return LastContactService.deleteLastContact(contactId.contactId)
      .then(() => {
        commit('DELETE_LASTCONTACT')
        const notification = {
          type: 'success',
          message: 'Un contact a été supprimé !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: "Problème de suppression d'un contact ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  }
}
