// Listes de choix de la rubrique "Inscriptions" — reprises des tableaux Excel. Utilisées dans
// des v-combobox : une valeur hors liste reste possible.

export const STATUTS_BENEFICIAIRE = [
  'Nouveau-elle',
  'Renouvelé-e 1',
  'Renouvelé-e 2',
  'Renouvelé-e 3',
  'Renouvelé-e 4',
  'De A1 - renouvelé-e 1',
  'Interrompu',
]

export const OBJECTIFS_ATTEINTS = ['oui', 'partiellement', 'non']

export const ORIENTATIONS = [
  'Renouv. de la MIS',
  'MIS RLS',
  'Passage en A2',
  'Autre (précisez)',
  'Sans solution',
]

export const NIVEAUX = ['A1.1', 'A1.2', 'A2.1', 'A2.2', 'B1.1', 'B1.2']

export const MANDANTS = ['CSIR', 'CSR']

export const STATUTS_CANDIDATURE = [
  'À évaluer',
  'AS à contacter',
  'Attente du retour AS',
  'En attente A1 F&I',
  'En attente A2 F&I',
  'En attente A1 Télécours',
  'En attente A2 Télécours',
  'Inscrit-e',
  'Retiré-e',
]

// Mesure visée par une candidature ; pour F&I, le niveau (A1/A2) est porté par le statut.
export const MESURES = ['F&I', 'Télécours']

// Abréviation affichée dans la pastille de la liste des candidat·e·s.
export const abreviationMesure = (m) =>
  m === 'Télécours' ? 'FenT' : (m === 'F&I' || m === 'A1' || m === 'A2') ? 'F&I' : '?'

// Couleurs propres à chaque mesure et à chaque mandant, pour les distinguer d'un coup d'œil.
export const couleurMesure = (m) =>
  m === 'Télécours' ? '#5E35B1' : (m === 'F&I' || m === 'A1' || m === 'A2') ? '#00897B' : '#90A4AE'
export const couleurMandant = (m) => ({ CSR: '#1565C0', CSIR: '#C2185B' }[m] || '#78909C')

// Groupe (télécours / F&I A1 / F&I A2) d'une candidature — même règle que l'API
// (InscriptionsController.GroupeCandidature).
export function groupeCandidature(mesure, statut) {
  if (mesure === 'Télécours') return 'telecours'
  if (mesure === 'F&I' || mesure === 'A1' || mesure === 'A2') {
    return mesure === 'A2' || /A2/.test(statut || '') ? 'fi_a2' : 'fi_a1'
  }
  return null
}

// Mesure impliquée par un statut "En attente A1/A2 F&I/Télécours" (null pour les autres).
export const mesureDuStatut = (statut) =>
  /^En attente A[12] F&I$/.test(statut || '') ? 'F&I'
    : /^En attente A[12] Télécours$/.test(statut || '') ? 'Télécours'
    : null

export const CIVILITES = ['Madame', 'Monsieur']

// Couleur de chaque statut de candidature (liseré, pastille, récapitulatif). `ink` = couleur du
// texte de la pastille quand la couleur de fond est trop claire pour être lue (jaune, vert clair).
export const STATUT_CANDIDATURE_COULEURS = {
  'À évaluer': { color: '#EF6C00' },
  'AS à contacter': { color: '#D32F2F' },
  'Attente du retour AS': { color: '#FBC02D', ink: '#8D6E00' },
  // Listes d'attente : vert clair / foncé pour F&I, turquoise clair / foncé pour le télécours.
  'En attente A1 F&I': { color: '#81C784', ink: '#2E7D32' },
  'En attente A2 F&I': { color: '#1B5E20' },
  'En attente A1 Télécours': { color: '#80CBC4', ink: '#00695C' },
  'En attente A2 Télécours': { color: '#00695C' },
  'Inscrit-e': { color: '#1976D2' },
  'Retiré-e': { color: '#BDBDBD', ink: '#757575' },
}
const couleurStatut = (s) => STATUT_CANDIDATURE_COULEURS[s] || { color: '#90A4AE' }
export const statutCandidatureColor = (s) => couleurStatut(s).color
export const statutCandidatureInk = (s) => couleurStatut(s).ink || couleurStatut(s).color

export const asLabel = (a) => {
  if (!a) return ''
  const name = [a.prenom, a.nom].filter(Boolean).join(' ')
  return name ? `${name} (${a.email})` : a.email
}

// Message d'erreur lisible d'une réponse API (BadRequest("...") arrive encodé en JSON).
export async function readApiError(res, fallback = "Erreur lors de l'enregistrement.") {
  const text = await res.text()
  try {
    const json = JSON.parse(text)
    return typeof json === 'string' ? json : (json.title || fallback)
  } catch {
    return text || fallback
  }
}

// ─── Groupes et sessions (même logique que Bilans & attestations / Accomp. & Ateliers) ───

// statutAttente : statut proposé pour un·e candidat·e créé·e depuis la vue d'une session.
export const GROUPES = [
  { label: 'Télécours', value: 'telecours', mesure: 'Télécours', statutAttente: null },
  { label: 'F&I A1', value: 'fi_a1', mesure: 'F&I', statutAttente: 'En attente A1 F&I' },
  { label: 'F&I A2', value: 'fi_a2', mesure: 'F&I', statutAttente: 'En attente A2 F&I' },
]

const MOIS = ['janv.', 'févr.', 'mars', 'avr.', 'mai', 'juin', 'juil.', 'août', 'sept.', 'oct.', 'nov.', 'déc.']
const fmtJourMois = (d) => `${d.getDate()} ${MOIS[d.getMonth()]}`

// Sessions d'un groupe d'après le calendrier de formation (SessionFormation), triées par date.
// Le numéro est le rang de la session dans son année (S1, S2...), ce qui correspond au
// SessionNumberId des inscriptions. Une session "à planifier" est ajoutée après la dernière
// du calendrier, pour pouvoir préparer la suivante avant qu'elle y soit saisie.
//
// minAVenir : nombre minimal de sessions proposées après la session en cours — complété par des
// sessions "à planifier" (listes d'attente qui portent parfois loin dans le temps).
export function sessionsDuGroupe(calendarSessions, groupe, { minAVenir = 1 } = {}) {
  const rangParAnnee = {}
  const list = (calendarSessions || [])
    .filter(s => (s.type || '').toLowerCase() === groupe && s.startDate)
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
    .map(s => {
      const start = new Date(s.startDate)
      const end = s.endDate ? new Date(s.endDate) : null
      const year = start.getFullYear()
      const number = (rangParAnnee[year] = (rangParAnnee[year] || 0) + 1)
      return {
        year, number, key: `${year}-${number}`, start, end, planned: false,
        label: `Session ${number} — ${year}`,
        dates: end ? `${fmtJourMois(start)} – ${fmtJourMois(end)} ${end.getFullYear()}` : `À partir du ${fmtJourMois(start)} ${year}`,
      }
    })

  const ajouterSuivante = () => {
    const last = list[list.length - 1]
    const next = !last ? { year: new Date().getFullYear(), number: 1 }
      : last.number >= 4 ? { year: last.year + 1, number: 1 }
      : { year: last.year, number: last.number + 1 }
    list.push({
      ...next, key: `${next.year}-${next.number}`, start: null, end: null, planned: true,
      label: `Session ${next.number} — ${next.year}`,
      dates: 'Pas encore dans le calendrier de formation',
    })
  }
  ajouterSuivante()
  const courante = indexSessionCourante(list)
  while (list.length - 1 - courante < minAVenir) ajouterSuivante()
  return list
}

// Session "en cours" d'un groupe : celle qui contient aujourd'hui, sinon la prochaine à venir,
// sinon la dernière passée.
export function indexSessionCourante(sessions) {
  const now = new Date()
  let idx = sessions.findIndex(s => s.start && s.end && s.start <= now && now <= s.end)
  if (idx < 0) idx = sessions.findIndex(s => s.start && s.start > now)
  if (idx < 0) idx = sessions.findLastIndex(s => !s.planned)
  return Math.max(0, idx)
}

export const isRenouvellement = (orientation) =>
  !!orientation && (orientation.toLowerCase().startsWith('renouv') || orientation === 'Passage en A2')

// ─── Couleurs de statut (liseré des lignes + barre récapitulative, cf. scss/inscriptions.css) ───

export const RAIL = {
  nouveau: 'var(--st-nouveau)',
  renouvele: 'var(--st-renouvele)',
  prevu: 'var(--st-prevu)',
  candidat: 'var(--st-candidat)',
  interrompu: 'var(--st-interrompu)',
  autre: 'var(--st-autre)',
}

// Statut par défaut quand rien n'est saisi : d'après le nombre de sessions déjà suivies.
export const statutDeduit = (p) => p.sessionsPrecedentes > 0 ? `Renouvelé-e ${p.sessionsPrecedentes}` : 'Nouveau-elle'

// Catégorie d'un·e inscrit·e d'après son statut de la session (saisi ou déduit).
export function categorieInscrit(p) {
  if (p.dateFinParticipation || p.statutBeneficiaire === 'Interrompu') return 'interrompu'
  const s = (p.statutBeneficiaire || statutDeduit(p)).toLowerCase()
  if (s.startsWith('nouveau')) return 'nouveau'
  if (s.includes('renouvel')) return 'renouvele'
  return 'autre'
}

export const railCandidature = (statut) => statutCandidatureColor(statut)

export const initiales = (prenom, nom) =>
  `${(prenom || '').trim()[0] || ''}${(nom || '').trim()[0] || ''}`.toUpperCase() || '?'
