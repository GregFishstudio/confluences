<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateMoyen = ref(true)
const formCreateMoyen = ref(null)

const typeIntershipActivity = ref({
  typeIntershipActivityId: 0,
  nom: null
})

const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 50 || 'Le champ doit contenir moins de 50 caractères'
]

async function submit() {
  const { valid } = await formCreateMoyen.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch(
      'typeIntershipActivity/createTypeIntershipActivity',
      typeIntershipActivity.value
    )
    .then(() => {
      formCreateMoyen.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateMoyen.value?.resetValidation()
}
</script>

<template>
  <v-row justify="start">
    <v-form
      ref="formCreateMoyen"
      v-model="validCreateMoyen"
      lazy-validation
    >
      <v-dialog v-model="dialog" max-width="520" persistent>

        <!-- Button -->
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            rounded
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter un type d'activité
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <!-- Header -->
          <v-card-title class="d-flex align-center py-4">
            <v-icon color="primary" class="mr-2">mdi-briefcase-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un type d'activité</span>
          </v-card-title>

          <v-divider />

          <!-- Form -->
          <v-card-text class="pt-6">

            <v-text-field
              v-model="typeIntershipActivity.nom"
              label="Nom"
              :rules="libelleRules"
              :counter="50"
              required
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

          </v-card-text>

          <v-divider />

          <!-- Buttons -->
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
  transition: 0.2s;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
