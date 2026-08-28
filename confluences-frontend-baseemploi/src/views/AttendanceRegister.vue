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
            <div class="d-flex justify-center flex-wrap gap-1 mt-2" v-if="currentSessionParts.length">
              <v-chip v-for="part in currentSessionParts" :key="part.index"
                :color="part.index === currentPartIndex ? 'primary' : 'default'"
                :variant="part.index === currentPartIndex ? 'flat' : 'outlined'"
                size="small" style="cursor:pointer;font-size:0.72rem;margin-right:5px"
                @click="goToPart(part.index)">
                P{{ part.index + 1 }}
                <span class="ml-1" style="opacity:0.65;font-size:0.65rem">{{ part.weekCount }}sem</span>
              </v-chip>
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
          <v-chip x-small class="ml-2" color="primary" variant="outlined">{{ groupCount(g.value) }}</v-chip>
        </v-tab>
      </v-tabs>
    </v-card>

    <!-- Tableau principal -->
    <v-card class="elevation-4" rounded="lg" :loading="isLoading">
      <v-row align="center" class="px-2">
        <v-col cols="12" md="5" class="pa-6">
          <v-text-field v-model="search" append-icon="mdi-magnify"
            label="Rechercher un·e participant·e..." variant="outlined"
            density="compact" clearable hide-details bg-color="white" />
        </v-col>
        <v-col cols="12" md="7" class="d-flex flex-wrap justify-md-end align-center gap-2 pa-2">
          <!-- Mode sélection multiple -->
          <v-btn size="small"
            :variant="selectionMode ? 'flat' : 'outlined'"
            :color="selectionMode ? 'warning' : 'default'"
            @click="toggleSelectionMode">
            <v-icon start>mdi-cursor-default-click</v-icon>
            {{ selectionMode ? `${selectedCells.size} case(s) sélectionnée(s)` : 'Sélection multiple' }}
          </v-btn>
          <v-btn v-if="selectionMode && selectedCells.size > 0"
            size="small" variant="flat" color="primary"
            @click="openBulkStatusPicker">
            <v-icon start>mdi-check-all</v-icon> Appliquer statut
          </v-btn>
          <v-btn size="small" variant="text" :to="{ name: 'calendrier' }">
            <v-icon start>mdi-calendar-edit</v-icon> Calendrier
          </v-btn>
          <v-btn size="small" variant="outlined" color="primary"
            @click="downloadAllAttestations"
            :loading="!!downloadAllProgress"
            :disabled="!!downloadAllProgress">
            <v-icon start>mdi-download-multiple</v-icon>
            {{ downloadAllProgress ? `${downloadAllProgress.current}/${downloadAllProgress.total}` : 'Imprimer tous' }}
          </v-btn>
        </v-col>
      </v-row>

      <!-- Aide clavier (desktop uniquement) 
      <div v-if="activeCell && !isTouchDevice()" class="keyboard-hint pa-2 px-4">
        <v-icon size="x-small" class="mr-1">mdi-keyboard</v-icon>
        Cellule sélectionnée — tapez :
        <span v-for="b in currentStatusButtons" :key="b.key"
          class="hint-key" :style="{ background: b.bg, color: b.color }">
          {{ b.kbdKey }} = {{ b.label }}
        </span>
        <span class="hint-key" style="background:#eee;color:#333">Suppr = Présent</span>
      </div>-->

      <v-divider></v-divider>

      <!-- Légende -->
      <div class="d-flex flex-wrap gap-2 pa-3 legend-bar">
        <span v-for="s in currentStatusLegend" :key="s.key" class="legend-item"
          :style="{ background: s.bg, color: s.color }">{{ s.label }}</span>
      </div>

      <v-divider></v-divider>

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
                <th class="sticky-col name-col" style="height:20px;border:none"></th>
                <th v-for="pi in 3" :key="'ph0'+pi" class="part-rate-col sticky-col"
                  :class="{ 'last-part-col': pi === 3 }"
                  :style="{ left: (200 + (pi-1)*48) + 'px' }"
                  style="height:20px;border:none"></th>
                <template v-for="(monthGroup, mi) in visibleMonthGroups" :key="'m'+mi">
                  <th :colspan="monthGroup.colspan" class="month-header">{{ monthGroup.label }}</th>
                </template>
                <th class="stats-col d-none d-md-table-cell" style="border:none"></th>
                <th class="actions-col" style="border:none"></th>
              </tr>
              <tr>
                <th class="sticky-col name-col">Participant·e</th>
                <th v-for="pi in 3" :key="'ph1'+pi" class="part-rate-col sticky-col"
                  :class="{ 'last-part-col': pi === 3 }"
                  :style="{ left: (200 + (pi-1)*48) + 'px' }">
                  Abs.P{{ pi }}
                </th>
                <template v-for="(week,wi) in visibleWeekGroups" :key="'w'+wi">
                  <th class="week-sep-header" :colspan="week.days.length">Sem. {{ week.weekNum }}</th>
                </template>
                <th class="stats-col d-none d-md-table-cell">Stats</th>
                <th class="actions-col">Fiche</th>
              </tr>
              <tr>
                <th class="sticky-col name-col" style="height:38px"></th>
                <th v-for="pi in 3" :key="'ph2'+pi" class="part-rate-col sticky-col"
                  :class="{ 'last-part-col': pi === 3 }"
                  :style="{ left: (200 + (pi-1)*48) + 'px' }"
                  style="height:38px"></th>
                <template v-for="(week,wi) in visibleWeekGroups" :key="'wd'+wi">
                  <th v-for="day in week.days" :key="day.fullDate" class="day-header"
                    :class="{ 'ferie-header': isJourFerie(day.fullDate)||isVacance(day.fullDate), 'monday-col': day.isMonday, 'today-header': day.isToday }">
                    <div class="day-header-content">
                      <div class="day-name">{{ day.dayShort }}</div>
                      <div class="day-date">{{ day.date }}</div>
                      <div v-if="isJourFerie(day.fullDate)" class="ferie-indicator">F</div>
                      <div v-else-if="isVacance(day.fullDate)" class="vacance-indicator">V</div>
                    </div>
                  </th>
                </template>
                <th class="stats-col d-none d-md-table-cell"></th>
                <th class="actions-col"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filteredUsers.length === 0">
                <td :colspan="3 + currentPeriodDays.length" class="text-center pa-8 text-grey">
                  Aucun participant trouvé.
                </td>
              </tr>

              <tr v-for="user in filteredUsers" :key="user.id" class="table-row"
                v-memo="[presenceCache, ratesByPart, user.id, currentPartIndex, navSessionIndex, selectedCells.size, selectionMode, activeCell]">
                <td class="sticky-col name-col">
                  <div class="user-info">
                    <div class="name participant-name-link" @click="openParticipantPage(user)">{{ user.nom }} {{ user.prenom }}</div>
                    <div class="trigramme">{{ user.userName || user.username }}</div>
                    <div class="text-caption font-weight-bold text-medium-emphasis">{{ getNiveauForSession(user) }}</div>
                    <div class="text-caption font-weight-bold text-primary">{{ getUserSessionInfo(user) }}</div>
                    <div class="text-caption text-primary clickable-date" @click="openStartDateModal(user)">
                      <v-icon size="x-small" color="primary">mdi-calendar-start</v-icon>
                      {{ user.dateDebutParticipation ? formatDateFr(user.dateDebutParticipation) : 'Définir début' }}
                    </div>
                  </div>
                </td>

                <td v-for="pi in 3" :key="'pr'+pi" class="part-rate-col sticky-col"
                  :class="[
                    { 'last-part-col': pi === 3 },
                    ratesByPart[user.id]?.[pi-1] != null ? getAbsRateClass(ratesByPart[user.id][pi-1]) : 'text-grey'
                  ]"
                  :style="{ left: (200 + (pi-1)*48) + 'px' }">
                  <div class="rate-inner">
                    <strong v-if="ratesByPart[user.id]?.[pi-1] != null">
                      {{ roundHalfDown(ratesByPart[user.id][pi-1]) }}%
                    </strong>
                    <span v-else class="text-grey" style="font-size:0.65rem">—</span>
                  </div>
                </td>

                <template v-for="(week,wi) in visibleWeekGroups" :key="'cr'+wi">
                  <template v-for="(day, di) in week.days" :key="day.fullDate">
                    <td v-if="computeCell(user, week.days, di).show"
                      :colspan="computeCell(user, week.days, di).span"
                      :class="[
                        'presence-cell',
                        { 'monday-col': day.isMonday, 'today-cell': day.isToday,
                          'merged-cell': computeCell(user, week.days, di).span > 1 },
                        computeCell(user, week.days, di).cssClass,
                        { 'active-cell': activeCell && activeCell.id === user.id && activeCell.date === day.fullDate },
                        { 'selected-cell': selectionMode && selectedCells.has(`${user.id}__${day.fullDate}`) }
                      ]"
                      @click="!computeCell(user, week.days, di).locked && handleCellClickWithMerge(user, week.days, di)"
                      @touchend.prevent="!computeCell(user, week.days, di).locked && handleCellClickWithMerge(user, week.days, di)"
                    >{{ computeCell(user, week.days, di).text }}</td>
                  </template>
                </template>

                <td class="stats-col d-none d-md-table-cell">
                  <div class="stats-inner">
                    <div class="stat-value present">{{ countStatus(user, 'p') }}P</div>
                    <div class="stat-value absent">{{ countStatus(user, 'a') }}A</div>
                    <div class="stat-value excused">{{ countStatus(user, 'e') }}E</div>
                    <div class="stat-value retard">{{ countStatus(user, 'r') }}R</div>
                    <div class="stat-value stage">{{ countStatus(user, 's') }}S</div>
                    <div class="stat-value conge">{{ countStatus(user, 'c') }}C</div>
                    <div v-if="activeGroupValue === 'telecours'" class="stat-value visite">{{ countStatus(user, 'v') }}Vi</div>
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
    <v-dialog v-model="modalVisible" max-width="480px" :fullscreen="$vuetify.display.xs" persistent>
      <v-card rounded="lg" v-if="modalVisible">
        <v-card-title class="bg-primary text-white pa-4 d-flex align-center gap-2">
          <v-icon>mdi-file-pdf-box</v-icon>
          Fiche de présence
          <v-spacer />
          <v-btn icon size="small" variant="text" color="white" @click="modalVisible = false">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>
        <v-card-text class="pa-4">
          <div v-if="selectedUser" class="mb-4 d-flex align-center gap-3">
            <v-avatar :color="isUserTelecours ? 'teal' : 'blue'" size="40">
              <span class="text-white font-weight-bold text-subtitle-1">
                {{ (selectedUser.nom || '').charAt(0) }}{{ (selectedUser.prenom || '').charAt(0) }}
              </span>
            </v-avatar>
            <div>
              <div class="font-weight-bold text-body-1">{{ selectedUser.nom }} {{ selectedUser.prenom }}</div>
              <v-chip size="x-small" :color="isUserTelecours ? 'teal' : 'blue'" variant="tonal"
                :prepend-icon="isUserTelecours ? 'mdi-monitor' : 'mdi-school'">
                {{ isUserTelecours ? 'Télécours (10/20/32j)' : 'F&I (20/40/60j)' }}
              </v-chip>
            </div>
          </div>
          <div class="text-caption text-grey font-weight-bold text-uppercase mb-2 letter-spacing">
            Sélectionnez la période à imprimer
          </div>
          <v-alert v-if="!pdfOptions.length" type="warning" variant="tonal" density="compact">
            Aucune session configurée.
            <v-btn size="x-small" variant="text" :to="{ name: 'calendrier' }">Calendrier</v-btn>
          </v-alert>
          <div class="pdf-list" v-if="pdfOptions.length">
  
  <template v-for="group in currentSessionOnlyOption" :key="group.sessionName">
    <div class="pdf-group-header">
      <v-icon size="x-small" color="primary" class="mr-1">mdi-calendar-check</v-icon>
      <span class="font-weight-bold text-primary">{{ group.sessionName }} (Session Actuelle)</span>
      <span class="text-grey text-caption ml-2">{{ group.fullRange }}</span>
    </div>
    
    <div v-for="opt in group.parts" :key="opt.key" class="pdf-option"
      :class="{
        'pdf-option--active': selectedPeriodKey === opt.key,
        'pdf-option--recommended': opt.key === recommendedPeriodKey && selectedPeriodKey !== opt.key
      }"
      @click="selectedPeriodKey = opt.key">
      <div class="d-flex align-center gap-2 flex-grow-1">
        <div class="pdf-radio" :class="{ 'pdf-radio--on': selectedPeriodKey === opt.key }"></div>
        <div>
          <div class="pdf-opt-title">{{ opt.label }}</div>
          <div class="pdf-opt-range">{{ opt.cumulRange }}</div>
        </div>
      </div>
      <div class="d-flex align-center gap-1 flex-shrink-0">
        <v-chip v-if="opt.key === recommendedPeriodKey" size="x-small" color="success" variant="flat">
          En cours
        </v-chip>
        <v-chip size="x-small" :color="opt.isTele ? 'teal' : 'blue'" variant="tonal">
          {{ opt.targetDays }}j
        </v-chip>
      </div>
    </div>
  </template>

  <div class="d-flex justify-center my-3" v-if="pastSessionsOptions.length">
    <v-btn variant="tonal" size="small" color="secondary" prepend-icon="mdi-history" @click="showAllSessions = !showAllSessions">
      {{ showAllSessions ? 'Masquer les sessions précédentes' : 'Afficher les sessions précédentes' }}
    </v-btn>
  </div>

  <v-expand-transition>
    <div v-if="showAllSessions">
      <template v-for="group in pastSessionsOptions" :key="group.sessionName">
        <div class="pdf-group-header mt-3">
          <v-icon size="x-small" color="grey" class="mr-1">mdi-calendar-clock</v-icon>
          <span class="font-weight-bold text-grey-darken-1">{{ group.sessionName }}</span>
          <span class="text-grey text-caption ml-2">{{ group.fullRange }}</span>
        </div>
        
        <div v-for="opt in group.parts" :key="opt.key" class="pdf-option"
          :class="{
            'pdf-option--active': selectedPeriodKey === opt.key,
            'pdf-option--recommended': opt.key === recommendedPeriodKey && selectedPeriodKey !== opt.key
          }"
          @click="selectedPeriodKey = opt.key">
          <div class="d-flex align-center gap-2 flex-grow-1">
            <div class="pdf-radio" :class="{ 'pdf-radio--on': selectedPeriodKey === opt.key }"></div>
            <div>
              <div class="pdf-opt-title">{{ opt.label }}</div>
              <div class="pdf-opt-range">{{ opt.cumulRange }}</div>
            </div>
          </div>
          <div class="d-flex align-center gap-1 flex-shrink-0">
            <v-chip v-if="opt.key === recommendedPeriodKey" size="x-small" color="success" variant="flat">
              En cours
            </v-chip>
            <v-chip size="x-small" :color="opt.isTele ? 'teal' : 'blue'" variant="tonal">
              {{ opt.targetDays }}j
            </v-chip>
          </div>
        </div>
      </template>
    </div>
  </v-expand-transition>
</div>
        </v-card-text>
        <v-divider />
        <v-card-actions class="pa-3">
          <v-btn variant="text" @click="modalVisible = false" :disabled="isLoadingModal">Annuler</v-btn>
          <v-spacer />
          <v-btn color="primary" variant="flat" rounded @click="downloadAttestation"
            :loading="isLoadingModal" :disabled="!selectedPeriodKey">
            <v-icon start>mdi-download</v-icon>Télécharger
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Status picker — mobile uniquement + sélection multiple ════════ -->
    <v-bottom-sheet v-model="statusPickerVisible" inset>
      <v-card class="pa-5 pb-8" style="border-radius:24px 24px 0 0">
        <div class="d-flex justify-center mb-3">
          <div style="width:40px;height:4px;background:#ddd;border-radius:2px"></div>
        </div>
        <div class="text-center mb-4">
          <div class="text-caption text-uppercase text-grey font-weight-bold">
            {{ isBulkMode ? `Appliquer à ${selectedCells.size} case(s)` : 'Changer le statut' }}
          </div>
          <div class="text-h6 font-weight-bold text-primary mt-1">{{ statusPickerDay }}</div>
        </div>
        <div class="status-grid">
          <button v-for="s in currentStatusButtons" :key="s.key"
            class="status-tile" :style="{ background: s.bg, color: s.color }"
            @click="applyStatus(s.key)">
            <span class="status-tile-key">{{ s.displayKey }}</span>
            <span class="status-tile-label">{{ s.label }}</span>
          </button>
          <!-- Bouton retour à Présent -->
          <button class="status-tile" style="background:#e8eaf6;color:#1A237E" @click="applyStatus('p')">
            <span class="status-tile-key">✓</span>
            <span class="status-tile-label">Présent</span>
          </button>
        </div>
        <button class="cancel-btn mt-4" @click="statusPickerVisible = false">Annuler</button>
      </v-card>
    </v-bottom-sheet>

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
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
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
const search             = ref('')
const activeTabIndex     = ref(0)
const isLoading          = ref(false)
const isLoadingModal     = ref(false)
const allSessionLinks    = ref([])
const calendrier         = ref({ sessions: [], feries: [], vacances: [] })
const activeCell         = ref(null)   // { id, date, user } — cellule sélectionnée sur desktop
const presenceCache      = ref({})
const navSessionIndex    = ref(0)
const currentPartIndex   = ref(0)
const mobileWeekIndex    = ref(0)
const pendingSaves       = ref({})
let   saveDebounceTimer  = null

// Sélection multiple
const selectionMode  = ref(false)
const selectedCells  = ref(new Set())
const isBulkMode     = ref(false)

// Modals
const modalVisible          = ref(false)
const selectedUser          = ref(null)
const selectedPeriodKey     = ref(null)
const statusPickerVisible   = ref(false)
const statusPickerCell      = ref(null)
const statusPickerDay       = ref('')
const startDateModalVisible = ref(false)
const editingUser           = ref(null)
const newStartDate          = ref(null)
const showAllSessions       = ref(false)

// Téléchargement groupé
const downloadAllProgress   = ref(null) // null | { current, total }

const groups = [
  { label: 'Télécours', value: 'telecours' },
  { label: 'F&I A1',    value: 'fi_a1'    },
  { label: 'F&I A2',    value: 'fi_a2'    },
]

// FIX 6 : mapping formateur → groupe par défaut (clé = preferred_username en minuscules)
const FORMATEUR_GROUPS = {
  'myriam':        'telecours',
  'mls':           'fi_a2',
  'sac':           'fi_a2',
  'cs':            'fi_a1',
  'mec':           'fi_a1',
}

// ─── Détection tactile ────────────────────────────────────────────────────────
function isTouchDevice() {
  return window.matchMedia('(hover: none) and (pointer: coarse)').matches
}

// ─── Statuts — FIX 2 : retard affiché 'p', FIX 3 : 'c'=congé, 'v'=visite télécours ──
const allStatusButtons = [
  { key: 'a',  displayKey: 'A',  kbdKey: 'A', label: 'Absent',   bg: '#FFCDD2', color: '#B71C1C', groups: ['telecours','fi_a1','fi_a2'] },
  { key: 'e',  displayKey: 'E',  kbdKey: 'E', label: 'Excusé',   bg: '#FFE0B2', color: '#E65100', groups: ['telecours','fi_a1','fi_a2'] },
  { key: 'r',  displayKey: 'p',  kbdKey: 'P', label: 'Retard',   bg: '#B3E5FC', color: '#01579B', groups: ['telecours','fi_a1','fi_a2'] },
  { key: 's',  displayKey: 'S',  kbdKey: 'S', label: 'Stage',    bg: '#C8E6C9', color: '#1B5E20', groups: ['telecours','fi_a1','fi_a2'] },
  { key: 'c',  displayKey: 'C',  kbdKey: 'C', label: 'En congé', bg: '#1B5E20', color: '#fff',    groups: ['telecours','fi_a1','fi_a2'] },
  { key: 'v',  displayKey: 'Vi', kbdKey: 'V', label: 'Visite',   bg: '#CE93D8', color: '#4A148C', groups: ['telecours'] },
]

const activeGroupValue = computed(() => groups[activeTabIndex.value].value)

const currentStatusButtons = computed(() =>
  allStatusButtons.filter(b => b.groups.includes(activeGroupValue.value))
)

// Légende
const baseLegend = [
  { key: 'p', label: 'Présent',                              bg: '#1A237E', color: '#fff'    },
  { key: 'a', label: 'A - Absent',                           bg: '#FFCDD2', color: '#B71C1C' },
  { key: 'e', label: 'E - Excusé',                           bg: '#FFE0B2', color: '#E65100' },
  { key: 'r', label: 'p - Retard >15min ou départ anticipé', bg: '#B3E5FC', color: '#01579B' },
  { key: 's', label: 'S - Stage',                            bg: '#C8E6C9', color: '#1B5E20' },
  { key: 'c', label: 'C - En congé',                         bg: '#1B5E20', color: '#fff'    },
  { key: 'f', label: 'F - Férié',                            bg: '#FFF9C4', color: '#827717' },
]
const telecourseExtra = { key: 'v', label: 'Vi - Visite', bg: '#CE93D8', color: '#4A148C' }

const currentStatusLegend = computed(() =>
  activeGroupValue.value === 'telecours' ? [...baseLegend, telecourseExtra] : baseLegend
)

const currentSessionOnlyOption = computed(() => {
  if (!pdfOptions.value || pdfOptions.value.length === 0) return []
  const sessStart = currentSession.value?.startDate
  if (sessStart) {
    // Matcher par startDate (unique par session, évite l'ambiguïté des noms en cas de 3+ sessions)
    const ref   = moment(sessStart)
    const match = pdfOptions.value
      .map(g => ({ g, diff: Math.abs(moment(g.parts?.[0]?.sessionStart).diff(ref, 'days')) }))
      .filter(({ diff }) => diff <= 14)
      .sort((a, b) => a.diff - b.diff)[0]?.g
    if (match) return [match]
  }
  return [pdfOptions.value[pdfOptions.value.length - 1]]
})

const pastSessionsOptions = computed(() => {
  if (!pdfOptions.value || pdfOptions.value.length <= 1) return []
  const current = currentSessionOnlyOption.value[0]
  return pdfOptions.value.filter(g => g !== current)
})

const recommendedPeriodKey = computed(() => {
  const today    = moment()
  const allOpts  = sessionMonthOptions.value
  const sessName = currentSession.value?.name
  const sessOpts = sessName ? allOpts.filter(o => o.sessionName === sessName) : allOpts
  const current  = sessOpts.find(o => !moment(o.periodEnd).isBefore(today, 'day')) || sessOpts.at(-1)
  return current?.key || null
})

// 3. Pensez à réinitialiser le bouton quand on ferme/ouvre la modal
// Dans votre fonction openModal(user) existante ou dans un watch sur la visibilité :
watch(modalVisible, (newVal) => {
  if (!newVal) {
    showAllSessions.value = false // Masque à la fermeture pour les prochaines fois
  }
})

// ─── Sessions ─────────────────────────────────────────────────────────────────
const activeGroupSessions = computed(() => {
  const gv = groups[activeTabIndex.value].value
  return calendrier.value.sessions
    .filter(s => {
      const t = (s.type || '').toLowerCase()
      return (gv === 'telecours' ? t === 'telecours' : gv === 'fi_a1' ? t === 'fi_a1' : t === 'fi_a2') && s.startDate
    })
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
})

const currentSession = computed(() => activeGroupSessions.value[navSessionIndex.value] || null)

const currentSessionParts = computed(() => {
  if (!currentSession.value?.startDate || !currentSession.value?.endDate) return []
  const start      = moment(currentSession.value.startDate).startOf('isoWeek')
  const end        = moment(currentSession.value.endDate)
  const totalWeeks = Math.max(3, Math.ceil(end.diff(start, 'weeks', true)))
  const base = Math.floor(totalWeeks / 3), extra = totalWeeks % 3
  const sizes = [base + (extra >= 2 ? 1 : 0), base + (extra >= 1 ? 1 : 0), base]
  // Télécours : dernier jour = jeudi (pas de cours ven.) → -4j ; F&I : vendredi → -3j
  const isTele   = (currentSession.value?.type || '').toLowerCase() === 'telecours'
  const daysBack = isTele ? 4 : 3
  let wc = 0
  return sizes.map((weekCount, i) => {
    const ps = start.clone().add(wc, 'weeks')
    const pe = moment.min(ps.clone().add(weekCount, 'weeks').subtract(daysBack, 'days'), end.clone())
    wc += weekCount
    return { index: i, weekCount, start: ps.format('YYYY-MM-DD'), end: pe.format('YYYY-MM-DD') }
  })
})

const currentPart    = computed(() => currentSessionParts.value[currentPartIndex.value] || null)
const currentYear    = computed(() => currentSession.value ? moment(currentSession.value.startDate).year() : moment().year())
const currentQuarter = computed(() => navSessionIndex.value + 1)

const canGoPrev = computed(() => navSessionIndex.value > 0 || currentPartIndex.value > 0)
const canGoNext = computed(() =>
  !(navSessionIndex.value >= activeGroupSessions.value.length - 1 &&
    currentPartIndex.value >= currentSessionParts.value.length - 1)
)

const navLabel = computed(() => {
  if (!currentSession.value) return 'Aucune session'
  const n = `Session ${navSessionIndex.value + 1}`
  const p = currentPart.value
  if (isMobile.value) {
    const w = currentWeekGroups.value[mobileWeekIndex.value]
    if (w && p) return `${n} P${p.index + 1} — Sem. ${w.weekNum}`
  }
  return p ? `${n} — Partie ${p.index + 1}` : n
})

const currentPeriodRangeMobile = computed(() => {
  if (!isMobile.value) return null
  const w = currentWeekGroups.value[mobileWeekIndex.value]
  if (!w?.days?.length) return null
  const first = moment(w.days[0].fullDate, 'ddd MMM DD YYYY', 'en')
  const last  = moment(w.days[w.days.length - 1].fullDate, 'ddd MMM DD YYYY', 'en')
  return `${first.locale('fr').format('DD MMM')} – ${last.locale('fr').format('DD MMM YYYY')}`
})

const currentPeriodRange = computed(() => {
  if (isMobile.value && currentPeriodRangeMobile.value) return currentPeriodRangeMobile.value
  const p = currentPart.value
  if (p) return `${moment(p.start).locale('fr').format('DD MMM')} – ${moment(p.end).locale('fr').format('DD MMM YYYY')}`
  if (!currentSession.value?.startDate) return 'Aucune session configurée'
  const s = moment(currentSession.value.startDate)
  const e = currentSession.value.endDate ? moment(currentSession.value.endDate) : null
  return e ? `${s.locale('fr').format('DD MMM')} – ${e.locale('fr').format('DD MMM YYYY')}` : `À partir du ${s.locale('fr').format('DD MMM YYYY')}`
})

const currentPeriodDays = computed(() => {
  const part = currentPart.value
  if (!part) return []
  const start = moment(part.start).startOf('day')
  const end   = moment(part.end).endOf('day')
  const days  = []
  let curr = start.clone()
  while (curr.isSameOrBefore(end, 'day')) {
    if (curr.isoWeekday() < 6) {
      days.push({
        fullDate:  curr.clone().locale('en').format('ddd MMM DD YYYY'),
        date:      curr.format('D'),
        dayShort:  curr.locale('fr').format('ddd').charAt(0).toUpperCase(),
        weekNum:   curr.isoWeek(),
        isToday:   curr.isSame(moment(), 'day'),
        isMonday:  curr.isoWeekday() === 1,
      })
    }
    curr.add(1, 'day')
  }
  return days
})

const visibleMonthGroups = computed(() => {
  const result = []
  for (const week of visibleWeekGroups.value) {
    for (const day of week.days) {
      const label = moment(day.fullDate, 'ddd MMM DD YYYY', 'en').locale('fr').format('MMMM YYYY')
      const cap   = label.charAt(0).toUpperCase() + label.slice(1)
      const last  = result[result.length - 1]
      if (last && last.label === cap) last.colspan++
      else result.push({ label: cap, colspan: 1 })
    }
  }
  return result
})

const currentWeekGroups = computed(() => {
  const result = []
  let lastWeek = null
  for (const day of currentPeriodDays.value) {
    if (day.weekNum !== lastWeek) { result.push({ weekNum: day.weekNum, days: [] }); lastWeek = day.weekNum }
    result[result.length - 1].days.push(day)
  }
  return result
})

const isMobile = computed(() => typeof window !== 'undefined' && window.matchMedia('(max-width: 600px)').matches)

const visibleWeekGroups = computed(() =>
  isMobile.value ? [currentWeekGroups.value[mobileWeekIndex.value]].filter(Boolean) : currentWeekGroups.value
)

function mobileWeekPrev() {
  if (mobileWeekIndex.value > 0) mobileWeekIndex.value--
  else navigatePrev()
}
function mobileWeekNext() {
  if (mobileWeekIndex.value < currentWeekGroups.value.length - 1) mobileWeekIndex.value++
  else { navigateNext(); mobileWeekIndex.value = 0 }
}
function resetMobileWeek() {
  const today = moment()
  const idx   = currentWeekGroups.value.findIndex(w => w.days.some(d => moment(d.fullDate, 'ddd MMM DD YYYY', 'en').isSame(today, 'week')))
  mobileWeekIndex.value = idx >= 0 ? idx : 0
}
function goToPart(idx) { currentPartIndex.value = idx; fetchAttendancesForPeriod(); resetMobileWeek() }

function navigatePrev() {
  if (isMobile.value) { mobileWeekPrev(); return }
  if (currentPartIndex.value > 0) currentPartIndex.value--
  else if (navSessionIndex.value > 0) { navSessionIndex.value--; currentPartIndex.value = 2 }
  fetchAttendancesForPeriod(); resetMobileWeek()
}
function navigateNext() {
  if (isMobile.value) { mobileWeekNext(); return }
  if (currentPartIndex.value < currentSessionParts.value.length - 1) currentPartIndex.value++
  else if (navSessionIndex.value < activeGroupSessions.value.length - 1) { navSessionIndex.value++; currentPartIndex.value = 0 }
  fetchAttendancesForPeriod(); resetMobileWeek()
}

function onTabChange() {
  selectionMode.value = false
  selectedCells.value = new Set()
  activeCell.value    = null
  // Conserve la session/partie affichée plutôt que de revenir à "aujourd'hui" :
  // on ne fait que recaler l'index si le nouveau groupe a moins de sessions/parties.
  const sessions = activeGroupSessions.value
  if (!sessions.length) { navSessionIndex.value = 0; currentPartIndex.value = 0; fetchAttendancesForPeriod(); return }
  navSessionIndex.value = Math.min(navSessionIndex.value, sessions.length - 1)
  const parts = currentSessionParts.value
  currentPartIndex.value = Math.min(currentPartIndex.value, Math.max(parts.length - 1, 0))
  fetchAttendancesForPeriod()
}

function initializeNav() {
  const today    = moment()
  const sessions = activeGroupSessions.value
  if (!sessions.length) { navSessionIndex.value = 0; currentPartIndex.value = 0; fetchAttendancesForPeriod(); return }
  const idx = sessions.findIndex(s => s.startDate && s.endDate && today.isBetween(moment(s.startDate), moment(s.endDate), 'day', '[]'))
  navSessionIndex.value = idx >= 0 ? idx : sessions.length - 1
  const parts = currentSessionParts.value
  let found = parts.length - 1
  for (let i = 0; i < parts.length; i++) {
    if (today.isSameOrBefore(moment(parts[i].end), 'day')) { found = i; break }
  }
  currentPartIndex.value = found
  fetchAttendancesForPeriod()
}

// Sets précalculés O(1)
const feriesSet = computed(() => {
  const s = new Set()
  for (const f of (calendrier.value.feries || [])) s.add(moment(f.date).format('YYYY-MM-DD'))
  return s
})
const vacancesRanges = computed(() => {
  const ranges = []
  for (const v of (calendrier.value.vacances || []))
    ranges.push([moment(v.startDate).startOf('day'), moment(v.endDate).endOf('day')])
  for (const s of (calendrier.value.sessions || []))
    if (s.vacanceStart && s.vacanceEnd)
      ranges.push([moment(s.vacanceStart).startOf('day'), moment(s.vacanceEnd).endOf('day')])
  return ranges
})
const periodDayMeta = computed(() => {
  const meta = {}
  for (const day of currentPeriodDays.value) {
    const iso     = moment(day.fullDate, 'ddd MMM DD YYYY', 'en').format('YYYY-MM-DD')
    const isFerie = feriesSet.value.has(iso)
    const d       = moment(iso)
    const isVac   = !isFerie && vacancesRanges.value.some(([s, e]) => d.isBetween(s, e, 'day', '[]'))
    meta[day.fullDate] = { iso, isFerie, isVac }
  }
  return meta
})
function isJourFerie(fd) { return periodDayMeta.value[fd]?.isFerie ?? false }
function isVacance(fd)   { return periodDayMeta.value[fd]?.isVac   ?? false }

// ─── Groupes / filtrage ───────────────────────────────────────────────────────
function findSessionLink(user) {
  return allSessionLinks.value.find(l => {
    const uid = l.studentId || l.StudentId
    const s   = l.session || l.Session
    if (String(uid).toLowerCase() !== String(user.id).toLowerCase() || !s) return false
    const y = new Date(s.dateStart || s.DateStart).getFullYear()
    const n = s.sessionNumberId || s.SessionNumberId
    return Number(y) === currentYear.value && Number(n) === currentQuarter.value
  })
}

function getUserSessionInfo(user) {
  const link = findSessionLink(user)
  const s = link?.session || link?.Session
  return s ? `Session ${s.sessionNumberId || s.SessionNumberId}` : ''
}

// Niveau (F&I A1/A2/Télécours) tel qu'il était PENDANT la session affichée,
// et non le niveau courant du stagiaire (qui peut avoir changé depuis) —
// on lit la classe (SchoolClassRoom) rattachée au lien de session correspondant.
function getNiveauForSession(user) {
  const link  = findSessionLink(user)
  const s     = link?.session || link?.Session
  const room  = s?.schoolClassRoom || s?.SchoolClassRoom
  const name  = room?.schoolClassRoomName || room?.SchoolClassRoomName
  return name || user.niveau || user.Niveau || ''
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
  const niveau      = getNiveauForSession(user).toLowerCase().trim()
  const isTelecours = niveau.includes('tele') || niveau.includes('télé')
  if (groupValue === 'telecours') return isTelecours
  if (groupValue === 'fi_a1')    return !isTelecours && niveau.includes('a1')
  if (groupValue === 'fi_a2')    return !isTelecours && niveau.includes('a2')
  return false
}
function groupCount(val) { return users.value.filter(u => matchesGroup(u, val)).length }

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
  return list.sort((a, b) => (a.nom || '').localeCompare((b.nom || ''), 'fr', { sensitivity: 'base' }))
})

const isUserTelecours = computed(() => {
  const n = (selectedUser.value?.niveau || selectedUser.value?.Niveau || '').toLowerCase()
  return n.includes('tele') || n.includes('télé')
})

// ─── Gestion des clics et du clavier ─────────────────────────────────────────
// Desktop : clic = sélectionner la cellule → taper la lettre au clavier pour changer le statut
// Mobile/tactile : clic = ouvrir le picker de statut
function handleCellClick(user, dateString) {
  // Mode sélection multiple (tous appareils)
  if (selectionMode.value) {
    const key  = `${user.id}__${dateString}`
    const next = new Set(selectedCells.value)
    if (next.has(key)) next.delete(key)
    else next.add(key)
    selectedCells.value = next
    return
  }

  // Mémoriser la cellule active
  activeCell.value = { id: user.id, date: dateString, user }

  if (isTouchDevice()) {
    // Mobile : ouvrir le picker
    isBulkMode.value       = false
    statusPickerCell.value = { user, dateString }
    statusPickerDay.value  = moment(dateString, 'ddd MMM DD YYYY', 'en').locale('fr').format('dddd DD MMM')
    statusPickerVisible.value = true
  }
  // Desktop : rien d'autre — l'utilisateur tape la lettre (handleKeyDown ci-dessous)
}

function handleCellClickWithMerge(user, weekDays, di) {
  const cell = computeCell(user, weekDays, di)

  // Selection mode: toggle all merged dates
  if (selectionMode.value && cell.span > 1) {
    const mergedDates = []
    for (let j = di; j < di + cell.span && j < weekDays.length; j++)
      mergedDates.push(weekDays[j].fullDate)
    const next = new Set(selectedCells.value)
    for (const ds of mergedDates) {
      const key = `${user.id}__${ds}`
      if (next.has(key)) next.delete(key)
      else next.add(key)
    }
    selectedCells.value = next
    return
  }

  // Mobile with merged cell: open picker that applies to all days in the run
  if (isTouchDevice() && cell.span > 1) {
    activeCell.value = { id: user.id, date: weekDays[di].fullDate, user }
    const mergedDates = []
    for (let j = di; j < di + cell.span && j < weekDays.length; j++)
      mergedDates.push(weekDays[j].fullDate)
    isBulkMode.value          = false
    statusPickerCell.value    = { user, dateString: mergedDates[0], mergedDates }
    statusPickerDay.value     = `${mergedDates.length} jour${mergedDates.length > 1 ? 's' : ''} (${cell.text})`
    statusPickerVisible.value = true
    return
  }

  // Default: normal click (desktop selects cell, then keyboard changes status)
  handleCellClick(user, weekDays[di].fullDate)
}

// Returns all dates in the merged run containing the active cell (or [date] if not merged)
function getActiveMergedDates() {
  if (!activeCell.value) return []
  const { user, date } = activeCell.value
  for (const week of currentWeekGroups.value) {
    const di = week.days.findIndex(d => d.fullDate === date)
    if (di < 0) continue
    const cell = computeCell(user, week.days, di)
    if (cell.span <= 1) return [date]
    const result = []
    for (let j = di; j < di + cell.span && j < week.days.length; j++)
      result.push(week.days[j].fullDate)
    return result
  }
  return [date]
}

// Keyboard : a=absent, e=excusé, p=retard, s=stage, c=congé, v=visite, Suppr/Del=présent
function handleKeyDown(e) {
  if (!activeCell.value) return
  if (['INPUT', 'TEXTAREA'].includes(e.target.tagName)) return
  const map = {
    a: 'a', e: 'e', p: 'r', s: 's', c: 'c', v: 'v',
    delete: 'p', backspace: 'p',
  }
  const key = map[e.key.toLowerCase()]
  if (key) {
    if (key === 'p') {
      // DELETE/Backspace: set présent on all days in the merged run
      for (const ds of getActiveMergedDates())
        updateStatus(activeCell.value.user, ds, 'p')
    } else {
      updateStatus(activeCell.value.user, activeCell.value.date, key)
    }
    e.preventDefault()
  }
}

// Sélection multiple : ouvrir picker
function openBulkStatusPicker() {
  isBulkMode.value       = true
  statusPickerDay.value  = ''
  statusPickerVisible.value = true
}

function toggleSelectionMode() {
  selectionMode.value = !selectionMode.value
  if (!selectionMode.value) { selectedCells.value = new Set(); isBulkMode.value = false }
}

// Applique le statut depuis le picker (mobile ou bulk)
function applyStatus(key) {
  if (isBulkMode.value && selectedCells.value.size > 0) {
    for (const cellKey of selectedCells.value) {
      const [userId, ...dateParts] = cellKey.split('__')
      const dateString = dateParts.join('__')
      const user = filteredUsers.value.find(u => String(u.id) === String(userId))
      if (user) updateStatus(user, dateString, key)
    }
    selectedCells.value   = new Set()
    selectionMode.value   = false
    isBulkMode.value      = false
    statusPickerVisible.value = false
    return
  }
  if (!statusPickerCell.value) return
  const { user, dateString, mergedDates } = statusPickerCell.value
  if (mergedDates && mergedDates.length > 1) {
    for (const ds of mergedDates) updateStatus(user, ds, key)
  } else {
    updateStatus(user, dateString, key)
  }
  statusPickerVisible.value = false
  statusPickerCell.value    = null
}

function updateStatus(user, dateString, newStatus) {
  presenceCache.value = { ...presenceCache.value, [`${user.id}__${dateString}`]: newStatus }
  const date = moment(dateString, 'ddd MMM DD YYYY', 'en').format('YYYY-MM-DD')
  pendingSaves.value[`${user.id}__${date}`] = { stagiaireId: user.id, date, statut: newStatus }
  clearTimeout(saveDebounceTimer)
  saveDebounceTimer = setTimeout(flushPendingSaves, 800)
}

async function flushPendingSaves() {
  const batch = Object.values(pendingSaves.value)
  if (!batch.length) return
  pendingSaves.value = {}
  try { await authFetch(`${API_BASE}/api/attendance/save`, { method: 'POST', body: JSON.stringify(batch) }) }
  catch (e) { console.error('Save error:', e) }
}

// ─── Cellules ─────────────────────────────────────────────────────────────────
function computeCell(user, weekDays, di) {
  const day      = weekDays[di]
  const meta     = periodDayMeta.value[day.fullDate] || {}
  const isBefore = isBeforeParticipation(user, day.fullDate)

  if (isBefore)      return { show: true, span: 1, cssClass: 'not-started', text: '', locked: true }
  if (meta.isFerie)  return { show: true, span: 1, cssClass: 'status-f', text: 'F', locked: true }

  const rawSt = presenceCache.value[`${user.id}__${day.fullDate}`] ?? user.presences?.[day.fullDate] ?? 'p'
  // Vacance calendrier → 'c' (congé) si pas de statut manuel
  const st = (meta.isVac && rawSt === 'p') ? 'c' : rawSt

  // Cellules simples (pas 'c', 's' ni 'v')
  if (st !== 'c' && st !== 's' && st !== 'v') {
    const cssClass = getStatusClass(st)
    // FIX 2 : retard affiché 'p' (minuscule)
    const textMap  = { a: 'A', e: 'E', r: 'p', f: 'F', p: '' }
    return { show: true, span: 1, cssClass, text: textMap[st] ?? st.toUpperCase(), locked: false }
  }

  // Fusion des runs consécutifs (c, s ou v)
  const getEffSt = (d) => {
    const m   = periodDayMeta.value[d.fullDate] || {}
    const raw = presenceCache.value[`${user.id}__${d.fullDate}`] ?? user.presences?.[d.fullDate] ?? 'p'
    return (m.isVac && raw === 'p') ? 'c' : raw
  }
  if (di > 0 && getEffSt(weekDays[di - 1]) === st) return { show: false, span: 0, cssClass: '', text: '', locked: true }

  let span = 1
  for (let j = di + 1; j < weekDays.length; j++) {
    if (getEffSt(weekDays[j]) === st) span++
    else break
  }

  const cssClass = st === 'c' ? 'status-c' : st === 'v' ? 'status-v' : 'status-s'
  // Texte complet dès 2 jours
  const label = st === 'c'
    ? (span >= 3 ? 'Congés/Vacances' : span >= 2 ? 'Vac.' : 'C')
    : st === 'v'
      ? (span >= 3 ? 'Visite' : span >= 2 ? 'Visite' : 'Vi')
      : (span >= 3 ? 'Stage'  : span >= 2 ? 'Stage'  : 'S')
  const locked = meta.isVac && rawSt === 'p'
  return { show: true, span, cssClass, text: label, locked }
}

function getStatusClass(st) {
  const map = { p: 'status-p', a: 'status-a', e: 'status-e', r: 'status-r', s: 'status-s', c: 'status-c', v: 'status-v', f: 'status-f' }
  return map[st] || 'status-p'
}

function countStatus(user, status) {
  return currentPeriodDays.value.filter(d => {
    const m   = periodDayMeta.value[d.fullDate]
    if (isBeforeParticipation(user, d.fullDate) || m?.isFerie) return false
    const raw = presenceCache.value[`${user.id}__${d.fullDate}`] ?? user.presences?.[d.fullDate] ?? 'p'
    const st  = (m?.isVac && raw === 'p') ? 'c' : raw
    return st === status
  }).length
}
function countFeries(user) {
  return currentPeriodDays.value.filter(d => {
    const m = periodDayMeta.value[d.fullDate]
    return !isBeforeParticipation(user, d.fullDate) && (m?.isFerie || m?.isVac)
  }).length
}

// ─── Taux d'absence par partie (cumulatif, base jours-cibles fixes) ───────────
// Télécours : cours lun/mar/jeu ; base 10j/20j/32j
// F&I       : cours lun–ven    ; base 20j/40j/60j
// Fériés/vacances sur un jour de cours → réduisent le dénominateur.
// Fériés sur mercredi ou vendredi (télécours) → sans impact.
const ratesByPart = computed(() => {
  if (!currentSession.value) return {}
  const parts        = currentSessionParts.value
  const today        = moment()
  const sessionStart = moment(currentSession.value.startDate).startOf('day')
  const isTele       = (currentSession.value?.type || '').toLowerCase() === 'telecours'
  const targets      = isTele ? [10, 20, 32] : [20, 40, 60]
  const schoolDows   = isTele ? new Set([1, 2, 4]) : new Set([1, 2, 3, 4, 5])
  const result       = {}

  for (const user of filteredUsers.value) {
    result[user.id] = parts.map((part, i) => {
      const partStart = moment(part.start).startOf('day')
      const partEnd   = moment(part.end).endOf('day')
      if (partStart.isAfter(today, 'day')) return null

      let deductDays = 0
      let aCount = 0, eCount = 0, rCount = 0

      const cur = sessionStart.clone()
      while (cur.isSameOrBefore(partEnd)) {
        if (schoolDows.has(cur.isoWeekday())) {
          const isoStr  = cur.format('YYYY-MM-DD')
          const dk      = cur.clone().locale('en').format('ddd MMM DD YYYY')
          const isFerie = feriesSet.value.has(isoStr)
          const isVac   = !isFerie && vacancesRanges.value.some(([s, e]) => cur.isBetween(s, e, 'day', '[]'))
          if (isFerie || isVac) {
            deductDays++
          } else if (!isBeforeParticipation(user, dk)) {
            const st = presenceCache.value[`${user.id}__${dk}`] ?? user.presences?.[dk] ?? 'p'
            if (st === 'a') aCount++
            else if (st === 'e') eCount++
            else if (st === 'r') rCount++
          }
        }
        cur.add(1, 'day')
      }

      const realDays = Math.max(1, targets[i] - deductDays)
      const absScore = aCount + eCount + rCount * 0.5
      return absScore / realDays * 100
    })
  }
  return result
})

// ─── FIX 4 : Taux d'absence = (A + E + R×0.5) / jours travaillés × 100 ──────
const ratesCache = computed(() => {
  if (!currentSession.value) return {}
  const start   = moment(currentSession.value.startDate).startOf('day')
  const end     = moment(currentSession.value.endDate).endOf('day')
  const today   = moment()
  const scanEnd = today.isBefore(end) ? today.clone() : end.clone()
  const result  = {}

  for (const user of filteredUsers.value) {
    let aCount = 0, eCount = 0, rCount = 0, totalWorkingDays = 0
    const cur = start.clone()
    while (cur.isSameOrBefore(scanEnd)) {
      if (cur.isoWeekday() < 6) {
        const dk   = cur.clone().locale('en').format('ddd MMM DD YYYY')
        const meta = periodDayMeta.value[dk]
        if (!isBeforeParticipation(user, dk) && !meta?.isFerie && !meta?.isVac) {
          totalWorkingDays++
          const st = presenceCache.value[`${user.id}__${dk}`] ?? user.presences?.[dk] ?? 'p'
          if (st === 'a') aCount++
          else if (st === 'e') eCount++
          else if (st === 'r') rCount++
        }
      }
      cur.add(1, 'day')
    }
    const absScore = aCount + eCount + rCount * 0.5
    const absRate  = totalWorkingDays > 0 ? (absScore / totalWorkingDays * 100) : 0
    result[user.id] = { absRate, absScore, aCount, eCount, rCount, totalWorkingDays }
  }
  return result
})

function calculateRates(user) {
  return ratesCache.value[user?.id] ?? { absRate: 0, absScore: 0 }
}
function getAbsRateClass(rate) {
  return rate >= 20 ? 'text-error' : rate >= 10 ? 'text-warning' : 'text-success'
}

// ─── Date début participation ─────────────────────────────────────────────────
function isBeforeParticipation(user, dateString) {
  const raw = user.DateDebutParticipation || user.dateDebutParticipation
  if (!raw || raw === 'undefined' || raw === 'null') return false
  const cell  = moment(dateString, 'ddd MMM DD YYYY', 'en').startOf('day')
  const start = moment(raw).startOf('day')
  return cell.isValid() && start.isValid() && cell.isBefore(start, 'day')
}
function openParticipantPage(user) {
  const resolved = router.resolve({ name: 'Participant-Detail', params: { id: user.id } })
  window.open(resolved.href, '_blank')
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
function formatDateFr(d) { return moment(d).locale('fr').format('DD/MM/YYYY') }

// Arrondi "demi-bas" : > x.5 → haut, ≤ x.5 → bas (ex: 2.5→2, 2.6→3)
function roundHalfDown(x) { return Math.ceil(x - 0.5) }

// ─── PDF ──────────────────────────────────────────────────────────────────────
function computeSessionPartsForPdf(sessStart, sessEnd, sessType) {
  const isTele   = (sessType || '').toLowerCase() === 'telecours'
  const daysBack = isTele ? 4 : 3
  const start      = sessStart.clone().startOf('isoWeek')
  const totalWeeks = Math.max(3, Math.ceil(sessEnd.diff(start, 'weeks', true)))
  const base = Math.floor(totalWeeks / 3), extra = totalWeeks % 3
  const sizes = [base + (extra >= 2 ? 1 : 0), base + (extra >= 1 ? 1 : 0), base]
  let wc = 0
  return sizes.map((weekCount, i) => {
    const ps = start.clone().add(wc, 'weeks')
    const pe = moment.min(ps.clone().add(weekCount, 'weeks').subtract(daysBack, 'days'), sessEnd.clone())
    wc += weekCount
    return { index: i, weekCount, start: ps, end: pe }
  })
}

const pdfOptions = computed(() => {
  if (!selectedUser.value) return []
  const isTele      = isUserTelecours.value
  const userNiveau  = (selectedUser.value?.niveau || selectedUser.value?.Niveau || '').toLowerCase()
  const isA2        = !isTele && userNiveau.includes('a2')
  const isA1        = !isTele && userNiveau.includes('a1')
  const targets     = isTele ? [10, 20, 32] : [20, 40, 60]
  const today       = moment()
  return calendrier.value.sessions
    .filter(s => {
      const t = (s.type || '').toLowerCase()
      if (!s.startDate || !s.endDate || moment(s.startDate).isAfter(today, 'day')) return false
      if (isTele) return t === 'telecours'
      if (isA2)   return t === 'fi_a2'
      if (isA1)   return t === 'fi_a1'
      return t === 'fi_a1' || t === 'fi_a2' || t.startsWith('fi')
    })
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
    .map(sess => {
      const sessStart = moment(sess.startDate)
      const sessEnd   = moment(sess.endDate)
      const parts     = computeSessionPartsForPdf(sessStart, sessEnd, sess.type)
      if (sessStart.isAfter(today, 'day')) return null
      const partOpts = parts.map((p, i) => {
        const isFuture = p.start.isAfter(today, 'day')
        return {
          key:          `${sess.name}__P${i + 1}`,
          label:        `Partie ${i + 1}`,
          cumulRange:   isFuture
            ? `Commence le ${p.start.locale('fr').format('DD MMM YYYY')}`
            : `Cumulatif : ${sessStart.locale('fr').format('DD MMM')} → ${p.end.locale('fr').format('DD MMM YYYY')}`,
          isTele,
          targetDays:   targets[i],
          sessionName:  sess.name,
          sessionStart: sessStart.format('YYYY-MM-DD'),
          sessionEnd:   sessEnd.format('YYYY-MM-DD'),
          periodEnd:    p.end.format('YYYY-MM-DD'),
          sessionMonth: i + 1,
          sessionType:  sess.type,
          isFuture,
        }
      })
      if (!partOpts.length) return null
      return {
        sessionName: sess.name,
        fullRange: `${sessStart.locale('fr').format('DD MMM YYYY')} – ${sessEnd.locale('fr').format('DD MMM YYYY')}`,
        parts: partOpts,
      }
    })
    .filter(Boolean)
})

const sessionMonthOptions = computed(() => pdfOptions.value.flatMap(g => g.parts))

async function openModal(user) {
  selectedUser.value      = user
  selectedPeriodKey.value = null
  modalVisible.value      = true
  await Promise.resolve()
  const allOpts  = sessionMonthOptions.value
  const sessName = currentSession.value?.name
  const today    = moment()
  // Sélectionner la partie qui contient aujourd'hui (periodEnd >= today), sinon la dernière
  const sessOpts = sessName ? allOpts.filter(o => o.sessionName === sessName) : allOpts
  const current  = sessOpts.find(o => !moment(o.periodEnd).isBefore(today, 'day')) || sessOpts.at(-1)
  selectedPeriodKey.value = current?.key || allOpts.at(-1)?.key || null
}

async function downloadAttestation() {
  const opt = sessionMonthOptions.value.find(o => o.key === selectedPeriodKey.value)
  if (!opt) return
  isLoadingModal.value = true
  try {
    const params = new URLSearchParams({
      startDate: opt.sessionStart, endDate: opt.periodEnd,
      sessionEndDate: opt.sessionEnd,
      sessionName: opt.sessionName, sessionType: opt.sessionType, sessionMonth: opt.sessionMonth,
    })
    const res = await authFetch(`${API_BASE}/api/documents/attestationpresence/${selectedUser.value.id}?${params}`, { headers: { 'Content-Type': '' } })
    if (res?.ok) {
      const blob = await res.blob()
      const link = document.createElement('a')
      link.href  = window.URL.createObjectURL(blob)
      link.download = `Presence-${selectedUser.value.nom?.trim()}-${opt.sessionName}-P${opt.sessionMonth}.pdf`
      link.click()
    } else alert('Erreur serveur')
  } catch (e) { console.error(e) }
  finally { isLoadingModal.value = false; modalVisible.value = false }
}

async function downloadAllAttestations() {
  const today = moment()
  const users = filteredUsers.value
  console.log('[DownloadAll] Démarrage — utilisateurs dans le groupe:', users.length, users.map(u => u.nom))
  console.log('[DownloadAll] Session courante:', currentSession.value)
  console.log('[DownloadAll] Calendrier sessions:', calendrier.value.sessions)

  downloadAllProgress.value = { current: 0, total: users.length }

  for (const user of users) {
    console.log(`[DownloadAll] ▶ Traitement: ${user.nom} ${user.prenom} (id=${user.id})`)
    try {
      const niveauLower = (user.niveau || user.Niveau || '').toLowerCase()
      const isTele  = niveauLower.includes('tele') || niveauLower.includes('télé')
      console.log(`[DownloadAll]   niveau="${user.niveau}" niveauLower="${niveauLower}" isTele=${isTele}`)

      const filteredSessions = calendrier.value.sessions.filter(s => {
        const t = (s.type || '').toLowerCase()
        return (isTele ? t === 'telecours' : (t === 'fi_a1' || t === 'fi_a2' || t.startsWith('fi')))
          && s.startDate && s.endDate && !moment(s.startDate).isAfter(today, 'day')
      })
      console.log(`[DownloadAll]   sessions filtrées (${filteredSessions.length}):`, filteredSessions.map(s => s.name))

      const userOpts = filteredSessions
        .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
        .flatMap(sess => {
          const sessStart = moment(sess.startDate)
          const sessEnd   = moment(sess.endDate)
          return computeSessionPartsForPdf(sessStart, sessEnd, sess.type)
            .filter(p => !p.start.isAfter(today, 'day'))
            .map((p, i) => ({
              sessionStart: sessStart.format('YYYY-MM-DD'),
              sessionEnd:   sessEnd.format('YYYY-MM-DD'),
              periodEnd:    p.end.format('YYYY-MM-DD'),
              sessionName:  sess.name, sessionType: sess.type, sessionMonth: i + 1,
            }))
        })
      console.log(`[DownloadAll]   options PDF calculées (${userOpts.length}):`, userOpts)

      const sessName = currentSession.value?.name
      const matched  = sessName ? userOpts.filter(o => o.sessionName === sessName) : []
      const opt      = matched.length ? matched.at(-1) : userOpts.at(-1)
      console.log(`[DownloadAll]   sessName="${sessName}" matched=${matched.length} → option choisie:`, opt)

      if (!opt) {
        console.warn(`[DownloadAll]   ⚠ Aucune option PDF pour ${user.nom} — skip`)
        downloadAllProgress.value.current++
        continue
      }

      const params = new URLSearchParams({
        startDate: opt.sessionStart, endDate: opt.periodEnd,
        sessionEndDate: opt.sessionEnd,
        sessionName: opt.sessionName, sessionType: opt.sessionType, sessionMonth: opt.sessionMonth,
      })
      const url = `${API_BASE}/api/documents/attestationpresence/${user.id}?${params}`
      console.log(`[DownloadAll]   → requête: ${url}`)

      const res = await authFetch(url, { headers: { 'Content-Type': '' } })
      console.log(`[DownloadAll]   ← réponse: status=${res?.status} ok=${res?.ok}`)

      if (res?.ok) {
        const blob = await res.blob()
        console.log(`[DownloadAll]   blob reçu: size=${blob.size} type="${blob.type}"`)
        const blobUrl = window.URL.createObjectURL(blob)
        const link    = document.createElement('a')
        link.href     = blobUrl
        link.download = `Presence-${user.nom?.trim()}-${opt.sessionName}-P${opt.sessionMonth}.pdf`
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(blobUrl)
        console.log(`[DownloadAll]   ✓ téléchargement déclenché: ${link.download}`)
        await new Promise(r => setTimeout(r, 600))
      } else {
        console.error(`[DownloadAll]   ✗ Erreur HTTP ${res?.status} pour ${user.nom}`)
      }
    } catch (e) {
      console.error(`[DownloadAll]   ✗ Exception pour ${user.nom}:`, e)
    }
    downloadAllProgress.value.current++
  }
  console.log('[DownloadAll] Terminé.')
  downloadAllProgress.value = null
}

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  try { await store.dispatch('user/fetchUsers', true) } catch (e) { console.error(e) }
  await Promise.all([loadCalendrier(), loadAllSessionLinks()])

  // FIX 6 : auto-sélectionner le groupe selon l'identifiant connecté
  const storedUser    = JSON.parse(localStorage.getItem('user') || '{}')
  const prefUsername  = (storedUser.profile?.preferred_username || '').toLowerCase()
  const defaultGroup  = FORMATEUR_GROUPS[prefUsername]
  if (defaultGroup) {
    const idx = groups.findIndex(g => g.value === defaultGroup)
    if (idx >= 0) activeTabIndex.value = idx
  }

  initializeNav()
  window.addEventListener('keyup', handleKeyDown)
})
onBeforeUnmount(() => {
  window.removeEventListener('keyup', handleKeyDown)
  clearTimeout(saveDebounceTimer)
  const batch = Object.values(pendingSaves.value)
  if (batch.length) navigator.sendBeacon(`${API_BASE}/api/attendance/save`, new Blob([JSON.stringify(batch)], { type: 'application/json' }))
})

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
async function fetchAttendancesForPeriod() {
  isLoading.value = true
  const start = currentSession.value?.startDate
    ? moment(currentSession.value.startDate).format('YYYY-MM-DD')
    : moment().startOf('month').format('YYYY-MM-DD')
  const end = currentSession.value?.endDate
    ? moment(currentSession.value.endDate).format('YYYY-MM-DD')
    : moment().endOf('month').format('YYYY-MM-DD')
  try {
    const res     = await authFetch(`${API_BASE}/api/attendance/byWeek?start=${start}&end=${end}`)
    const json    = await res?.json()
    const records = Array.isArray(json) ? json : []
    const cache   = { ...presenceCache.value }
    records.forEach(r => {
      const uid  = r.stagiaireId || r.userId || r.StagiaireId || r.UserId
      const date = r.date || r.Date
      const st   = (r.statut || r.Statut || 'p').toLowerCase()
      if (uid && date) cache[`${uid}__${moment(date).locale('en').format('ddd MMM DD YYYY')}`] = st
    })
    presenceCache.value = cache
    await store.dispatch('user/updateUserAttendances', {
      attendances: records, currentPeriodDays: currentPeriodDays.value.map(d => d.fullDate),
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
.part-rate-col   { min-width: 48px; width: 48px; padding: 3px 2px; text-align: center; font-size: 0.7rem; background-color: #f8f8f8; z-index: 2; }
.last-part-col   { border-right: 2px solid #ddd; }
.actions-col     { position: sticky; right: 0; background-color: #f8f9fa; z-index: 2; min-width: 60px; }

.presence-table th,
.presence-table td { border: 1px solid #e0e0e0; text-align: center; padding: 4px 2px; }
.presence-table thead th { background: #f5f5f5; position: sticky; top: 0; z-index: 3; }
.presence-table thead .sticky-col { z-index: 5; }

.day-header         { min-width: 42px; width: 42px; }
.day-header-content { line-height: 1.3; }
.day-name           { font-size: 0.65rem; color: #888; text-transform: capitalize; }
.day-date           { font-weight: bold; font-size: 0.8rem; }
.ferie-header       { background-color: #FFF9C4 !important; }
.ferie-indicator    { font-size: 0.6rem; font-weight: bold; color: #827717; }
.vacance-indicator  { font-size: 0.6rem; font-weight: bold; color: #2E7D32; }

.presence-cell       { cursor: pointer; height: 38px; width: 42px; transition: all 0.1s; font-weight: bold; font-size: 0.75rem; }
.presence-cell:hover { filter: brightness(0.88); }
.presence-cell.active-cell  { box-shadow: inset 0 0 0 3px #FF6F00 !important; z-index: 10; position: relative; }
.presence-cell.selected-cell { box-shadow: inset 0 0 0 3px #F59E0B !important; filter: brightness(0.82); z-index: 10; position: relative; }

.status-p { background-color: #1A237E; color: #FFFFFF; }
.status-a { background-color: #FFCDD2; color: #B71C1C; }
.status-e { background-color: #FFE0B2; color: #E65100; }
.status-r { background-color: #B3E5FC; color: #01579B; }
.status-s { background-color: #C8E6C9; color: #1B5E20; }
.status-c { background-color: #1B5E20; color: #FFFFFF; }  /* congé, ex-vacances */
.status-v { background-color: #CE93D8; color: #4A148C; }  /* visite télécours */
.status-f { background-color: #FFF9C4; color: #827717; }

.presence-cell.not-started {
  background: repeating-linear-gradient(45deg,#eee,#eee 5px,#e0e0e0 5px,#e0e0e0 10px);
  cursor: not-allowed; opacity: 0.5;
}

/* Aide clavier */
.keyboard-hint {
  background: #FFFDE7; border-bottom: 1px solid #F9A825;
  font-size: 0.75rem; color: #555; display: flex; flex-wrap: wrap; align-items: center; gap: 6px;
}
.hint-key {
  display: inline-block; padding: 1px 7px; border-radius: 8px;
  font-size: 0.7rem; font-weight: 700;
}

.stats-col   { min-width: 150px; }
.stats-inner { display: flex; gap: 6px; align-items: center; justify-content: center; flex-wrap: wrap; padding: 2px; }

.stat-value         { font-weight: bold; font-size: 0.72rem; }
.stat-value.present { color: #1A237E; }
.stat-value.absent  { color: #B71C1C; }
.stat-value.excused { color: #E65100; }
.stat-value.retard  { color: #01579B; }
.stat-value.stage   { color: #1B5E20; }
.stat-value.conge   { color: #1B5E20; }
.stat-value.visite  { color: #4A148C; }
.stat-value.ferie   { color: #827717; }

.legend-bar  { background: #fafafa; }
.legend-item { display: inline-block; padding: 2px 10px; border-radius: 12px; font-size: 0.72rem; font-weight: 600; margin-right: 10px; }

.user-info .name      { font-weight: bold; color: #2c3e50; }
.user-info .trigramme { font-size: 0.72rem; color: #999; text-transform: uppercase; }
.clickable-date:hover { text-decoration: underline; background: rgba(0,0,0,0.04); border-radius: 4px; }
.participant-name-link { cursor: pointer; }
.participant-name-link:hover { text-decoration: underline; color: #1976D2; }
.rate-inner { display: flex; flex-direction: column; align-items: center; }
.merged-cell { font-size: 0.7rem !important; font-weight: 700 !important; letter-spacing: 0.03em; }
.today-header { background: #FFF9C4 !important; }
.today-cell   { box-shadow: inset 0 0 0 2px #FF6F00; }

/* Mobile status picker */
.status-grid {
  display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px;
}
.status-tile {
  border: none; border-radius: 16px; padding: 14px 8px 12px;
  display: flex; flex-direction: column; align-items: center; gap: 4px;
  cursor: pointer; transition: transform 0.1s, box-shadow 0.1s;
  box-shadow: 0 2px 8px rgba(0,0,0,0.12);
}
.status-tile:active { transform: scale(0.93); box-shadow: none; }
.status-tile-key   { font-size: 1.6rem; font-weight: 900; line-height: 1; }
.status-tile-label { font-size: 0.65rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; opacity: 0.85; }
.cancel-btn {
  display: block; width: 100%; padding: 12px;
  border: none; background: #f5f5f5; border-radius: 12px;
  font-size: 0.9rem; font-weight: 600; color: #666; cursor: pointer;
}
.cancel-btn:active { background: #eee; }

/* Semaines / mois */
.week-sep-header {
  background: #EEF2FF !important; color: #1976D2;
  font-size: 0.62rem !important; font-weight: 700 !important;
  border-left: 2px solid #BFDBFE !important; padding: 2px !important; height: 20px; text-align: center;
}
.monday-col { border-left: 2px solid #BFDBFE !important; }
.month-header {
  background: #EEF2FF !important; color: #1976D2;
  font-size: 0.7rem !important; font-weight: 700 !important;
  text-transform: capitalize; border-bottom: 2px solid #BFDBFE !important;
  padding: 3px 4px !important; text-align: center; position: sticky; top: 0; z-index: 3;
}

/* PDF modal */
.letter-spacing { letter-spacing: 0.06em; }
.pdf-list { max-height: 55vh; overflow-y: auto; }
.pdf-group-header {
  display: flex; align-items: center; padding: 8px 4px 4px; font-size: 0.72rem;
  border-bottom: 1px solid #E3F2FD; margin-bottom: 4px;
  position: sticky; top: 0; background: white; z-index: 1;
}
.pdf-option {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 12px; margin: 3px 0; border-radius: 10px; cursor: pointer;
  border: 1px solid transparent; transition: all 0.12s; gap: 8px;
}
.pdf-option:hover        { background: #F8FAFF; border-color: #E3F2FD; }
.pdf-option--active      { background: #EEF2FF !important; border-color: #BFDBFE !important; }
.pdf-option--recommended { background: #F0FFF4 !important; border-color: #BBF7D0 !important; }
.pdf-option--future      { opacity: 0.45; cursor: not-allowed; pointer-events: none; }
.pdf-radio     { width: 16px; height: 16px; flex-shrink: 0; border: 2px solid #ccc; border-radius: 50%; transition: all 0.12s; }
.pdf-radio--on { border-color: #1976D2; background: #1976D2; box-shadow: inset 0 0 0 3px white; }
.pdf-opt-title { font-weight: 600; font-size: 0.85rem; color: #1a1a1a; }
.pdf-opt-range { font-size: 0.72rem; color: #888; margin-top: 1px; }

/* Mobile */
@media (max-width: 600px) {
  .part-rate-col { display: none !important; }
  .name-col { min-width: 80px !important; max-width: 80px !important; padding: 4px !important; }
  .name { font-size: 0.65rem !important; line-height: 1.2 !important; }
  .trigramme { display: none; }
  .rate-col, .sticky-rate-col { display: none !important; }
  .actions-col { display: none !important; }
  .keyboard-hint { display: none !important; }
  .day-header { min-width: calc((100vw - 96px) / 5.5) !important; width: calc((100vw - 96px) / 5.5) !important; }
  .presence-cell { width: calc((100vw - 96px) / 5.5) !important; height: 50px !important; font-size: 0.85rem !important; font-weight: 900 !important; }
  .day-name { font-size: 0.58rem !important; }
  .day-date { font-size: 0.82rem !important; font-weight: 800 !important; }
  .d-none.d-md-table-cell { display: none !important; }
  .table-container { overflow-x: hidden !important; max-height: 65vh; }
  .user-info { padding: 2px 0; }
  .text-caption { font-size: 0.58rem !important; line-height: 1.2 !important; }
}
</style>