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
const validCreateMetier = ref(true)
const formCreateMetier = ref(null)

/* --- STORE STATE --- */
const typeMetiers = computed(() => store.state.typeMetier.typeMetiers ?? [])

/* --- LOAD --- */
onBeforeMount(() => {
  store.dispatch('typeMetier/fetchTypeMetiers').catch(err =>
    console.error('Erreur chargement métiers:', err)
  )
})

/* --- DATA MODEL --- */
const entrepriseMetier = ref({
  entrepriseId: props.typeEntrepriseId,
  typeMetierId: null,
  typeMetier: {
    libelle: null
  }
})

/* --- METHODS --- */
function addNewData(data) {
  store.dispatch('entreprise/addMetier', data).catch(err =>
    console.error('Erreur addMetier:', err)
  )
}

async function submit() {
  const { valid } = await formCreateMetier.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('entrepriseMetier/createEntrepriseMetier', entrepriseMetier.value)
    .then(() => {
      const clone = JSON.parse(JSON.stringify(entrepriseMetier.value))

      const selected = typeMetiers.value.find(
        t => t.typeMetierId === clone.typeMetierId
      )
      clone.typeMetier.libelle = selected?.libelle ?? null

      addNewData(clone)

      entrepriseMetier.value.typeMetierId = null
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateMetier.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form
      ref="formCreateMetier"
      v-model="validCreateMetier"
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
            Ajouter un métier
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <v-card-title class="d-flex align-center py-4">
            <v-icon color="primary" class="mr-2">mdi-hammer-wrench</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un métier</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">
            <v-autocomplete
              class="mb-4"
              v-model="entrepriseMetier.typeMetierId"
              :items="typeMetiers"
              item-value="typeMetierId"
              item-title="libelle"
              :rules="[v => !!v || 'Obligatoire']"
              label="Type de métier"
              variant="outlined"
              rounded
              density="comfortable"
              clearable
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
  transition: 0.2s ease;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
