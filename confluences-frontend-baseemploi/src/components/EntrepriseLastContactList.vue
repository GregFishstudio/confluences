<script setup>
import store from '@/store/index.js'
import NProgress from 'nprogress'
import moment from 'moment'

import CreateLastContact from '@/components/CreateLastContact.vue'
import EditLastContact from '@/components/EditLastContact.vue'

// Props
const props = defineProps({
  entreprise: {
    type: Object,
    required: true
  }
})

// --- Méthodes ---
function deleteLastContact(lastContactId) {
  NProgress.start()

  store
    .dispatch('lastContact/deleteLastContact', { contactId: lastContactId })
    .then(() => store.dispatch('entreprise/deleteLastContact', lastContactId))
    .catch(err => console.error('Erreur suppression dernier contact:', err))
    .finally(() => NProgress.done())
}

function formatDate(date) {
  return moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD')
}

function displayRemarks(text) {
  if (!text) return ''
  const max = 80
  return text.length > max ? text.slice(0, max) + '…' : text
}
</script>

<template>
  <v-card class="mx-auto" outlined rounded="lg">
    <v-list>

      <!-- Header -->
      <v-subheader class="d-flex align-center">
        <span class="font-weight-bold text-body-1">Derniers contacts</span>
        <v-spacer></v-spacer>
        <CreateLastContact :typeEntrepriseId="entreprise.entrepriseId" />
      </v-subheader>

      <!-- Tableau -->
      <v-table
        v-if="entreprise.lastContacts.length > 0"
        class="text-body-2"
        density="comfortable"
      >
        <thead>
          <tr>
            <th class="text-left">Date</th>
            <th class="text-left">Nom</th>
            <th class="text-left">Remarques</th>
            <th class="text-left">Actions</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="item in entreprise.lastContacts" :key="item.lastContactId">
            <td>{{ formatDate(item.dateOfContact) }}</td>
            <td>{{ item.name }}</td>
            <td>{{ displayRemarks(item.remarks) }}</td>

            <td class="d-flex gap-2">
              <EditLastContact :lastContact="item" />

              <v-btn
                icon
                variant="text"
                color="red-darken-1"
                @click="deleteLastContact(item.lastContactId)"
              >
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </td>
          </tr>
        </tbody>
      </v-table>

      <!-- Aucun contact -->
      <div
        v-else
        class="text-center text-medium-emphasis pa-4 font-italic"
      >
        Aucun dernier contact enregistré.
      </div>
    </v-list>
  </v-card>
</template>
