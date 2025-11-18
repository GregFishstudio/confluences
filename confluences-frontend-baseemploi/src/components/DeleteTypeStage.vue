<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

// Props
const props = defineProps({
  typeStage: {
    type: Object,
    required: true
  }
})

// Router
const router = useRouter()

// Validation + Dialog
const dialog = ref(false)
const valid = ref(true)
const formDelete = ref(null)

function closeDialog() {
  dialog.value = false
}

function deleteTypeStage() {
  NProgress.start()

  store
    .dispatch('typeStage/deleteTypeStage', props.typeStage.typeStageId)
    .then(() => {
      router.push({ name: 'Type-Stages' })
    })
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}
</script>

<template>
  <v-form ref="formDelete" v-model="valid" lazy-validation>
    <v-dialog v-model="dialog" max-width="650">

      <!-- Bouton d'ouverture -->
      <template #activator="{ props: activatorProps }">
        <v-btn
          v-bind="activatorProps"
          color="red"
          rounded
          class="ma-2"
        >
          <v-icon small left>mdi-delete-outline</v-icon>
          Supprimer
        </v-btn>
      </template>

      <!-- Carte -->
      <v-card rounded="lg" class="pa-1">

        <!-- Header -->
        <v-card-title class="py-4">
          <v-icon color="red" class="mr-2">mdi-alert-outline</v-icon>
          <span class="text-h6 font-weight-bold">
            Supprimer un taux d'occupation
          </span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">
          <h3 class="text-body-1 font-weight-medium mb-4">
            Attention, cette suppression est définitive !
          </h3>

          <!-- Liste des stages concernés -->
          <v-card
            v-if="props.typeStage.stages.length > 0"
            class="mb-2"
          >
            <v-list disabled>
              <v-subheader>
                Vous devez d'abord supprimer les stages utilisant ce taux d’occupation :
              </v-subheader>

              <v-list-item
                v-for="(stage, i) in props.typeStage.stages"
                :key="i"
              >
                <v-list-item-content>
                  <v-list-item-title>
                    {{ stage.nom }} — {{ stage.stagiaire.firstname }} {{ stage.stagiaire.lastName }}
                  </v-list-item-title>
                </v-list-item-content>
              </v-list-item>

            </v-list>
          </v-card>
        </v-card-text>

        <v-divider />

        <!-- Actions -->
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="closeDialog">
            Fermer
          </v-btn>

          <v-btn
            v-if="props.typeStage.stages.length === 0"
            color="red"
            rounded
            @click="deleteTypeStage"
          >
            <v-icon small left>mdi-delete</v-icon>
            Supprimer
          </v-btn>
        </v-card-actions>

      </v-card>

    </v-dialog>
  </v-form>
</template>

<style scoped>
.v-btn {
  transition: 0.2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
