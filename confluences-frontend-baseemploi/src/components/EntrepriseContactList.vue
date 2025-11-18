<script setup>
import { computed } from 'vue'
import NProgress from 'nprogress'
import store from '@/store/index.js'

import CreateContact from '@/components/CreateContact.vue'
import EditContact from '@/components/EditContact.vue'

// Props
const props = defineProps({
  entreprise: {
    type: Object,
    required: true
  }
})

// Columns (Vuetify 3 v-data-table)
const headers = [
  { title: 'Nom', key: 'fullname' },
  { title: 'Fonction', key: 'fonction' },
  { title: 'Email', key: 'email' },
  { title: 'Fixe', key: 'telFix' },
  { title: 'Natel', key: 'natel' },
  { title: 'Actions', key: 'actions', sortable: false }
]

// Contacts formatés
const contacts = computed(() =>
  props.entreprise.contacts.map(c => ({
    ...c,
    fullname: `${c.prenom} ${c.nom}`
  }))
)

// Suppression d’un contact
function deleteContact(contactId) {
  NProgress.start()

  store
    .dispatch('contact/deleteContact', { contactId })
    .then(() => store.dispatch('entreprise/deleteContact', contactId))
    .catch(err => console.error('Erreur suppression contact:', err))
    .finally(() => NProgress.done())
}
</script>

<template>
  <v-card class="mx-auto" outlined rounded="lg">
    <v-list>

      <!-- Header -->
      <v-subheader class="d-flex align-center">
        <span class="font-weight-bold text-body-1">Contacts</span>
        <v-spacer />
        <CreateContact :typeEntrepriseId="entreprise.entrepriseId" />
      </v-subheader>

      <!-- Table -->
      <v-data-table
        :headers="headers"
        :items="contacts"
        class="pa-4"
        v-if="contacts.length > 0"
      >
        <template #item.fullname="{ item }">
          {{ item.fullname }}
        </template>

        <template #item.actions="{ item }">
          <EditContact :contact="{ ...item }" />

          <v-btn
            icon
            color="red-darken-1"
            variant="text"
            @click="deleteContact(item.contactId)"
          >
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-data-table>

      <div
        v-else
        class="text-center text-medium-emphasis pa-4"
      >
        Aucun contact pour cette entreprise.
      </div>
    </v-list>
  </v-card>
</template>
