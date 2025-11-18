<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire d'ajout d'un métier au filtre
  -- Fichier : FilterEntrepriseAddMetier.vue
  -->

<template>
  <v-row justify="end">
    <v-form ref="formCreateMetier" v-model="validCreateMetier" lazy-validation>
      <v-dialog v-model="dialog" max-width="600px">
        <template v-slot:activator="{ on, attrs }">
          <v-btn
            color="success"
            dark
            v-bind="attrs"
            v-on="on"
            outlined
            fab
            x-small
            class="mx-3"
          >
            <v-icon>
              mdi-plus
            </v-icon>
          </v-btn>
        </template>
        <v-card>
          <v-card-title>
            <span class="headline">Ajouter un métier</span>
          </v-card-title>
          <v-card-text>
            <v-autocomplete
              v-model="metier.typeMetierId"
              :items="typeMetier.typeMetiers"
              item-value="typeMetierId"
              item-text="libelle"
              :rules="[v => !!v || 'Obligatoire']"
              label="Type de métier"
              required
            ></v-autocomplete>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="dialog = false">
              Fermer
            </v-btn>
            <v-btn color="blue darken-1" text @click="submit()"
              >Sauvegarder
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-form>
  </v-row>
</template>

<script>
import store from '@/store/index.js'
import { mapState } from 'vuex'
import NProgress from 'nprogress'

function getTypeMetiers() {
  store.dispatch('typeMetier/fetchTypeMetiers', {}).then(() => {})
}

export default {
  data: () => ({
    validCreateMetier: true,
    dialog: false,
    metier: {
      typeMetierId: 0,
      libelle: '',
      oldNames: ''
    }
  }),

  // Chargement les métiers avant la création du composant
  beforeCreate(routeTo, routeFrom, next) {
    getTypeMetiers(routeTo, next)
  },

  computed: {
    ...mapState(['entreprise', 'typeMetier'])
  },

  methods: {
    // Ajoute le métier au filtre
    submit() {
      if (this.$refs.formCreateMetier.validate()) {
        this.metier.libelle = this.typeMetier.typeMetiers.find(
          t => t.typeMetierId == this.metier.typeMetierId
        ).libelle
        this.metier.oldNames = this.typeMetier.typeMetiers.find(
          t => t.typeMetierId == this.metier.typeMetierId
        ).oldNames

        NProgress.start()
        store
          .dispatch('entreprise/addFilterEntrepriseMetier', this.metier)
          .then(() => {})
          .catch(() => {})
        this.dialog = false
        NProgress.done()
      }
    }
  }
}
</script>
