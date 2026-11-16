// Statuts d'une marraine/d'un parrain dans la liste de marrainage. "Actif·ve" ne se choisit
// normalement pas à la main : il découle des relations de marrainage (au moins une relation
// active avec un·e participant·e). Il reste proposé pour les binômes anciens dont le/la
// filleul·e n'est pas lié·e à une fiche participant·e.
export const MARRAINAGE_STATUTS = [
  { value: 'nouveau', title: 'Nouveau·elle', color: 'blue' },
  { value: 'en_attente', title: 'En attente de binôme', color: 'orange' },
  { value: 'disponible', title: 'Disponible', color: 'light-blue-darken-2' },
  { value: 'a_relancer', title: 'À relancer', color: 'purple' },
  { value: 'a_verifier', title: 'À vérifier', color: 'amber-darken-3' },
  { value: 'actif', title: 'Actif·ve', color: 'success' },
  { value: 'sans_suite', title: 'Sans suite', color: 'grey' },
]

// Statuts retirés de la liste mais encore présents sur d'anciennes fiches : affichés tels quels,
// plus proposés à la saisie.
const STATUTS_HISTORIQUES = [
  { value: 'a_former', title: 'À former', color: 'teal' },
]

// Statuts proposés sur la fiche d'une marraine/d'un parrain : "Actif·ve" n'y figure pas, il
// découle des relations de marrainage affichées plus bas.
export const MARRAINAGE_STATUTS_FICHE = MARRAINAGE_STATUTS.filter(s => s.value !== 'actif')

// Statuts qui peuvent s'afficher à côté de "Actif·ve" quand une relation est active
// (disponibilité de la personne ou alerte à traiter). Les autres (nouveau·elle, en attente,
// sans suite) n'ont pas de sens avec une relation active et ne sont alors pas affichés.
export const STATUTS_COMPATIBLES_ACTIF = ['disponible', 'a_relancer', 'a_verifier']

const BY_VALUE = Object.fromEntries([...MARRAINAGE_STATUTS, ...STATUTS_HISTORIQUES].map(s => [s.value, s]))

export const statutInfo = (value) => BY_VALUE[value] || (value ? { value, title: value, color: 'grey' } : null)

// Relations actives, y compris le lien historique Marrainage.Participant qui n'a pas forcément
// de ligne MarrainageParticipant (considéré actif, comme sur la fiche marrainage).
export function hasActiveRelation(marrainage) {
  const links = marrainage.participantLinks || []
  if (links.some(pl => pl.actif)) return true
  const legacy = marrainage.participant
  return !!legacy && !links.some(pl => pl.participantId === legacy.id)
}

export const effectiveStatut = (marrainage) =>
  hasActiveRelation(marrainage) ? 'actif' : (marrainage.statut || null)

// Statuts à afficher dans la liste : "Actif·ve" si une relation est active, accompagné du
// statut de la fiche quand il reste pertinent (disponible, à relancer, à vérifier) — ainsi une
// alerte "à vérifier" n'est plus masquée par "Actif·ve".
export function statutsAffiches(marrainage) {
  const actif = hasActiveRelation(marrainage)
  const fiche = marrainage.statut || null
  if (!actif) return fiche ? [fiche] : []
  return fiche && STATUTS_COMPATIBLES_ACTIF.includes(fiche) ? ['actif', fiche] : ['actif']
}

// Date de début de la relation de marrainage la plus récente ; à défaut, l'ancienne date
// globale (Marrainage.Date) reprise des imports Excel.
export function dateDebutMarrainage(marrainage) {
  const dates = (marrainage.participantLinks || []).map(pl => pl.dateDebut).filter(Boolean).sort()
  return dates.length ? dates[dates.length - 1] : (marrainage.date || null)
}

// Fin du marrainage : seulement si plus aucune relation n'est active — date de fin la plus récente.
export function dateFinMarrainage(marrainage) {
  const links = marrainage.participantLinks || []
  if (!links.length || links.some(pl => pl.actif)) return null
  const dates = links.map(pl => pl.dateFin).filter(Boolean).sort()
  return dates.length ? dates[dates.length - 1] : null
}

// "Prendre des nouvelles" : trois mois sans commentaire ni modification de la fiche (relations,
// réponses, documents compris). Ne concerne pas les personnes "sans suite".
export const DELAI_NOUVELLES_MOIS = 3
export function derniereActivite(marrainage) {
  const dates = [marrainage.updatedAt, marrainage.dernierCommentaire, marrainage.createdAt].filter(Boolean).map(d => new Date(d))
  return dates.length ? new Date(Math.max(...dates)) : null
}
export function prendreDesNouvelles(marrainage) {
  if (marrainage.statut === 'sans_suite') return false
  const last = derniereActivite(marrainage)
  if (!last) return false
  const limite = new Date()
  limite.setMonth(limite.getMonth() - DELAI_NOUVELLES_MOIS)
  return last < limite
}
