<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

/* --- PROPS --- */
const props = defineProps({
  typeEntrepriseId: {
    type: Number,
    required: true
  }
})

/* --- STATE --- */
const dialog = ref(false)
const validCreateOffre = ref(true)
const formCreateOffre = ref(null)

/* --- DATA MODEL --- */
const entrepriseOffre = ref({
  entrepriseId: props.typeEntrepriseId,
  typeOffreId: null,
  typeOffre: {
    libelle: null
  }
})

/* --- STORE STATE --- */
const typeOffres = computed(() => store.state.typeOffre.typeOffres ?? [])

/* --- LOAD DATA --- */
onBeforeMount(() => {
  store.dispatch('typeOffre/fetchTypeOffres').catch(err =>
    console.error('Erreur chargement types offre:', err)
  )
})

/* --- METHODS --- */
function addNewData(data) {
  store.dispatch('entreprise/addOffre', data).catch(err => {
    console.error('Erreur addOffre:', err)
  })
}

async function submit() {
  const { valid } = await formCreateOffre.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('entrepriseOffre/createEntrepriseOffre', entrepriseOffre.value)
    .then(() => {
      const clone = JSON.parse(JSON.stringify(entrepriseOffre.value))

      // ajouter le libellé au clone
      const selected = typeOffres.value.find(
        t => t.typeOffreId === clone.typeOffreId
      )
      clone.typeOffre.libelle = selected?.libelle ?? null

      addNewData(clone)

      entrepriseOffre.value.typeOffreId = null
      dialog.value = false
    })
    .catch(err => console.error('Erreur createEntrepriseOffre:', err))
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateOffre.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form
      ref="formCreateOffre"
      v-model="validCreateOffre"
      lazy-validation
    >
      <v-dialog v-model="dialog" max-width="520" persistent>
        <!-- Bouton -->
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            rounded
            class="elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter une offre
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">
          <!-- Header -->
          <v-card-title class="d-flex align-center py-4">
            <v-icon color="primary" class="mr-2">mdi-briefcase-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter une offre</span>
          </v-card-title>

          <v-divider />

          <!-- Form -->
          <v-card-text class="pt-6">
            <v-autocomplete
              class="mb-4"
              v-model="entrepriseOffre.typeOffreId"
              :items="typeOffres"
              item-title="libelle"
              item-value="typeOffreId"
              :rules="[v => !!v || 'Obligatoire']"
              label="Type d'offre"
              variant="outlined"
              density="comfortable"
              rounded
              clearable
              hide-details="auto"
              required
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
  transition: 0.2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.12);
}
</style>
