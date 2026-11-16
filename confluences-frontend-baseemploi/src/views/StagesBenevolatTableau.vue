<!--
  Tableau stages & bénévolat : participant·e·s qui cherchent un stage ou du bénévolat (rubriques
  Stage / Bénévolat « Oui » ou « À vérifier » de leur fiche), par session — remplace le tableau Word
  « Stages et bénévolat session N ». Pour chaque personne, les entreprises de sa région qui ont un
  métier ou un domaine en commun avec ses intérêts sont proposées comme pistes.
-->
<template>
  <v-container fluid class="pa-4 pa-md-6">

    <!-- ════════ En-tête ════════ -->
    <div class="d-flex align-center flex-wrap ga-2 mb-4">
      <div>
        <div class="text-h5 font-weight-bold text-primary">Stages &amp; bénévolat</div>
        <div class="text-caption text-grey">
          Participant·e·s qui cherchent un stage ou du bénévolat — {{ sessionLabel(fSession) }}
        </div>
      </div>
      <v-spacer />
      <v-btn variant="text" prepend-icon="mdi-refresh" :loading="loading" class="no-print" @click="load">Actualiser</v-btn>
      <v-btn variant="tonal" prepend-icon="mdi-printer-outline" class="no-print" @click="print">Imprimer</v-btn>
    </div>

    <!-- ════════ Chiffres clés ════════ -->
    <div class="kpis mb-4 no-print">
      <div v-for="k in kpis" :key="k.key" class="kpi" :class="{ 'kpi--active': k.active }" @click="k.toggle && k.toggle()">
        <v-icon :color="k.color" size="26" class="mr-3">{{ k.icon }}</v-icon>
        <div>
          <div class="kpi__value">{{ k.value }}</div>
          <div class="kpi__label">{{ k.label }}</div>
        </div>
      </div>
    </div>

    <!-- ════════ Filtres ════════ -->
    <v-card class="mb-4 elevation-1 no-print" rounded="lg">
      <v-card-text class="py-3">
        <div class="d-flex flex-wrap align-center ga-3">
          <v-text-field
            v-model="search" placeholder="Nom, commune, métier…" prepend-inner-icon="mdi-magnify"
            variant="outlined" density="compact" hide-details clearable style="max-width: 240px; min-width: 190px"
          />
          <v-select
            v-model="fSession" :items="sessionOptions" label="Session" variant="outlined" density="compact" hide-details
            style="min-width: 190px; max-width: 220px"
          />
          <div class="filter-group">
            <span class="filter-label">Groupe</span>
            <v-btn-toggle v-model="fGroupe" multiple density="compact" variant="outlined" divided color="primary">
              <v-btn v-for="g in GROUPES" :key="g.value" :value="g.value" size="small">{{ g.label }}</v-btn>
            </v-btn-toggle>
          </div>
          <div class="filter-group">
            <span class="filter-label">Cherche</span>
            <v-btn-toggle v-model="fType" multiple density="compact" variant="outlined" divided color="primary">
              <v-btn value="stage" size="small">Stage</v-btn>
              <v-btn value="benevolat" size="small">Bénévolat</v-btn>
            </v-btn-toggle>
          </div>
          <v-select
            v-model="fRegions" :items="regionOptions" label="Région" multiple chips closable-chips
            variant="outlined" density="compact" hide-details clearable style="min-width: 160px; max-width: 300px"
          />
          <v-select
            v-model="fDomainesBenevolat" :items="BENEVOLAT_DOMAINES" label="Domaine bénévolat" multiple chips closable-chips
            variant="outlined" density="compact" hide-details clearable style="min-width: 200px; max-width: 340px"
          />
          <div class="filter-group">
            <span class="filter-label">Intérêt</span>
            <v-btn-toggle v-model="fDegre" multiple density="compact" variant="outlined" divided color="primary">
              <v-btn v-for="d in DEGRES" :key="d.value" :value="d.value" size="small">
                <v-icon start size="small" :color="d.color">{{ d.icon }}</v-icon>{{ d.short }}
              </v-btn>
            </v-btn-toggle>
          </div>
          <v-spacer />
          <v-btn v-if="hasFilters" variant="text" size="small" prepend-icon="mdi-filter-remove-outline" @click="resetFilters">
            Effacer les filtres
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- ════════ Tableau ════════ -->
    <v-card class="elevation-1" rounded="lg">
      <v-data-table
        v-model:expanded="expanded"
        :headers="headers" :items="rows" item-value="id" show-expand :items-per-page="-1"
        :loading="loading" density="comfortable" hover class="sb-table"
        :sort-by="[{ key: 'groupeOrdre', order: 'asc' }, { key: 'nom', order: 'asc' }]"
        no-data-text="Aucun·e participant·e ne cherche de stage ou de bénévolat avec ces filtres."
      >
        <template #item.nom="{ item }">
          <a href="#" class="link font-weight-bold" @click.prevent="goParticipant(item.id)">{{ item.firstname }} {{ item.lastName }}</a>
          <div class="text-caption text-grey">
            {{ item.commune || 'Localité ?' }} · {{ identite(item) }}
          </div>
        </template>

        <template #item.groupeOrdre="{ item }">
          <v-chip v-if="item.groupe" size="x-small" label variant="tonal" :color="item.groupe.color">{{ item.groupe.label }}</v-chip>
          <span v-else class="text-grey">—</span>
        </template>

        <template #item.cherche="{ item }">
          <div class="d-flex flex-wrap ga-1">
            <v-chip v-if="item.besoinStage" size="x-small" label :variant="item.besoinStage === 'oui' ? 'flat' : 'outlined'"
              :color="item.besoinStage === 'oui' ? 'green-darken-1' : 'orange-darken-2'">
              Stage{{ item.besoinStage === 'a_verifier' ? ' ?' : '' }}
            </v-chip>
            <v-chip v-if="item.besoinBenevolat" size="x-small" label :variant="item.besoinBenevolat === 'oui' ? 'flat' : 'outlined'"
              :color="item.besoinBenevolat === 'oui' ? 'indigo' : 'orange-darken-2'">
              Bénévolat{{ item.besoinBenevolat === 'a_verifier' ? ' ?' : '' }}
            </v-chip>
          </div>
        </template>

        <template #item.interets="{ item }">
          <div v-if="item.metiers.length || item.domaines.length || item.domainesBenevolat.length" class="d-flex flex-wrap ga-1">
            <v-chip v-for="m in item.metiers" :key="'m' + m.id" size="x-small" color="primary" variant="tonal">{{ m.libelle }}</v-chip>
            <v-chip v-for="d in item.domaines" :key="'d' + d.id" size="x-small" variant="outlined">{{ d.libelle }}</v-chip>
            <v-chip v-for="d in item.domainesBenevolat" :key="'b' + d" size="x-small" color="indigo" variant="tonal" title="Domaine d'intérêt pour le bénévolat">
              <v-icon start size="x-small">mdi-hand-heart-outline</v-icon>{{ d }}
            </v-chip>
          </div>
          <a v-else href="#" class="text-caption text-error" @click.prevent="goParticipant(item.id)">À renseigner</a>
        </template>

        <template #item.regionOrdre="{ item }">
          <span v-if="item.region" class="region-pill" :style="{ background: regionColor(item.region) }">{{ item.region }}</span>
          <span v-else class="text-caption text-grey">—</span>
        </template>

        <template #item.degreOrdre="{ item }">
          <span v-if="item.degreInfo" class="d-inline-flex align-center text-no-wrap">
            <v-icon size="16" :color="item.degreInfo.color" class="mr-1">{{ item.degreInfo.icon }}</v-icon>{{ item.degreInfo.short }}
          </span>
          <span v-else class="text-grey">—</span>
        </template>

        <template #item.suivi="{ item }">
          <div v-if="item.dernierCommentaire" class="suivi">
            <div class="suivi__text">{{ item.dernierCommentaire.text }}</div>
            <div class="text-caption text-grey">
              {{ fmtDate(item.dernierCommentaire.createdAt) }}<span v-if="item.dernierCommentaire.authorName"> · {{ item.dernierCommentaire.authorName }}</span>
            </div>
          </div>
          <span v-else class="text-caption text-grey">—</span>
        </template>

        <template #item.situation="{ item }">
          <div v-if="item.activites.length" class="d-flex flex-column ga-1">
            <a v-for="a in item.activites" :key="a.stageId" href="#" class="activite" @click.prevent="goStage(a.stageId)">
              <v-icon size="14" color="success">mdi-briefcase-check-outline</v-icon>
              {{ a.type || 'Stage' }}<span v-if="a.entreprise"> · {{ a.entreprise }}</span>
            </a>
          </div>
          <v-chip v-else-if="item.pistes.length" size="x-small" color="blue" variant="tonal" class="no-print" @click="toggleExpand(item.id)">
            <v-icon start size="14">mdi-office-building-marker-outline</v-icon>{{ item.pistes.length }} piste(s)
          </v-chip>
          <span v-else class="text-caption text-grey">À chercher</span>
        </template>

        <!-- Détail : pistes d'entreprises + disponibilités -->
        <template #expanded-row="{ columns, item }">
          <tr class="expanded">
            <td :colspan="columns.length" class="py-3">
              <v-row dense>
                <v-col cols="12" md="8">
                  <div class="detail-title">
                    <v-icon size="16" class="mr-1">mdi-office-building-marker-outline</v-icon>
                    Entreprises {{ item.region ? `de la région ${item.region}` : '' }} avec un métier ou domaine en commun
                  </div>
                  <div v-if="!item.region" class="text-caption text-grey">
                    Région inconnue : renseigner l'adresse ou la région sur la fiche pour voir des pistes.
                  </div>
                  <div v-else-if="!item.metiers.length && !item.domaines.length" class="text-caption text-grey">
                    Renseigner les métiers visés ou domaines d'intérêt sur la fiche pour voir des pistes.
                  </div>
                  <div v-else-if="!item.pistes.length" class="text-caption text-grey">
                    Aucune entreprise connue dans cette région pour ces métiers/domaines.
                  </div>
                  <div v-else class="pistes">
                    <a v-for="e in item.pistes.slice(0, 12)" :key="e.id" href="#" class="piste" @click.prevent="goEntreprise(e.id)">
                      <div class="font-weight-medium">{{ e.nom }}</div>
                      <div class="text-caption text-grey">{{ e.ville }} · {{ e.communs.join(', ') }}</div>
                    </a>
                    <div v-if="item.pistes.length > 12" class="text-caption text-grey align-self-center">
                      + {{ item.pistes.length - 12 }} autre(s)
                    </div>
                  </div>
                </v-col>
                <v-col cols="12" md="4">
                  <div class="detail-title"><v-icon size="16" class="mr-1">mdi-clock-outline</v-icon>Disponibilités</div>
                  <div class="text-body-2 pre-line">{{ item.disponibilites || '—' }}</div>
                </v-col>
              </v-row>
            </td>
          </tr>
        </template>
      </v-data-table>
    </v-card>

    <v-snackbar v-model="snack.show" color="error" timeout="3000" location="bottom right">{{ snack.text }}</v-snackbar>
  </v-container>
</template>

<script setup>
import { BENEVOLAT_DOMAINES, domainesAffiches, aUnDesDomaines } from '@/utils/benevolat'
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import { useRegions, regionDeCommune } from '@/utils/regions'
import { matchesSearch } from '@/utils/search'

moment.locale('fr')
const router = useRouter()
const { regionNames, regionColor } = useRegions()

// ─── Constantes ───────────────────────────────────────────────────────────────
const GROUPES = [
  { value: 'fi_a1', label: 'A1', color: 'teal', ordre: 1 },
  { value: 'fi_a2', label: 'A2', color: 'teal-darken-3', ordre: 2 },
  { value: 'telecours', label: 'Télécours', color: 'deep-purple', ordre: 3 },
]
const DEGRES = [
  { value: 'fort', short: 'Très motivé·e', icon: 'mdi-fire', color: 'deep-orange', ordre: 0 },
  { value: 'normal', short: 'Normal', icon: 'mdi-circle-medium', color: 'grey', ordre: 1 },
  { value: 'a_confirmer', short: 'À confirmer', icon: 'mdi-help-circle-outline', color: 'orange', ordre: 2 },
]
const TOUTES = 'all'
const SANS_REGION = 'Sans région'

// ─── État ─────────────────────────────────────────────────────────────────────
const loading = ref(false)
const raw = ref({ participants: [], entreprises: [] })
const expanded = ref([])
const snack = ref({ show: false, text: '' })

const search = ref('')
const fSession = ref(TOUTES)
const fGroupe = ref([])
const fType = ref([])
const fRegions = ref([])
const fDegre = ref([])
const fAVerifier = ref(false)
const fSansInteret = ref(false)
const fEnActivite = ref(false)
const fDomainesBenevolat = ref([])

const hasFilters = computed(() =>
  !!search.value || fGroupe.value.length || fType.value.length || fRegions.value.length || fDegre.value.length || fDomainesBenevolat.value.length ||
  fAVerifier.value || fSansInteret.value || fEnActivite.value)

function resetFilters() {
  search.value = ''
  fGroupe.value = []; fType.value = []; fRegions.value = []; fDegre.value = []; fDomainesBenevolat.value = []
  fAVerifier.value = false; fSansInteret.value = false; fEnActivite.value = false
}

const regionOptions = computed(() => [...regionNames.value, SANS_REGION])

// ─── Sessions ─────────────────────────────────────────────────────────────────
const sessionKey = s => (s ? `${s.year}-${s.number}` : null)
function sessionLabel(key) {
  if (!key || key === TOUTES) return 'toutes les sessions'
  const [y, n] = key.split('-')
  return `session ${n} ${y}`
}
const sessionOptions = computed(() => {
  const keys = [...new Set(raw.value.participants.map(p => sessionKey(p.session)).filter(Boolean))]
    .sort((a, b) => {
      const [ya, na] = a.split('-').map(Number)
      const [yb, nb] = b.split('-').map(Number)
      return yb - ya || nb - na
    })
  return [{ title: 'Toutes les sessions', value: TOUTES }, ...keys.map(k => ({ title: `Session ${k.split('-')[1]} — ${k.split('-')[0]}`, value: k }))]
})

// ─── Normalisation ────────────────────────────────────────────────────────────
function groupeDe(classe) {
  const c = (classe || '').toLowerCase()
  if (!c) return null
  if (c.includes('tele') || c.includes('télé')) return GROUPES[2]
  if (c.includes('a1')) return GROUPES[0]
  if (c.includes('a2')) return GROUPES[1]
  return null
}
function sexeDe(g) {
  const s = (g || '').toLowerCase()
  if (s.startsWith('f')) return 'F'
  if (s.startsWith('h') || s.startsWith('m')) return 'M'
  return null
}
// Localité seule, quelle que soit la façon dont l'adresse a été saisie (« Rue X 3, 1004 Lausanne »,
// « Rue X 3 1004 Lausanne », « Lausanne ») : après le NPA s'il y en a un, sinon dernier segment
// après une virgule, sinon l'adresse entière seulement si elle ne contient pas de numéro.
function communeDe(adresse) {
  const a = (adresse || '').trim()
  if (!a) return ''
  const apresNpa = a.match(/\d{4}\s+([^,\d][^,]*)/)
  if (apresNpa) return apresNpa[1].trim()
  if (a.includes(',')) return a.split(',').pop().trim().replace(/^\d{4}\s*/, '')
  return /\d/.test(a) ? '' : a
}
const knownRegion = r => (r && regionNames.value.includes(r) ? r : null)

// Entreprises par région (déduite de leur ville), pour les rapprochements.
const entreprisesParRegion = computed(() => {
  const map = {}
  for (const e of raw.value.entreprises) {
    const region = knownRegion(regionDeCommune(e.ville))
    if (region) (map[region] ||= []).push(e)
  }
  return map
})

function pistesPour(p, region) {
  if (!region) return []
  const metiers = new Map(p.metiers.map(m => [m.id, m.libelle]))
  const domaines = new Map(p.domaines.map(d => [d.id, d.libelle]))
  if (!metiers.size && !domaines.size) return []
  return (entreprisesParRegion.value[region] || [])
    .map(e => ({
      ...e,
      communs: [...e.metierIds.filter(id => metiers.has(id)).map(id => metiers.get(id)),
        ...e.domaineIds.filter(id => domaines.has(id)).map(id => domaines.get(id))],
    }))
    .filter(e => e.communs.length)
    .sort((a, b) => b.communs.length - a.communs.length || a.nom.localeCompare(b.nom, 'fr'))
}

const participants = computed(() => raw.value.participants.map(p => {
  const region = knownRegion(p.region) || knownRegion(regionDeCommune(p.localite || p.adresse))
  const groupe = groupeDe(p.session?.classe)
  const degreInfo = DEGRES.find(d => d.value === p.degre) || null
  const besoinStage = ['oui', 'a_verifier'].includes(p.besoinStage) ? p.besoinStage : null
  const besoinBenevolat = ['oui', 'a_verifier'].includes(p.besoinBenevolat) ? p.besoinBenevolat : null
  return {
    ...p,
    nom: `${p.lastName} ${p.firstname}`,
    commune: p.localite || communeDe(p.adresse),
    domainesBenevolat: domainesAffiches(p),
    sexe: sexeDe(p.genre),
    region, regionOrdre: region ? regionNames.value.indexOf(region) : 999,
    groupe, groupeOrdre: groupe?.ordre ?? 9,
    degreInfo, degreOrdre: degreInfo?.ordre ?? 1,
    besoinStage, besoinBenevolat,
    aVerifier: besoinStage === 'a_verifier' || besoinBenevolat === 'a_verifier' || p.degre === 'a_confirmer',
    sansInteret: !p.metiers.length && !p.domaines.length && !(p.benevolatDomaines || []).length,
    sessionKey: sessionKey(p.session),
    pistes: pistesPour(p, region),
  }
}))

// Filtres hors indicateurs : la session, puis les autres critères.
const parSession = computed(() =>
  participants.value.filter(p => fSession.value === TOUTES || p.sessionKey === fSession.value))

const rows = computed(() => parSession.value.filter(p =>
  (!fGroupe.value.length || fGroupe.value.includes(p.groupe?.value)) &&
  (!fType.value.length || (fType.value.includes('stage') && p.besoinStage) || (fType.value.includes('benevolat') && p.besoinBenevolat)) &&
  (!fRegions.value.length || fRegions.value.includes(p.region || SANS_REGION)) &&
  (!fDegre.value.length || fDegre.value.includes(p.degreInfo?.value || 'normal')) &&
  (!fAVerifier.value || p.aVerifier) &&
  (!fSansInteret.value || p.sansInteret) &&
  (!fEnActivite.value || p.activites.length) &&
  aUnDesDomaines(p, fDomainesBenevolat.value) &&
  (!search.value || matchesSearch(search.value, p.firstname, p.lastName, p.adresse,
    p.metiers.map(m => m.libelle), p.domaines.map(d => d.libelle), p.domainesBenevolat, p.benevolatTypesActivite || []))
))

// ─── Chiffres clés (cliquables : filtrent le tableau) ─────────────────────────
const kpis = computed(() => {
  const list = parSession.value
  const toggleType = t => () => { fType.value = fType.value.length === 1 && fType.value[0] === t ? [] : [t] }
  return [
    { key: 'total', label: 'Personnes', value: list.length, icon: 'mdi-account-multiple', color: 'primary', toggle: resetFilters },
    { key: 'stage', label: 'Cherchent un stage', value: list.filter(p => p.besoinStage).length, icon: 'mdi-briefcase-search-outline', color: 'green-darken-1',
      active: fType.value.length === 1 && fType.value[0] === 'stage', toggle: toggleType('stage') },
    { key: 'benevolat', label: 'Cherchent du bénévolat', value: list.filter(p => p.besoinBenevolat).length, icon: 'mdi-hand-heart-outline', color: 'indigo',
      active: fType.value.length === 1 && fType.value[0] === 'benevolat', toggle: toggleType('benevolat') },
    { key: 'verifier', label: 'À vérifier', value: list.filter(p => p.aVerifier).length, icon: 'mdi-help-circle-outline', color: 'orange-darken-2',
      active: fAVerifier.value, toggle: () => { fAVerifier.value = !fAVerifier.value } },
    { key: 'interets', label: 'Intérêts à renseigner', value: list.filter(p => p.sansInteret).length, icon: 'mdi-tag-off-outline', color: 'error',
      active: fSansInteret.value, toggle: () => { fSansInteret.value = !fSansInteret.value } },
    { key: 'activite', label: 'En stage / bénévolat', value: list.filter(p => p.activites.length).length, icon: 'mdi-briefcase-check-outline', color: 'success',
      active: fEnActivite.value, toggle: () => { fEnActivite.value = !fEnActivite.value } },
  ]
})

const headers = [
  { title: 'Nom', key: 'nom' },
  { title: 'Groupe', key: 'groupeOrdre', width: 90 },
  { title: 'Cherche', key: 'cherche', sortable: false },
  { title: "Métiers / domaines d'intérêt", key: 'interets', sortable: false },
  { title: 'Région', key: 'regionOrdre' },
  { title: 'Intérêt', key: 'degreOrdre' },
  { title: 'Dernier commentaire', key: 'suivi', sortable: false },
  { title: 'Situation / pistes', key: 'situation', sortable: false },
  { title: '', key: 'data-table-expand', width: 40 },
]

// ─── Helpers ──────────────────────────────────────────────────────────────────
const fmtDate = d => (d ? moment(d).format('DD.MM.YYYY') : '')
function identite(p) {
  return [p.sexe || '—', p.age != null ? `${p.age} ans` : 'âge ?'].join(' · ')
}
function toggleExpand(id) {
  expanded.value = expanded.value.includes(id) ? expanded.value.filter(x => x !== id) : [...expanded.value, id]
}
const goParticipant = id => router.push({ name: 'Participant-Detail', params: { id } })
const goEntreprise = id => router.push({ name: 'Entreprise-Modifier', params: { id } })
const goStage = id => router.push({ name: 'Stage-Modifier', params: { id } })
const print = () => window.print()

// ─── Chargement ───────────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/StagesBenevolat/tableau`)
    if (!res?.ok) { snack.value = { show: true, text: 'Impossible de charger le tableau' }; return }
    raw.value = await res.json()
    // Par défaut : la session la plus récente.
    if (fSession.value === TOUTES && sessionOptions.value.length > 1) fSession.value = sessionOptions.value[1].value
  } catch (e) {
    console.error(e)
    snack.value = { show: true, text: 'Erreur serveur' }
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 12px; }
.kpi {
  display: flex; align-items: center; padding: 12px 14px; cursor: pointer;
  background: #fff; border: 1px solid #e4e9f2; border-radius: 12px; transition: border-color .15s, box-shadow .15s;
}
.kpi:hover { border-color: #90caf9; box-shadow: 0 2px 8px rgba(30, 60, 120, .08); }
.kpi--active { border-color: #1976d2; background: #f3f8ff; }
.kpi__value { font-size: 1.5rem; font-weight: 800; line-height: 1.1; color: #1a2233; }
.kpi__label { font-size: .78rem; color: #5a6578; font-weight: 600; }

.filter-group { display: flex; align-items: center; gap: 6px; }
.filter-label { font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .04em; color: #7a8599; }

.sb-table :deep(td) { vertical-align: top; padding-top: 10px !important; padding-bottom: 10px !important; }
.region-pill { color: #fff; font-size: .72rem; font-weight: 700; padding: 2px 8px; border-radius: 10px; white-space: nowrap; }
.link { color: inherit; text-decoration: none; }
.link:hover { text-decoration: underline; color: #1976d2; }

.suivi { max-width: 280px; }
.suivi__text { font-size: .82rem; display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; overflow: hidden; }
.activite { font-size: .8rem; color: #2e7d32; text-decoration: none; white-space: nowrap; }
.activite:hover { text-decoration: underline; }

.expanded td { background: #f8fafd; }
.detail-title { font-size: .72rem; font-weight: 800; text-transform: uppercase; letter-spacing: .04em; color: #5a6578; display: flex; align-items: center; margin-bottom: 6px; }
.pistes { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 6px; }
.piste { display: block; padding: 6px 10px; background: #fff; border: 1px solid #e4e9f2; border-radius: 8px; color: inherit; text-decoration: none; }
.piste:hover { border-color: #90caf9; }
.pre-line { white-space: pre-line; }

@media print {
  .no-print { display: none !important; }
  .sb-table :deep(.v-data-table-footer), .sb-table :deep(.v-data-table__td--expanded-row) { display: none; }
}
</style>
