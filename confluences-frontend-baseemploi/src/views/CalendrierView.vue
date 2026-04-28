<template>
  <v-container fluid class="pa-6">

    <!-- Header -->
    <div class="d-flex align-center mb-6">
      <div>
        <div class="text-h5 font-weight-bold text-primary">Calendrier de formation</div>
        <div class="text-caption text-grey">Gestion des sessions, jours fériés et périodes de vacances</div>
      </div>
      <v-spacer />
      <!-- Indicateur auto-save -->
      <v-fade-transition>
        <v-chip v-if="saveStatus === 'saving'" color="orange" variant="tonal" size="small" class="mr-3">
          <v-progress-circular size="12" width="2" indeterminate class="mr-1" />
          Enregistrement…
        </v-chip>
        <v-chip v-else-if="saveStatus === 'saved'" color="success" variant="tonal" size="small" class="mr-3">
          <v-icon start size="small">mdi-check-circle</v-icon>
          Enregistré
        </v-chip>
        <v-chip v-else-if="saveStatus === 'error'" color="error" variant="tonal" size="small" class="mr-3">
          <v-icon start size="small">mdi-alert-circle</v-icon>
          Erreur
        </v-chip>
      </v-fade-transition>
    </div>

    <v-row>
      <!-- ════════════════════════════════════════════
           COL GAUCHE : Sessions
      ════════════════════════════════════════════ -->
      <v-col cols="12" lg="7">
        <v-card class="elevation-3" rounded="lg">
          <v-card-title class="bg-primary text-white pa-4">
            <v-icon start color="white">mdi-calendar-clock</v-icon>
            Sessions / Trimestres
          </v-card-title>

          <v-card-text class="pa-4">

            <!-- Alertes cibles -->
            <v-row class="mb-4">
              <v-col cols="12" sm="6">
                <v-alert type="info" variant="tonal" density="compact" icon="mdi-school">
                  <strong>F&amp;I (60j/session)</strong><br>
                  Mois 1 = 20j · Mois 2 = 40j · Mois 3 = 60j
                </v-alert>
              </v-col>
              <v-col cols="12" sm="6">
                <v-alert type="success" variant="tonal" density="compact" icon="mdi-monitor">
                  <strong>Télécours (32j/session)</strong><br>
                  Mois 1 = 10j · Mois 2 = 20j · Mois 3 = 32j
                </v-alert>
              </v-col>
            </v-row>

            <!-- Onglets -->
            <v-tabs v-model="sessionTab" color="primary" class="mb-4">
              <v-tab value="fi_a1">
                <v-icon start size="small">mdi-school</v-icon>F&amp;I A1
                <v-chip x-small class="ml-1" color="blue" variant="tonal">{{ sessionsByType('fi_a1').length }}</v-chip>
              </v-tab>
              <v-tab value="fi_a2">
                <v-icon start size="small">mdi-school-outline</v-icon>F&amp;I A2
                <v-chip x-small class="ml-1" color="blue" variant="tonal">{{ sessionsByType('fi_a2').length }}</v-chip>
              </v-tab>
              <v-tab value="telecours">
                <v-icon start size="small">mdi-monitor</v-icon>Télécours
                <v-chip x-small class="ml-1" color="teal" variant="tonal">{{ sessionsByType('telecours').length }}</v-chip>
              </v-tab>
            </v-tabs>

            <!-- Formulaire nouvelle/édition session -->
            <v-card variant="outlined" class="pa-4 mb-4 form-card">
              <div class="d-flex align-center mb-3">
                <v-icon size="small" class="mr-2" :color="editingIndex !== null ? 'orange' : 'primary'">
                  {{ editingIndex !== null ? 'mdi-pencil' : 'mdi-plus-circle' }}
                </v-icon>
                <span class="text-subtitle-2 font-weight-bold">
                  {{ editingIndex !== null ? 'Modifier la session' : `Nouvelle session — ${sessionTabLabel}` }}
                </span>
              </div>

              <v-row dense>
                <v-col cols="12" sm="5">
                  <v-text-field
                    v-model="form.name"
                    :label="`Nom (ex: S1-2025-${sessionTab.toUpperCase()})`"
                    variant="outlined" density="compact" hide-details
                  />
                </v-col>
                <v-col cols="6" sm="3">
                  <v-text-field v-model="form.startDate" label="Début" type="date" variant="outlined" density="compact" hide-details />
                </v-col>
                <v-col cols="6" sm="4">
                  <v-text-field v-model="form.endDate" label="Fin" type="date" variant="outlined" density="compact" hide-details />
                </v-col>
              </v-row>

              <div class="text-caption text-grey mt-3 mb-1">Vacances inter-session (optionnel) :</div>
              <v-row dense>
                <v-col cols="6" sm="3">
                  <v-text-field v-model="form.vacanceStart" label="Début vacances" type="date" variant="outlined" density="compact" hide-details />
                </v-col>
                <v-col cols="6" sm="3">
                  <v-text-field v-model="form.vacanceEnd" label="Fin vacances" type="date" variant="outlined" density="compact" hide-details />
                </v-col>
                <v-col cols="12" sm="6" class="d-flex align-center gap-2">
                  <v-btn
                    :color="editingIndex !== null ? 'orange' : 'primary'"
                    :loading="isSavingSession"
                    @click="submitForm"
                    :disabled="!form.name || !form.startDate"
                  >
                    <v-icon start>{{ editingIndex !== null ? 'mdi-check' : 'mdi-plus' }}</v-icon>
                    {{ editingIndex !== null ? 'Mettre à jour' : 'Ajouter' }}
                  </v-btn>
                  <v-btn v-if="editingIndex !== null" variant="text" @click="cancelEdit">Annuler</v-btn>
                </v-col>
              </v-row>
            </v-card>

            <!-- Tableau sessions -->
            <v-window v-model="sessionTab">
              <v-window-item v-for="tabVal in ['fi_a1','fi_a2','telecours']" :key="tabVal" :value="tabVal">
                <div v-if="sessionsByType(tabVal).length === 0" class="text-center pa-8 text-grey">
                  <v-icon size="40" color="grey-lighten-2">mdi-calendar-remove</v-icon>
                  <div class="text-caption mt-2">Aucune session définie pour {{ tabLabels[tabVal] }}</div>
                </div>
                <div v-else class="sessions-list">
                  <div
                    v-for="sess in sessionsByType(tabVal)"
                    :key="sess.id || sess._localId"
                    class="session-row"
                    :class="{ 'session-row--editing': editingIndex === sess._localId }"
                  >
                    <div class="session-row__main">
                      <div class="session-name">{{ sess.name }}</div>
                      <div class="session-dates text-caption text-grey">
                        <v-icon size="x-small" class="mr-1">mdi-calendar-start</v-icon>
                        {{ fmtDate(sess.startDate) }}
                        <span class="mx-1">→</span>
                        <v-icon size="x-small" class="mr-1">mdi-calendar-end</v-icon>
                        {{ fmtDate(sess.endDate) }}
                      </div>
                      <div v-if="sess.vacanceStart" class="session-vacances text-caption text-green-darken-2 mt-1">
                        <v-icon size="x-small" class="mr-1">mdi-beach</v-icon>
                        Vacances : {{ fmtDate(sess.vacanceStart) }} → {{ fmtDate(sess.vacanceEnd) }}
                      </div>
                    </div>
                    <div class="session-row__right d-flex align-center gap-2">
                      <v-chip size="x-small" :color="tabVal === 'telecours' ? 'teal' : 'blue'" variant="tonal">
                        {{ tabVal === 'telecours' ? '10/20/32j' : '20/40/60j' }}
                      </v-chip>
                      <v-btn icon size="x-small" variant="text" @click="startEdit(sess)">
                        <v-icon size="small">mdi-pencil-outline</v-icon>
                      </v-btn>
                      <v-btn icon size="x-small" variant="text" color="error" @click="deleteSession(sess)">
                        <v-icon size="small">mdi-delete-outline</v-icon>
                      </v-btn>
                    </div>
                  </div>
                </div>
              </v-window-item>
            </v-window>

          </v-card-text>
        </v-card>
      </v-col>

      <!-- ════════════════════════════════════════════
           COL DROITE : Fériés + Vacances
      ════════════════════════════════════════════ -->
      <v-col cols="12" lg="5">

        <!-- Jours fériés -->
        <v-card class="elevation-3 mb-4" rounded="lg">
          <v-card-title class="bg-amber-darken-3 text-white pa-4">
            <v-icon start color="white">mdi-calendar-star</v-icon>
            Jours fériés
            <v-chip size="x-small" variant="tonal" color="white" class="ml-2">{{ joursFeries.length }}</v-chip>
          </v-card-title>
          <v-card-text class="pa-4">
            <v-date-picker
              v-model="joursFeries"
              multiple width="100%"
              color="amber-darken-3"
              :first-day-of-week="1"
              class="elevation-0 mb-3"
              @update:modelValue="saveFeries"
            />
            <div class="d-flex flex-wrap gap-1">
              <v-chip
                v-for="d in sortedFeries" :key="d.raw"
                color="amber-darken-3" variant="tonal" size="small"
                closable @click:close="removeFerie(d.raw)"
              >
                {{ d.display }}
              </v-chip>
              <span v-if="!joursFeries.length" class="text-grey text-caption">
                Aucun jour férié sélectionné.
              </span>
            </div>
          </v-card-text>
        </v-card>

        <!-- Vacances générales -->
        <v-card class="elevation-3" rounded="lg">
          <v-card-title class="bg-green-darken-2 text-white pa-4">
            <v-icon start color="white">mdi-beach</v-icon>
            Périodes de vacances générales
            <v-chip size="x-small" variant="tonal" color="white" class="ml-2">{{ periodesVacances.length }}</v-chip>
          </v-card-title>
          <v-card-text class="pa-4">
            <v-alert type="info" variant="tonal" density="compact" class="mb-4">
              Ces vacances s'appliquent à tous les groupes. Les vacances propres à une session se définissent dans la session.
            </v-alert>

            <!-- Formulaire vacance -->
            <v-card variant="outlined" class="pa-3 mb-3 form-card">
              <div class="text-caption font-weight-bold mb-2 text-green-darken-2">
                <v-icon size="x-small" class="mr-1">mdi-plus-circle</v-icon>Ajouter une période
              </div>
              <v-row dense>
                <v-col cols="12">
                  <v-text-field
                    v-model="newVacance.label"
                    label="Libellé (ex: Noël 2025)"
                    variant="outlined" density="compact" hide-details class="mb-2"
                  />
                </v-col>
                <v-col cols="5">
                  <v-text-field v-model="newVacance.startDate" label="Début" type="date" variant="outlined" density="compact" hide-details />
                </v-col>
                <v-col cols="5">
                  <v-text-field v-model="newVacance.endDate" label="Fin" type="date" variant="outlined" density="compact" hide-details />
                </v-col>
                <v-col cols="2" class="d-flex align-center justify-center">
                  <v-btn
                    icon color="green-darken-2" size="small"
                    :disabled="!newVacance.startDate || !newVacance.endDate"
                    :loading="isSavingVacance"
                    @click="addVacance"
                  >
                    <v-icon>mdi-plus</v-icon>
                  </v-btn>
                </v-col>
              </v-row>
            </v-card>

            <!-- Liste vacances -->
            <div v-if="!periodesVacances.length" class="text-center pa-4 text-grey">
              <v-icon size="32" color="grey-lighten-2">mdi-beach</v-icon>
              <div class="text-caption mt-1">Aucune période définie</div>
            </div>
            <div v-else class="sessions-list">
              <div v-for="(v, i) in periodesVacances" :key="i" class="session-row">
                <div class="session-row__main">
                  <div class="session-name">{{ v.label || 'Vacances' }}</div>
                  <div class="session-dates text-caption text-grey">
                    {{ fmtDate(v.startDate) }} → {{ fmtDate(v.endDate) }}
                  </div>
                </div>
                <v-btn icon size="x-small" variant="text" color="error" @click="deleteVacance(i)">
                  <v-icon size="small">mdi-delete-outline</v-icon>
                </v-btn>
              </div>
            </div>
          </v-card-text>
        </v-card>

      </v-col>
    </v-row>

    <!-- Snackbar -->
    <v-snackbar v-model="snackbar.visible" :color="snackbar.color" timeout="2500" location="bottom right">
      <v-icon start>{{ snackbar.color === 'success' ? 'mdi-check-circle' : 'mdi-alert-circle' }}</v-icon>
      {{ snackbar.text }}
    </v-snackbar>

  </v-container>
</template>

<script setup>
import { ref, computed } from 'vue'
import { onMounted } from 'vue'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'

moment.locale('fr')

// ─── State ────────────────────────────────────────────────────────────────────
const saveStatus       = ref('')   // 'saving' | 'saved' | 'error' | ''
const isSavingSession  = ref(false)
const isSavingVacance  = ref(false)
const sessions         = ref([])
const joursFeries      = ref([])
const periodesVacances = ref([])
const sessionTab       = ref('fi_a1')
const editingIndex     = ref(null)
const form             = ref(emptyForm())
const newVacance       = ref({ label: '', startDate: '', endDate: '' })
const snackbar         = ref({ visible: false, text: '', color: 'success' })
let   _localCounter    = 0
let   saveStatusTimer  = null

const tabLabels      = { fi_a1: 'F&I A1', fi_a2: 'F&I A2', telecours: 'Télécours' }
const sessionTabLabel = computed(() => tabLabels[sessionTab.value] || '')

// ─── Computed ─────────────────────────────────────────────────────────────────
const sortedFeries = computed(() =>
  [...joursFeries.value]
    .map(d => ({ raw: d, display: moment(d).format('DD MMM YYYY') }))
    .sort((a, b) => new Date(a.raw) - new Date(b.raw))
)

function sessionsByType(type) {
  return sessions.value.filter(s => s.type === type)
}

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadSessions(), loadJoursFeries(), loadVacances()])
})

// ─── Helpers UI ───────────────────────────────────────────────────────────────
function showSaveStatus(status) {
  saveStatus.value = status
  clearTimeout(saveStatusTimer)
  if (status === 'saved') {
    saveStatusTimer = setTimeout(() => { saveStatus.value = '' }, 2500)
  }
}

function toast(text, color = 'success') {
  snackbar.value = { visible: true, text, color }
}

function authHeaders() {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${localStorage.getItem('access_token')}`,
  }
}

// ─── API Load ─────────────────────────────────────────────────────────────────
async function loadSessions() {
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/sessions`)
    if (res?.ok) {
      const data = await res.json()
      sessions.value = data.map(s => ({ ...s, _localId: ++_localCounter }))
    }
  } catch (e) { console.error('loadSessions:', e) }
}

async function loadJoursFeries() {
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/feries`)
    if (res?.ok) {
      const data = await res.json()
      joursFeries.value = data.map(f => f.date.substring(0, 10))
    }
  } catch (e) { console.error('loadJoursFeries:', e) }
}

async function loadVacances() {
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/vacances`)
    if (res?.ok) periodesVacances.value = await res.json()
  } catch (e) { console.error('loadVacances:', e) }
}

// ─── Sessions — auto-save ──────────────────────────────────────────────────────
function emptyForm() {
  return { name: '', startDate: '', endDate: '', vacanceStart: '', vacanceEnd: '' }
}

async function submitForm() {
  const sess = { ...form.value, type: sessionTab.value }
  if (editingIndex.value !== null) {
    const idx = sessions.value.findIndex(s => s._localId === editingIndex.value)
    if (idx >= 0) sessions.value[idx] = { ...sessions.value[idx], ...sess }
    editingIndex.value = null
  } else {
    sessions.value.push({ ...sess, _localId: ++_localCounter })
  }
  form.value = emptyForm()
  await saveSessions()
}

function startEdit(sess) {
  editingIndex.value = sess._localId
  form.value = {
    name:         sess.name,
    startDate:    sess.startDate    ? sess.startDate.substring(0, 10)    : '',
    endDate:      sess.endDate      ? sess.endDate.substring(0, 10)      : '',
    vacanceStart: sess.vacanceStart ? sess.vacanceStart.substring(0, 10) : '',
    vacanceEnd:   sess.vacanceEnd   ? sess.vacanceEnd.substring(0, 10)   : '',
  }
}

function cancelEdit() {
  editingIndex.value = null
  form.value = emptyForm()
}

async function deleteSession(sess) {
  sessions.value = sessions.value.filter(s => s._localId !== sess._localId)
  await saveSessions()
}

async function saveSessions() {
  isSavingSession.value = true
  showSaveStatus('saving')
  try {
    const payload = sessions.value.map(({ _localId, ...rest }) => rest)
    const res = await apiFetch(`${API_BASE}/api/calendrier/sessions`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
    if (res?.ok) {
      showSaveStatus('saved')
      toast('Sessions enregistrées ✓')
      await loadSessions()
    } else {
      showSaveStatus('error')
      toast('Erreur lors de l\'enregistrement', 'error')
    }
  } catch (e) {
    console.error(e)
    showSaveStatus('error')
    toast('Erreur serveur', 'error')
  } finally {
    isSavingSession.value = false
  }
}

// ─── Fériés — auto-save ───────────────────────────────────────────────────────
async function saveFeries() {
  showSaveStatus('saving')
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/feries`, {
      method: 'PUT',
      body: JSON.stringify(joursFeries.value),
    })
    if (res?.ok) { showSaveStatus('saved'); toast('Jours fériés enregistrés ✓') }
    else         { showSaveStatus('error'); toast('Erreur lors de l\'enregistrement', 'error') }
  } catch (e) { showSaveStatus('error'); console.error(e) }
}

function removeFerie(raw) {
  joursFeries.value = joursFeries.value.filter(d => d !== raw)
  saveFeries()
}

// ─── Vacances — auto-save ─────────────────────────────────────────────────────
async function addVacance() {
  if (!newVacance.value.startDate || !newVacance.value.endDate) return
  periodesVacances.value.push({ ...newVacance.value })
  newVacance.value = { label: '', startDate: '', endDate: '' }
  await saveVacances()
}

async function deleteVacance(i) {
  periodesVacances.value.splice(i, 1)
  await saveVacances()
}

async function saveVacances() {
  isSavingVacance.value = true
  showSaveStatus('saving')
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/vacances`, {
      method: 'PUT',
      body: JSON.stringify(periodesVacances.value),
    })
    if (res?.ok) { showSaveStatus('saved'); toast('Vacances enregistrées ✓') }
    else         { showSaveStatus('error'); toast('Erreur lors de l\'enregistrement', 'error') }
  } catch (e) { showSaveStatus('error'); console.error(e) }
  finally { isSavingVacance.value = false }
}

// ─── Helpers ──────────────────────────────────────────────────────────────────
function fmtDate(d) {
  return d ? moment(d).format('DD/MM/YYYY') : '—'
}
</script>

<style scoped>
/* Sessions list */
.sessions-list { display: flex; flex-direction: column; gap: 8px; }

.session-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  border-radius: 10px;
  border: 1px solid #e8e8e8;
  background: #fafafa;
  transition: background 0.15s, border-color 0.15s;
}
.session-row:hover { background: #f0f4ff; border-color: #c7d7f5; }
.session-row--editing { background: #fff8e1; border-color: #ffcc02; border-left: 4px solid #fb8c00; }

.session-row__main { flex: 1; min-width: 0; }
.session-name { font-weight: 600; font-size: 0.875rem; color: #1a202c; }
.session-dates { margin-top: 2px; }

/* Formulaire */
.form-card { background: #fafbff !important; border-color: #d0d9f0 !important; }

/* Tab header sticky */
.sessions-table th {
  font-size: 0.72rem;
  color: #718096;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
</style>