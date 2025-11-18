<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const props = defineProps({
  typeEntrepriseId: {
    type: Number,
    required: true
  }
})

const dialog = ref(false)
const validCreateDomaine = ref(true)
const formCreateDomaine = ref(null)

const typeDomaineState = computed(() => store.state.typeDomaine.typeDomaines || [])

onBeforeMount(() => {
  store.dispatch('typeDomaine/fetchTypeDomaines')
})

const entrepriseDomaine = ref({
  entrepriseId: props.typeEntrepriseId,
  typeDomaineId: 0,
  typeDomaine: {
    entrepriseDomaines: [],
    libelle: '',
    typeDomaineId: 0
  }
})

function addNewData(data) {
  store.dispatch('entreprise/addDomaine', data)
}

async function submit() {
  const { valid } = await formCreateDomaine.value.validate()
  if (!valid) return

  NProgress.start()

  store.dispatch('entrepriseDomaine/createEntrepriseDomaine', entrepriseDomaine.value)
    .then(() => {
      let clone = JSON.parse(JSON.stringify(entrepriseDomaine.value))

      const selected = typeDomaineState.value.find(
        t => t.typeDomaineId === clone.typeDomaineId
      )

      if (selected) clone.typeDomaine.libelle = selected.libelle

      addNewData(clone)

      entrepriseDomaine.value.typeDomaineId = 0
      formCreateDomaine.value.resetValidation()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateDomaine.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form ref="formCreateDomaine" v-model="validCreateDomaine" lazy-validation>
      <v-dialog v-model="dialog" max-width="520">

        <template #activator="{ props: activatorProps }">
          <v-btn
            color="primary"
            rounded
            v-bind="activatorProps"
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter un domaine
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">
          <v-card-title class="d-flex align-center py-4">
            <v-icon class="mr-2" color="primary">mdi-domain</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un domaine</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">
            <v-autocomplete
              v-model="entrepriseDomaine.typeDomaineId"
              :items="typeDomaineState"
              item-value="typeDomaineId"
              item-title="libelle"
              :rules="[v => !!v || 'Obligatoire']"
              label="Type de domaine"
              variant="outlined"
              rounded
              clearable
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
  transition: .2s ease;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,.12);
}
</style>
