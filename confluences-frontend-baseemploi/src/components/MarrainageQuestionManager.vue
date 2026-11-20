<!--
  Gestion des questions du questionnaire d'inscription parrain/marraine (ajout, modification,
  suppression, ordre avec les flèches — enregistré aussitôt). Accessible depuis la page
  "Nouveau marrainage" et depuis une fiche.
-->
<template>
  <v-dialog v-model="dialog" max-width="700" scrollable>
    <template #activator="{ props }">
      <v-btn v-bind="props" variant="outlined" prepend-icon="mdi-cog-outline" @click="load">
        Gérer les questions
      </v-btn>
    </template>

    <v-card rounded="lg">
      <v-card-title class="py-4">Questions du questionnaire</v-card-title>
      <v-divider />
      <v-card-text class="pt-4" style="max-height: 70vh;">
        <div class="text-caption text-medium-emphasis mb-3">
          Les flèches changent la place d'une question dans le questionnaire (enregistré aussitôt).
        </div>
        <div v-for="(q, idx) in questions" :key="q.marrainageQuestionId" class="question-row mb-3 pa-3">
          <div class="d-flex align-center ga-2 mb-2">
            <div class="d-flex flex-column align-center" style="width: 36px">
              <v-btn icon="mdi-chevron-up" size="x-small" variant="text" :disabled="idx === 0 || moving" title="Monter" @click="move(idx, -1)" />
              <span class="text-caption font-weight-bold">{{ idx + 1 }}</span>
              <v-btn icon="mdi-chevron-down" size="x-small" variant="text" :disabled="idx === questions.length - 1 || moving" title="Descendre" @click="move(idx, 1)" />
            </div>
            <v-text-field v-model="q.label" label="Question" variant="outlined" density="compact" hide-details class="flex-grow-1" />
            <v-icon color="red" style="cursor: pointer" @click="remove(q)">mdi-delete-outline</v-icon>
          </div>
          <div class="d-flex align-center ga-2">
            <v-select
              v-model="q.type"
              :items="[
                { title: 'Texte court', value: 'text' },
                { title: 'Texte long', value: 'textarea' },
                { title: 'Nombre', value: 'number' },
                { title: 'Choix (options)', value: 'choice' },
              ]"
              label="Type"
              variant="outlined" density="compact" hide-details style="max-width: 200px"
            />
            <v-text-field
              v-if="q.type === 'choice'"
              v-model="q.options"
              label="Options (séparées par |)"
              variant="outlined" density="compact" hide-details class="flex-grow-1"
            />
            <v-switch v-model="q.required" label="Obligatoire" color="primary" hide-details />
            <v-switch v-model="q.active" label="Actif" color="success" hide-details />
          </div>
          <div class="d-flex justify-end mt-1">
            <v-btn size="small" variant="text" color="primary" @click="save(q)">Sauvegarder</v-btn>
          </div>
        </div>

        <v-divider class="my-4" />

        <div class="text-subtitle-2 font-weight-bold mb-2">Ajouter une question</div>
        <div class="d-flex align-center ga-2 mb-2">
          <v-text-field v-model="newQuestion.label" label="Question" variant="outlined" density="compact" hide-details class="flex-grow-1" />
          <v-select
            v-model="newQuestion.type"
            :items="[
              { title: 'Texte court', value: 'text' },
              { title: 'Texte long', value: 'textarea' },
              { title: 'Nombre', value: 'number' },
              { title: 'Choix (options)', value: 'choice' },
            ]"
            label="Type"
            variant="outlined" density="compact" hide-details style="max-width: 200px"
          />
        </div>
        <v-text-field
          v-if="newQuestion.type === 'choice'"
          v-model="newQuestion.options"
          label="Options (séparées par |)"
          variant="outlined" density="compact" class="mb-2"
        />
        <div class="d-flex justify-end">
          <v-btn color="primary" :disabled="!newQuestion.label" @click="create">Ajouter</v-btn>
        </div>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref } from 'vue'
import MarrainageQuestionService from '@/services/marrainageQuestionService.js'

const dialog = ref(false)
const questions = ref([])
const emit = defineEmits(['changed'])

const newQuestion = ref({ label: '', type: 'text', options: '' })

function load() {
  MarrainageQuestionService.getQuestions(false).then(response => {
    questions.value = response.data
  })
}

// Déplace une question d'un cran et enregistre tout l'ordre (positions 1, 2, 3…).
const moving = ref(false)
async function move(idx, delta) {
  const cible = idx + delta
  if (cible < 0 || cible >= questions.value.length) return
  const liste = [...questions.value]
  ;[liste[idx], liste[cible]] = [liste[cible], liste[idx]]
  questions.value = liste
  moving.value = true
  try {
    await MarrainageQuestionService.putOrdre(liste.map(q => q.marrainageQuestionId))
    liste.forEach((q, i) => { q.order = i + 1 })
    emit('changed')
  } catch (e) {
    load()
  } finally {
    moving.value = false
  }
}

function save(q) {
  MarrainageQuestionService.putQuestion(q).then(() => emit('changed'))
}

function remove(q) {
  MarrainageQuestionService.deleteQuestion(q.marrainageQuestionId).then(() => {
    questions.value = questions.value.filter(item => item.marrainageQuestionId !== q.marrainageQuestionId)
    emit('changed')
  })
}

function create() {
  if (!newQuestion.value.label) return
  MarrainageQuestionService.postQuestion({
    label: newQuestion.value.label,
    type: newQuestion.value.type,
    options: newQuestion.value.type === 'choice' ? newQuestion.value.options : null,
    required: false,
    order: 0,
    active: true
  }).then(response => {
    questions.value.push(response.data)
    newQuestion.value = { label: '', type: 'text', options: '' }
    emit('changed')
  })
}
</script>

<style scoped>
.question-row {
  border: 1px solid rgba(0, 0, 0, 0.1);
  border-radius: 8px;
}
</style>
