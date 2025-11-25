<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher une entreprise..."
            placeholder="Nom, Ville, Domaine..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateEntreprise />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(entreprise.entreprises) ? entreprise.entreprises : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="entreprise-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        :item-class="needsToBeRecalled"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des entreprises
            </v-toolbar-title>
            <v-spacer></v-spacer>

            <FilterEntreprise />
            
            <div v-if="isFilterActive" class="filter-clear-text ml-3">
              <v-btn
                text
                small
                color="red darken-1"
                @click="clearFilter"
                class="pa-0 font-weight-bold"
                title="Effacer les filtres actifs"
              >
                Effacer le filtre
              </v-btn>
            </div>
          </v-toolbar>
        </template>
        
        <template v-slot:item.dateDernierContact="{ item }">
          <v-chip 
            small 
            :color="formatDate(item.dateDernierContact) ? 'blue-grey lighten-5' : ''"
            :text-color="formatDate(item.dateDernierContact) ? 'blue-grey darken-3' : 'grey'"
          >
            {{ formatDate(item.dateDernierContact) || 'N/A' }}
          </v-chip>
        </template>
        
        <template v-slot:item.dateLastRecall="{ item }">
          <v-chip 
            small 
            :color="needsToBeRecalled(item) ? 'orange lighten-4' : (formatDate(item.dateLastRecall) ? 'green lighten-4' : 'grey lighten-3')"
            :text-color="needsToBeRecalled(item) ? 'orange darken-4' : (formatDate(item.dateLastRecall) ? 'green darken-4' : 'grey darken-1')"
          >
            {{ formatDate(item.dateLastRecall) || 'À définir' }}
          </v-chip>
        </template>
        
        <template v-slot:item.totalInterships="{ item }">
            <v-chip small color="primary lighten-5" text-color="primary darken-3" class="font-weight-bold">
                {{ item.totalInterships }}
            </v-chip>
        </template>
        
        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucune entreprise trouvée. Modifiez les filtres ou la recherche.
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
import CreateEntreprise from '@/components/CreateEntreprise.vue' 
import FilterEntreprise from '@/components/FilterEntreprise.vue'
import moment from 'moment'

// Fonctions de chargement des données (inchangées)
function getEntreprises(routeTo, next) {
  store.dispatch('entreprise/fetchEntreprises', true).then(() => {
    next()
  })
}

function getEntreprisesWithFilter(routeTo, next, filter) {
  store.dispatch('entreprise/saveFilterEntreprise', filter).then(() => {
    next()
  })
}

function loadData(routeTo, routeFrom, next) {
  const filter = store.state.entreprise.filter;
  if (
    filter.metiers.length > 0 ||
    filter.offres.length > 0 ||
    filter.domaines.length > 0 ||
    filter.codePostal != null ||
    filter.nom != null ||
    filter.createur != null ||
    filter.dateModification != null
  ) {
    getEntreprisesWithFilter(routeTo, next, filter)
  } else {
    getEntreprises(routeTo, next)
  }
}

export default {
  components: {
    CreateEntreprise,
    FilterEntreprise
  },

  data: () => ({
    options: {},
    search: '',
    headers: [
      {
        text: 'Nom',
        value: 'nom',
        class: 'font-weight-bold'
      },
      { text: 'Domaine', value: 'domaines' },
      { text: 'Ville', value: 'ville' },
      { text: 'Catégorie', value: 'typeEntreprise.nom' },
      { text: 'Dernier contact', value: 'dateDernierContact' },
      { text: 'Date de rappel', value: 'dateLastRecall' },
      { text: 'Total Stages', value: 'totalInterships', align: 'center' }
    ],
    entreprises: []
  }),

  // Hooks de navigation (inchangés)
  beforeRouteEnter(routeTo, routeFrom, next) {
    loadData(routeTo, routeFrom, next)
  },
  beforeRouteUpdate(routeTo, routeFrom, next) {
    loadData(routeTo, routeFrom, next)
  },
  beforeRouteLeave(routeTo, routeFrom, next) {
    store
      .dispatch('settings/setCurrentPageEntreprise', {
        number: this.options.page
      })
      .then(() => {})
    next()
  },

  created() {
    this.options.page = store.state.settings.currentPageEntreprise
  },

  computed: {
    ...mapState(['entreprise', 'settings']),
    isFilterActive() {
      const filter = this.entreprise.filter;
      return (
        filter.metiers.length > 0 ||
        filter.offres.length > 0 ||
        filter.domaines.length > 0 ||
        filter.codePostal != null ||
        filter.nom != null ||
        filter.createur != null ||
        filter.dateModification != null
      );
    }
  },

 methods: {
    selectRow(event, row) {
        let item;

        // --- Fix 1: Handle Vuetify 3 / Modern structure (event, { item }) ---
        if (row && row.item) {
            item = row.item;
        } 
        // --- Fix 2: Handle Vuetify 2 structure (item only) ---
        else if (event && event.nom) {
            item = event;
        } else {
            console.error("ERROR: Row object structure unexpected.", event);
            return;
        }

        // Find the ID, checking for casing differences (camelCase vs. PascalCase)
        const id = item.entrepriseId || item.EntrepriseId; 
        
        if (!id) {
            console.error("CRITICAL: Final ID not found in item.", item);
            return;
        }

        this.$router.push({
            name: 'Entreprise-Modifier',
            params: { id: id }
        });
    },
    updateNumberItems(event) {
      store
        .dispatch('settings/setItemsPerPage', {
          number: event
        })
        .then(() => {})
    },
    updatePageSearch() {
      this.options.page = 1
    },
    needsToBeRecalled: function(item) {
      const date = new Date(item.dateLastRecall)
      const twoMonthsAgo = new Date(
        new Date().getFullYear(),
        new Date().getMonth() - 2,
        new Date().getDate()
      )
      const today = new Date()

      if (isNaN(date)) return '';
      
      return date >= twoMonthsAgo && date <= today ? 'style-recall-alert' : ''
    },
    formatDate(value) {
      let date = moment(value).format('YYYY-MM-DD')
      if (date == 'Invalid date') {
        date = null
      }
      return date
    },
    clearFilter() {
      const emptyFilter = {
        metiers: [],
        offres: [],
        domaines: [],
        codePostal: null,
        nom: null,
        createur: null,
        dateModification: null
      };
      // Sauvegarde le filtre vide et recharge la liste
      store.dispatch('entreprise/saveFilterEntreprise', emptyFilter).then(() => {
        store.dispatch('entreprise/fetchEntreprises', true)
      })
    }
  }
}
</script>

<style scoped>
/* Le style du tableau reste inchangé pour garder l'aspect moderne */
.style-recall-alert {
  background-color: #fff3e0 !important;
  border-left: 4px solid #ff9800 !important;
}

.entreprise-table >>> tbody tr:hover {
  cursor: pointer;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.entreprise-table >>> thead th {
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}

/* Le texte "Effacer le filtre" utilise la classe v-btn text */
/* On s'assure qu'il ressemble plus à un lien qu'à un bouton */
.filter-clear-text .v-btn {
    text-decoration: underline;
    text-transform: none; /* Pas de majuscules */
    min-width: unset; /* Retirer la largeur minimale du bouton */
}
</style>