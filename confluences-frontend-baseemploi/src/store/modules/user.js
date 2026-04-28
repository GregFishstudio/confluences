/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020 (Mis à jour le 08.04.2026)
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
    // Normalise les clés PascalCase de l'API en camelCase + initialise presences
    state.users = users.map(u => ({
      ...u,
      id:                     u.id                     || u.StagiaireId,
      prenom:                 u.prenom                 || u.Prenom                 || '',
      nom:                    u.nom                    || u.Nom                    || '',
      userName:               u.userName               || u.Username               || u.UserName || '',
      niveau:                 u.niveau                 || u.Niveau                 || '',
      isArchived:             u.isArchived             ?? u.IsArchived             ?? false,
      typeAffiliationId:      u.typeAffiliationId      ?? u.TypeAffiliationId      ?? 0,
      dateDebutParticipation: u.DateDebutParticipation || u.dateDebutParticipation || null,
      presences: u.presences || {}
    }))
  },

  // Mise à jour d'une case de présence (appelée par updateStatus)
  SET_ATTENDANCE_STATUS(state, { userId, date, status }) {
    const user = state.users.find(u => u.id === userId)
    if (user) {
      if (!user.presences) user.presences = {}
      user.presences[date] = status
    }
  },

  // Mise à jour d'un utilisateur (appelée par saveStartDate)
  UPDATE_USER(state, updatedUser) {
    const index = state.users.findIndex(u => u.id === (updatedUser.Id || updatedUser.id))
    if (index !== -1) {
      state.users.splice(index, 1, {
        ...state.users[index],
        // Normaliser la date en camelCase depuis le DTO PascalCase
        dateDebutParticipation: updatedUser.DateDebutParticipation || updatedUser.dateDebutParticipation || state.users[index].dateDebutParticipation,
        // Préserver les presences existantes
        presences: state.users[index].presences || {}
      })
    }
  },

  // Chargement des présences depuis l'API (appelée par updateUserAttendances)
  UPDATE_USER_ATTENDANCES(state, { attendances, currentPeriodDays }) {
    state.users.forEach(user => {
      const newPresences = { ...user.presences }

      // Nettoyer les jours de la période pour éviter les doublons
      currentPeriodDays.forEach(day => {
        delete newPresences[day]
      })

      // Appliquer les présences de l'API pour cet utilisateur
      attendances
        .filter(a => {
          const uid = a.stagiaireId || a.userId || a.StagiaireId || a.UserId
          return String(uid).toLowerCase() === String(user.id).toLowerCase()
        })
        .forEach(a => {
          const dateKey = new Date(a.date || a.Date).toDateString()
          const statut  = (a.statut || a.Statut || 'p').toLowerCase()
          newPresences[dateKey] = statut
        })

      user.presences = newPresences
    })
  }
}

export const actions = {
  async fetchUsers({ commit, dispatch }) {
    try {
      const res = await UserService.getUsers()
      commit('SET_USERS', res.data)
    } catch (error) {
      const notification = {
        type: 'error',
        message: 'Problème au chargement des utilisateurs : ' + error.message
      }
      dispatch('notification/add', notification, { root: true })
    }
  },

  // Action pour mettre à jour les présences depuis l'API
  updateUserAttendances({ commit }, payload) {
    commit('UPDATE_USER_ATTENDANCES', payload)
  }
}