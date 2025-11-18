<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un stagiaire..."
            placeholder="Nom, Prénom, Trigramme, Affiliation..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>
        </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="stagiaires || []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="stagiaires-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des stagiaires
            </v-toolbar-title>
          </v-toolbar>
        </template>
        
        <template v-slot:item.username="{ item }">
          <v-chip color="secondary lighten-5" text-color="secondary darken-4" class="font-weight-bold">
            {{ item.username }}
          </v-chip>
        </template>

        <template v-slot:item.nom="{ item }">
          <span class="font-weight-medium text-uppercase">
            {{ item.nom }}
          </span>
        </template>
        <template v-slot:item.prenom="{ item }">
          <span class="font-weight-medium text-capitalize">
            {{ item.prenom }}
          </span>
        </template>

        <template v-slot:item.typeAffiliation.libelle="{ item }">
          <v-chip small color="blue-grey lighten-5" text-color="blue-grey darken-3">
            {{ item.typeAffiliation.libelle }}
          </v-chip>
        </template>

        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucun stagiaire trouvé.
                </p>
            </div>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script>
import store from '@/store/index.js'
import { mapState } from 'vuex'

function getStagiaires(routeTo, next) {
   store.dispatch('stagiaire/fetchStagiaires', true).then(() => {
      next()
   })
}

function loadData(routeTo, routeFrom, next) {
   getStagiaires(routeTo, next)
}

export default {
   data: () => ({
      options: {},
      search: '',
      headers: [
         {
            text: 'Trigramme',
            value: 'username',
        class: 'font-weight-bold'
         },
         {
            text: 'Prénom', // Correction de la typo 'Prenom'
            value: 'prenom'
         },
         {
            text: 'Nom',
            value: 'nom'
         },
         { text: 'Affiliation', value: 'typeAffiliation.libelle' }
      ],
      stagiaires: []
   }),

   beforeRouteEnter(routeTo, routeFrom, next) {
      loadData(routeTo, routeFrom, next)
   },
   beforeRouteUpdate(routeTo, routeFrom, next) {
      loadData(routeTo, routeFrom, next)
   },
   beforeRouteLeave(routeTo, routeFrom, next) {
      // Sauvegarde le numéro de page consulté du tableau stagiaire avant de changer de page
      store
         .dispatch('settings/setCurrentPageStagiaire', {
            number: this.options.page
         })
         .then(() => {})
      next()
   },

   created() {
      // Récupère la dernier numéro de page consulté
      this.options.page = store.state.settings.currentPageStagiaire
   },

   computed: {
    // Note: Utilisation correcte des mapState pour les deux modules
      ...mapState('stagiaire', ['stagiaires']),
      ...mapState(['settings'])                         
   },

   methods: {
      // Lorsqu'une donnée est selectionnée dans le tableau, redirige vers le formulaire de modification
      selectRow(event) {
         this.$router.push({
            name: 'Stagiaire-Modifier',
            params: { id: event.stagiaireId }
         })
      },
      // Met à jour le nombre d'élément à afficher dans un tableau
      updateNumberItems(event) {
         store
            .dispatch('settings/setItemsPerPage', {
               number: event
            })
            .then(() => {})
      },
      // Quand une recherche est effectuée, partir de la première page
      updatePageSearch() {
         this.options.page = 1
      }
   }
}
</script>

<style scoped>
/* Styles pour un look moderne et cliquable */
.stagiaires-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.stagiaires-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
</style>