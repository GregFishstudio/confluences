<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'un stagiaire
  -- Fichier : StagiaireEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Stagiaire</h1>
      </v-col>
    </v-row>
    <v-form ref="formStagiaire" v-model="valid" lazy-validation>
      <v-row>
        <v-col>
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="stagiaire.nom"
                :rules="lastnameRules"
                label="Nom du stagiaire"
                required
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="stagiaire.prenom"
                :rules="firstnameRules"
                label="Prénom du stagiaire"
                required
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="stagiaire.typeAffiliationId"
                :items="typeAffiliation.typeAffiliations"
                item-value="typeAffiliationId"
                item-text="libelle"
                label="Affiliation"
                clearable
              ></v-autocomplete>
            </v-col>
          </v-row>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-card class="mx-auto" outlined>
            <v-list>
              <v-subheader>
                Stages effectués
                <v-spacer></v-spacer>
              </v-subheader>
              <v-data-table
                :headers="headers"
                :items="stagiaire.stageStagiaires"
                :items-per-page="10"
                @click:row="viewStage"
                class="elevation-1"
                outlined
              >
                <template v-slot:item.debut="{ item }">
                  {{ formatDate(item.debut) }}
                </template>
                <template v-slot:item.fin="{ item }">
                  {{ formatDate(item.fin) }}
                </template>
                <template v-slot:item.rapport="{ item }">
                  <v-checkbox v-model="item.rapport" disabled></v-checkbox>
                </template>
                <template v-slot:item.attestation="{ item }">
                  <v-checkbox v-model="item.attestation" disabled></v-checkbox>
                </template>
              </v-data-table>
            </v-list>
          </v-card>
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
              <DeleteStagiaire :stagiaire="this.stagiaire" />
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
    </v-form>
  </v-container>
</template>

<script>
import store from '@/store/index.js'
import { mapState } from 'vuex'
import NProgress from 'nprogress'
import moment from 'moment'
import DeleteStagiaire from '@/components/DeleteStagiaire.vue'

function getTypeAffiliations() {
  store.dispatch('typeAffiliation/fetchTypeAffiliations', {}).then(() => {})
}

export default {
  props: {
    stagiaire: {
      type: Object,
      required: true
    }
  },

  components: { DeleteStagiaire },

  data: () => ({
    headers: [
      {
        text: 'Entreprise',
        value: 'entreprise.nom'
      },
      { text: 'Adresse', value: 'entreprise.adr1' },
      { text: 'Ville', value: 'entreprise.ville' },
      { text: 'Début', value: 'debut' },
      { text: 'Fin', value: 'fin' },
      { text: 'Métier', value: 'typeMetier.libelle' },
      { text: 'Rapport', value: 'rapport' },
      { text: 'Attestation', value: 'attestation' }
    ],
    valid: true,
    lastnameRules: [
      v => /(\b[A-Z0-9]{1,}\b)/.test(v) || 'En majuscule seulement',
      v => !!v || 'Le champ est obligatoire',
      v => (v && v.length <= 50) || 'Le champ doit être moins que 50 caractères'
    ],
    firstnameRules: [
      v => !!v || 'Le champ est obligatoire',
      v => (v && v.length <= 50) || 'Le champ doit être moins que 50 caractères'
    ],
    select: null
  }),

  // Charger les différents possibilités de choix avant la création du composant
  beforeCreate(routeTo, routeFrom, next) {
    getTypeAffiliations(routeTo, next)
  },

  computed: {
    ...mapState(['typeAffiliation'])
  },

  methods: {
    // Si le formulaire est valide, sauvegarde du stagiaire
    submit() {
      if (this.$refs.formStagiaire.validate()) {
        NProgress.start()
        if (this.stagiaire.typeAffiliationId == null) {
          console.log('doesnt exist')
          this.stagiaire.typeAffiliation = null
        } else {
          console.log(this.stagiaire.typeAffiliationId)
        }
        store
          .dispatch('stagiaire/editStagiaire', this.stagiaire)
          .then(() => {
            this.$router.push({ name: 'Stagiaires' })
          })
          .catch(() => {})
        NProgress.done()
      }
    },
    // Affiche le stage séléctionné du tableau des stages
    viewStage(item) {
      this.$router.push({
        name: 'Stage-Modifier',
        params: { id: item.stageId }
      })
    },
    formatDate: function(date) {
      let dateFormat = moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD')
      if (dateFormat == 'Invalid date') {
        return null
      } else {
        return dateFormat
      }
    }
  }
}
</script>
