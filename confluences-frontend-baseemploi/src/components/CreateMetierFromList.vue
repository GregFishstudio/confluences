<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

/* --- STATE --- */
const dialog = ref(false)
const validCreateMetier = ref(true)
const formCreateMetier = ref(null)

/* --- DATA MODEL --- */
const typeMetier = ref({
  typeMetierId: 0,
  libelle: null,
  oldNames: null
})

/* --- RULES --- */
const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 60 || 'Maximum 60 caractères'
]

const oldNamesRules = [
  v => !v || v.length <= 300 || 'Maximum 300 caractères'
]

/* --- SUBMIT --- */
async function submit() {
  const { valid } = await formCreateMetier.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('typeMetier/createTypeMetier', typeMetier.value)
    .then(() => {
      formCreateMetier.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

/* --- CLOSE --- */
function closeDialog() {
  dialog.value = false
  formCreateMetier.value?.resetValidation()
}
</script>

<template>
  <v-row justify="start">
    <v-form
      ref="formCreateMetier"
      v-model="validCreateMetier"
      lazy-validation
    >
      <v-dialog v-model="dialog" max-width="520" persistent>
        <!-- Bouton -->
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            rounded
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter un métier
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <!-- Header -->
          <v-card-title class="d-flex align-center py-4">
            <v-icon color="primary" class="mr-2">mdi-hammer-wrench</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un métier</span>
          </v-card-title>

          <v-divider />

          <!-- Form -->
          <v-card-text class="pt-6">

            <v-text-field
              v-model="typeMetier.libelle"
              class="mb-4"
              label="Nom"
              :rules="libelleRules"
              :counter="60"
              variant="outlined"
              rounded
              density="comfortable"
              clearable
              hide-details="auto"
              required
            />

            <v-text-field
              v-model="typeMetier.oldNames"
              class="mb-4"
              label="Autres noms"
              :rules="oldNamesRules"
              :counter="300"
              variant="outlined"
              rounded
              density="comfortable"
              clearable
              hide-details="auto"
            />

          </v-card-text>

          <v-divider />

          <!-- Actions -->
          <v-card-actions class="d-flex justify-end">
            <v-btn variant="text" color="grey" @click="closeDialog">
              Fermer
            </v-btn>

            <v-btn color="primary" rounded @click="submit">
              <v-icon left small>mdi-content-save-outline</v-icon>
              Sauvegarder
            </v-btn>
          </v-card-actions>

        </v-card>
      </v-dialog>
    </v-form>
  </v-row>
</template>

<style scoped>
.v-btn {
  transition: 0.2s ease;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
