<script setup>
import { ref, onMounted } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'
import moment from 'moment'

// Props
const props = defineProps({
  lastContact: {
    type: Object,
    required: true
  }
})

// Dialog + Form
const dialog = ref(false)
const valid = ref(true)
const formEdit = ref(null)

// Date Picker
const menuDate = ref(false)

/**
 * Format la date pour être compatible avec input et date-picker
 */
function formatDate(date) {
  return moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD')
}

onMounted(() => {
  props.lastContact.dateOfContact = formatDate(props.lastContact.dateOfContact)
})

/**
 * Sauvegarde les modifications du dernier contact
 */
async function submit() {
  const { valid: isValid } = await formEdit.value.validate()
  if (!isValid) return

  NProgress.start()

  try {
    // 1. Mise à jour dans lastContact
    await store.dispatch('lastContact/editLastContact', props.lastContact)

    // 2. Mise à jour dans l'entreprise
    const clone = JSON.parse(JSON.stringify(props.lastContact))
    await store.dispatch('entreprise/editLastContact', clone)

    dialog.value = false
  } catch (e) {
    console.error('Erreur lors de la sauvegarde du dernier contact:', e)
  } finally {
    NProgress.done()
  }
}
</script>

<template>
  <v-row justify="center">
    <v-form ref="formEdit" v-model="valid" lazy-validation>
      <v-dialog v-model="dialog" max-width="600">

        <!-- Bouton d'ouverture -->
        <template #activator="{ props: activatorProps }">
          <v-btn
            v-bind="activatorProps"
            icon
            color="primary"
            variant="tonal"
            class="ma-1"
          >
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
        </template>

        <v-card rounded="lg">
          <v-card-title class="py-4">
            <span class="text-h6 font-weight-bold">
              Modifier un dernier contact
            </span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">

            <!-- Sélecteur Date -->
            <v-menu
              v-model="menuDate"
              transition="scale-transition"
              close-on-content-click="false"
            >
              <template #activator="{ props: menuProps }">
                <v-text-field
                  v-bind="menuProps"
                  v-model="props.lastContact.dateOfContact"
                  label="Date"
                  readonly
                />
              </template>

              <v-date-picker
                v-model="props.lastContact.dateOfContact"
                color="primary"
                @input="menuDate = false"
              />
            </v-menu>

            <!-- Nom -->
            <v-text-field
              v-model="props.lastContact.name"
              label="Nom"
              :counter="50"
            />

            <!-- Remarques -->
            <v-textarea
              v-model="props.lastContact.remarks"
              label="Remarque"
              rows="3"
              auto-grow
            />

          </v-card-text>

          <v-divider />

          <v-card-actions class="d-flex justify-end">
            <v-btn variant="text" color="grey" @click="dialog = false">
              Fermer
            </v-btn>

            <v-btn color="blue-darken-1" variant="flat" @click="submit">
              Sauvegarder
            </v-btn>
          </v-card-actions>
        </v-card>

      </v-dialog>
    </v-form>
  </v-row>
</template>
