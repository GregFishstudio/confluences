/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020
 * Description : Configuration générale
 * Fichier : settings.js (Mis à jour pour Vue 3 / import.meta.env)
 **/

export const namespaced = true

export const state = {
  // CORRECTION : Remplacer process.env par import.meta.env
  authorityUrl: import.meta.env.VUE_APP_AUTHORITY_URL,
  applicationUrl: import.meta.env.VUE_APP_APPLICATION_URL,
  apiUrl: import.meta.env.VITE_API_URL,
  
  itemsPerPage: 10,
  currentPageEntreprise: 1,
  currentPageStage: 1,
  currentPageStagiaire: 1,
  currentPageContact: 1,
  currentPageTypeMetier: 1,
  currentPageTypeAffiliation: 1,
  currentPageTypeAnnonce: 1,
  currentPageTypeDomaine: 1,
  currentPageTypeEntreprise: 1,
  currentPageTypeStage: 1,
  currentPageTypeMoyen: 1,
  currentPageTypeOffre: 1,
  currentPageTypeIntershipActivity: 1,
  currentPageJobSearchAssistance: 1
}

// ... (mutations et actions restent inchangées car elles n'utilisent pas process.env)

export const mutations = {
  SET_ITEMSPERPAGE(state, number) {
    state.itemsPerPage = number
  },
  SET_CURRENTPAGEENTREPRISE(state, number) {
    state.currentPageEntreprise = number
  },
  // ... (Toutes les autres mutations) ...
  SET_CURRENTPAGESTAGE(state, number) {
    state.currentPageStage = number
  },
  SET_CURRENTPAGESTAGIAIRE(state, number) {
    state.currentPageStagiaire = number
  },
  SET_CURRENTPAGECONTACT(state, number) {
    state.currentPageContact = number
  },
  SET_CURRENTPAGETYPEMETIER(state, number) {
    state.currentPageTypeMetier = number
  },
  SET_CURRENTPAGETYPEAFFILIATION(state, number) {
    state.currentPageTypeAffiliation = number
  },
  SET_CURRENTPAGETYPEANNONCE(state, number) {
    state.currentPageTypeAnnonce = number
  },
  SET_CURRENTPAGETYPEDOMAINE(state, number) {
    state.currentPageTypeDomaine = number
  },
  SET_CURRENTPAGETYPEENTREPRISE(state, number) {
    state.currentPageTypeEntreprise = number
  },
  SET_CURRENTPAGETYPESTAGE(state, number) {
    state.currentPageTypeStage = number
  },
  SET_CURRENTPAGETYPEMOYEN(state, number) {
    state.currentPageTypeMoyen = number
  },
  SET_CURRENTPAGETYPEOFFRE(state, number) {
    state.currentPageTypeOffre = number
  },
  SET_CURRENTPAGETYPEINTERSHIPACTIVITY(state, number) {
    state.currentPageTypeIntershipActivity = number
  },
  SET_CURRENTPAGEJOBSEARCHASSISTANCE(state, number) {
    state.currentPageJobSearchAssistance = number
  }
}

export const actions = {
  // Affecte le nombre d'élément à afficher dans un tableau
  setItemsPerPage({ commit }, number) {
    commit('SET_ITEMSPERPAGE', number.number)
  },
  // Affecte le numéro de page consulté du tableau d'entreprises
  setCurrentPageEntreprise({ commit }, number) {
    commit('SET_CURRENTPAGEENTREPRISE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des stages
  setCurrentPageStage({ commit }, number) {
    commit('SET_CURRENTPAGESTAGE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des stagiaires
  setCurrentPageStagiaire({ commit }, number) {
    commit('SET_CURRENTPAGESTAGIAIRE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des contacts
  setCurrentPageContact({ commit }, number) {
    commit('SET_CURRENTPAGECONTACT', number.number)
  },
  // Affecte le numéro de page consulté du tableau des métiers
  setCurrentPageTypeMetier({ commit }, number) {
    commit('SET_CURRENTPAGETYPEMETIER', number.number)
  },
  // Affecte le numéro de page consulté du tableau des affiliations
  setCurrentPageTypeAffiliation({ commit }, number) {
    commit('SET_CURRENTPAGETYPEAFFILIATION', number.number)
  },
  // Affecte le numéro de page consulté du tableau des annonces
  setCurrentPageTypeAnnonce({ commit }, number) {
    commit('SET_CURRENTPAGETYPEANNONCE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des domaines
  setCurrentPageTypeDomaine({ commit }, number) {
    commit('SET_CURRENTPAGETYPEDOMAINE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des types d'entreprises
  setCurrentPageTypeEntreprise({ commit }, number) {
    commit('SET_CURRENTPAGETYPEENTREPRISE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des types de stages
  setCurrentPageTypeStage({ commit }, number) {
    commit('SET_CURRENTPAGETYPESTAGE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des types de moyens
  setCurrentPageTypeMoyen({ commit }, number) {
    commit('SET_CURRENTPAGETYPEMOYEN', number.number)
  },
  // Affecte le numéro de page consulté du tableau des types d'offre
  setCurrentPageTypeOffre({ commit }, number) {
    commit('SET_CURRENTPAGETYPEOFFRE', number.number)
  },
  // Affecte le numéro de page consulté du tableau des types d'activité
  setCurrentPageTypeIntershipActivity({ commit }, number) {
    commit('SET_CURRENTPAGETYPEINTERSHIPACTIVITY', number.number)
  },
  // Affecte le numéro de page consulté du tableau des ARE
  setCurrentPageJobSearchAssistance({ commit }, number) {
    commit('SET_CURRENTPAGEJOBSEARCHASSISTANCE', number.number)
  }
}