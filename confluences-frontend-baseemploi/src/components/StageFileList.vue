<!--
  Projet: Gestion des stagiaires
  Migration Vue 3 + Vuetify 3
  Fichier : StageFileList.vue
-->

<template>
  <v-card class="pa-4 elevation-2 rounded-lg" outlined>
    <div class="d-flex align-center mb-4">
      <v-icon start color="primary">mdi-paperclip</v-icon>
      <span class="text-subtitle-1 font-weight-bold">Fichiers</span>
      <v-spacer></v-spacer>
      <CreateStageFile :stageId="stage.stageId" />
    </div>

    <v-row v-if="stage.stageFiles.length > 0" dense>
      <v-col
        v-for="(item, i) in stage.stageFiles"
        :key="i"
        cols="12"
        sm="6"
        md="4"
        lg="3"
      >
        <v-card
          class="file-card d-flex align-center pa-3"
          variant="outlined"
          rounded="lg"
          @click="downloadFile(item.stageFileId)"
        >
          <v-icon color="primary" size="28" class="mr-3">mdi-file-document-outline</v-icon>

          <span class="text-body-2 file-name">{{ item.fileName }}</span>

          <v-spacer></v-spacer>

          <v-icon
            color="red"
            size="20"
            style="cursor: pointer"
            @click.stop="deleteStageFile(item.stageFileId)"
          >
            mdi-delete-outline
          </v-icon>
        </v-card>
      </v-col>
    </v-row>

    <div v-else class="pa-6 text-center">
      <v-icon size="36" color="grey lighten-1" class="mb-2">mdi-folder-open-outline</v-icon>
      <p class="text-body-2 text-medium-emphasis mb-0">Aucun fichier attaché à ce stage.</p>
    </div>
  </v-card>
</template>

<script>
import store from "@/store/index.js";
import NProgress from "nprogress";
import CreateStageFile from "@/components/CreateStageFile.vue";

export default {
  props: {
    stage: {
      type: Object,
      required: true,
    },
  },

  components: {
    CreateStageFile,
  },

  methods: {
    /** 🗑️ Supprime un fichier lié au stage */
    deleteStageFile(stageFileId) {
      NProgress.start();

      store
        .dispatch("stageFile/deleteStageFile", { stageFileId })
        .then(() => {
          store.dispatch("stage/deleteStageFile", stageFileId);
        })
        .finally(() => NProgress.done());
    },

    /** ⬇️ Télécharge le fichier */
    downloadFile(stageFileId) {
      store
        .dispatch("stageFile/fetchStageFile", stageFileId)
        .then((response) => {
          let fileName = "fichier";

          // Extraction du filename
          const cd = response.headers["content-disposition"];
          const regex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
          const result = regex.exec(cd);
          if (result && result[1]) {
            fileName = result[1].replace(/['"]/g, "");
          }

          // Création du blob
          const url = window.URL.createObjectURL(
            new Blob([response.data], {
              type: response.headers["content-type"],
            })
          );

          // Téléchargement
          const link = document.createElement("a");
          link.href = url;
          link.download = fileName;
          document.body.appendChild(link);
          link.click();
          link.remove();
        })
        .catch(() => {});
    },
  },
};
</script>

<style scoped>
.file-card {
  cursor: pointer;
  transition: box-shadow 0.2s ease, transform 0.2s ease;
}

.file-card:hover {
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.file-name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
