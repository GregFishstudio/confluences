<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'une activité
  -- Fichier : TypeIntershipActivityEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Type d'activité</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateTypeIntershipActivity"
          v-model="validCreateTypeIntershipActivity"
          lazy-validation
        >
          <v-text-field
            v-model="typeIntershipActivity.nom"
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
            <DeleteTypeIntershipActivity
              :typeIntershipActivity="this.typeIntershipActivity"
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
import DeleteTypeIntershipActivity from '@/components/DeleteTypeIntershipActivity.vue'

export default {
  props: {
    typeIntershipActivity: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteTypeIntershipActivity
  },

  data: () => ({
    validCreateTypeIntershipActivity: true,
    dialog: false,
    libelleRules: [
      v => !!v || 'Le champ est obligatoire',
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ]
  }),

  methods: {
    // Si le formulaire est valide, sauvegarde de l'activité
    submit() {
      if (this.$refs.formCreateTypeIntershipActivity.validate()) {
        NProgress.start()
        store
          .dispatch(
            'typeIntershipActivity/editTypeIntershipActivity',
            this.typeIntershipActivity
          )
          .then(() => {
            this.$router.push({
              name: 'Activites'
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
