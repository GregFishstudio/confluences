<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'un moyen
  -- Fichier : MoyenEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Communication</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeMoyen"
          v-model="validCreateTypeMoyen"
          lazy-validation
        >
          <v-text-field
            v-model="typeMoyen.libelle"
            :counter="20"
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
            <DeleteTypeMoyen :typeMoyen="this.typeMoyen" />
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
import DeleteTypeMoyen from '@/components/DeleteTypeMoyen.vue'

export default {
  props: {
    typeMoyen: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeMoyen
  },

  data: () => ({
    validCreateTypeMoyen: true,
    dialog: false,
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 20 || 'Le champ doit être moins que 20 caractères'
    ]
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde du moyen
    submit() {
      if (this.$refs.formCreateTypeMoyen.validate()) {
        NProgress.start()
        store
          .dispatch('typeMoyen/editTypeMoyen', this.typeMoyen)
          .then(() => {
            this.$router.push({
              name: 'Moyens'
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
