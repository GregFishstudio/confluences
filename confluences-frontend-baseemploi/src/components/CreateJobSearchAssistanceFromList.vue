<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

/* --- STATE --- */
const dialog = ref(false)
const validCreateJobSearchAssistance = ref(true)
const formCreateJobSearchAssistance = ref(null)
const menuDate = ref(false)

/* --- DATA --- */
const jobSearchAssistance = ref({
  jobSearchAssistanceId: 0,
  address: null,
  town: null,
  zipCode: null,
  date: null,
  description: null,
  website: null,
  keyWords: null,
  typeJobSearchAssistanceId: null,
  typeJobSearchAssistanceOccurrenceId: null
})

/* --- STORE --- */
const typeJobSearchAssistanceState = computed(
  () => store.state.typeJobSearchAssistance.typeSearchAssistances || []
)
const typeJobSearchAssistanceOccurrenceState = computed(
  () => store.state.typeJobSearchAssistanceOccurrence.typeSearchAssistanceOccurrences || []
)

onBeforeMount(() => {
  store.dispatch('typeJobSearchAssistance/fetchTypeJobSearchAssistances')
  store.dispatch('typeJobSearchAssistanceOccurrence/fetchtypeSearchAssistanceOccurrences')
})

/* --- METHODS --- */
function addNewData(data) {
  store.dispatch('jobSearchAssistance/addJobSearchAssistance', data).catch(err =>
    console.error('Erreur addJobSearchAssistance:', err)
  )
}

async function submit() {
  const { valid } = await formCreateJobSearchAssistance.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('jobSearchAssistance/createJobSearchAssistance', jobSearchAssistance.value)
    .then(() => {
      const clone = JSON.parse(
        JSON.stringify(store.state.jobSearchAssistance.jobSearchAssistance)
      )

      addNewData(clone)
      formCreateJobSearchAssistance.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateJobSearchAssistance.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form
      ref="formCreateJobSearchAssistance"
      v-model="validCreateJobSearchAssistance"
      lazy-validation
    >
      <v-dialog v-model="dialog" max-width="520" persistent>

        <!-- Opening button -->
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            rounded
            class="mx-3 elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter un ARE
          </v-btn>
        </template>

        <!-- Popup -->
        <v-card rounded="lg" class="pa-2">

          <!-- Header -->
          <v-card-title class="d-flex align-center py-4">
            <v-icon color="primary" class="mr-2">mdi-school-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un ARE</span>
          </v-card-title>

          <v-divider />

          <!-- Form -->
          <v-card-text class="pt-6">

            <v-text-field
              v-model="jobSearchAssistance.address"
              label="Adresse"
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-text-field
              v-model="jobSearchAssistance.town"
              label="Ville"
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-text-field
              v-model="jobSearchAssistance.zipCode"
              label="Code postal"
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-autocomplete
              v-model="jobSearchAssistance.typeJobSearchAssistanceOccurrenceId"
              :items="typeJobSearchAssistanceOccurrenceState"
              item-value="typeJobSearchAssistanceOccurrenceId"
              item-title="description"
              label="Occurrence"
              clearable
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-menu
              v-model="menuDate"
              :close-on-content-click="true"
              transition="scale-transition"
              offset-y
              min-width="290px"
            >
              <template #activator="{ props }">
                <v-text-field
                  v-model="jobSearchAssistance.date"
                  label="Date"
                  readonly
                  v-bind="props"
                  variant="outlined"
                  rounded
                  class="mb-4"
                />
              </template>

              <v-date-picker
                v-model="jobSearchAssistance.date"
                color="primary"
                @update:model-value="menuDate = false"
              />
            </v-menu>

            <v-text-field
              v-model="jobSearchAssistance.description"
              label="Description"
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-text-field
              v-model="jobSearchAssistance.website"
              label="Site internet"
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-text-field
              v-model="jobSearchAssistance.keyWords"
              label="Mots clés"
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-autocomplete
              v-model="jobSearchAssistance.typeJobSearchAssistanceId"
              :items="typeJobSearchAssistanceState"
              item-value="typeJobSearchAssistanceId"
              item-title="description"
              label="Type"
              clearable
              variant="outlined"
              rounded
              hide-details="auto"
            />

          </v-card-text>

          <v-divider />

          <!-- Buttons -->
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
  transition: 0.2s;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.12);
}
</style>
