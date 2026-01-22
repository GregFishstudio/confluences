<template>
  <v-container fluid class="pa-4 follow-up-sheet">

    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        
        <v-col cols="12" md="3">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un stagiaire..."
            placeholder="Nom, Prénom, Trigramme..."
            outlined dense clearable hide-details
            background-color="white"
          />
        </v-col>

        <v-col cols="12" md="3">
          <v-select
            v-model="filterMesure"
            :items="mesures"
            label="Filtrer par Mesure"
            outlined dense clearable hide-details
            placeholder="Toutes les mesures"
          />
        </v-col>

        <v-col cols="12" md="6" class="d-flex align-center justify-end">
          <v-btn icon @click="previousQuarter" :disabled="isLoading">
            <v-icon>mdi-chevron-left</v-icon>
          </v-btn>

          <div class="text-center mx-4">
            <div class="text-h6 font-weight-bold primary--text">
              {{ currentPeriodLabel }}
            </div>
            <div class="text-caption grey--text">
              {{ momentHelper(currentSessionDates.start).format('DD MMM') }} - 
              {{ momentHelper(currentSessionDates.end).format('DD MMM YYYY') }}
            </div>
          </div>

          <v-btn icon @click="nextQuarter" :disabled="isCurrentQuarter || isLoading">
            <v-icon>mdi-chevron-right</v-icon>
          </v-btn>
        </v-col>

      </v-row>
    </v-card>

    <v-card class="elevation-4" rounded="lg" :loading="isLoading">
      
      <v-card-title class="pa-4">
        <span class="text-h5 font-weight-bold">Synthèse Trimestrielle</span>
        <v-spacer></v-spacer>
        <v-btn color="primary" outlined @click="$router.push({ name: 'AteliersFollowUp' })">
            <v-icon left>mdi-view-grid</v-icon>
            Gérer les Ateliers
        </v-btn>
      </v-card-title>

      <v-card-text class="pa-0">
        <div class="table-container">
          <table class="follow-up-table">

            <thead>
              <tr>
                <th class="sticky-col name-col">Stagiaire</th>

                <th class="saisie-col">Niveau Oral</th>
                <th class="saisie-col">Niveau Écrit</th>

                <th
                  v-for="activity in activities"
                  :key="activity.id"
                  class="activity-col"
                  :style="{ minWidth: activity.type === 'details' ? '300px' : '140px' }"
                >
                  {{ activity.libelle }}
                  <v-tooltip bottom v-if="activity.description">
                    <template #activator="{ on, attrs }">
                      <v-icon small class="ml-1" v-bind="attrs" v-on="on">mdi-information</v-icon>
                    </template>
                    <span>{{ activity.description }}</span>
                  </v-tooltip>
                </th>

                <th class="synthesis-col">Présence trimestre</th>
                <th class="sticky-col actions-col">Bilan</th>
                <th class="sticky-col actions-col">PDFs</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="st in filteredStagiaires" :key="st.stagiaireId">
                
                <td class="sticky-col name-col">
                  <div class="stagiaire-info">
                    <div class="name">{{ st.prenom }} {{ st.nom }}</div>
                    <div class="trigramme">{{ st.username }}</div>
                    <div class="affiliation text-caption">
                      {{ st.typeAffiliation?.libelle || 'N/A' }}
                    </div>
                    <div v-if="st.dateDebutParticipation" class="text-caption grey--text">
                      Début: {{ momentHelper(st.dateDebutParticipation).format('DD/MM/YYYY') }}
                    </div>
                  </div>
                </td>

                <td class="saisie-col">
                  <v-select
                    :key="`${st.stagiaireId}-oral-${currentPeriod.key}`"
                    :value="st.oralLevel || ''"
                    :items="levels"
                    label="Oral"
                    dense outlined single-line hide-details
                    @update:modelValue="(newVal) => saveLevel(st.stagiaireId, 'oralLevel', newVal)"
                    style="width: 90px; margin: 0 auto;"
                    placeholder="-"
                  />
                </td>
                
                <td class="saisie-col">
                  <v-select
                    :key="`${st.stagiaireId}-written-${currentPeriod.key}`"
                    :value="st.writtenLevel || ''"
                    :items="levels"
                    label="Écrit"
                    dense outlined single-line hide-details
                    @update:modelValue="(newVal) => saveLevel(st.stagiaireId, 'writtenLevel', newVal)"
                    style="width: 90px; margin: 0 auto;"
                    placeholder="-"
                  />
                </td>

                <td v-for="activity in activities" :key="activity.key" class="saisie-col">
                  
                  <div class="d-flex justify-center" style="flex-direction: column; align-items: center;">

                    <div v-if="activity.type === 'details'" class="text-left pa-2" style="max-width: 95%;">
                      
                      <div v-if="st.courseDetails.isUnknown">
                        <div class="text-caption error--text font-weight-bold mb-1">
                          <v-icon x-small color="error">mdi-alert-circle</v-icon> Aucune formation
                        </div>
                        <a 
                          href="https://cours.confluences.ch" 
                          target="_blank" 
                          class="text-caption text-decoration-underline primary--text"
                          style="cursor: pointer;"
                        >
                          Ajouter une formation sur cours.confluences.ch
                          <v-icon x-small color="primary">mdi-open-in-new</v-icon>
                        </a>
                      </div>

                      <div v-else>
                        <div class="text-caption font-weight-medium primary--text">
                          {{ st.courseDetails.mainInfo }}
                        </div>
                        <ul v-if="st.courseDetails.subCourses.length" class="text-caption ml-n6 mt-1">
                          <li v-for="course in st.courseDetails.subCourses" :key="course" style="list-style-type: disc;">
                            {{ course }}
                          </li>
                        </ul>
                      </div>
                    </div>

                    <div v-else-if="activity.type === 'rate'" class="d-flex flex-column align-center">
  <v-text-field
    :value="st.followUp.sessionAttendanceRate.rate.toFixed(1) + '%'"
    dense outlined single-line hide-details readonly
    class="centered-input"
    style="width: 80px;"
    :class="st.followUp.sessionAttendanceRate.hasData ? getRateClass(st.followUp.sessionAttendanceRate.rate) : 'grey--text text--darken-1'"
    :background-color="st.followUp.sessionAttendanceRate.hasData && st.followUp.sessionAttendanceRate.rate < 100 ? '#fff0f0' : undefined"
  />
  
  <div v-if="st.followUp.sessionAttendanceRate.totalAbsencePenalty > 0" class="text-caption error--text font-weight-bold" style="line-height: 1.2;">
    <v-icon x-small color="red">mdi-clock-alert</v-icon> -{{ st.followUp.sessionAttendanceRate.totalAbsencePenalty.toFixed(1) }} J
  </div>
</div>

                    <div v-else-if="activity.type === 'summary'" class="d-flex flex-column align-center">
                        <div class="text-h6 font-weight-bold primary--text mb-0">
                            {{ st.accompanyingDaysCount || 0 }}
                        </div>
                        <div class="text-caption grey--text mb-2" style="font-size: 0.7rem; line-height: 1;">
                            Jour(s) unique(s)
                        </div>
                        
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on, attrs }">
                                <v-btn 
                                    icon x-small color="primary" 
                                    v-bind="attrs" v-on="on"
                                    @click="$router.push({ name: 'AteliersFollowUp' })"
                                >
                                    <v-icon>mdi-pencil-box-multiple-outline</v-icon>
                                </v-btn>
                            </template>
                            <span>Ouvrir la Grille</span>
                        </v-tooltip>
                    </div>

                  </div>
                </td>

                <td class="synthesis-col font-weight-bold" :class="st.followUp.sessionAttendanceRate.hasData ? getRateClass(st.globalRate) : 'grey--text text--darken-1'">
  {{ st.globalRate.toFixed(1) }}%
  
  <v-progress-linear
    :value="st.globalRate"
    height="6"
    rounded
    :color="st.followUp.sessionAttendanceRate.hasData ? getRateColor(st.globalRate) : 'grey lighten-1'"
    class="mt-1"
  />
</td>

                <td class="sticky-col actions-col">
                  <v-btn icon small color="primary" @click="openBilanModal(st)">
                    <v-icon>mdi-file-document-edit-outline</v-icon>
                  </v-btn>
                </td>
                
                <td class="sticky-col actions-col">
                  <div class="d-flex justify-center">
                    <v-tooltip left>
                      <template #activator="{ on, attrs }">
                        <v-btn icon small color="primary" class="mr-1" v-bind="attrs" v-on="on" @click="downloadPdf('bilan', st)">
                          <v-icon>mdi-file-chart-outline</v-icon>
                        </v-btn>
                      </template>
                      <span>Bilan</span>
                    </v-tooltip>

                    <v-tooltip left>
                      <template #activator="{ on, attrs }">
                        <v-btn icon small color="secondary" v-bind="attrs" v-on="on" @click="downloadPdf('attestation', st)">
                          <v-icon>mdi-certificate-outline</v-icon>
                        </v-btn>
                      </template>
                      <span>Attestation</span>
                    </v-tooltip>
                  </div>
                </td>

              </tr>
              
              <tr v-if="!isLoading && filteredStagiaires.length === 0">
                  <td colspan="10" class="text-center pa-4 grey--text">Aucun stagiaire trouvé.</td>
              </tr>

            </tbody>
          </table>
        </div>
      </v-card-text>
    </v-card>

    <BilanEditModal
      v-model="bilanModalVisible"
      :stagiaire-data="selectedStagiaire"
      :period-info="currentPeriod"
      @bilan-saved="handleBilanSaved"
    />

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useStore } from 'vuex';
import moment from "moment";
import 'moment/locale/fr';
import BilanEditModal from "../components/BilanEditModal.vue"; 
import API_BASE from "@/services/api";

moment.locale("fr");
const momentHelper = moment; 
const store = useStore();

// --- ETAT LOCAL ---
const search = ref("");
const filterMesure = ref(null);
const isLoading = ref(false);
const bilanModalVisible = ref(false);
const selectedStagiaire = ref(null);

// Map pour stocker les niveaux spécifiquement pour la période chargée
const periodLevelsMap = ref({});

const currentYear = ref(moment().year());
const currentQuarter = ref(Math.floor((moment().month() / 3)) + 1); 

// Dates Spéciales
const joursFeries = ref([]);
const periodesVacances = ref([]);

// Données Matrice
const accompanyingMatrixData = ref([]);

const mesures = ["Français & Intégration", "Français en Télécours"];
const levels = ['A1.1', 'A1.2', 'A2.1', 'A2.2', 'B1.1', 'B1.2', 'B2.1', 'B2.2', 'C1.1', 'C1.2'];

// --- CONFIGURATION DES COLONNES ---
const activities = [
    { 
        id: 1, 
        key: "courseDetails", 
        libelle: "Formations Suivies", 
        type: "details", 
        description: "Détails des cours suivis selon l'affiliation."
    },
 
    { 
        id: 3, 
        key: "workshopsAndAccompanying", 
        libelle: "Ateliers & Accomp.", 
        type: "summary", 
        description: "Jours de présence aux ateliers et accompagnements.",
        weight: 40 
    },
       { 
        id: 2, 
        key: "sessionAttendanceRate", 
        libelle: "Présence Session (%)", 
        type: "rate", 
        description: "Taux calculé sur le trimestre (excluant fériés/vacances).",
        weight: 60 
    }
];

// --- COMPUTED PROPERTIES ---

const stagiaires = computed(() => store.state.stagiaire?.stagiaires || []);

const isCurrentQuarter = computed(() => {
    const today = moment();
    const currentQ = Math.floor((today.month() / 3)) + 1;
    return (currentQuarter.value === currentQ && currentYear.value === today.year());
});

const currentSessionDates = computed(() => {
    const year = currentYear.value;
    const quarter = currentQuarter.value;
    const startMonth = (quarter - 1) * 3;
    const start = moment([year, startMonth, 1]).startOf('month');
    const end = moment([year, startMonth + 2, 1]).endOf('month');
    return { 
        start: start.toDate(), 
        end: end.toDate(), 
        startKey: start.format('YYYY-MM-DD'), 
        endKey: end.format('YYYY-MM-DD') 
    };
});

const currentPeriodLabel = computed(() => {
    const start = moment(currentSessionDates.value.start);
    const end = moment(currentSessionDates.value.end);
    const cap = (s) => s.charAt(0).toUpperCase() + s.slice(1);
    return `T${currentQuarter.value} ${currentYear.value} (${cap(start.format('MMMM'))} - ${cap(end.format('MMMM'))})`;
});

const currentPeriod = computed(() => ({
    year: currentYear.value,
    quarter: currentQuarter.value,
    key: `${currentYear.value}-${currentQuarter.value}`,
    label: currentPeriodLabel.value
}));

// --- LOGIQUE PRINCIPALE : Filtrage et Calculs ---

const filteredStagiaires = computed(() => {
    let result = stagiaires.value.map(st => {
        
        st.dateDebutParticipation = st.dateDebutParticipation || moment(currentSessionDates.value.start).subtract(1, 'year').toDate();

        // 1. Calcul Taux
        const rateDetails = computeRate(st);
        
        // 2. Détails Cours
        const courseDetails = getCourseDetails(st);
        
        // 3. Détection Type Mesure
        // Initialisation à 'Autre'
        let mesureType = 'Autre';
        const className = (courseDetails.mainInfo || "").toLowerCase();

        // CORRECTION ICI : On utilise strictement 'Télécours' pour que la Modale le reconnaisse
        if (className.includes("télécours") || className.includes("telecours")) {
            mesureType = 'Télécours'; 
        } 
        else if (
            className.includes("français & intégration") || 
            className.includes("f&i") ||
            /(a|b|c)[1-2]/.test(className) 
        ) {
            mesureType = 'Français & Intégration';
        }

        // 4. Récupération Niveaux (Spécifique Période)
        const periodLevels = periodLevelsMap.value[st.stagiaireId] || {};
        const displayOral = periodLevels.oralLevel || ''; 
        const displayWritten = periodLevels.writtenLevel || '';

        // 5. Compteur Ateliers
        let daysCount = 0;
        if (accompanyingMatrixData.value.length > 0) {
            const items = accompanyingMatrixData.value.filter(x => x.stagiaireId === st.stagiaireId);
            const uniqueDates = new Set();
            items.forEach(item => {
                if(item.dates) item.dates.forEach(d => uniqueDates.add(d));
            });
            daysCount = uniqueDates.size;
        }

        return {
            ...st,
            courseDetails: courseDetails,
            mesureType: mesureType,
            oralLevel: displayOral,
            writtenLevel: displayWritten,
            followUp: {
                sessionAttendanceRate: { 
                    rate: rateDetails.attendanceRate, 
                    hasData: rateDetails.hasPresenceData,
                    totalAbsencePenalty: rateDetails.totalAbsencePenalty
                }
            },
            globalRate: rateDetails.globalRate,
            accompanyingDaysCount: daysCount 
        };
    });

    // CORRECTION FILTRE: Gestion du cas où le Select = "Français en Télécours" et la data = "Télécours"
    if (filterMesure.value) {
        result = result.filter(st => {
             if (filterMesure.value === 'Français en Télécours' && st.mesureType === 'Télécours') {
                 return true;
             }
             return st.mesureType === filterMesure.value;
        });
    }

    if (search.value) {
        const s = search.value.toLowerCase();
        result = result.filter(st =>
            st.nom.toLowerCase().includes(s) ||
            st.prenom.toLowerCase().includes(s) ||
            st.username.toLowerCase().includes(s) ||
            (st.typeAffiliation?.libelle || "").toLowerCase().includes(s)
        );
    }

    return result;
});

// --- API & DATA FETCHING ---

const fetchDataForPeriod = async () => {
    if (isLoading.value) return;
    isLoading.value = true;
    
    // IMPORTANT : Reset local map pour éviter les conflits T3/T4
    periodLevelsMap.value = {};

    const start = currentSessionDates.value.startKey;
    const end = currentSessionDates.value.endKey;
    const token = localStorage.getItem("access_token");

    const year = currentYear.value;
    const quarter = currentQuarter.value;
    
    try {
        // 1. Présences
        const urlAtt = `${API_BASE}/api/attendance/byWeek?start=${start}&end=${end}`;
        const resAtt = await fetch(urlAtt, { headers: { Authorization: `Bearer ${token}` } });
        if (resAtt.ok) {
            const data = await resAtt.json();
            await store.dispatch("stagiaire/updateStagiaireAttendances", {
                attendances: Array.isArray(data) ? data : [],
                currentPeriodDays: [] 
            });
        }

        // 2. Matrice
        const urlMatrix = `${API_BASE}/api/accompanying/matrix?start=${start}&end=${end}`;
        const resMatrix = await fetch(urlMatrix, { headers: { Authorization: `Bearer ${token}` } });
        if (resMatrix.ok) {
            accompanyingMatrixData.value = await resMatrix.json();
        }

        // 3. Niveaux du Trimestre
        const urlLevels = `${API_BASE}/api/stagiaires/levels?year=${year}&quarter=${quarter}`;
        const resLevels = await fetch(urlLevels, { headers: { Authorization: `Bearer ${token}` } });
        
        if (resLevels.ok) {
            const levelsData = await resLevels.json(); 
            const map = {};
            if (Array.isArray(levelsData)) {
                levelsData.forEach(l => {
                    map[l.stagiaireId] = {
                        oralLevel: l.oralLevel,
                        writtenLevel: l.writtenLevel
                    };
                });
            }
            periodLevelsMap.value = map;
        }

    } catch (err) {
        console.error("Erreur Fetch Data", err);
    } finally {
        isLoading.value = false;
    }
};

const saveLevel = async (stagiaireId, field, newLevel) => {
    // Mise à jour locale immédiate
    if (!periodLevelsMap.value[stagiaireId]) {
        periodLevelsMap.value[stagiaireId] = {};
    }
    periodLevelsMap.value[stagiaireId][field] = newLevel;

    const period = currentPeriod.value; 
    const token = localStorage.getItem("access_token");
    const url = `${API_BASE}/api/stagiaires/${stagiaireId}/level`; 

    try {
        const res = await fetch(url, {
            method: "POST",
            headers: { 
                "Content-Type": "application/json", 
                Authorization: `Bearer ${token}` 
            },
            body: JSON.stringify({
                field: field,         
                value: newLevel,
                year: period.year,
                quarter: period.quarter
            })
        });

        if (!res.ok) {
            console.error("Erreur API sauvegarde niveau", await res.text());
        }
    } catch (e) {
        console.error("Erreur réseau niveau", e);
    }
};

const computeRate = (st) => {
    const dates = currentSessionDates.value;
    if (!dates.start || !dates.end) return { attendanceRate: 100, globalRate: 100, hasPresenceData: false, totalAbsencePenalty: 0 };

    // Date de début de participation du stagiaire
    const participationStart = st.dateDebutParticipation 
        ? moment(st.dateDebutParticipation).startOf('day')
        : moment('2000-01-01');

    // DATE CLÉ : On arrête le calcul "Session" à ce soir
    const today = moment().endOf('day');

    let denomGlobal = 0; // Dénominateur Trimestre (ex: 60 jours)
    let denomActive = 0; // Dénominateur Session (ex: 15 jours si on est le 22 Janvier)
    
    let penalty = 0; // Total des pénalités sur la période
    let penaltyActive = 0; // Pénalités sur la période active (normalement identique si pas de saisie future)
    let hasExplicitBadData = false;

    let curr = moment(dates.start);
    const end = moment(dates.end);

    while (curr.isSameOrBefore(end, 'day')) {
        const dateStr = curr.format('YYYY-MM-DD'); 
        const dateKey = curr.format('ddd MMM DD YYYY'); 
        const dayOfWeek = curr.isoWeekday();
        const isWorking = dayOfWeek >= 1 && dayOfWeek <= 5;
        
        if (isWorking) {
            const isFerie = joursFeries.value.includes(dateStr);
            const isVac = periodesVacances.value.some(v => 
                curr.isBetween(moment(v.start), moment(v.end), 'day', '[]')
            );

            if (!isFerie && !isVac) {
                // 1. Dénominateur GLOBAL (Tout le trimestre)
                denomGlobal++;
                
                // Récupération de la pénalité pour ce jour
                const status = st.presences?.[dateKey];
                let currentPenalty = 0;
                
                if (status === 'a') {
                    currentPenalty = 1;
                    hasExplicitBadData = true;
                } else if (status === 'r') {
                    currentPenalty = 0.5;
                    hasExplicitBadData = true;
                }
                
                // Ajout à la pénalité globale
                penalty += currentPenalty;

                // 2. Dénominateur SESSION (Borné à "Début Stagiaire" ET "Aujourd'hui")
                const isStarted = curr.isSameOrAfter(participationStart, 'day');
                const isPastOrToday = curr.isSameOrBefore(today, 'day');

                if (isStarted && isPastOrToday) {
                    denomActive++;
                    // On n'ajoute la pénalité au taux session que si le jour est passé
                    penaltyActive += currentPenalty;
                }
            }
        }
        curr.add(1, 'day');
    }
    
    // --- Calcul Taux Session (Réalisé à date) ---
    // Ex: 12 absences sur 15 jours écoulés = (15-12)/15 = 20%
    let rateActive = 100;
    if (denomActive > 0) {
        rateActive = ((denomActive - penaltyActive) / denomActive) * 100;
    }

    // --- Calcul Taux Global (Projection Trimestre) ---
    // Ex: 12 absences sur 60 jours trimestre = (60-12)/60 = 80%
    let rateGlobal = 100;
    if (denomGlobal > 0) {
        rateGlobal = ((denomGlobal - penalty) / denomGlobal) * 100;
    }

    return { 
        attendanceRate: Math.max(0, rateActive), // C'est celui-ci qui ira dans "Présence Session"
        globalRate: Math.max(0, rateGlobal),     // Celui-ci ira dans "Taux Global"
        hasPresenceData: hasExplicitBadData, 
        totalAbsencePenalty: penaltyActive 
    };
};

const getCourseDetails = (st) => {
    // 1. Chercher les sessions actives
    const sessions = st.sessionStudents?.map(ss => ss.session).filter(s => s) || [];
    
    let mainInfo = '';
    let subCourses = [];
    let isUnknown = false; 

    if (sessions.length > 0) {
        // Une session existe
        const latestSession = sessions.sort((a, b) => new Date(b.dateStart) - new Date(a.dateStart))[0];
        const classNameFromSession = latestSession?.schoolClassRoom?.schoolClassRoomName;
        
        mainInfo = classNameFromSession || st.typeAffiliation?.libelle;
        
        // Si malgré la session, pas de nom de classe, on considère inconnu
        if (!mainInfo) isUnknown = true;

    } else {
        // PAS DE SESSION = COURS INCONNU
        // CORRECTION : On force isUnknown à true même s'il y a une affiliation générique (ex: "Stagiaire")
        // Cela garantit l'affichage du lien
        isUnknown = true;
    }

    return { mainInfo, subCourses, isUnknown };
};

const loadJoursFeriesFromStorage = () => {
    try {
        const savedJF = localStorage.getItem('joursFeries');
        joursFeries.value = savedJF ? JSON.parse(savedJF) : ['2025-01-01', '2025-12-25'];
        const savedVac = localStorage.getItem('periodesVacances');
        periodesVacances.value = savedVac ? JSON.parse(savedVac) : [];
    } catch (e) { console.error(e); }
};

// --- NAVIGATION ---
const previousQuarter = () => {
    currentQuarter.value--;
    if (currentQuarter.value < 1) { currentQuarter.value = 4; currentYear.value--; }
    fetchDataForPeriod();
};
const nextQuarter = () => {
    currentQuarter.value++;
    if (currentQuarter.value > 4) { currentQuarter.value = 1; currentYear.value++; }
    fetchDataForPeriod();
};

const openBilanModal = (st) => {
    selectedStagiaire.value = st;
    bilanModalVisible.value = true;
};

const handleBilanSaved = (payload) => {
    bilanModalVisible.value = false;
};

const downloadPdf = async (type, st) => {
    const endpointType = type === 'bilan' ? 'bilan-session' : 'attestation-session';
    const year = currentYear.value;
    const quarter = currentQuarter.value;
    const stagiaireId = st.stagiaireId;
    const mesure = st.mesureType;

    const url = `${API_BASE}/api/documents/${endpointType}/${stagiaireId}?year=${year}&trimestre=${quarter}&mesure=${encodeURIComponent(mesure)}`;
    const token = localStorage.getItem("access_token");
    try {
        isLoading.value = true;
        const res = await fetch(url, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            const blob = await res.blob();
            const downloadUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = downloadUrl;
            const fileName = `${type}-${st.nom}-${year}-T${quarter}.pdf`;
            link.setAttribute('download', fileName);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(downloadUrl); 
        } else {
            alert(`Impossible de générer le ${type}. Erreur serveur.`);
        }
    } catch (e) {
        alert("Erreur de connexion.");
    } finally {
        isLoading.value = false;
    }
};

const getRateColor = (r) => r >= 80 ? "green" : r >= 50 ? "orange" : "red";
const getRateClass = (r) => r >= 80 ? "success--text" : r >= 50 ? "warning--text" : "error--text";

onMounted(() => {
    loadJoursFeriesFromStorage();
    store.dispatch("stagiaire/fetchStagiaires", true)
        .then(() => fetchDataForPeriod())
        .catch(e => console.error(e));
});
</script>

<style scoped>
.table-container { overflow-x: auto; max-height: 80vh; }
.follow-up-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.follow-up-table th, .follow-up-table td { border: 1px solid #e0e0e0; padding: 8px 6px; text-align: center; vertical-align: middle; }
.sticky-col { position: sticky; background: white; z-index: 3; }
.name-col { min-width: 220px; left: 0; text-align: left; padding: 12px; }
.actions-col { min-width: 80px; right: 0; background: #f8f9fa; }
.activity-col { background: #fafafa; position: sticky; top: 0; z-index: 4; font-weight: 600; color: #555; }
.follow-up-table thead th.sticky-col { z-index: 5; }
.saisie-col { min-width: 100px; }
.synthesis-col { min-width: 120px; background: #f5f5f5; }
.stagiaire-info .name { font-weight: bold; color: #333; font-size: 0.95rem; }
.stagiaire-info .affiliation { font-size: 0.75rem; color: #666; font-weight: 500; margin-top: 2px; }
.centered-input :deep(input) { text-align: center; font-weight: bold; }
ul { padding-left: 15px; margin: 0; }
</style>