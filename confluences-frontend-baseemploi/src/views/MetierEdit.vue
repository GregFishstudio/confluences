<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'un métier
  -- Fichier : MetierEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Métier</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeMetier"
          v-model="validCreateTypeMetier"
          lazy-validation
        >
          <v-text-field
            v-model="typeMetier.libelle"
            :rules="libelleRules"
            label="Nom"
            required
          ></v-text-field>
          <v-text-field
            v-model="typeMetier.oldNames"
            label="Autres noms"
            :rules="oldNamesRules"
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
            <DeleteTypeMetier :typeMetier="this.typeMetier" />
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
import DeleteTypeMetier from '@/components/DeleteTypeMetier.vue'

export default {
  props: {
    typeMetier: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeMetier
  },

  data: () => ({
    validCreateTypeMetier: true,
    dialog: false,
    oldNamesRules: [],
    libelleRules: [v => !!v || 'Le champ est obligatoire']
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde du métier
    submit() {
      if (this.$refs.formCreateTypeMetier.validate()) {
        NProgress.start()
        store
          .dispatch('typeMetier/editTypeMetier', this.typeMetier)
          .then(() => {
            this.$router.push({
              name: 'Metiers'
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
