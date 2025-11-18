<!--  
  Projet: Gestion des stagiaires
  Migration Vue 3 + Vuetify 3
  Fichier : StageFileList.vue
-->

<template>
  <v-card class="mx-auto" outlined>
    <v-list>
      <v-subheader>
        Fichiers
        <v-spacer></v-spacer>
        <CreateStageFile :stageId="stage.stageId" />
      </v-subheader>

      <v-table v-if="stage.stageFiles.length > 0" class="mt-2">
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
            @click="downloadFile(item.stageFileId)"
            class="file-row"
          >
            <td>{{ item.fileName }}</td>

            <td @click.stop>
              <v-icon
                color="red"
                class="mr-2"
                style="cursor: pointer"
                @click="deleteStageFile(item.stageFileId)"
              >
                mdi-delete
              </v-icon>
            </td>
          </tr>
        </tbody>
      </v-table>
    </v-list>
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
.file-row {
  cursor: pointer;
}

.file-row:hover {
  background: #f5f5f5;
}
</style>
