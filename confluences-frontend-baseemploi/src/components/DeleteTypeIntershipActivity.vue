<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const props = defineProps({
  typeIntershipActivity: {
    type: Object,
    required: true
  }
})

const router = useRouter()

const dialog = ref(false)
const validCreate = ref(true)
const formDelete = ref(null)

function closeDialog() {
  dialog.value = false
}

function deleteTypeIntershipActivity() {
  NProgress.start()
  store
    .dispatch(
      'typeIntershipActivity/deleteTypeIntershipActivity',
      props.typeIntershipActivity.typeIntershipActivityId
    )
    .then(() => {
      router.push({ name: 'Activites' })
    })
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}
</script>

<template>
  <v-form
    ref="formDelete"
    v-model="validCreate"
    lazy-validation
  >
    <v-dialog v-model="dialog" max-width="650">
      
      <!-- Activation -->
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

      <v-card rounded="lg" class="pa-1">

        <v-card-title class="py-4">
          <v-icon color="red" class="mr-2">mdi-alert-outline</v-icon>
          <span class="text-h6 font-weight-bold">Supprimer une activité</span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">
          <h3 class="text-body-1 font-weight-medium mb-4">
            Attention une suppression est définitive !
          </h3>

          <!-- Liste des stages liés -->
          <v-card
            class="mb-4"
            v-if="props.typeIntershipActivity.stages.length > 0"
          >
            <v-list disabled>
              <v-subheader>
                Supprimez les stages liés à cette activité avant de la supprimer.
              </v-subheader>

              <v-list-item
                v-for="(stage, i) in props.typeIntershipActivity.stages"
                :key="i"
              >
                <v-list-item-title>
                  Stage #{{ stage.stageId }}
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
            v-if="props.typeIntershipActivity.stages.length === 0"
            color="red"
            rounded
            @click="deleteTypeIntershipActivity"
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
  box-shadow: 0 4px 10px rgba(0,0,0,.12);
}
</style>
