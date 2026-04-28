<template>
  <v-container fluid class="pa-4">

    <!-- Navigation période -->
    <v-card class="mb-4 elevation-2" rounded="lg">
      <v-card-text class="pa-4">
        <div class="d-flex justify-space-between align-center">
          <v-btn icon @click="changePeriod(-1)" :disabled="isLoading || navSessionIndex <= 0">
            <v-icon>mdi-chevron-left</v-icon>
          </v-btn>
          <div class="text-center">
            <div class="text-h6 font-weight-bold text-primary">{{ currentPeriodLabel }}</div>
            <div class="text-caption text-grey">{{ periodDateLabel }}</div>
          </div>
          <v-btn icon @click="changePeriod(1)" :disabled="isLoading || navSessionIndex >= activeGroupSessions.length - 1">
            <v-icon>mdi-chevron-right</v-icon>
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- Onglets groupes -->
    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-tabs v-model="activeTab" class="px-4" color="primary" @update:modelValue="onTabChange">
        <v-tab v-for="tab in groups" :key="tab.value">
          {{ tab.label }}
          <v-chip size="x-small" class="ml-2" color="primary" variant="outlined">
            {{ groupCount(tab.value) }}
          </v-chip>
        </v-tab>
      </v-tabs>
    </v-card>

    <!-- Tableau principal -->
    <v-card class="elevation-4" rounded="lg" :loading="isLoading">
      <v-row align="center" class="px-2">
        <v-col cols="12" md="6" class="pa-6">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un participant..."
            variant="outlined" density="compact" clearable hide-details
            bg-color="white"
          />
        </v-col>
        <v-col cols="12" md="6" class="d-flex justify-md-end align-center gap-2 pa-2">
          <v-btn color="primary" variant="outlined" rounded="lg" @click="openConfigDialog">
            <v-icon start>mdi-view-column-outline</v-icon>
            Gérer les colonnes
          </v-btn>
        </v-col>
      </v-row>

      <!-- Légende -->
      <div class="d-flex flex-wrap align-center gap-2 px-4 pb-3">
        <span class="legend-pill acco-pill">
          <v-icon size="x-small" class="mr-1">mdi-account-heart</v-icon>
          Accompagnement
        </span>
        <span class="legend-pill atelier-pill">
          <v-icon size="x-small" class="mr-1">mdi-hammer-wrench</v-icon>
          Atelier
        </span>
        <span class="text-caption text-grey ml-2">Cliquez sur une cellule pour ajouter des dates</span>
      </div>

      <v-divider></v-divider>

      <v-card-text class="pa-0">
        <div class="table-container">
          <table class="matrix-table">
            <thead>
              <tr>
                <th class="sticky-col name-col">Participant·e</th>
                <th v-for="col in accompanyingCols" :key="col.key" class="activity-header acco-header">
                  <div class="header-content">
                    <v-icon size="x-small" class="mr-1">mdi-account-heart</v-icon>
                    {{ col.label }}
                  </div>
                </th>
                <th v-for="col in workshopCols" :key="col.key" class="activity-header atelier-header">
                  <div class="header-content">
                    <v-icon size="x-small" class="mr-1">mdi-hammer-wrench</v-icon>
                    {{ col.label }}
                  </div>
                </th>
                <th class="sticky-col actions-col">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in filteredUsers" :key="user.id" class="table-row">
                <td class="sticky-col name-col">
                  <div class="user-info">
                    <div class="name">{{ user.nom }} {{ user.prenom }}</div>
                    <div class="trigramme">{{ user.userName }}</div>
                    <div class="text-caption font-weight-bold text-medium-emphasis">{{ user.niveau }}</div>
                    <div class="text-caption font-weight-bold text-primary">{{ getUserSessionInfo(user) }} ({{ currentYear }})</div>
                  </div>
                </td>

                <td
                  v-for="col in accompanyingCols" :key="col.key"
                  class="interactive-cell acco-cell"
                  :class="{ 'has-data': getDatesCount(user.id, col.key) > 0 }"
                  @click="openDateEditor(user, col)"
                >
                  <div v-if="getDatesCount(user.id, col.key) > 0" class="cell-badge acco-badge">
                    <v-icon size="x-small" class="mr-1">mdi-calendar-check</v-icon>
                    {{ getDatesCount(user.id, col.key) }} séance{{ getDatesCount(user.id, col.key) > 1 ? 's' : '' }}
                  </div>
                  <div v-else class="cell-empty">
                    <v-icon size="small" color="blue-lighten-3">mdi-plus-circle-outline</v-icon>
                  </div>
                </td>

                <td
                  v-for="col in workshopCols" :key="col.key"
                  class="interactive-cell atelier-cell"
                  :class="{ 'has-data': getDatesCount(user.id, col.key) > 0 }"
                  @click="openDateEditor(user, col)"
                >
                  <div v-if="getDatesCount(user.id, col.key) > 0" class="cell-badge atelier-badge">
                    <v-icon size="x-small" class="mr-1">mdi-calendar-check</v-icon>
                    {{ getDatesCount(user.id, col.key) }} séance{{ getDatesCount(user.id, col.key) > 1 ? 's' : '' }}
                  </div>
                  <div v-else class="cell-empty">
                    <v-icon size="small" color="green-lighten-3">mdi-plus-circle-outline</v-icon>
                  </div>
                </td>

                <td class="sticky-col actions-col">
                  <v-btn icon size="small" color="grey-darken-1" @click="openSummary(user)">
                    <v-icon size="small">mdi-printer-eye</v-icon>
                  </v-btn>
                </td>
              </tr>
              <tr v-if="!isLoading && filteredUsers.length === 0">
                <td :colspan="2 + workshopCols.length + accompanyingCols.length" class="text-center pa-8 text-grey">
                  Aucun participant trouvé.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </v-card-text>
    </v-card>

    <!-- ════════ Modal éditeur de dates ════════ -->
    <v-dialog v-model="editorDialog" max-width="380px">
      <v-card v-if="editingCell" rounded="lg">
        <v-card-title
          class="py-3 px-4 d-flex align-center gap-2"
          :class="editingCell.column.type === 'acco' ? 'bg-acco-header' : 'bg-atelier-header'"
        >
          <v-icon :color="editingCell.column.type === 'acco' ? 'blue-darken-2' : 'green-darken-2'">
            {{ editingCell.column.type === 'acco' ? 'mdi-account-heart' : 'mdi-hammer-wrench' }}
          </v-icon>
          <span class="text-subtitle-1 font-weight-bold">{{ editingCell.column.label }}</span>
          <v-chip size="x-small" :color="editingCell.column.type === 'acco' ? 'blue' : 'green'" variant="tonal" class="ml-auto">
            {{ editingCell.column.type === 'acco' ? 'Accompagnement' : 'Atelier' }}
          </v-chip>
        </v-card-title>
        <div class="px-4 pt-2 pb-1 text-caption text-grey">
          <v-icon size="x-small">mdi-account</v-icon>
          {{ editingCell.user.nom }} {{ editingCell.user.prenom }}
        </div>
        <v-date-picker
          v-model="datesPickerBuffer"
          multiple hide-header scrollable width="100%"
          locale="fr-CH" :first-day-of-week="1"
          :color="editingCell.column.type === 'acco' ? 'blue' : 'green'"
        ></v-date-picker>
        <v-divider></v-divider>
        <v-card-actions class="pa-3">
          <div class="text-caption text-grey">
            {{ datesPickerBuffer.length }} date{{ datesPickerBuffer.length > 1 ? 's' : '' }} sélectionnée{{ datesPickerBuffer.length > 1 ? 's' : '' }}
          </div>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="editorDialog = false">Annuler</v-btn>
          <v-btn
            :color="editingCell.column.type === 'acco' ? 'blue' : 'green'"
            variant="flat" @click="saveCellChanges" :loading="isSaving"
          >Enregistrer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Modal récapitulatif ════════ -->
    <v-dialog v-model="summaryDialog" max-width="500px">
      <v-card rounded="lg">
        <v-card-title class="d-flex align-center pa-4">
          <v-icon class="mr-2" color="primary">mdi-printer-eye</v-icon>
          {{ selectedUser?.nom }} {{ selectedUser?.prenom }}
          <v-spacer></v-spacer>
          <v-btn icon size="small" @click="summaryDialog = false"><v-icon>mdi-close</v-icon></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="pa-4">
          <div class="section-title acco-section-title mb-2">
            <v-icon size="small" class="mr-1">mdi-account-heart</v-icon>Accompagnements
          </div>
          <div v-for="col in accompanyingCols" :key="col.key" class="mb-3">
            <div v-if="getDatesCount(selectedUser?.id, col.key) > 0">
              <div class="text-caption font-weight-bold text-blue-darken-2 mb-1">{{ col.label }}</div>
              <div class="d-flex flex-wrap gap-1">
                <v-chip v-for="date in getSortedDates(selectedUser?.id, col.key)" :key="date"
                  size="small" color="blue" variant="tonal">{{ formatDate(date) }}</v-chip>
              </div>
            </div>
          </div>
          <v-divider class="my-3"></v-divider>
          <div class="section-title atelier-section-title mb-2">
            <v-icon size="small" class="mr-1">mdi-hammer-wrench</v-icon>Ateliers
          </div>
          <div v-for="col in workshopCols" :key="col.key" class="mb-3">
            <div v-if="getDatesCount(selectedUser?.id, col.key) > 0">
              <div class="text-caption font-weight-bold text-green-darken-2 mb-1">{{ col.label }}</div>
              <div class="d-flex flex-wrap gap-1">
                <v-chip v-for="date in getSortedDates(selectedUser?.id, col.key)" :key="date"
                  size="small" color="green" variant="tonal">{{ formatDate(date) }}</v-chip>
              </div>
            </div>
          </div>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="pa-4">
          <v-btn color="secondary" block variant="outlined" @click="copyToClipboard">
            <v-icon start>mdi-content-copy</v-icon> Copier le texte
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Modal configuration ════════ -->
    <v-dialog v-model="configDialog" max-width="520px" scrollable>
      <v-card rounded="lg">
        <v-card-title class="bg-primary text-white pa-4">
          <v-icon start>mdi-cog</v-icon>Configuration des activités
        </v-card-title>
        <v-tabs v-model="configTab" grow>
          <v-tab color="blue">
            <v-icon start size="small">mdi-account-heart</v-icon>
            Accompagnements
            <v-chip size="x-small" class="ml-1" color="blue" variant="tonal">{{ accompanyingCols.length }}</v-chip>
          </v-tab>
          <v-tab color="green">
            <v-icon start size="small">mdi-hammer-wrench</v-icon>
            Ateliers
            <v-chip size="x-small" class="ml-1" color="green" variant="tonal">{{ workshopCols.length }}</v-chip>
          </v-tab>
        </v-tabs>
        <v-card-text class="pa-4" style="min-height: 300px; max-height: 420px; overflow-y: auto;">
          <v-btn :color="configTab === 0 ? 'blue' : 'green'" block variant="tonal" class="mb-4"
            @click="addColumn(configTab === 0 ? 'acco' : 'atelier')">
            <v-icon start>mdi-plus</v-icon>
            Ajouter {{ configTab === 0 ? 'un accompagnement' : 'un atelier' }}
          </v-btn>
          <div v-for="col in (configTab === 0 ? accompanyingCols : workshopCols)" :key="col.key"
            class="col-item mb-2" :class="configTab === 0 ? 'col-item-acco' : 'col-item-atelier'">
            <v-icon size="small" class="drag-icon mr-2" color="grey">mdi-drag</v-icon>
            <v-text-field v-model="col.label" hide-details density="compact" variant="outlined"
              :color="configTab === 0 ? 'blue' : 'green'" bg-color="white" class="flex-grow-1"></v-text-field>
            <v-btn icon size="small" color="error" variant="text" class="ml-1"
              @click="removeColumn(col, configTab === 0 ? 'acco' : 'atelier')">
              <v-icon size="small">mdi-delete-outline</v-icon>
            </v-btn>
          </div>
          <div v-if="(configTab === 0 ? accompanyingCols : workshopCols).length === 0"
            class="text-center text-grey pa-6">
            <v-icon size="40" color="grey-lighten-2">{{ configTab === 0 ? 'mdi-account-heart-outline' : 'mdi-hammer-wrench' }}</v-icon>
            <div class="text-caption mt-2">Aucune colonne configurée</div>
          </div>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="pa-4">
          <v-btn variant="text" @click="configDialog = false">Fermer</v-btn>
          <v-spacer></v-spacer>
          <v-btn color="primary" variant="flat" @click="saveConfiguration" :loading="isSavingConfig">
            <v-icon start>mdi-content-save</v-icon>Enregistrer
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import moment from 'moment/min/moment-with-locales'
import API_BASE, {apiFetch} from '@/services/api'

moment.locale('fr')
const store  = useStore()
const router = useRouter()

// ─── Auth fetch avec logout auto ──────────────────────────────────────────────
async function authFetch(url, options = {}) {
  const res = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${localStorage.getItem('access_token')}`,
      ...options.headers,
    },
  })
  if (res.status === 401) {
    localStorage.removeItem('access_token')
    router.push({ name: 'login' })
    return null
  }
  return res
}

// ─── State ────────────────────────────────────────────────────────────────────
const activeTab         = ref(0)
const search            = ref('')
const isLoading         = ref(false)
const isSaving          = ref(false)
const isSavingConfig    = ref(false)
const editorDialog      = ref(false)
const summaryDialog     = ref(false)
const configDialog      = ref(false)
const configTab         = ref(0)
const accompanyingCols  = ref([])
const workshopCols      = ref([])
const presencesMatrix   = ref({})
const editingCell       = ref(null)
const selectedUser      = ref(null)
const datesPickerBuffer = ref([])
const allSessionLinks   = ref([])
const calendrier        = ref({ sessions: [], feries: [], vacances: [] })

// ── Index de navigation dans la liste des sessions du groupe ───────────────────
const navSessionIndex = ref(0)

const groups = [
  { label: 'F&I A1', value: 'fi_a1' },
  { label: 'F&I A2', value: 'fi_a2' },
]

const users = computed(() => store.state.user?.users || [])

// ─── Sessions du groupe actif triées par date de début ────────────────────────
const activeGroupSessions = computed(() => {
  const groupValue = groups[activeTab.value].value
  return calendrier.value.sessions
    .filter(s => {
      const t = (s.type || '').toLowerCase()
      return (groupValue === 'fi_a1' ? t === 'fi_a1' : t === 'fi_a2') && s.startDate
    })
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
})

// ─── Session courante = élément à l'index navSessionIndex ─────────────────────
const currentSession = computed(() =>
  activeGroupSessions.value[navSessionIndex.value] || null
)

// ─── currentYear et currentQuarter pour matchesGroup / getUserSessionInfo ──────
const currentYear = computed(() => {
  if (currentSession.value?.startDate) return moment(currentSession.value.startDate).year()
  return moment().year()
})

const currentQuarter = computed(() => navSessionIndex.value + 1)

// ─── Dates de la session pour loadMatrix ──────────────────────────────────────
const currentSessionDates = computed(() => {
  if (currentSession.value?.startDate) {
    return {
      start: moment(currentSession.value.startDate).format('YYYY-MM-DD'),
      end:   currentSession.value.endDate
        ? moment(currentSession.value.endDate).format('YYYY-MM-DD')
        : moment(currentSession.value.startDate).add(3, 'months').endOf('month').format('YYYY-MM-DD'),
    }
  }
  // Fallback trimestre calculé
  const start = moment([currentYear.value, (currentQuarter.value - 1) * 3, 1])
  return {
    start: start.format('YYYY-MM-DD'),
    end:   start.clone().endOf('quarter').format('YYYY-MM-DD'),
  }
})

// ─── Label de navigation ──────────────────────────────────────────────────────
const currentPeriodLabel = computed(() => {
  if (currentSession.value?.name) return currentSession.value.name
  return `Session ${currentQuarter.value} — ${currentYear.value}`
})

// ─── Dates réelles affichées en sous-titre ────────────────────────────────────
const periodDateLabel = computed(() => {
  if (currentSession.value?.startDate) {
    const s = moment(currentSession.value.startDate)
    const e = currentSession.value.endDate ? moment(currentSession.value.endDate) : null
    if (!e) return `À partir du ${s.locale('fr').format('DD MMM YYYY')}`
    return `${s.locale('fr').format('DD MMM')} – ${e.locale('fr').format('DD MMM YYYY')}`
  }
  return 'Dates à saisir dans calendrier'
})

const isFuturePeriod = computed(() => {
  const today = moment()
  return currentYear.value > today.year() ||
    (currentYear.value === today.year() && currentQuarter.value > Math.floor(today.month() / 3) + 1)
})

const allColumnsOrdered = computed(() => [...accompanyingCols.value, ...workshopCols.value])

// ─── Filtrage ─────────────────────────────────────────────────────────────────
const matchesGroup = (user, groupValue) => {
  const userId    = user.id
  const userLinks = allSessionLinks.value.filter(link =>
    String(link.studentId || link.StudentId).toLowerCase() === String(userId).toLowerCase()
  )
  if (userLinks.length === 0) return false

  const hasActiveSession = userLinks.some(link => {
    const s = link.session || link.Session
    if (!s) return false
    const sessionYear   = new Date(s.dateStart || s.DateStart).getFullYear()
    const sessionNumber = s.sessionNumberId || s.SessionNumberId
    return Number(sessionYear)   === Number(currentYear.value) &&
           Number(sessionNumber) === Number(currentQuarter.value)
  })
  if (!hasActiveSession) return false

  const niveauUser  = (user.niveau || '').toLowerCase().trim()
  const isTelecours = niveauUser.includes('tele') || niveauUser.includes('télé')
  if (groupValue === 'fi_a1') return !isTelecours && niveauUser.includes('a1')
  if (groupValue === 'fi_a2') return !isTelecours && niveauUser.includes('a2')
  return false
}

const groupCount = (val) => users.value.filter(u => matchesGroup(u, val)).length

const filteredUsers = computed(() => {
  const groupValue = groups[activeTab.value].value
  let list = users.value.filter(u => matchesGroup(u, groupValue))
  if (search.value) {
    const s = search.value.toLowerCase()
    list = list.filter(u =>
      (u.nom    || '').toLowerCase().includes(s) ||
      (u.prenom || '').toLowerCase().includes(s) ||
      (u.userName || '').toLowerCase().includes(s)
    )
  }
   list = list.sort((a, b) =>
    (a.nom || '').localeCompare((b.nom || ''), 'fr', { sensitivity: 'base' })
  )
  return list
})

const getUserSessionInfo = (user) => {
  const activeLink = allSessionLinks.value.find(link => {
    const linkUserId = link.studentId || link.StudentId
    const s = link.session || link.Session
    if (String(linkUserId).toLowerCase() !== String(user.id).toLowerCase() || !s) return false
    const year = new Date(s.dateStart || s.DateStart).getFullYear()
    const num  = s.sessionNumberId || s.SessionNumberId
    return Number(year) === Number(currentYear.value) &&
           Number(num)  === Number(currentQuarter.value)
  })
  const s = activeLink?.session || activeLink?.Session
  return s ? `Session ${s.sessionNumberId || s.SessionNumberId}` : 'Aucune session'
}

// ─── Init : pointer sur la session qui contient aujourd'hui ───────────────────
function initSessionIndex() {
  const today    = moment()
  const sessions = activeGroupSessions.value
  if (sessions.length === 0) { navSessionIndex.value = 0; return }

  const idx = sessions.findIndex(s =>
    s.startDate && s.endDate &&
    today.isBetween(moment(s.startDate), moment(s.endDate), 'day', '[]')
  )
  navSessionIndex.value = idx >= 0 ? idx : sessions.length - 1
}

function onTabChange() {
  initSessionIndex()
  loadMatrix()
}

// ─── API ──────────────────────────────────────────────────────────────────────
const loadCalendrier = async () => {
  try {
    const res = await authFetch(`${API_BASE}/api/calendrier`)
    if (res?.ok) calendrier.value = await res.json()
  } catch (e) { console.error('loadCalendrier:', e) }
}

const loadAllSessionLinks = async () => {
  try {
    const res = await authFetch(`${API_BASE}/api/SessionStudents`)
    if (res?.ok) allSessionLinks.value = await res.json()
  } catch (e) { console.error('Erreur liens session:', e) }
}

const loadColumns = async () => {
  try {
    const res = await authFetch(`${API_BASE}/api/accompanying/columns`)
    if (res?.ok) {
      const data = await res.json()
      accompanyingCols.value = data.filter(c => c.type === 'acco')
      workshopCols.value     = data.filter(c => c.type === 'atelier')
    }
  } catch (e) { console.error(e) }
}

const loadMatrix = async () => {
  isLoading.value = true
  const { start, end } = currentSessionDates.value
  try {
    const res = await authFetch(`${API_BASE}/api/accompanying/matrix?start=${start}&end=${end}`)
    if (res?.ok) {
      const data   = await res.json()
      const matrix = {}
      data.forEach(item => {
        const uid = item.userId || item.UserId || item.stagiaireId
        if (!matrix[uid]) matrix[uid] = {}
        matrix[uid][item.activityKey || item.ActivityKey] = item.dates || item.Dates
      })
      presencesMatrix.value = matrix
    }
  } catch (e) { console.error(e) }
  finally { isLoading.value = false }
}

const saveCellChanges = async () => {
  if (!editingCell.value) return
  isSaving.value = true
  const { user, column } = editingCell.value

  const normalizedDates = datesPickerBuffer.value.map(d => {
    if (typeof d === 'string') return d.substring(0, 10)
    return moment(d).format('YYYY-MM-DD')
  })

  const payload = {
    userId:      user.id,
    activityKey: column.key,
    dates:       normalizedDates,
    periodStart: currentSessionDates.value.start,
    periodEnd:   currentSessionDates.value.end,
  }

  try {
    const res = await authFetch(`${API_BASE}/api/accompanying/sync-dates`, {
      method: 'POST',
      body:   JSON.stringify(payload),
    })
    if (res?.ok) {
      if (!presencesMatrix.value[user.id]) presencesMatrix.value[user.id] = {}
      presencesMatrix.value[user.id][column.key] = [...normalizedDates]
      editorDialog.value = false
    } else {
      alert('Erreur lors de la sauvegarde.')
    }
  } catch (e) { alert('Impossible de joindre le serveur.') }
  finally { isSaving.value = false }
}

const openDateEditor = (user, col) => {
  editingCell.value       = { user, column: col }
  datesPickerBuffer.value = [...(presencesMatrix.value[user.id]?.[col.key] || [])]
  editorDialog.value      = true
}

const openSummary = (user) => {
  selectedUser.value  = user
  summaryDialog.value = true
}

const getDatesCount  = (userId, key) => presencesMatrix.value[userId]?.[key]?.length || 0
const getSortedDates = (userId, key) => [...(presencesMatrix.value[userId]?.[key] || [])].sort()
const formatDate     = (d) => moment(d).format('DD.MM')

// ─── Navigation par index ──────────────────────────────────────────────────────
const changePeriod = (delta) => {
  const newIdx = navSessionIndex.value + delta
  if (newIdx >= 0 && newIdx < activeGroupSessions.value.length) {
    navSessionIndex.value = newIdx
    loadMatrix()
  }
}

const openConfigDialog = () => { configDialog.value = true }

const addColumn = (type) => {
  const newCol = { key: `Act_${Date.now()}`, label: 'Nouvelle activité', type, displayOrder: 0 }
  if (type === 'atelier') workshopCols.value.push(newCol)
  else accompanyingCols.value.push(newCol)
}

const removeColumn = async (item, type) => {
  if (!confirm('Supprimer cette colonne ?')) return
  if (type === 'atelier') {
    workshopCols.value = workshopCols.value.filter(c => c.key !== item.key)
  } else {
    accompanyingCols.value = accompanyingCols.value.filter(c => c.key !== item.key)
  }
  if (item.id) {
    try {
      await authFetch(`${API_BASE}/api/accompanying/columns/${item.id}`, { method: 'DELETE' })
    } catch (e) { console.error('Erreur suppression colonne:', e) }
  }
}

const saveConfiguration = async () => {
  isSavingConfig.value = true
  try {
    const res = await authFetch(`${API_BASE}/api/accompanying/columns`, {
      method: 'POST',
      body:   JSON.stringify([...workshopCols.value, ...accompanyingCols.value]),
    })
    if (res?.ok) {
      configDialog.value = false
      await loadColumns()
    }
  } catch (e) { alert('Erreur config') }
  finally { isSavingConfig.value = false }
}

const copyToClipboard = () => {
  let text = `Dates pour ${selectedUser.value.nom} ${selectedUser.value.prenom}\n\n`
  allColumnsOrdered.value.forEach(col => {
    const dates = getSortedDates(selectedUser.value.id, col.key)
    if (dates.length > 0) text += `${col.label} : ${dates.map(d => moment(d).format('DD.MM')).join(', ')}\n`
  })
  navigator.clipboard.writeText(text)
  alert('Copié !')
}

onMounted(async () => {
  isLoading.value = true
  try {
    await store.dispatch('user/fetchUsers')
    await Promise.all([loadAllSessionLinks(), loadCalendrier()])
    initSessionIndex()
    await loadColumns()
    await loadMatrix()
  } catch (e) {
    console.error('Erreur chargement:', e)
  } finally {
    isLoading.value = false
  }
})
</script>

<style scoped>
.table-container { overflow-x: auto; max-height: 70vh; }
.matrix-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.matrix-table th, .matrix-table td { border: 1px solid #e8e8e8; padding: 10px 8px; text-align: center; }

.sticky-col { position: sticky; background: white; z-index: 2; }
.name-col   { left: 0; text-align: center !important; min-width: 220px; box-shadow: 2px 0 5px rgba(0,0,0,0.06); }
.matrix-table td.name-col { text-align: center !important; }
.actions-col { right: 0; min-width: 60px; background: #fafafa; }

.activity-header { position: sticky; top: 0; z-index: 3; min-width: 130px; }
.acco-header    { background: #EFF6FF; border-top: 3px solid #2196F3; color: #1565C0; font-weight: 600; }
.atelier-header { background: #F0FDF4; border-top: 3px solid #4CAF50; color: #2E7D32; font-weight: 600; }
.header-content { display: flex; align-items: center; justify-content: center; gap: 2px; font-size: 0.78rem; }

.interactive-cell { cursor: pointer; transition: background 0.15s; height: 52px; vertical-align: middle; }
.acco-cell:hover    { background: #DBEAFE !important; }
.atelier-cell:hover { background: #DCFCE7 !important; }
.acco-cell.has-data    { background: #EFF6FF; }
.atelier-cell.has-data { background: #F0FDF4; }

.cell-badge { display: inline-flex; align-items: center; border-radius: 20px; padding: 3px 10px; font-size: 0.72rem; font-weight: 600; }
.acco-badge    { background: #2196F3; color: #fff; }
.atelier-badge { background: #4CAF50; color: #fff; }

.cell-empty { opacity: 0.4; transition: opacity 0.15s; }
.interactive-cell:hover .cell-empty { opacity: 0.9; }

.legend-pill { display: inline-flex; align-items: center; padding: 3px 10px; border-radius: 20px; font-size: 0.72rem; font-weight: 600; }
.acco-pill    { background: #EFF6FF; color: #1565C0; border: 1px solid #BFDBFE; }
.atelier-pill { background: #F0FDF4; color: #2E7D32; border: 1px solid #BBF7D0; }

.section-title { display: flex; align-items: center; font-weight: 700; font-size: 0.85rem; }
.acco-section-title    { color: #1565C0; }
.atelier-section-title { color: #2E7D32; }

.col-item { display: flex; align-items: center; border-radius: 8px; padding: 6px 8px; }
.col-item-acco    { background: #EFF6FF; border: 1px solid #BFDBFE; }
.col-item-atelier { background: #F0FDF4; border: 1px solid #BBF7D0; }
.drag-icon { cursor: grab; }

.bg-acco-header    { background: #EFF6FF; }
.bg-atelier-header { background: #F0FDF4; }

.user-info { padding: 4px 8px; }
.user-info .name { font-weight: 700; color: #1a1a1a; }
.user-info .trigramme { font-size: 0.72rem; color: #999; text-transform: uppercase; }
</style>