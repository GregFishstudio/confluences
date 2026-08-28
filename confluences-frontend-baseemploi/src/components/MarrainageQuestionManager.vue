<!--
  Gestion des questions du questionnaire d'inscription parrain/marraine (ajout, modification,
  suppression, ordre). Accessible depuis la page "Nouveau marrainage".
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
        <div v-for="q in questions" :key="q.marrainageQuestionId" class="question-row mb-3 pa-3">
          <div class="d-flex align-center ga-2 mb-2">
            <v-text-field v-model.number="q.order" type="number" label="Ordre" variant="outlined" density="compact" style="max-width: 90px" hide-details />
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
