<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher une occurrence..."
            placeholder="Description de l'occurrence..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateTypeJobSearchAssistanceOccurenceFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(typeJobSearchAssistanceOccurrence.typeSearchAssistanceOccurrences) ? typeJobSearchAssistanceOccurrence.typeSearchAssistanceOccurrences : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="occurrence-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des types d'occurrence ARE
            </v-toolbar-title>
          </v-toolbar>
        </template>
        
        <template v-slot:item.description="{ item }">
          <v-chip color="primary lighten-5" text-color="primary darken-3" class="font-weight-medium">
            {{ item.description }}
          </v-chip>
        </template>

        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucun type d'occurrence trouvé.
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
import CreateTypeJobSearchAssistanceOccurenceFromList from '@/components/CreateTypeJobSearchAssistanceOccurenceFromList.vue'

function getTypeJobSearchAssistances(routeTo, next) {
  store
    .dispatch(
      'typeJobSearchAssistanceOccurrence/fetchtypeSearchAssistanceOccurrences',
      true
    )
    .then(() => {
      next()
    })
}

function loadData(routeTo, routeFrom, next) {
  getTypeJobSearchAssistances(routeTo, next)
}

export default {
  components: {
    CreateTypeJobSearchAssistanceOccurenceFromList
  },

  data: () => ({
    options: {},
    search: '',
    headers: [
      {
        text: 'Description de l\'occurrence',
        value: 'description',
        class: 'font-weight-bold'
      }
    ]
  }),

  // Hooks de navigation (inchangés)
  beforeRouteEnter(routeTo, routeFrom, next) {
    loadData(routeTo, routeFrom, next)
  },
  beforeRouteUpdate(routeTo, routeFrom, next) {
    loadData(routeTo, routeFrom, next)
  },
  beforeRouteLeave(routeTo, routeFrom, next) {
    // Le code de sauvegarde de la page était commenté, je le laisse commenté.
    /*store
      .dispatch('settings/setCurrentPageTypeJobSearchAssistance', {
        number: this.options.page
      })
      .then(() => {})*/
    next()
  },

  created() {
    // Récupère la dernière page (ou 1 si non définie)
    // Je suppose que vous utilisez 'currentPageTypeJobSearchAssistanceOccurrence' ou le par défaut à 1
    // J'initialise à 1 comme dans votre `created()` d'origine.
    this.options.page = 1 
  },

  computed: {
    ...mapState(['typeJobSearchAssistanceOccurrence', 'settings'])
  },

  methods: {
    selectRow(event) {
      this.$router.push({
        name: 'TypeJobSearchAssistanceOccurrence-Modifier',
        params: { id: event.typeJobSearchAssistanceOccurrenceId }
      })
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
    }
  }
}
</script>

<style scoped>
/* Styles pour un look moderne et cliquable */
.occurrence-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.occurrence-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
</style>