<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Migration Vue 3 / Vuetify 3 + UI modernisée
  -- Description : Filtre de l'entreprise
  -- Fichier : FilterEntreprise.vue
-->

<template>
  <v-form
    ref="formFilterEntreprise"
    v-model="validFilterEntreprise"
    lazy-validation
  >
    <v-row class="mb-4" no-gutters>
      <v-btn
        color="primary"
        class="mr-4 mb-4"
        @click="dialog = true"
      >
        <v-icon start>mdi-filter-variant</v-icon>
        Filtrer
      </v-btn>

      <v-btn
        color="primary"
        variant="outlined"
        class="mr-4 mb-4"
        @click="deletefilterEntreprise"
      >
        <v-icon start>mdi-filter-remove-outline</v-icon>
        Effacer le filtre
      </v-btn>
    </v-row>

    <v-dialog
      v-model="dialog"
      max-width="900"
      persistent
    >
      <v-card rounded="lg">
        <v-card-title class="d-flex align-center">
          <v-icon class="mr-2">mdi-filter-variant</v-icon>
          <span class="text-h6 font-weight-bold">
            Filtrer les entreprises
          </span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">
          <p class="mb-4 text-body-2 text-medium-emphasis">
            Tous les filtres sont optionnels. Combine ceux dont tu as besoin pour affiner la recherche.
          </p>

          <v-expansion-panels multiple>
            <!-- Métiers -->
            <v-expansion-panel>
              <v-expansion-panel-title>
                <div class="d-flex align-center justify-space-between w-100">
                  <span>Métiers</span>
                  <FilterEntrepriseAddMetier />
                </div>
              </v-expansion-panel-title>
              <v-expansion-panel-text>
                <v-card variant="outlined">
                  <v-list density="comfortable">
                    <v-list-item
                      v-if="entreprise.filter.metiers.length === 0"
                      disabled
                    >
                      <v-list-item-title>Aucun métier sélectionné.</v-list-item-title>
                    </v-list-item>

                    <v-list-item
                      v-for="(metier, i) in entreprise.filter.metiers"
                      :key="'metier-' + metier.typeMetierId + '-' + i"
                    >
                      <v-list-item-title>
                        {{ metier.libelle }}
                      </v-list-item-title>

                      <template #append>
                        <v-btn
                          icon
                          variant="text"
                          size="small"
                          @click.stop="deleteMetier(metier.typeMetierId)"
                        >
                          <v-icon color="error">mdi-delete</v-icon>
                        </v-btn>
                      </template>
                    </v-list-item>
                  </v-list>
                </v-card>
              </v-expansion-panel-text>
            </v-expansion-panel>

            <!-- Noms communs (oldNames métiers) -->
            <v-expansion-panel>
              <v-expansion-panel-title>
                <div class="d-flex align-center justify-space-between w-100">
                  <span>Nom commun | (métier)</span>
                  <FilterEntrepriseAddMetierOldNames />
                </div>
              </v-expansion-panel-title>
              <v-expansion-panel-text>
                <v-card variant="outlined">
                  <v-list density="comfortable">
                    <v-list-item
                      v-if="
                        entreprise.filter.metiers.filter(
                          item => item.oldNames != null && item.oldNames !== ''
                        ).length === 0
                      "
                      disabled
                    >
                      <v-list-item-title>Aucun nom commun sélectionné.</v-list-item-title>
                    </v-list-item>

                    <v-list-item
                      v-for="(metier, i) in entreprise.filter.metiers.filter(
                        item => item.oldNames != null && item.oldNames !== ''
                      )"
                      :key="'oldname-' + metier.typeMetierId + '-' + i"
                    >
                      <v-list-item-title>
                        {{ metier.oldNames }}
                      </v-list-item-title>

                      <template #append>
                        <v-btn
                          icon
                          variant="text"
                          size="small"
                          @click.stop="deleteMetier(metier.typeMetierId)"
                        >
                          <v-icon color="error">mdi-delete</v-icon>
                        </v-btn>
                      </template>
                    </v-list-item>
                  </v-list>
                </v-card>
              </v-expansion-panel-text>
            </v-expansion-panel>

            <!-- Domaines -->
            <v-expansion-panel>
              <v-expansion-panel-title>
                <div class="d-flex align-center justify-space-between w-100">
                  <span>Domaines</span>
                  <FilterEntrepriseAddDomaine />
                </div>
              </v-expansion-panel-title>
              <v-expansion-panel-text>
                <v-card variant="outlined">
                  <v-list density="comfortable">
                    <v-list-item
                      v-if="entreprise.filter.domaines.length === 0"
                      disabled
                    >
                      <v-list-item-title>Aucun domaine sélectionné.</v-list-item-title>
                    </v-list-item>

                    <v-list-item
                      v-for="(domaine, i) in entreprise.filter.domaines"
                      :key="'domaine-' + domaine.typeDomaineId + '-' + i"
                    >
                      <v-list-item-title>
                        {{ domaine.libelle }}
                      </v-list-item-title>

                      <template #append>
                        <v-btn
                          icon
                          variant="text"
                          size="small"
                          @click.stop="deleteDomaine(domaine.typeDomaineId)"
                        >
                          <v-icon color="error">mdi-delete</v-icon>
                        </v-btn>
                      </template>
                    </v-list-item>
                  </v-list>
                </v-card>
              </v-expansion-panel-text>
            </v-expansion-panel>

            <!-- Offres -->
            <v-expansion-panel>
              <v-expansion-panel-title>
                <div class="d-flex align-center justify-space-between w-100">
                  <span>Offres</span>
                  <FilterEntrepriseAddOffre />
                </div>
              </v-expansion-panel-title>
              <v-expansion-panel-text>
                <v-card variant="outlined">
                  <v-list density="comfortable">
                    <v-list-item
                      v-if="entreprise.filter.offres.length === 0"
                      disabled
                    >
                      <v-list-item-title>Aucune offre sélectionnée.</v-list-item-title>
                    </v-list-item>

                    <v-list-item
                      v-for="(offre, i) in entreprise.filter.offres"
                      :key="'offre-' + offre.typeOffreId + '-' + i"
                    >
                      <v-list-item-title>
                        {{ offre.libelle }}
                      </v-list-item-title>

                      <template #append>
                        <v-btn
                          icon
                          variant="text"
                          size="small"
                          @click.stop="deleteOffre(offre.typeOffreId)"
                        >
                          <v-icon color="error">mdi-delete</v-icon>
                        </v-btn>
                      </template>
                    </v-list-item>
                  </v-list>
                </v-card>
              </v-expansion-panel-text>
            </v-expansion-panel>

            <!-- Autres filtres -->
            <v-expansion-panel>
              <v-expansion-panel-title>
                <span>Autres filtres</span>
              </v-expansion-panel-title>
              <v-expansion-panel-text>
                <v-card variant="outlined" class="pa-4">
                  <v-row dense>
                    <v-col cols="12" md="6">
                      <v-text-field
                        v-model="entreprise.filter.city"
                        label="Ville"
                        hide-details="auto"
                      />
                    </v-col>

                    <v-col cols="12" md="6">
                      <v-text-field
                        v-model="entreprise.filter.codePostal"
                        :counter="4"
                        :rules="[codePostalRules]"
                        label="Code postal"
                        hide-details="auto"
                        type="number"
                      />
                    </v-col>

                    <v-col cols="12" md="6">
                      <v-text-field
                        v-model="entreprise.filter.nom"
                        :counter="50"
                        :rules="nameRules"
                        label="Nom"
                        hide-details="auto"
                      />
                    </v-col>

                    <v-col cols="12" md="6">
                      <v-menu
                        v-model="menuModification"
                        :close-on-content-click="false"
                        transition="scale-transition"
                        offset-y
                        max-width="290"
                        min-width="290"
                      >
                        <template #activator="{ props: menuProps }">
                          <v-text-field
                            v-model="entreprise.filter.dateModification"
                            label="Date de modification"
                            readonly
                            clearable
                            v-bind="menuProps"
                            hide-details="auto"
                          />
                        </template>

                        <v-date-picker
                          v-model="entreprise.filter.dateModification"
                          @update:model-value="menuModification = false"
                        />
                      </v-menu>
                    </v-col>

                    <v-col cols="12" md="6">
                      <v-autocomplete
                        v-model="entreprise.filter.createur"
                        :items="user.users"
                        item-value="id"
                        item-title="nom"
                        label="Créateur-trice"
                        clearable
                        hide-details="auto"
                      />
                    </v-col>
                  </v-row>
                </v-card>
              </v-expansion-panel-text>
            </v-expansion-panel>
          </v-expansion-panels>
        </v-card-text>

        <v-divider />

        <v-card-actions class="py-4 px-6">
          <v-spacer />
          <v-btn
            variant="text"
            color="grey"
            @click="dialog = false"
          >
            Fermer
          </v-btn>
          <v-btn
            color="primary"
            @click="filterEntreprise"
          >
            <v-icon start>mdi-check</v-icon>
            Appliquer le filtre
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-form>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

import FilterEntrepriseAddMetier from '@/components/FilterEntrepriseAddMetier.vue'
import FilterEntrepriseAddMetierOldNames from '@/components/FilterEntrepriseAddMetierOldNames.vue'
import FilterEntrepriseAddOffre from '@/components/FilterEntrepriseAddOffre.vue'
import FilterEntrepriseAddDomaine from '@/components/FilterEntrepriseAddDomaine.vue'

const formFilterEntreprise = ref(null)
const validFilterEntreprise = ref(true)
const dialog = ref(false)
const menuModification = ref(false)

const entreprise = computed(() => store.state.entreprise)
const user = computed(() => store.state.user)

const nameRules = [
  v =>
    !v ||
    (v && v.length <= 50) ||
    'Le champ doit être moins que 50 caractères'
]

const codePostalRules = v => {
  if (!v && v !== 0) return true
  if (v >= 0 && v <= 9999) return true
  return 'En Suisse, 4 chiffres'
}

onBeforeMount(() => {
  store.dispatch('user/fetchUsers', {}).catch(error => {
    console.error('Erreur lors du chargement des utilisateurs:', error)
  })
})

async function filterEntreprise() {
  const { valid } = await formFilterEntreprise.value.validate()

  if (valid) {
    NProgress.start()
    store
      .dispatch('entreprise/saveFilterEntreprise', entreprise.value.filter)
      .then(() => {})
      .catch(() => {})
      .finally(() => {
        dialog.value = false
        NProgress.done()
      })
  }
}

function deletefilterEntreprise() {
  NProgress.start()
  store
    .dispatch('entreprise/deleteFilterEntreprise')
    .then(() => {})
    .catch(() => {})

  store
    .dispatch('entreprise/fetchEntreprises', true)
    .then(() => {})
    .catch(() => {})
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}

function deleteMetier(typeMetierId) {
  NProgress.start()
  store
    .dispatch('entreprise/deleteFilterMetier', typeMetierId)
    .then(() => {})
    .catch(() => {})
    .finally(() => {
      NProgress.done()
    })
}

function deleteDomaine(typeDomaineId) {
  store
    .dispatch('entreprise/deleteFilterDomaine', typeDomaineId)
    .then(() => {})
    .catch(() => {})
}

function deleteOffre(typeOffreId) {
  store
    .dispatch('entreprise/deleteFilterOffre', typeOffreId)
    .then(() => {})
    .catch(() => {})
}
</script>

<style scoped>
</style>
