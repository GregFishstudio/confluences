<script setup>
import { useRouter } from 'vue-router'
import moment from 'moment'

// Props
const props = defineProps({
  entreprise: {
    type: Object,
    required: true
  }
})

// Router
const router = useRouter()

// Colonnes du tableau
const headers = [
  { title: 'Date début', key: 'debut' },
  { title: 'Date fin', key: 'fin' },
  { title: 'Stagiaire', key: 'stagiaire' },
  { title: 'Métier', key: 'typeMetier.libelle' }
]

// Helpers
function formatDate(date) {
  return moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD')
}

// Accéder à la page d’édition
function viewStage(item) {
  router.push({
    name: 'Stage-Modifier',
    params: { id: item.stageId }
  })
}
</script>

<template>
  <v-data-table
    :headers="headers"
    :items="entreprise.stages"
    :items-per-page="5"
    class="elevation-2"
    rounded="lg"
    density="comfortable"
    @click:row="viewStage"
  >

    <!-- DATE DEBUT -->
    <template #item.debut="{ item }">
      {{ formatDate(item.debut) }}
    </template>

    <!-- DATE FIN -->
    <template #item.fin="{ item }">
      {{ formatDate(item.fin) }}
    </template>

    <!-- STAGIAIRE -->
    <template #item.stagiaire="{ item }">
      {{ item.stagiaire.firstname }} {{ item.stagiaire.lastName }}
    </template>

  </v-data-table>
</template>
