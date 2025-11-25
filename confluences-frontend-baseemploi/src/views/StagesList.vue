<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un stage..."
            placeholder="Nom, Stagiaire, Entreprise..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateStage />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(stage.stages) ? stage.stages : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="stage-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        :options.sync="options"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des stages
            </v-toolbar-title>
            <v-spacer></v-spacer>

            <FilterStage />
            
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

        <template v-slot:item.stagiaire.firstname="{ item }">
          <span class="font-weight-medium">
            {{ item.stagiaire.firstname + ' ' + item.stagiaire.lastName }}
          </span>
        </template>
        
        <template v-slot:item.year="{ item }">
          <v-chip small color="grey lighten-3" text-color="grey darken-3">
            {{ item.year }}
          </v-chip>
        </template>
        
        <template v-slot:item.debut="{ item }">
          <v-chip small color="green lighten-5" text-color="green darken-3">
            {{ formatDate(item.debut) }}
          </v-chip>
        </template>
        
        <template v-slot:item.fin="{ item }">
          <v-chip small color="orange lighten-5" text-color="orange darken-3">
            {{ formatDate(item.fin) }}
          </v-chip>
        </template>

        <template v-slot:item.typeIntershipActivity.nom="{ item }">
          <span class="d-inline-block text-truncate" style="max-width: 150px;">
            {{ item.typeIntershipActivity.nom }}
          </span>
        </template>

        <template v-slot:item.documents="{ item }">
          <v-btn
            small
            color="green"
            class="ml-2"
            @click.stop="downloadBilan(item.stageId)"
          >
            Bilan
          </v-btn>
        </template>


        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucun stage trouvé. Modifiez les filtres ou la recherche.
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
import CreateStage from '@/components/CreateStage.vue';
import FilterStage from '@/components/FilterStage.vue';
import moment from 'moment';
import API_BASE from "@/services/api";

const store = useStore();
const router = useRouter();

// --- État Réactif ---
const search = ref('');
const options = ref({}); 

// --- Computed Properties ---
const stage = computed(() => store.state.stage);
const settings = computed(() => store.state.settings);

// --- Données Statiques ---
const headers = [
    { text: 'Activité', value: 'typeIntershipActivity.nom', class: 'font-weight-bold' },
    { text: 'Métier', value: 'typeMetier.libelle' },
    { text: 'Entreprise', value: 'entreprise.nom' },
    { text: 'Stagiaire', value: 'stagiaire.firstname' },
    { text: 'Année', value: 'year' },
    { text: 'Début', value: 'debut' },
    { text: 'Fin', value: 'fin' },
    { text: 'Session', value: 'session.description' },
    { text: 'Type Stage', value: 'typeStage.nom' },
    { text: 'Annonce', value: 'typeAnnonce.libelle' },
    { text: 'Documents', value: 'documents', sortable: false }
];

const isFilterActive = computed(() => {
    const filter = store.state.stage.filter;
    return (
        filter.nom != null ||
        filter.typeMetierId != null ||
        filter.entrepriseId != null ||
        filter.stagiaireId != null ||
        filter.dateDebut != null ||
        filter.dateFin != null ||
        filter.typeStageId != null ||
        filter.typeAnnonceId != null ||
        filter.typeIntershipActivityId != null
    );
});

// --- Hooks ---

// FIX: Charger les données au montage pour résoudre le problème de rechargement après l'édition
onBeforeMount(() => {
    store.dispatch('stage/fetchStages', {});
});

// Hook de navigation de sortie (sauvegarde la page actuelle)
onBeforeRouteLeave((routeTo, routeFrom, next) => {
    store.dispatch('settings/setCurrentPageStage', { number: options.value.page });
    next();
});

// Initialisation (simule le 'created')
options.value.page = store.state.settings.currentPageStage;

// --- Méthodes ---
const selectRow = (event, row) => {
    const item = row ? row.item : event;
    const id = item.stageId || item.StageId;

    if (id) {
        router.push({ name: 'Stage-Modifier', params: { id: id } });
    } else {
        console.error("Erreur: ID de stage manquant lors de la sélection.");
    }
};

const updateNumberItems = (event) => {
    store.dispatch('settings/setItemsPerPage', { number: event });
};

const updatePageSearch = () => {
    options.value.page = 1;
};

const formatDate = (value) => {
    let date = moment(value).format('YYYY-MM-DD');
    return date === 'Invalid date' ? null : date;
};

const downloadBilan = async (id) => {
    const now = new Date().toISOString();
    const token = localStorage.getItem("token") || localStorage.getItem("access_token");

    const res = await fetch(`${API_BASE}/documents/bilan/${id}?date=${now}`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    if (res.ok) {
        const blob = await res.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = `bilan-${id}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
    } else {
        console.error("Échec du téléchargement du bilan:", res.status);
    }
};

const clearFilter = () => {
    const emptyFilter = {
        nom: null,
        typeMetierId: null,
        entrepriseId: null,
        stagiaireId: null,
        dateDebut: null,
        dateFin: null,
        typeStageId: null,
        typeAnnonceId: null,
        typeIntershipActivityId: null
    };
    store.dispatch('stage/saveFilterStage', emptyFilter).then(() => {
        store.dispatch('stage/fetchStages', {})
    });
};
</script>

<style scoped>
/* Styles généraux pour un look moderne et cliquable */
.stage-table >>> tbody tr:hover {
  cursor: pointer;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.stage-table >>> thead th {
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}

/* Style pour le texte "Effacer le filtre" */
.filter-clear-text .v-btn {
    text-decoration: underline;
    text-transform: none;
    min-width: unset;
}
</style>