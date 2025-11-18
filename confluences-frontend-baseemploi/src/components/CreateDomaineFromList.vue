<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateDomaine = ref(true)
const formCreateDomaine = ref(null)

const typeDomaine = ref({
  typeDomaineId: 0,
  code: null,
  libelle: null,
  oldNames: null
})

const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 60 || 'Le champ doit contenir moins de 60 caractères'
]

const oldNamesRules = [
  v => !v || v.length <= 300 || 'Le champ doit contenir moins de 300 caractères'
]

async function submit() {
  const { valid } = await formCreateDomaine.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('typeDomaine/createTypeDomaine', typeDomaine.value)
    .then(() => {
      formCreateDomaine.value.reset()
      dialog.value = false
    })
    .catch(err => {
      console.error('Erreur lors de la création du domaine:', err)
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateDomaine.value?.resetValidation()
}
</script>

<template>
  <v-row justify="start">
    <v-form
      ref="formCreateDomaine"
      v-model="validCreateDomaine"
      lazy-validation
    >
      <v-dialog v-model="dialog" max-width="520" persistent>

        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            rounded
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter un domaine
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <v-card-title class="d-flex align-center py-4">
            <v-icon color="primary" class="mr-2">mdi-domain</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un domaine</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">

            <v-text-field
              v-model="typeDomaine.libelle"
              class="mb-4"
              :counter="60"
              :rules="libelleRules"
              label="Nom"
              required
              variant="outlined"
              rounded
              density="comfortable"
              clearable
              hide-details="auto"
            />

            <v-text-field
              v-model="typeDomaine.oldNames"
              class="mb-4"
              :counter="300"
              :rules="oldNamesRules"
              label="Autres noms"
              variant="outlined"
              rounded
              density="comfortable"
              clearable
              hide-details="auto"
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
  transition: 0.2s ease;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
