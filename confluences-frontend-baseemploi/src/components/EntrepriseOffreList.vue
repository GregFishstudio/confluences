<script setup>
import store from '@/store/index.js'
import NProgress from 'nprogress'

import CreateOffre from '@/components/CreateOffre.vue'

// Props
const props = defineProps({
  entreprise: {
    type: Object,
    required: true
  }
})

// Méthodes
function deleteOffre(entrepriseId, offreId) {
  NProgress.start()

  store
    .dispatch('entrepriseOffre/deleteEntrepriseOffre', { entrepriseId, offreId })
    .then(() => store.dispatch('entreprise/deleteOffre', offreId))
    .catch(err => console.error('Erreur suppression offre:', err))
    .finally(() => NProgress.done())
}
</script>

<template>
  <v-card class="mx-auto" outlined rounded="lg">
    <v-list>

      <!-- Header -->
      <v-subheader class="d-flex align-center">
        <span class="font-weight-bold text-body-1">Types d'offres</span>
        <v-spacer></v-spacer>
        <CreateOffre :typeEntrepriseId="entreprise.entrepriseId" />
      </v-subheader>

      <!-- Liste -->
      <v-list density="comfortable" class="py-0">
        <template v-if="entreprise.entrepriseOffres.length > 0">

          <v-expand-transition>
            <div>
              <v-list-item
                v-for="item in entreprise.entrepriseOffres"
                :key="item.typeOffreId"
                class="d-flex align-center"
              >
                <v-list-item-title>
                  {{ item.typeOffre.libelle }}
                </v-list-item-title>

                <v-spacer></v-spacer>

                <v-btn
                  icon
                  variant="text"
                  color="red-darken-1"
                  @click="deleteOffre(item.entrepriseId, item.typeOffreId)"
                >
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </v-list-item>
            </div>
          </v-expand-transition>

        </template>

        <!-- Aucun élément -->
        <div
          v-else
          class="text-center text-medium-emphasis pa-4 font-italic"
        >
          Aucune offre enregistrée.
        </div>
      </v-list>
    </v-list>
  </v-card>
</template>
