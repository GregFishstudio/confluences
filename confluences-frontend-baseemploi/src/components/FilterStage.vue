<!-- 
  Projet: Gestion des stagiaires
  Migration Vue 3 / Vuetify 3
  Description : Filtre des stages
  Fichier : FilterStage.vue
-->

<template>
  <!-- Boutons -->
  <div class="mb-4 d-flex ga-3">
    <v-btn color="primary" @click="dialog = true">Filtrer</v-btn>

    <v-btn color="primary" @click="exportStages">Excel</v-btn>

    <v-btn color="primary" @click="deletefilterStage">Effacer le filtre</v-btn>
  </div>

  <!-- Dialog principal -->
  <v-dialog v-model="dialog" max-width="650px" persistent>
    <v-card rounded="lg">
      <v-card-title class="text-h6 font-weight-bold">
        Filtrer les stages
      </v-card-title>

      <v-divider />

      <v-card-text>
        <v-form ref="formFilterStage" v-model="validFilterStage">

          <v-text-field
            v-model="stage.filter.nom"
            label="Nom"
            :rules="nameRules"
            clearable
            density="comfortable"
          />

          <v-autocomplete
            v-model="stage.filter.typeIntershipActivityId"
            :items="typeIntershipActivity.typeIntershipActivities"
            item-title="nom"
            item-value="typeIntershipActivityId"
            label="Type d'activité"
            clearable
            density="comfortable"
          />

          <v-autocomplete
            v-model="stage.filter.typeMetierId"
            :items="typeMetier.typeMetiers"
            item-title="libelle"
            item-value="typeMetierId"
            label="Métier"
            clearable
            density="comfortable"
          />

          <v-autocomplete
            v-model="stage.filter.entrepriseId"
            :items="entreprise.entreprises"
            item-title="nom"
            item-value="entrepriseId"
            label="Entreprise"
            clearable
            density="comfortable"
          />

          <v-autocomplete
            v-model="stage.filter.stagiaireId"
            :items="user.users"
            item-title="nom"
            item-value="id"
            label="Stagiaire"
            clearable
            density="comfortable"
          />

          <v-text-field
            v-model="stage.filter.year"
            label="Année"
            type="number"
            clearable
            density="comfortable"
          />

          <!-- DATE DE DÉBUT -->
          <v-menu v-model="menuDebut" :close-on-content-click="false">
            <template #activator="{ props }">
              <v-text-field
                v-bind="props"
                v-model="stage.filter.dateDebut"
                label="Date de début"
                readonly
                clearable
                density="comfortable"
              />
            </template>
            <v-date-picker
              v-model="stage.filter.dateDebut"
              scrollable
              @update:modelValue="menuDebut = false"
            />
          </v-menu>

          <!-- DATE DE FIN -->
          <v-menu v-model="menuFin" :close-on-content-click="false">
            <template #activator="{ props }">
              <v-text-field
                v-bind="props"
                v-model="stage.filter.dateFin"
                label="Date de fin"
                readonly
                clearable
                density="comfortable"
              />
            </template>
            <v-date-picker
              v-model="stage.filter.dateFin"
              scrollable
              @update:modelValue="menuFin = false"
            />
          </v-menu>

          <v-autocomplete
            v-model="stage.filter.typeStageId"
            :items="typeStage.typeStages"
            item-title="nom"
            item-value="typeStageId"
            label="Taux d'occupation"
            clearable
            density="comfortable"
          />

          <v-autocomplete
            v-model="stage.filter.typeAnnonceId"
            :items="typeAnnonce.typeAnnonces"
            item-title="libelle"
            item-value="typeAnnonceId"
            label="Type d'annonce"
            clearable
            density="comfortable"
          />

          <v-autocomplete
            v-model="stage.filter.sessionId"
            :items="session.sessions"
            item-title="description"
            item-value="sessionId"
            label="Session"
            clearable
            density="comfortable"
          />

        </v-form>
      </v-card-text>

      <v-divider />

      <v-card-actions class="py-3 px-4">
        <v-spacer />

        <v-btn variant="text" @click="dialog = false">
          Fermer
        </v-btn>

        <v-btn color="primary" @click="filterStage">
          Filtrer
        </v-btn>
      </v-card-actions>

    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref } from 'vue'
import store from '@/store'
import { mapState, useStore } from 'vuex'
import NProgress from 'nprogress'

const dialog = ref(false)
const menuDebut = ref(false)
const menuFin = ref(false)
const validFilterStage = ref(true)

const nameRules = [
  v => !v || v.length <= 50 || 'Maximum 50 caractères'
]

/* ACCÈS AUX STATES */
const storeInstance = useStore()
const stage = storeInstance.state.stage
const user = storeInstance.state.user
const typeMetier = storeInstance.state.typeMetier
const entreprise = storeInstance.state.entreprise
const typeStage = storeInstance.state.typeStage
const typeAnnonce = storeInstance.state.typeAnnonce
const typeIntershipActivity = storeInstance.state.typeIntershipActivity
const session = storeInstance.state.session

const formFilterStage = ref(null)

/* FILTRER */
function filterStage() {
  formFilterStage.value.validate().then(({ valid }) => {
    if (!valid) return

    NProgress.start()
    store
      .dispatch('stage/saveFilterStage', stage.filter)
      .finally(() => {
        dialog.value = false
        NProgress.done()
      })
  })
}

/* RESET */
function deletefilterStage() {
  NProgress.start()

  store.dispatch('stage/deleteFilterStage')
  store.dispatch('stage/fetchStages', true)

  dialog.value = false
  NProgress.done()
}

/* EXPORT EXCEL */
function exportStages() {
  NProgress.start()

  store
    .dispatch('stage/fetchStagesExport', stage.filter)
    .then(response => {
      const fileURL = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      const today = new Date().toISOString().slice(0, 10)

      link.href = fileURL
      link.setAttribute('download', `${today}_experiences_prof.xlsx`)
      document.body.appendChild(link)
      link.click()
    })
    .finally(() => NProgress.done())
}
</script>

<style scoped>
</style>
