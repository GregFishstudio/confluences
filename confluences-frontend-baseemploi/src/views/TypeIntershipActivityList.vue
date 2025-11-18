<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un type d'activité..."
            placeholder="Nom de l'activité..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateIntershipActivityFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(typeIntershipActivity.typeIntershipActivities) ? typeIntershipActivity.typeIntershipActivities : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="activity-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des types d'activités de stage
            </v-toolbar-title>
          </v-toolbar>
        </template>
        
        <template v-slot:item.nom="{ item }">
          <v-chip color="primary lighten-5" text-color="primary darken-3" class="font-weight-medium">
            {{ item.nom }}
          </v-chip>
        </template>

        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucun type d'activité de stage trouvé.
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
import CreateIntershipActivityFromList from '@/components/CreateIntershipActivityFromList.vue'

function getTypeIntershipActivities(routeTo, next) {
  store
    .dispatch('typeIntershipActivity/fetchTypeIntershipActivities', true)
    .then(() => {
      next()
    })
}

function loadData(routeTo, routeFrom, next) {
  getTypeIntershipActivities(routeTo, next)
}

export default {
  components: {
    CreateIntershipActivityFromList
  },

  data: () => ({
    options: {},
    search: '',
    headers: [
      {
        text: 'Nom de l\'activité',
        value: 'nom',
        class: 'font-weight-bold'
      }
    ],
    typeIntershipActivities: []
  }),

  // Hooks de navigation (inchangés)
  beforeRouteEnter(routeTo, routeFrom, next) {
    loadData(routeTo, routeFrom, next)
  },
  beforeRouteUpdate(routeTo, routeFrom, next) {
    loadData(routeTo, routeFrom, next)
  },
  beforeRouteLeave(routeTo, routeFrom, next) {
    // Sauvegarde le numéro de page consulté du tableau type d'activité avant de changer de page
    store
      .dispatch('settings/setCurrentPageTypeIntershipActivity', {
        number: this.options.page
      })
      .then(() => {})
    next()
  },

  created() {
    // Récupère la dernier numéro de page consulté
    this.options.page = store.state.settings.currentPageTypeIntershipActivity
  },

  computed: {
    ...mapState(['typeIntershipActivity', 'settings'])
  },

  methods: {
    selectRow(event) {
      this.$router.push({
        name: 'TypeIntershipActivity-Modifier',
        params: { id: event.typeIntershipActivityId }
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
.activity-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.activity-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
</style>