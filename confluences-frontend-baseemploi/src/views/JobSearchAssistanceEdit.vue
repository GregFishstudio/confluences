<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 06.11.2022
  -- Description : Formulaire de modification d'un ARE
  -- Fichier : JobSearchAssistanceEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>ARE</h1>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-form
          ref="formCreateContact"
          v-model="validCreateContact"
          lazy-validation
        >
          <v-text-field
            v-model="jobSearchAssistance.address"
            label="Adresse"
          ></v-text-field>
          <v-text-field
            v-model="jobSearchAssistance.town"
            label="Ville"
          ></v-text-field>
          <v-text-field
            v-model="jobSearchAssistance.zipCode"
            label="Code postale"
          ></v-text-field>

          <v-autocomplete
            v-model="jobSearchAssistance.typeJobSearchAssistanceOccurrenceId"
            :items="
              typeJobSearchAssistanceOccurrence.typeSearchAssistanceOccurrences
            "
            item-value="typeJobSearchAssistanceOccurrenceId"
            item-text="description"
            label="Occurrence"
            clearable
          ></v-autocomplete>
          <v-menu
            ref="menuCreation"
            v-model="menuCreation"
            :close-on-content-click="true"
            :return-value.sync="date"
            transition="scale-transition"
            offset-y
            min-width="290px"
          >
            <template v-slot:activator="{ on, attrs }">
              <v-text-field
                v-model="jobSearchAssistance.date"
                label="Date"
                readonly
                v-bind="attrs"
                v-on="on"
              ></v-text-field>
            </template>
            <v-date-picker
              v-model="jobSearchAssistance.date"
              no-title
              scrollable
            >
            </v-date-picker>
          </v-menu>
          <v-text-field
            v-model="jobSearchAssistance.description"
            label="Description"
          ></v-text-field>
          <v-text-field
            v-model="jobSearchAssistance.website"
            label="Site internet"
          ></v-text-field>
          <v-text-field
            v-model="jobSearchAssistance.keyWords"
            label="Mots clés"
          ></v-text-field>
          <v-autocomplete
            v-model="jobSearchAssistance.typeJobSearchAssistanceId"
            :items="typeJobSearchAssistance.typeSearchAssistances"
            item-value="typeJobSearchAssistanceId"
            item-text="description"
            label="Type"
            clearable
          ></v-autocomplete>
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
            <DeleteJobSearchAssistance
              :jobSearchAssistance="this.jobSearchAssistance"
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
import { mapState } from 'vuex'
import NProgress from 'nprogress'
import DeleteJobSearchAssistance from '@/components/DeleteJobSearchAssistance.vue'
import moment from 'moment'

function getTypeJobSearchAssistances() {
  store
    .dispatch('typeJobSearchAssistance/fetchTypeJobSearchAssistances', {})
    .then(() => {})
}

function getTypeJobSearchAssistanceOccurrences() {
  store
    .dispatch(
      'typeJobSearchAssistanceOccurrence/fetchtypeSearchAssistanceOccurrences',
      {}
    )
    .then(() => {})
}

export default {
  props: {
    jobSearchAssistance: {
      type: Object,
      required: true
    }
  },

  components: {
    DeleteJobSearchAssistance
  },

  data: () => ({
    validCreateContact: true,
    dialog: false,
    menuCreation: false,
    date: null
  }),

  beforeCreate(routeTo, routeFrom, next) {
    getTypeJobSearchAssistances(routeTo, next)
    getTypeJobSearchAssistanceOccurrences(routeTo, next)
  },

  created() {
    // Formattage des dates afin qu'elles s'affichent correctement
    this.jobSearchAssistance.date = this.formatDate(
      this.jobSearchAssistance.date
    )
  },

  computed: {
    ...mapState([
      'typeJobSearchAssistanceOccurrence',
      'typeJobSearchAssistance'
    ])
  },

  methods: {
    // Si le formulaire est valide, sauvegarde du contact
    submit() {
      if (this.$refs.formCreateContact.validate()) {
        NProgress.start()
        store
          .dispatch(
            'jobSearchAssistance/editJobSearchAssistance',
            this.jobSearchAssistance
          )
          .then(() => {
            this.$router.push({
              name: 'JobSearchAssistances'
            })
          })
          .catch(() => {})
        this.dialog = false
        NProgress.done()
      }
    },
    formatDate: function(date) {
      return moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD')
    }
  }
}
</script>
