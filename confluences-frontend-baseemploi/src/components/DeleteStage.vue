<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const props = defineProps({
  stage: {
    type: Object,
    required: true
  }
})

const router = useRouter()
const dialog = ref(false)
const validCreateStage = ref(true)
const formDeleteStage = ref(null)

function closeDialog() {
  dialog.value = false
}

function deleteStage() {
  NProgress.start()

  store
    .dispatch('stage/deleteStage', props.stage.stageId)
    .then(() => {
      router.push({ name: 'Stages' })
    })
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}
</script>

<template>
  <v-form
    ref="formDeleteStage"
    v-model="validCreateStage"
    lazy-validation
  >
    <v-dialog v-model="dialog" max-width="500">
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
          <v-icon class="mr-2" color="red">mdi-alert-outline</v-icon>
          <span class="text-h6 font-weight-bold">Supprimer une expérience</span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">
          <h3 class="text-body-1 font-weight-medium mb-4">
            Attention : une suppression est <strong>définitive</strong> !
          </h3>
        </v-card-text>

        <v-divider />

        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="closeDialog">
            Fermer
          </v-btn>

          <v-btn color="red" rounded @click="deleteStage">
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
