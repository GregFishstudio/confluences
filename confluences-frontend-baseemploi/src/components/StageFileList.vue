<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Liste des fichiers depuis un stage
  -- Fichier : StageFileList.vue
  -->

<template>
  <v-card class="mx-auto" outlined>
    <v-list>
      <v-subheader>
        Fichiers
        <v-spacer></v-spacer>
        <CreateStageFile :stageId="stage.stageId" />
      </v-subheader>
      <v-simple-table v-if="stage.stageFiles.length > 0">
        <template v-slot:default>
          <thead>
            <tr>
              <th class="text-left">Nom</th>
              <th class="text-left">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(item, i) in stage.stageFiles"
              :key="i"
              @click="getFile(item.stageFileId)"
            >
              <td>{{ item.fileName }}</td>
              <td>
                <v-icon color="red" @click="deleteStageFile(item.stageFileId)">
                  mdi-delete
                </v-icon>
              </td>
            </tr>
          </tbody>
        </template>
      </v-simple-table>
    </v-list>
  </v-card>
</template>

<script>
import store from '@/store/index.js'
import NProgress from 'nprogress'
import CreateStageFile from '@/components/CreateStageFile.vue'

export default {
  props: {
    stage: {
      type: Object,
      required: true
    }
  },

  components: {
    CreateStageFile
  },

  methods: {
    // Supprime un contact
    deleteStageFile(stageFileId) {
      NProgress.start()
      store
        .dispatch('stageFile/deleteStageFile', {
          stageFileId
        })
        .then(() => {
          store.dispatch('stage/deleteStageFile', stageFileId)
        })
        .catch(() => {})
      this.dialog = false
      NProgress.done()
    },
    getFile(stageFileId) {
      store
        .dispatch('stageFile/fetchStageFile', stageFileId)
        .then(response => {
          let fileName = ''
          var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/
          var matches = filenameRegex.exec(
            response.headers['content-disposition']
          )
          if (matches != null && matches[1]) {
            fileName = matches[1].replace(/['"]/g, '')
          }
          const url = window.URL.createObjectURL(
            new Blob([response.data], {
              type: response.headers['content-type']
            })
          )
          const link = document.createElement('a')
          link.href = url
          link.setAttribute('download', fileName)
          document.body.appendChild(link)
          link.click()
        })
        .catch(() => {})
    }
  }
}
</script>

<style></style>
