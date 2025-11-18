<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire d'ajout d'une offre au filtre
  -- Fichier : FilterEntrepriseAddOffre.vue
  -->

<template>
  <v-row justify="end">
    <v-form ref="formCreateOffre" v-model="validCreateOffre" lazy-validation>
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
            <span class="headline">Ajouter un offre</span>
          </v-card-title>
          <v-card-text>
            <v-autocomplete
              v-model="offre.typeOffreId"
              :items="typeOffre.typeOffres"
              item-value="typeOffreId"
              item-text="libelle"
              :rules="[v => !!v || 'Obligatoire']"
              label="Type d'offre"
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

function getTypeOffres() {
  store.dispatch('typeOffre/fetchTypeOffres', {}).then(() => {})
}

export default {
  data: () => ({
    validCreateOffre: true,
    dialog: false,
    offre: {
      typeOffreId: 0,
      libelle: ''
    }
  }),

  // Chargement des offres avant la création du composant
  beforeCreate(routeTo, routeFrom, next) {
    getTypeOffres(routeTo, next)
  },

  computed: {
    ...mapState(['entreprise', 'typeOffre'])
  },

  methods: {
    // Ajout d'une offre au filtre
    submit() {
      if (this.$refs.formCreateOffre.validate()) {
        this.offre.libelle = this.typeOffre.typeOffres.find(
          t => t.typeOffreId == this.offre.typeOffreId
        ).libelle
        NProgress.start()
        store
          .dispatch('entreprise/addFilterEntrepriseOffre', this.offre)
          .then(() => {})
          .catch(() => {})
        this.dialog = false
        NProgress.done()
      }
    }
  }
}
</script>
