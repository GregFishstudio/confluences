<template>
  <v-container fluid class="pa-4">
    
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" md="5">
          <h2 class="headline primary--text">Ateliers & Accompagnement</h2>
          <p class="caption grey--text mb-0">Vue globale par trimestre</p>
        </v-col>
        
        <v-col cols="12" md="7" class="d-flex justify-end align-center">
           <v-btn icon @click="changePeriod(-1)" :disabled="isLoading">
             <v-icon>mdi-chevron-left</v-icon>
           </v-btn>
           
           <div class="text-center mx-3" style="min-width: 150px;">
             <div class="font-weight-bold">{{ currentPeriodLabel }}</div>
             <div class="caption grey--text">
               {{ momentHelper(currentSessionDates.start).format('DD MMM') }} - 
               {{ momentHelper(currentSessionDates.end).format('DD MMM YYYY') }}
             </div>
           </div>

           <v-btn icon @click="changePeriod(1)" :disabled="isLoading">
             <v-icon>mdi-chevron-right</v-icon>
           </v-btn>

          
        </v-col>
      </v-row>
    </v-card>

    <v-card class="elevation-4" rounded="lg" :loading="isLoading">
      <v-card-text class="pa-0">
        <div class="table-container">
          <table class="matrix-table">
            <thead>
              <tr>
                <th class="sticky-col name-col z-index-high">Stagiaire</th>
                <th v-for="col in allColumns" :key="col.key" class="activity-header" :class="col.type">
                  {{ col.label }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="st in safeStagiaires" :key="st.stagiaireId">
                <td class="sticky-col name-col">
                  <div class="font-weight-bold">{{ st.prenom }} {{ st.nom }}</div>
                </td>

                <td 
                  v-for="col in allColumns" 
                  :key="col.key" 
                  class="interactive-cell"
                  @click="openDateEditor(st, col)"
                  :class="{ 'has-data': getDatesCount(st.stagiaireId, col.key) > 0 }"
                >
                  <div class="cell-content">
                    <span v-if="getDatesCount(st.stagiaireId, col.key) > 0" class="date-badge">
                      {{ getDatesCount(st.stagiaireId, col.key) }} date(s)
                    </span>
                    <span v-else class="add-icon"><v-icon x-small>mdi-plus</v-icon></span>
                  </div>
                </td>
              </tr>
              
              <tr v-if="safeStagiaires.length === 0">
                <td :colspan="allColumns.length + 1" class="text-center pa-4">
                  Aucun stagiaire trouvé ou chargement en cours...
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </v-card-text>
    </v-card>

    <v-dialog v-model="editorDialog" max-width="350px">
      <v-card v-if="editingCell">
        <v-card-title class="primary white--text subtitle-2">
          {{ editingCell.column.label }} - {{ editingCell.stagiaire.prenom }}
        </v-card-title>
        <v-card-text class="pa-0">
          <v-date-picker
            v-model="datesPickerBuffer"
            multiple
            no-title
            scrollable
            locale="fr"
            first-day-of-week="1"
            width="100%"
            color="primary"
          ></v-date-picker>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn text @click="editorDialog = false">Annuler</v-btn>
          <v-btn color="primary" @click="saveCellChanges" :loading="isSaving">Enregistrer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useStore } from 'vuex';
import moment from "moment";
import API_BASE from "@/services/api";

const momentHelper = moment;
const store = useStore();

// --- Colonnes ---
const accompanyingCols = [
  { key: 'Admin', label: 'Admin', type: 'acco' },
  { key: 'Français', label: 'Français', type: 'acco' },
  { key: 'Logement', label: 'Logement', type: 'acco' },
  { key: 'Insertion', label: 'Insertion', type: 'acco' },
  { key: 'Projets', label: 'Projets', type: 'acco' },
  { key: 'Informat.', label: 'Info', type: 'acco' },
];
const workshopCols = [
  { key: 'Atelier_Fr', label: 'Atelier FR', type: 'atelier' },
  { key: 'Cuisine', label: 'Cuisine', type: 'atelier' },
  { key: 'Théorie', label: 'Théorie Auto', type: 'atelier' },
  { key: 'Jardinage', label: 'Jardinage', type: 'atelier' },
  { key: 'Sortie', label: 'Sortie Cult.', type: 'atelier' },
];
const allColumns = [...accompanyingCols, ...workshopCols];

// --- Données ---
const isLoading = ref(false);
const isSaving = ref(false);
const editorDialog = ref(false);
const editingCell = ref(null);
const datesPickerBuffer = ref([]);
const presencesMatrix = ref({}); // Structure : { [stagiaireId]: { [ActivityKey]: ['2025-01-01'] } }

// --- Temps ---
const currentYear = ref(moment().year());
const currentQuarter = ref(Math.floor((moment().month() / 3)) + 1);

// --- Computed Stagiaires (Sécurisé) ---
const safeStagiaires = computed(() => {
    // Tente de récupérer la liste, renvoie tableau vide si échec pour éviter crash
    return store.state.stagiaire?.stagiaires || [];
});

const currentSessionDates = computed(() => {
    const startMonth = (currentQuarter.value - 1) * 3;
    const start = moment([currentYear.value, startMonth, 1]);
    const end = start.clone().add(2, 'months').endOf('month');
    return { start: start.toDate(), end: end.toDate() };
});

const currentPeriodLabel = computed(() => `T${currentQuarter.value} ${currentYear.value}`);

// --- Méthodes ---

const getDatesCount = (stId, key) => {
    if (!presencesMatrix.value[stId]) return 0;
    return (presencesMatrix.value[stId][key] || []).length;
};

const changePeriod = (delta) => {
    currentQuarter.value += delta;
    if (currentQuarter.value > 4) { currentQuarter.value = 1; currentYear.value++; }
    else if (currentQuarter.value < 1) { currentQuarter.value = 4; currentYear.value--; }
    loadMatrix();
};

const openDateEditor = (st, col) => {
    editingCell.value = { stagiaire: st, column: col };
    const dates = presencesMatrix.value[st.stagiaireId]?.[col.key] || [];
    datesPickerBuffer.value = [...dates];
    editorDialog.value = true;
};

// --- API ---

const loadMatrix = async () => {
    isLoading.value = true;
    const start = moment(currentSessionDates.value.start).format('YYYY-MM-DD');
    const end = moment(currentSessionDates.value.end).format('YYYY-MM-DD');
    const token = localStorage.getItem("access_token");

    try {
        const url = `${API_BASE}/api/accompanying/matrix?start=${start}&end=${end}`;
        const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` }});
        
        if (res.ok) {
            const data = await res.json();
            // Transformer le tableau plat en objet indexé
            const matrix = {};
            data.forEach(item => {
                if (!matrix[item.stagiaireId]) matrix[item.stagiaireId] = {};
                matrix[item.stagiaireId][item.activityKey] = item.dates;
            });
            presencesMatrix.value = matrix;
        }
    } catch (e) {
        console.error("Erreur Matrix", e);
    } finally {
        isLoading.value = false;
    }
};

const saveCellChanges = async () => {
    if (!editingCell.value) return;
    isSaving.value = true;
    const { stagiaire, column } = editingCell.value;
    const newDates = datesPickerBuffer.value;

    const token = localStorage.getItem("access_token");
    try {
        const res = await fetch(`${API_BASE}/api/accompanying/sync-dates`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
            body: JSON.stringify({
                stagiaireId: stagiaire.stagiaireId,
                activityKey: column.key,
                dates: newDates // Array de dates YYYY-MM-DD
            })
        });

        if (res.ok) {
            // Mise à jour locale
            if (!presencesMatrix.value[stagiaire.stagiaireId]) presencesMatrix.value[stagiaire.stagiaireId] = {};
            presencesMatrix.value[stagiaire.stagiaireId][column.key] = newDates;
            editorDialog.value = false;
        }
    } catch (e) {
        alert("Erreur sauvegarde");
    } finally {
        isSaving.value = false;
    }
};

onMounted(() => {
    // 1. Charger les stagiaires
    store.dispatch("stagiaire/fetchStagiaires", true).then(() => {
        // 2. Charger la matrice une fois les stagiaires là
        loadMatrix();
    });
});
</script>

<style scoped>
.table-container { overflow-x: auto; max-height: 75vh; }
.matrix-table { width: 100%; border-collapse: separate; border-spacing: 0; font-size: 0.85rem; }
.matrix-table th { position: sticky; top: 0; z-index: 4; padding: 10px; border-bottom: 2px solid #ddd; }
.activity-header.acco { background: #E1F5FE; color: #01579B; }
.activity-header.atelier { background: #E8F5E9; color: #1B5E20; }
.matrix-table td { border: 1px solid #eee; padding: 0; height: 50px; text-align: center; }
.sticky-col { position: sticky; left: 0; background: white; z-index: 3; padding: 8px; text-align: left; min-width: 180px; box-shadow: 2px 0 5px rgba(0,0,0,0.05); }
.interactive-cell { cursor: pointer; transition: 0.2s; }
.interactive-cell:hover { background: #f5f5f5; }
.interactive-cell.has-data { background: #E3F2FD; }
.date-badge { font-weight: bold; color: #1976D2; font-size: 0.8rem; }
.add-icon { opacity: 0.2; }
.interactive-cell:hover .add-icon { opacity: 1; }
</style>