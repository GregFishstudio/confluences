<!--
  Composant générique de gestion de pièces jointes (upload / téléchargement / suppression).
  Réutilisé pour les CV/LM des participant·e·s et les conventions de marrainage.
-->
<template>
  <v-card class="pa-4 elevation-1 rounded-lg" variant="outlined">
    <div class="d-flex align-center mb-4">
      <v-icon start color="primary">mdi-paperclip</v-icon>
      <span class="text-subtitle-1 font-weight-bold">{{ title }}</span>
      <v-spacer />
      <v-dialog v-model="dialog" max-width="500">
        <template #activator="{ props }">
          <v-btn v-bind="props" color="success" variant="outlined" rounded size="x-small">
            <v-icon>mdi-plus</v-icon>
          </v-btn>
        </template>
        <v-card rounded="lg">
          <v-card-title class="py-4">Ajouter un fichier</v-card-title>
          <v-divider />
          <v-card-text class="pt-6">
            <v-select
              v-if="categoryOptions.length"
              v-model="category"
              :items="categoryOptions"
              label="Catégorie"
              variant="outlined"
              class="mb-2"
            />
            <v-file-input v-model="file" label="Fichier (PDF)" variant="outlined" accept="application/pdf" hide-details="auto" />
          </v-card-text>
          <v-divider />
          <v-card-actions class="d-flex justify-end">
            <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
            <v-btn color="primary" :disabled="!file" :loading="uploading" @click="submit">Sauvegarder</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>

    <v-row v-if="visibleFiles.length" dense>
      <v-col v-for="item in visibleFiles" :key="item[idKey]" cols="12" sm="6" md="4">
        <v-card class="file-card d-flex align-center pa-3" variant="outlined" rounded="lg" @click="download(item)">
          <v-icon color="primary" size="28" class="mr-3">mdi-file-document-outline</v-icon>
          <div class="file-name">
            <div class="text-body-2">{{ item.fileName }}</div>
            <div v-if="item.category" class="text-caption text-grey">{{ item.category }}</div>
          </div>
          <v-spacer />
          <v-icon color="red" size="20" style="cursor: pointer" @click.stop="remove(item)">mdi-delete-outline</v-icon>
        </v-card>
      </v-col>
    </v-row>
    <div v-else class="pa-6 text-center">
      <v-icon size="32" color="grey-lighten-1" class="mb-2">mdi-folder-open-outline</v-icon>
      <p class="text-body-2 text-medium-emphasis mb-0">Aucun fichier attaché{{ filterCategory ? ' dans cette catégorie' : '' }}.</p>
    </div>
  </v-card>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL

const props = defineProps({
  title: { type: String, default: 'Fichiers' },
  controller: { type: String, required: true }, // ex: 'ParticipantFiles', 'MarrainageFiles'
  idKey: { type: String, required: true },       // ex: 'participantFileId', 'marrainageFileId'
  extraFields: { type: Object, required: true }, // ex: { studentId }, { marrainageId }
  categoryOptions: { type: Array, default: () => [] },
  fixedCategory: { type: String, default: null }, // catégorie appliquée sans afficher de sélecteur
  filterCategory: { type: String, default: null }, // n'affiche que les fichiers de cette catégorie (modelValue reste complet)
  excludeCategories: { type: Array, default: () => [] }, // masque les fichiers de ces catégories
  modelValue: { type: Array, default: () => [] }
})

const emit = defineEmits(['update:modelValue'])

const files = ref([...props.modelValue])
watch(() => props.modelValue, (val) => { files.value = [...(val || [])] })

const visibleFiles = computed(() => {
  let list = files.value
  if (props.filterCategory) list = list.filter(f => f.category === props.filterCategory)
  if (props.excludeCategories.length) list = list.filter(f => !props.excludeCategories.includes(f.category))
  return list
})

const dialog = ref(false)
const file = ref(null)
const category = ref(props.categoryOptions[0] || null)
const uploading = ref(false)

function submit() {
  if (!file.value) return
  const formData = new FormData()
  formData.append('file', file.value)
  Object.entries(props.extraFields).forEach(([key, value]) => formData.append(key, value))
  if (props.categoryOptions.length) formData.append('category', category.value)
  else if (props.fixedCategory) formData.append('category', props.fixedCategory)

  uploading.value = true
  axios.post(`${API_URL}/api/${props.controller}`, formData)
    .then(response => {
      files.value = [...files.value, response.data]
      emit('update:modelValue', files.value)
      file.value = null
      dialog.value = false
    })
    .finally(() => { uploading.value = false })
}

function download(item) {
  axios.get(`${API_URL}/api/${props.controller}/${item[props.idKey]}`, { responseType: 'blob' })
    .then(response => {
      const url = window.URL.createObjectURL(new Blob([response.data], { type: response.headers['content-type'] }))
      const link = document.createElement('a')
      link.href = url
      link.download = item.fileName
      document.body.appendChild(link)
      link.click()
      link.remove()
    })
}

function remove(item) {
  axios.delete(`${API_URL}/api/${props.controller}/${item[props.idKey]}`)
    .then(() => {
      files.value = files.value.filter(f => f[props.idKey] !== item[props.idKey])
      emit('update:modelValue', files.value)
    })
}
</script>

<style scoped>
.file-card { cursor: pointer; transition: box-shadow 0.2s ease, transform 0.2s ease; }
.file-card:hover { box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1); transform: translateY(-2px); }
.file-name { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
</style>
