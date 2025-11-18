<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'un type d'entreprise
  -- Fichier : TypeEntrepriseEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Catégorie pour l'entreprise</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeEntreprise"
          v-model="validCreateTypeEntreprise"
          lazy-validation
        >
          <v-text-field
            v-model="typeEntreprise.nom"
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
            <DeleteTypeEntreprise :typeEntreprise="this.typeEntreprise" />
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
import DeleteTypeEntreprise from '@/components/DeleteTypeEntreprise.vue'

export default {
  props: {
    typeEntreprise: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeEntreprise
  },

  data: () => ({
    validCreateTypeEntreprise: true,
    dialog: false,
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ]
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde du type d'entreprise
    submit() {
      if (this.$refs.formCreateTypeEntreprise.validate()) {
        NProgress.start()
        store
          .dispatch('typeEntreprise/editTypeEntreprise', this.typeEntreprise)
          .then(() => {
            this.$router.push({
              name: 'Type-Entreprises'
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
