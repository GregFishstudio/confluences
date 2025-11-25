<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un taux d'occupation..."
            placeholder="Nom..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>
        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateTypeStageFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-data-table
      :headers="headers"
      :items="Array.isArray(typeStage.typeStages) ? typeStage.typeStages : []"
      :items-per-page="settings.itemsPerPage"
      :search="search"
      class="elevation-1"
      @click:row="selectRow"
      @update:items-per-page="updateNumberItems"
      :options.sync="options"
    >
      <template v-slot:item.nom="{ item }">
        <v-chip color="primary lighten-5" text-color="primary darken-3" class="font-weight-bold">
          {{ item.nom }}
        </v-chip>
      </template>
      
      <template v-slot:no-data>
        <div class="pa-4 text-center">
            <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
            <p class="text-subtitle-1 grey--text">
                Aucun taux d'occupation trouvé.
            </p>
        </div>
      </template>
    </v-data-table>
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue';
import { useStore } from 'vuex';
import { useRouter, onBeforeRouteLeave } from 'vue-router';
import CreateTypeStageFromList from '@/components/CreateTypeStageFromList.vue';

const store = useStore();
const router = useRouter();

// --- État Réactif ---
const search = ref('');
const options = ref({}); 

// --- Computed Properties ---
const typeStage = computed(() => store.state.typeStage);
const settings = computed(() => store.state.settings);

// --- Données Statiques ---
const headers = [
    { text: 'Nom', value: 'nom', class: 'font-weight-bold' }
];

// --- Hooks ---

// FIX: Charger les données au montage pour résoudre le problème de rechargement
onBeforeMount(() => {
    store.dispatch('typeStage/fetchTypeStages', true);
});

// Hook de navigation de sortie (sauvegarde la page actuelle)
onBeforeRouteLeave((routeTo, routeFrom, next) => {
    store.dispatch('settings/setCurrentPageTypeStage', { number: options.value.page });
    next();
});

// Initialisation (simule le 'created')
options.value.page = store.state.settings.currentPageTypeStage;

// --- Méthodes ---
const selectRow = (event, row) => {
    const item = row ? row.item : event;
    const id = item.typeStageId || item.TypeStageId;

    if (id) {
        router.push({ name: 'TypeStage-Modifier', params: { id: id } });
    } else {
        console.error("Erreur: ID de type de stage manquant lors de la sélection.");
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
/* Ajout de styles pour améliorer l'UI du tableau */
.v-data-table >>> tbody tr:hover {
  cursor: pointer;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}
</style>