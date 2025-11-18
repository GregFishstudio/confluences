<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'un contact
  -- Fichier : ContactEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Contact</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateContact"
          v-model="validCreateContact"
          lazy-validation
        >
          <v-autocomplete
            v-model="contact.entrepriseId"
            :items="entreprise.entreprises"
            item-value="entrepriseId"
            item-text="nom"
            label="Entreprise"
          ></v-autocomplete>
          <v-text-field
            v-model="contact.prenom"
            :counter="50"
            label="Prénom"
            :rules="nameRules"
            required
          ></v-text-field>
          <v-text-field
            v-model="contact.nom"
            :counter="50"
            :rules="nameRules"
            label="Nom"
            required
          ></v-text-field>
          <v-text-field
            v-model="contact.fonction"
            :counter="50"
            :rules="nameRules"
            label="Fonction"
            required
          ></v-text-field>
          <v-text-field
            v-model="contact.email"
            :counter="50"
            :rules="emailRules"
            label="Email"
          ></v-text-field>
          <v-text-field
            v-model="contact.telFix"
            :counter="13"
            :rules="phonesRules"
            label="Téléphone fixe"
          ></v-text-field>
          <v-text-field
            v-model="contact.natel"
            :counter="13"
            :rules="phonesRules"
            label="Natel"
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
            <DeleteContact :contact="this.contact" />
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
import { mapState } from 'vuex'
import NProgress from 'nprogress'
import DeleteContact from '@/components/DeleteContact.vue'

function getEntreprises() {
  store.dispatch('entreprise/fetchEntreprises', {}).then(() => {})
}

export default {
  props: {
    contact: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteContact
  },

  data: () => ({
    validCreateContact: true,
    dialog: false,
    nameRules: [
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ],
    emailRules: [
      v => !v || /.+@.+\..+/.test(v) || "L'email doit être valide",
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ],
    phonesRules: [
      v => !v || v.length <= 13 || 'Le champ doit être moins que 13 caractères'
    ],
    requiredRule: [v => !!v || 'Le champ est obligatoire']
  }),

  beforeCreate(routeTo, routeFrom, next) {
    // Charger les entreprises avant de créer le composant
    getEntreprises(routeTo, next)
  },

  computed: {
    ...mapState(['entreprise'])
  },

  methods: {
    // Si le formulaire est valide, sauvegarde du contact
    submit() {
      if (this.$refs.formCreateContact.validate()) {
        NProgress.start()
        store
          .dispatch('contact/editContact', this.contact)
          .then(() => {
            this.$router.push({
              name: 'Contacts'
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
