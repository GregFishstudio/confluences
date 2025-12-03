/**
 * Projet: Gestion des stagiaires
 * Auteur : Tim Allemann
 * Date : 16.09.2020 (Mis à jour pour Vue 3)
 * Description : Fichier de configuration pour la navigation entre les vues
 * Fichier : index.js (Corrigé pour la compatibilité Vite)
 **/

import { createRouter, createWebHistory } from 'vue-router'
// ... (imports des vues)
import EntreprisesList from '../views/EntreprisesList.vue'
import EntrepriseEdit from '../views/EntrepriseEdit.vue'
import StagesList from '../views/StagesList.vue'
import StageEdit from '../views/StageEdit.vue'
import StagiairesList from '../views/StagiairesList.vue'
import StagiaireEdit from '../views/StagiaireEdit.vue'
import ContactsList from '../views/ContactsList.vue'
import ContactEdit from '../views/ContactEdit.vue'
import MetiersList from '../views/MetiersList.vue'
import MetierEdit from '../views/MetierEdit.vue'
import AffiliationsList from '../views/AffiliationsList.vue'
import AffiliationEdit from '../views/AffiliationEdit.vue'
import AnnoncesList from '../views/AnnoncesList.vue'
import AnnonceEdit from '../views/AnnonceEdit.vue'
import DomainesList from '../views/DomainesList.vue'
import DomaineEdit from '../views/DomaineEdit.vue'
import TypeEntreprisesList from '../views/TypeEntreprisesList.vue'
import TypeEntrepriseEdit from '../views/TypeEntrepriseEdit.vue'
import TypeStagesList from '../views/TypeStagesList.vue'
import TypeStageEdit from '../views/TypeStageEdit.vue'
import MoyensList from '../views/MoyensList.vue'
import MoyenEdit from '../views/MoyenEdit.vue'
import TypeOffresList from '../views/TypeOffresList.vue'
import TypeOffreEdit from '../views/TypeOffreEdit.vue'
import JobSearchAssistanceList from '../views/JobSearchAssistanceList.vue'
import JobSearchAssistanceEdit from '../views/JobSearchAssistanceEdit.vue'
import TypeJobSearchAssistanceList from '../views/TypeJobSearchAssistanceList.vue'
import TypeJobSearchAssistanceEdit from '../views/TypeJobSearchAssistanceEdit.vue'
import TypeJobSearchAssistanceOccurenceList from '../views/TypeJobSearchAssistanceOccurenceList.vue'
import TypeJobSearchAssistanceOccurenceEdit from '../views/TypeJobSearchAssistanceOccurenceEdit.vue'
import TypeIntershipActivityList from '../views/TypeIntershipActivityList.vue'
import TypeIntershipActivityEdit from '../views/TypeIntershipActivityEdit.vue'
import AttendanceRegister from '../views/AttendanceRegister.vue';
import StagiairesFollowUp from '../views/FollowUpPage.vue';
import Login from '../views/Login.vue'
import Callback from '../views/Callback.vue'
import NotFound from '../views/NotFound.vue'
import Unauthorized from '../views/Unauthorized.vue'
import Home from '../views/Home.vue'
import NProgress from 'nprogress'
import store from '../store'
// --- Définition des Routes ---

const routes = [
  {
    path: '/',
    redirect: '/login',
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
  },
   {
    path: '/home',
    name: 'Home',
    component: Home,
  },
  {
    path: '/attendance',
    name: 'AttendanceRegister',
    component: AttendanceRegister,
    meta: { requiresAuth: true } 
  },

    {
    path: '/stagiaires/follow-up',
    name: 'StagiairesFollowUp', 
    component: StagiairesFollowUp,
  },
  // Entreprises
  { path: '/entreprises', name: 'Entreprises', component: EntreprisesList, meta: { requiresAuth: true } },
  {
    path: '/entreprise/modifier/:id',
    name: 'Entreprise-Modifier',
    component: EntrepriseEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('entreprise/fetchEntreprise', routeTo.params.id)
        .then(entreprise => {
          if (entreprise == undefined) {
            next({ name: '404', params: { resource: 'entreprise' } })
          } else {
            routeTo.params.entreprise = entreprise
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'entreprise' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Stages
  { path: '/stages', name: 'Stages', component: StagesList, meta: { requiresAuth: true } },
  {
    path: '/stage/modifier/:id',
    name: 'Stage-Modifier',
    component: StageEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('stage/fetchStage', routeTo.params.id)
        .then(stage => {
          if (stage == undefined) {
            next({ name: '404', params: { resource: 'stage' } })
          } else {
            routeTo.params.stage = stage
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'stage' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Stagiaires
  { path: '/stagiaires', name: 'Stagiaires', component: StagiairesList, meta: { requiresAuth: true } },
  {
    path: '/stagiaire/modifier/:id',
    name: 'Stagiaire-Modifier',
    component: StagiaireEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('stagiaire/fetchStagiaire', routeTo.params.id)
        .then(stagiaire => {
          if (stagiaire == undefined) {
            next({ name: '404', params: { resource: 'stagiaire' } })
          } else {
            routeTo.params.stagiaire = stagiaire
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'stagiaire' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Contacts
  { path: '/contacts', name: 'Contacts', component: ContactsList, meta: { requiresAuth: true } },
  {
    path: '/contact/modifier/:id',
    name: 'Contact-Modifier',
    component: ContactEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('contact/fetchContact', routeTo.params.id)
        .then(contact => {
          if (contact == undefined) {
            next({ name: '404', params: { resource: 'contact' } })
          } else {
            routeTo.params.contact = contact
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'contact' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Métiers
  { path: '/metiers', name: 'Metiers', component: MetiersList, meta: { requiresAuth: true } },
  {
    path: '/metier/modifier/:id',
    name: 'TypeMetier-Modifier',
    component: MetierEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeMetier/fetchTypeMetier', routeTo.params.id)
        .then(typeMetier => {
          if (typeMetier == undefined) {
            next({ name: '404', params: { resource: 'métier' } })
          } else {
            routeTo.params.typeMetier = typeMetier
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'metier' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Affiliations
  { path: '/affiliations', name: 'Affiliations', component: AffiliationsList, meta: { requiresAuth: true } },
  {
    path: '/affiliation/modifier/:id',
    name: 'TypeAffiliation-Modifier',
    component: AffiliationEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeAffiliation/fetchTypeAffiliation', routeTo.params.id)
        .then(typeAffiliation => {
          if (typeAffiliation == undefined) {
            next({ name: '404', params: { resource: 'affiliation' } })
          } else {
            routeTo.params.typeAffiliation = typeAffiliation
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'affiliation' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Annonces
  { path: '/annonces', name: 'Annonces', component: AnnoncesList, meta: { requiresAuth: true } },
  {
    path: '/annonce/modifier/:id',
    name: 'TypeAnnonce-Modifier',
    component: AnnonceEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeAnnonce/fetchTypeAnnonce', routeTo.params.id)
        .then(typeAnnonce => {
          if (typeAnnonce == undefined) {
            next({ name: '404', params: { resource: 'annonce' } })
          } else {
            routeTo.params.typeAnnonce = typeAnnonce
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'annonce' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Domaines
  { path: '/domaines', name: 'Domaines', component: DomainesList, meta: { requiresAuth: true } },
  {
    path: '/domaine/modifier/:id',
    name: 'TypeDomaine-Modifier',
    component: DomaineEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeDomaine/fetchTypeDomaine', routeTo.params.id)
        .then(typeDomaine => {
          if (typeDomaine == undefined) {
            next({ name: '404', params: { resource: 'domaine' } })
          } else {
            routeTo.params.typeDomaine = typeDomaine
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'domaine' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Types d'Entreprises
  { path: '/typeentreprises', name: 'Type-Entreprises', component: TypeEntreprisesList, meta: { requiresAuth: true } },
  {
    path: '/typeentreprise/modifier/:id',
    name: 'TypeEntreprise-Modifier',
    component: TypeEntrepriseEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeEntreprise/fetchTypeEntreprise', routeTo.params.id)
        .then(typeEntreprise => {
          if (typeEntreprise == undefined) {
            next({ name: '404', params: { resource: 'catégorie' } })
          } else {
            routeTo.params.typeEntreprise = typeEntreprise
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'catégorie' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Types de Stages
  { path: '/typestages', name: 'Type-Stages', component: TypeStagesList, meta: { requiresAuth: true } },
  {
    path: '/typestage/modifier/:id',
    name: 'TypeStage-Modifier',
    component: TypeStageEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeStage/fetchTypeStage', routeTo.params.id)
        .then(typeStage => {
          if (typeStage == undefined) {
            next({ name: '404', params: { resource: "taux d'occupation" } })
          } else {
            routeTo.params.typeStage = typeStage
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: "taux d'occupation" } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Moyens
  { path: '/moyens', name: 'Moyens', component: MoyensList, meta: { requiresAuth: true } },
  {
    path: '/moyen/modifier/:id',
    name: 'TypeMoyen-Modifier',
    component: MoyenEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeMoyen/fetchTypeMoyen', routeTo.params.id)
        .then(typeMoyen => {
          if (typeMoyen == undefined) {
            next({ name: '404', params: { resource: 'moyen' } })
          } else {
            routeTo.params.typeMoyen = typeMoyen
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'moyen' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Types d'Offres
  { path: '/offres', name: 'Offres', component: TypeOffresList, meta: { requiresAuth: true } },
  {
    path: '/offre/modifier/:id',
    name: 'TypeOffre-Modifier',
    component: TypeOffreEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeOffre/fetchTypeOffre', routeTo.params.id)
        .then(typeOffre => {
          if (typeOffre == undefined) {
            next({ name: '404', params: { resource: 'offre' } })
          } else {
            routeTo.params.typeOffre = typeOffre
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'offre' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Types d'Assistance à la Recherche d'Emploi
  { path: '/typeJobSearchAssistances', name: 'TypeJobSearchAssistances', component: TypeJobSearchAssistanceList, meta: { requiresAuth: true } },
  {
    path: '/typeJobSearchAssistance/modifier/:id',
    name: 'TypeJobSearchAssistance-Modifier',
    component: TypeJobSearchAssistanceEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeJobSearchAssistance/fetchTypeJobSearchAssistance', routeTo.params.id)
        .then(typeJobSearchAssistance => {
          if (typeJobSearchAssistance == undefined) {
            next({ name: '404', params: { resource: 'typeJobSearchAssistance' } })
          } else {
            routeTo.params.typeJobSearchAssistance = typeJobSearchAssistance
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'typeJobSearchAssistance' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Types d'Occurrences d'Assistance à la Recherche d'Emploi
  { path: '/typeJobSearchAssistanceOccurrences', name: 'TypeJobSearchAssistanceOccurences', component: TypeJobSearchAssistanceOccurenceList, meta: { requiresAuth: true } },
  {
    path: '/typeJobSearchAssistanceOccurrence/modifier/:id',
    name: 'TypeJobSearchAssistanceOccurrence-Modifier',
    component: TypeJobSearchAssistanceOccurenceEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeJobSearchAssistanceOccurrence/fetchtypeSearchAssistanceOccurrence', routeTo.params.id)
        .then(typeJobSearchAssistanceOccurence => {
          if (typeJobSearchAssistanceOccurence == undefined) {
            next({ name: '404', params: { resource: 'typeJobSearchAssistanceOccurrence' } })
          } else {
            routeTo.params.typeJobSearchAssistanceOccurence = typeJobSearchAssistanceOccurence
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'typeJobSearchAssistanceOccurrence' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Assistance à la Recherche d'Emploi (ARE)
  { path: '/jobSearchAssistances', name: 'JobSearchAssistances', component: JobSearchAssistanceList, meta: { requiresAuth: true } },
  {
    path: '/are/modifier/:id',
    name: 'JobSearchAssistance-Modifier',
    component: JobSearchAssistanceEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('jobSearchAssistance/fetchJobSearchAssistance', routeTo.params.id)
        .then(jobSearchAssistance => {
          if (jobSearchAssistance == undefined) {
            next({ name: '404', params: { resource: 'ARE' } })
          } else {
            routeTo.params.jobSearchAssistance = jobSearchAssistance
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'ARE' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Types d'Activité de Stage (Intership Activity)
  { path: '/activites', name: 'Activites', component: TypeIntershipActivityList, meta: { requiresAuth: true } },
  {
    path: '/activite/modifier/:id',
    name: 'TypeIntershipActivity-Modifier',
    component: TypeIntershipActivityEdit,
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: (routeTo, routeFrom, next) => {
      store.dispatch('typeIntershipActivity/fetchTypeIntershipActivity', routeTo.params.id)
        .then(typeIntershipActivity => {
          if (typeIntershipActivity == undefined) {
            next({ name: '404', params: { resource: 'activité' } })
          } else {
            routeTo.params.typeIntershipActivity = typeIntershipActivity
            next()
          }
        })
        .catch(error => {
          if (error.response && error.response.status == 404) {
            next({ name: '404', params: { resource: 'activité' } })
          } else {
            next(routeFrom)
          }
        })
    }
  },

  // Routes de Service / Erreur
  { path: '/callback', name: 'Callback', component: Callback },
  { path: '/404', name: '404', component: NotFound, props: true },
  { path: '/unauthorized', name: 'Unauthorized', component: Unauthorized, props: true },
  
  // Catch-all (redirige toutes les autres routes vers 404)
  {
    path: '/:catchAll(.*)',
    redirect: { name: '404', params: { resource: 'page' } }
  }
]

// --- Création et Configuration du Router (Vue 3) ---

const router = createRouter({
  // CORRECTION: Remplacer process.env.BASE_URL par import.meta.env.BASE_URL
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

// --- Fonctions d'Authentification / Rôles ---

function isTeacher() {
  const user = JSON.parse(localStorage.getItem('user') || 'null')
  if (!user || !user.profile) return false

  const roles = user.profile.role
  if (!roles) return false

  if (roles === 'Teacher') return true
  if (Array.isArray(roles)) return roles.includes('Teacher')

  return false
}


// --- Global Before Guard ---

router.beforeEach(async (to, from, next) => {
  NProgress.start()

  const user = JSON.parse(localStorage.getItem('user') || 'null')
  const token = user?.access_token

  const publicPaths = ['/login', '/callback', '/unauthorized']
  if (publicPaths.includes(to.path)) {
    NProgress.done()
    return next()
  }

  // 🔒 Auth requise
  if (to.matched.some(r => r.meta.requiresAuth)) {
    if (!token) {
      console.warn('🚫 Aucun token détecté, redirection vers /login')
      NProgress.done()
      return next({ name: 'Login', query: { returnUrl: to.fullPath } })
    }

    // 🧩 Vérifie le rôle seulement si besoin
    if (!isTeacher()) {
      console.warn('⚠️ Accès refusé : non-teacher')
      NProgress.done()
      return next({ name: 'Unauthorized' })
    }
  }

  next()
  NProgress.done()
})



router.afterEach(() => NProgress.done())

export default router