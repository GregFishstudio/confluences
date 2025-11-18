<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const props = defineProps({
  typeDomaine: {
    type: Object,
    required: true
  }
})

const router = useRouter()

const dialog = ref(false)
const validCreateDomaine = ref(true)
const formDeleteDomaine = ref(null)

function closeDialog() {
  dialog.value = false
}

function deleteTypeDomaine() {
  NProgress.start()

  store
    .dispatch(
      'typeDomaine/deleteTypeDomaine',
      props.typeDomaine.typeDomaineId
    )
    .then(() => {
      router.push({ name: 'Domaines' })
    })
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}
</script>

<template>
  <v-form
    ref="formDeleteDomaine"
    v-model="validCreateDomaine"
    lazy-validation
  >
    <v-dialog v-model="dialog" max-width="650">
      
      <!-- Bouton d’ouverture -->
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
          <span class="text-h6 font-weight-bold">Supprimer un domaine</span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">
          <h3 class="text-body-1 font-weight-medium mb-4">
            Attention une suppression est définitive.
          </h3>

          <!-- Entreprises empêchant la suppression -->
          <v-card
            v-if="props.typeDomaine.entrepriseDomaines.length > 0"
            class="mb-4"
          >
            <v-list disabled>
              <v-subheader>
                Supprimez d’abord les entreprises liées à ce domaine.
              </v-subheader>

              <v-list-item
                v-for="(ed, i) in props.typeDomaine.entrepriseDomaines"
                :key="i"
              >
                <v-list-item-title>
                  {{ ed.entreprise.nom }}
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
            v-if="props.typeDomaine.entrepriseDomaines.length === 0"
            color="red"
            rounded
            @click="deleteTypeDomaine"
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
