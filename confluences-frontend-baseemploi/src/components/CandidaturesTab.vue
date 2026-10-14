<!--
  Onglet "Candidat·e·s" de la page Inscriptions : demandes d'inscription reçues des AS
  (remplace le tableau Excel des demandes). Les remarques internes ("pour nous") ne sont pas
  affichées ici : seulement dans le formulaire d'une candidature.
-->
<template>
  <v-card class="elevation-4" rounded="lg" :loading="loading">
    <v-row align="center" class="px-2" dense>
      <v-col cols="12" md="3" class="pa-3 pa-md-4">
        <v-text-field
          v-model="search" append-icon="mdi-magnify" label="Rechercher (nom, AS, remarques...)"
          variant="outlined" density="compact" clearable hide-details bg-color="white"
        />
      </v-col>
      <v-col cols="6" md="2" class="pa-3 pa-md-4">
        <v-select v-model="filterMesure" :items="MESURES" label="Mesure" variant="outlined" density="compact" clearable hide-details bg-color="white" />
      </v-col>
      <v-col cols="6" md="2" class="pa-3 pa-md-4">
        <v-select
          v-model="filterSession" :items="sessionOptions" item-title="title" item-value="value" label="Session visée"
          variant="outlined" density="compact" clearable hide-details bg-color="white"
        />
      </v-col>
      <v-col cols="6" md="2" class="pa-3 pa-md-4">
        <v-select
          v-model="filterStatuts" :items="STATUTS_CANDIDATURE" label="Statut" multiple
          variant="outlined" density="compact" clearable hide-details bg-color="white"
        >
          <template #selection="{ item, index }">
            <span v-if="index === 0" class="text-truncate">{{ item.title }}</span>
            <span v-else-if="index === 1" class="text-grey text-caption ml-1">+{{ filterStatuts.length - 1 }}</span>
          </template>
        </v-select>
      </v-col>
      <v-col cols="6" md="2" class="pa-3 pa-md-4">
        <v-select v-model="sortMode" :items="SORTS" label="Trier par" variant="outlined" density="compact" hide-details bg-color="white" />
      </v-col>
      <v-col cols="12" md="1" class="pa-3 pa-md-4 d-flex justify-end">
        <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" @click="openDialog(null)">Nouvelle</v-btn>
      </v-col>
    </v-row>
    <div class="d-flex justify-end px-4 pb-2">
      <v-switch v-model="hideRetires" label="Masquer les retiré·e·s" color="primary" hide-details density="compact" class="flex-grow-0" />
    </div>
    <v-divider />

    <!-- Totaux pour la mesure / session choisies ci-dessus (indépendamment du statut et de la recherche) -->
    <div class="insc-recap">
      <div class="insc-recap-total">
        <span class="l">Totaux</span>
        <span class="insc-recap-scope">
          {{ filterMesure || 'Toutes mesures' }} · {{ filterSession ? sessionLabel(filterSession) : 'toutes sessions' }}
        </span>
      </div>
      <div class="insc-legend">
        <span v-for="s in recap" :key="s.label" class="insc-recap-item" :class="{ active: filterStatuts.length === 1 && filterStatuts[0] === s.statut }" @click="toggleStatut(s.statut)">
          <i :style="{ background: s.color }" /><b>{{ s.n }}</b> {{ s.label }}
        </span>
      </div>
    </div>

    <div v-if="!loading && !filtered.length" class="insc-empty-state">
      <v-icon size="40" color="grey-lighten-1">mdi-account-search-outline</v-icon>
      <div class="mt-2">Aucune candidature ne correspond.</div>
      <div class="text-caption">Ajoutez une demande avec « Nouvelle » ou modifiez les filtres.</div>
    </div>

    <div v-else class="insc-scroll">
      <table class="insc-table">
        <thead>
          <tr>
            <th>Bénéficiaire</th>
            <th>Statut</th>
            <th>Remarques générales</th>
            <th>AS · remarques</th>
            <th>Évaluation</th>
            <th>Inscription · transmission AS / ML</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="c in filtered" :key="c.candidatureId" class="clickable" tabindex="0"
            :class="{ 'is-retired': c.statut === 'Retiré-e' }"
            :style="{ '--rail': railCandidature(c.statut), '--pill-ink': statutCandidatureInk(c.statut) }"
            @click="openDialog(c)" @keydown.enter="openDialog(c)"
          >
            <td>
              <div class="insc-person">
                <div class="insc-initials insc-mesure" :style="{ background: couleurMesure(c.mesureVisee) }" :title="c.mesureVisee || 'Mesure non précisée'">
                  {{ abreviationMesure(c.mesureVisee) }}
                </div>
                <div>
                  <div class="insc-name">{{ c.civilite ? `${c.civilite} ` : '' }}{{ c.nom }} <span class="first">{{ c.prenom }}</span></div>
                  <div class="insc-sub d-flex align-center flex-wrap ga-1">
                    <span v-if="c.mandant" class="insc-tag" :style="{ '--tag': couleurMandant(c.mandant) }">{{ c.mandant }}</span>
                    <span>{{ c.origine || '' }}</span>
                  </div>
                  <div class="insc-sub">Demande du {{ formatDate(c.dateDemande) }}<span v-if="c.priorite != null"> · priorité {{ c.priorite }}</span></div>
                  <v-chip v-if="c.participantId" size="x-small" color="success" variant="tonal" class="mt-1">
                    <v-icon start size="x-small">mdi-link-variant</v-icon>{{ c.participantNom }}
                  </v-chip>
                </div>
              </div>
            </td>
            <td>
              <span class="insc-pill">{{ c.statut || 'Sans statut' }}</span>
              <div v-if="c.mesureVisee || c.sessionNumberVisee" class="insc-sub">
                <span class="insc-tag" :style="{ '--tag': couleurMesure(c.mesureVisee) }">{{ abreviationMesure(c.mesureVisee) }}</span>
                <span v-if="c.sessionNumberVisee"> S{{ c.sessionNumberVisee }} {{ c.anneeVisee }}</span>
              </div>
              <div v-if="c.statut === 'AS à contacter' && c.derniereRemarqueAsAContacter" class="insc-note quote mt-1">
                <span class="insc-date">{{ formatDate(c.derniereRemarqueAsAContacter.createdAt) }}</span> {{ c.derniereRemarqueAsAContacter.text }}
              </div>
              <div v-if="c.contratMaori" class="insc-sub text-red font-weight-bold">Contrat Maori</div>
            </td>
            <td><div class="insc-note" :class="{ 'insc-empty': !c.remarquesGenerales }">{{ c.remarquesGenerales || '—' }}</div></td>
            <td>
              <div class="insc-strong">{{ c.assistantSocial ? asName(c.assistantSocial) : '—' }}</div>
              <div v-if="c.derniereRemarqueAS" class="insc-note quote mt-1">{{ c.derniereRemarqueAS }}</div>
              <div v-if="c.nbRemarquesAS > 1" class="insc-sub">
                <v-icon size="x-small">mdi-comment-multiple-outline</v-icon> {{ c.nbRemarquesAS }} remarques
              </div>
            </td>
            <td>
              <div v-if="c.dateEvaluation" class="insc-sub font-weight-bold">Évaluation du {{ formatDate(c.dateEvaluation) }}</div>
              <div v-if="c.evaluationNiveau" class="insc-note">{{ c.evaluationNiveau }}</div>
              <span v-else-if="!c.dateEvaluation" class="insc-empty">—</span>
            </td>
            <td>
              <span v-if="c.inscriptionConfirmee" class="insc-sub text-success font-weight-bold">
                <v-icon size="x-small" color="success">mdi-check-circle</v-icon> Confirmée
              </span>
              <div v-if="c.derniereTransmission" class="insc-note quote mt-1">
                <span class="insc-date">{{ formatDate(c.derniereTransmission.createdAt) }}</span> {{ c.derniereTransmission.text }}
              </div>
              <div v-if="c.nbTransmissions > 1" class="insc-sub">
                <v-icon size="x-small">mdi-history</v-icon> {{ c.nbTransmissions }} transmissions
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </v-card>

  <CandidatureDialog
    v-model="dialog" :candidature="editing" :assistants="assistants" :calendar-sessions="calendarSessions"
    @saved="onSaved" @assistants-changed="emit('assistants-changed')"
  />
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { matchesSearch } from '@/utils/search'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import '@/scss/inscriptions.css'
import CandidatureDialog from '@/components/CandidatureDialog.vue'
import {
  STATUTS_CANDIDATURE, MESURES, asLabel, railCandidature, abreviationMesure,
  statutCandidatureColor, statutCandidatureInk, couleurMesure, couleurMandant,
} from '@/utils/inscriptionOptions'

defineProps({
  assistants: { type: Array, required: true },
  calendarSessions: { type: Array, default: () => [] },
})
const emit = defineEmits(['assistants-changed', 'changed'])

const candidatures = ref([])
const loading = ref(false)
const search = ref('')
const filterStatuts = ref([])
const filterMesure = ref(null)
const filterSession = ref(null)
const hideRetires = ref(true)

const SORTS = [
  { title: 'Date de la demande (récente → ancienne)', value: 'date_desc' },
  { title: 'Date de la demande (ancienne → récente)', value: 'date_asc' },
  { title: 'Nom (A → Z)', value: 'nom' },
  { title: 'Session visée', value: 'session' },
  { title: 'Mesure', value: 'mesure' },
  { title: 'Priorité', value: 'priorite' },
]
const sortMode = ref('date_desc')

const dialog = ref(false)
const editing = ref(null)

const asName = (a) => [a.prenom, a.nom].filter(Boolean).join(' ') || a.email
const formatDate = (d) => d ? moment(d).format('DD.MM.YYYY') : '—'

const sessionKeyOf = (c) => (c.anneeVisee && c.sessionNumberVisee) ? `${c.anneeVisee}-${c.sessionNumberVisee}` : null
const sessionLabel = (key) => { const [y, n] = key.split('-'); return `S${n} ${y}` }
const sessionSortValue = (c) => (c.anneeVisee || 9999) * 10 + (c.sessionNumberVisee || 9)

// Sessions présentes dans les candidatures, de la plus ancienne à la plus lointaine.
const sessionOptions = computed(() => {
  const keys = [...new Set(candidatures.value.map(sessionKeyOf).filter(Boolean))]
  return keys
    .sort((a, b) => { const [ya, na] = a.split('-').map(Number); const [yb, nb] = b.split('-').map(Number); return ya - yb || na - nb })
    .map(k => ({ value: k, title: sessionLabel(k) }))
})

// Mesure + session : périmètre commun à la liste et aux totaux.
const inScope = computed(() => candidatures.value.filter(c =>
  (!filterMesure.value || c.mesureVisee === filterMesure.value)
  && (!filterSession.value || sessionKeyOf(c) === filterSession.value)))

const filtered = computed(() => {
  let list = inScope.value
  if (hideRetires.value && !filterStatuts.value.includes('Retiré-e')) list = list.filter(c => c.statut !== 'Retiré-e')
  if (filterStatuts.value.length) list = list.filter(c => filterStatuts.value.includes(c.statut))
  if (search.value) {
    list = list.filter(c => matchesSearch(search.value,
      c.nom, c.prenom, c.email, c.telephone, c.origine, c.remarquesGenerales, c.derniereRemarqueAS,
      c.notesRdv, c.evaluationNiveau, c.statut, c.assistantSocial && asLabel(c.assistantSocial)))
  }
  const byNom = (a, b) => (a.nom || '').localeCompare(b.nom || '', 'fr')
  const byDate = (a, b) => new Date(a.dateDemande || 0) - new Date(b.dateDemande || 0)
  const sorters = {
    date_desc: (a, b) => byDate(b, a),
    date_asc: byDate,
    nom: byNom,
    session: (a, b) => sessionSortValue(a) - sessionSortValue(b) || byDate(a, b),
    mesure: (a, b) => (a.mesureVisee || '~').localeCompare(b.mesureVisee || '~', 'fr') || sessionSortValue(a) - sessionSortValue(b) || byDate(a, b),
    priorite: (a, b) => (a.priorite ?? 99) - (b.priorite ?? 99) || byDate(a, b),
  }
  return [...list].sort(sorters[sortMode.value])
})

// Totaux par statut pour la mesure / session choisies. Un clic filtre la liste sur ce statut.
const recap = computed(() => STATUTS_CANDIDATURE.map(statut => ({
  statut,
  label: statut,
  n: inScope.value.filter(c => c.statut === statut).length,
  color: statutCandidatureColor(statut),
})))

function toggleStatut(statut) {
  filterStatuts.value = (filterStatuts.value.length === 1 && filterStatuts.value[0] === statut) ? [] : [statut]
}

async function load() {
  loading.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/Candidatures`)
    if (res?.ok) candidatures.value = await res.json()
  } finally {
    loading.value = false
  }
}

function openDialog(c) {
  editing.value = c ? { candidatureId: c.candidatureId } : null
  dialog.value = true
}

async function onSaved() {
  await load()
  emit('changed')
}

onMounted(load)
defineExpose({ load })
</script>
