/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Gestion et stockage des entreprises
 * Fichier : entreprise.js
 **/

import EntrepriseService from '@/services/entrepriseService.js'

export const namespaced = true

export const state = {
  entreprises: [],
  entreprise: {},
  filter: {
    metiers: [],
    domaines: [],
    offres: [],
    city: null,
    codePostal: null,
    nom: null,
    dateModification: null,
    createur: null
  }
}

export const mutations = {
  SET_ENTREPRISES(state, entreprises) {
    state.entreprises = entreprises
  },
  SET_ENTREPRISE(state, entreprise) {
    state.entreprise = entreprise
  },
  ADD_ENTREPRISE(state, entrepriseNew) {
    state.entreprises.push({
      ...entrepriseNew
    })
  },
  DELETE_ENTREPRISE(state) {
    state.entreprise = null
  },
  SET_OFFRE_ENTREPRISE(state, offre) {
    state.entreprise.entrepriseOffres.push({
      ...offre
    })
  },
  DELETE_OFFRE_ENTREPRISE(state, offreId) {
    state.entreprise.entrepriseOffres = state.entreprise.entrepriseOffres.filter(
      entrepriseOffre => entrepriseOffre.typeOffreId !== offreId
    )
  },
  SET_DOMAINE_ENTREPRISE(state, domaine) {
    state.entreprise.entrepriseDomaines.push({
      ...domaine
    })
  },
  DELETE_DOMAINE_ENTREPRISE(state, domaineId) {
    state.entreprise.entrepriseDomaines = state.entreprise.entrepriseDomaines.filter(
      entrepriseDomaine => entrepriseDomaine.typeDomaineId !== domaineId
    )
  },
  SET_METIER_ENTREPRISE(state, metier) {
    state.entreprise.entrepriseMetiers.push({
      ...metier
    })
  },
  EDIT_ENTREPRISE(state, entrepriseNew) {
    state.entreprises = state.entreprises.filter(
      entreprise => entreprise.entrepriseId !== entrepriseNew.entrepriseId
    )
    state.entreprises.push({
      ...entrepriseNew
    })
  },
  DELETE_METIER_ENTREPRISE(state, metierId) {
    state.entreprise.entrepriseMetiers = state.entreprise.entrepriseMetiers.filter(
      entrepriseMetier => entrepriseMetier.typeMetierId !== metierId
    )
  },
  SET_CONTACT(state, contact) {
    state.entreprise.contacts.push({
      ...contact
    })
  },
  DELETE_CONTACT(state, contactId) {
    state.entreprise.contacts = state.entreprise.contacts.filter(
      contact => contact.contactId !== contactId
    )
  },
  EDIT_CONTACT(state, contactNew) {
    state.entreprise.contacts = state.entreprise.contacts.filter(
      contact => contact.contactId !== contactNew.contactId
    )
    state.entreprise.contacts.push({
      ...contactNew
    })
  },

  SET_LASTCONTACT(state, lastContact) {
    state.entreprise.lastContacts.push({
      ...lastContact
    })
  },
  DELETE_LASTCONTACT(state, lastContactId) {
    state.entreprise.lastContacts = state.entreprise.lastContacts.filter(
      lastContact => lastContact.lastContactId !== lastContactId
    )
  },
  EDIT_LASTCONTACT(state, lastContactNew) {
    state.entreprise.lastContacts = state.entreprise.lastContacts.filter(
      lastContact => lastContact.lastContactId !== lastContactNew.lastContactId
    )
    state.entreprise.lastContacts.push({
      ...lastContactNew
    })
  },

  ADD_FILTER_METIER_ENTREPRISE(state, metier) {
    state.filter.metiers.push({
      ...metier
    })
  },
  ADD_FILTER_OFFRE_ENTREPRISE(state, offre) {
    state.filter.offres.push({
      ...offre
    })
  },
  ADD_FILTER_DOMAINE_ENTREPRISE(state, domaine) {
    state.filter.domaines.push({
      ...domaine
    })
  },
  SAVE_FILTER_ENTREPRISE(state, filter) {
    state.filter = filter
  },
  DELETE_FILTER_ENTREPRISE() {
    state.filter = {
      metiers: [],
      domaines: [],
      offres: [],
      city: null,
      codePostal: null,
      nom: null,
      dateModification: null,
      createur: null
    }
  },
  DELETE_FILTER_METIER(state, typeMetierId) {
    state.filter.metiers = state.filter.metiers.filter(
      metier => metier.typeMetierId !== typeMetierId
    )
  },
  DELETE_FILTER_OFFRE(state, typeOffreId) {
    state.filter.offres = state.filter.offres.filter(
      offre => offre.typeOffreId !== typeOffreId
    )
  },
  DELETE_FILTER_DOMAINE(state, typeDomaineId) {
    state.filter.domaines = state.filter.domaines.filter(
      domaine => domaine.typeDomaineId !== typeDomaineId
    )
  }
}

export const actions = {
  // Récupère les entreprises et notifie l'utilisateur en cas de succès ou erreur
  fetchEntreprises({ commit, dispatch }, notify) {
    return EntrepriseService.getEntreprises()
      .then(response => {
        commit('SET_ENTREPRISES', response.data)
        if (notify == true) {
          // const notification = {
          //  type: 'success',
          // message: 'Entreprises chargées'
          //}
          //dispatch('notification/add', notification, { root: true })
        }
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème entreprises : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Récupère une entreprise et notifie l'utilisateur en cas de succès ou erreur
  fetchEntreprise({ commit, dispatch }, id) {
    return EntrepriseService.getEntreprise(id)
      .then(response => {
        commit('SET_ENTREPRISE', response.data)
        const notification = {
          type: 'success',
          message: 'Entreprise chargée'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Il y un problème charger une entreprise ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Créé une entreprise et notifie l'utilisateur en cas de succès ou erreur
  createEntreprise({ commit, dispatch }, entreprise) {
    return EntrepriseService.postEntreprise(entreprise)
      .then(response => {
        commit('ADD_ENTREPRISE', entreprise)
        commit('SET_ENTREPRISE', response.data)
        const notification = {
          type: 'success',
          message: 'Une entreprise a été ajoutée !'
        }
        dispatch('notification/add', notification, { root: true })
        return response.data
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'entreprise existe déjà"
        } else {
          message = "Erreur à l'ajout d'une entreprise : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Modifie une entreprise et notifie l'utilisateur en cas de succès ou erreur
  editEntreprise({ commit, dispatch }, entreprise) {
    console.log(entreprise)
    return EntrepriseService.putEntreprise(entreprise)
      .then(() => {
        commit('EDIT_ENTREPRISE', entreprise)
        const notification = {
          type: 'success',
          message: 'Une entreprise a été modifiée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        let message = ''
        if (error.response && error.response.status == 409) {
          message = "L'entreprise existe déjà"
        } else {
          message = "Erreur à l'ajout d'une entreprise : " + error.message
        }
        const notification = {
          type: 'error',
          message: message
        }
        dispatch('notification/add', notification, { root: true })
        throw error
      })
  },
  // Supprime une entreprise et notifie l'utilisateur en cas de succès ou erreur
  deleteEntreprise({ commit, dispatch }, entrepriseId) {
    return EntrepriseService.deleteEntreprise(entrepriseId)
      .then(() => {
        commit('DELETE_ENTREPRISE')
        const notification = {
          type: 'success',
          message: 'Une entreprise a été supprimée !'
        }
        dispatch('notification/add', notification, { root: true })
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message:
            "Problème de suppression d'une entreprise ! : " + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Ajoute une offre à l'entreprise
  addOffre({ commit }, offre) {
    commit('SET_OFFRE_ENTREPRISE', offre)
  },
  // Supprime une offre à l'entreprise
  deleteOffre({ commit }, offreId) {
    commit('DELETE_OFFRE_ENTREPRISE', offreId)
  },
  // Ajoute un métier à l'entreprise
  addMetier({ commit }, offre) {
    commit('SET_METIER_ENTREPRISE', offre)
  },
  // Supprime un métier à l'entreprise
  deleteMetier({ commit }, offreId) {
    commit('DELETE_METIER_ENTREPRISE', offreId)
  },
  // Ajoute un domaine à l'entreprise
  addDomaine({ commit }, domaine) {
    commit('SET_DOMAINE_ENTREPRISE', domaine)
  },
  // Supprime un domaine à l'entreprise
  deleteDomaine({ commit }, domaineId) {
    commit('DELETE_DOMAINE_ENTREPRISE', domaineId)
  },
  // Ajoute un contact à l'entreprise
  addContact({ commit }, contact) {
    commit('SET_CONTACT', contact)
  },
  // Supprime un contact à l'entreprise
  deleteContact({ commit }, contactId) {
    commit('DELETE_CONTACT', contactId)
  },
  // Met à jour un contact à l'entreprise
  editContact({ commit }, contact) {
    commit('EDIT_CONTACT', contact)
  },
  // Ajoute un dernier contact à l'entreprise
  addLastContact({ commit }, lastContact) {
    commit('SET_LASTCONTACT', lastContact)
  },
  // Supprime un dernier contact à l'entreprise
  deleteLastContact({ commit }, lastContactId) {
    commit('DELETE_LASTCONTACT', lastContactId)
  },
  // Met à jour un dernier contact à l'entreprise
  editLastContact({ commit }, lastContact) {
    commit('EDIT_LASTCONTACT', lastContact)
  },
  // Ajoute un métier au filtre de l'entreprise
  addFilterEntrepriseMetier({ commit }, metier) {
    commit('ADD_FILTER_METIER_ENTREPRISE', metier)
  },
  // Ajoute une offre au filtre de l'entreprise
  addFilterEntrepriseOffre({ commit }, offre) {
    commit('ADD_FILTER_OFFRE_ENTREPRISE', offre)
  },
  // Ajoute un domaine au filtre de l'entreprise
  addFilterEntrepriseDomaine({ commit }, domaine) {
    commit('ADD_FILTER_DOMAINE_ENTREPRISE', domaine)
  },
  // Sauvegarde le filtre de l'entreprise et l'applique
  saveFilterEntreprise({ commit, dispatch }, filter) {
    commit('SAVE_FILTER_ENTREPRISE', filter)
    return EntrepriseService.getEntreprisesWithFilter(filter)
      .then(response => {
        commit('SET_ENTREPRISES', response.data)
        
      })
      .catch(error => {
        const notification = {
          type: 'error',
          message: 'Problème entreprises avec filtre : ' + error.message
        }
        dispatch('notification/add', notification, { root: true })
      })
  },
  // Supprime le filtre de l'entreprise
  deleteFilterEntreprise({ commit }) {
    commit('DELETE_FILTER_ENTREPRISE')
  },
  // Supprime un métier du filtre de l'entreprise
  deleteFilterMetier({ commit }, typeMetierId) {
    commit('DELETE_FILTER_METIER', typeMetierId)
  },
  // Supprime un domaine du filtre de l'entreprise
  deleteFilterDomaine({ commit }, typeDomaineId) {
    commit('DELETE_FILTER_DOMAINE', typeDomaineId)
  },
  // Supprime une offre du filtre de l'entreprise
  deleteFilterOffre({ commit }, typeOffreId) {
    commit('DELETE_FILTER_OFFRE', typeOffreId)
  }
}
