/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Migration : Vue 3 + Vuex 4 (by Greg)
 */

import { createStore } from 'vuex'

// --- modules ---
import * as settings from '@/store/modules/settings.js'
import * as notification from '@/store/modules/notification.js'
import * as authentification from '@/store/modules/authentification.js'
import * as entreprise from '@/store/modules/entreprise.js'
import * as entrepriseOffre from '@/store/modules/entrepriseOffre.js'
import * as entrepriseMetier from '@/store/modules/entrepriseMetier.js'
import * as entrepriseDomaine from '@/store/modules/entrepriseDomaine.js'
import * as typeEntreprise from '@/store/modules/typeEntreprise.js'
import * as typeDomaine from '@/store/modules/typeDomaine.js'
import * as typeOffre from '@/store/modules/typeOffre.js'
import * as typeMetier from '@/store/modules/typeMetier.js'
import * as typeAnnonce from '@/store/modules/typeAnnonce.js'
import * as typeAffiliation from '@/store/modules/typeAffiliation.js'
import * as typeStage from '@/store/modules/typeStage.js'
import * as typeMoyen from '@/store/modules/typeMoyen.js'
import * as typeIntershipActivity from '@/store/modules/typeIntershipActivity.js'
import * as contact from '@/store/modules/contact.js'
import * as lastContact from '@/store/modules/lastContact.js'
import * as user from '@/store/modules/user.js'
import * as stage from '@/store/modules/stage.js'
import * as stagiaire from '@/store/modules/stagiaire.js'
import * as typeJobSearchAssistance from '@/store/modules/typeJobSearchAssistance.js'
import * as typeJobSearchAssistanceOccurrence from '@/store/modules/typeJobSearchAssistanceOccurrence.js'
import * as jobSearchAssistance from '@/store/modules/jobSearchAssistance.js'
import * as stageFile from '@/store/modules/stageFile.js'
import * as session from '@/store/modules/session.js'

// --- Création du store ---
const store = createStore({
  state: {},
  mutations: {},
  actions: {},
  modules: {
    settings,
    notification,
    authentification,
    entreprise,
    entrepriseOffre,
    entrepriseMetier,
    entrepriseDomaine,
    typeEntreprise,
    typeDomaine,
    typeOffre,
    typeMetier,
    typeAnnonce,
    typeAffiliation,
    typeStage,
    typeMoyen,
    typeIntershipActivity,
    contact,
    lastContact,
    user,
    stage,
    stagiaire,
    typeJobSearchAssistance,
    typeJobSearchAssistanceOccurrence,
    jobSearchAssistance,
    stageFile,
    session,
  },
})

export default store
