<template>
  <v-container fluid class="pa-4">

    <!-- ─── Header ─────────────────────────────────────────────────────────── -->
    <div class="d-flex align-center justify-space-between mb-6">
      <div>
        <div class="text-h5 font-weight-bold">Liste des participant·e·s</div>
        <div class="text-caption text-grey">{{ sortedUsers.length }} participant·e·s affiché·e·s</div>
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
          <v-col cols="6" md="2">
            <v-select
              v-model="sortMode"
              :items="[
                { title: 'Nom de famille (A → Z)', value: 'nom' },
                { title: 'Plus récent·e·s d\'abord', value: 'recent' },
              ]"
              label="Trier par"
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
        :items="sortedUsers"
        :items-per-page="25"
        hover
        @click:row="(_, { item }) => openDetail(item)"
        class="participants-table"
      >
        <!-- Prénom -->
        <template #item.prenom="{ item }">
          <div class="d-flex align-center py-2">
            <v-avatar size="36" :color="avatarColor(item)" class="mr-3">
              <span class="text-caption font-weight-bold text-white">{{ getInitials(item) }}</span>
            </v-avatar>
            <div>
              <div class="font-weight-bold">{{ item.prenom }}</div>
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
        <template #item.currentSessionYear="{ item }">
          <div class="text-caption">
            <v-chip v-if="isActif(item)" size="small" color="success" label variant="tonal">
              S{{ item.currentSessionNumber }} — {{ item.currentSessionYear }}
            </v-chip>
            <span v-else class="text-grey">—</span>
          </div>
        </template>

        <!-- Nationalité -->
        <template #item.nationality="{ item }">
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

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { matchesSearch } from '@/utils/search'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import moment from 'moment/min/moment-with-locales'

const router = useRouter()

moment.locale('fr')
const store = useStore()

// ─── State ────────────────────────────────────────────────────────────────────
const isLoading           = ref(false)
const search              = ref('')
const filterNiveau        = ref(null)
const filterSessionNumber = ref(null)
const filterYear          = ref(null)
const filterActif         = ref('all')
const sortMode            = ref('nom')

// ─── Store ────────────────────────────────────────────────────────────────────
const users = computed(() => store.state.user?.users || [])

// ─── Headers ─────────────────────────────────────────────────────────────────
const headers = [
  { title: 'Prénom',        key: 'prenom',             sortable: true  },
  { title: 'Nom',           key: 'nom',                sortable: true  },
  { title: 'Contact',       key: 'contact',            sortable: false },
  { title: 'Mesure',        key: 'niveau',             sortable: true  },
  { title: 'Session',       key: 'currentSessionYear', sortable: true  },
  { title: 'Nationalité',   key: 'nationality',        sortable: true  },
  { title: '',              key: 'actions',            sortable: false },
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
    list = list.filter(u =>
      matchesSearch(search.value, u.prenom, u.nom, u.email, u.userName, u.phoneNumber, u.nationality))
  }

  return list
})

const sortedUsers = computed(() => {
  const list = [...filteredUsers.value]
  if (sortMode.value === 'recent') {
    return list.sort((a, b) => new Date(b.dateDebutParticipation || 0) - new Date(a.dateDebutParticipation || 0))
  }
  return list.sort((a, b) => (a.nom || '').localeCompare(b.nom || '', 'fr'))
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
  router.push({ name: 'Participant-Detail', params: { id: user.id } })
}

onMounted(async () => {
  isLoading.value = true
  try {
    await store.dispatch('user/fetchUsers')
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