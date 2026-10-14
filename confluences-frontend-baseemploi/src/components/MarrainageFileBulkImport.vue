<!--
  Import en masse de pièces jointes (Convention, Charte, Extrait de casier judiciaire) pour les
  marrainages, avec détection automatique du parrain/marraine à partir du nom de fichier
  (ex: "Charte Alexis Heubi.pdf").
-->
<template>
  <v-dialog v-model="dialog" max-width="800" @update:model-value="onOpen">
    <template #activator="{ props }">
      <v-btn v-bind="props" variant="outlined" prepend-icon="mdi-file-multiple-outline">
        Importer des fichiers
      </v-btn>
    </template>

    <v-card rounded="lg">
      <v-card-title class="py-4">Importer des fichiers en masse</v-card-title>
      <v-divider />
      <v-card-text class="pt-6">
        <v-select
          v-model="category"
          :items="['Convention', 'Charte', 'Extrait de casier judiciaire']"
          label="Catégorie des fichiers importés"
          variant="outlined"
          class="mb-2"
        />
        <v-file-input
          v-model="selectedFiles"
          label="Fichiers (PDF, Word, Excel, image)"
          variant="outlined"
          multiple
          :accept="ACCEPTED_DOC_ACCEPT"
          hide-details="auto"
          @update:model-value="matchFiles"
        />

        <template v-if="rows.length">
          <div class="text-body-2 text-medium-emphasis mt-4 mb-2">
            {{ matchedCount }} / {{ rows.length }} fichier(s) associé(s) automatiquement. Corrige les autres ci-dessous.
          </div>
          <div v-for="row in rows" :key="row.file.name" class="d-flex align-center ga-2 mb-2">
            <v-icon :color="row.marrainageId ? 'success' : 'grey'" size="20">
              {{ row.marrainageId ? 'mdi-check-circle-outline' : 'mdi-help-circle-outline' }}
            </v-icon>
            <div class="file-label text-body-2">{{ row.file.name }}</div>
            <v-autocomplete
              v-model="row.marrainageId"
              :items="marrainageOptions"
              item-title="label"
              item-value="value"
              label="Marraine / parrain"
              variant="outlined" density="compact" clearable hide-details
              class="flex-grow-1"
            />
          </div>
        </template>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
        <v-btn color="primary" :disabled="!importableCount" :loading="importing" @click="confirmImport">
          Importer {{ importableCount }} fichier(s) — {{ category }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useStore } from 'vuex'
import axios from 'axios'
import { ACCEPTED_DOC_ACCEPT } from '@/utils/fileTypes'

const API_URL = import.meta.env.VITE_API_URL

const store = useStore()
const emit = defineEmits(['imported'])

const dialog = ref(false)
const category = ref('Convention')
const selectedFiles = ref([])
const rows = ref([])
const importing = ref(false)

const marrainages = computed(() => store.state.marrainage?.marrainages || [])
const marrainageOptions = computed(() =>
  marrainages.value.map(m => ({ label: `${m.prenom} ${m.nom}`, value: m.marrainageId }))
)

const matchedCount = computed(() => rows.value.filter(r => r.marrainageId).length)
const importableCount = computed(() => rows.value.filter(r => r.marrainageId).length)

function onOpen(isOpen) {
  if (isOpen) store.dispatch('marrainage/fetchMarrainages', {})
}

function normalize(str) {
  return (str || '')
    .toString()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/[^a-z0-9]+/g, ' ')
    .trim()
}

function matchFiles() {
  rows.value = (selectedFiles.value || []).map(file => {
    const nameTokens = normalize(file.name.replace(/\.pdf$/i, '')).split(' ').filter(Boolean)

    let best = null
    let bestScore = 0
    for (const m of marrainages.value) {
      const nomTokens = normalize(m.nom).split(' ').filter(Boolean)
      const prenomTokens = normalize(m.prenom).split(' ').filter(Boolean)
      const nomMatch = nomTokens.length && nomTokens.every(t => nameTokens.includes(t))
      const prenomMatch = prenomTokens.length && prenomTokens.every(t => nameTokens.includes(t))
      const score = (nomMatch ? 1 : 0) + (prenomMatch ? 1 : 0)
      // On exige au moins le nom de famille pour éviter les faux positifs sur un prénom courant.
      if (nomMatch && score > bestScore) {
        best = m
        bestScore = score
      }
    }

    return { file, marrainageId: best ? best.marrainageId : null }
  })
}

async function confirmImport() {
  importing.value = true
  try {
    const toImport = rows.value.filter(r => r.marrainageId)
    for (const row of toImport) {
      const formData = new FormData()
      formData.append('file', row.file)
      formData.append('marrainageId', row.marrainageId)
      formData.append('category', category.value)
      await axios.post(`${API_URL}/api/MarrainageFiles`, formData)
    }
    dialog.value = false
    selectedFiles.value = []
    rows.value = []
    emit('imported')
  } finally {
    importing.value = false
  }
}
</script>

<style scoped>
.file-label {
  min-width: 220px;
  max-width: 280px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
