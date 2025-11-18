<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un type d'entreprise..."
            placeholder="Nom du type..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateTypeEntrepriseFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(typeEntreprise.typeEntreprises) ? typeEntreprise.typeEntreprises : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="type-entreprise-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des types d'entreprise
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
                    Aucun type d'entreprise trouvé.
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
import CreateTypeEntrepriseFromList from '@/components/CreateTypeEntrepriseFromList.vue'

function getTypeEntreprises(routeTo, next) {
   store.dispatch('typeEntreprise/fetchTypeEntreprises', true).then(() => {
      next()
   })
}

function loadData(routeTo, routeFrom, next) {
   getTypeEntreprises(routeTo, next)
}

export default {
   components: {
      CreateTypeEntrepriseFromList
   },

   data: () => ({
      options: {},
      search: '',
      headers: [
         {
            text: 'Nom',
            value: 'nom',
        class: 'font-weight-bold'
         }
      ],
      typeEntreprises: []
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
         .dispatch('settings/setCurrentPageTypeEntreprise', {
            number: this.options.page
         })
         .then(() => {})
      next()
   },

   created() {
      this.options.page = store.state.settings.currentPageTypeEntreprise
   },

   computed: {
      ...mapState(['typeEntreprise', 'settings'])
   },

   methods: {
      selectRow(event) {
         this.$router.push({
            name: 'TypeEntreprise-Modifier',
            params: { id: event.typeEntrepriseId }
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
.type-entreprise-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.type-entreprise-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
</style>