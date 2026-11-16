// Régions utilisées pour organiser les marrainages (tableau de marrainage) et correspondance
// commune → région. La liste de référence est la table Regions (api/Regions, éditable depuis le
// tableau de marrainage) ; REGIONS/REGION_COLORS ne servent que de valeurs par défaut tant
// qu'elle n'est pas chargée. La région peut toujours être corrigée à la main sur la fiche d'une
// marraine/d'un parrain ou d'un·e participant·e ; les communes ne servent qu'à la proposer.
// Établie d'après les exemples du tableau des formatrices (ex. Grandvaux/Cully rangés en Riviera,
// Payerne/Lucens/Palézieux en Moudon, Gimel/Mont-la-Ville en Morges, Servion en Autres).

import { ref, computed } from 'vue'
import API_BASE, { apiFetch } from '@/services/api'

export const REGIONS = ['Lausanne', 'Moudon', 'Riviera', 'Morges', 'Yverdon', 'Aigle/Leysin', 'Nyon', 'Vallorbe', 'Autres']

export const REGION_COLORS = {
  Lausanne: '#1565C0',
  Moudon: '#6D4C41',
  Riviera: '#00838F',
  Morges: '#2E7D32',
  Yverdon: '#EF6C00',
  'Aigle/Leysin': '#6A1B9A',
  Nyon: '#AD1457',
  Vallorbe: '#455A64',
  Autres: '#9E9E9E',
}

const COMMUNES = {
  Lausanne: [
    'lausanne', 'renens', 'prilly', 'pully', 'lutry', 'epalinges', 'le mont-sur-lausanne', 'mont-sur-lausanne',
    'crissier', 'bussigny', 'ecublens', 'chavannes-pres-renens', 'chavannes', 'saint-sulpice', 'st-sulpice', 'paudex',
    'belmont-sur-lausanne', 'belmont', 'cheseaux-sur-lausanne', 'cheseaux', 'romanel-sur-lausanne', 'romanel',
    'jouxtens-mezery', 'cugy', 'froideville', 'bretigny-sur-morrens', 'morrens', 'villars-sainte-croix',
    'savigny', 'echallens', 'assens', 'bottens', 'ouchy', 'la sallaz', 'vennes', 'malley',
  ],
  Moudon: [
    'moudon', 'lucens', 'payerne', 'avenches', 'palezieux', 'oron', 'oron-la-ville', 'mezieres', 'ropraz', 'thierrens',
    'granges-pres-marnand', 'valbroye', 'corcelles-pres-payerne', 'chatillens', 'bussy-sur-moudon', 'curtilles',
    'syens', 'hermenches', 'chavannes-sur-moudon', 'servion-moudon',
  ],
  Riviera: [
    'vevey', 'montreux', 'clarens', 'chernex', 'villeneuve', 'la tour-de-peilz', 'tour-de-peilz', 'corsier-sur-vevey',
    'corsier', 'corseaux', 'chardonne', 'blonay', 'saint-legier', 'st-legier', 'blonay - saint-legier', 'jongny',
    'veytaux', 'territet', 'glion', 'brent', 'grandvaux', 'cully', 'bourg-en-lavaux', 'puidoux', 'chexbres',
    'saint-saphorin', 'rivaz', 'epesses', 'villette', 'riex', 'aran',
  ],
  Morges: [
    'morges', 'tolochenaz', 'preverenges', 'saint-prex', 'st-prex', 'echichens', 'lonay', 'denges', 'echandens',
    'bremblens', 'aclens', 'vufflens-le-chateau', 'vufflens-la-ville', 'cossonay', 'penthalaz', 'penthaz',
    'mont-la-ville', 'apples', 'aubonne', 'gimel', 'etoy', 'allaman', 'buchillon', 'bière', 'biere', 'l\'isle',
  ],
  Yverdon: [
    'yverdon', 'yverdon-les-bains', 'grandson', 'sainte-croix', 'ste-croix', 'chavornay', 'orbe', 'yvonand',
    'montagny-pres-yverdon', 'cheseaux-noreaz', 'treycovagnes', 'baulmes', 'champvent', 'concise', 'pomy',
    'essertines-sur-yverdon', 'valeyres-sous-montagny', 'ependes', 'belmont-sur-yverdon',
  ],
  'Aigle/Leysin': [
    'aigle', 'leysin', 'bex', 'ollon', 'villars-sur-ollon', 'villars', 'roche', 'yvorne', 'corbeyrier', 'gryon',
    'lavey', 'lavey-morcles', 'les diablerets', 'ormont-dessus', 'ormont-dessous', 'le sepey', 'chateau-d\'oex',
    'rougemont', 'noville', 'rennaz', 'chessel',
  ],
  Nyon: [
    'nyon', 'gland', 'rolle', 'prangins', 'coppet', 'founex', 'commugny', 'crans', 'crans-pres-celigny', 'eysins',
    'begnins', 'trelex', 'mies', 'chavannes-de-bogis', 'saint-cergue', 'st-cergue', 'perroy', 'mont-sur-rolle',
    'celigny', 'tannay', 'duillier', 'genolier', 'arzier', 'signy',
  ],
  Vallorbe: [
    'vallorbe', 'le sentier', 'le brassus', 'l\'abbaye', 'le pont', 'le chenit', 'ballaigues', 'la sarraz',
    'romainmotier', 'croy', 'les clees', 'lignerolle', 'vaulion',
  ],
}

const normalize = (s) => (s || '')
  .toString()
  .toLowerCase()
  .normalize('NFD')
  .replace(/[̀-ͯ]/g, '')
  .replace(/[-']/g, ' ')
  .replace(/\bste\b\.?/g, 'sainte')
  .replace(/\bst\b\.?/g, 'saint')
  .replace(/\s+/g, ' ')
  .trim()

const LOOKUP = new Map()
for (const [region, communes] of Object.entries(COMMUNES)) {
  for (const c of communes) LOOKUP.set(normalize(c), region)
}

// Région d'une commune (ou d'une adresse contenant la commune, ex. "Rue X 3, 1800 Vevey").
// Renvoie null si rien ne correspond.
export function regionDeCommune(texte) {
  const n = normalize(texte)
  if (!n) return null
  if (LOOKUP.has(n)) return LOOKUP.get(n)
  // Adresse complète : on cherche le nom de commune le plus long contenu dans le texte.
  let best = null
  for (const [commune, region] of LOOKUP) {
    if (commune.length < 4) continue
    const re = new RegExp(`(^|[^a-z])${commune.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}($|[^a-z])`)
    if (re.test(n) && (!best || commune.length > best.commune.length)) best = { commune, region }
  }
  return best?.region || null
}

// Région affichée pour une marraine/un parrain : celle saisie sur la fiche, sinon déduite de la commune.
export const regionMarrainage = (m) => m.region || regionDeCommune(m.commune) || null

// Région affichée pour un·e participant·e : celle saisie sur sa fiche, sinon déduite de son adresse.
export const regionParticipant = (p) => p.region || regionDeCommune(p.adresse) || null

// ─── Liste des régions (table Regions), partagée entre les pages ─────────────────────────────
const regionsList = ref(REGIONS.map((nom, i) => ({ regionId: null, nom, couleur: REGION_COLORS[nom], ordre: i + 1, pistesRecherche: null })))
let loadPromise = null

export async function reloadRegions() {
  try {
    const res = await apiFetch(`${API_BASE}/api/Regions`)
    if (res?.ok) {
      const data = await res.json()
      if (data.length) regionsList.value = data
    }
  } catch (e) { console.error('reloadRegions:', e) }
}

export const regionColor = (nom) =>
  regionsList.value.find(r => r.nom === nom)?.couleur || REGION_COLORS[nom] || '#9E9E9E'

export function useRegions() {
  if (!loadPromise) loadPromise = reloadRegions()
  return {
    regions: regionsList,
    regionNames: computed(() => regionsList.value.map(r => r.nom)),
    reloadRegions,
    regionColor,
    ready: loadPromise,
  }
}
