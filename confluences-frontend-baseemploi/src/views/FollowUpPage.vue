<script setup>
import { ref, computed, onMounted } from 'vue';
import { useStore } from 'vuex';
import moment from "moment";
import 'moment/locale/fr';
import BilanEditModal from "../components/BilanEditModal.vue"; 
import API_BASE from "@/services/api";

moment.locale("fr");

// --- FIX MOMENT VISIBILITY ---
// On expose moment à la template pour éviter l'erreur TypeError: _ctx.moment is not a function
const momentHelper = moment; 

// --- Configuration et Données Locales ---
const store = useStore();

const search = ref("");
const filterMesure = ref(null);
const isLoading = ref(false);

const bilanModalVisible = ref(false);
const selectedStagiaire = ref(null);

const currentYear = ref(moment().year());
// On initialise par défaut au trimestre actuel
const currentQuarter = ref(Math.floor((moment().month() / 3)) + 1); 

// Dates spéciales (pour le calcul précis - chargées depuis localStorage/défaut)
const joursFeries = ref([]);
const periodesVacances = ref([]);

const mesures = [
    "Français & Intégration",
    "Français en Télécours"
];

const accompanyingItems = [
    'Questions administratives', 'Appui en français', 'Recherche de stages et d’emplois',
    'Bilans et perspectives', 'Informatique', 'Théorie auto', 'Maths',
    'Recherche de logement', 'Sortie culturelle', 'Théâtre', 'Cuisine', 'Jardinage',
];

// --- ACTIVITÉS ---
const activities = [
    // 1. Formations Suivies (Détails)
    { 
        id: 1, 
        key: "courseDetails", 
        libelle: "Formations Suivies", 
        type: "details", 
        description: "Liste des formations suivies par le stagiaire."
    },
    // 2. Taux de Présence de Session (Calculé sur l'intégralité du trimestre)
    { 
        id: 2, 
        key: "sessionAttendanceRate", 
        libelle: "Présence Session (%)", 
        type: "rate", 
        description: "Taux de présence calculé sur le trimestre entier (incluant R=0.5, jours exclus).",
        weight: 60 // Poids dans le Taux Global
    },
    // 3. Ateliers & Accompagnement (Saisie)
    { 
        id: 3, 
        key: "workshopsAndAccompanying", 
        libelle: "Ateliers & Accompagnement", 
        unit: "Items cochés", 
        type: "list", 
        totalMax: accompanyingItems.length,
        description: "Ateliers thématiques et séances d'accompagnement.",
        items: accompanyingItems,
        weight: 40 // Poids dans le Taux Global
    }
];

// Données fictives pour les items cochés (À remplacer par API si nécessaire)
const accompanyingData = ref({
    // Initialisation mockée pour que l'affichage soit fonctionnel
    "s1": { '2025-4': { selectedItems: ['Questions administratives', 'Informatique'] } },
    "s2": { '2025-4': { selectedItems: ['Appui en français'] } },
});


// --- Computed Properties ---

const stagiaires = computed(() => store.state.stagiaire.stagiaires);

const isCurrentQuarter = computed(() => {
    const today = moment();
    const currentQ = Math.floor((today.month() / 3)) + 1;
    return (currentQuarter.value === currentQ && currentYear.value === today.year());
});

const currentPeriodLabel = computed(() => {
    const dates = currentSessionDates.value;
    const start = moment(dates.start);
    const end = moment(dates.end);

    const startMonth = start.format('MMMM'); // Ex: Octobre
    const endMonth = end.format('MMMM'); // Ex: Décembre
    
    // Capitaliser la première lettre des mois (moment le fait souvent, mais pour être sûr)
    const capitalize = (s) => s.charAt(0).toUpperCase() + s.slice(1);

    return `T${currentQuarter.value} (${capitalize(startMonth)} à ${capitalize(endMonth)})`;
});

// Mise à jour de la computed property 'currentPeriod'
const currentPeriod = computed(() => ({
    year: currentYear.value,
    quarter: currentQuarter.value,
    key: `${currentYear.value}-${currentQuarter.value}`,
    label: currentPeriodLabel.value // <-- AJOUTEZ CE LABEL
}));

/**
 * Détermine les dates de début et de fin de la session/trimestre civil.
 */
const currentSessionDates = computed(() => {
    const year = currentYear.value;
    const quarter = currentQuarter.value; // 1 à 4

    const startMonth = (quarter - 1) * 3;
    const endMonth = startMonth + 2;
    
    const start = moment([year, startMonth, 1]).startOf('month');
    const end = moment([year, endMonth, 1]).endOf('month');

    return { 
        start: start.toDate(), 
        end: end.toDate(), 
        startKey: start.format('YYYY-MM-DD'), 
        endKey: end.format('YYYY-MM-DD') 
    };
});


const filteredStagiaires = computed(() => {
    let result = stagiaires.value.map(st => {
        // Le `dateDebutParticipation` doit être présent sur l'objet stagiaire, sinon on prend une valeur par défaut.
        st.dateDebutParticipation = st.dateDebutParticipation || moment(currentSessionDates.value.start).subtract(1, 'year').toDate();

        const accompanyingFollowUp = accompanyingData.value[st.stagiaireId]?.[currentPeriod.value.key] || 
                             getDefaultAccompanyingFollowUp(st.stagiaireId); 
        
        const rateDetails = computeRate(st, accompanyingFollowUp);
        const courseDetails = getCourseDetails(st);

        return {
            ...st,
            courseDetails: courseDetails, 
            followUp: {
                courseDetails: courseDetails,
                sessionAttendanceRate: { 
                    rate: rateDetails.attendanceRate, 
                    hasData: rateDetails.hasPresenceData,
                    totalAbsencePenalty: rateDetails.totalAbsencePenalty
                },
                workshopsAndAccompanying: accompanyingFollowUp
            },
            globalRate: rateDetails.globalRate 
        };
    });

    if (filterMesure.value) {
        result = result.filter(st => {
            const mesureToFilter = st.mesure || st.typeAffiliation?.libelle;
            return mesureToFilter === filterMesure.value; 
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

// --- Fonctions de Logique Métier ---

const loadJoursFeriesFromStorage = () => {
    try {
        const defaultJoursFeries = ['2025-01-01', '2025-04-18', '2025-04-21', '2025-05-01', '2025-05-29', '2025-06-09', '2025-09-18', '2025-12-25'];
        const defaultPeriodesVacances = [{ start: '2025-12-22', end: '2026-01-04' }, { start: '2025-07-07', end: '2025-08-15' }];

        const savedJoursFeries = localStorage.getItem('joursFeries');
        joursFeries.value = savedJoursFeries ? JSON.parse(savedJoursFeries) : defaultJoursFeries;
        
        const savedPeriodesVacances = localStorage.getItem('periodesVacances');
        periodesVacances.value = savedPeriodesVacances ? JSON.parse(savedPeriodesVacances) : defaultPeriodesVacances;
    } catch (e) {
        console.error("Erreur chargement dates spéciales", e);
    }
};

const getDefaultAccompanyingFollowUp = (stagiaireId) => {
    if (!accompanyingData.value[stagiaireId]) {
        accompanyingData.value[stagiaireId] = {};
    }
    if (!accompanyingData.value[stagiaireId][currentPeriod.value.key]) {
        accompanyingData.value[stagiaireId][currentPeriod.value.key] = { selectedItems: [] };
    }
    return accompanyingData.value[stagiaireId][currentPeriod.value.key];
};

/**
 * Fonction pour déterminer les cours suivis par le stagiaire (selon l'affiliation).
 */
const getCourseDetails = (st) => {
    const affiliation = st.typeAffiliation?.libelle || st.mesure || ""; 
    let mainInfo = '';
    let subCourses = [];

    if (affiliation.includes("Français & Intégration")) {
        mainInfo = 'A suivi la formation "Français & Intégration" (15 heures par semaine).';
        subCourses = [
            'Français de niveau A1',
            'Informatique et bureautique',
            'Aide à la recherche d’emploi'
        ];
    } else if (affiliation.includes("Télécours")) {
        mainInfo = 'A suivi la formation "Français en TéléCours" (9 heures par semaine en télé-enseignement).';
        subCourses = [
            'Français de niveau A2',
            'Compétences numériques de base'
        ];
    } else {
        mainInfo = `${st.typeAffiliation?.libelle || 'Affiliation non spécifiée'}.`;
        subCourses = [];
    }

    return { mainInfo, subCourses };
};

/**
 * Calcule les taux sur la période du trimestre (Session).
 * LOGIQUE ROBUSTE : Incorpore Jours Fériés, Vacances, Début de Participation, et Pénalité R=0.5.
 */
const computeRate = (st, accompanyingFollowUp) => {
    const dates = currentSessionDates.value;
    
    if (!dates.start || !dates.end) return { attendanceRate: 0, globalRate: 0, hasPresenceData: false, totalAbsencePenalty: 0 };

    const participationStartDate = moment(st.dateDebutParticipation).startOf('day'); 
                                     
    
    let achievedDaysNets = 0; 
    let joursNotésPendantPériodeThéorique = 0; 
    let hasPresenceData = false;
    let totalAbsencePenalty = 0;

    let currentDay = moment(dates.start);
    const endDay = moment(dates.end); 

    while (currentDay.isSameOrBefore(endDay, 'day')) {
        const dayOfWeek = currentDay.isoWeekday(); 
        const dateKey = currentDay.format('ddd MMM DD YYYY'); 
        const dateString = currentDay.format('YYYY-MM-DD'); 
        const status = st.presences?.[dateKey]; // Données de présence de la page d'assiduité (Store)

        // 1. Déterminer si le jour est théoriquement attendu
        const isWorkingDay = dayOfWeek >= 1 && dayOfWeek <= 5;
        const isAfterParticipationStart = currentDay.isSameOrAfter(participationStartDate, 'day');
        const isFerie = joursFeries.value.includes(dateString);
        
        const isVacance = periodesVacances.value.some(vac => {
            const start = moment(vac.start, 'YYYY-MM-DD').startOf('day');
            const end = moment(vac.end, 'YYYY-MM-DD').endOf('day');
            return currentDay.isBetween(start, end, 'day', '[]');
        });
        
        if (isWorkingDay && isAfterParticipationStart && !isFerie && !isVacance) {
            
            if (status && status !== "") { 
                joursNotésPendantPériodeThéorique++;
                hasPresenceData = true;

                if (status === 'p' || status === 'e') {
                    achievedDaysNets++; 
                } else if (status === 'r') {
                    achievedDaysNets += 0.5; // Pénalité: 0.5 absence = 0.5 présence acquise
                    totalAbsencePenalty += 0.5;
                } 
            }
        }
        currentDay.add(1, 'day');
    }
    
    // Taux de présence Trimestriel
    const attendanceRate = joursNotésPendantPériodeThéorique > 0 
        ? Math.min(100, (achievedDaysNets / joursNotésPendantPériodeThéorique) * 100) 
        : 0;

    // Taux de complétion des activités
    const accompanyingActivity = activities.find(a => a.key === 'workshopsAndAccompanying');
    let activityRate = 0;
    if (accompanyingActivity && accompanyingActivity.totalMax > 0) {
        const achievedItems = accompanyingFollowUp?.selectedItems?.length || 0;
        activityRate = Math.min(100, (achievedItems / accompanyingActivity.totalMax) * 100);
    }
    
    // Taux Global = (60% Présence + 40% Activités)
    const presenceWeight = 60;
    const activityWeight = 40;
    const totalWeight = presenceWeight + activityWeight;

    let globalRate = 0;
    if (totalWeight > 0) {
        globalRate = ((attendanceRate * presenceWeight) + (activityRate * activityWeight)) / totalWeight;
    }
    
    return { 
        attendanceRate: attendanceRate, 
        globalRate: Math.min(100, globalRate),
        hasPresenceData: hasPresenceData,
        totalAbsencePenalty: totalAbsencePenalty
    };
};


// --- Navigation / Actions ---

const previousQuarter = () => {
    currentQuarter.value--;
    if (currentQuarter.value < 1) {
        currentQuarter.value = 4;
        currentYear.value--;
    }
    fetchDataForPeriod();
};

const nextQuarter = () => {
    currentQuarter.value++;
    if (currentQuarter.value > 4) {
        currentQuarter.value = 1;
        currentYear.value++;
    }
    fetchDataForPeriod();
};

const fetchDataForPeriod = async () => {
    if (isLoading.value) return;
    isLoading.value = true;
    
    const startDate = currentSessionDates.value.startKey;
    const endDate = currentSessionDates.value.endKey;
    
    const token = localStorage.getItem("access_token");
    const url = `${API_BASE}/api/attendance/byWeek?start=${startDate}&end=${endDate}`;

    try {
        const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } });
        
        if (res.ok) {
            const rawData = await res.json();
            const attendances = Array.isArray(rawData) ? rawData : [];

            await store.dispatch("stagiaire/updateStagiaireAttendances", {
                attendances: attendances,
                currentPeriodDays: [] 
            });
        } else {
            console.error("Erreur API FollowUp", await res.text());
        }
    } catch (err) {
        console.error("Erreur Fetch FollowUp", err);
    } finally {
        isLoading.value = false;
    }
};

const saveAssiduity = (stagiaireId, activityKey) => {
    console.log(`Sauvegarde locale Accompagnement ${stagiaireId} pour ${activityKey}`);
};

const openBilanModal = (st) => {
    selectedStagiaire.value = st;
    bilanModalVisible.value = true;
};

// FollowUp.vue (partie <script setup>)

// ... (définition de currentPeriod, filteredStagiaires, etc.) ...

const handleBilanSaved = (savedPayload) => {
    bilanModalVisible.value = false;
    
    if (selectedStagiaire.value) {
        const periodKey = `${savedPayload.year}-${savedPayload.quarter}`;
        
        const report = {
            evaluationText: savedPayload.evaluationText,
            followUpActions: savedPayload.followUpActions,
            globalRate: savedPayload.globalRate,
        };
        store.dispatch("stagiaire/updateSessionReport", {
            stagiaireId: selectedStagiaire.value.stagiaireId,
            periodKey: periodKey,
            report: report
        });
    }
};

const downloadPdf = async (type, st) => {
    // type = 'bilan' ou 'attestation'
    // st = l'objet stagiaire
    
    // On détermine l'endpoint : attention à bien faire correspondre avec le Controller C#
    const endpointType = type === 'bilan' ? 'bilan-session' : 'attestation-session';
    
    const year = currentYear.value;
    const quarter = currentQuarter.value;
    const stagiaireId = st.stagiaireId;

    const url = `${API_BASE}/api/documents/${endpointType}/${stagiaireId}?year=${year}&trimestre=${quarter}`;
    const token = localStorage.getItem("access_token");

    try {
        // On affiche un loader si besoin (global ou local)
        isLoading.value = true;

        const res = await fetch(url, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (res.ok) {
            // Conversion de la réponse en Blob pour le téléchargement
            const blob = await res.blob();
            const downloadUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = downloadUrl;
            
            // Nom du fichier
            const fileName = `${type}-${st.nom}-${year}-T${quarter}.pdf`;
            link.setAttribute('download', fileName);
            
            document.body.appendChild(link);
            link.click();
            link.remove();
        } else {
            console.error("Erreur génération PDF");
        }
    } catch (e) {
        console.error("Erreur réseau PDF", e);
    } finally {
        isLoading.value = false;
    }
};
// --- Helpers Visuels ---
const getRateColor = (rate) => {
    if (rate >= 80) return "green";
    if (rate >= 50) return "orange";
    return "red";
};

const getRateClass = (rate) => {
    if (rate >= 80) return "success--text";
    if (rate >= 50) return "warning--text";
    return "error--text";
};

// --- Lifecycle ---

onMounted(() => {
    loadJoursFeriesFromStorage();
    
    store.dispatch("stagiaire/fetchStagiaires", true).then(() => {
        fetchDataForPeriod();
    }).catch(err => {
        console.error("Échec chargement stagiaires:", err);
        fetchDataForPeriod();
    });
});
</script>

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
                            Session {{ currentQuarter }} - {{ currentYear }}
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
                <span class="text-h5 font-weight-bold">Saisie et Synthèse des Activités</span>
                <v-spacer></v-spacer>
            </v-card-title>

            <v-card-text class="pa-0">
                <div class="table-container">
                    <table class="follow-up-table">

                        <thead>
                            <tr>
                                <th class="sticky-col name-col">Stagiaire</th>

                                <th
                                    v-for="activity in activities"
                                    :key="activity.id"
                                    class="activity-col"
                                    :style="{ minWidth: activity.type === 'details' ? '300px' : '160px' }"
                                >
                                    {{ activity.libelle }}
                                    <v-tooltip bottom>
                                        <template #activator="{ on, attrs }">
                                            <v-icon small class="ml-1" v-bind="attrs" v-on="on">
                                                mdi-information
                                            </v-icon>
                                        </template>
                                        <span>{{ activity.description }}</span>
                                    </v-tooltip>
                                </th>

                                <th class="synthesis-col">Taux Global</th>

                                <th class="sticky-col actions-col">Bilan</th>
                                <th class="sticky-col actions-col">PDFs </th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr v-for="st in filteredStagiaires" :key="st.stagiaireId">
                                <td class="sticky-col name-col">
                                    <div class="stagiaire-info">
                                        <div class="name">{{ st.prenom }} {{ st.nom }}</div>
                                        <div class="trigramme">{{ st.username }}</div>
                                        <div class="affiliation text-caption">
                                            {{ st.typeAffiliation?.libelle || 'N/A' }} – Niveau {{ st.level || 'N/A' }}
                                        </div>
                                        <div v-if="st.dateDebutParticipation" class="text-caption grey--text">
                                            Début: {{ momentHelper(st.dateDebutParticipation).format('DD/MM/YYYY') }}
                                        </div>
                                    </div>
                                </td>

                                <td v-for="activity in activities" :key="activity.key" class="saisie-col">
                                    <div class="d-flex justify-center" style="flex-direction: column; align-items: center;">

                                        <div v-if="activity.type === 'details'" class="text-left pa-2" style="max-width: 95%;">
                                            <div class="text-caption font-weight-medium primary--text">
                                                {{ st.courseDetails.mainInfo }}
                                            </div>
                                            <ul v-if="st.courseDetails.subCourses.length" class="text-caption ml-n6 mt-1">
                                                <li v-for="course in st.courseDetails.subCourses" :key="course" style="list-style-type: disc;">
                                                    {{ course }}
                                                </li>
                                            </ul>
                                        </div>

                                        <div v-else-if="activity.type === 'rate'" class="d-flex flex-column align-center">
                                            <v-text-field
                                                :value="st.followUp.sessionAttendanceRate.hasData ? st.followUp.sessionAttendanceRate.rate.toFixed(1) + '%' : '-'"
                                                dense outlined single-line hide-details readonly
                                                class="centered-input"
                                                style="width: 80px;"
                                                :class="st.followUp.sessionAttendanceRate.hasData ? getRateClass(st.followUp.sessionAttendanceRate.rate) : 'grey--text'"
                                            />
                                            <div v-if="st.followUp.sessionAttendanceRate.totalAbsencePenalty > 0" class="text-caption error--text" style="line-height: 1.2;">
                                                <v-icon small color="red">mdi-clock-alert</v-icon> Pénalité: {{ st.followUp.sessionAttendanceRate.totalAbsencePenalty.toFixed(1) }} Jours
                                            </div>
                                        </div>

                                        <v-select
                                            v-else-if="activity.type === 'list'"
                                            v-model="st.followUp[activity.key].selectedItems"
                                            :items="activity.items"
                                            label="Sélectionner"
                                            multiple
                                            small-chips
                                            dense outlined single-line hide-details
                                            @update:modelValue="saveAssiduity(st.stagiaireId, activity.key)"
                                            style="width: 180px;"
                                        >
                                            <template v-slot:selection="{ item, index }">
                                                <span v-if="index === 0" class="text-caption text-truncate" style="max-width: 100px;">
                                                    {{ item }}
                                                </span>
                                                <span v-if="index === 1" class="grey--text caption ml-1">
                                                    (+{{ st.followUp[activity.key].selectedItems.length - 1 }})
                                                </span>
                                            </template>
                                        </v-select>

                                    </div>
                                </td>

                                <td class="synthesis-col font-weight-bold"
                                    :class="getRateClass(st.globalRate)">
                                    
                                    {{ st.globalRate.toFixed(1) }}%

                                    <v-progress-linear
                                        :value="st.globalRate"
                                        height="6"
                                        rounded
                                        :color="getRateColor(st.globalRate)"
                                        class="mt-1"
                                    />
                                </td>

                                <td class="sticky-col actions-col">
                                    <v-btn icon small color="primary" @click="openBilanModal(st)">
                                        <v-icon>mdi-file-document-edit-outline</v-icon>
                                    </v-btn>
                                </td>
                                
                                <td class="sticky-col actions-col">
    <div class="d-flex flex-column align-center">
        
        <v-tooltip left>
            <template #activator="{ on, attrs }">
                <v-btn 
                    icon small 
                    color="primary" 
                    class="mb-1"
                    v-bind="attrs" v-on="on"
                    @click="downloadPdf('bilan', st)"
                >
                    <v-icon>mdi-file-chart-outline</v-icon>
                </v-btn>
            </template>
            <span>Télécharger le Bilan de Mesure</span>
        </v-tooltip>

        <v-tooltip left>
            <template #activator="{ on, attrs }">
                <v-btn 
                    icon small 
                    color="secondary" 
                    v-bind="attrs" v-on="on"
                    @click="downloadPdf('attestation', st)"
                >
                    <v-icon>mdi-certificate-outline</v-icon>
                </v-btn>
            </template>
            <span>Télécharger l'Attestation</span>
        </v-tooltip>
    </div>
</td>

                            </tr>
                        </tbody>

                    </table>

                    <v-alert
                        v-if="!filteredStagiaires.length && !isLoading"
                        type="info"
                        class="ma-4"
                    >
                        Aucun stagiaire trouvé selon les filtres actuels.
                    </v-alert>

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


<style scoped>
/* Styles ajustés */
.table-container { overflow-x: auto; max-height: 80vh; }
.follow-up-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.follow-up-table th, .follow-up-table td { border: 1px solid #e0e0e0; padding: 8px 6px; text-align: center; font-size: 0.9rem; }
.sticky-col { position: sticky; background: white; z-index: 3; }
.name-col { min-width: 250px; left: 0; text-align: left; padding: 12px; }
.actions-col { min-width: 80px; right: 0; background: #f8f9fa; box-shadow: -2px 0 4px rgba(0,0,0,0.1); }
.activity-col { min-width: 160px; background: #fafafa; position: sticky; top: 0; z-index: 4; }
.follow-up-table thead th.sticky-col { z-index: 5; }
/* Augmenté la largeur pour les détails de formation */
.activity-col:nth-of-type(2) { min-width: 300px; } 
.saisie-col { min-width: 160px; }
.synthesis-col { min-width: 120px; background: #f5f5f5; }
.stagiaire-info .name { font-weight: bold; color: #333; }
.stagiaire-info .affiliation { font-size: 0.75rem; color: #444; font-weight: 500; }

/* Correction du centrage des inputs */
.saisie-col :deep(.centered-input input) {
    text-align: center;
    font-weight: bold;
}

ul {
    padding-left: 15px; /* Ajuster la puce de liste */
    margin: 0;
}
</style>