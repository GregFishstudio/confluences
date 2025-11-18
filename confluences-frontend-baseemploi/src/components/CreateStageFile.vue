<template>
  <v-row justify="end">
    <v-dialog v-model="dialog" max-width="600" persistent>
      <template #activator="{ props }">
        <v-btn
          v-bind="props"
          color="success"
          variant="outlined"
          rounded
          size="x-small"
          class="mx-3"
        >
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </template>

      <v-card rounded="lg">
        <v-card-title class="py-4">
          <span class="text-h6 font-weight-bold">Ajouter un fichier</span>
        </v-card-title>

        <v-divider />

        <v-card-text class="pt-6">
          <v-form ref="formCreateContact" v-model="validCreateContact">
            <v-file-input
              v-model="file"
              label="Fichier"
              variant="outlined"
              rounded
              hide-details="auto"
            />
          </v-form>
        </v-card-text>

        <v-divider />

        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="dialog = false">
            Fermer
          </v-btn>

          <v-btn color="primary" rounded @click="submit">
            <v-icon left small>mdi-content-save-outline</v-icon>
            Sauvegarder
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-row>
</template>

<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

/* --- Props --- */
const props = defineProps({
  stageId: {
    type: Number,
    required: true
  }
})

/* --- State --- */
const dialog = ref(false)
const validCreateContact = ref(true)
const file = ref(null)
const formCreateContact = ref(null)

const stageFile = ref({
  stageId: 0,
  fileName: null,
  fileType: null,
  stageFileId: 0
})

/* --- Methods --- */
function addNewData(data) {
  store.dispatch('stage/addStageFile', data)
}

function submit() {
  const formData = new FormData()
  formData.append('stageId', props.stageId)
  formData.append('file', file.value)

  if (formCreateContact.value.validate()) {
    NProgress.start()

    store
      .dispatch('stageFile/createStageFile', formData)
      .then(response => {
        stageFile.value.fileName = response.data.fileName
        stageFile.value.fileType = response.data.fileType
        stageFile.value.stageId = response.data.stageId
        stageFile.value.stageFileId = response.data.stageFileId

        addNewData({ ...stageFile.value })

        file.value = null
        formCreateContact.value.reset()
      })
      .finally(() => {
        dialog.value = false
        NProgress.done()
      })
  }
}
</script>

<style scoped>
.v-btn {
  transition: 0.2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
