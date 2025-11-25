<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un type d'aide..."
            placeholder="Description du type..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateTypeJobSearchAssistanceFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(typeJobSearchAssistance.typeSearchAssistances) ? typeJobSearchAssistance.typeSearchAssistances : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="type-job-search-assistance-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des types d'aides à la recherche d'emploi
            </v-toolbar-title>
          </v-toolbar>
        </template>
        
        <template v-slot:item.description="{ item }">
          <v-chip color="secondary lighten-5" text-color="secondary darken-4" class="font-weight-medium">
            {{ item.description }}
          </v-chip>
        </template>

        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucun type d'aide à la recherche d'emploi trouvé.
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
import CreateTypeJobSearchAssistanceFromList from '@/components/CreateTypeJobSearchAssistanceFromList.vue';

const store = useStore();
const router = useRouter();

// --- État Réactif ---
const search = ref('');
const options = ref({}); 

// --- Computed Properties ---
const typeJobSearchAssistance = computed(() => store.state.typeJobSearchAssistance);
const settings = computed(() => store.state.settings);

// --- Données Statiques ---
const headers = [
    { text: 'Description', value: 'description', class: 'font-weight-bold' }
];

// --- Hooks ---

// FIX: Charger les données au montage pour résoudre le problème de rechargement
onBeforeMount(() => {
    store.dispatch('typeJobSearchAssistance/fetchTypeJobSearchAssistances', true);
});

// Hook de navigation de sortie
onBeforeRouteLeave((routeTo, routeFrom, next) => {
    // Note: Le code de sauvegarde de la page est souvent déplacé ici
    // store.dispatch('settings/setCurrentPageTypeJobSearchAssistance', { number: options.value.page });
    next();
});

// Initialisation (simule le 'created')
// Initialisation simple car la clé de paramètre de page n'est pas claire dans les fichiers précédents
options.value.page = 1;

// --- Méthodes ---
const selectRow = (event, row) => {
    const item = row ? row.item : event;
    const id = item.typeJobSearchAssistanceId || item.TypeJobSearchAssistanceId;

    if (id) {
        router.push({ name: 'TypeJobSearchAssistance-Modifier', params: { id: id } });
    } else {
        console.error("Erreur: ID de type ARE manquant lors de la sélection.");
    }
};

const updateNumberItems = (event) => {
    store.dispatch('settings/setItemsPerPage', { number: event });
};

const updatePageSearch = () => {
    options.value.page = 1;
};
</script>

<style scoped>
/* Styles pour un look moderne et cliquable */
.type-job-search-assistance-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.type-job-search-assistance-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
</style>