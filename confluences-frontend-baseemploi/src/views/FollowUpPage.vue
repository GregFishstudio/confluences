<template>
  <v-container fluid class="pa-2 pa-md-4 follow-up-sheet">

    <v-card class="mb-4 elevation-2" rounded="lg">
      <v-card-text class="pa-4">
        <div class="d-flex justify-space-between align-center">
          <v-btn icon @click="previousQuarter" :disabled="isLoading">
            <v-icon>mdi-chevron-left</v-icon>
          </v-btn>
          <div class="text-center">
            <div class="text-h6 font-weight-bold text-primary">{{ currentPeriodLabel }}</div>
              <div class="text-caption text-grey">{{ periodDateLabel }}</div>  <!-- Ajouter cette ligne -->
          </div>
          <v-btn icon @click="nextQuarter" :disabled="isCurrentQuarter || isLoading">
            <v-icon>mdi-chevron-right</v-icon>
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-tabs v-model="activeTab" class="px-4" color="primary" @update:modelValue="onTabChange">
        <v-tab v-for="tab in tabOptions" :key="tab.value">
          {{ tab.label }}
          <v-chip size="x-small" class="ml-2" color="primary" variant="outlined">
            {{ tabCounts[tab.value] || 0 }}
          </v-chip>
        </v-tab>
      </v-tabs>
    </v-card>

    <v-card class="elevation-4" rounded="lg" :loading="isLoading">
      <v-row align="center" class="px-2">
        <v-col cols="12" md="6" class="pa-3 pa-md-6">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un participant..."
            variant="outlined" density="compact" clearable hide-details
            bg-color="white"
          />
        </v-col>
      </v-row>
      <v-divider></v-divider>

      <!-- ── Vue MOBILE : cartes ── -->
      <v-card-text class="pa-2 d-md-none">
        <div v-if="!isLoading && finalFilteredUsers.length === 0" class="text-center pa-8 text-grey">
          <v-icon size="large" color="grey-lighten-1">mdi-account-search-outline</v-icon>
          <div class="mt-2">Aucun participant trouvé.</div>
        </div>
        <v-card
          v-for="user in finalFilteredUsers" :key="'m'+user.id"
          class="mb-4 elevation-2" rounded="xl"
        >
          <!-- En-tête participant -->
          <div class="d-flex align-center px-4 pt-4 pb-3 gap-3">
            <v-avatar color="primary" size="38">
              <span class="text-white font-weight-bold text-body-2">
                {{ (user.nom||'A').charAt(0) }}{{ (user.prenom||'A').charAt(0) }}
              </span>
            </v-avatar>
            <div class="flex-grow-1 text-truncate-parent">
              <div class="font-weight-bold text-body-2 participant-name-link" @click="openParticipantPage(user)">{{ user.nom }} {{ user.prenom }}</div>
              <div class="text-caption text-grey text-uppercase">{{ user.userName }}</div>
              <v-chip size="x-small" variant="tonal" class="mt-1">
                {{ getUserSessionInfo(user) || user.mesureType }}
              </v-chip>
            </div>
            <div class="text-right flex-shrink-0 ml-2">
              <div class="text-caption text-grey">Mesure</div>
              <div class="text-caption font-weight-bold text-primary" style="max-width:80px">
                {{ user.courseDetails.isUnknown ? '—' : user.courseDetails.mainInfo }}
              </div>
            </div>
          </div>

          <v-divider />

          <!-- Niveaux -->
          <div class="d-flex gap-3 px-4 py-3">
            <div class="flex-grow-1">
              <div class="text-caption text-grey mb-1 font-weight-bold">Oral</div>
              <v-select
                :key="`${user.id}-oral-${currentPeriod.key}`"
                :model-value="user.oralLevel || ''"
                :items="levels" density="compact" variant="outlined" hide-details
                @update:model-value="(val) => saveLevel(user.id, 'oralLevel', val)"
                placeholder="-"
              />
            </div>
            <div class="flex-grow-1">
              <div class="text-caption text-grey mb-1 font-weight-bold">Écrit</div>
              <v-select
                :key="`${user.id}-written-${currentPeriod.key}`"
                :model-value="user.writtenLevel || ''"
                :items="levels" density="compact" variant="outlined" hide-details
                @update:model-value="(val) => saveLevel(user.id, 'writtenLevel', val)"
                placeholder="-"
              />
            </div>
          </div>

          <v-divider />

          <!-- Actions -->
          <div class="d-flex gap-3 px-4 py-3">
            <v-btn color="primary" variant="tonal" size="small" rounded="lg"
              prepend-icon="mdi-file-document-edit-outline"
              class="flex-grow-1 text-none" @click="openBilanModal(user)">
              Bilan
            </v-btn>
            <v-btn color="secondary" variant="tonal" size="small" rounded="lg"
              prepend-icon="mdi-file-chart-outline"
              class="flex-grow-1 text-none" @click="downloadPdf('bilan', user)">
              PDF
            </v-btn>
            <v-btn color="secondary" variant="tonal" size="small" rounded="lg"
              prepend-icon="mdi-certificate-outline"
              class="flex-grow-1 text-none" @click="downloadPdf('attestation', user)">
              Attest.
            </v-btn>
          </div>
        </v-card>
      </v-card-text>

      <!-- ── Vue DESKTOP : tableau ── -->
      <v-card-text class="pa-0 d-none d-md-block">
        <div class="table-container">
          <table class="follow-up-table">
            <thead>
              <tr>
                <th class="sticky-col name-col">Participant·e</th>
                <th class="saisie-col">Niveau Oral</th>
                <th class="saisie-col">Niveau Écrit</th>
                <th
                  v-for="activity in activities"
                  :key="activity.id"
                  class="activity-col"
                  :style="{ minWidth: activity.type === 'details' ? '250px' : '140px' }"
                >
                  {{ activity.libelle }}
                  <v-tooltip location="bottom" v-if="activity.description">
                    <template #activator="{ props }">
                      <v-icon size="small" class="ml-1" v-bind="props">mdi-information</v-icon>
                    </template>
                    <span>{{ activity.description }}</span>
                  </v-tooltip>
                </th>
                <th class="sticky-col actions-col">Bilan</th>
                <th class="sticky-col actions-col">Attestation</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="user in finalFilteredUsers" :key="user.id">

                <td class="sticky-col name-col">
                  <div class="user-info">
                    <div class="name participant-name-link" @click="openParticipantPage(user)">{{ user.prenom }} {{ user.nom }}</div>
                    <div class="trigramme">{{ user.userName }}</div>
                    <div class="text-caption font-weight-bold" style="color:#1565C0">
                      {{ getUserSessionInfo(user) }}
                    </div>
                  </div>
                </td>

                <td class="saisie-col">
                  <v-select
                    :key="`${user.id}-oral-${currentPeriod.key}`"
                    :model-value="user.oralLevel || ''"
                    :items="levels"
                    density="compact" variant="outlined" hide-details
                    @update:model-value="(val) => saveLevel(user.id, 'oralLevel', val)"
                    style="width: 100px; margin: 0 auto;"
                    placeholder="-"
                  />
                </td>

                <td class="saisie-col">
                  <v-select
                    :key="`${user.id}-written-${currentPeriod.key}`"
                    :model-value="user.writtenLevel || ''"
                    :items="levels"
                    density="compact" variant="outlined" hide-details
                    @update:model-value="(val) => saveLevel(user.id, 'writtenLevel', val)"
                    style="width: 100px; margin: 0 auto;"
                    placeholder="-"
                  />
                </td>

                <td v-for="activity in activities" :key="activity.key" class="saisie-col">
                  <div class="d-flex justify-center" style="flex-direction: column; align-items: center;">

                    <div v-if="activity.type === 'details'" class="text-left pa-2" style="max-width: 95%;">
                      <div v-if="user.courseDetails.isUnknown">
                        <div class="text-caption text-error font-weight-bold mb-1">
                          <v-icon size="x-small" color="error">mdi-alert-circle</v-icon> Aucune formation
                        </div>
                        <a href="https://cours.confluences.ch" target="_blank" class="text-caption text-decoration-underline text-primary">
                          Ajouter <v-icon size="x-small">mdi-open-in-new</v-icon>
                        </a>
                      </div>
                      <div v-else>
                        <v-chip-group column>
                          <v-chip size="x-small" color="primary" variant="outlined" class="font-weight-bold">
                            {{ user.courseDetails.mainInfo }}
                          </v-chip>
                          <v-chip v-for="course in user.courseDetails.subCourses" :key="course" size="x-small" color="grey-lighten-3">
                            {{ course }}
                          </v-chip>
                        </v-chip-group>
                      </div>
                    </div>

                    <div v-else-if="activity.type === 'rate'" class="d-flex flex-column align-center">
                      <v-text-field
                        :model-value="user.followUp.sessionAttendanceRate.rate.toFixed(1) + '%'"
                        density="compact" variant="outlined" hide-details readonly
                        class="centered-input"
                        style="width: 80px;"
                        :class="user.followUp.sessionAttendanceRate.hasData ? getRateClass(user.followUp.sessionAttendanceRate.rate) : 'text-grey-darken-1'"
                      />
                    </div>

                    <div v-else-if="activity.type === 'summary'" class="d-flex flex-column align-center">
                      <div class="text-h6 font-weight-bold text-primary mb-0">{{ user.accompanyingDaysCount || 0 }}</div>
                    </div>

                  </div>
                </td>

               <td class="sticky-col actions-col">
  <div class="d-flex flex-column ga-2 pa-2">
    
    <v-btn
      size="x-small"
      color="primary"
      variant="elevated"  elevation="4"       @click="openBilanModal(user)"
      prepend-icon="mdi-file-document-edit-outline"
      class="text-none justify-start w-100"
      style="min-height: 40px;"  >
      
      Bilan
    </v-btn>

    <v-btn
      size="x-small"
      color="secondary"
      variant="elevated"
      elevation="4"
      @click="downloadPdf('bilan', user)"
      prepend-icon="mdi-file-chart-outline"
      class="text-none justify-start w-100"
      style="min-height: 40px;"
    >
      PDF Bilan
    </v-btn>
  </div>
</td>

                <td class="sticky-col actions-col">
  <div class="d-flex justify-center pa-2">
    <v-btn
      size="x-small"
      color="secondary"
      variant="elevated"
      elevation="4"
      @click="downloadPdf('attestation', user)"
      prepend-icon="mdi-certificate-outline"
      class="text-none w-100"
      style="min-height: 40px;"
    >
      PDF Attestation
    </v-btn>
  </div>
</td>

              </tr>
              <tr v-if="!isLoading && finalFilteredUsers.length === 0">
                <td colspan="10" class="text-center pa-8 text-grey">
                  <v-icon size="large" color="grey-lighten-1">mdi-account-search-outline</v-icon>
                  <div class="mt-2">Aucun participant trouvé pour cette sélection.</div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </v-card-text>
    </v-card>

    <BilanEditModal
      v-model="bilanModalVisible"
      :stagiaire-data="selectedUser"
      :period-info="currentPeriod"
      @bilan-saved="handleBilanSaved"
    />

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import moment from 'moment/min/moment-with-locales'
import BilanEditModal from '../components/BilanEditModal.vue'
import API_BASE, { apiFetch } from '@/services/api'

moment.locale('fr')

const store = useStore()
const router = useRouter()

function openParticipantPage(user) {
  const resolved = router.resolve({ name: 'Participant-Detail', params: { id: user.id } })
  window.open(resolved.href, '_blank')
}

// ─── State ────────────────────────────────────────────────────────────────────
const search               = ref('')
const activeTab            = ref(0)
const isLoading            = ref(false)
const bilanModalVisible    = ref(false)
const selectedUser         = ref(null)
const periodLevelsMap      = ref({})
const navSessionIndex      = ref(0)
const joursFeries          = ref([])
const periodesVacances     = ref([])

const allSessionLinks      = ref([])

const tabOptions = [
  { label: 'Télécours', value: 'telecours' },
  { label: 'F&I A1',    value: 'fi_a1'    },
  { label: 'F&I A2',    value: 'fi_a2'    },
  //{ label: 'Archives',  value: 'archives' },
]

// Mapping formateur → onglet prioritaire à l'ouverture (clé = preferred_username en minuscules)
const FORMATEUR_GROUPS = {
  'myriam':        'telecours',
  'mls':           'fi_a2',
  'sac':           'fi_a2',
  'cs':            'fi_a1',
  'mec':           'fi_a1',
}

const levels = ['A1.1','A1.2','A2.1','A2.2','B1.1','B1.2','B2.1','B2.2','C1.1','C1.2']

const activities = [
  { id: 1, key: 'courseDetails', libelle: 'Mesure', type: 'details', description: 'Classes et cours rattachés.' },
]

// ─── Store ────────────────────────────────────────────────────────────────────
const users = computed(() => store.state.user?.users || [])

// ─── Computed période ─────────────────────────────────────────────────────────
const currentSessionDates = computed(() => {
  if (currentSession.value?.startDate) {
    const start = moment(currentSession.value.startDate).startOf('day')
    const end   = currentSession.value.endDate
      ? moment(currentSession.value.endDate).endOf('day')
      : start.clone().add(3, 'months').endOf('month')
    return {
      start:    start.toDate(),
      end:      end.toDate(),
      startKey: start.format('YYYY-MM-DD'),
      endKey:   end.format('YYYY-MM-DD'),
    }
  }
  // Fallback si aucune session dans le calendrier
  const start = moment().startOf('month')
  const end   = moment().endOf('month')
  return { start: start.toDate(), end: end.toDate(), startKey: start.format('YYYY-MM-DD'), endKey: end.format('YYYY-MM-DD') }
})

const currentPeriodLabel = computed(() =>
  `Session ${currentQuarter.value} — ${currentYear.value}`
)

// FollowUp.vue — remplacer les computed liés aux dates

const calendrier = ref({ sessions: [], feries: [], vacances: [] })

const loadCalendrier = async () => {
  const res = await fetch(`${API_BASE}/api/calendrier`, {
    headers: { Authorization: `Bearer ${localStorage.getItem('access_token')}` }
  })
  if (res.ok) calendrier.value = await res.json()
}

// ─── Sessions du groupe actif triées par date de début ────────────────────────
const activeGroupSessions = computed(() => {
  const groupValue = tabOptions[activeTab.value].value
  return calendrier.value.sessions
    .filter(s => {
      const t = (s.type || '').toLowerCase()
      if (!s.startDate) return false
      if (groupValue === 'telecours') return t === 'telecours'
      if (groupValue === 'fi_a1')    return t === 'fi_a1'
      if (groupValue === 'fi_a2')    return t === 'fi_a2'
      return false
    })
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
})

const currentSession = computed(() =>
  activeGroupSessions.value[navSessionIndex.value] || null
)

const currentYear    = computed(() =>
  currentSession.value?.startDate ? moment(currentSession.value.startDate).year() : moment().year()
)
const currentQuarter = computed(() => navSessionIndex.value + 1)

// Identique à Accompanying.vue
const periodDateLabel = computed(() => {
  if (currentSession.value?.startDate) {
    const s = moment(currentSession.value.startDate)
    const e = currentSession.value.endDate ? moment(currentSession.value.endDate) : null
    if (!e) return `À partir du ${s.locale('fr').format('DD MMM YYYY')}`
    return `${s.locale('fr').format('DD MMM')} – ${e.locale('fr').format('DD MMM YYYY')}`
  }
  // Fallback
  const monthNames = ['janvier','février','mars','avril','mai','juin',
                      'juillet','août','septembre','octobre','novembre','décembre']
  const startMonth = (currentQuarter.value - 1) * 3
  return `${monthNames[startMonth]} – ${monthNames[startMonth + 2]} ${currentYear.value}`
})

const isCurrentQuarter = computed(() =>
  navSessionIndex.value >= activeGroupSessions.value.length - 1
)

const currentPeriod = computed(() => ({
  year:    currentYear.value,
  quarter: currentQuarter.value,
  key:     `${currentYear.value}-${currentQuarter.value}`,
}))

// ─── Filtrage ─────────────────────────────────────────────────────────────────
const finalFilteredUsers = computed(() => {
  let result = users.value.map(user => {
    const rateDetails  = computeRate(user)
    const courseDetails = getCourseDetails(user)
    const className     = (courseDetails.mainInfo || '').toLowerCase()

    const isTelecours = className.includes('télécours') || className.includes('telecours')
    const isA1        = className.includes('a1')
    const isA2        = className.includes('a2')
    const isFI        = !isTelecours && (isA1 || isA2)

    let mesureType = 'Autre'
    if (isTelecours)       mesureType = 'telecours'
    else if (isFI && isA1) mesureType = 'fi_a1'
    else if (isFI && isA2) mesureType = 'fi_a2'

    const periodLevels = periodLevelsMap.value[user.id] || {}


    return {
      ...user,
      courseDetails,
      mesureType,
      oralLevel:    periodLevels.oralLevel    || '',
      writtenLevel: periodLevels.writtenLevel || '',
      followUp: {
        sessionAttendanceRate: {
          rate:               rateDetails.attendanceRate,
          hasData:            rateDetails.hasPresenceData,
          totalAbsencePenalty: rateDetails.totalAbsencePenalty,
        },
      },
      globalRate:           rateDetails.globalRate,
    }
  })

  const filterValue = tabOptions[activeTab.value].value
  result = result.filter(user => matchesGroup(user, filterValue))

  if (search.value) {
    const s = search.value.toLowerCase()
    result = result.filter(user =>
      (user.nom     || '').toLowerCase().includes(s) ||
      (user.prenom  || '').toLowerCase().includes(s) ||
      (user.userName || '').toLowerCase().includes(s)
    )
  }
result = result.sort((a, b) =>
    (a.nom || '').localeCompare((b.nom || ''), 'fr', { sensitivity: 'base' })
  )
  return result
})

const tabCounts = computed(() => {
  const counts = { telecours: 0, fi_a1: 0, fi_a2: 0, archives: 0 }
  users.value.forEach(user => {
    tabOptions.forEach(tab => {
      if (matchesGroup(user, tab.value)) counts[tab.value]++
    })
  })
  return counts
})

// ─── Helpers ──────────────────────────────────────────────────────────────────

// Jours ouvrés du trimestre pré-calculés une seule fois par période
// Évite de recréer des objets moment() dans chaque appel computeRate
const workingDaysCache = computed(() => {
  const days = []
  const startPeriod = moment(currentSessionDates.value.start)
  const endPeriod   = moment(currentSessionDates.value.end)
  const jfSet       = new Set(joursFeries.value)
  let curr = startPeriod.clone()

  while (curr.isSameOrBefore(endPeriod, 'day')) {
    const dateStr  = curr.format('YYYY-MM-DD')
    const isWorking = curr.isoWeekday() <= 5 && !jfSet.has(dateStr)
    const isVac     = periodesVacances.value.some(v => curr.isBetween(v.start, v.end, 'day', '[]'))

    if (isWorking && !isVac) {
      days.push({
        dateStr,
        dateKey: curr.clone().locale('en').format('ddd MMM DD YYYY'),
        ts:      curr.valueOf(), // timestamp pour comparaison rapide
      })
    }
    curr.add(1, 'day')
  }
  return days
})

const computeRate = (user) => {
  const participationTs = user.dateDebutParticipation
    ? moment(user.dateDebutParticipation).startOf('day').valueOf()
    : 0
  const todayTs = moment().endOf('day').valueOf()

  let dGlobal = 0, dActive = 0, pen = 0, penActive = 0, hasData = false

  for (const day of workingDaysCache.value) {
    dGlobal++
    const status = user.presences?.[day.dateKey]
    const p = status === 'a' ? 1 : status === 'r' ? 0.5 : 0
    if (p > 0) { pen += p; hasData = true }

    if (day.ts >= participationTs && day.ts <= todayTs) {
      dActive++
      penActive += p
    }
  }

  return {
    attendanceRate:      dActive > 0 ? ((dActive - penActive) / dActive) * 100 : 100,
    globalRate:          dGlobal > 0 ? ((dGlobal - pen) / dGlobal) * 100 : 100,
    hasPresenceData:     hasData,
    totalAbsencePenalty: penActive,
  }
}

const getCourseDetails = (user) => {
  // Utilise le niveau normalisé qui vient du store (déjà extrait de la session la plus récente)
  const niveau = user.niveau || ''
  if (!niveau || niveau === 'N/A') return { mainInfo: '', subCourses: [], isUnknown: true }
  return { mainInfo: niveau, subCourses: [], isUnknown: false }
}

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
  return s ? `Session ${s.sessionNumberId || s.SessionNumberId}` : ''
}

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
    return Number(sessionYear) === Number(currentYear.value) &&
           Number(sessionNumber) === Number(currentQuarter.value)
  })
  if (!hasActiveSession) return false

  // Niveau depuis le champ normalisé en camelCase par SET_USERS
  const niveauUser  = (user.niveau || '').toLowerCase().trim()
  const isTelecours = niveauUser.includes('tele') || niveauUser.includes('télé')

  if (groupValue === 'telecours') return isTelecours
  if (groupValue === 'fi_a1')    return !isTelecours && niveauUser.includes('a1')
  if (groupValue === 'fi_a2')    return !isTelecours && niveauUser.includes('a2')
  if (groupValue === 'archives') return user.isArchived === true
  return false
}

// ─── API ──────────────────────────────────────────────────────────────────────
const loadAllSessionLinks = async () => {
  try {
    const res = await fetch(`${API_BASE}/api/SessionStudents`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('access_token')}` },
    })
    if (res.ok) {
      allSessionLinks.value = await res.json()
    }
  } catch (e) { console.error('Erreur liens session:', e) }
}


const fetchDataForPeriod = async () => {
  isLoading.value       = true
  periodLevelsMap.value = {}
  const token           = localStorage.getItem('access_token')
  const { startKey, endKey } = currentSessionDates.value

  try {
    const [resAtt, resLevels] = await Promise.all([
      fetch(`${API_BASE}/api/attendance/byWeek?start=${startKey}&end=${endKey}`, { headers: { Authorization: `Bearer ${token}` } }),
      fetch(`${API_BASE}/api/stagiaires/levels?year=${currentYear.value}&quarter=${currentQuarter.value}`, { headers: { Authorization: `Bearer ${token}` } }),
    ])

    if (resAtt.ok) {
      store.dispatch('user/updateUserAttendances', {
        attendances:       await resAtt.json(),
        currentPeriodDays: [],
      })
    }
    if (resLevels.ok) {
      const data = await resLevels.json()
      const map  = {}
      data.forEach(l => { map[l.stagiaireId] = { oralLevel: l.oralLevel, writtenLevel: l.writtenLevel } })
      periodLevelsMap.value = map
    }
  } catch (e) { console.error(e) } finally { isLoading.value = false }
}

const saveLevel = async (userId, field, newLevel) => {
  if (!periodLevelsMap.value[userId]) periodLevelsMap.value[userId] = {}
  periodLevelsMap.value[userId][field] = newLevel
  try {
    await fetch(`${API_BASE}/api/stagiaires/${userId}/level`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${localStorage.getItem('access_token')}` },
      body:    JSON.stringify({ field, value: newLevel, year: currentYear.value, quarter: currentQuarter.value }),
    })
  } catch (e) { console.error(e) }
}

const downloadPdf = async (type, user) => {
  const endpoint = type === 'bilan' ? 'bilan-session' : 'attestation-session'
  const sess = currentSession.value
  const params = new URLSearchParams({
    year:     currentYear.value,
    trimestre: currentQuarter.value,
    mesure:   user.mesureType || '',
  })
  if (sess?.startDate) params.set('startDate', sess.startDate.substring(0, 10))
  if (sess?.endDate)   params.set('endDate',   sess.endDate.substring(0, 10))
  const url = `${API_BASE}/api/documents/${endpoint}/${user.id}?${params}`
  try {
    isLoading.value = true
    const res = await fetch(url, { headers: { Authorization: `Bearer ${localStorage.getItem('access_token')}` } })
    if (res.ok) {
      const blob = await res.blob()
      const link = document.createElement('a')
      link.href  = window.URL.createObjectURL(blob)
      link.download = `${type}-${user.nom}-${currentYear.value}-T${currentQuarter.value}.pdf`
      link.click()
    } else if (res.status === 404 && type === 'bilan') {
      alert("Aucun bilan enregistré pour ce participant. Veuillez d'abord saisir un bilan.")
    } else {
      alert('Erreur lors de la génération du PDF.')
    }
  } catch (e) { alert('Erreur PDF') } finally { isLoading.value = false }
}

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

const previousQuarter = () => {
  if (navSessionIndex.value > 0) { navSessionIndex.value--; fetchDataForPeriod() }
}

const nextQuarter = () => {
  if (navSessionIndex.value < activeGroupSessions.value.length - 1) { navSessionIndex.value++; fetchDataForPeriod() }
}

const onTabChange = () => { initSessionIndex(); fetchDataForPeriod() }

const openBilanModal = (user) => {
  selectedUser.value = { ...user, mesureType: user.mesureType }
  bilanModalVisible.value = true
}

const handleBilanSaved = () => { bilanModalVisible.value = false; fetchDataForPeriod() }
const getRateClass = (r) => r >= 80 ? 'text-success' : r >= 50 ? 'text-warning' : 'text-error'

onMounted(async () => {
  joursFeries.value      = JSON.parse(localStorage.getItem('joursFeries')      || '[]')
  periodesVacances.value = JSON.parse(localStorage.getItem('periodesVacances') || '[]')

  try { await store.dispatch('user/fetchUsers', true) } catch (e) { console.error('Erreur chargement users:', e) }

  // Auto-sélectionner l'onglet du groupe selon le formateur connecté
  const storedUser   = JSON.parse(localStorage.getItem('user') || '{}')
  const prefUsername = (storedUser.profile?.preferred_username || '').toLowerCase()
  const defaultGroup = FORMATEUR_GROUPS[prefUsername]
  if (defaultGroup) {
    const idx = tabOptions.findIndex(t => t.value === defaultGroup)
    if (idx >= 0) activeTab.value = idx
  }

  await Promise.all([loadAllSessionLinks(), loadCalendrier()])
  initSessionIndex()
  fetchDataForPeriod()
})
</script>

<style scoped>
.table-container { overflow-x: auto; max-height: 70vh; }
.follow-up-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.follow-up-table th, .follow-up-table td { border: 1px solid #e0e0e0; padding: 10px 6px; text-align: center; vertical-align: middle; }
.sticky-col { position: sticky; background: white; z-index: 3; }
.name-col { min-width: 200px; left: 0; text-align: left; }
.actions-col { min-width: 80px; right: 0; background: #fafafa; }
.activity-col { background: #fdfdfd; position: sticky; top: 0; z-index: 4; font-weight: 600; }
.saisie-col { min-width: 100px; }
.synthesis-col { min-width: 110px; background: #f9f9f9; }
.user-info .name { font-weight: bold; color: #2c3e50; }
.participant-name-link { cursor: pointer; }
.participant-name-link:hover { text-decoration: underline; color: #1976D2; }
.user-info .trigramme { font-size: 0.75rem; color: #999; text-transform: uppercase; }
.centered-input :deep(input) { text-align: center; font-weight: bold; font-size: 0.9rem; }
.text-truncate-parent { min-width: 0; overflow: hidden; }
</style>