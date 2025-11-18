<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'une offre
  -- Fichier : typeJobSearchAssistanceEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Type ARE</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreatetypeJobSearchAssistance"
          v-model="validCreatetypeJobSearchAssistance"
          lazy-validation
        >
          <v-text-field
            v-model="typeJobSearchAssistance.description"
            :rules="descriptionRules"
            label="Description"
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
            <DeleteTypeJobSearchAssistance
              :typeJobSearchAssistance="this.typeJobSearchAssistance"
            />
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
import DeleteTypeJobSearchAssistance from '@/components/DeleteTypeJobSearchAssistance.vue'

export default {
  props: {
    typeJobSearchAssistance: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeJobSearchAssistance
  },

  data: () => ({
    validCreatetypeJobSearchAssistance: true,
    dialog: false,
    descriptionRules: [v => !!v || 'Le champ est obligatoire']
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde de l'offre
    submit() {
      if (this.$refs.formCreatetypeJobSearchAssistance.validate()) {
        NProgress.start()
        store
          .dispatch(
            'typeJobSearchAssistance/editTypeJobSearchAssistance',
            this.typeJobSearchAssistance
          )
          .then(() => {
            this.$router.push({
              name: 'TypeJobSearchAssistances'
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
