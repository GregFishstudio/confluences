<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher une affiliation..."
            placeholder="Nom, Code..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateAffiliationFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(typeAffiliation.typeAffiliations) ? typeAffiliation.typeAffiliations : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="affiliation-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des types d'affiliations
            </v-toolbar-title>
          </v-toolbar>
        </template>
        
        <template v-slot:item.code="{ item }">
          <v-chip small color="secondary lighten-5" text-color="secondary darken-4" class="font-weight-bold">
            {{ item.code }}
          </v-chip>
        </template>
        
        <template v-slot:item.libelle="{ item }">
          <span class="font-weight-medium">
            {{ item.libelle }}
          </span>
        </template>

        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucun type d'affiliation trouvé.
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
import CreateAffiliationFromList from '@/components/CreateAffiliationFromList.vue'

function getTypeAffiliations(routeTo, next) {
  store.dispatch('typeAffiliation/fetchTypeAffiliations', true).then(() => {
    next()
  })
}

function loadData(routeTo, routeFrom, next) {
  getTypeAffiliations(routeTo, next)
}

export default {
  components: {
    CreateAffiliationFromList
  },

  data: () => ({
    options: {},
    search: '',
    headers: [
      {
        text: 'Code',
        value: 'code',
        class: 'font-weight-bold'
      },
      {
        text: 'Nom',
        value: 'libelle',
        class: 'font-weight-medium'
      }
    ],
    typeAffiliations: []
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
      .dispatch('settings/setCurrentPageTypeAffiliation', {
        number: this.options.page
      })
      .then(() => {})
    next()
  },

  created() {
    this.options.page = store.state.settings.currentPageTypeAffiliation
  },

  computed: {
    ...mapState(['typeAffiliation', 'settings'])
  },

  methods: {
    selectRow(event) {
      this.$router.push({
        name: 'TypeAffiliation-Modifier',
        params: { id: event.typeAffiliationId }
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
.affiliation-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.affiliation-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
</style>