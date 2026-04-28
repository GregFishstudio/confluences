<template>
  <v-container fluid class="pa-4">

    <!-- ─── Header ─────────────────────────────────────────────────────────── -->
    <div class="d-flex align-center justify-space-between mb-6">
      <div>
        <div class="text-h5 font-weight-bold">Participants</div>
        <div class="text-caption text-grey">{{ filteredUsers.length }} participant·e·s affiché·e·s</div>
      </div>
    </div>

    <!-- ─── Filtres ─────────────────────────────────────────────────────────── -->
    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-card-text class="pa-4">
        <v-row dense>
          <v-col cols="12" md="4">
            <v-text-field
              v-model="search"
              prepend-inner-icon="mdi-magnify"
              label="Rechercher (nom, prénom, email...)"
              variant="outlined" density="compact" clearable hide-details
            />
          </v-col>
          <v-col cols="6" md="2">
            <v-select
              v-model="filterNiveau"
              :items="niveauOptions"
              label="Mesure"
              variant="outlined" density="compact" clearable hide-details
            />
          </v-col>
          <v-col cols="6" md="2">
            <v-select
              v-model="filterSessionNumber"
              :items="sessionOptions"
              label="N° Session"
              variant="outlined" density="compact" clearable hide-details
            />
          </v-col>
          <v-col cols="6" md="2">
            <v-select
              v-model="filterYear"
              :items="yearOptions"
              label="Année"
              variant="outlined" density="compact" clearable hide-details
            />
          </v-col>
          <v-col cols="6" md="2">
            <v-select
              v-model="filterActif"
              :items="[
                { title: 'Tous', value: 'all' },
                { title: 'En session', value: 'actif' },
                { title: 'Hors session', value: 'inactif' },
              ]"
              label="Statut session"
              variant="outlined" density="compact" hide-details
            />
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- ─── Tableau ─────────────────────────────────────────────────────────── -->
    <v-card class="elevation-2" rounded="lg" :loading="isLoading">
      <v-data-table
        :headers="headers"
        :items="filteredUsers"
        :items-per-page="25"
        hover
        @click:row="(_, { item }) => openDetail(item)"
        class="participants-table"
      >
        <!-- Nom complet -->
        <template #item.fullName="{ item }">
          <div class="d-flex align-center py-2">
            <v-avatar size="36" :color="avatarColor(item)" class="mr-3">
              <span class="text-caption font-weight-bold text-white">{{ getInitials(item) }}</span>
            </v-avatar>
            <div>
              <div class="font-weight-bold">{{ item.prenom }} {{ item.nom }}</div>
              <div class="text-caption text-grey">{{ item.userName }}</div>
            </div>
          </div>
        </template>

        <!-- Contact -->
        <template #item.contact="{ item }">
          <div class="text-caption">
            <div v-if="item.email">
              <v-icon size="x-small" class="mr-1">mdi-email-outline</v-icon>{{ item.email }}
            </div>
            <div v-if="item.phoneNumber">
              <v-icon size="x-small" class="mr-1">mdi-phone-outline</v-icon>{{ item.phoneNumber }}
            </div>
          </div>
        </template>

        <!-- Mesure -->
        <template #item.niveau="{ item }">
          <v-chip v-if="item.niveau && item.niveau !== 'N/A'" size="small" :color="niveauColor(item.niveau)" label>
            {{ item.niveau }}
          </v-chip>
          <span v-else class="text-grey text-caption">—</span>
        </template>

        <!-- Session -->
        <template #item.session="{ item }">
          <div class="text-caption">
            <v-chip v-if="isActif(item)" size="small" color="success" label variant="tonal">
              S{{ item.currentSessionNumber }} — {{ item.currentSessionYear }}
            </v-chip>
            <span v-else class="text-grey">—</span>
          </div>
        </template>

        <!-- Nationalité -->
        <template #item.nationalite="{ item }">
          <div class="text-caption">{{ item.nationality || '—' }}</div>
        </template>

        <!-- Actions -->
        <template #item.actions="{ item }">
          <v-btn icon size="small" variant="text" @click.stop="openDetail(item)">
            <v-icon size="small">mdi-eye-outline</v-icon>
          </v-btn>
        </template>
      </v-data-table>
    </v-card>

    <!-- ─── Drawer détail ───────────────────────────────────────────────────── -->
    <v-navigation-drawer v-model="detailDrawer" location="right" width="500" temporary>
      <template v-if="selectedUser">

        <!-- Header drawer -->
        <div class="pa-5 bg-primary">
          <div class="d-flex align-center justify-space-between mb-3">
            <v-btn icon variant="text" color="white" @click="detailDrawer = false">
              <v-icon>mdi-close</v-icon>
            </v-btn>
            <v-btn icon variant="text" color="white" @click="copyContact">
              <v-icon>mdi-content-copy</v-icon>
            </v-btn>
          </div>
          <div class="d-flex align-center">
            <v-avatar size="56" color="white" class="mr-4">
              <span class="text-h6 font-weight-bold" :style="{ color: avatarColor(selectedUser) }">
                {{ getInitials(selectedUser) }}
              </span>
            </v-avatar>
            <div>
              <div class="text-h6 font-weight-bold text-white">
                {{ selectedUser.prenom }} {{ selectedUser.nom }}
              </div>
              <div class="text-caption text-white opacity-80">{{ selectedUser.userName }}</div>
              <v-chip v-if="isActif(selectedUser)" size="x-small" color="white" variant="outlined" class="mt-1">
                S{{ selectedUser.currentSessionNumber }} — {{ selectedUser.currentSessionYear }}
              </v-chip>
              <v-chip v-else size="x-small" color="white" variant="outlined" class="mt-1 opacity-60">
                Hors session
              </v-chip>
            </div>
          </div>
        </div>

        <v-list class="pa-0">

          <!-- ── Infos personnelles ── -->
          <v-list-subheader class="bg-grey-lighten-4 font-weight-bold text-uppercase text-caption px-5">
            Informations personnelles
          </v-list-subheader>
          <div class="pa-4">
            <v-row dense>
              <v-col cols="6">
                <div class="text-caption text-grey mb-1">Email</div>
                <div class="text-body-2">{{ selectedUser.email || '—' }}</div>
              </v-col>
              <v-col cols="6">
                <div class="text-caption text-grey mb-1">Téléphone</div>
                <div class="text-body-2">{{ selectedUser.phoneNumber || '—' }}</div>
              </v-col>
              <v-col cols="6" class="mt-3">
                <div class="text-caption text-grey mb-1">Date de naissance</div>
                <div class="text-body-2">{{ formatDate(selectedUser.birthday) }}</div>
              </v-col>
              <v-col cols="6" class="mt-3">
                <div class="text-caption text-grey mb-1">Nationalité</div>
                <div class="text-body-2">{{ selectedUser.nationality || '—' }}</div>
              </v-col>
              <v-col cols="6" class="mt-3">
                <div class="text-caption text-grey mb-1">Langue</div>
                <div class="text-body-2">{{ selectedUser.language || '—' }}</div>
              </v-col>
              <v-col cols="6" class="mt-3">
                <div class="text-caption text-grey mb-1">Début participation</div>
                <div class="text-body-2">{{ formatDate(selectedUser.dateDebutParticipation) }}</div>
              </v-col>
              <v-col cols="12" class="mt-3">
                <div class="text-caption text-grey mb-1">Affiliation</div>
                <div class="text-body-2">{{ selectedUser.typeAffiliation?.name || '—' }}</div>
              </v-col>
            </v-row>
          </div>

          <v-divider />

          <!-- ── Mesure ── -->
          <v-list-subheader class="bg-grey-lighten-4 font-weight-bold text-uppercase text-caption px-5">
            Mesure
          </v-list-subheader>
          <div class="pa-4">
            <v-chip
              v-if="selectedUser.niveau && selectedUser.niveau !== 'N/A'"
              size="small" :color="niveauColor(selectedUser.niveau)" label
            >
              {{ selectedUser.niveau }}
            </v-chip>
            <span v-else class="text-caption text-grey">—</span>
          </div>

          <v-divider />

          <!-- ── Niveaux oral/écrit session en cours ── -->
          <v-list-subheader class="bg-grey-lighten-4 font-weight-bold text-uppercase text-caption px-5">
            Niveaux — session en cours
          </v-list-subheader>
          <div class="pa-4">
            <div v-if="getCurrentNiveau(selectedUser)">
              <v-row dense>
                <v-col cols="6">
                  <div class="text-caption text-grey mb-1">Niveau oral</div>
                  <v-chip size="small" color="green" label>
                    {{ getCurrentNiveau(selectedUser)?.oralLevel || '—' }}
                  </v-chip>
                </v-col>
                <v-col cols="6">
                  <div class="text-caption text-grey mb-1">Niveau écrit</div>
                  <v-chip size="small" color="blue" label>
                    {{ getCurrentNiveau(selectedUser)?.writtenLevel || '—' }}
                  </v-chip>
                </v-col>
              </v-row>
            </div>
            <div v-else class="text-caption text-grey">Aucun niveau enregistré pour la session en cours.</div>
          </div>

          <v-divider />

          <!-- ── Historique sessions ── -->
          <v-list-subheader class="bg-grey-lighten-4 font-weight-bold text-uppercase text-caption px-5">
            Historique des sessions
          </v-list-subheader>
          <div class="pa-4">
            <div v-if="userSessionLinks.length === 0" class="text-caption text-grey">
              Aucune session trouvée.
            </div>
            <v-timeline density="compact" side="end" v-else>
              <v-timeline-item
                v-for="link in userSessionLinks"
                :key="link.id"
                size="x-small"
                dot-color="primary"
              >
                <div class="text-body-2 font-weight-bold">
                  Session {{ link.session?.sessionNumberId || link.Session?.SessionNumberId }}
                  — {{ getSessionYear(link) }}
                </div>
                <div class="text-caption text-grey">
                  {{ formatDate(link.session?.dateStart || link.Session?.DateStart) }}
                  → {{ formatDate(link.session?.dateEnd || link.Session?.DateEnd) }}
                </div>
                <div class="mt-1" v-if="getNiveauForSession(link)">
                  <v-chip size="x-small" color="green" label class="mr-1">
                    O: {{ getNiveauForSession(link)?.oralLevel || '—' }}
                  </v-chip>
                  <v-chip size="x-small" color="blue" label>
                    É: {{ getNiveauForSession(link)?.writtenLevel || '—' }}
                  </v-chip>
                </div>
              </v-timeline-item>
            </v-timeline>
          </div>

          <v-divider />

          <!-- ── Stages ── -->
          <v-list-subheader class="bg-grey-lighten-4 font-weight-bold text-uppercase text-caption px-5">
            Stages associés
          </v-list-subheader>
          <div class="pa-4">
            <div v-if="!userStages.length" class="text-caption text-grey">Aucun stage.</div>
            <div v-for="stage in userStages" :key="stage.id" class="mb-2">
              <div class="text-body-2 font-weight-bold">
                {{ stage.entreprise?.nom || 'Stage' }}
              </div>
              <div class="text-caption text-grey">
                {{ formatDate(stage.dateDebut) }}
                <span v-if="stage.dateFin"> → {{ formatDate(stage.dateFin) }}</span>
              </div>
            </div>
          </div>

        </v-list>
      </template>
    </v-navigation-drawer>

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useStore } from 'vuex'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'

moment.locale('fr')
const store = useStore()

// ─── State ────────────────────────────────────────────────────────────────────
const isLoading           = ref(false)
const search              = ref('')
const filterNiveau        = ref(null)
const filterSessionNumber = ref(null)
const filterYear          = ref(null)
const filterActif         = ref('all')
const detailDrawer        = ref(false)
const selectedUser        = ref(null)
const allSessionLinks     = ref([])
const allStages           = ref([])
const allNiveaux          = ref([])

// ─── Store ────────────────────────────────────────────────────────────────────
const users = computed(() => store.state.user?.users || [])

// ─── Headers ─────────────────────────────────────────────────────────────────
const headers = [
  { title: 'Participant·e', key: 'fullName',    sortable: true  },
  { title: 'Contact',       key: 'contact',     sortable: false },
  { title: 'Mesure',        key: 'niveau',      sortable: false },
  { title: 'Session',       key: 'session',     sortable: true  },
  { title: 'Nationalité',   key: 'nationalite', sortable: true  },
  { title: '',              key: 'actions',     sortable: false },
]

// ─── Options filtres ──────────────────────────────────────────────────────────
const niveauOptions = computed(() => {
  const niveaux = [...new Set(users.value.map(u => u.niveau).filter(n => n && n !== 'N/A'))]
  return niveaux.sort()
})

const sessionOptions = computed(() =>
  [1, 2, 3, 4].map(n => ({ title: `Session ${n}`, value: n }))
)

const yearOptions = computed(() => {
  const years = [...new Set(users.value.map(u => u.currentSessionYear).filter(Boolean))]
  return years.sort((a, b) => b - a)
})

// ─── Helpers ─────────────────────────────────────────────────────────────────
const isActif = (user) =>
  !!user.currentSessionNumber && !!user.currentSessionYear

const getCurrentNiveau = (user) => {
  if (!isActif(user)) return null
  return allNiveaux.value.find(n =>
    n.stagiaireId === user.id &&
    n.annee       === user.currentSessionYear &&
    n.trimestre   === user.currentSessionNumber
  ) || null
}

const getNiveauForSession = (link) => {
  const s      = link.session || link.Session
  const year   = s ? new Date(s.dateStart || s.DateStart).getFullYear() : null
  const num    = s?.sessionNumberId || s?.SessionNumberId
  const userId = link.studentId || link.StudentId
  if (!year || !num) return null
  return allNiveaux.value.find(n =>
    n.stagiaireId === userId &&
    n.annee       === year &&
    n.trimestre   === num
  ) || null
}

const getSessionYear = (link) => {
  const s = link.session || link.Session
  if (!s) return '—'
  return new Date(s.dateStart || s.DateStart).getFullYear()
}

// ─── Filtrage ─────────────────────────────────────────────────────────────────
const filteredUsers = computed(() => {
  let list = users.value

  if (filterActif.value === 'actif') {
    list = list.filter(u => isActif(u))
  } else if (filterActif.value === 'inactif') {
    list = list.filter(u => !isActif(u))
  }

  if (filterSessionNumber.value) {
    list = list.filter(u => u.currentSessionNumber === filterSessionNumber.value)
  }

  if (filterYear.value) {
    list = list.filter(u => u.currentSessionYear === filterYear.value)
  }

  if (filterNiveau.value) {
    list = list.filter(u => u.niveau === filterNiveau.value)
  }

  if (search.value) {
    const s = search.value.toLowerCase()
    list = list.filter(u =>
      (u.prenom      || '').toLowerCase().includes(s) ||
      (u.nom         || '').toLowerCase().includes(s) ||
      (u.email       || '').toLowerCase().includes(s) ||
      (u.userName    || '').toLowerCase().includes(s) ||
      (u.phoneNumber || '').toLowerCase().includes(s) ||
      (u.nationality || '').toLowerCase().includes(s)
    )
  }

  return list
})

// ─── Détail ───────────────────────────────────────────────────────────────────
const userSessionLinks = computed(() => {
  if (!selectedUser.value) return []
  return allSessionLinks.value
    .filter(l =>
      String(l.studentId || l.StudentId).toLowerCase() ===
      String(selectedUser.value.id).toLowerCase()
    )
    .sort((a, b) => {
      const aY = new Date(a.session?.dateStart || a.Session?.DateStart || 0).getFullYear()
      const bY = new Date(b.session?.dateStart || b.Session?.DateStart || 0).getFullYear()
      const aN = a.session?.sessionNumberId || a.Session?.SessionNumberId || 0
      const bN = b.session?.sessionNumberId || b.Session?.SessionNumberId || 0
      return bY - aY || bN - aN
    })
})

const userStages = computed(() => {
  if (!selectedUser.value) return []
  return allStages.value.filter(s =>
    String(s.stagiaireId || '').toLowerCase() ===
    String(selectedUser.value.id).toLowerCase()
  )
})

// ─── Helpers UI ───────────────────────────────────────────────────────────────
const COLORS = ['#1565C0', '#2E7D32', '#6A1B9A', '#E65100', '#00695C', '#AD1457']
const avatarColor = (user) =>
  COLORS[((user.prenom || user.firstname || '')[0] || '').charCodeAt(0) % COLORS.length]

const getInitials = (u) =>
  `${(u.prenom || u.firstname || '')[0] || ''}${(u.nom || u.lastName || '')[0] || ''}`.toUpperCase() || 'U'

const formatDate = (d) => d ? moment(d).format('DD.MM.YYYY') : '—'

const niveauColor = (niveau) => {
  if (!niveau) return 'grey'
  const n = niveau.toLowerCase()
  if (n.includes('télé') || n.includes('tele')) return 'purple'
  if (n.includes('a1'))  return 'green'
  if (n.includes('a2'))  return 'blue'
  if (n.includes('b1'))  return 'orange'
  if (n.includes('b2'))  return 'deep-orange'
  return 'grey'
}

const openDetail = (user) => {
  selectedUser.value = user
  detailDrawer.value = true
}

const copyContact = () => {
  if (!selectedUser.value) return
  const u = selectedUser.value
  const text = `${u.prenom} ${u.nom}\n${u.email || ''}\n${u.phoneNumber || ''}`
  navigator.clipboard.writeText(text)
}

// ─── API ──────────────────────────────────────────────────────────────────────
const loadSessionLinks = async () => {
  try {
    const res = await fetch(`${API_BASE}/api/SessionStudents`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('access_token')}` },
    })
    if (res.ok) allSessionLinks.value = await res.json()
  } catch (e) { console.error(e) }
}

const loadStages = async () => {
  try {
    const res = await fetch(`${API_BASE}/api/stages`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('access_token')}` },
    })
    if (res.ok) allStages.value = await res.json()
  } catch (e) { console.error(e) }
}

const loadNiveaux = async () => {
  try {
    const res = await fetch(`${API_BASE}/api/StagiaireNiveaux`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('access_token')}` },
    })
    if (res.ok) allNiveaux.value = await res.json()
  } catch (e) { console.error(e) }
}

onMounted(async () => {
  isLoading.value = true
  try {
    await store.dispatch('user/fetchUsers')
    await Promise.all([loadSessionLinks(), loadStages(), loadNiveaux()])
  } catch (e) {
    console.error(e)
  } finally {
    isLoading.value = false
  }
})
</script>

<style scoped>
.participants-table :deep(tbody tr) { cursor: pointer; }
.participants-table :deep(tbody tr:hover) { background: #f5f5f5 !important; }
</style>