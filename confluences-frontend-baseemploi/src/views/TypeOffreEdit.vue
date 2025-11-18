<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'une offre
  -- Fichier : TypeOffreEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Offre</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeOffre"
          v-model="validCreateTypeOffre"
          lazy-validation
        >
          <v-text-field
            v-model="typeOffre.libelle"
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
            <DeleteTypeOffre :typeOffre="this.typeOffre" />
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
import DeleteTypeOffre from '@/components/DeleteTypeOffre.vue'

export default {
  props: {
    typeOffre: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeOffre
  },

  data: () => ({
    validCreateTypeOffre: true,
    dialog: false,
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 30 || 'Le champ doit être moins que 30 caractères'
    ]
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde de l'offre
    submit() {
      if (this.$refs.formCreateTypeOffre.validate()) {
        NProgress.start()
        store
          .dispatch('typeOffre/editTypeOffre', this.typeOffre)
          .then(() => {
            this.$router.push({
              name: 'Offres'
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
