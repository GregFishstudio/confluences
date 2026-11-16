<!--
  Import d'un tableau d'inscriptions (CSV exporté d'Excel) en candidatures, en 3 étapes :
   1. fichier + session visée / mesure / statut par défaut ;
   2. correspondance colonne du fichier → champ de la candidature (détectée d'après les
      en-têtes, mémorisée pour le prochain import) ;
   3. vérification ligne par ligne : création, complément d'une candidature existante (même
      nom/prénom), ou ignorée (déjà participant·e — reconduit·e par l'onglet de la session).
-->
<template>
  <v-dialog :model-value="modelValue" max-width="1200" scrollable persistent @update:model-value="close">
    <v-card rounded="lg" class="import-card">
      <v-card-title class="d-flex align-center pa-4">
        <v-icon start color="primary">mdi-file-import-outline</v-icon>
        Importer des inscriptions (CSV)
        <v-spacer />
        <v-btn icon variant="text" size="small" :disabled="importing" @click="close(false)"><v-icon>mdi-close</v-icon></v-btn>
      </v-card-title>

      <!-- Étapes -->
      <div class="steps px-4 pb-3">
        <div v-for="(s, i) in STEPS" :key="s" class="step" :class="{ 'step--done': step > i + 1, 'step--active': step === i + 1 }">
          <span class="step__num">{{ step > i + 1 ? '✓' : i + 1 }}</span>{{ s }}
        </div>
      </div>
      <v-divider />

      <v-card-text class="pa-4">
        <v-window v-model="step">

          <!-- ════════ 1. Fichier et paramètres ════════ -->
          <v-window-item :value="1">
            <v-row>
              <v-col cols="12" md="6">
                <v-file-input
                  v-model="file" accept=".csv,text/csv" label="Fichier CSV" prepend-icon="mdi-file-delimited-outline"
                  variant="outlined" density="comfortable" show-size :error-messages="fileError" @update:model-value="onFile"
                />
                <v-alert type="info" variant="tonal" density="compact" class="text-caption">
                  Depuis Excel : <b>Fichier → Enregistrer sous → CSV (séparateur : point-virgule)</b>.
                  Une ligne de titre au-dessus des en-têtes ne pose pas de problème.
                </v-alert>
                <div v-if="csv" class="text-caption text-grey mt-3">
                  {{ csv.rows.length }} lignes lues · encodage {{ csv.encoding }} · séparateur « {{ csv.delimiter === '\t' ? 'tabulation' : csv.delimiter }} »
                </div>
                <v-select
                  v-if="csv" v-model="headerIndex" :items="headerChoices" label="Ligne des en-têtes"
                  variant="outlined" density="compact" class="mt-3" hide-details
                />
              </v-col>
              <v-col cols="12" md="6">
                <div class="text-subtitle-2 font-weight-bold mb-2">Valeurs par défaut</div>
                <div class="text-caption text-grey mb-3">Appliquées aux lignes dont le fichier ne précise pas la valeur.</div>
                <div class="d-flex align-center ga-3 mb-3">
                  <span class="param-label">Mesure</span>
                  <v-btn-toggle v-model="params.mesure" mandatory density="compact" variant="outlined" divided color="primary">
                    <v-btn v-for="m in MESURES" :key="m" :value="m" size="small">{{ m }}</v-btn>
                  </v-btn-toggle>
                </div>
                <v-row dense>
                  <v-col cols="6">
                    <v-select v-model="params.annee" :items="annees" label="Année visée" variant="outlined" density="compact" />
                  </v-col>
                  <v-col cols="6">
                    <v-select v-model="params.session" :items="[1, 2, 3, 4]" label="Session visée" variant="outlined" density="compact" />
                  </v-col>
                  <v-col cols="12">
                    <v-select
                      v-model="params.statut" :items="statutsPourMesure" label="Statut par défaut" variant="outlined" density="compact"
                      hint="Si le fichier a une colonne « Niveau » (A1/A2), le statut devient « En attente A1/A2 … »" persistent-hint
                    />
                  </v-col>
                </v-row>
              </v-col>
            </v-row>
          </v-window-item>

          <!-- ════════ 2. Correspondance des colonnes ════════ -->
          <v-window-item :value="2">
            <div class="d-flex align-center flex-wrap ga-2 mb-3">
              <div class="text-body-2">
                Indiquez pour chaque colonne du fichier le champ de la candidature correspondant. Les colonnes laissées vides sont ignorées.
              </div>
              <v-spacer />
              <v-btn size="small" variant="text" prepend-icon="mdi-auto-fix" @click="autoMap(true)">Détecter à nouveau</v-btn>
            </div>
            <v-alert v-if="!nomMappe" type="warning" variant="tonal" density="compact" class="mb-3">
              Associez au moins une colonne à <b>Nom</b> ou à <b>Nom et prénom</b>.
            </v-alert>
            <v-table density="compact" class="map-table">
              <thead>
                <tr>
                  <th style="width: 26%">Colonne du fichier</th>
                  <th>Exemples</th>
                  <th style="width: 32%">Champ dans Confluences</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(h, i) in headers" :key="i" :class="{ 'row-ignored': !mapping[i] }">
                  <td class="font-weight-medium">{{ h || `(colonne ${i + 1})` }}</td>
                  <td class="text-caption text-grey samples">{{ samples(i) }}</td>
                  <td>
                    <v-select
                      v-model="mapping[i]" :items="fieldItems" item-title="label" item-value="key"
                      placeholder="— Ignorer —" variant="outlined" density="compact" hide-details clearable
                    >
                      <template #item="{ props: p, item }">
                        <v-list-subheader v-if="item.raw.header">{{ item.raw.header }}</v-list-subheader>
                        <v-list-item v-else v-bind="p" density="compact" />
                      </template>
                    </v-select>
                  </td>
                </tr>
              </tbody>
            </v-table>
          </v-window-item>

          <!-- ════════ 3. Vérification ════════ -->
          <v-window-item :value="3">
            <div class="d-flex flex-wrap align-center ga-2 mb-3">
              <v-chip color="success" variant="tonal" prepend-icon="mdi-plus-circle-outline">{{ counts.create }} à créer</v-chip>
              <v-chip color="blue" variant="tonal" prepend-icon="mdi-pencil-plus-outline">{{ counts.update }} à compléter</v-chip>
              <v-chip variant="tonal" prepend-icon="mdi-minus-circle-outline">{{ counts.skip }} ignorée(s)</v-chip>
              <v-chip v-if="counts.errors" color="error" variant="tonal" prepend-icon="mdi-alert-circle-outline">{{ counts.errors }} en erreur</v-chip>
              <v-spacer />
              <v-switch v-model="onlyIssues" label="Seulement les lignes à vérifier" color="primary" density="compact" hide-details class="flex-grow-0" />
            </div>
            <div class="text-caption text-grey mb-2">
              Session visée : <b>Session {{ params.session }} — {{ params.annee }}</b>.
              « Compléter » ajoute les valeurs du fichier à la candidature existante sans effacer ce qui y est déjà.
            </div>
            <div class="preview-scroll">
              <v-table density="compact" fixed-header class="preview-table">
                <thead>
                  <tr>
                    <th style="width: 200px">Action</th>
                    <th>Personne</th>
                    <th>Mesure · statut</th>
                    <th>AS</th>
                    <th>Déjà connu·e ?</th>
                    <th>À vérifier</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="r in previewRows" :key="r.idx" :class="`act-${r.action}`">
                    <td>
                      <v-btn-toggle v-model="r.action" mandatory density="compact" variant="outlined" divided :disabled="!!r.errors.length">
                        <v-btn value="create" size="x-small" :disabled="!!r.errors.length">Créer</v-btn>
                        <v-btn v-if="r.match?.type === 'candidature'" value="update" size="x-small">Compléter</v-btn>
                        <v-btn value="skip" size="x-small">Ignorer</v-btn>
                      </v-btn-toggle>
                    </td>
                    <td>
                      <div class="font-weight-medium">{{ r.data.nom }} {{ r.data.prenom }}</div>
                      <div class="text-caption text-grey">
                        {{ [r.data.dateNaissance && fmtDate(r.data.dateNaissance), r.data.origine, r.data.telephone].filter(Boolean).join(' · ') }}
                      </div>
                    </td>
                    <td>
                      <div class="text-caption">{{ r.data.mesureVisee }}</div>
                      <div class="text-caption text-grey">{{ r.data.statut }}</div>
                    </td>
                    <td class="text-caption">
                      <span v-if="r.as">{{ r.as.label }}<v-icon v-if="r.as.nouveau" size="14" color="orange" class="ml-1" title="Sera ajouté·e au carnet des AS">mdi-account-plus</v-icon></span>
                      <span v-else class="text-grey">—</span>
                    </td>
                    <td class="text-caption">
                      <span v-if="r.match" :class="r.match.type === 'participant' ? 'text-purple' : 'text-blue'">
                        <v-icon size="14">{{ r.match.type === 'participant' ? 'mdi-account-check' : 'mdi-account-clock' }}</v-icon>
                        {{ r.match.label }}
                      </span>
                      <span v-else class="text-grey">Nouveau·elle</span>
                    </td>
                    <td class="text-caption">
                      <div v-for="e in r.errors" :key="e" class="text-error">{{ e }}</div>
                      <div v-for="w in r.warnings" :key="w" class="text-orange-darken-3">{{ w }}</div>
                    </td>
                  </tr>
                </tbody>
              </v-table>
            </div>
          </v-window-item>
        </v-window>

        <v-alert v-if="result" type="success" variant="tonal" class="mt-4">
          Import terminé : {{ result.created }} candidature(s) créée(s), {{ result.updated }} complétée(s)<span v-if="result.asCreated">,
          {{ result.asCreated }} AS ajouté·e(s) au carnet</span>.
        </v-alert>
        <v-alert v-if="importError" type="error" variant="tonal" class="mt-4">{{ importError }}</v-alert>
      </v-card-text>

      <v-divider />
      <v-card-actions class="pa-4">
        <v-btn v-if="step > 1 && !result" variant="text" prepend-icon="mdi-arrow-left" :disabled="importing" @click="step--">Retour</v-btn>
        <v-spacer />
        <v-btn variant="text" :disabled="importing" @click="close(false)">{{ result ? 'Fermer' : 'Annuler' }}</v-btn>
        <v-btn v-if="step === 1" color="primary" variant="flat" :disabled="!csv || !dataRows.length" append-icon="mdi-arrow-right" @click="goMapping">
          Colonnes
        </v-btn>
        <v-btn v-else-if="step === 2" color="primary" variant="flat" :disabled="!nomMappe" append-icon="mdi-arrow-right" @click="goPreview">
          Vérifier
        </v-btn>
        <v-btn
          v-else-if="!result" color="primary" variant="flat" prepend-icon="mdi-database-import-outline" :loading="importing"
          :disabled="!counts.create && !counts.update" @click="doImport"
        >
          Importer {{ counts.create + counts.update }} ligne(s)
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useStore } from 'vuex'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import { readCsvFile, normalizeHeader } from '@/utils/csv'
import { parseDateFr } from '@/utils/dates'
import { normalizeSearch } from '@/utils/search'
import { MESURES, STATUTS_CANDIDATURE, MANDANTS, readApiError } from '@/utils/inscriptionOptions'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  assistants: { type: Array, default: () => [] },
  // Valeurs par défaut (ex. depuis l'onglet d'une session) : { mesure, statut, annee, session }.
  defaults: { type: Object, default: null },
})
const emit = defineEmits(['update:modelValue', 'imported'])
const store = useStore()

const STEPS = ['Fichier', 'Colonnes', 'Vérification']
const MAPPING_KEY = 'candidatureImport.mapping'

// ─── Champs cibles ────────────────────────────────────────────────────────────
// `aliases` : en-têtes reconnus automatiquement (normalisés : minuscules, sans accents).
const FIELDS = [
  { header: 'Personne' },
  { key: 'nomComplet', label: 'Nom et prénom (même colonne)', aliases: ['nom prenom', 'nom et prenom', 'prenom nom', 'prenom et nom', 'nom complet', 'beneficiaire', 'participant', 'participante', 'candidat', 'candidate'] },
  { key: 'nom', label: 'Nom', aliases: ['nom', 'nom de famille', 'nom beneficiaire'] },
  { key: 'prenom', label: 'Prénom', aliases: ['prenom', 'prenom beneficiaire'] },
  { key: 'civilite', label: 'Civilité / sexe', aliases: ['civilite', 'sexe', 'genre', 'mme m', 'titre'] },
  { key: 'dateNaissance', label: 'Date de naissance', aliases: ['date de naissance', 'naissance', 'date naissance', 'ne e le', 'ne le', 'ddn'] },
  { key: 'adresse', label: 'Adresse', aliases: ['adresse', 'adresse postale', 'domicile', 'rue'] },
  { key: 'localite', label: 'NPA / localité (ajouté à l\'adresse)', aliases: ['npa', 'localite', 'npa localite', 'commune', 'lieu', 'ville'] },
  { key: 'telephone', label: 'Téléphone', aliases: ['telephone', 'tel', 'natel', 'portable', 'mobile', 'no de tel', 'tel beneficiaire'] },
  { key: 'email', label: 'E-mail', aliases: ['email', 'e mail', 'mail', 'courriel', 'adresse e mail'] },
  { key: 'origine', label: 'Origine / pays', aliases: ['origine', 'pays', 'pays d origine', 'nationalite'] },
  { header: 'Inscription' },
  { key: 'mesureVisee', label: 'Mesure (F&I / Télécours)', aliases: ['mesure', 'cours', 'programme', 'mesure visee'] },
  { key: 'niveau', label: 'Niveau (A1 / A2)', aliases: ['niveau', 'groupe', 'classe', 'a1 a2', 'niveau vise'] },
  { key: 'statut', label: 'Statut', aliases: ['statut', 'etat', 'situation'] },
  { key: 'dateDemande', label: 'Date de la demande', aliases: ['date demande', 'date de la demande', 'demande', 'date de demande', 'date inscription', 'date d inscription', 'recu le', 'date'] },
  { key: 'priorite', label: 'Priorité', aliases: ['priorite', 'prio'] },
  { key: 'mandant', label: 'Mandant (CSIR / CSR)', aliases: ['mandant', 'csr csir', 'csir csr', 'service', 'organisme'] },
  { key: 'contratMaori', label: 'Contrat Maori (oui/non)', aliases: ['maori', 'contrat maori'] },
  { key: 'inscriptionConfirmee', label: 'Inscription confirmée (oui/non)', aliases: ['confirme', 'confirmee', 'inscription confirmee', 'confirmation'] },
  { header: 'Assistant·e social·e' },
  { key: 'asNom', label: 'AS : nom (ou « Prénom Nom »)', aliases: ['as', 'assistant social', 'assistante sociale', 'assistant e social e', 'nom as', 'as nom', 'referent', 'referente'] },
  { key: 'asEmail', label: 'AS : e-mail', aliases: ['email as', 'e mail as', 'mail as', 'courriel as', 'as email', 'as e mail'] },
  { key: 'asTelephone', label: 'AS : téléphone', aliases: ['tel as', 'telephone as', 'as tel', 'as telephone'] },
  { header: 'Remarques et évaluation' },
  { key: 'remarqueAS', label: 'Remarque AS (fil daté)', aliases: ['remarques as', 'remarque as', 'commentaire as', 'commentaires as'] },
  { key: 'remarquesGenerales', label: 'Remarques générales', aliases: ['remarques', 'remarque', 'remarques generales', 'remarques generales as', 'commentaires', 'commentaire', 'observations', 'notes'] },
  { key: 'notesRdv', label: 'Notes du RDV', aliases: ['rdv', 'notes rdv', 'entretien', 'notes entretien'] },
  { key: 'evaluationNiveau', label: 'Évaluation du niveau', aliases: ['evaluation', 'niveau evalue', 'test', 'resultat test'] },
  { key: 'dateEvaluation', label: "Date de l'évaluation", aliases: ['date evaluation', 'date test', 'date de l evaluation'] },
  { key: 'transmissionAS', label: 'Transmission AS / ML', aliases: ['transmission', 'transmission as', 'transmis'] },
]
const fieldItems = FIELDS.map(f => (f.header ? { header: f.header, key: `h-${f.header}`, label: f.header, props: { disabled: true } } : f))

// ─── État ─────────────────────────────────────────────────────────────────────
const step = ref(1)
const file = ref(null)
const fileError = ref('')
const csv = ref(null)
const headerIndex = ref(0)
const mapping = ref([])
const params = ref(defaultParams())
const previewRows = ref([])
const allRows = ref([])
const onlyIssues = ref(false)
const importing = ref(false)
const importError = ref('')
const result = ref(null)
const candidatures = ref([])

const annees = computed(() => {
  const y = new Date().getFullYear()
  return [y - 1, y, y + 1, y + 2]
})
const statutsPourMesure = computed(() =>
  STATUTS_CANDIDATURE.filter(s => !/^En attente/.test(s) || s.endsWith(params.value.mesure)))

function defaultParams() {
  const d = props.defaults || {}
  return {
    mesure: d.mesure || 'F&I',
    annee: d.annee || new Date().getFullYear() + 1,
    session: d.session || 1,
    statut: d.statut || 'À évaluer',
  }
}

watch(() => params.value.mesure, () => {
  if (!statutsPourMesure.value.includes(params.value.statut)) params.value.statut = 'À évaluer'
})

watch(() => props.modelValue, open => {
  if (!open) return
  step.value = 1
  file.value = null
  csv.value = null
  fileError.value = ''
  result.value = null
  importError.value = ''
  params.value = defaultParams()
})

// ─── Étape 1 : lecture du fichier ─────────────────────────────────────────────
const headerChoices = computed(() => (csv.value?.rows || []).slice(0, 10).map((r, i) => ({
  title: `Ligne ${i + 1} : ${r.filter(Boolean).slice(0, 5).join(' | ')}`.slice(0, 110),
  value: i,
})))

function scoreHeader(row) {
  const norm = row.map(normalizeHeader)
  return norm.filter(h => h && FIELDS.some(f => f.aliases?.includes(h))).length
}

async function onFile(f) {
  const picked = Array.isArray(f) ? f[0] : f
  fileError.value = ''
  csv.value = null
  if (!picked) return
  try {
    const parsed = await readCsvFile(picked)
    if (parsed.rows.length < 2) { fileError.value = 'Le fichier semble vide.'; return }
    csv.value = parsed
    // En-têtes : la ligne (parmi les 10 premières) qui reconnaît le plus de colonnes.
    let best = 0
    parsed.rows.slice(0, 10).forEach((r, i) => { if (scoreHeader(r) > scoreHeader(parsed.rows[best])) best = i })
    headerIndex.value = best
  } catch (e) {
    console.error(e)
    fileError.value = 'Impossible de lire ce fichier. Est-ce bien un CSV ?'
  }
}

const headers = computed(() => {
  if (!csv.value) return []
  const width = Math.max(...csv.value.rows.map(r => r.length))
  const h = csv.value.rows[headerIndex.value] || []
  return Array.from({ length: width }, (_, i) => h[i] || '')
})
const dataRows = computed(() => (csv.value?.rows || []).slice(headerIndex.value + 1))

// ─── Étape 2 : correspondance ─────────────────────────────────────────────────
function loadSavedMapping() {
  try { return JSON.parse(localStorage.getItem(MAPPING_KEY) || '{}') } catch { return {} }
}
function saveMapping() {
  try {
    const saved = loadSavedMapping()
    headers.value.forEach((h, i) => {
      const n = normalizeHeader(h)
      if (!n) return
      if (mapping.value[i]) saved[n] = mapping.value[i]
      else saved[n] = null
    })
    localStorage.setItem(MAPPING_KEY, JSON.stringify(saved))
  } catch { /* stockage indisponible : la détection automatique suffit */ }
}

function guess(header) {
  const n = normalizeHeader(header)
  if (!n) return null
  const exact = FIELDS.find(f => f.aliases?.includes(n))
  if (exact) return exact.key
  // En-tête plus long contenant un alias (« Date de naissance (jj.mm.aaaa) ») : l'alias le plus long gagne.
  let best = null
  for (const f of FIELDS) {
    for (const a of f.aliases || []) {
      if (a.length >= 4 && new RegExp(`(^| )${a}( |$)`).test(n) && (!best || a.length > best.len)) best = { key: f.key, len: a.length }
    }
  }
  return best?.key || null
}

function autoMap(ignoreSaved = false) {
  const saved = ignoreSaved ? {} : loadSavedMapping()
  mapping.value = headers.value.map(h => {
    const n = normalizeHeader(h)
    if (n in saved) return saved[n]
    return guess(h)
  })
}

const nomMappe = computed(() => mapping.value.includes('nom') || mapping.value.includes('nomComplet'))

function samples(i) {
  return dataRows.value.map(r => r[i]).filter(Boolean).slice(0, 3).join(' · ') || '—'
}

function goMapping() {
  autoMap()
  step.value = 2
}

// ─── Normalisation des valeurs ────────────────────────────────────────────────
const truthy = v => /^(oui|o|x|yes|y|1|true|vrai|ok|✓|✔)$/i.test((v || '').trim())

function civiliteDe(v) {
  const n = normalizeHeader(v)
  if (!n) return null
  if (/^(mme|madame|f|femme|mlle|mademoiselle)\b/.test(n)) return 'Madame'
  if (/^(m|mr|monsieur|h|homme)\b/.test(n)) return 'Monsieur'
  return null
}
function mesureDe(v) {
  const n = normalizeHeader(v)
  if (!n) return null
  if (n.includes('tele') || n.includes('fent') || n.includes('distance')) return 'Télécours'
  if (n.includes('f&i') || /\bfi\b/.test(n) || n.includes('f et i') || n.includes('integration')) return 'F&I'
  return null
}
function mandantDe(v) {
  const n = normalizeHeader(v)
  if (!n) return null
  if (n.includes('csir')) return 'CSIR'
  if (n.includes('csr')) return 'CSR'
  return MANDANTS.includes(v) ? v : v.trim()
}
const niveauDe = v => (/a\s*2/i.test(v || '') ? 'A2' : /a\s*1/i.test(v || '') ? 'A1' : null)
function statutDe(v) {
  const n = normalizeHeader(v)
  if (!n) return null
  return STATUTS_CANDIDATURE.find(s => normalizeHeader(s) === n) || v.trim()
}
// Date "jj.mm.aaaa", "aaaa-mm-jj" ou numéro de série Excel (45678).
function dateDe(v) {
  const s = (v || '').trim()
  if (!s) return { value: null }
  if (/^\d{5}$/.test(s)) return { value: moment('1899-12-30').add(Number(s), 'days').format('YYYY-MM-DD') }
  const d = parseDateFr(s.split(' ')[0])
  return d ? { value: d } : { value: null, invalid: s }
}

// "DUPONT Marie" → nom DUPONT / prénom Marie ; "Marie Dupont" → dernier mot = nom.
function splitNomComplet(v) {
  const parts = (v || '').trim().split(/\s+/).filter(Boolean)
  if (parts.length < 2) return { nom: parts[0] || '', prenom: '' }
  const upper = parts.filter(p => p.length > 1 && p === p.toUpperCase() && /[A-Z]/.test(p))
  if (upper.length && upper.length < parts.length) {
    return { nom: upper.join(' '), prenom: parts.filter(p => !upper.includes(p)).join(' ') }
  }
  return { nom: parts.slice(1).join(' '), prenom: parts[0] }
}

// ─── Doublons ─────────────────────────────────────────────────────────────────
const personKey = (nom, prenom) => [normalizeSearch(nom), normalizeSearch(prenom)].sort().join('|')

const candidatureIndex = computed(() => {
  const map = new Map()
  for (const c of candidatures.value) map.set(personKey(c.nom, c.prenom), c)
  return map
})
const participantIndex = computed(() => {
  const map = new Map()
  for (const u of store.state.user?.users || []) {
    map.set(personKey(u.nom || u.lastName, u.prenom || u.firstname), u)
  }
  return map
})
const sameBirth = (a, b) => !a || !b || String(a).substring(0, 10) === String(b).substring(0, 10)

function findAs(row) {
  const email = (row.asEmail || (row.asNom?.includes('@') ? row.asNom : '') || '').trim()
  const nom = row.asNom && !row.asNom.includes('@') ? row.asNom.trim() : ''
  if (!email && !nom) return null
  let a = email && props.assistants.find(x => x.email?.toLowerCase() === email.toLowerCase())
  if (!a && nom) {
    const k = normalizeSearch(nom).split(' ').sort().join(' ')
    a = props.assistants.find(x => normalizeSearch(`${x.prenom || ''} ${x.nom || ''}`).split(' ').filter(Boolean).sort().join(' ') === k)
  }
  if (a) return { email: a.email, label: [a.prenom, a.nom].filter(Boolean).join(' ') || a.email, nouveau: false }
  if (email) {
    const parts = nom.split(/\s+/)
    return { email, prenom: parts.length > 1 ? parts[0] : '', nom: parts.length > 1 ? parts.slice(1).join(' ') : nom, telephone: row.asTelephone, label: nom || email, nouveau: true }
  }
  return { email: null, label: nom, inconnu: true }
}

// ─── Étape 3 : lignes ─────────────────────────────────────────────────────────
const ADRESSE_KEYS = ['adresse', 'localite']
const TEXT_KEYS = ['remarquesGenerales', 'remarqueAS', 'notesRdv', 'transmissionAS', 'evaluationNiveau']
const columnsPerField = computed(() => {
  const n = {}
  mapping.value.forEach(k => { if (k) n[k] = (n[k] || 0) + 1 })
  return n
})

function buildRow(cells, idx) {
  // Valeurs brutes par champ. Plusieurs colonnes vers un même champ texte : valeurs jointes
  // (adresse : « rue, localité » ; remarques : une ligne par colonne, précédée de son en-tête).
  const v = {}
  mapping.value.forEach((key, i) => {
    if (!key) return
    const cell = (cells[i] || '').trim()
    if (!cell) return
    if (ADRESSE_KEYS.includes(key)) v[key] = v[key] ? `${v[key]}, ${cell}` : cell
    else if (TEXT_KEYS.includes(key) && columnsPerField.value[key] > 1) {
      const line = `${headers.value[i]} : ${cell}`
      v[key] = v[key] ? `${v[key]}\n${line}` : line
    } else if (!v[key]) v[key] = cell
  })

  const errors = []
  const warnings = []
  let { nom, prenom } = v.nomComplet ? splitNomComplet(v.nomComplet) : { nom: '', prenom: '' }
  if (v.nom) nom = v.nom
  if (v.prenom) prenom = v.prenom
  if (!nom) errors.push('Nom manquant')

  const mesure = mesureDe(v.mesureVisee) || params.value.mesure
  if (v.mesureVisee && !mesureDe(v.mesureVisee)) warnings.push(`Mesure « ${v.mesureVisee} » non reconnue → ${params.value.mesure}`)
  const niveau = niveauDe(v.niveau)
  const statut = statutDe(v.statut) || (niveau ? `En attente ${niveau} ${mesure}` : params.value.statut)
  if (v.statut && !STATUTS_CANDIDATURE.includes(statut)) warnings.push(`Statut « ${statut} » hors liste`)

  const naissance = dateDe(v.dateNaissance)
  const demande = dateDe(v.dateDemande)
  const evaluation = dateDe(v.dateEvaluation)
  for (const [label, d] of [['Naissance', naissance], ['Date de demande', demande], ["Date d'évaluation", evaluation]]) {
    if (d.invalid) warnings.push(`${label} « ${d.invalid} » illisible (ignorée)`)
  }
  const priorite = v.priorite ? parseInt(v.priorite, 10) : null
  if (v.priorite && Number.isNaN(priorite)) warnings.push(`Priorité « ${v.priorite} » illisible`)

  const as = findAs(v)
  let remarquesGenerales = v.remarquesGenerales || ''
  if (as?.inconnu) {
    warnings.push(`AS « ${as.label} » introuvable dans le carnet (noté en remarque)`)
    remarquesGenerales = [remarquesGenerales, `AS : ${as.label}`].filter(Boolean).join('\n')
  }

  const data = {
    nom, prenom: prenom || null,
    civilite: civiliteDe(v.civilite),
    dateNaissance: naissance.value,
    adresse: [v.adresse, v.localite].filter(Boolean).join(', ') || null,
    telephone: v.telephone || null,
    email: v.email || null,
    origine: v.origine || null,
    mandant: mandantDe(v.mandant),
    mesureVisee: mesure,
    statut,
    anneeVisee: params.value.annee,
    sessionNumberVisee: params.value.session,
    sessionsVisees: [`${params.value.annee}-${params.value.session}`],
    dateDemande: demande.value,
    priorite: Number.isNaN(priorite) ? null : priorite,
    remarquesGenerales: remarquesGenerales || null,
    notesRdv: v.notesRdv || null,
    evaluationNiveau: v.evaluationNiveau || null,
    dateEvaluation: evaluation.value || (v.evaluationNiveau ? moment().format('YYYY-MM-DD') : null),
    transmissionAS: v.transmissionAS || null,
    contratMaori: truthy(v.contratMaori),
    inscriptionConfirmee: truthy(v.inscriptionConfirmee),
    participantId: null,
  }
  if (v.evaluationNiveau && !evaluation.value) warnings.push("Évaluation sans date : date du jour utilisée")

  // Personne déjà connue ?
  let match = null
  const key = personKey(nom, prenom)
  const c = candidatureIndex.value.get(key)
  const u = participantIndex.value.get(key)
  if (c && sameBirth(c.dateNaissance, data.dateNaissance)) {
    match = { type: 'candidature', id: c.candidatureId, label: `Candidature existante (${c.statut || 'sans statut'})` }
  } else if (u && sameBirth(u.birthday, data.dateNaissance)) {
    match = { type: 'participant', id: u.id, label: 'Déjà participant·e' }
    data.participantId = u.id
  } else if (c || u) {
    warnings.push('Homonyme avec une autre date de naissance')
  }

  const action = errors.length ? 'skip' : match?.type === 'candidature' ? 'update' : match?.type === 'participant' ? 'skip' : 'create'
  if (match?.type === 'participant') warnings.push('Déjà participant·e : reconduit·e depuis l\'onglet de la session, ou « Créer » pour une candidature liée à sa fiche')

  return { idx, data, as, remarqueAS: v.remarqueAS || null, match, errors, warnings, action }
}

async function goPreview() {
  saveMapping()
  // Candidatures existantes et participant·e·s, pour repérer les doublons.
  const [res] = await Promise.all([
    apiFetch(`${API_BASE}/api/Candidatures`),
    (store.state.user?.users || []).length ? null : store.dispatch('user/fetchUsers').catch(() => null),
  ])
  if (res?.ok) candidatures.value = await res.json()

  const rows = dataRows.value.map((cells, i) => buildRow(cells, i))
  // Doublons à l'intérieur du fichier.
  const seen = new Map()
  for (const r of rows) {
    const k = personKey(r.data.nom, r.data.prenom)
    if (!r.data.nom) continue
    if (seen.has(k)) r.warnings.push(`Doublon dans le fichier (ligne ${seen.get(k) + 1})`)
    else seen.set(k, r.idx)
  }
  allRows.value = rows
  onlyIssues.value = false
  step.value = 3
}

watch([allRows, onlyIssues], () => {
  previewRows.value = onlyIssues.value
    ? allRows.value.filter(r => r.errors.length || r.warnings.length || r.match)
    : allRows.value
}, { deep: false })

const counts = computed(() => ({
  create: allRows.value.filter(r => r.action === 'create').length,
  update: allRows.value.filter(r => r.action === 'update').length,
  skip: allRows.value.filter(r => r.action === 'skip').length,
  errors: allRows.value.filter(r => r.errors.length).length,
}))

// ─── Import ───────────────────────────────────────────────────────────────────
async function doImport() {
  importing.value = true
  importError.value = ''
  try {
    const payload = allRows.value
      .filter(r => r.action === 'create' || r.action === 'update')
      .map(r => ({
        candidatureId: r.action === 'update' ? r.match.id : null,
        data: { ...r.data, assistantSocialId: null },
        remarqueAS: r.remarqueAS,
        asEmail: r.as?.email || null,
        asPrenom: r.as?.prenom || null,
        asNom: r.as?.nom || null,
        asTelephone: r.as?.telephone || null,
      }))
    const res = await apiFetch(`${API_BASE}/api/Candidatures/import`, { method: 'POST', body: JSON.stringify(payload) })
    if (!res) return
    if (!res.ok) { importError.value = await readApiError(res, "L'import a échoué ; rien n'a été enregistré."); return }
    result.value = await res.json()
    emit('imported', result.value)
  } catch (e) {
    console.error(e)
    importError.value = "L'import a échoué ; rien n'a été enregistré."
  } finally {
    importing.value = false
  }
}

const fmtDate = d => moment(d).format('DD.MM.YYYY')

function close() {
  emit('update:modelValue', false)
}
</script>

<style scoped>
.steps { display: flex; gap: 8px; flex-wrap: wrap; }
.step { display: flex; align-items: center; gap: 8px; padding: 6px 12px; border-radius: 20px; background: #f1f4f9; color: #7a8599; font-size: .85rem; font-weight: 600; }
.step__num { width: 22px; height: 22px; border-radius: 50%; background: #d5dbe6; color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: .75rem; }
.step--active { background: #e3efff; color: #1565c0; }
.step--active .step__num { background: #1976d2; }
.step--done { color: #2e7d32; }
.step--done .step__num { background: #43a047; }
.param-label { font-size: .75rem; font-weight: 700; text-transform: uppercase; letter-spacing: .04em; color: #7a8599; }

.map-table td { vertical-align: middle; padding-top: 4px !important; padding-bottom: 4px !important; }
.samples { max-width: 380px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.row-ignored td:first-child { color: #a7b0bf; }

.preview-scroll { max-height: 52vh; overflow: auto; border: 1px solid #e4e9f2; border-radius: 8px; }
.preview-table td { vertical-align: top; padding-top: 6px !important; padding-bottom: 6px !important; }
.act-skip td { background: #fafafa; color: #9aa3b2; }
.act-update td:first-child { box-shadow: inset 3px 0 #1e88e5; }
.act-create td:first-child { box-shadow: inset 3px 0 #43a047; }
</style>
