<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Migration Vue 3 / Vuetify 3
  -- Description : Ajout d’un domaine au filtre
  -- Fichier : FilterEntrepriseAddDomaine.vue
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

  <v-dialog
    v-model="dialog"
    max-width="450px"
    persistent
  >
    <v-card rounded="lg">
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2">mdi-plus-circle-outline</v-icon>
        <span class="text-h6 font-weight-bold">Ajouter un domaine</span>
      </v-card-title>

      <v-divider />

      <v-card-text class="pt-4">
        <v-form ref="formCreateDomaine" v-model="validCreateDomaine">

          <v-autocomplete
            v-model="domaine.typeDomaineId"
            :items="typeDomaine.typeDomaines"
            item-title="libelle"
            item-value="typeDomaineId"
            :rules="[v => !!v || 'Obligatoire']"
            label="Type de domaine"
            density="comfortable"
            clearable
          />

        </v-form>
      </v-card-text>

      <v-divider />

      <v-card-actions class="py-3 px-4">
        <v-spacer />

        <v-btn
          variant="text"
          color="grey"
          @click="dialog = false"
        >
          Fermer
        </v-btn>

        <v-btn
          color="primary"
          @click="submit"
        >
          Sauvegarder
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, onBeforeMount, computed } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

// --- STATE ---
const dialog = ref(false)
const validCreateDomaine = ref(true)

const domaine = ref({
  typeDomaineId: null,
  libelle: ''
})

// --- STORE ---
const typeDomaine = computed(() => store.state.typeDomaine)
const entreprise = computed(() => store.state.entreprise)

// --- LOAD DOMAINES ---
onBeforeMount(() => {
  store.dispatch('typeDomaine/fetchTypeDomaines')
})

// --- SUBMIT ---
function submit() {
  const form = formCreateDomaine.value
  if (!form) return

  form.validate().then(({ valid }) => {
    if (!valid) return

    const selected = typeDomaine.value.typeDomaines.find(
      t => t.typeDomaineId === domaine.value.typeDomaineId
    )

    if (!selected) return

    domaine.value.libelle = selected.libelle

    NProgress.start()

    store
      .dispatch('entreprise/addFilterEntrepriseDomaine', domaine.value)
      .catch(() => {})
      .finally(() => {
        dialog.value = false
        NProgress.done()
      })
  })
}

const formCreateDomaine = ref(null)
</script>

<style scoped></style>
