/**
* Projet: Gestion des stagiaires
* Auteur : Tim Allemann
* Date : 16.09.2020 (Mis à jour pour la gestion des présences)
* Description : Gestion et stockage des stagiaires
* Fichier : stagiaires.js
**/

import StagiaireService from '@/services/stagiaireService.js'

export const namespaced = true

export const state = {
 stagiaires: [],
 stagiaire: {}
}

export const mutations = {
 SET_STAGIAIRES(state, stagiaires) {
    // 1. Mise à jour : Assure que chaque stagiaire a un objet 'presences' initialisé pour la réactivité.
  state.stagiaires = stagiaires.map(s => ({
        ...s,
        presences: s.presences || {}
    }))
 },
 SET_STAGIAIRE(state, stagiaire) {
  state.stagiaire = stagiaire
 },
 EDIT_STAGIAIRE(state, stagiaireNew) {
    // La logique d'édition simple est conservée
    // Note: Pour une édition complète, il faudrait conserver les présences existantes.
  state.stagiaires = state.stagiaires.filter(
   stagiaire => stagiaire.stagiaireId !== stagiaireNew.stagiaireId
  )
  state.stagiaires.push({
   ...stagiaireNew
  })
 },
 DELETE_STAGIAIRE(state) {
  state.stagiaire = null
 },
    
  // 2. NOUVEAU: Mutation pour la mise à jour immédiate d'une case (toggleStatus)
  SET_ATTENDANCE_STATUS(state, { stagiaireId, date, status }) {
      const stagiaire = state.stagiaires.find(s => s.stagiaireId === stagiaireId)
      if (stagiaire) {
          if (!stagiaire.presences) stagiaire.presences = {}
          // La clé 'date' est au format toDateString (e.g., "Wed Nov 26 2025")
          stagiaire.presences[date] = status
      }
  },
    
  // 3. NOUVEAU: Mutation pour charger les présences depuis l'API au rechargement (persistance)
   UPDATE_STAGIAIRE_ATTENDANCES(state, { attendances, currentPeriodDays }) {
      state.stagiaires.forEach(stagiaire => {
          // Utiliser une copie pour déclencher la réactivité si l'objet racine est remplacé
          const newPresences = { ...stagiaire.presences } 
          
          // Nettoyer les données des jours affichés pour éviter les conflits lors de la recharge
          currentPeriodDays.forEach(day => {
              if (newPresences.hasOwnProperty(day)) {
                  delete newPresences[day]
              }
          })

          // Appliquer les nouvelles données de l'API (filtrées pour ce stagiaire)
          attendances.filter(a => a.stagiaireId === stagiaire.stagiaireId).forEach(a => {
              // CRUCIAL: Convertir la date ISO de l'API au format DateString
              const dateString = new Date(a.date).toDateString()
              newPresences[dateString] = a.statut
          })
          
          // Remplacer l'objet pour garantir la réactivité de l'objet entier 'presences'
          stagiaire.presences = newPresences
      })
  }
}

export const actions = {
 // Récupère les stagiaires (appelée dans beforeRouteEnter)
 fetchStagiaires({ commit, dispatch }) {
  return StagiaireService.getStagiaires()
   .then(response => {
    commit('SET_STAGIAIRES', response.data)
    
   })
   .catch(error => {
    const notification = {
     type: 'error',
     message: 'Problème stagiaires : ' + error.message
    }
    dispatch('notification/add', notification, { root: true })
   })
 },

  // NOUVEAU: Action pour appeler la mutation de mise à jour des présences (pour la persistance)
  updateStagiaireAttendances({ commit }, payload) {
      commit("UPDATE_STAGIAIRE_ATTENDANCES", payload)
  },

 // Récupère un stagiaire
 fetchStagiaire({ commit, dispatch }, id) {
  return StagiaireService.getStagiaire(id)
   .then(response => {
    commit('SET_STAGIAIRE', response.data)
    const notification = {
     type: 'success',
     message: 'Stagiaire chargé'
    }
    dispatch('notification/add', notification, { root: true })
    return response.data
   })
   .catch(error => {
    const notification = {
     type: 'error',
     message: 'Il y un problème charger un stagiaire ' + error.message
    }
    dispatch('notification/add', notification, { root: true })
   })
 },
 // Modifie un stagiaire
 editStagiaire({ commit, dispatch }, stagiaire) {
  return StagiaireService.putStagiaire(stagiaire)
   .then(() => {
    commit('EDIT_STAGIAIRE', stagiaire)
    const notification = {
     type: 'success',
     message: 'Un-e stagiaire a été modifié !'
    }
    dispatch('notification/add', notification, { root: true })
   })
   .catch(error => {
    let message = ''
    if (error.response && error.response.status == 409) {
     message = 'Le-la stagiaire existe déjà'
    } else {
     message = "Erreur à l'ajout d'un-e stagiaire : " + error.message
    }
    const notification = {
     type: 'error',
     message: message
    }
    dispatch('notification/add', notification, { root: true })
    throw error
   })
 },
 // Supprime un stagiaire
 deleteStagiaire({ commit, dispatch }, stagiaireId) {
  return StagiaireService.deleteStagiaire(stagiaireId)
   .then(() => {
    commit('DELETE_STAGIAIRE')
    const notification = {
     type: 'success',
     message: 'Un stagiaire a été supprimée !'
    }
    dispatch('notification/add', notification, { root: true })
   })
   .catch(error => {
    const notification = {
     type: 'error',
     message: "Problème de suppression d'un stagiaire ! : " + error.message
    }
    dispatch('notification/add', notification, { root: true })
   })
 }
}