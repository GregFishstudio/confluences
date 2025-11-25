<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher une aide à l'emploi..."
            placeholder="Ville, Type, Description, Mots-clés..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateJobSearchAssistanceFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(jobSearchAssistance.jobSearchAssistances) ? jobSearchAssistance.jobSearchAssistances : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="are-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des aides à la recherche d'emploi (ARE)
            </v-toolbar-title>
          </v-toolbar>
        </template>
        
        <template v-slot:item.date="{ item }">
          <v-chip small color="grey lighten-3" text-color="grey darken-3">
            {{ formatDate(item.date) }}
          </v-chip>
        </template>

        <template v-slot:item.typeJobSearchAssistance.description="{ item }">
          <v-chip small color="primary lighten-5" text-color="primary darken-3" class="font-weight-medium">
            {{ item.typeJobSearchAssistance.description }}
          </v-chip>
        </template>

        <template v-slot:item.typeJobSearchAssistanceOccurrence.description="{ item }">
          <v-chip x-small color="orange lighten-5" text-color="orange darken-4">
            {{ item.typeJobSearchAssistanceOccurrence.description }}
          </v-chip>
        </template>

        <template v-slot:item.website="{ item }">
          <a v-if="item.website" :href="item.website" target="_blank" class="text-decoration-none">
            <v-icon small color="blue" class="mr-1">mdi-web</v-icon>Lien
          </a>
          <span v-else class="grey--text">N/A</span>
        </template>

        <template v-slot:item.keyWords="{ item }">
          <div class="d-flex flex-wrap" style="max-width: 250px;">
            <v-chip
              v-for="(word, index) in item.keyWords.split(',').map(s => s.trim()).filter(s => s.length > 0)"
              :key="index"
              x-small
              class="ma-05"
              color="blue-grey lighten-5"
              text-color="blue-grey darken-2"
            >
              {{ word }}
            </v-chip>
          </div>
        </template>
        
        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucune aide à la recherche d'emploi trouvée.
                </p>
            </div>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue';
import { useStore } from 'vuex';
import { useRouter, onBeforeRouteLeave } from 'vue-router';
import CreateJobSearchAssistanceFromList from '@/components/CreateJobSearchAssistanceFromList.vue';
import moment from 'moment';

const store = useStore();
const router = useRouter();

// --- État Réactif ---
const search = ref('');
const options = ref({}); 

// --- Computed Properties ---
const jobSearchAssistance = computed(() => store.state.jobSearchAssistance);
const settings = computed(() => store.state.settings);

// --- Données Statiques ---
const headers = [
    { text: 'Adresse', value: 'address' },
    { text: 'Ville', value: 'town' },
    { text: 'Code postal', value: 'zipCode' },
    { text: 'Occurrence', value: 'typeJobSearchAssistanceOccurrence.description' },
    { text: 'Date', value: 'date' },
    { text: 'Description', value: 'description' },
    { text: 'Site internet', value: 'website' },
    { text: 'Mots clés', value: 'keyWords' },
    { text: 'Type', value: 'typeJobSearchAssistance.description', class: 'font-weight-bold' }
];

// --- Hooks ---

// FIX: Charger les données au montage pour résoudre le problème de rechargement
onBeforeMount(() => {
    store.dispatch('jobSearchAssistance/fetchJobSearchAssistances', true);
});

// Hook de navigation de sortie (sauvegarde la page actuelle)
onBeforeRouteLeave((routeTo, routeFrom, next) => {
    store.dispatch('settings/setCurrentPageJobSearchAssistance', { number: options.value.page });
    next();
});

// Initialisation (simule le 'created')
options.value.page = store.state.settings.currentPageJobSearchAssistance;

// --- Méthodes ---
const selectRow = (event, row) => {
    const item = row ? row.item : event;
    const id = item.jobSearchAssistanceId || item.JobSearchAssistanceId;

    if (id) {
        router.push({ name: 'JobSearchAssistance-Modifier', params: { id: id } });
    } else {
        console.error("Erreur: ID ARE manquant lors de la sélection.");
    }
};

const updateNumberItems = (event) => {
    store.dispatch('settings/setItemsPerPage', { number: event });
};

const updatePageSearch = () => {
    options.value.page = 1;
};

const formatDate = (date) => {
    return moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD');
};
</script>

<style scoped>
/* Styles pour un look moderne et cliquable */
.are-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.are-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
.ma-05 {
    margin: 2px !important;
}
</style>