<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const props = defineProps({
  stagiaire: {
    type: Object,
    required: true
  }
})

const router = useRouter()

const dialog = ref(false)
const validCreateStagiaire = ref(true)
const formDeleteStagiaire = ref(null)

function closeDialog() {
  dialog.value = false
}

function deleteStagiaire() {
  NProgress.start()

  store
    .dispatch('stagiaire/deleteStagiaire', props.stagiaire.stagiaireId)
    .then(() => {
      router.push({ name: 'Stagiaires' })
    })
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}
</script>

<template>
  <v-form
    ref="formDeleteStagiaire"
    v-model="validCreateStagiaire"
    lazy-validation
  >
    <v-dialog v-model="dialog" max-width="650">
      <template #activator="{ props: activatorProps }">
        <v-btn
          v-bind="activatorProps"
          color="red"
          rounded
          class="ma-2"
        >
          <v-icon left small>mdi-delete-outline</v-icon>
          Supprimer
        </v-btn>
      </template>

      <v-card rounded="lg" class="pa-2">

        <v-card-title class="py-4">
          <v-icon color="red" class="mr-2">mdi-alert-outline</v-icon>
          <span class="text-h6 font-weight-bold">Supprimer un stagiaire</span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">

          <h3 class="text-body-1 font-weight-medium mb-4">
            La suppression est bloquée.
          </h3>

          <!-- Stages liés -->
          <v-card class="mb-4" v-if="props.stagiaire.stageStagiaires.length > 0">
            <v-list disabled>
              <v-subheader>Il faut supprimer ces stages :</v-subheader>
              <v-list-item
                v-for="(stage,i) in props.stagiaire.stageStagiaires"
                :key="i"
              >
                <v-list-item-title v-if="stage.entreprise">
                  {{ stage.entreprise.nom }}
                </v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>

          <!-- Derniers contacts - formateur -->
          <v-card class="mb-4"
            v-if="props.stagiaire.entreprisFormateurIdDernierContactNavigations.length > 0">
            <v-list disabled>
              <v-subheader class="text-red">
                Dernier contact lié à une entreprise (par qui) à supprimer manuellement
              </v-subheader>
              <v-list-item
                v-for="(dc,i) in props.stagiaire.entreprisFormateurIdDernierContactNavigations"
                :key="i"
              >
                <v-list-item-title>{{ dc.nom }}</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>

          <!-- Derniers contacts - stagiaire -->
          <v-card class="mb-4"
            v-if="props.stagiaire.entreprisStagiaireIdDernierContactNavigations.length > 0">
            <v-list disabled>
              <v-subheader class="text-red">
                Dernier contact lié à une entreprise (pour qui) à supprimer manuellement
              </v-subheader>
              <v-list-item
                v-for="(dc,i) in props.stagiaire.entreprisStagiaireIdDernierContactNavigations"
                :key="i"
              >
                <v-list-item-title>{{ dc.nom }}</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>

          <!-- Créateurs de stages -->
          <v-card class="mb-4"
            v-if="props.stagiaire.stageCreateurs.length > 0">
            <v-list disabled>
              <v-subheader class="text-red">
                Créateur-trice lié à un stage à supprimer manuellement
              </v-subheader>
              <v-list-item
                v-for="(creator,i) in props.stagiaire.stageCreateurs"
                :key="i"
              >
                <v-list-item-title>{{ creator.nom }}</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>

          <h3 class="mt-4 text-red">
            Une fois les stages supprimés, supprimer aussi l’utilisateur
            depuis le site de télétravail.
          </h3>

        </v-card-text>

        <v-divider />

        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="closeDialog">
            Fermer
          </v-btn>

          <v-btn
            v-if="
              props.stagiaire.stageStagiaires.length === 0 &&
              props.stagiaire.entreprisFormateurIdDernierContactNavigations.length === 0 &&
              props.stagiaire.entreprisStagiaireIdDernierContactNavigations.length === 0 &&
              props.stagiaire.stageCreateurs.length === 0
            "
            color="red"
            rounded
            @click="deleteStagiaire"
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
  transition: 0.2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,.12);
}
</style>
