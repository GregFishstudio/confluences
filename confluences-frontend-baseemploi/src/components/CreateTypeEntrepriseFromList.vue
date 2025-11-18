<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateTypeEntreprise = ref(true)
const formCreateTypeEntreprise = ref(null)

const typeEntreprise = ref({
  typeEntrepriseId: 0,
  code: null,
  nom: null
})

const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 50 || 'Max 50 caractères'
]

async function submit() {
  const { valid } = await formCreateTypeEntreprise.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('typeEntreprise/createTypeEntreprise', typeEntreprise.value)
    .then(() => {
      formCreateTypeEntreprise.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateTypeEntreprise.value?.resetValidation()
}
</script>

<template>
  <v-row>
    <v-form
      ref="formCreateTypeEntreprise"
      v-model="validCreateTypeEntreprise"
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
            <v-icon left>mdi-tag-plus</v-icon>
            Ajouter une catégorie
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <v-card-title class="d-flex align-center py-4">
            <v-icon class="mr-2" color="primary">mdi-tag-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter une catégorie</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">
            <v-text-field
              v-model="typeEntreprise.nom"
              :counter="50"
              :rules="libelleRules"
              label="Nom"
              variant="outlined"
              rounded
              density="comfortable"
              hide-details="auto"
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
