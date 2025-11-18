<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateAnnonce = ref(true)
const formCreateAnnonce = ref(null)

const typeAnnonce = ref({
  typeAnnonceId: 0,
  code: null,
  libelle: null
})

const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 30 || 'Le champ doit contenir moins de 30 caractères'
]

async function submit() {
  const { valid } = await formCreateAnnonce.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('typeAnnonce/createTypeAnnonce', typeAnnonce.value)
    .then(() => {
      formCreateAnnonce.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateAnnonce.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form ref="formCreateAnnonce" v-model="validCreateAnnonce" lazy-validation>
      <v-dialog v-model="dialog" max-width="520">

        <template #activator="{ props }">
          <v-btn
            color="primary"
            rounded
            v-bind="props"
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter une annonce
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <v-card-title class="d-flex align-center py-4">
            <v-icon class="mr-2" color="primary">mdi-bullhorn-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter une annonce de stage</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">
            <v-text-field
              v-model="typeAnnonce.libelle"
              label="Nom"
              :rules="libelleRules"
              :counter="30"
              clearable
              variant="outlined"
              rounded
              hide-details="auto"
              density="comfortable"
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
  transition: 0.2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
