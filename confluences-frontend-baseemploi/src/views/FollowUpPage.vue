<template>
  <v-container fluid class="pa-4 follow-up-sheet">

    <v-card class="mb-4 elevation-2" rounded="lg">
      <v-card-text class="pa-4">
        <div class="d-flex justify-space-between align-center">
          <v-btn icon @click="previousQuarter" :disabled="isLoading">
            <v-icon>mdi-chevron-left</v-icon>
          </v-btn>
          <div class="text-center">
            <div class="text-h6 font-weight-bold text-primary">{{ currentPeriodLabel }}</div>
          </div>
          <v-btn icon @click="nextQuarter" :disabled="isCurrentQuarter || isLoading">
            <v-icon>mdi-chevron-right</v-icon>
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-tabs v-model="activeTab" class="px-4" color="primary">
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
        <v-col cols="12" md="6" class="pa-6">
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

      <v-card-text class="pa-0">
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
                    <div class="name">{{ user.nom }} {{ user.prenom }}</div>
                    <div class="trigramme">{{ user.userName }}</div>
                    <div class="text-caption font-weight-bold text-medium-emphasis">{{ user.niveau }}</div>
                    <div class="text-caption font-weight-bold text-primary">{{ getUserSessionInfo(user) }} ({{ currentYear }})</div>
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
import moment from 'moment/min/moment-with-locales'
import BilanEditModal from '../components/BilanEditModal.vue'
import API_BASE, { apiFetch } from '@/services/api'

moment.locale('fr')

const store = useStore()

// ─── State ────────────────────────────────────────────────────────────────────
const search               = ref('')
const activeTab            = ref(0)
const isLoading            = ref(false)
const bilanModalVisible    = ref(false)
const selectedUser         = ref(null)
const periodLevelsMap      = ref({})
const currentYear          = ref(moment().year())
const currentQuarter       = ref(Math.floor(moment().month() / 3) + 1)
const joursFeries          = ref([])
const periodesVacances     = ref([])

const allSessionLinks      = ref([])

const tabOptions = [
  { label: 'Télécours', value: 'telecours' },
  { label: 'F&I A1',    value: 'fi_a1'    },
  { label: 'F&I A2',    value: 'fi_a2'    },
  //{ label: 'Archives',  value: 'archives' },
]

const levels = ['A1.1','A1.2','A2.1','A2.2','B1.1','B1.2','B2.1','B2.2','C1.1','C1.2']

const activities = [
  { id: 1, key: 'courseDetails', libelle: 'Mesure', type: 'details', description: 'Classes et cours rattachés.' },
]

// ─── Store ────────────────────────────────────────────────────────────────────
const users = computed(() => store.state.user?.users || [])

// ─── Computed période ─────────────────────────────────────────────────────────
const currentSessionDates = computed(() => {
  const startMonth = (currentQuarter.value - 1) * 3
  const start = moment([currentYear.value, startMonth, 1]).startOf('month')
  const end   = moment([currentYear.value, startMonth + 2, 1]).endOf('month')
  return {
    start:    start.toDate(),
    end:      end.toDate(),
    startKey: start.format('YYYY-MM-DD'),
    endKey:   end.format('YYYY-MM-DD'),
  }
})

const currentPeriodLabel = computed(() =>
  `Session ${currentQuarter.value} — ${currentYear.value}`
)

const periodDateLabel = computed(() => {
  const start = moment(currentSessionDates.value.start).locale('fr').format('DD MMM')
  const end   = moment(currentSessionDates.value.end).locale('fr').format('DD MMM YYYY')
  return `${start} - ${end}`
})

const isCurrentQuarter = computed(() => {
  const today = moment()
  return currentQuarter.value === Math.floor(today.month() / 3) + 1 &&
         currentYear.value === today.year()
})

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
  const url = `${API_BASE}/api/documents/${endpoint}/${user.id}?year=${currentYear.value}&trimestre=${currentQuarter.value}&mesure=${encodeURIComponent(user.mesureType)}`
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

const previousQuarter = () => {
  currentQuarter.value--
  if (currentQuarter.value < 1) { currentQuarter.value = 4; currentYear.value-- }
  fetchDataForPeriod()
}

const nextQuarter = () => {
  currentQuarter.value++
  if (currentQuarter.value > 4) { currentQuarter.value = 1; currentYear.value++ }
  fetchDataForPeriod()
}

const openBilanModal = (user) => {
  selectedUser.value = { ...user, mesureType: user.mesureType }
  bilanModalVisible.value = true
}

const handleBilanSaved = () => { bilanModalVisible.value = false; fetchDataForPeriod() }
const getRateClass = (r) => r >= 80 ? 'text-success' : r >= 50 ? 'text-warning' : 'text-error'

onMounted(async () => {
  joursFeries.value      = JSON.parse(localStorage.getItem('joursFeries')      || '[]')
  periodesVacances.value = JSON.parse(localStorage.getItem('periodesVacances') || '[]')

  try {
    await store.dispatch('user/fetchUsers', true)
  } catch (e) {
    console.error('Erreur chargement users:', e)
  }
  loadAllSessionLinks()
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
.user-info { padding: 4px 8px; }
.user-info .name { font-weight: 700; color: #1a1a1a; }
.user-info .trigramme { font-size: 0.72rem; color: #999; text-transform: uppercase; }
.centered-input :deep(input) { text-align: center; font-weight: bold; font-size: 0.9rem; }
</style>