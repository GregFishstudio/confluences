<!--
  Import en masse des réponses au questionnaire d'inscription (CSV — export Excel/Google
  Forms). Détection automatique de la correspondance colonne → champ/question, avec mapping
  manuel en secours pour chaque colonne. Met à jour les marrainages déjà importés (par nom)
  sans écraser leurs réponses existantes aux questions non couvertes par ce fichier.
-->
<template>
  <v-dialog v-model="dialog" max-width="900" @update:model-value="onOpen">
    <template #activator="{ props }">
      <v-btn v-bind="props" variant="outlined" prepend-icon="mdi-clipboard-text-multiple-outline">
        Importer les réponses (CSV)
      </v-btn>
    </template>

    <v-card rounded="lg">
      <v-card-title class="py-4">Importer les réponses au questionnaire</v-card-title>
      <v-divider />
      <v-card-text class="pt-6">
        <v-file-input
          v-model="file"
          label="Fichier CSV"
          variant="outlined"
          accept=".csv,text/csv"
          hide-details="auto"
          @update:model-value="onFileChange"
        />

        <div v-if="error" class="text-error text-body-2 mt-3">{{ error }}</div>

        <template v-if="rawRows.length">
          <div class="text-body-2 text-medium-emphasis mt-4 mb-2">
            Correspondance des colonnes — corrige si une colonne s'est mal alignée :
          </div>
          <v-row dense class="mb-2">
            <v-col v-for="(header, idx) in rawHeaderRow" :key="idx" cols="6" md="4">
              <v-select
                :model-value="columnTargets[idx] || null"
                @update:model-value="(v) => setTarget(idx, v)"
                :items="targetOptions"
                item-title="title"
                item-value="value"
                :label="header || `(colonne ${idx + 1})`"
                variant="outlined" density="compact" clearable hide-details
              />
            </v-col>
          </v-row>

          <div class="text-body-2 text-medium-emphasis mt-4 mb-2">
            {{ rows.length }} ligne(s) détectée(s). Aperçu des 3 premières :
          </div>
          <div class="table-scroll">
            <table class="preview-table">
              <thead>
                <tr>
                  <th v-for="idx in mappedColumnIndexes" :key="idx">{{ targetLabel(columnTargets[idx]) }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(row, i) in rawDataRows.slice(0, 3)" :key="i">
                  <td v-for="idx in mappedColumnIndexes" :key="idx">{{ row[idx] }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
        <v-btn color="primary" :disabled="!rows.length" :loading="importing" @click="confirmImport">
          Importer {{ rows.length }} réponse(s)
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useStore } from 'vuex'
import MarrainageQuestionService from '@/services/marrainageQuestionService.js'
import MarrainageAnswerService from '@/services/marrainageAnswerService.js'

const store = useStore()
const emit = defineEmits(['imported'])

const dialog = ref(false)
const file = ref(null)
const error = ref('')
const importing = ref(false)

const questions = ref([])
const rawRows = ref([])
const rawHeaderRow = ref([])
const headerRowIndex = ref(0)
const columnTargets = ref({}) // { [colIndex]: 'core:prenom' | 'question:12' }

const rawDataRows = computed(() => rawRows.value.slice(headerRowIndex.value + 1))
const mappedColumnIndexes = computed(() =>
  Object.keys(columnTargets.value).map(Number).filter(idx => columnTargets.value[idx])
)

const CORE_FIELDS = [
  { value: 'core:prenom', label: 'Prénom' },
  { value: 'core:nom', label: 'Nom' },
  { value: 'core:nomComplet', label: 'Nom complet (séparé automatiquement)' },
  { value: 'core:numero', label: 'Téléphone' },
  { value: 'core:email', label: 'Email' },
  { value: 'core:commune', label: 'Commune' },
  { value: 'core:date', label: 'Date de démarrage' },
  { value: 'core:periodeDemande', label: 'Mois/année de demande' },
]

const targetOptions = computed(() => [
  ...CORE_FIELDS.map(f => ({ title: `[Champ] ${f.label}`, value: f.value })),
  ...questions.value.map(q => ({ title: q.label, value: `question:${q.marrainageQuestionId}` })),
])

function targetLabel(target) {
  if (!target) return ''
  const core = CORE_FIELDS.find(f => f.value === target)
  if (core) return `[Champ] ${core.label}`
  const qid = Number(target.split(':')[1])
  return questions.value.find(q => q.marrainageQuestionId === qid)?.label || target
}

function setTarget(idx, value) {
  columnTargets.value = { ...columnTargets.value, [idx]: value }
}

function onOpen(isOpen) {
  if (isOpen) {
    MarrainageQuestionService.getQuestions(false).then(response => { questions.value = response.data })
  }
}

function normalize(str) {
  return (str || '')
    .toString()
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/[^a-z0-9]+/g, ' ')
    .trim()
}

const CORE_ALIASES = {
  'core:prenom': ['prenom'],
  'core:nom': ['nom'],
  'core:nomComplet': ['nom et prenom', 'nom prenom', 'nom complet'],
  'core:numero': ['telephone', 'numero de telephone', 'no de telephone', 'tel'],
  'core:email': ['email', 'adresse e mail', 'e mail'],
  'core:commune': ['commune', 'commune de residence', 'commune de domicile'],
  'core:date': ['date de demarrage', 'date'],
  'core:periodeDemande': ['mois annee de demande', 'colonne 1'],
}

function autoMatchColumn(header) {
  const h = normalize(header)
  if (!h) return null

  for (const [target, aliases] of Object.entries(CORE_ALIASES)) {
    if (aliases.includes(h)) return target
  }

  // Correspondance exacte avec une question, sinon inclusion partielle (dans un sens ou l'autre).
  const exact = questions.value.find(q => normalize(q.label) === h)
  if (exact) return `question:${exact.marrainageQuestionId}`

  const partial = questions.value.find(q => {
    const ql = normalize(q.label)
    return ql.length > 3 && (h.includes(ql) || ql.includes(h))
  })
  return partial ? `question:${partial.marrainageQuestionId}` : null
}

function detectDelimiter(text) {
  const firstLine = text.split(/\r?\n/, 1)[0] || ''
  const commas = (firstLine.match(/,/g) || []).length
  const semicolons = (firstLine.match(/;/g) || []).length
  return semicolons > commas ? ';' : ','
}

function parseCsv(text, delimiter) {
  const rows = []
  let row = []
  let field = ''
  let inQuotes = false

  for (let i = 0; i < text.length; i++) {
    const c = text[i]
    if (inQuotes) {
      if (c === '"') {
        if (text[i + 1] === '"') { field += '"'; i++ } else { inQuotes = false }
      } else {
        field += c
      }
    } else if (c === '"') {
      inQuotes = true
    } else if (c === delimiter) {
      row.push(field); field = ''
    } else if (c === '\n' || c === '\r') {
      if (c === '\r' && text[i + 1] === '\n') i++
      row.push(field); field = ''
      rows.push(row); row = []
    } else {
      field += c
    }
  }
  if (field.length || row.length) { row.push(field); rows.push(row) }
  return rows.filter(r => r.some(cell => cell.trim() !== ''))
}

function findHeaderRow(raw) {
  let headerRowIdx = 0
  let bestMatchCount = -1
  for (let i = 0; i < Math.min(5, raw.length); i++) {
    const matchCount = raw[i].filter(h => autoMatchColumn(h)).length
    if (matchCount > bestMatchCount) {
      bestMatchCount = matchCount
      headerRowIdx = i
    }
  }
  return { headerRowIdx, bestMatchCount }
}

function onFileChange() {
  error.value = ''
  rawRows.value = []
  rawHeaderRow.value = []
  columnTargets.value = {}
  if (!file.value) return

  const reader = new FileReader()
  reader.onload = () => {
    try {
      const buffer = reader.result
      const candidates = ['utf-8', 'windows-1252', 'macintosh'].map(encoding => {
        const text = new TextDecoder(encoding).decode(buffer)
        const delimiter = detectDelimiter(text)
        const raw = parseCsv(text, delimiter)
        const { headerRowIdx, bestMatchCount } = findHeaderRow(raw)
        return { raw, headerRowIdx, bestMatchCount }
      })
      const best = candidates.sort((a, b) => b.bestMatchCount - a.bestMatchCount)[0]

      if (best.raw.length < 2) { error.value = 'Le fichier semble vide.'; return }

      rawRows.value = best.raw
      rawHeaderRow.value = best.raw[best.headerRowIdx]
      headerRowIndex.value = best.headerRowIdx

      const detected = {}
      rawHeaderRow.value.forEach((header, idx) => {
        const match = autoMatchColumn(header)
        if (match) detected[idx] = match
      })
      columnTargets.value = detected
    } catch (e) {
      error.value = 'Impossible de lire ce fichier : ' + e.message
    }
  }
  reader.readAsArrayBuffer(file.value)
}

const rows = computed(() => {
  if (!rawDataRows.value.length) return []

  return rawDataRows.value.map(cells => {
    const core = { actif: true }
    const answers = []

    Object.entries(columnTargets.value).forEach(([idxStr, target]) => {
      if (!target) return
      const value = (cells[Number(idxStr)] || '').trim()
      if (!value) return

      if (target === 'core:nomComplet') {
        const parts = value.split(/\s+/)
        if (parts.length > 1) {
          core.prenom = parts[0]
          core.nom = parts.slice(1).join(' ')
        } else {
          core.nom = value
        }
      } else if (target.startsWith('core:')) {
        core[target.slice(5)] = value
      } else if (target.startsWith('question:')) {
        answers.push({ questionId: Number(target.split(':')[1]), value })
      }
    })

    return { core, answers }
  }).filter(r => r.core.nom)
})

async function confirmImport() {
  importing.value = true
  try {
    const marrainagesPayload = rows.value.map(r => r.core)
    const created = await store.dispatch('marrainage/importMarrainages', marrainagesPayload)

    for (let i = 0; i < created.length; i++) {
      const marrainageId = created[i].marrainageId
      const newAnswers = rows.value[i].answers
      if (!newAnswers.length) continue

      // Fusionne avec les réponses déjà enregistrées pour ne pas écraser les questions
      // non couvertes par ce fichier (l'endpoint de sauvegarde remplace tout le lot envoyé).
      const existingResponse = await MarrainageAnswerService.getAnswers(marrainageId)
      const merged = new Map(existingResponse.data.map(a => [a.marrainageQuestionId, a.value]))
      newAnswers.forEach(a => merged.set(a.questionId, a.value))

      await MarrainageAnswerService.saveAnswers(
        marrainageId,
        Array.from(merged.entries()).map(([questionId, value]) => ({ questionId, value }))
      )
    }

    dialog.value = false
    file.value = null
    rawRows.value = []
    rawHeaderRow.value = []
    columnTargets.value = {}
    emit('imported')
  } finally {
    importing.value = false
  }
}
</script>

<style scoped>
.table-scroll { max-height: 260px; overflow: auto; }
.preview-table { border-collapse: collapse; width: 100%; font-size: 0.8rem; }
.preview-table th, .preview-table td {
  border: 1px solid rgba(0, 0, 0, 0.12);
  padding: 4px 8px;
  text-align: left;
  white-space: nowrap;
  max-width: 220px;
  overflow: hidden;
  text-overflow: ellipsis;
}
.preview-table th { background: rgba(0, 0, 0, 0.04); }
</style>
