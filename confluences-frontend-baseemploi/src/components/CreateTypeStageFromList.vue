<template>
  <v-row class="d-flex justify-end">
    <v-dialog v-model="dialog" max-width="520px" persistent>
      <!-- Bouton d'ouverture -->
      <template v-slot:activator="{ on, attrs }">
        <v-btn
          color="primary"
          rounded
          v-bind="attrs"
          v-on="on"
          class="elevation-2"
        >
          <v-icon left>mdi-plus</v-icon>
          Ajouter un taux d'occupation
        </v-btn>
      </template>

      <!-- Contenu de la popup -->
      <v-card rounded="lg" class="pa-2">
        <!-- Header moderne -->
        <v-card-title class="d-flex align-center py-4">
          <v-icon color="primary" class="mr-2">mdi-briefcase-clock-outline</v-icon>
          <span class="text-h6 font-weight-bold">
            Ajouter un taux d'occupation
          </span>
        </v-card-title>

        <v-divider></v-divider>

        <!-- Formulaire -->
        <v-card-text class="pt-6">
          <v-form ref="formCreateTypeStage" v-model="validCreateTypeStage" lazy-validation>
            <v-text-field
              v-model="typeStage.nom"
              :counter="50"
              :rules="libelleRules"
              label="Nom du taux d'occupation"
              outlined
              dense
              clearable
              rounded
              hide-details="auto"
            ></v-text-field>
          </v-form>
        </v-card-text>

        <!-- Boutons -->
        <v-divider></v-divider>
        <v-card-actions class="d-flex justify-end">
          <v-btn text color="grey darken-1" @click="dialog = false">
            Annuler
          </v-btn>

          <v-btn color="primary" @click="submit" rounded>
            <v-icon left small>mdi-content-save-outline</v-icon>
            Sauvegarder
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-row>
</template>

<script>
import store from '@/store/index.js'
import NProgress from 'nprogress'

export default {
  data: () => ({
    validCreateTypeStage: true,
    dialog: false,
    typeStage: {
      typeStageId: 0,
      code: null,
      libelle: null,
      nom: null
    },
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 50 || 'Le nom doit contenir moins de 50 caractères'
    ]
  }),

  methods: {
    submit() {
      if (this.$refs.formCreateTypeStage.validate()) {
        NProgress.start()

        store
          .dispatch('typeStage/createTypeStage', this.typeStage)
          .then(() => {
            this.$refs.formCreateTypeStage.reset()
            this.dialog = false
          })
          .catch(() => {})

        NProgress.done()
      }
    }
  }
}
</script>

<style scoped>
/* Style léger du hover bouton */
.v-btn {
  transition: 0.2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
