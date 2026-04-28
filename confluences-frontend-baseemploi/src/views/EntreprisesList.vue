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
          <CreateEntreprise @created="fetchData" />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
  :headers="headers"
  :items="entreprisesList"
  item-key="entrepriseId"
  :items-per-page="settings.itemsPerPage"
  :search="search"
  :loading="loading"
  loading-text="Chargement des entreprises en cours..."
  class="entreprise-table"
  @click:row="logClick"
  @update:items-per-page="updateNumberItems"
  :options.sync="options"
  :item-class="getRowClass"
  mobile-breakpoint="0"
>
        <template v-slot:top>
          <v-toolbar flat>
          
            <v-spacer></v-spacer>

            <FilterEntreprise />
            
            <div v-if="isFilterActive" class="filter-clear-text ml-3">
              <v-btn
                text
                small
                color="red darken-1"
                @click="clearFilter"
                class="pa-0 font-weight-bold"
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
            :color="isRecallAlert(item.dateLastRecall) ? 'orange lighten-4' : (formatDate(item.dateLastRecall) ? 'green lighten-4' : 'grey lighten-3')"
            :text-color="isRecallAlert(item.dateLastRecall) ? 'orange darken-4' : (formatDate(item.dateLastRecall) ? 'green darken-4' : 'grey darken-1')"
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
                    Aucune entreprise trouvée.
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

export default {
  components: {
    CreateEntreprise,
    FilterEntreprise
  },

  data: () => ({
    loading: false,
    options: {},
    search: '',
    headers: [
      { text: 'Nom', value: 'nom', class: 'font-weight-bold' },
      { text: 'Domaine', value: 'domaines' },
      { text: 'Ville', value: 'ville' },
      { text: 'Catégorie', value: 'typeEntreprise.nom' },
      { text: 'Dernier contact', value: 'dateDernierContact' },
      { text: 'Date de rappel', value: 'dateLastRecall' },
      { text: 'Total Stages', value: 'totalInterships', align: 'center' }
    ]
  }),

  computed: {
    ...mapState(['entreprise', 'settings']),
    
    entreprisesList() {
      return Array.isArray(this.entreprise.entreprises) ? this.entreprise.entreprises : [];
    },

    isFilterActive() {
      const f = this.entreprise.filter;
      return !!(f.metiers.length || f.offres.length || f.domaines.length || 
             f.codePostal || f.nom || f.createur || f.dateModification);
    },

    // Optimisation : Calculé une seule fois pour toutes les lignes
    recallThreshold() {
      const now = new Date();
      return new Date(now.getFullYear(), now.getMonth() - 2, now.getDate());
    }
  },

  async created() {
    this.options.page = this.settings.currentPageEntreprise || 1;
    await this.fetchData();
  },

  beforeRouteLeave(routeTo, routeFrom, next) {
    store.dispatch('settings/setCurrentPageEntreprise', { number: this.options.page });
    next();
  },

  methods: {
    async fetchData() {
      this.loading = true;
      try {
        const filter = this.entreprise.filter;
        if (this.isFilterActive) {
          await store.dispatch('entreprise/saveFilterEntreprise', filter);
        } else {
          await store.dispatch('entreprise/fetchEntreprises', true);
        }
      } finally {
        this.loading = false;
      }
    },

    isRecallAlert(dateString) {
      if (!dateString) return false;
      const date = new Date(dateString);
      return !isNaN(date) && date >= this.recallThreshold && date <= new Date();
    },

    getRowClass(item) {
      return this.isRecallAlert(item.dateLastRecall) ? 'style-recall-alert' : '';
    },

    logClick(event, row) {
  // row.item contient ton objet entreprise
  if (row && row.item) {
    this.selectRow(row.item);
  } else {
    console.warn("Aucun item trouvé dans le clic :", row);
  }
},

selectRow(item) {
  if (!item) {
    console.warn("Aucun item reçu :", item);
    return;
  }

  const id = item.entrepriseId || item.id || item.EntrepriseId;

  console.log("Navigation vers entreprise :", id, item);

  if (!id) {
    console.error("Aucun ID trouvé dans :", item);
    return;
  }

  this.$router.push({
    name: 'Entreprise-Modifier',
    params: {
      id: id,
      entreprise: item
    }
  });
},
    updateNumberItems(val) {
      store.dispatch('settings/setItemsPerPage', { number: val });
    },

    updatePageSearch() {
      this.options.page = 1;
    },

    formatDate(value) {
      if (!value) return null;
      const date = moment(value);
      return date.isValid() ? date.format('YYYY-MM-DD') : null;
    },

    async clearFilter() {
      const emptyFilter = {
        metiers: [], offres: [], domaines: [],
        codePostal: null, nom: null, createur: null, dateModification: null
      };
      this.loading = true;
      await store.dispatch('entreprise/saveFilterEntreprise', emptyFilter);
      await store.dispatch('entreprise/fetchEntreprises', true);
      this.loading = false;
    }
  }
}
</script>

<style scoped>
.style-recall-alert {
  background-color: #fff3e0 !important;
  border-left: 4px solid #ff9800 !important;
}

.entreprise-table >>> tbody tr:hover {
  cursor: pointer;
  background-color: #f5f5f5 !important;
  transition: background-color 0.2s ease;
}

.filter-clear-text .v-btn {
    text-decoration: underline;
    text-transform: none;
    min-width: unset;
}
</style>