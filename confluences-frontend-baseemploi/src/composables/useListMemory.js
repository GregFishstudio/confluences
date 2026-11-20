// Mémoire d'une liste (recherche, filtres, tri, page, position dans la page) pour que le bouton
// Retour d'une fiche ramène à la liste telle qu'on l'avait laissée, et non à la liste générale.
//
// La mémoire est liée à l'entrée de l'historique du navigateur : elle est reprise quand on revient
// sur cette même entrée (bouton Retour de la fiche, flèche « précédent » du navigateur, rechargement
// de la page), mais une liste ouverte depuis le menu repart de zéro.
//
//   const page = ref(1)
//   useListMemory('participants', { search, filterActif, itemsPerPage }, { page, ready: () => !loading.value })
//
// - `champs` : refs restaurées dès l'ouverture de la liste.
// - `page` : ref de la page du tableau, restaurée une fois les données chargées (`ready`) — avant,
//   le tableau vide la ramènerait à 1 ; la position de défilement est restaurée au même moment.
import { watch, nextTick, isRef } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'

const PREFIXE = 'liste:'
const positionHistorique = () => window.history.state?.position ?? null

function lire(cle) {
  try {
    return JSON.parse(sessionStorage.getItem(PREFIXE + cle) || 'null')
  } catch {
    return null
  }
}

function ecrire(cle, valeur) {
  try {
    sessionStorage.setItem(PREFIXE + cle, JSON.stringify(valeur))
  } catch {
    // Stockage indisponible (navigation privée…) : la liste repartira simplement de zéro.
  }
}

export function useListMemory(cle, champs = {}, { page = null, ready = () => true } = {}) {
  const memoire = lire(cle)
  const retourSurCetteListe = memoire && memoire.position != null && memoire.position === positionHistorique()

  if (retourSurCetteListe) {
    for (const [nom, r] of Object.entries(champs)) {
      if (isRef(r) && nom in (memoire.champs || {})) r.value = memoire.champs[nom]
    }

    // Page et défilement : une fois les lignes affichées.
    let fait = false
    const restaurer = async () => {
      if (fait) return
      fait = true
      if (page && memoire.page) page.value = memoire.page
      await nextTick()
      // Deux images : le tableau a eu le temps de se dessiner à la bonne page.
      requestAnimationFrame(() => requestAnimationFrame(() => window.scrollTo(0, memoire.scrollY || 0)))
    }
    const stop = watch(ready, (ok) => {
      if (!ok) return
      restaurer()
      nextTick(() => stop())
    }, { immediate: true })
    // Filet de sécurité si les données n'arrivent jamais (erreur de chargement).
    setTimeout(() => { restaurer(); stop() }, 4000)
  }

  // Au départ vers une fiche (ou ailleurs) : on mémorise l'état pour cette entrée de l'historique.
  onBeforeRouteLeave(() => {
    const valeurs = {}
    for (const [nom, r] of Object.entries(champs)) {
      if (isRef(r)) valeurs[nom] = r.value
    }
    ecrire(cle, {
      position: positionHistorique(),
      champs: valeurs,
      page: page ? page.value : null,
      scrollY: window.scrollY,
    })
  })
}

// Bouton Retour d'une fiche (et retour après enregistrement) : revient à la page précédente — la
// liste avec ses filtres, ou la fiche d'où l'on venait ; à défaut (lien ouvert directement), la liste.
export function retourOuListe(router, nomListe) {
  if (window.history.state?.back) router.back()
  else router.push({ name: nomListe })
}
