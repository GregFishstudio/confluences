<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'un type de stage
  -- Fichier : TypeStageEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Taux d'occupation</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeStage"
          v-model="validCreateTypeStage"
          lazy-validation
        >
          <v-text-field
            v-model="typeStage.nom"
            :counter="50"
            :rules="libelleRules"
            label="Nom"
            required
          ></v-text-field>
        </v-form>
      </v-col>
    </v-row>

    <div class="action-container">
      <v-row>
        <v-col>
          <div class="text-center">
            <v-btn
              class="ma-2"
              tile
              color="success"
              dark
              min-width="150"
              @click="submit()"
            >
              Sauvegarder
            </v-btn>
            <DeleteTypeStage :typeStage="this.typeStage" />
            <v-btn
              class="ma-2"
              tile
              color="primary"
              dark
              min-width="150"
              @click="$router.go(-1)"
            >
              Annuler
            </v-btn>
          </div>
        </v-col>
      </v-row>
    </div>
  </v-container>
</template>

<script>
import store from '@/store/index.js'
import NProgress from 'nprogress'
import DeleteTypeStage from '@/components/DeleteTypeStage.vue'

export default {
  props: {
    typeStage: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeStage
  },

  data: () => ({
    validCreateTypeStage: true,
    dialog: false,
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ]
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde du type de stage
    submit() {
      if (this.$refs.formCreateTypeStage.validate()) {
        NProgress.start()
        store
          .dispatch('typeStage/editTypeStage', this.typeStage)
          .then(() => {
            this.$router.push({
              name: 'Type-Stages'
            })
          })
          .catch(() => {})
        this.dialog = false
        NProgress.done()
      }
    }
  }
}
</script>
