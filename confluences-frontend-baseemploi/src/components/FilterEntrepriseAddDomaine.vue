<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire d'ajout d'un domaine au filtre
  -- Fichier : FilterEntrepriseAddDomaine.vue
  -->

<template>
  <v-row justify="end">
    <v-form
      ref="formCreateDomaine"
      v-model="validCreateDomaine"
      lazy-validation
    >
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
            <span class="headline">Ajouter un domaine</span>
          </v-card-title>
          <v-card-text>
            <v-autocomplete
              v-model="domaine.typeDomaineId"
              :items="typeDomaine.typeDomaines"
              item-value="typeDomaineId"
              item-text="libelle"
              :rules="[v => !!v || 'Obligatoire']"
              label="Type de domaine"
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

function getTypeDomaines() {
  store.dispatch('typeDomaine/fetchTypeDomaines', {}).then(() => {})
}

export default {
  data: () => ({
    validCreateDomaine: true,
    dialog: false,
    domaine: {
      domaineId: 0,
      libelle: ''
    }
  }),

  // Chargement la liste des domaines
  beforeCreate(routeTo, routeFrom, next) {
    getTypeDomaines(routeTo, next)
  },

  computed: {
    ...mapState(['entreprise', 'typeDomaine'])
  },

  methods: {
    // Ajoute le domaine au filtre
    submit() {
      if (this.$refs.formCreateDomaine.validate()) {
        this.domaine.libelle = this.typeDomaine.typeDomaines.find(
          t => t.typeDomaineId == this.domaine.typeDomaineId
        ).libelle
        NProgress.start()
        store
          .dispatch('entreprise/addFilterEntrepriseDomaine', this.domaine)
          .then(() => {})
          .catch(() => {})
        this.dialog = false
        NProgress.done()
      }
    }
  }
}
</script>
