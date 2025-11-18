<script setup>
import NProgress from 'nprogress'
import store from '@/store/index.js'

import CreateDomaine from '@/components/CreateDomaine.vue'

// Props
const props = defineProps({
  entreprise: {
    type: Object,
    required: true
  }
})

// Suppression d’un domaine
function deleteDomaine(entrepriseId, domaineId) {
  NProgress.start()

  store
    .dispatch('entrepriseDomaine/deleteEntrepriseDomaine', {
      entrepriseId,
      domaineId
    })
    .then(() => store.dispatch('entreprise/deleteDomaine', domaineId))
    .catch(err => console.error('Erreur suppression domaine:', err))
    .finally(() => NProgress.done())
}
</script>

<template>
  <v-card class="mx-auto" outlined rounded="lg">
    <v-list>

      <!-- Header -->
      <v-subheader class="d-flex align-center">
        <span class="font-weight-bold text-body-1">Domaines</span>
        <v-spacer></v-spacer>
        <CreateDomaine :typeEntrepriseId="entreprise.entrepriseId" />
      </v-subheader>

      <!-- Liste des domaines -->
      <v-list>
        <transition-group name="slide-fade" appear>
          <v-list-item
            v-for="(item, index) in entreprise.entrepriseDomaines"
            :key="index"
          >
            <v-list-item-title>
              {{ item.typeDomaine.libelle }}
            </v-list-item-title>

            <v-spacer />

            <v-btn
              icon
              color="red-darken-1"
              variant="text"
              @click="deleteDomaine(item.entrepriseId, item.typeDomaineId)"
            >
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </v-list-item>
        </transition-group>
      </v-list>

      <!-- Aucun domaine -->
      <div
        v-if="entreprise.entrepriseDomaines.length === 0"
        class="text-center text-medium-emphasis pa-4"
      >
        Aucun domaine pour cette entreprise.
      </div>
    </v-list>
  </v-card>
</template>
