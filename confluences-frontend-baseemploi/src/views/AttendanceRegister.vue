<template>
  <v-container fluid class="pa-4">

    <!-- Navigation période -->
    <v-card class="mb-4 elevation-2" rounded="lg">
      <v-card-text class="pa-3">
        <div class="d-flex justify-space-between align-center">
          <v-btn icon @click="navigatePrev" :disabled="isLoading || !canGoPrev">
            <v-icon>mdi-chevron-left</v-icon>
          </v-btn>

          <div class="text-center flex-grow-1">
            <div class="text-h6 font-weight-bold text-primary">{{ navLabel }}</div>
            <div class="text-caption text-grey">{{ currentPeriodRange }}</div>

            <!-- Toggle Session / Mois -->
            <div class="d-flex justify-center mt-2 gap-2">
              <v-btn-toggle v-model="navMode" mandatory density="compact" color="primary" rounded="lg">
                <v-btn value="session" size="small">
                  <v-icon start size="x-small">mdi-view-week</v-icon>
                  Session
                </v-btn>
                <v-btn value="month" size="small">
                  <v-icon start size="x-small">mdi-calendar-month</v-icon>
                  Mois
                </v-btn>
              </v-btn-toggle>
            </div>
          </div>

          <v-btn icon @click="navigateNext" :disabled="isLoading || !canGoNext">
            <v-icon>mdi-chevron-right</v-icon>
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- Onglets groupes -->
    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-tabs v-model="activeTabIndex" class="px-4" color="primary" @update:modelValue="onTabChange">
        <v-tab v-for="g in groups" :key="g.value">
          {{ g.label }}
          <v-chip x-small class="ml-2" color="primary" variant="outlined">
            {{ groupCount(g.value) }}
          </v-chip>
        </v-tab>
      </v-tabs>
    </v-card>

    <!-- Tableau principal -->
    <v-card class="elevation-4" rounded="lg" :loading="isLoading">
      <v-row align="center" class="px-2">
        <v-col cols="12" md="5" class="pa-6">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un·e participant·e..."
            variant="outlined" density="compact" clearable hide-details
            bg-color="white"
          />
        </v-col>
        <v-col cols="12" md="7" class="d-flex flex-wrap justify-md-end align-center gap-2 pa-2">
          <v-btn size="small" variant="text" :to="{ name: 'calendrier' }">
            <v-icon start>mdi-calendar-edit</v-icon> Calendrier
          </v-btn>
          <v-btn size="small" variant="outlined" color="primary" @click="downloadAllAttestations">
            <v-icon start>mdi-download-multiple</v-icon> Imprimer tous
          </v-btn>
        </v-col>
      </v-row>

      <v-divider></v-divider>

      <!-- Légende -->
      <div class="d-flex flex-wrap gap-2 pa-3 legend-bar">
        <span v-for="s in statusLegend" :key="s.key" class="legend-item"
          :style="{ background: s.bg, color: s.color }">
          {{ s.label }}
        </span>
      </div>

      <v-divider></v-divider>

      <!-- Info si pas de session calendrier -->
      <v-card-text v-if="navMode === 'session' && !currentSession" class="pa-2">
        <v-alert type="info" variant="tonal" density="compact" class="ma-2">
          <v-icon start>mdi-calendar-alert</v-icon>
          Aucune session trouvée dans le calendrier pour ce groupe.
          <v-btn size="x-small" variant="text" :to="{ name: 'calendrier' }" class="ml-1">Configurer</v-btn>
        </v-alert>
      </v-card-text>

      <v-card-text class="pa-0">
        <div class="table-container">
          <table class="presence-table">
            <thead>
              <tr>
                <th class="sticky-col name-col">Participant·e</th>
                <th class="rate-col sticky-col sticky-rate-col">Taux</th>
                <th v-for="day in currentPeriodDays" :key="day.fullDate"
                    class="day-header"
                    :class="{ 'ferie-header': isJourFerie(day.fullDate) || isVacance(day.fullDate) }">
                  <div class="day-header-content">
                    <div class="day-name">{{ day.dayNameShort }}</div>
                    <div class="day-date">{{ day.date }}</div>
                    <div class="day-month" v-if="day.showMonth">{{ day.monthShort }}</div>
                    <div v-if="isJourFerie(day.fullDate)" class="ferie-indicator">F</div>
                    <div v-else-if="isVacance(day.fullDate)" class="vacance-indicator">V</div>
                  </div>
                </th>
                <th class="stats-col">Stats</th>
                <th class="actions-col">Fiche</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filteredUsers.length === 0">
                <td :colspan="3 + currentPeriodDays.length" class="text-center pa-8 text-grey">
                  Aucun participant trouvé.
                </td>
              </tr>

              <tr v-for="user in filteredUsers" :key="user.id" class="table-row">
                <td class="sticky-col name-col">
                  <div class="user-info">
                    <div class="name">{{ user.nom }} {{ user.prenom }}</div>
                    <div class="trigramme">{{ user.userName || user.username }}</div>
                    <div class="text-caption font-weight-bold text-medium-emphasis">{{ user.niveau }}</div>
                    <div class="text-caption font-weight-bold text-primary">{{ getUserSessionInfo(user) }} ({{ currentYear }})</div>
                  </div>
                </td>

                <td class="rate-col sticky-col sticky-rate-col" :class="getRateClass(calculateRates(user).globalRate)">
                  <strong>{{ calculateRates(user).globalRate.toFixed(1) }}%</strong>
                </td>

                <td
                  v-for="day in currentPeriodDays"
                  :key="day.fullDate"
                  :class="[
                    'presence-cell',
                    isBeforeParticipation(user, day.fullDate) ? 'not-started' :
                    isJourFerie(day.fullDate) ? 'status-f' :
                    isVacance(day.fullDate)   ? 'status-v' :
                    getStatusClass(user, day.fullDate),
                    { 'active-cell': activeCell && activeCell.id === user.id && activeCell.date === day.fullDate }
                  ]"
                  @click="!isBeforeParticipation(user, day.fullDate)
                    && !isJourFerie(day.fullDate)
                    && !isVacance(day.fullDate)
                    && selectCell(user, day.fullDate)"
                >
                  {{ isJourFerie(day.fullDate) ? 'F'
                     : isVacance(day.fullDate)  ? 'V'
                     : getStatusText(user, day.fullDate) }}
                </td>

                <td class="stats-col">
                  <div class="stats-inner">
                    <div class="stat-value present">{{ countStatus(user, 'p') }}P</div>
                    <div class="stat-value absent">{{ countStatus(user, 'a') }}A</div>
                    <div class="stat-value excused">{{ countStatus(user, 'e') }}E</div>
                    <div class="stat-value retard">{{ countStatus(user, 'r') }}R</div>
                    <div class="stat-value stage">{{ countStatus(user, 's') }}S</div>
                    <div class="stat-value vacances">{{ countStatus(user, 'v') }}V</div>
                    <div class="stat-value ferie">{{ countFeries(user) }}F</div>
                  </div>
                </td>
                <td class="actions-col">
                  <v-btn icon size="small" color="primary" @click="openModal(user)">
                    <v-icon>mdi-file-account-outline</v-icon>
                  </v-btn>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </v-card-text>
    </v-card>

    <!-- ════════ Modal Attestation PDF ════════ -->
    <v-dialog v-model="modalVisible" max-width="560px" persistent>
      <v-card rounded="lg" v-if="modalVisible">
        <v-card-title class="headline bg-primary text-white pa-4">
          <v-icon start color="white">mdi-file-pdf-box</v-icon>
          Générer la fiche de présence
        </v-card-title>
        <v-card-text class="pa-6">
          <div v-if="selectedUser" class="mb-5 text-center">
            <v-avatar color="primary-lighten-4" size="56" class="mb-2">
              <span class="text-primary text-h6">
                {{ (selectedUser.prenom || selectedUser.nom || '').charAt(0).toUpperCase() }}
              </span>
            </v-avatar>
            <div class="text-h6">{{ selectedUser.prenom || '' }} {{ selectedUser.nom || '' }}</div>
            <div class="text-caption text-grey text-uppercase mb-1">
              {{ selectedUser.userName || selectedUser.username || '' }}
            </div>
            <v-chip :color="isUserTelecours ? 'teal' : 'blue'" variant="tonal" size="small"
              :prepend-icon="isUserTelecours ? 'mdi-monitor' : 'mdi-school'">
              {{ isUserTelecours ? 'Français Télécours' : 'Français & Intégration' }}
            </v-chip>
          </div>

          <v-tabs v-model="pdfModeTab" color="primary" class="mb-4">
            <v-tab value="session">Par Session</v-tab>
            <v-tab value="month">Par Mois</v-tab>
          </v-tabs>

          <v-window v-model="pdfModeTab">
            <v-window-item value="session">
              <v-select v-model="selectedSessionId" :items="filteredSessionsForUser"
                item-title="name" item-value="name"
                :label="filteredSessionsForUser.length ? 'Session' : 'Aucune session correspondante'"
                variant="outlined" density="compact" hide-details prepend-inner-icon="mdi-calendar-range">
                <template #item="{ item, props }">
                  <v-list-item v-bind="props">
                    <template #append>
                      <v-chip x-small :color="item.raw.type === 'telecours' ? 'teal' : 'blue'" variant="tonal">
                        {{ item.raw.type === 'telecours' ? '10/20/32j' : '20/40/60j' }}
                      </v-chip>
                    </template>
                  </v-list-item>
                </template>
              </v-select>
              <div v-if="!filteredSessionsForUser.length" class="mt-3">
                <v-alert type="warning" variant="tonal" density="compact">
                  Aucune session configurée.
                  <v-btn size="x-small" variant="text" :to="{ name: 'calendrier' }">Calendrier</v-btn>
                </v-alert>
              </div>
            </v-window-item>
            <v-window-item value="month">
              <v-row>
                <v-col cols="6">
                  <v-select v-model="selectedYear" :items="availableYears" label="Année"
                    variant="outlined" density="compact" hide-details prepend-inner-icon="mdi-calendar" />
                </v-col>
                <v-col cols="6">
                  <v-select v-model="selectedMonth" :items="availableMonths" item-title="text" item-value="value"
                    label="Mois" variant="outlined" density="compact" hide-details prepend-inner-icon="mdi-calendar-month" />
                </v-col>
              </v-row>
            </v-window-item>
          </v-window>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="pa-4">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="modalVisible = false" :disabled="isLoadingModal">Annuler</v-btn>
          <v-btn color="primary" rounded @click="downloadAttestation" :loading="isLoadingModal"
            :disabled="!selectedUser || (pdfModeTab === 'session' && !selectedSessionId)" class="px-6">
            <v-icon start>mdi-download</v-icon>Télécharger PDF
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Modal Date début participation ════════ -->
    <v-dialog v-model="startDateModalVisible" max-width="400px">
      <v-card rounded="lg">
        <v-card-title class="headline bg-primary text-white pa-4">Date de début de participation</v-card-title>
        <v-card-text class="pa-4">
          <v-date-picker v-model="newStartDate" width="100%" locale="fr" :first-day-of-week="1" />
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="startDateModalVisible = false">Annuler</v-btn>
          <v-btn color="primary" @click="saveStartDate" :loading="isLoading">Enregistrer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'

moment.locale('fr')

const store  = useStore()
const router = useRouter()
const users  = computed(() => store.state.user?.users || [])

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
const search          = ref('')
const activeTabIndex  = ref(0)
const isLoading       = ref(false)
const isLoadingModal  = ref(false)
const allSessionLinks = ref([])
const calendrier      = ref({ sessions: [], feries: [], vacances: [] })
const activeCell      = ref(null)
const presenceCache   = ref({})

// ── Mode navigation : 'session' ou 'month' ─────────────────────────────────────
const navMode = ref('session')

// ── Navigation Session : index dans activeGroupSessions ────────────────────────
const navSessionIndex = ref(0)

// ── Navigation Mois ────────────────────────────────────────────────────────────
const navMonthYear = ref(moment().year())
const navMonth     = ref(moment().month() + 1) // 1-12

const groups = [
  { label: 'Télécours', value: 'telecours' },
  { label: 'F&I A1',    value: 'fi_a1'    },
  { label: 'F&I A2',    value: 'fi_a2'    },
]

// Modals PDF
const modalVisible      = ref(false)
const selectedUser      = ref(null)
const selectedYear      = ref(null)
const selectedMonth     = ref(null)
const availableYears    = ref([])
const pdfModeTab        = ref('session')
const selectedSessionId = ref(null)

// Modal date début
const startDateModalVisible = ref(false)
const editingUser           = ref(null)
const newStartDate          = ref(null)

const statusLegend = [
  { key: 'p', label: 'Présent',                              bg: '#1A237E', color: '#fff'    },
  { key: 'a', label: 'A - Absent',                           bg: '#FFCDD2', color: '#B71C1C' },
  { key: 'e', label: 'E - Excusé',                           bg: '#FFE0B2', color: '#E65100' },
  { key: 'r', label: 'P - Retard >15min ou départ anticipé', bg: '#B3E5FC', color: '#01579B' },
  { key: 's', label: 'S - Stage',                            bg: '#C8E6C9', color: '#1B5E20' },
  { key: 'v', label: 'V - Vacances',                         bg: '#1B5E20', color: '#fff'    },
  { key: 'f', label: 'F - Férié',                            bg: '#FFF9C4', color: '#827717' },
]

// ─── Sessions du groupe actif triées par date de début ────────────────────────
const activeGroupSessions = computed(() => {
  const groupValue = groups[activeTabIndex.value].value
  return calendrier.value.sessions
    .filter(s => {
      const t = (s.type || '').toLowerCase()
      return (
        groupValue === 'telecours' ? t === 'telecours' :
        groupValue === 'fi_a1'    ? t === 'fi_a1'     :
        groupValue === 'fi_a2'    ? t === 'fi_a2'     : false
      ) && s.startDate
    })
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
})

// ─── Session courante = simplement l'élément à l'index navSessionIndex ─────────
const currentSession = computed(() => {
  if (navMode.value === 'month') return null
  return activeGroupSessions.value[navSessionIndex.value] || null
})

// ─── Numéro affiché = position dans la liste (1, 2, 3, 4) ─────────────────────
const currentSessionDisplayNumber = computed(() => navSessionIndex.value + 1)

// ─── Année courante (pour matchesGroup) ───────────────────────────────────────
const currentYear = computed(() => {
  if (navMode.value === 'month') return navMonthYear.value
  return currentSession.value
    ? moment(currentSession.value.startDate).year()
    : navMonthYear.value
})

// ─── Numéro de session (pour matchesGroup / getUserSessionInfo) ────────────────
const currentQuarter = computed(() => {
  if (navMode.value === 'month') {
    // En mode mois, trouver quelle session du calendrier contient ce mois
    const firstDayOfMonth = moment({ year: navMonthYear.value, month: navMonth.value - 1, day: 1 })
    const sess = calendrier.value.sessions.find(s =>
      s.startDate && s.endDate &&
      firstDayOfMonth.isBetween(moment(s.startDate), moment(s.endDate), 'month', '[]')
    )
    if (sess) {
      const idx = activeGroupSessions.value.indexOf(sess)
      return idx >= 0 ? idx + 1 : 1
    }
    return 1
  }
  return currentSessionDisplayNumber.value
})

// ─── Cannes de navigation ─────────────────────────────────────────────────────
const canGoPrev = computed(() => {
  if (navMode.value === 'month') return true // on peut toujours aller dans le passé
  return navSessionIndex.value > 0
})

const canGoNext = computed(() => {
  if (navMode.value === 'month') {
    const today = moment()
    return !(navMonthYear.value === today.year() && navMonth.value >= today.month() + 1)
  }
  return navSessionIndex.value < activeGroupSessions.value.length - 1
})

// ─── Label affiché en haut ────────────────────────────────────────────────────
const navLabel = computed(() => {
  if (navMode.value === 'month') {
    return moment({ year: navMonthYear.value, month: navMonth.value - 1 })
      .locale('fr').format('MMMM YYYY').replace(/^\w/, c => c.toUpperCase())
  }
  if (currentSession.value) {
    return currentSession.value.name || `Session ${currentSessionDisplayNumber.value} — ${currentYear.value}`
  }
  return 'Aucune session'
})

// ─── Plage de dates affichée ──────────────────────────────────────────────────
const currentPeriodRange = computed(() => {
  if (navMode.value === 'month') {
    const s = moment({ year: navMonthYear.value, month: navMonth.value - 1, day: 1 })
    const e = s.clone().endOf('month')
    return `${s.locale('fr').format('DD MMM')} – ${e.locale('fr').format('DD MMM YYYY')}`
  }
  if (!currentSession.value?.startDate) return 'Aucune session configurée'
  const s = moment(currentSession.value.startDate)
  const e = currentSession.value.endDate ? moment(currentSession.value.endDate) : null
  if (!e) return `À partir du ${s.locale('fr').format('DD MMM YYYY')}`
  return `${s.locale('fr').format('DD MMM')} – ${e.locale('fr').format('DD MMM YYYY')}`
})

// ─── Jours de la période ──────────────────────────────────────────────────────
const currentPeriodDays = computed(() => {
  let start, end

  if (navMode.value === 'month') {
    start = moment({ year: navMonthYear.value, month: navMonth.value - 1, day: 1 }).startOf('day')
    end   = start.clone().endOf('month')
  } else if (currentSession.value?.startDate) {
    start = moment(currentSession.value.startDate).startOf('day')
    end   = currentSession.value.endDate
      ? moment(currentSession.value.endDate).endOf('day')
      : moment(currentSession.value.startDate).add(3, 'months').endOf('month')
  } else {
    // Aucune session dans le calendrier : afficher le mois courant
    start = moment().startOf('month')
    end   = moment().endOf('month')
  }

  const days = []
  let curr = start.clone()
  let lastMonth = null
  while (curr.isSameOrBefore(end, 'day')) {
    if (curr.isoWeekday() < 6) {
      const monthNum  = curr.month()
      const showMonth = monthNum !== lastMonth
      if (showMonth) lastMonth = monthNum
      days.push({
        fullDate:     curr.clone().locale('en').format('ddd MMM DD YYYY'),
        date:         curr.format('D'),
        dayNameShort: curr.locale('fr').format('ddd'),
        monthShort:   curr.locale('fr').format('MMM'),
        showMonth,
        isToday:      curr.isSame(moment(), 'day'),
      })
    }
    curr.add(1, 'day')
  }
  return days
})

// ─── Navigation ───────────────────────────────────────────────────────────────
function navigatePrev() {
  if (navMode.value === 'month') {
    navMonth.value--
    if (navMonth.value < 1) { navMonth.value = 12; navMonthYear.value-- }
  } else {
    if (navSessionIndex.value > 0) navSessionIndex.value--
  }
  fetchAttendancesForPeriod()
}

function navigateNext() {
  if (navMode.value === 'month') {
    navMonth.value++
    if (navMonth.value > 12) { navMonth.value = 1; navMonthYear.value++ }
  } else {
    if (navSessionIndex.value < activeGroupSessions.value.length - 1) navSessionIndex.value++
  }
  fetchAttendancesForPeriod()
}

function onTabChange() {
  initializeNav()
}

// ─── Init : pointer sur la session qui contient aujourd'hui ───────────────────
function initializeNav() {
  const today    = moment()
  const sessions = activeGroupSessions.value

  if (sessions.length > 0) {
    // Chercher la session dont les dates encadrent aujourd'hui
    const idx = sessions.findIndex(s =>
      s.startDate && s.endDate &&
      today.isBetween(moment(s.startDate), moment(s.endDate), 'day', '[]')
    )
    navSessionIndex.value = idx >= 0 ? idx : sessions.length - 1
  } else {
    navSessionIndex.value = 0
  }

  // Mode mois = mois courant
  navMonthYear.value = today.year()
  navMonth.value     = today.month() + 1

  fetchAttendancesForPeriod()
}

// ─── Jours fériés & vacances ──────────────────────────────────────────────────
function isJourFerie(fullDate) {
  if (!calendrier.value.feries?.length) return false
  const d = moment(fullDate, 'ddd MMM DD YYYY', 'en')
  return calendrier.value.feries.some(f => moment(f.date).isSame(d, 'day'))
}

function isVacance(fullDate) {
  const d = moment(fullDate, 'ddd MMM DD YYYY', 'en')
  const inGeneral = calendrier.value.vacances?.some(v =>
    d.isBetween(moment(v.startDate), moment(v.endDate), 'day', '[]')
  ) ?? false
  if (inGeneral) return true
  return calendrier.value.sessions?.some(s =>
    s.vacanceStart && s.vacanceEnd &&
    d.isBetween(moment(s.vacanceStart), moment(s.vacanceEnd), 'day', '[]')
  ) ?? false
}

// ─── Groupes ──────────────────────────────────────────────────────────────────
function getUserSessionInfo(user) {
  const link = allSessionLinks.value.find(l => {
    const uid = l.studentId || l.StudentId
    const s   = l.session || l.Session
    if (String(uid).toLowerCase() !== String(user.id).toLowerCase() || !s) return false
    const y = new Date(s.dateStart || s.DateStart).getFullYear()
    const n = s.sessionNumberId || s.SessionNumberId
    return Number(y) === currentYear.value && Number(n) === currentQuarter.value
  })
  const s = link?.session || link?.Session
  return s ? `Session ${s.sessionNumberId || s.SessionNumberId}` : ''
}

function matchesGroup(user, groupValue) {
  const uid   = user.id || user.Id
  const links = allSessionLinks.value.filter(l =>
    String(l.studentId || l.StudentId).toLowerCase() === String(uid).toLowerCase()
  )
  if (!links.length) return false
  const hasActive = links.some(l => {
    const s = l.session || l.Session
    if (!s) return false
    const y = new Date(s.dateStart || s.DateStart).getFullYear()
    const n = s.sessionNumberId || s.SessionNumberId
    return Number(y) === currentYear.value && Number(n) === currentQuarter.value
  })
  if (!hasActive) return false
  const niveau      = (user.niveau || user.Niveau || '').toLowerCase().trim()
  const isTelecours = niveau.includes('tele') || niveau.includes('télé')
  if (groupValue === 'telecours') return isTelecours
  if (groupValue === 'fi_a1')    return !isTelecours && niveau.includes('a1')
  if (groupValue === 'fi_a2')    return !isTelecours && niveau.includes('a2')
  return false
}

function groupCount(val) {
  return users.value.filter(u => matchesGroup(u, val)).length
}

const filteredUsers = computed(() => {
  const g = groups[activeTabIndex.value].value
  let list = users.value.filter(u => matchesGroup(u, g))
  if (search.value) {
    const s = search.value.toLowerCase()
    list = list.filter(u =>
      (u.nom || '').toLowerCase().includes(s) ||
      (u.prenom || '').toLowerCase().includes(s) ||
      (u.userName || u.username || '').toLowerCase().includes(s)
    )
  }
  list = list.sort((a, b) =>
    (a.nom || '').localeCompare((b.nom || ''), 'fr', { sensitivity: 'base' })
  )
  return list
})

const availableMonths = computed(() => {
  const all = [
    { text: 'Janvier', value: 1 },  { text: 'Février', value: 2 },   { text: 'Mars', value: 3 },
    { text: 'Avril', value: 4 },    { text: 'Mai', value: 5 },        { text: 'Juin', value: 6 },
    { text: 'Juillet', value: 7 },  { text: 'Août', value: 8 },       { text: 'Septembre', value: 9 },
    { text: 'Octobre', value: 10 }, { text: 'Novembre', value: 11 },  { text: 'Décembre', value: 12 },
  ]
  if (selectedYear.value === moment().year())
    return all.filter(m => m.value <= moment().month() + 1)
  return all
})

const isUserTelecours = computed(() => {
  const n = (selectedUser.value?.niveau || selectedUser.value?.Niveau || '').toLowerCase()
  return n.includes('tele') || n.includes('télé')
})

const filteredSessionsForUser = computed(() => {
  if (!selectedUser.value) return []
  return calendrier.value.sessions.filter(s => {
    const t = (s.type || '').toLowerCase()
    return isUserTelecours.value
      ? t === 'telecours'
      : (t === 'fi_a1' || t === 'fi_a2' || t.startsWith('fi'))
  })
})

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  try { await store.dispatch('user/fetchUsers', true) } catch (e) { console.error(e) }
  await Promise.all([loadCalendrier(), loadAllSessionLinks()])
  initializeNav()
  window.addEventListener('keyup', handleKeyDown)
})
onBeforeUnmount(() => window.removeEventListener('keyup', handleKeyDown))

// ─── API ──────────────────────────────────────────────────────────────────────
async function loadCalendrier() {
  try {
    const res = await authFetch(`${API_BASE}/api/calendrier`)
    if (res?.ok) calendrier.value = await res.json()
  } catch (e) { console.error(e) }
}

async function loadAllSessionLinks() {
  try {
    const res = await authFetch(`${API_BASE}/api/SessionStudents`)
    if (res?.ok) allSessionLinks.value = await res.json()
  } catch (e) { console.error(e) }
}

// ─── Cellules ─────────────────────────────────────────────────────────────────
function selectCell(user, dateString) {
  activeCell.value = { id: user.id, date: dateString, user }
  if (!presenceCache.value[`${user.id}__${dateString}`] && !user.presences?.[dateString])
    updateStatus(user, dateString, 'p')
}

function handleKeyDown(e) {
  if (!activeCell.value) return
  if (['INPUT', 'TEXTAREA'].includes(e.target.tagName)) return
  const map = { p: 'r', a: 'a', e: 'e', r: 'r', s: 's', v: 'v', f: 'f', delete: 'p' }
  const key = map[e.key.toLowerCase()]
  if (key) updateStatus(activeCell.value.user, activeCell.value.date, key)
}

async function updateStatus(user, dateString, newStatus) {
  presenceCache.value = { ...presenceCache.value, [`${user.id}__${dateString}`]: newStatus }
  try {
    const date = moment(dateString, 'ddd MMM DD YYYY', 'en').format('YYYY-MM-DD')
    await authFetch(`${API_BASE}/api/attendance/save`, {
      method: 'POST',
      body:   JSON.stringify([{ stagiaireId: user.id, date, statut: newStatus }]),
    })
  } catch (e) { console.error(e) }
}

function getStatusText(user, date) {
  const s = presenceCache.value[`${user.id}__${date}`] ?? user.presences?.[date]
  if (!s || s === 'p') return ''
  if (s === 'r') return 'P'
  return s.toUpperCase()
}

function getStatusClass(user, date) {
  const s   = presenceCache.value[`${user.id}__${date}`] ?? user.presences?.[date] ?? 'p'
  const map = { p: 'status-p', a: 'status-a', e: 'status-e', r: 'status-r', s: 'status-s', v: 'status-v', f: 'status-f' }
  return map[s] || 'status-p'
}

function countStatus(user, status) {
  return currentPeriodDays.value
    .filter(d => !isBeforeParticipation(user, d.fullDate) && !isJourFerie(d.fullDate) && !isVacance(d.fullDate))
    .filter(d => (presenceCache.value[`${user.id}__${d.fullDate}`] ?? user.presences?.[d.fullDate] ?? 'p') === status)
    .length
}

function countFeries(user) {
  return currentPeriodDays.value
    .filter(d => !isBeforeParticipation(user, d.fullDate) && (isJourFerie(d.fullDate) || isVacance(d.fullDate)))
    .length
}

// ─── Taux ─────────────────────────────────────────────────────────────────────
function calculateRates(user) {
  if (!user) return { globalRate: 0 }
  const niveau      = (user.niveau || user.Niveau || '').toLowerCase().trim()
  const isTelecours = niveau.includes('tele') || niveau.includes('télé')
  const sess        = currentSession.value
  const scanStart   = sess?.startDate
    ? moment(sess.startDate).startOf('day')
    : currentPeriodDays.value.length
      ? moment(currentPeriodDays.value[0].fullDate, 'ddd MMM DD YYYY', 'en').startOf('day')
      : moment().startOf('month')
  const scanEnd = sess?.endDate
    ? moment(sess.endDate).endOf('day')
    : currentPeriodDays.value.length
      ? moment(currentPeriodDays.value[currentPeriodDays.value.length - 1].fullDate, 'ddd MMM DD YYYY', 'en').endOf('day')
      : moment().endOf('month')

  let absenceScore = 0
  const cur = scanStart.clone()
  while (cur.isSameOrBefore(scanEnd)) {
    if (cur.isoWeekday() < 6) {
      const dk = cur.clone().locale('en').format('ddd MMM DD YYYY')
      if (!isBeforeParticipation(user, dk) && !isJourFerie(dk) && !isVacance(dk)) {
        const st = presenceCache.value[`${user.id}__${dk}`] ?? user.presences?.[dk] ?? 'p'
        if (st === 'a' || st === 'e') absenceScore += 1
        else if (st === 'r')          absenceScore += 0.5
      }
    }
    cur.add(1, 'day')
  }

  const today               = moment()
  const endOfDisplayedMonth = today.isBefore(scanEnd) ? today : scanEnd
  const monthInSession      = endOfDisplayedMonth.diff(scanStart.clone().startOf('month'), 'months') + 1
  const targets             = isTelecours ? [10, 20, 32] : [20, 40, 60]
  const target              = targets[Math.min(monthInSession, 3) - 1]
  const globalRate          = target > 0 ? Math.max(0, (target - absenceScore) / target * 100) : 100

  return { globalRate }
}

function getRateClass(rate) {
  return rate >= 80 ? 'text-success' : rate >= 50 ? 'text-warning' : 'text-error'
}

// ─── Date début participation ──────────────────────────────────────────────────
function isBeforeParticipation(user, dateString) {
  const raw = user.DateDebutParticipation || user.dateDebutParticipation
  if (!raw || raw === 'undefined' || raw === 'null') return false
  const cell  = moment(dateString, 'ddd MMM DD YYYY', 'en').startOf('day')
  const start = moment(raw).startOf('day')
  return cell.isValid() && start.isValid() && cell.isBefore(start, 'day')
}

function openStartDateModal(user) {
  editingUser.value  = user
  newStartDate.value = user.dateDebutParticipation
    ? moment(user.dateDebutParticipation).format('YYYY-MM-DD')
    : moment().format('YYYY-MM-DD')
  startDateModalVisible.value = true
}

async function saveStartDate() {
  if (!editingUser.value || !newStartDate.value) return
  isLoading.value = true
  const userId = editingUser.value.id
  const date   = moment(newStartDate.value).format('YYYY-MM-DD')
  try {
    const res = await authFetch(`${API_BASE}/api/Users/${userId}`, {
      method: 'PUT',
      body:   JSON.stringify({ Id: userId, DateDebutParticipation: date }),
    })
    if (!res?.ok) { alert('Erreur serveur'); return }
    Object.assign(editingUser.value, { dateDebutParticipation: date })
    store.commit('user/UPDATE_USER', { Id: userId, DateDebutParticipation: date })
    startDateModalVisible.value = false
    await fetchAttendancesForPeriod()
  } catch (e) { alert('Impossible de sauvegarder.') }
  finally { isLoading.value = false }
}

function formatDateFr(d) {
  return moment(d).locale('fr').format('DD/MM/YYYY')
}

// ─── PDF ──────────────────────────────────────────────────────────────────────
async function openModal(user) {
  selectedUser.value   = user
  modalVisible.value   = true
  availableYears.value = [moment().year(), moment().year() - 1]
  const sessionStart   = currentSession.value?.startDate
  selectedYear.value   = sessionStart ? moment(sessionStart).year()      : moment().year()
  selectedMonth.value  = sessionStart ? moment(sessionStart).month() + 1 : moment().month() + 1
  await Promise.resolve()
  const matching          = filteredSessionsForUser.value
  pdfModeTab.value        = matching.length ? 'session' : 'month'
  const currentMatch      = matching.find(s => s.name === currentSession.value?.name)
  selectedSessionId.value = currentMatch?.name || matching[0]?.name || null
}

function getSessionMonthForDate(dateStr, sessionType) {
  const m    = moment(dateStr)
  const sess = calendrier.value.sessions.find(s => {
    const t         = (s.type || '').toLowerCase()
    const typeMatch = sessionType === 'telecours'
      ? t === 'telecours'
      : (t === 'fi_a1' || t === 'fi_a2' || t.startsWith('fi'))
    return typeMatch && s.startDate && s.endDate &&
      m.isBetween(moment(s.startDate).startOf('month'), moment(s.endDate).endOf('month'), 'month', '[]')
  })
  if (sess) {
    const diff = m.diff(moment(sess.startDate).startOf('month'), 'months') + 1
    return Math.max(1, Math.min(3, diff))
  }
  const qStart = Math.floor(m.month() / 3) * 3
  return m.month() - qStart + 1
}

function getNiveauType(userNiveau, userId) {
  const storeUser = users.value.find(u => (u.id || u.Id) === userId)
  const niveau    = (userNiveau || storeUser?.niveau || storeUser?.Niveau || '').toLowerCase().trim()
  const isTele    = niveau.includes('tele') || niveau.includes('télé')
  return { isTele, sType: isTele ? 'telecours' : (niveau.includes('a2') ? 'fi_a2' : 'fi_a1') }
}

function buildAttestationUrl(userId, userNiveau, year, month) {
  const sess = currentSession.value
  const y    = year  ?? (sess ? moment(sess.startDate).year()      : moment().year())
  const mo   = month ?? (sess ? moment(sess.startDate).month() + 1 : moment().month() + 1)
  const { sType } = getNiveauType(userNiveau, userId)
  const sm   = getSessionMonthForDate(moment({ year: y, month: mo - 1 }).format('YYYY-MM-DD'), sType)
  return `${API_BASE}/api/documents/attestationpresence/${userId}?year=${y}&month=${mo}&sessionType=${encodeURIComponent(sType)}&sessionMonth=${sm}`
}

async function downloadAttestation() {
  isLoadingModal.value = true
  try {
    let url
    const { sType } = getNiveauType(selectedUser.value.niveau, selectedUser.value.id)
    if (pdfModeTab.value === 'session' && selectedSessionId.value) {
      const sess = calendrier.value.sessions.find(s => s.name === selectedSessionId.value)
      if (!sess) { alert('Session introuvable.'); return }
      const y  = selectedYear.value  || moment(sess.startDate).year()
      const mo = selectedMonth.value || moment(sess.startDate).month() + 1
      const sm = getSessionMonthForDate(moment(sess.startDate).format('YYYY-MM-DD'), sType)
      url = `${API_BASE}/api/documents/attestationpresence/${selectedUser.value.id}?year=${y}&month=${mo}&sessionName=${encodeURIComponent(sess.name)}&sessionType=${encodeURIComponent(sType)}&sessionMonth=${sm}`
    } else {
      url = buildAttestationUrl(selectedUser.value.id, selectedUser.value.niveau, selectedYear.value, selectedMonth.value)
    }
    const res = await authFetch(url, { headers: { 'Content-Type': '' } })
    if (res?.ok) {
      const blob = await res.blob()
      const link = document.createElement('a')
      link.href  = window.URL.createObjectURL(blob)
      link.download = `Presence-${selectedUser.value.nom?.trim()}.pdf`
      link.click()
    } else { alert('Erreur serveur') }
  } catch (e) { console.error(e) }
  finally { isLoadingModal.value = false; modalVisible.value = false }
}

async function downloadAllAttestations() {
  for (const user of filteredUsers.value) {
    try {
      const url = buildAttestationUrl(user.id, user.niveau)
      const res = await authFetch(url, { headers: { 'Content-Type': '' } })
      if (res?.ok) {
        const blob = await res.blob()
        const link = document.createElement('a')
        link.href  = window.URL.createObjectURL(blob)
        link.download = `Presence-${user.nom?.trim()}-${currentSession.value?.name || navLabel.value}.pdf`
        link.click()
        await new Promise(r => setTimeout(r, 300))
      }
    } catch (e) { console.error(e) }
  }
}

// ─── Fetch présences ──────────────────────────────────────────────────────────
async function fetchAttendancesForPeriod() {
  isLoading.value = true
  let start, end

  if (navMode.value === 'month') {
    const m = moment({ year: navMonthYear.value, month: navMonth.value - 1, day: 1 })
    start = m.format('YYYY-MM-DD')
    end   = m.clone().endOf('month').format('YYYY-MM-DD')
  } else if (currentSession.value?.startDate) {
    start = moment(currentSession.value.startDate).format('YYYY-MM-DD')
    end   = currentSession.value.endDate
      ? moment(currentSession.value.endDate).format('YYYY-MM-DD')
      : moment(currentSession.value.startDate).add(3, 'months').endOf('month').format('YYYY-MM-DD')
  } else {
    start = moment().startOf('month').format('YYYY-MM-DD')
    end   = moment().endOf('month').format('YYYY-MM-DD')
  }

  try {
    const res     = await authFetch(`${API_BASE}/api/attendance/byWeek?start=${start}&end=${end}`)
    const json    = await res?.json()
    const records = Array.isArray(json) ? json : []
    const cache   = { ...presenceCache.value }
    records.forEach(r => {
      const uid  = r.stagiaireId || r.userId || r.StagiaireId || r.UserId
      const date = r.date || r.Date
      const st   = (r.statut || r.Statut || 'p').toLowerCase()
      if (uid && date)
        cache[`${uid}__${moment(date).locale('en').format('ddd MMM DD YYYY')}`] = st
    })
    presenceCache.value = cache
    await store.dispatch('user/updateUserAttendances', {
      attendances:       records,
      currentPeriodDays: currentPeriodDays.value.map(d => d.fullDate),
    })
  } catch (e) { console.error(e) }
  finally { isLoading.value = false }
}
</script>

<style scoped>
.table-container { overflow-x: auto; max-height: 72vh; }
.presence-table  { width: 100%; border-collapse: collapse; font-size: 0.875rem; }

.sticky-col      { position: sticky; left: 0; background-color: white; z-index: 2; box-shadow: 2px 0 4px rgba(0,0,0,0.08); }
.name-col        { min-width: 200px; padding: 10px 12px; text-align: left; }
.sticky-rate-col { left: 200px; background-color: #f8f8f8; z-index: 2; border-right: 2px solid #ddd; }
.actions-col     { position: sticky; right: 0; background-color: #f8f9fa; z-index: 2; min-width: 60px; }

.presence-table th,
.presence-table td { border: 1px solid #e0e0e0; text-align: center; padding: 4px 2px; }
.presence-table thead th { background: #f5f5f5; position: sticky; top: 0; z-index: 3; }
.presence-table thead .sticky-col { z-index: 5; }

.day-header         { min-width: 42px; width: 42px; }
.day-header-content { line-height: 1.3; }
.day-name           { font-size: 0.65rem; color: #888; text-transform: capitalize; }
.day-date           { font-weight: bold; font-size: 0.8rem; }
.day-month          { font-size: 0.6rem; font-weight: 700; color: #1A237E; text-transform: uppercase; border-top: 2px solid #1A237E; margin-top: 1px; padding-top: 1px; }
.ferie-header       { background-color: #FFF9C4 !important; }
.ferie-indicator    { font-size: 0.6rem; font-weight: bold; color: #827717; }
.vacance-indicator  { font-size: 0.6rem; font-weight: bold; color: #2E7D32; }
.rate-col           { min-width: 80px; padding: 6px 8px; }

.presence-cell       { cursor: pointer; height: 38px; width: 42px; transition: all 0.1s; font-weight: bold; font-size: 0.75rem; }
.presence-cell:hover { filter: brightness(0.90); }
.presence-cell.active-cell { box-shadow: inset 0 0 0 3px #FF6F00 !important; z-index: 10; position: relative; }

.status-p { background-color: #1A237E; color: #FFFFFF; }
.status-a { background-color: #FFCDD2; color: #B71C1C; }
.status-e { background-color: #FFE0B2; color: #E65100; }
.status-r { background-color: #B3E5FC; color: #01579B; }
.status-s { background-color: #C8E6C9; color: #1B5E20; }
.status-v { background-color: #1B5E20; color: #FFFFFF; }
.status-f { background-color: #FFF9C4; color: #827717; }

.presence-cell.not-started {
  background: repeating-linear-gradient(45deg,#eee,#eee 5px,#e0e0e0 5px,#e0e0e0 10px);
  cursor: not-allowed; opacity: 0.5;
}

.stats-col   { min-width: 130px; }
.stats-inner { display: flex; gap: 6px; align-items: center; justify-content: center; flex-wrap: wrap; padding: 2px; }

.stat-value          { font-weight: bold; font-size: 0.72rem; }
.stat-value.present  { color: #1A237E; }
.stat-value.absent   { color: #B71C1C; }
.stat-value.excused  { color: #E65100; }
.stat-value.retard   { color: #01579B; }
.stat-value.stage    { color: #1B5E20; }
.stat-value.vacances { color: #1B5E20; }
.stat-value.ferie    { color: #827717; }

.legend-bar  { background: #fafafa; }
.legend-item { display: inline-block; padding: 2px 10px; border-radius: 12px; font-size: 0.72rem; font-weight: 600; margin-right: 10px; }

.user-info { padding: 4px 8px; }
.user-info .name      { font-weight: 700; color: #1a1a1a; }
.user-info .trigramme { font-size: 0.72rem; color: #999; text-transform: uppercase; }
.clickable-date:hover { text-decoration: underline; background: rgba(0,0,0,0.04); border-radius: 4px; }
</style>