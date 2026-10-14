<!--
  Composant générique de gestion de pièces jointes (upload / aperçu / renommage / téléchargement / suppression).
  Réutilisé pour les CV/LM des participant·e·s et les conventions de marrainage.
-->
<template>
  <v-card class="pa-4 elevation-1 rounded-lg" variant="outlined">
    <div class="d-flex align-center flex-wrap ga-2 mb-4">
      <v-icon v-if="title" start color="primary">mdi-paperclip</v-icon>
      <span class="text-subtitle-1 font-weight-bold">{{ title }}</span>
      <v-spacer />
      <v-btn color="primary" variant="flat" prepend-icon="mdi-upload" @click="openUpload">
        {{ addLabel }}
      </v-btn>
    </div>

    <v-row v-if="visibleFiles.length" dense>
      <v-col v-for="item in visibleFiles" :key="item[idKey]" cols="12" sm="6" md="4">
        <v-card class="file-card d-flex align-center pa-3" variant="outlined" rounded="lg" title="Aperçu" @click="preview(item)">
          <v-icon :color="fileIcon(item.fileName).color" size="28" class="mr-3">{{ fileIcon(item.fileName).icon }}</v-icon>
          <div class="file-name flex-grow-1">
            <div class="text-body-2">{{ item.fileName }}</div>
            <div v-if="item.category" class="text-caption text-grey">{{ item.category }}</div>
          </div>
          <div class="d-flex flex-shrink-0">
            <v-btn icon="mdi-pencil-outline" size="x-small" variant="text" title="Renommer" @click.stop="openRename(item)" />
            <v-btn icon="mdi-download-outline" size="x-small" variant="text" title="Télécharger" @click.stop="download(item)" />
            <v-btn icon="mdi-delete-outline" size="x-small" variant="text" color="red" title="Supprimer" @click.stop="askRemove(item)" />
          </div>
        </v-card>
      </v-col>
    </v-row>
    <div v-else class="drop-hint pa-6 text-center" @click="openUpload">
      <v-icon size="32" color="grey-lighten-1" class="mb-2">mdi-file-upload-outline</v-icon>
      <p class="text-body-2 text-medium-emphasis mb-0">
        Aucun fichier attaché{{ filterCategory || filterCategories.length ? ' dans cette catégorie' : '' }}.
        Cliquez sur « {{ addLabel }} » pour en déposer un.
      </p>
    </div>

    <!-- Ajout -->
    <v-dialog v-model="dialog" max-width="500">
      <v-card rounded="lg">
        <v-card-title class="py-4">{{ addLabel }}</v-card-title>
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
          <v-file-input
            v-model="file" label="Fichier (PDF, Word, Excel, image)" variant="outlined" :accept="ACCEPTED_DOC_ACCEPT"
            prepend-icon="" prepend-inner-icon="mdi-paperclip" class="mb-2"
            @update:model-value="onFileChosen"
          />
          <v-text-field
            v-model="newName" label="Intitulé du document (facultatif)" variant="outlined"
            hint="Ex. « CV mars 2026 ». L'extension du fichier est conservée automatiquement." persistent-hint
          />
        </v-card-text>
        <v-divider />
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
          <v-btn color="primary" :disabled="!file" :loading="uploading" @click="submit">Ajouter</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Renommage -->
    <v-dialog v-model="renameDialog" max-width="460">
      <v-card rounded="lg">
        <v-card-title class="py-4">Renommer le document</v-card-title>
        <v-divider />
        <v-card-text class="pt-6">
          <v-text-field v-model="renameValue" label="Nouveau nom" variant="outlined" autofocus hide-details @keyup.enter="confirmRename" />
        </v-card-text>
        <v-divider />
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="renameDialog = false">Annuler</v-btn>
          <v-btn color="primary" :disabled="!renameValue?.trim()" :loading="renaming" @click="confirmRename">Renommer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Suppression -->
    <v-dialog v-model="removeDialog" max-width="420">
      <v-card rounded="lg">
        <v-card-title class="py-4">Supprimer ce document ?</v-card-title>
        <v-divider />
        <v-card-text class="pt-4">« {{ removeTarget?.fileName }} » sera définitivement supprimé.</v-card-text>
        <v-divider />
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="removeDialog = false">Annuler</v-btn>
          <v-btn color="error" @click="confirmRemove">Supprimer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Aperçu -->
    <v-dialog v-model="previewDialog" max-width="1000" @after-leave="clearPreview">
      <v-card rounded="lg">
        <v-card-title class="d-flex align-center py-3">
          <span class="file-name flex-grow-1">{{ previewItem?.fileName }}</span>
          <v-btn icon="mdi-download-outline" variant="text" title="Télécharger" @click="download(previewItem)" />
          <v-btn icon="mdi-close" variant="text" title="Fermer" @click="previewDialog = false" />
        </v-card-title>
        <v-divider />
        <div class="preview-body">
          <div v-if="previewLoading" class="d-flex justify-center align-center fill-height pa-8">
            <v-progress-circular indeterminate color="primary" />
          </div>
          <img v-else-if="previewKind === 'image'" :src="previewUrl" :alt="previewItem?.fileName" class="preview-image" />
          <iframe v-else-if="previewKind === 'pdf'" :src="previewUrl" class="preview-frame" title="Aperçu du document" />
          <div v-else class="pa-8 text-center">
            <v-icon size="48" color="grey" class="mb-3">mdi-eye-off-outline</v-icon>
            <p class="text-body-2 mb-4">
              L'aperçu n'est disponible que pour les PDF et les images.<br>
              Les documents Word ou Excel doivent être téléchargés pour être consultés.
            </p>
            <v-btn color="primary" prepend-icon="mdi-download-outline" @click="download(previewItem)">Télécharger</v-btn>
          </div>
        </div>
      </v-card>
    </v-dialog>
  </v-card>
</template>

<script setup>
import { ref, computed, watch, onBeforeUnmount } from 'vue'
import axios from 'axios'
import { ACCEPTED_DOC_ACCEPT } from '@/utils/fileTypes'

const API_URL = import.meta.env.VITE_API_URL

const props = defineProps({
  title: { type: String, default: 'Fichiers' },
  addLabel: { type: String, default: 'Ajouter un document' },
  controller: { type: String, required: true }, // ex: 'ParticipantFiles', 'MarrainageFiles'
  idKey: { type: String, required: true },       // ex: 'participantFileId', 'marrainageFileId'
  extraFields: { type: Object, required: true }, // ex: { studentId }, { marrainageId }
  categoryOptions: { type: Array, default: () => [] },
  fixedCategory: { type: String, default: null }, // catégorie appliquée sans afficher de sélecteur
  filterCategory: { type: String, default: null }, // n'affiche que les fichiers de cette catégorie (modelValue reste complet)
  filterCategories: { type: Array, default: () => [] }, // idem, pour plusieurs catégories
  excludeCategories: { type: Array, default: () => [] }, // masque les fichiers de ces catégories
  modelValue: { type: Array, default: () => [] }
})

const emit = defineEmits(['update:modelValue'])

const files = ref([...props.modelValue])
watch(() => props.modelValue, (val) => { files.value = [...(val || [])] })

const visibleFiles = computed(() => {
  let list = files.value
  if (props.filterCategory) list = list.filter(f => f.category === props.filterCategory)
  if (props.filterCategories.length) list = list.filter(f => props.filterCategories.includes(f.category))
  if (props.excludeCategories.length) list = list.filter(f => !props.excludeCategories.includes(f.category))
  return list
})

const extensionOf = (name) => (name || '').toLowerCase().match(/\.([a-z0-9]+)$/)?.[1] || ''

const IMAGE_EXT = ['png', 'jpg', 'jpeg', 'gif', 'webp']
function fileIcon(name) {
  const ext = extensionOf(name)
  if (ext === 'pdf') return { icon: 'mdi-file-pdf-box', color: 'red-darken-1' }
  if (['doc', 'docx'].includes(ext)) return { icon: 'mdi-file-word-box', color: 'blue-darken-2' }
  if (['xls', 'xlsx'].includes(ext)) return { icon: 'mdi-file-excel-box', color: 'green-darken-2' }
  if (IMAGE_EXT.includes(ext)) return { icon: 'mdi-file-image-outline', color: 'purple' }
  return { icon: 'mdi-file-document-outline', color: 'primary' }
}

function emitFiles() {
  emit('update:modelValue', files.value)
}

// ── Ajout ──
const dialog = ref(false)
const file = ref(null)
const newName = ref('')
const category = ref(props.categoryOptions[0] || null)
const uploading = ref(false)

function openUpload() {
  file.value = null
  newName.value = ''
  category.value = props.categoryOptions[0] || null
  dialog.value = true
}

// Pré-remplit l'intitulé avec le nom du fichier (sans extension), modifiable avant l'envoi.
function onFileChosen(value) {
  const chosen = Array.isArray(value) ? value[0] : value
  if (chosen && !newName.value) newName.value = chosen.name.replace(/\.[^.]+$/, '')
}

async function submit() {
  const chosen = Array.isArray(file.value) ? file.value[0] : file.value
  if (!chosen) return
  const formData = new FormData()
  formData.append('file', chosen)
  Object.entries(props.extraFields).forEach(([key, value]) => formData.append(key, value))
  if (props.categoryOptions.length) formData.append('category', category.value)
  else if (props.fixedCategory) formData.append('category', props.fixedCategory)

  uploading.value = true
  try {
    const response = await axios.post(`${API_URL}/api/${props.controller}`, formData)
    const created = response.data
    const wantedName = newName.value.trim()
    if (wantedName && wantedName !== chosen.name.replace(/\.[^.]+$/, '')) {
      const renamed = await axios.patch(`${API_URL}/api/${props.controller}/${created[props.idKey]}`, { fileName: wantedName })
      created.fileName = renamed.data.fileName
    }
    // La réponse contient les octets du fichier : inutile de les garder en mémoire.
    delete created.data
    files.value = [...files.value, created]
    emitFiles()
    dialog.value = false
  } finally {
    uploading.value = false
  }
}

// ── Renommage ──
const renameDialog = ref(false)
const renameTarget = ref(null)
const renameValue = ref('')
const renaming = ref(false)

function openRename(item) {
  renameTarget.value = item
  renameValue.value = item.fileName.replace(/\.[^.]+$/, '')
  renameDialog.value = true
}

async function confirmRename() {
  const value = renameValue.value?.trim()
  if (!value || !renameTarget.value) return
  renaming.value = true
  try {
    const id = renameTarget.value[props.idKey]
    const response = await axios.patch(`${API_URL}/api/${props.controller}/${id}`, { fileName: value })
    files.value = files.value.map(f => (f[props.idKey] === id ? { ...f, fileName: response.data.fileName } : f))
    emitFiles()
    renameDialog.value = false
  } finally {
    renaming.value = false
  }
}

// ── Aperçu / téléchargement ──
function fetchBlob(item) {
  return axios.get(`${API_URL}/api/${props.controller}/${item[props.idKey]}`, { responseType: 'blob' })
}

const previewDialog = ref(false)
const previewItem = ref(null)
const previewUrl = ref(null)
const previewKind = ref(null)
const previewLoading = ref(false)

async function preview(item) {
  clearPreview()
  previewItem.value = item
  const ext = extensionOf(item.fileName)
  previewKind.value = ext === 'pdf' ? 'pdf' : IMAGE_EXT.includes(ext) ? 'image' : 'other'
  previewDialog.value = true
  if (previewKind.value === 'other') return

  previewLoading.value = true
  try {
    const response = await fetchBlob(item)
    const type = previewKind.value === 'pdf' ? 'application/pdf' : (response.headers['content-type'] || response.data.type)
    previewUrl.value = window.URL.createObjectURL(new Blob([response.data], { type }))
  } finally {
    previewLoading.value = false
  }
}

function clearPreview() {
  if (previewUrl.value) window.URL.revokeObjectURL(previewUrl.value)
  previewUrl.value = null
}
onBeforeUnmount(clearPreview)

async function download(item) {
  if (!item) return
  const response = await fetchBlob(item)
  const url = window.URL.createObjectURL(new Blob([response.data], { type: response.headers['content-type'] }))
  const link = document.createElement('a')
  link.href = url
  link.download = item.fileName
  document.body.appendChild(link)
  link.click()
  link.remove()
  window.URL.revokeObjectURL(url)
}

// ── Suppression ──
const removeDialog = ref(false)
const removeTarget = ref(null)

function askRemove(item) {
  removeTarget.value = item
  removeDialog.value = true
}

async function confirmRemove() {
  const item = removeTarget.value
  if (!item) return
  await axios.delete(`${API_URL}/api/${props.controller}/${item[props.idKey]}`)
  files.value = files.value.filter(f => f[props.idKey] !== item[props.idKey])
  emitFiles()
  removeDialog.value = false
}
</script>

<style scoped>
.file-card { cursor: pointer; transition: box-shadow 0.2s ease, transform 0.2s ease; }
.file-card:hover { box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1); transform: translateY(-2px); }
.file-name { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; min-width: 0; }
.drop-hint { border: 2px dashed #CFD8DC; border-radius: 8px; cursor: pointer; }
.drop-hint:hover { background: #F5F9FF; border-color: #90CAF9; }
.preview-body { height: 75vh; overflow: auto; background: #F5F5F5; }
.preview-frame { width: 100%; height: 100%; border: 0; display: block; }
.preview-image { display: block; max-width: 100%; margin: 0 auto; }
</style>
