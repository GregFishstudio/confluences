<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Formulaire de modification d'une entreprise
  -- Fichier : EntrepriseEdit.vue
  -->

<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Entreprise</h1>
      </v-col>
    </v-row>
    <v-form ref="formEntreprise" v-model="valid" lazy-validation>
      <v-row>
        <v-col cols="12" md="3">
          <v-row>
            <v-col>
              <v-autocomplete
                v-model="entreprise.typeEntrepriseId"
                :items="typeEntreprise.typeEntreprises"
                item-value="typeEntrepriseId"
                item-text="nom"
                label="Catégorie"
                clearable
              ></v-autocomplete>
            </v-col>
          </v-row>
        </v-col>
        <v-col cols="12" md="9">
          <v-row>
            <v-col cols="12" md="3">
              <v-menu
                ref="menuContact"
                v-model="menuContact"
                :close-on-content-click="true"
                :return-value.sync="date"
                transition="scale-transition"
                offset-y
                min-width="290px"
              >
                <template v-slot:activator="{ on, attrs }">
                  <v-text-field
                    v-model="entreprise.dateDernierContact"
                    label="Date du dernier contact"
                    readonly
                    v-bind="attrs"
                    v-on="on"
                  ></v-text-field>
                </template>
                <v-date-picker
                  v-model="entreprise.dateDernierContact"
                  no-title
                  scrollable
                >
                </v-date-picker>
              </v-menu>
              <v-menu
                ref="menuContactRecall"
                v-model="menuContactRecall"
                :close-on-content-click="true"
                :return-value.sync="date"
                transition="scale-transition"
                offset-y
                min-width="290px"
              >
                <template v-slot:activator="{ on, attrs }">
                  <v-text-field
                    v-model="entreprise.dateLastRecall"
                    label="Date de rappel"
                    readonly
                    v-bind="attrs"
                    v-on="on"
                    clearable
                  ></v-text-field>
                </template>
                <v-date-picker
                  v-model="entreprise.dateLastRecall"
                  no-title
                  scrollable
                >
                </v-date-picker>
              </v-menu>
            </v-col>
            <v-col cols="12" md="3">
              <v-autocomplete
                v-model="entreprise.formateurIdDernierContact"
                :items="user.users"
                item-value="id"
                item-text="nom"
                label="Par"
                clearable
              ></v-autocomplete>
            </v-col>
            <v-col cols="12" md="3">
              <v-autocomplete
                v-model="entreprise.stagiaireIdDernierContact"
                :items="user.users"
                item-value="id"
                item-text="nom"
                label="Pour"
                clearable
              ></v-autocomplete>
            </v-col>
            <v-col cols="12" md="3">
              <v-autocomplete
                v-model="entreprise.typeMoyenId"
                :items="typeMoyen.typeMoyens"
                item-value="typeMoyenId"
                item-text="libelle"
                label="Moyen de communication"
                clearable
              ></v-autocomplete>
            </v-col>
          </v-row>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="4">
          <v-text-field
            v-model="entreprise.nom"
            :counter="50"
            :rules="nameRules"
            label="Nom"
            required
          ></v-text-field>
          <v-text-field
            v-model="entreprise.adr1"
            :counter="50"
            :rules="adressRules"
            label="Adresse"
          ></v-text-field>
          <v-text-field
            v-model="entreprise.adr2"
            :counter="50"
            :rules="adressRules"
            label="Complément d'adresse"
          ></v-text-field>
          <v-text-field
            v-model="entreprise.ville"
            :counter="50"
            :rules="nameRules"
            label="Ville"
            required
          ></v-text-field>
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="entreprise.codePostal"
                :counter="4"
                :rules="[codePostalRules]"
                label="Code postal"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
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
                    v-model="entreprise.dateCreation"
                    label="Chez Confluences depuis"
                    readonly
                    v-bind="attrs"
                    v-on="on"
                  ></v-text-field>
                </template>
                <v-date-picker
                  v-model="entreprise.dateCreation"
                  no-title
                  scrollable
                >
                </v-date-picker>
              </v-menu>
            </v-col>
          </v-row>
          <v-text-field
            v-model="entreprise.email"
            :counter="50"
            :rules="emailRules"
            label="Email"
          ></v-text-field>
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="entreprise.telFix"
                :counter="13"
                :rules="phonesRules"
                label="Téléphone fixe"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="entreprise.telNatel"
                :counter="13"
                :rules="phonesRules"
                label="Natel"
              ></v-text-field>
            </v-col>
          </v-row>
        </v-col>
        <v-col cols="12" md="8">
          <EntrepriseDomaineList :entreprise="entreprise" />
          <v-row>
            <v-col cols="12" md="6">
              <EntrepriseMetierList :entreprise="entreprise" />
            </v-col>
            <v-col cols="12" md="6">
              <EntrepriseOffreList :entreprise="entreprise" />
            </v-col>
          </v-row>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-textarea
            v-model="entreprise.remarque"
            outlined
            label="Remarques"
            counter
            maxlength="10000"
            auto-grow
          ></v-textarea>
        </v-col>
      </v-row>
      <v-row style="margin-bottom:100px">
        <v-col cols="12" md="12">
          <EntrepriseContactList :entreprise="entreprise" />
        </v-col>
        <v-col cols="12" md="12">
          <v-card class="mx-auto px-0" outlined>
            <v-list>
              <v-subheader>Stages</v-subheader>
              <EntrepriseStageList :entreprise="entreprise" />
            </v-list>
          </v-card>
        </v-col>
        <v-col cols="12" md="12">
          <EntrepriseLastContactList :entreprise="entreprise" />
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
              <DeleteEntreprise :entreprise="this.entreprise" />
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
import EntrepriseOffreList from '@/components/EntrepriseOffreList.vue'
import EntrepriseDomaineList from '@/components/EntrepriseDomaineList.vue'
import EntrepriseMetierList from '@/components/EntrepriseMetierList.vue'
import EntrepriseStageList from '@/components/EntrepriseStageList.vue'
import EntrepriseContactList from '@/components/EntrepriseContactList.vue'
import EntrepriseLastContactList from '@/components/EntrepriseLastContactList.vue'
import DeleteEntreprise from '@/components/DeleteEntreprise.vue'
import moment from 'moment'

function getTypeEntreprises() {
  store.dispatch('typeEntreprise/fetchTypeEntreprises', {}).then(() => {})
}

function getTypeDomaines() {
  store.dispatch('typeDomaine/fetchTypeDomaines', {}).then(() => {})
}

function getTypeMoyens() {
  store.dispatch('typeMoyen/fetchTypeMoyens', {}).then(() => {})
}

function getUsers() {
  store.dispatch('user/fetchUsers', {}).then(() => {})
}

export default {
  props: {
    entreprise: {
      type: Object,
      required: true
    }
  },

  components: {
    EntrepriseOffreList,
    EntrepriseMetierList,
    EntrepriseDomaineList,
    EntrepriseStageList,
    EntrepriseContactList,
    EntrepriseLastContactList,
    DeleteEntreprise
  },

  data: () => ({
    date: null,
    menuContact: false,
    menuCreation: false,
    menuContactRecall: false,
    valid: true,
    nameRules: [
      v => !!v || 'Le nom est obligatoire',
      v => (v && v.length <= 50) || 'Le nom doit être moins que 50 caractères'
    ],
    adressRules: [
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ],
    emailRules: [
      v => !v || /.+@.+\..+/.test(v) || "L'email doit être valide",
      v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
    ],
    codePostalRules: v => {
      if (v >= 0 && v <= 9999) return true
      return 'En Suisse, 4 chiffres'
    },
    phonesRules: [
      v => !v || v.length <= 13 || 'Le champ doit être moins que 13 caractères'
    ],
    requiredRule: [v => !!v || 'Obligatoire'],
    select: null
  }),

  // Charger les différents possibilités de choix avant la création du composant
  beforeCreate(routeTo, routeFrom, next) {
    getTypeEntreprises(routeTo, next)
    getTypeDomaines(routeTo, next)
    getTypeMoyens(routeTo, next)
    getUsers(routeTo, next)
  },

  created() {
    // Formattage des dates afin qu'elles s'affichent correctement
    this.entreprise.dateDernierContact = this.formatDate(
      this.entreprise.dateDernierContact
    )
    this.entreprise.dateCreation = this.formatDate(this.entreprise.dateCreation)
    this.entreprise.dateLastRecall = this.formatDate(
      this.entreprise.dateLastRecall
    )
    this.entreprise.dateLastRecall = this.formatDate(
      this.entreprise.dateLastRecall
    )
  },

  computed: {
    ...mapState(['typeEntreprise', 'typeDomaine', 'typeMoyen', 'user'])
  },

  methods: {
    // Si le formulaire est valide, sauvegarde de l'entreprise
    submit() {
      if (this.$refs.formEntreprise.validate()) {
        NProgress.start()
        store
          .dispatch('entreprise/editEntreprise', this.entreprise)
          .then(() => {
            this.$router.push({ name: 'Entreprises' })
          })
          .catch(() => {})
        NProgress.done()
      }
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
