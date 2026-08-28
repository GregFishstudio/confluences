<template>
  <v-container fluid class="pa-4">
    <div class="d-flex align-center justify-space-between mb-6">
      <div>
        <div class="text-h5 font-weight-bold">Marrainages / Parrainages</div>
        <div class="text-caption text-grey">{{ filtered.length }} marraine·s/parrain·s affiché·e·s</div>
      </div>
      <div class="d-flex align-center ga-2">
        <v-btn
          icon variant="text" size="small" color="grey" :title="showAdminTools ? 'Masquer les outils' : 'Afficher les outils'"
          @click="showAdminTools = !showAdminTools"
        >
          <v-icon size="18">{{ showAdminTools ? 'mdi-cog' : 'mdi-cog-outline' }}</v-icon>
        </v-btn>

        <template v-if="showAdminTools">
          <v-dialog v-model="confirmClearDialog" max-width="480">
            <template #activator="{ props }">
              <v-btn v-bind="props" color="error" variant="outlined" prepend-icon="mdi-delete-sweep-outline">
                Vider la liste
              </v-btn>
            </template>
            <v-card rounded="lg">
              <v-card-title class="py-4">Vider tous les marrainages ?</v-card-title>
              <v-divider />
              <v-card-text class="pt-4">
                Cette action supprime définitivement les {{ marrainages.length }} marrainage(s) existant(s)
                (y compris leurs commentaires et pièces jointes). Impossible à annuler.
              </v-card-text>
              <v-divider />
              <v-card-actions class="d-flex justify-end">
                <v-btn variant="text" color="grey" @click="confirmClearDialog = false">Annuler</v-btn>
                <v-btn color="error" :loading="clearing" @click="clearAll">Tout supprimer</v-btn>
              </v-card-actions>
            </v-card>
          </v-dialog>
          <MarrainageImportDialog @imported="load" />
          <MarrainageQuestionnaireImportDialog @imported="load" />
          <MarrainageDuplicatesDialog @merged="load" />
          <MarrainageFileBulkImport @imported="load" />
        </template>

        <v-btn color="primary" prepend-icon="mdi-account-plus-outline" :to="{ name: 'Marrainage-Nouveau' }">
          Nouveau marrainage
        </v-btn>
      </div>
    </div>

    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-card-text class="pa-4">
        <v-row dense>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="search"
              prepend-inner-icon="mdi-magnify"
              label="Rechercher (nom, prénom, commune, participant·e...)"
              variant="outlined" density="compact" clearable hide-details
              @update:model-value="load"
            />
          </v-col>
          <v-col cols="6" md="3">
            <v-select
              v-model="filterActif"
              :items="[
                { title: 'Tous', value: 'all' },
                { title: 'Actifs', value: 'actif' },
                { title: 'Inactifs', value: 'inactif' },
              ]"
              label="Statut"
              variant="outlined" density="compact" hide-details
              @update:model-value="load"
            />
          </v-col>
          <v-col cols="6" md="3">
            <v-select
              v-model="sortMode"
              :items="[
                { title: 'Nom (A → Z)', value: 'nom' },
                { title: 'Date de démarrage (récent → ancien)', value: 'date' },
              ]"
              label="Trier par"
              variant="outlined" density="compact" hide-details
            />
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card class="elevation-2" rounded="lg" :loading="isLoading">
      <v-data-table
        :headers="headers"
        :items="sorted"
        :items-per-page="25"
        v-model:page="page"
        hover
        @click:row="onRowClick"
        class="marrainages-table"
      >
        <template #item.nom="{ item }">
          <div class="font-weight-bold">{{ item.nom }}</div>
          <div class="text-caption text-grey">{{ item.email }}</div>
        </template>
        <template #item.numero="{ item }">
          {{ item.numero || '—' }}
        </template>
        <template #item.participant="{ item }">
          <div v-if="linkedParticipants(item).length">
            <div v-for="p in linkedParticipants(item)" :key="p.id">
              <a href="#" class="participant-link" @click.stop.prevent="goToParticipant(p.id)">{{ p.firstname }} {{ p.lastName }}</a>
            </div>
          </div>
          <span v-else-if="item.filleulNom">{{ item.filleulNom }}</span>
          <span v-else class="text-grey text-caption">—</span>
        </template>
        <template #item.actif="{ item }">
          <v-chip size="small" :color="item.actif ? 'success' : 'error'" label :variant="item.actif ? 'tonal' : 'flat'">
            {{ item.actif ? 'Actif' : 'Inactif' }}
          </v-chip>
        </template>
        <template #item.statut="{ item }">
          <v-chip v-if="item.statut" size="small" color="orange" label variant="tonal">
            {{ statutLabel(item.statut) }}
          </v-chip>
          <span v-else class="text-grey text-caption">—</span>
        </template>
        <template #item.date="{ item }">
          {{ formatDate(item.date) }}
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useStore } from 'vuex'
import moment from 'moment/min/moment-with-locales'
import MarrainageImportDialog from '@/components/MarrainageImportDialog.vue'
import MarrainageQuestionnaireImportDialog from '@/components/MarrainageQuestionnaireImportDialog.vue'
import MarrainageDuplicatesDialog from '@/components/MarrainageDuplicatesDialog.vue'
import MarrainageFileBulkImport from '@/components/MarrainageFileBulkImport.vue'

moment.locale('fr')
const store = useStore()
const router = useRouter()
const route = useRoute()

const isLoading = ref(false)
const search = ref('')
const filterActif = ref('all')
const sortMode = ref('nom')
const confirmClearDialog = ref(false)
const showAdminTools = ref(false)
const clearing = ref(false)

// Page courante du tableau, reflétée dans l'URL (?page=2) pour que le bouton "retour"
// depuis une fiche marrainage restaure la bonne page plutôt que de revenir à la page 1.
const page = ref(Number(route.query.page) || 1)
watch(page, (p) => {
  router.replace({ query: { ...route.query, page: p !== 1 ? String(p) : undefined } })
})

const headers = [
  { title: 'Prénom', key: 'prenom', sortable: true },
  { title: 'Nom', key: 'nom', sortable: true },
  { title: 'No de téléphone', key: 'numero', sortable: true },
  { title: 'Commune', key: 'commune', sortable: true },
  { title: 'Participant·e·s associé·e·s', key: 'participant', sortable: false },
  { title: 'Statut', key: 'actif', sortable: true },
  { title: 'En attente / à relancer', key: 'statut', sortable: true },
  { title: 'Date de démarrage', key: 'date', sortable: true },
]

const STATUT_LABELS = { en_attente: 'En attente', a_relancer: 'À relancer' }
const statutLabel = (s) => STATUT_LABELS[s] || s

const marrainages = computed(() => store.state.marrainage?.marrainages || [])
const filtered = computed(() => marrainages.value)

const sorted = computed(() => {
  const list = [...filtered.value]
  if (sortMode.value === 'date') {
    return list.sort((a, b) => new Date(b.date || 0) - new Date(a.date || 0))
  }
  return list.sort((a, b) => (a.nom || '').localeCompare(b.nom || '', 'fr'))
})

const linkedParticipants = (item) => {
  const links = (item.participantLinks || []).map(pl => pl.participant).filter(Boolean)
  if (item.participant && !links.some(p => p.id === item.participant.id)) {
    links.unshift(item.participant)
  }
  return links
}

const goToParticipant = (id) => {
  router.push({ name: 'Participant-Detail', params: { id } })
}

const formatDate = (d) => d ? moment(d).format('DD.MM.YYYY') : '—'

const load = () => {
  isLoading.value = true
  const params = {}
  if (search.value) params.search = search.value
  if (filterActif.value === 'actif') params.actif = true
  if (filterActif.value === 'inactif') params.actif = false
  store.dispatch('marrainage/fetchMarrainages', params).finally(() => { isLoading.value = false })
}

const goToDetail = (item) => {
  router.push({ name: 'Marrainage-Modifier', params: { id: item.marrainageId } })
}

// Ignore le clic de ligne s'il vient d'un lien participant imbriqué (le stopPropagation
// sur le lien ne suffit pas toujours, la ligne du tableau peut intercepter le clic avant).
const onRowClick = (event, { item }) => {
  if (event.target.closest('.participant-link')) return
  goToDetail(item)
}

const clearAll = async () => {
  clearing.value = true
  try {
    await store.dispatch('marrainage/clearMarrainages')
    confirmClearDialog.value = false
  } finally {
    clearing.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.marrainages-table :deep(tbody tr) { cursor: pointer; }
.marrainages-table :deep(tbody tr:hover) { background: #f5f5f5 !important; }
.participant-link { color: #1976D2; text-decoration: underline; }
</style>
