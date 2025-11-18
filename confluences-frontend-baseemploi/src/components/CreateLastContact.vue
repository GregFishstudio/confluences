<script setup>
import { ref } from 'vue'
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
const validCreateContact = ref(true)
const formCreateContact = ref(null)
const menuDate = ref(false)

/* --- DATA --- */
const lastContact = ref({
  lastContactId: 0,
  name: null,
  dateOfContact: null,
  remarks: null,
  entrepriseId: props.typeEntrepriseId
})

/* --- METHODS --- */
function addNewData(data) {
  store.dispatch('entreprise/addLastContact', data).catch(err =>
    console.error('Erreur addLastContact:', err)
  )
}

async function submit() {
  const { valid } = await formCreateContact.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('lastContact/createLastContact', lastContact.value)
    .then(() => {
      const clone = JSON.parse(JSON.stringify(store.state.lastContact.lastContact))
      addNewData(clone)
      formCreateContact.value.reset()
      dialog.value = false
    })
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateContact.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form
      ref="formCreateContact"
      v-model="validCreateContact"
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
            Ajouter un suivi
          </v-btn>
        </template>

        <v-card rounded="lg" class="pa-2">

          <!-- Header -->
          <v-card-title class="d-flex align-center py-4">
            <v-icon color="primary" class="mr-2">mdi-account-clock-outline</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un suivi de contact</span>
          </v-card-title>

          <v-divider />

          <!-- Form -->
          <v-card-text class="pt-6">

            <v-menu
              v-model="menuDate"
              :close-on-content-click="true"
              transition="scale-transition"
              offset-y
              min-width="290"
            >
              <template #activator="{ props }">
                <v-text-field
                  v-model="lastContact.dateOfContact"
                  label="Date du contact"
                  readonly
                  v-bind="props"
                  variant="outlined"
                  rounded
                  class="mb-4"
                />
              </template>

              <v-date-picker
                v-model="lastContact.dateOfContact"
                color="primary"
                @update:model-value="menuDate = false"
              />
            </v-menu>

            <v-text-field
              v-model="lastContact.name"
              label="Nom de la personne"
              variant="outlined"
              rounded
              class="mb-4"
              hide-details="auto"
            />

            <v-textarea
              v-model="lastContact.remarks"
              label="Remarque"
              variant="outlined"
              rounded
              auto-grow
              hide-details="auto"
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
