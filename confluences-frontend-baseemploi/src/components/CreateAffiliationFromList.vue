<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateAffiliation = ref(true)
const formCreateAffiliation = ref(null)

const typeAffiliation = ref({
  typeAffiliationId: 0,
  code: null,
  libelle: null
})

const codeRules = [
  v => !!v || 'Le champ est obligatoire',
  v => /(\b[A-Z0-9]{1,}\b)/.test(v) || 'Majuscules + chiffres uniquement',
  v => !v || v.length <= 10 || 'Max 10 caractères'
]

const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 50 || 'Max 50 caractères'
]

function fetchAffiliations() {
  return store.dispatch('typeAffiliation/fetchTypeAffiliations')
}

async function submit() {
  const { valid } = await formCreateAffiliation.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('typeAffiliation/createTypeAffiliation', typeAffiliation.value)
    .then(() => {
      fetchAffiliations()
      formCreateAffiliation.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateAffiliation.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form ref="formCreateAffiliation" v-model="validCreateAffiliation" lazy-validation>
      <v-dialog v-model="dialog" max-width="520">

        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            rounded
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter une affiliation
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <v-card-title class="d-flex align-center py-4">
            <v-icon class="mr-2" color="primary">mdi-account-group-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter une affiliation</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">
            <v-text-field
              v-model="typeAffiliation.code"
              :counter="10"
              :rules="codeRules"
              label="Code"
              variant="outlined"
              rounded
              hide-details="auto"
              density="comfortable"
            />

            <v-text-field
              v-model="typeAffiliation.libelle"
              :counter="50"
              :rules="libelleRules"
              label="Nom"
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
  transition: .2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,.12);
}
</style>
