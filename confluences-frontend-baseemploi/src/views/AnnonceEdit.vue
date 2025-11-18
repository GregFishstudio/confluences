<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'une annonce
  -- Fichier : AnnonceEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Annonce de stage</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeAnnonce"
          v-model="validCreateTypeAnnonce"
          lazy-validation
        >
          <v-text-field
            v-model="typeAnnonce.libelle"
            :counter="30"
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
            <DeleteTypeAnnonce :typeAnnonce="this.typeAnnonce" />
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
import DeleteTypeAnnonce from '@/components/DeleteTypeAnnonce.vue'

export default {
  props: {
    typeAnnonce: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeAnnonce
  },

  data: () => ({
    validCreateTypeAnnonce: true,
    dialog: false,
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 30 || 'Le champ doit être moins que 30 caractères'
    ]
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde de l'annonce
    submit() {
      if (this.$refs.formCreateTypeAnnonce.validate()) {
        NProgress.start()
        store
          .dispatch('typeAnnonce/editTypeAnnonce', this.typeAnnonce)
          .then(() => {
            this.$router.push({
              name: 'Annonces'
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
