<!--
  Import en masse de marrainages depuis un fichier CSV (export Excel/Google Sheets).
  Les colonnes sont reconnues par intitulé (voir COLUMN_ALIASES) ; tout ce qui ne
  correspond pas à un champ connu est simplement ignoré (ex: "Total").
-->
<template>
  <v-dialog v-model="dialog" max-width="900">
    <template #activator="{ props }">
      <v-btn v-bind="props" variant="outlined" prepend-icon="mdi-file-upload-outline">
        Importer CSV
      </v-btn>
    </template>

    <v-card rounded="lg">
      <v-card-title class="py-4">Importer des marrainages depuis un CSV</v-card-title>
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
            <v-col v-for="field in mappedFields" :key="field.key" cols="6" md="4">
              <v-select
                :model-value="columnIndex[field.key] ?? null"
                @update:model-value="(v) => setColumnIndex(field.key, v)"
                :items="headerOptions"
                item-title="title"
                item-value="value"
                :label="field.label"
                variant="outlined" density="compact" clearable hide-details
              />
            </v-col>
          </v-row>

          <div class="text-body-2 text-medium-emphasis mt-4 mb-2">
            {{ rows.length }} ligne(s) détectée(s). Aperçu des 5 premières :
          </div>
          <div class="table-scroll">
            <table class="preview-table">
              <thead>
                <tr>
                  <th v-for="field in mappedFields" :key="field.key">{{ field.label }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(row, i) in rows.slice(0, 5)" :key="i">
                  <td v-for="field in mappedFields" :key="field.key">{{ row[field.key] }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="text-caption text-grey mt-2">
            Le prénom et le nom sont séparés automatiquement au premier espace — à corriger manuellement
            fiche par fiche si besoin. Le lien avec un·e participant·e n'est pas automatique, à faire
            ensuite depuis chaque fiche.
          </div>
        </template>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
        <v-btn color="primary" :disabled="!rows.length" :loading="importing" @click="confirmImport">
          Importer {{ rows.length }} marrainage(s)
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useStore } from 'vuex'

const store = useStore()
const emit = defineEmits(['imported'])

const dialog = ref(false)
const file = ref(null)
const error = ref('')
const importing = ref(false)

// État brut du fichier chargé, conservé pour permettre de corriger le mapping des colonnes.
const rawRows = ref([])
const rawHeaderRow = ref([])
const headerRowIndex = ref(0)
const columnIndex = ref({})

const headerOptions = computed(() =>
  rawHeaderRow.value.map((label, i) => ({ title: label || `(colonne ${i + 1})`, value: i }))
)

function setColumnIndex(field, value) {
  if (value === null || value === undefined) {
    delete columnIndex.value[field]
  } else {
    columnIndex.value[field] = value
  }
  columnIndex.value = { ...columnIndex.value }
}

const mappedFields = [
  { key: 'periodeDemande', label: 'Mois/année de demande' },
  { key: 'prenom', label: 'Prénom' },
  { key: 'nom', label: 'Nom' },
  { key: 'numero', label: 'Téléphone Parrain/Marraine' },
  { key: 'remarqueParrain', label: 'Remarque Parrain/Marraine' },
  { key: 'commune', label: 'Commune de domicile' },
  { key: 'email', label: 'Email' },
  { key: 'filleulNom', label: 'Filleul·e' },
  { key: 'telephoneFilleul', label: 'Téléphone Filleul·e' },
  { key: 'emailFilleul', label: 'Email Filleul·e' },
  { key: 'suivi', label: 'Suivi' },
  { key: 'extraitCasier', label: 'Extrait du casier' },
  { key: 'charteSignee', label: 'Charte signée' },
  { key: 'actif', label: 'Actif' },
  { key: 'relanceNotes', label: 'Relancer prochaine session' },
]

// Intitulés possibles (normalisés : minuscules, sans accents) pour chaque champ.
const COLUMN_ALIASES = {
  periodeDemande: ['mois/annee de demande', 'mois annee de demande', 'colonne 1 (mois/annee de demande)', 'colonne 1'],
  prenom: ['prenom'],
  nom: ['parrain/marraine', 'parrain marraine', 'nom'],
  numero: ['telephone parrain/marraine', 'telephone parrain marraine', 'numero'],
  actif: ['actif'],
  remarqueParrain: ['remarque parrain/marraine', 'remarque parrain marraine'],
  commune: ['commune de domicile', 'commune'],
  email: ['email', 'e-mail'],
  filleulNom: ['filleul.e', 'filleule', 'filleul'],
  telephoneFilleul: ['telephone filleul.e', 'telephone filleule', 'telephone filleul'],
  emailFilleul: ['email filleul.e', 'email filleule', 'email filleul'],
  suivi: ['suivi'],
  extraitCasier: ['extrait du casier', 'extrait casier'],
  charteSignee: ['charte signee', 'charte signée'],
  relanceNotes: ['relancer prochaine session', 'relance'],
}

const DIACRITICS_REGEX = new RegExp('[̀-ͯ]', 'g')

function normalize(str) {
  return (str || '')
    .toString()
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(DIACRITICS_REGEX, '')
}

// Détecte le séparateur (Excel FR exporte souvent en ";" plutôt qu'en ",").
function detectDelimiter(text) {
  const firstLine = text.split(/\r?\n/, 1)[0] || ''
  const commas = (firstLine.match(/,/g) || []).length
  const semicolons = (firstLine.match(/;/g) || []).length
  return semicolons > commas ? ';' : ','
}

// Parseur CSV simple gérant les champs entre guillemets (séparateurs/retours à la ligne inclus).
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

// Cherche, parmi les 5 premières lignes, celle qui matche le plus d'alias connus.
function findHeaderRow(raw) {
  let headerRowIndex = 0
  let bestMatchCount = -1
  for (let i = 0; i < Math.min(5, raw.length); i++) {
    const normalized = raw[i].map(normalize)
    const matchCount = Object.values(COLUMN_ALIASES)
      .filter(aliases => normalized.some(h => aliases.includes(h))).length
    if (matchCount > bestMatchCount) {
      bestMatchCount = matchCount
      headerRowIndex = i
    }
  }
  return { headerRowIndex, bestMatchCount }
}

function truthy(val) {
  const v = normalize(val)
  return v === 'true' || v === 'oui' || v === 'x' || v === '1'
}

// Excel convertit souvent les colonnes "téléphone" en nombre : le "+" saute, et les
// numéros longs passent parfois en notation scientifique (ex: "3.8095E+11").
// On reconstitue le format "+41..." quand c'est possible.
function fixPhoneNumber(val) {
  if (!val) return val
  let v = val.trim()
  if (v.startsWith('+')) return v

  // Notation scientifique Excel (ex: "3,8095E+11" ou "3.8095E+11") : on la reconvertit
  // en nombre entier — la précision au-delà de ~15 chiffres est perdue par Excel lui-même.
  if (/^[\d.,]+E\+?\d+$/i.test(v)) {
    const asNumber = Number(v.replace(',', '.'))
    if (!Number.isNaN(asNumber)) v = Math.round(asNumber).toString()
  }

  // Retire un éventuel ".0" laissé par l'export d'une colonne numérique.
  v = v.replace(/\.0$/, '')

  if (/^41\d{9}$/.test(v)) return '+' + v
  if (/^0\d{9}$/.test(v)) return '+41' + v.slice(1)
  return val
}

function onFileChange() {
  error.value = ''
  rawRows.value = []
  rawHeaderRow.value = []
  columnIndex.value = {}
  if (!file.value) return

  const reader = new FileReader()
  reader.onload = () => {
    try {
      const buffer = reader.result

      // Excel (surtout en français) exporte souvent le CSV en Windows-1252 avec un
      // séparateur ";" plutôt qu'en UTF-8/",". Excel pour Mac utilise parfois Mac Roman.
      // On essaie les trois décodages et on garde celui qui reconnaît le plus de colonnes.
      const candidates = ['utf-8', 'windows-1252', 'macintosh'].map(encoding => {
        const text = new TextDecoder(encoding).decode(buffer)
        const delimiter = detectDelimiter(text)
        const raw = parseCsv(text, delimiter)
        const { headerRowIndex: hIdx, bestMatchCount } = findHeaderRow(raw)
        return { raw, headerRowIndex: hIdx, bestMatchCount }
      })

      const best = candidates.sort((a, b) => b.bestMatchCount - a.bestMatchCount)[0]

      if (best.raw.length < 2) { error.value = 'Le fichier semble vide.'; return }
      if (best.bestMatchCount <= 0) {
        error.value = "Aucune colonne connue n'a été reconnue dans ce fichier. Utilise la correspondance manuelle ci-dessous."
      }

      const headerRow = best.raw[best.headerRowIndex].map(normalize)
      const detected = {}
      Object.entries(COLUMN_ALIASES).forEach(([field, aliases]) => {
        const idx = headerRow.findIndex(h => aliases.includes(h))
        if (idx !== -1) detected[field] = idx
      })

      rawRows.value = best.raw
      rawHeaderRow.value = best.raw[best.headerRowIndex]
      headerRowIndex.value = best.headerRowIndex
      columnIndex.value = detected
    } catch (e) {
      error.value = 'Impossible de lire ce fichier : ' + e.message
    }
  }
  reader.readAsArrayBuffer(file.value)
}

const rows = computed(() => {
  if (!rawRows.value.length) return []

  const hasPrenomColumn = columnIndex.value.prenom !== undefined
  const hasActifColumn = columnIndex.value.actif !== undefined

  return rawRows.value.slice(headerRowIndex.value + 1)
    .map(cells => {
      const entry = { actif: true }
      Object.entries(columnIndex.value).forEach(([field, idx]) => {
        entry[field] = (cells[idx] || '').trim()
      })
      entry.charteSignee = truthy(entry.charteSignee)
      entry.actif = hasActifColumn ? truthy(entry.actif) : true
      if (entry.numero) entry.numero = fixPhoneNumber(entry.numero)
      if (entry.telephoneFilleul) entry.telephoneFilleul = fixPhoneNumber(entry.telephoneFilleul)
      // Si aucune colonne "Prénom" dédiée n'est fournie, on suppose que "Nom" contient
      // "Prénom Nom" en un seul texte et on sépare au premier espace (meilleur effort).
      if (!hasPrenomColumn) {
        const nameParts = (entry.nom || '').trim().split(/\s+/)
        if (nameParts.length > 1) {
          entry.prenom = nameParts[0]
          entry.nom = nameParts.slice(1).join(' ')
        } else {
          entry.prenom = ''
        }
      }
      return entry
    })
    // Ignore les lignes où aucune des colonnes reconnues n'a de contenu
    // (lignes vides de fin de feuille Google Sheets/Excel, etc.)
    .filter(entry => Object.keys(columnIndex.value).some(field => entry[field]))
})

async function confirmImport() {
  importing.value = true
  try {
    await store.dispatch('marrainage/importMarrainages', rows.value)
    dialog.value = false
    file.value = null
    rawRows.value = []
    rawHeaderRow.value = []
    columnIndex.value = {}
    emit('imported')
  } finally {
    importing.value = false
  }
}
</script>

<style scoped>
.table-scroll { max-height: 320px; overflow: auto; }
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
