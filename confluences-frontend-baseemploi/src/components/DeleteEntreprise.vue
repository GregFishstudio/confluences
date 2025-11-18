<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const props = defineProps({
  entreprise: {
    type: Object,
    required: true
  }
})

const router = useRouter()
const dialog = ref(false)
const validCreateEntreprise = ref(true)
const formDeleteEntreprise = ref(null)

function closeDialog() {
  dialog.value = false
}

function deleteEntreprise() {
  NProgress.start()

  store
    .dispatch('entreprise/deleteEntreprise', props.entreprise.entrepriseId)
    .then(() => {
      router.push({ name: 'Entreprises' })
    })
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}
</script>

<template>
  <v-form
    ref="formDeleteEntreprise"
    v-model="validCreateEntreprise"
    lazy-validation
  >
    <v-dialog v-model="dialog" max-width="600">
      <template #activator="{ props: activatorProps }">
        <v-btn
          v-bind="activatorProps"
          rounded
          color="red"
          class="ma-2"
        >
          <v-icon left>mdi-delete-outline</v-icon>
          Supprimer
        </v-btn>
      </template>

      <v-card rounded="lg" class="pa-2">

        <v-card-title class="py-4">
          <v-icon class="mr-2" color="red">mdi-alert-outline</v-icon>
          <span class="text-h6 font-weight-bold">Supprimer une entreprise</span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">
          <h3 class="text-body-1 font-weight-medium mb-4">
            Attention : une suppression est <strong>définitive</strong> !
          </h3>

          <!-- Stages liés -->
          <v-card
            v-if="props.entreprise.stages.length > 0"
            class="mx-auto mb-4"
            rounded="lg"
          >
            <v-list disabled>
              <v-subheader>
                Supprime d’abord les stages liés à cette entreprise
              </v-subheader>

              <v-list-item
                v-for="(stage, i) in props.entreprise.stages"
                :key="i"
              >
                <v-list-item-title>
                  {{ stage.stagiaire.firstname }} {{ stage.stagiaire.lastName }} :
                  {{ stage.typeMetier.libelle }}
                </v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>

          <!-- Contacts liés -->
          <v-card
            v-if="props.entreprise.contacts.length > 0"
            class="mx-auto"
            rounded="lg"
          >
            <v-list disabled>
              <v-subheader>
                Supprime d’abord les contacts liés à cette entreprise
              </v-subheader>

              <v-list-item
                v-for="(contact, i) in props.entreprise.contacts"
                :key="i"
              >
                <v-list-item-title>
                  {{ contact.prenom }} {{ contact.nom }}
                </v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>
        </v-card-text>

        <v-divider />

        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="closeDialog">
            Fermer
          </v-btn>

          <v-btn
            v-if="props.entreprise.contacts.length === 0 && props.entreprise.stages.length === 0"
            color="red"
            rounded
            @click="deleteEntreprise"
          >
            <v-icon left small>mdi-delete</v-icon>
            Supprimer
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-form>
</template>

<style scoped>
.v-btn {
  transition: .2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,.12);
}
</style>
