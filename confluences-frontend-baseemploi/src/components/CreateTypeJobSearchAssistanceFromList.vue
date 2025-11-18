<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateTypeJobSearchAssistance = ref(true)
const formCreateTypeJobSearchAssistance = ref(null)

const typeJobSearchAssistance = ref({
  typeJobSearchAssistanceId: 0,
  description: null
})

const descriptionRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 80 || 'Max 80 caractères'
]

async function submit() {
  const { valid } = await formCreateTypeJobSearchAssistance.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch(
      'typeJobSearchAssistance/createTypeJobSearchAssistance',
      typeJobSearchAssistance.value
    )
    .then(() => {
      formCreateTypeJobSearchAssistance.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateTypeJobSearchAssistance.value?.resetValidation()
}
</script>

<template>
  <v-row>
    <v-form
      ref="formCreateTypeJobSearchAssistance"
      v-model="validCreateTypeJobSearchAssistance"
      lazy-validation
    >
      <v-dialog v-model="dialog" max-width="520">
        
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            rounded
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter un type ARE
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <v-card-title class="d-flex align-center py-4">
            <v-icon class="mr-2" color="primary">mdi-briefcase-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un type ARE</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">
            <v-text-field
              v-model="typeJobSearchAssistance.description"
              :rules="descriptionRules"
              label="Nom"
              variant="outlined"
              density="comfortable"
              rounded
              hide-details="auto"
              clearable
              required
            />
          </v-card-text>

          <v-divider />

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
  transition: .2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,.12);
}
</style>
