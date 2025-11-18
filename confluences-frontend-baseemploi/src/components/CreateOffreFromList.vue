<template>
  <v-row class="d-flex justify-end">
    <v-dialog v-model="dialog" max-width="520" persistent>

      <!-- Bouton -->
      <template #activator="{ props }">
        <v-btn
          v-bind="props"
          color="primary"
          rounded
          class="elevation-2"
        >
          <v-icon left>mdi-plus</v-icon>
          Ajouter une offre
        </v-btn>
      </template>

      <!-- Popup -->
      <v-card rounded="lg" class="pa-2">

        <!-- Header -->
        <v-card-title class="d-flex align-center py-4">
          <v-icon color="primary" class="mr-2">mdi-briefcase-outline</v-icon>
          <span class="text-h6 font-weight-bold">Ajouter une offre</span>
        </v-card-title>

        <v-divider />

        <!-- Form -->
        <v-card-text class="pt-6">
          <v-form ref="formCreateOffre" v-model="validCreateOffre">

            <v-text-field
              v-model="typeOffre.libelle"
              class="mb-4"
              label="Nom de l’offre"
              :counter="30"
              :rules="libelleRules"
              clearable
              variant="outlined"
              density="comfortable"
              rounded
              hide-details="auto"
            />

          </v-form>
        </v-card-text>

        <v-divider />

        <!-- Actions -->
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="dialog = false">
            Fermer
          </v-btn>

          <v-btn color="primary" rounded @click="submit">
            <v-icon left small>mdi-content-save-outline</v-icon>
            Sauvegarder
          </v-btn>
        </v-card-actions>

      </v-card>
    </v-dialog>
  </v-row>
</template>

<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

/* --- STATE --- */
const dialog = ref(false)
const validCreateOffre = ref(true)

const typeOffre = ref({
  typeOffreId: 0,
  libelle: null
})

const formCreateOffre = ref(null)

/* --- RULES --- */
const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 30 || 'Maximum 30 caractères'
]

/* --- SUBMIT --- */
function submit() {
  if (!formCreateOffre.value.validate()) return

  NProgress.start()

  store
    .dispatch('typeOffre/createTypeOffre', typeOffre.value)
    .then(() => {
      formCreateOffre.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}
</script>

<style scoped>
.v-btn {
  transition: 0.2s ease;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
