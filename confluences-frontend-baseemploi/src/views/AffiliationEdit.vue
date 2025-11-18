<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'une affiliation
  -- Fichier : AffiliationEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Affiliation</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeAffiliation"
          v-model="validCreateTypeAffiliation"
          lazy-validation
        >
          <v-text-field
            v-model="typeAffiliation.code"
            :counter="10"
            label="Code"
            :rules="codeRules"
            required
          ></v-text-field>
          <v-text-field
            v-model="typeAffiliation.libelle"
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
            <DeleteTypeAffiliation :typeAffiliation="this.typeAffiliation" />
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
import DeleteTypeAffiliation from '@/components/DeleteTypeAffiliation.vue'

export default {
  props: {
    typeAffiliation: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeAffiliation
  },

  data: () => ({
    validCreateTypeAffiliation: true,
    dialog: false,
    codeRules: [
      v => !!v || 'Le champ est obligatoire',
      v => /(\b[A-Z0-9]{1,}\b)/.test(v) || 'En majuscule seulement',
      v => (v && v.length <= 10) || 'Le nom doit être moins que 10 caractères'
    ],
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ]
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde de l'affiliation
    submit() {
      if (this.$refs.formCreateTypeAffiliation.validate()) {
        NProgress.start()
        store
          .dispatch('typeAffiliation/editTypeAffiliation', this.typeAffiliation)
          .then(() => {
            this.$router.push({
              name: 'Affiliations'
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
