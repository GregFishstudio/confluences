<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Migration Vue 3 / Vuetify 3
  -- Description : Ajout d’un métier au filtre
  -- Fichier : FilterEntrepriseAddMetier.vue
-->

<template>
  <v-btn
    icon
    variant="outlined"
    size="small"
    class="ml-2"
    color="success"
    @click="dialog = true"
  >
    <v-icon>mdi-plus</v-icon>
  </v-btn>

  <v-dialog v-model="dialog" max-width="450px" persistent>
    <v-card rounded="lg">
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2">mdi-briefcase-plus</v-icon>
        <span class="text-h6 font-weight-bold">Ajouter un métier</span>
      </v-card-title>

      <v-divider />

      <v-card-text class="pt-4">
        <v-form ref="formCreateMetier" v-model="validCreateMetier">
          <v-autocomplete
            v-model="metier.typeMetierId"
            :items="typeMetier.typeMetiers"
            item-title="libelle"
            item-value="typeMetierId"
            :rules="[v => !!v || 'Obligatoire']"
            label="Type de métier"
            density="comfortable"
            clearable
          />
        </v-form>
      </v-card-text>

      <v-divider />

      <v-card-actions class="py-3 px-4">
        <v-spacer />

        <v-btn variant="text" color="grey" @click="dialog = false">
          Fermer
        </v-btn>

        <v-btn color="primary" @click="submit">
          Sauvegarder
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateMetier = ref(true)

const metier = ref({
  typeMetierId: null,
  libelle: '',
  oldNames: ''
})

const typeMetier = computed(() => store.state.typeMetier)
const entreprise = computed(() => store.state.entreprise)

onBeforeMount(() => {
  store.dispatch('typeMetier/fetchTypeMetiers')
})

const formCreateMetier = ref(null)

function submit() {
  const form = formCreateMetier.value
  if (!form) return

  form.validate().then(({ valid }) => {
    if (!valid) return

    const selected = typeMetier.value.typeMetiers.find(
      t => t.typeMetierId === metier.value.typeMetierId
    )

    if (!selected) return

    metier.value.libelle = selected.libelle
    metier.value.oldNames = selected.oldNames

    NProgress.start()

    store
      .dispatch('entreprise/addFilterEntrepriseMetier', metier.value)
      .catch(() => {})
      .finally(() => {
        dialog.value = false
        NProgress.done()
      })
  })
}
</script>

<style scoped></style>
