<script setup>
import store from '@/store/index.js'
import NProgress from 'nprogress'

import CreateMetier from '@/components/CreateMetier.vue'

// Props
const props = defineProps({
  entreprise: {
    type: Object,
    required: true
  }
})

// --- Méthodes ---
function deleteMetier(entrepriseId, metierId) {
  NProgress.start()

  store
    .dispatch('entrepriseMetier/deleteEntrepriseMetier', { entrepriseId, metierId })
    .then(() => store.dispatch('entreprise/deleteMetier', metierId))
    .catch(err => console.error('Erreur suppression métier:', err))
    .finally(() => NProgress.done())
}
</script>

<template>
  <v-card class="mx-auto" outlined rounded="lg">
    <v-list>

      <!-- Header -->
      <v-subheader class="d-flex align-center">
        <span class="font-weight-bold text-body-1">Métiers</span>
        <v-spacer></v-spacer>
        <CreateMetier :typeEntrepriseId="entreprise.entrepriseId" />
      </v-subheader>

      <!-- Liste des métiers -->
      <v-list density="comfortable" class="py-0">
        <template v-if="entreprise.entrepriseMetiers.length > 0">

          <v-expand-transition>
            <div>
              <v-list-item
                v-for="item in entreprise.entrepriseMetiers"
                :key="item.typeMetierId"
                class="d-flex align-center"
              >
                <v-list-item-title>{{ item.typeMetier.libelle }}</v-list-item-title>

                <v-spacer></v-spacer>

                <v-btn
                  icon
                  variant="text"
                  color="red-darken-1"
                  @click="deleteMetier(item.entrepriseId, item.typeMetierId)"
                >
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </v-list-item>
            </div>
          </v-expand-transition>

        </template>

        <!-- Aucun métier -->
        <div
          v-else
          class="text-center text-medium-emphasis pa-4 font-italic"
        >
          Aucun métier enregistré.
        </div>
      </v-list>
    </v-list>
  </v-card>
</template>
