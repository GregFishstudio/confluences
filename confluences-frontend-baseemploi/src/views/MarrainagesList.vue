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
        <v-row dense align="center">
          <v-col cols="12" md="4">
            <v-text-field
              v-model="search"
              prepend-inner-icon="mdi-magnify"
              label="Rechercher (nom, prénom, commune, participant·e...)"
              variant="outlined" density="compact" clearable hide-details
              @update:model-value="page = 1"
            />
          </v-col>
          <v-col cols="6" md="2">
            <v-select
              v-model="filterStatut"
              :items="[{ title: 'Tous', value: 'all' }, { title: 'Prendre des nouvelles', value: 'nouvelles' }, ...MARRAINAGE_STATUTS, { title: 'Sans statut', value: 'none' }]"
              label="Statut"
              variant="outlined" density="compact" hide-details
              @update:model-value="page = 1"
            />
          </v-col>
          <v-col cols="6" md="2">
            <v-select
              v-model="filterRegion" :items="regionNames" label="Région"
              variant="outlined" density="compact" hide-details clearable
              @update:model-value="page = 1"
            />
          </v-col>
          <v-col cols="6" md="2">
            <v-select
              v-model="sortMode"
              :items="[
                { title: 'À traiter en premier', value: 'suivi' },
                { title: 'Nom (A → Z)', value: 'nom' },
                { title: 'Début du marrainage (récent → ancien)', value: 'date' },
                { title: 'Région', value: 'region' },
              ]"
              label="Trier par"
              variant="outlined" density="compact" hide-details
            />
          </v-col>
          <v-col cols="6" md="2" class="d-flex justify-end">
            <v-btn :to="{ name: 'Marrainage-Tableau' }" variant="tonal" color="primary" prepend-icon="mdi-map-outline" size="small">
              Tableau par région
            </v-btn>
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <v-card class="elevation-2" rounded="lg" :loading="isLoading">
      <v-data-table
        :headers="headers"
        :items="sorted"
        v-model:items-per-page="itemsPerPage"
        v-model:page="page"
        hover
        :row-props="rowProps"
        @click:row="onRowClick"
        class="marrainages-table"
      >
        <template #item.nom="{ item }">
          <router-link :to="detailRoute(item)" class="detail-link font-weight-bold">{{ item.nom }} {{ item.prenom }}</router-link>
          <div v-if="item.numero" class="text-caption"><v-icon size="12" class="mr-1">mdi-phone-outline</v-icon>{{ item.numero }}</div>
          <div v-if="item.email" class="text-caption text-grey"><v-icon size="12" class="mr-1">mdi-email-outline</v-icon>{{ item.email }}</div>
        </template>
        <template #item.commune="{ item }">
          <v-chip v-if="item.region" size="small" label variant="flat" :color="regionColor(item.region)" class="font-weight-bold mb-1">{{ item.region }}</v-chip>
          <div class="text-body-2">{{ item.commune || '—' }}</div>
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
        <template #item.statut="{ item }">
          <div class="d-flex flex-column align-start ga-1">
            <v-chip v-if="item.nouvelles" size="small" label color="red" variant="flat" prepend-icon="mdi-bell-ring-outline" :title="`Dernière action : ${formatDate(item.derniereActivite)}`">
              Prendre des nouvelles
            </v-chip>
            <v-chip
              v-for="st in item.statuts" :key="st"
              size="small" label
              :color="statutInfo(st).color"
              :variant="st === 'en_attente' ? 'flat' : 'tonal'"
            >
              {{ statutInfo(st).title }}
            </v-chip>
            <span v-if="!item.statuts.length && !item.nouvelles" class="text-grey text-caption">—</span>
          </div>
        </template>
        <template #item.date="{ item }">
          {{ formatMoisAnnee(item.date) || '—' }}
        </template>
        <template #item.dateFin="{ item }">
          {{ formatMoisAnnee(item.dateFin) || '—' }}
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
import {
  MARRAINAGE_STATUTS, statutInfo, effectiveStatut, statutsAffiches, dateDebutMarrainage, dateFinMarrainage,
  prendreDesNouvelles, derniereActivite,
} from '@/utils/marrainageStatut'
import { useRegions, regionMarrainage } from '@/utils/regions'
import { matchesSearch } from '@/utils/search'
import { useListMemory } from '@/composables/useListMemory'

moment.locale('fr')
const store = useStore()
const router = useRouter()
const route = useRoute()

const isLoading = ref(false)
const search = ref('')
const filterStatut = ref('all')
const sortMode = ref('suivi')
const filterRegion = ref(null)
const itemsPerPage = ref(25)
const { regionNames, regionColor } = useRegions()
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
  { title: 'Parrain / marraine', key: 'nom', sortable: true },
  { title: 'Région · commune', key: 'commune', sortable: true, sort: (a, b) => (a || '').localeCompare(b || '', 'fr') },
  { title: 'Participant·e·s associé·e·s', key: 'participant', sortable: false },
  { title: 'Statut', key: 'statut', sortable: true, sort: (a, b) => statutRank(a) - statutRank(b) },
  { title: 'Début du marrainage', key: 'date', sortable: true },
  { title: 'Fin du marrainage', key: 'dateFin', sortable: true },
]

const statutRank = (s) => { const i = MARRAINAGE_STATUTS.findIndex(x => x.value === s); return i < 0 ? 99 : i }

const rawMarrainages = computed(() => store.state.marrainage?.marrainages || [])

// `statut` affiché = "Actif·ve" dès qu'une relation de marrainage est active (l'activité se
// suit par relation), sinon le statut choisi sur la fiche. `date` / `dateFin` = début de la
// relation la plus récente / fin de la dernière relation quand plus aucune n'est active.
const marrainages = computed(() =>
  rawMarrainages.value.map(m => ({
    ...m,
    statut: effectiveStatut(m),
    statuts: statutsAffiches(m),
    date: dateDebutMarrainage(m),
    dateFin: dateFinMarrainage(m),
    region: regionMarrainage(m),
    nouvelles: prendreDesNouvelles(m),
    derniereActivite: derniereActivite(m),
  }))
)
// Recherche tolérante faite ici plutôt que par l'API (qui exigeait l'orthographe exacte :
// "Jean Marie" ne trouvait pas "Jean-Marie") : accents, tirets, ordre des mots, petite faute.
const searchFields = (m) => [
  m.nom, m.prenom, m.commune, m.region, m.email, m.numero, m.filleulNom,
  ...linkedParticipants(m).map(p => `${p.firstname} ${p.lastName}`),
]
const filtered = computed(() => {
  let list = marrainages.value
  if (search.value) list = list.filter(m => matchesSearch(search.value, searchFields(m)))
  if (filterRegion.value) list = list.filter(m => (m.region || 'Autres') === filterRegion.value)
  if (filterStatut.value === 'all') return list
  if (filterStatut.value === 'nouvelles') return list.filter(m => m.nouvelles)
  if (filterStatut.value === 'none') return list.filter(m => !m.statuts.length)
  return list.filter(m => m.statuts.includes(filterStatut.value))
})

// Les marraines/parrains en attente de binôme et les nouveaux contacts ressortent en couleur ;
// "Prendre des nouvelles" en rouge.
const rowProps = ({ item }) => (
  item.nouvelles ? { class: 'row-nouvelles' }
    : item.statut === 'en_attente' ? { class: 'row-en-attente' }
    : item.statut === 'nouveau' ? { class: 'row-nouveau' }
    : {}
)

// Ordre par défaut "à traiter en premier" : prendre des nouvelles, puis nouveaux contacts et
// binômes à former, puis le reste par nom.
const priorite = (m) => m.nouvelles ? 0 : m.statut === 'nouveau' ? 1 : m.statut === 'en_attente' ? 2 : 3
const byNom = (a, b) => (a.nom || '').localeCompare(b.nom || '', 'fr')
const sorted = computed(() => {
  const list = [...filtered.value]
  if (sortMode.value === 'date') return list.sort((a, b) => new Date(b.date || 0) - new Date(a.date || 0))
  if (sortMode.value === 'region') return list.sort((a, b) => (a.region || '~').localeCompare(b.region || '~', 'fr') || byNom(a, b))
  if (sortMode.value === 'suivi') return list.sort((a, b) => priorite(a) - priorite(b) || byNom(a, b))
  return list.sort(byNom)
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
// Début / fin de marrainage : au mois près.
const formatMoisAnnee = (d) => d ? moment(d).format('MM.YYYY') : ''

// Retour depuis une fiche : mêmes filtres et même position (la page est déjà dans l'adresse).
useListMemory(
  'marrainages',
  { search, filterStatut, sortMode, filterRegion, itemsPerPage },
  { ready: () => !isLoading.value && marrainages.value.length > 0 },
)

const load = () => {
  isLoading.value = true
  store.dispatch('marrainage/fetchMarrainages', {}).finally(() => { isLoading.value = false })
}

const detailRoute = (item) => ({ name: 'Marrainage-Modifier', params: { id: item.marrainageId } })

const goToDetail = (item) => {
  router.push(detailRoute(item))
}

// Ignore le clic de ligne s'il vient d'un lien imbriqué (participant·e ou nom), qui gère
// lui-même la navigation — sinon deux navigations partent en même temps et s'annulent.
const onRowClick = (event, { item }) => {
  if (event.target.closest('a')) return
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
.detail-link { color: inherit; text-decoration: none; }
.detail-link:hover { text-decoration: underline; }
.marrainages-table :deep(tbody tr.row-en-attente) { background: #FFF3E0; }
.marrainages-table :deep(tbody tr.row-nouveau) { background: #E3F2FD; }
.marrainages-table :deep(tbody tr.row-nouveau td:first-child) { box-shadow: inset 4px 0 0 #1E88E5; }
.marrainages-table :deep(tbody tr.row-nouvelles) { background: #FFEBEE; }
.marrainages-table :deep(tbody tr.row-nouvelles td:first-child) { box-shadow: inset 4px 0 0 #E53935; }
.marrainages-table :deep(tbody tr.row-en-attente:hover) { background: #FFE0B2 !important; }
.marrainages-table :deep(tbody tr.row-en-attente td:first-child) { box-shadow: inset 4px 0 0 #FB8C00; }
</style>
