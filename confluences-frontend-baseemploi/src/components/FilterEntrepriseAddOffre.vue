<!-- 
  Projet: Gestion des stagiaires
  Migration Vue 3 / Vuetify 3
  Description : Ajout d'une offre au filtre
  Fichier : FilterEntrepriseAddOffre.vue
-->

<template>
  <!-- Bouton d’ajout -->
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

  <!-- Dialog -->
  <v-dialog v-model="dialog" max-width="450px" persistent>
    <v-card rounded="lg">
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2">mdi-tag-plus-outline</v-icon>
        <span class="text-h6 font-weight-bold">Ajouter une offre</span>
      </v-card-title>

      <v-divider />

      <v-card-text class="pt-4">
        <v-form ref="formCreateOffre" v-model="validCreateOffre">

          <v-autocomplete
            v-model="offre.typeOffreId"
            :items="typeOffres"
            item-title="libelle"
            item-value="typeOffreId"
            :rules="[v => !!v || 'Obligatoire']"
            label="Type d'offre"
            clearable
            density="comfortable"
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
import store from '@/store'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateOffre = ref(true)

const offre = ref({
  typeOffreId: null,
  libelle: ''
})

const typeOffreState = computed(() => store.state.typeOffre)

onBeforeMount(() => {
  store.dispatch('typeOffre/fetchTypeOffres')
})

/* Liste simple des offres */
const typeOffres = computed(() => typeOffreState.value.typeOffres)

const formCreateOffre = ref(null)

function submit() {
  const form = formCreateOffre.value
  if (!form) return

  form.validate().then(({ valid }) => {
    if (!valid) return

    const selected = typeOffres.value.find(
      o => o.typeOffreId === offre.value.typeOffreId
    )

    if (!selected) return

    offre.value.libelle = selected.libelle

    NProgress.start()

    store
      .dispatch('entreprise/addFilterEntrepriseOffre', offre.value)
      .catch(() => {})
      .finally(() => {
        dialog.value = false
        NProgress.done()
      })
  })
}
</script>

<style scoped></style>
