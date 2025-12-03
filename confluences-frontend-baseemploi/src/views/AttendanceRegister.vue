<template>
  <v-container fluid class="pa-4">
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center" class="px-2">
        <v-col cols="12" md="6">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un stagiaire..."
            placeholder="Nom, Prénom, Trigramme, Affiliation..."
            outlined
            dense
            clearable
            hide-details
            background-color="white"
          />
        </v-col>

        <v-col cols="12" md="6" class="d-flex justify-md-end align-center">
          <v-btn small text class="mr-4" @click="openSpecialDatesModal">
            <v-icon left>mdi-calendar-edit</v-icon>
            Dates Spéciales
          </v-btn>

          <v-chip-group v-model="filterStatus" multiple>
            <v-chip small filter outlined value="p">Présent</v-chip>
            <v-chip small filter outlined value="a">Absent</v-chip>
            <v-chip small filter outlined value="e">Excusé</v-chip>
            <v-chip small filter outlined value="r">Retard</v-chip>
          </v-chip-group>
        </v-col>
      </v-row>
    </v-card>

    <v-card class="mb-4 elevation-2" rounded="lg">
      <v-card-text class="pa-4">
        <div class="d-flex justify-space-between align-center">
          <v-btn icon @click="previousPeriod" small :disabled="isLoading">
            <v-icon>mdi-chevron-left</v-icon>
          </v-btn>

          <div class="text-center">
            <div class="text-h6 font-weight-bold primary--text">
              {{ currentPeriodLabel }}
            </div>
            <div class="text-caption grey--text">
              {{ currentPeriodRange }}
            </div>
          </div>

          <v-btn icon @click="nextPeriod" small :disabled="isCurrentPeriod || isLoading">
            <v-icon>mdi-chevron-right</v-icon>
          </v-btn>
        </div>

        <v-divider class="my-4"></v-divider>
        <div class="d-flex flex-nowrap timeline-days-container">
          <div
            v-for="day in currentPeriodDays"
            :key="day.fullDate"
            class="text-center timeline-day"
            :class="{ 'weekend': day.isWeekend, 'today': day.isToday, 'month-view': true }"
          >
            <div class="text-caption font-weight-medium">{{ day.dayNameShort }}</div>
            <div class="text-body-2 font-weight-bold">{{ day.date }}</div>
            <div class="text-caption grey--text">{{ day.monthShort }}</div>
          </div>
        </div>
      </v-card-text>
    </v-card>

    <v-card class="elevation-4" rounded="lg" :loading="isLoading">
      <v-card-title class="pa-4">
        <div class="d-flex align-center" style="width: 100%;">
          <span class="text-h5 font-weight-bold">Suivi des présences</span>
          <v-spacer></v-spacer>
          <div class="d-flex align-center">
            <v-tooltip bottom><template v-slot:activator="{ on, attrs }"><v-icon color="green" small class="mr-1" v-bind="attrs" v-on="on">mdi-checkbox-marked-circle</v-icon></template><span>Présent</span></v-tooltip><span class="caption mr-3">P</span>
            <v-tooltip bottom><template v-slot:activator="{ on, attrs }"><v-icon color="red" small class="mr-1" v-bind="attrs" v-on="on">mdi-close-circle</v-icon></template><span>Absent</span></v-tooltip><span class="caption mr-3">A</span>
            <v-tooltip bottom><template v-slot:activator="{ on, attrs }"><v-icon color="orange" small class="mr-1" v-bind="attrs" v-on="on">mdi-alert-circle</v-icon></template><span>Excusé</span></v-tooltip><span class="caption mr-3">E</span>
            <v-tooltip bottom><template v-slot:activator="{ on, attrs }"><v-icon color="pink" small class="mr-1" v-bind="attrs" v-on="on">mdi-clock-alert</v-icon></template><span>Retard > 15min (0.5 Absence)</span></v-tooltip><span class="caption">R</span>
            <v-tooltip bottom><template v-slot:activator="{ on, attrs }"><v-icon color="blue" small class="mr-1" v-bind="attrs" v-on="on">mdi-beach</v-icon></template><span>Vacances (Neutre)</span></v-tooltip><span class="caption mr-3">V</span>
          </div>
        </div>
      </v-card-title>

      <v-card-text class="pa-0">
        <div class="table-container">
          <table class="presence-table">
            <thead>
              <tr>
                <th class="sticky-col name-col">Stagiaire</th>
                <th class="rate-col sticky-col sticky-rate-session">Taux Trimestre (%)</th>
                <th class="rate-col">Taux Période (4 Sem.)</th>
                <th
                  v-for="day in currentPeriodDays"
                  :key="day.fullDate"
                  class="day-header"
                  :class="{ 'weekend': day.isWeekend, 'month-view': true }"
                >
                  <div class="day-header-content">
                    <div class="day-name">{{ day.dayNameShort }}</div>
                    <div class="day-date">{{ day.date }}</div>
                    <div class="day-month-spacer">{{ day.monthShort }}</div>
                  </div>
                </th>
                <th class="stats-col">Statistiques</th>
                <th class="actions-col">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="stagiaire in filteredStagiaires"
                :key="stagiaire.stagiaireId"
                class="table-row"
              >
                <td class="sticky-col name-col">
                  <div class="stagiaire-info">
                    <div class="name">{{ stagiaire.prenom }} {{ stagiaire.nom }}</div>
                    <div class="trigramme">{{ stagiaire.username }}</div>
                    <div class="affiliation">{{ stagiaire.typeAffiliation?.libelle }}</div>
                    <div v-if="stagiaire.dateDebutParticipation" class="text-caption grey--text">
                       Début: {{ moment(stagiaire.dateDebutParticipation).format('DD/MM/YYYY') }}
                    </div>
                  </div>
                </td>

                <td 
                    class="rate-col sticky-col sticky-rate-session" 
                    :class="calculateQuarterlyRate(stagiaire).hasData ? getRateClass(calculateQuarterlyRate(stagiaire).rate) : 'grey--text'">
                    <div class="font-weight-bold">
                        {{ calculateQuarterlyRate(stagiaire).hasData ? calculateQuarterlyRate(stagiaire).rate.toFixed(1) + '%' : '-' }}
                    </div>
                    <div v-if="calculateQuarterlyRate(stagiaire).totalAbsencePenalty > 0" class="text-caption error--text" style="line-height: 1.2;">
                      <v-icon small color="red">mdi-clock-alert</v-icon> Pénalité: {{ calculateQuarterlyRate(stagiaire).totalAbsencePenalty.toFixed(1) }} Jours
                    </div>
                </td>
                
                <td 
                    class="rate-col" 
                    :class="calculateVisiblePeriodRate(stagiaire).hasData ? getRateClass(calculateVisiblePeriodRate(stagiaire).rate) : 'grey--text'">
                    <div class="font-weight-bold">
                        {{ calculateVisiblePeriodRate(stagiaire).hasData ? calculateVisiblePeriodRate(stagiaire).rate.toFixed(1) + '%' : '-' }}
                    </div>
                </td>


                <td
                  v-for="day in currentPeriodDays"
                  :key="day.fullDate"
                  @click="toggleStatus(stagiaire, day.fullDate)"
                  :class="[
                    'presence-cell',
                    getStatusClass(stagiaire, day.fullDate),
                    { 'weekend': day.isWeekend, 'month-view': true }
                  ]"
                >
                  <div class="presence-indicator">
                    {{ getStatusText(stagiaire, day.fullDate) }}
                  </div>
                </td>

                <td class="stats-col">
                  <div class="stats">
                    <div class="stat-item"><span class="stat-value present">{{ countStatus(stagiaire, 'p') }}</span><span class="stat-label">Présents</span></div>
                    <div class="stat-item"><span class="stat-value absent">{{ countStatus(stagiaire, 'a') }}</span><span class="stat-label">Absents</span></div>
                    <div class="stat-item"><span class="stat-value excused">{{ countStatus(stagiaire, 'e') }}</span><span class="stat-label">Excusés</span></div>
                    <div class="stat-item"><span class="stat-value retard">{{ countStatus(stagiaire, 'r') }}</span><span class="stat-label">Retards</span></div>
                    <div class="stat-item"><span class="stat-value vacances">{{ countStatus(stagiaire, 'v') }}</span><span class="stat-label">Vacances</span></div>
                  </div>
                </td>

                <td class="actions-col">
                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-btn
                        icon
                        color="primary"
                        @click="openModal(stagiaire)"
                        v-bind="attrs"
                        v-on="on"
                      >
                        <v-icon>mdi-file-document-outline</v-icon>
                      </v-btn>
                    </template>
                    <span>Générer Attestation PDF</span>
                  </v-tooltip>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </v-card-text>
    </v-card>
    
    <v-dialog v-model="specialDatesModalVisible" max-width="800px">
        <v-card>
            <v-card-title class="headline primary white--text">
                Gestion des Jours Fériés et Vacances
            </v-card-title>

            <v-card-text class="pa-4">
                <v-alert type="info" dense class="mb-4">
                    Ces dates sont utilisées pour exclure les jours de présence théoriquement attendus lors du calcul du Taux Session.
                    (Sauvegardé dans le navigateur pour cette démo.)
                </v-alert>

                <h3 class="title mb-2">Jours Fériés (Format YYYY-MM-DD, un par ligne)</h3>
                <v-textarea
                    v-model="joursFeriesInput"
                    outlined
                    dense
                    rows="5"
                    hide-details
                    placeholder="Ex: 2025-01-01"
                ></v-textarea>

                <h3 class="title mt-4 mb-2">Périodes de Vacances (Format JSON)</h3>
                <v-textarea
                    v-model="periodesVacancesInput"
                    outlined
                    dense
                    rows="5"
                    hide-details
                    placeholder='[{"start": "2025-07-07", "end": "2025-08-15"}]'
                ></v-textarea>
            </v-card-text>

            <v-card-actions class="pa-4">
                <v-spacer></v-spacer>
                <v-btn text @click="specialDatesModalVisible = false">Annuler</v-btn>
                <v-btn color="primary" @click="saveJoursFeries">
                    Sauvegarder & Recalculer
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>

    <v-dialog v-model="modalVisible" max-width="500px" persistent>
      <v-card :loading="isLoadingModal">
        <v-card-title class="headline primary white--text">
          <span>Générer une attestation Mensuelle</span>
          <v-spacer></v-spacer>
          <v-btn icon dark @click="modalVisible = false">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-4">
          <div class="text-subtitle-1 mb-2">
            Pour **{{ selectedStagiaire?.prenom }} {{ selectedStagiaire?.nom }}**
          </div>

          <v-select
            v-model="selectedYear"
            :items="availableYears"
            label="Année"
            outlined dense
            prepend-icon="mdi-calendar"
            :disabled="availableYears.length === 0"
            class="mb-3"
          />

          <v-select
            v-if="selectedYear"
            v-model="selectedMonth"
            :items="availableMonths"
            item-text="text"
            item-value="value"
            label="Mois"
            outlined dense
            prepend-icon="mdi-calendar-month"
            :disabled="availableMonths.length === 0"
          />

          <v-alert
            v-if="!isLoadingModal && availableYears.length === 0"
            type="info" dense class="mt-3"
          >
            Aucune présence notée pour ce stagiaire.
          </v-alert>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn text @click="modalVisible = false">Annuler</v-btn>
          <v-btn
            color="primary"
            dark
            :disabled="!selectedYear || !selectedMonth"
            @click="downloadAttestation"
            :loading="isLoadingModal"
          >
            <v-icon left>mdi-download</v-icon>
            Télécharger
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script>
import store from "@/store"
import { mapState } from "vuex"
import moment from "moment"
import 'moment/locale/fr'
import API_BASE from "@/services/api";

moment.locale('fr');

export default {
  data() {
    const defaultJoursFeries = [
          '2025-01-01', '2025-04-18', '2025-04-21', '2025-05-01', 
          '2025-05-29', '2025-06-09', '2025-09-18', '2025-12-25',
      ];
    const defaultPeriodesVacances = [
        { start: '2025-12-22', end: '2026-01-04' },
        { start: '2025-07-07', end: '2025-08-15' },
    ];
    
    return {
      search: "",
      filterStatus: [],
      currentPeriodStart: null,
      currentPeriodEnd: null,
      isLoading: false,
      isLoadingModal: false,

      modalVisible: false,
      selectedStagiaire: null,
      selectedYear: null,
      selectedMonth: null,
      availableYears: [],
      validMonths: [],
      
      // --- Gestion des dates spéciales depuis le Frontend ---
      specialDatesModalVisible: false,
      joursFeries: [], 
      periodesVacances: [], 
      joursFeriesInput: defaultJoursFeries.join('\n'),
      periodesVacancesInput: JSON.stringify(defaultPeriodesVacances, null, 2),
      // --------------------------------------------------------
    }
  },
  computed: {
    ...mapState("stagiaire", ["stagiaires"]),
    ...mapState(["settings"]),

    daysInView() {
      // Fixé à 28 jours pour la vue mensuelle (4 semaines)
      return 28
    },

    currentPeriodDays() {
      if (!this.currentPeriodStart) return []

      const days = []
      let currentDate = moment(this.currentPeriodStart).startOf('day')

      for (let i = 0; i < this.daysInView; i++) {
        const date = currentDate.clone().add(i, 'days').toDate()
        
        days.push({
          fullDate: moment(date).format('ddd MMM DD YYYY'), 
          date: moment(date).format('D'),
          dayName: moment(date).format('dddd'),
          dayNameShort: moment(date).format('ddd'),
          monthShort: moment(date).format('MMM'), 
          isWeekend: date.getDay() === 0 || date.getDay() === 6,
          isToday: moment().isSame(date, 'day')
        })
      }

      return days.filter(day => !day.isWeekend)
    },

    currentPeriodLabel() {
      if (!this.currentPeriodStart || !this.currentPeriodEnd) return ''

      const start = moment(this.currentPeriodStart)
      
      // Affiche le mois de début de la période
      return start.format('MMMM YYYY'); 
    },

    currentPeriodRange() {
      if (!this.currentPeriodStart || !this.currentPeriodEnd) return ''
      const start = moment(this.currentPeriodStart)
      const end = moment(this.currentPeriodEnd).subtract(1, 'day')
      
      return `Du ${start.format('DD MMM')} au ${end.format('DD MMM YYYY')}`
    },

    isCurrentPeriod() {
      const today = moment()
      const start = moment(this.currentPeriodStart)
      const end = moment(this.currentPeriodEnd)
      return today.isSameOrAfter(start, 'day') && today.isBefore(end, 'day')
    },

    filteredStagiaires() {
      let filtered = this.stagiaires

      if (this.search) {
        const s = this.search.toLowerCase()
        filtered = filtered.filter(st =>
          st.nom.toLowerCase().includes(s) ||
          st.prenom.toLowerCase().includes(s) ||
          st.username.toLowerCase().includes(s) ||
          (st.typeAffiliation?.libelle || "").toLowerCase().includes(s)
        )
      }

      if (this.filterStatus.length > 0) {
        const daysInPeriod = this.currentPeriodDays.map(day => day.fullDate)
        filtered = filtered.filter(st => {
          const currentPeriodStatuses = daysInPeriod.map(date =>
            st.presences?.[date]
          )
          return currentPeriodStatuses.some(status => this.filterStatus.includes(status))
        })
      }

      return filtered
    },

    availableMonths() {
      if (!this.selectedYear) return []

      const monthsData = [
        { text: 'Janvier', value: 1 }, { text: 'Février', value: 2 },
        { text: 'Mars', value: 3 }, { text: 'Avril', value: 4 },
        { text: 'Mai', value: 5 }, { text: 'Juin', value: 6 },
        { text: 'Juillet', value: 7 }, { text: 'Août', value: 8 },
        { text: 'Septembre', value: 9 }, { text: 'Octobre', value: 10 },
        { text: 'Novembre', value: 11 }, { text: 'Décembre', value: 12 },
      ]

      const validMonthValues = this.validMonths
        .filter(p => p.startsWith(`${this.selectedYear}-`))
        .map(p => parseInt(p.split("-")[1]))

      return monthsData.filter(m => validMonthValues.includes(m.value))
    },
  },

  created() {
    this.loadJoursFeriesFromStorage();
    if (!this.currentPeriodStart) {
      this.initializeCurrentPeriod()
    }
  },

  methods: {

    moment,
    // --- LOGIQUE DE NAVIGATION MENSUELLE FIXÉE ---
    getAnchorStart(date) {
      // Trouve la semaine ISO qui contient le 1er jour du mois
      return moment(date).startOf('month').startOf('isoWeek');
    },

    initializeCurrentPeriod() {
      const today = moment();
      let anchorStart = this.getAnchorStart(today);
      
      this.currentPeriodStart = anchorStart.toDate();
      this.currentPeriodEnd = anchorStart.clone().add(this.daysInView, 'days').toDate();
      
      this.fetchAttendancesForPeriod();
    },

    previousPeriod() {
      let newAnchor = moment(this.currentPeriodStart).subtract(1, 'month').startOf('month').startOf('isoWeek');
      
      this.currentPeriodStart = newAnchor.toDate();
      this.currentPeriodEnd = newAnchor.clone().add(this.daysInView, 'days').toDate();
      this.fetchAttendancesForPeriod();
    },

    nextPeriod() {
      let newAnchor = moment(this.currentPeriodStart).add(1, 'month').startOf('month').startOf('isoWeek');
      
      this.currentPeriodStart = newAnchor.toDate();
      this.currentPeriodEnd = newAnchor.clone().add(this.daysInView, 'days').toDate();
      this.fetchAttendancesForPeriod();
    },
    
    // --- LOGIQUE DE GESTION DES DATES SPÉCIALES ---
    loadJoursFeriesFromStorage() {
        try {
            // Utilise les valeurs par défaut si localStorage est vide
            const defaultJoursFeries = ['2025-01-01', '2025-04-18', '2025-04-21', '2025-05-01', '2025-05-29', '2025-06-09', '2025-09-18', '2025-12-25'];
            const defaultPeriodesVacances = [{ start: '2025-12-22', end: '2026-01-04' }, { start: '2025-07-07', end: '2025-08-15' }];

            const savedJoursFeries = localStorage.getItem('joursFeries');
            this.joursFeries = savedJoursFeries ? JSON.parse(savedJoursFeries) : defaultJoursFeries;
            
            const savedPeriodesVacances = localStorage.getItem('periodesVacances');
            this.periodesVacances = savedPeriodesVacances ? JSON.parse(savedPeriodesVacances) : defaultPeriodesVacances;
        } catch (e) {
            console.error("Erreur lors du chargement des dates spéciales depuis localStorage:", e);
        }
    },
    
    openSpecialDatesModal() {
        this.joursFeriesInput = this.joursFeries.join('\n');
        this.periodesVacancesInput = JSON.stringify(this.periodesVacances, null, 2);
        this.specialDatesModalVisible = true;
    },

    saveJoursFeries() {
        try {
            const newJoursFeries = this.joursFeriesInput
                .split('\n')
                .map(s => s.trim())
                .filter(s => s);
            
            const newPeriodesVacances = JSON.parse(this.periodesVacancesInput);

            if (newJoursFeries.some(d => !moment(d, 'YYYY-MM-DD', true).isValid())) {
                alert("Erreur: Au moins une date de jour férié est dans un format invalide (doit être YYYY-MM-DD).");
                return;
            }

            this.joursFeries = newJoursFeries;
            this.periodesVacances = newPeriodesVacances;
            
            localStorage.setItem('joursFeries', JSON.stringify(newJoursFeries));
            localStorage.setItem('periodesVacances', JSON.stringify(newPeriodesVacances));
            
            this.specialDatesModalVisible = false;
            this.fetchAttendancesForPeriod(); 

        } catch (e) {
            alert("Erreur lors de la sauvegarde des dates de vacances. Vérifiez le format JSON.");
            console.error("Erreur de formatage:", e);
        }
    },

    async fetchAttendancesForPeriod() {
      if (!this.currentPeriodStart || !this.currentPeriodEnd) return

      this.isLoading = true

      const startDate = moment(this.currentPeriodStart).format('YYYY-MM-DD')
      const endDate = moment(this.currentPeriodEnd).format('YYYY-MM-DD')

      const token = localStorage.getItem("access_token")
      const url = `${API_BASE}/api/attendance/byWeek?start=${startDate}&end=${endDate}`

      let fetchedAttendances = [];

      try {
        const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } })

        if (!res.ok) {
          const errorText = await res.text()
          throw new Error(`Erreur API ${res.status}: ${errorText || 'La requête a échoué.'}`)
        }

        const rawData = await res.json()

        if (Array.isArray(rawData)) {
          fetchedAttendances = rawData;
        } else {
          console.warn("L'API a renvoyé un format inattendu. Un tableau vide sera utilisé.");
        }

        await store.dispatch("stagiaire/updateStagiaireAttendances", {
          attendances: fetchedAttendances,
          currentPeriodDays: this.currentPeriodDays.map(d => d.fullDate)
        })

      } catch (err) {
        console.error("Échec du chargement des présences :", err.message || err)
        await store.dispatch("stagiaire/updateStagiaireAttendances", {
          attendances: [],
          currentPeriodDays: this.currentPeriodDays.map(d => d.fullDate)
        })

      } finally {
        this.isLoading = false
      }
    },
    
    // --- NOUVELLE LOGIQUE DE CALCUL DE TAUX ---
    
    getQuarterDates(anchorDate) {
        if (!anchorDate) return { start: null, end: null };

        const date = moment(anchorDate);
        const year = date.year();
        const month = date.month(); 

        let startMonth;
        let endMonth;
        
        // Détermination du trimestre civil (0=Jan, 1=Feb, 2=Mar, etc.)
        if (month >= 0 && month <= 2) { // Jan - Mar (Q1)
            startMonth = 0; endMonth = 2;
        } else if (month >= 3 && month <= 5) { // Apr - Jun (Q2)
            startMonth = 3; endMonth = 5;
        } else if (month >= 6 && month <= 8) { // Jul - Sep (Q3)
            startMonth = 6; endMonth = 8;
        } else { // Oct - Dec (Q4)
            startMonth = 9; endMonth = 11;
        }
        
        const quarterStart = moment([year, startMonth, 1]).startOf('month');
        const quarterEnd = moment([year, endMonth, 1]).endOf('month');

        return { 
            start: quarterStart.toDate(), 
            end: quarterEnd.clone().add(1, 'day').toDate() // Exclusif end
        };
    },

    /**
     * Calcule le taux de présence sur la PÉRIODE VISIBLE (4 semaines).
     */
    calculateVisiblePeriodRate(stagiaire) {
        const dates = {
            start: this.currentPeriodStart,
            end: this.currentPeriodEnd
        };
        return this.calculateRateForPeriod(stagiaire, dates);
    },

    /**
     * Calcule le taux de présence sur l'intégralité du TRIMESTRE (Session).
     */
    calculateQuarterlyRate(stagiaire) {
        const dates = this.getQuarterDates(this.currentPeriodStart);
        return this.calculateRateForPeriod(stagiaire, dates);
    },

    /**
     * Fonction générique de calcul du taux sur une période donnée (incluant toutes les règles).
     */
    calculateRateForPeriod(stagiaire, dates) {
        if (!dates.start || !dates.end) return { rate: 0, hasData: false, totalAbsencePenalty: 0 };
        
        const participationStartDate = stagiaire.dateDebutParticipation 
                                     ? moment(stagiaire.dateDebutParticipation).startOf('day')
                                     : moment(dates.start).startOf('day').subtract(1, 'year'); 
                                     
        const joursFeriesKeys = this.joursFeries;
        
        let achievedDaysNets = 0; 
        let joursNotésPendantPériodeThéorique = 0; 
        let hasPresenceData = false;
        let totalAbsencePenalty = 0; 

        if (dates.start && dates.end) {
            let currentDay = moment(dates.start);
            const endDay = moment(dates.end).subtract(1, 'day'); // Le 'end' est exclusif, on le rend inclusif pour l'itération

            while (currentDay.isSameOrBefore(endDay, 'day')) {
                const dayOfWeek = currentDay.isoWeekday(); 
                const dateKey = currentDay.format('ddd MMM DD YYYY');
                const dateString = currentDay.format('YYYY-MM-DD');
                const status = stagiaire.presences?.[dateKey]; 

                // 1. Déterminer si le jour est théoriquement attendu
                const isWorkingDay = dayOfWeek >= 1 && dayOfWeek <= 5;
                const isAfterParticipationStart = currentDay.isSameOrAfter(participationStartDate, 'day');
                const isFerie = joursFeriesKeys.includes(dateString);
                
                const isVacance = this.periodesVacances.some(vac => {
                    const start = moment(vac.start, 'YYYY-MM-DD').startOf('day');
                    const end = moment(vac.end, 'YYYY-MM-DD').endOf('day');
                    return currentDay.isBetween(start, end, 'day', '[]');
                });
                
                if (isWorkingDay && isAfterParticipationStart && !isFerie && !isVacance) {
                    if (status === 'v') {
        currentDay.add(1, 'day');
        continue;
    }
                    if (status && status !== "") { // Si un statut est noté (P, A, E, R)
                        joursNotésPendantPériodeThéorique++;
                        hasPresenceData = true;

                        if (status === 'p' || status === 'e') {
                            achievedDaysNets++; 
                        } else if (status === 'r') {
                            achievedDaysNets += 0.5; // 0.5 présence acquise
                            totalAbsencePenalty += 0.5;
                        } 
                    }
                }
                currentDay.add(1, 'day');
            }
        }
        
        const rate = joursNotésPendantPériodeThéorique > 0
            ? (achievedDaysNets / joursNotésPendantPériodeThéorique) * 100
            : 0;

        return { 
            rate: Math.min(100, rate),
            hasData: hasPresenceData,
            totalAbsencePenalty: totalAbsencePenalty
        };
    },
    // --- FIN LOGIQUE DE CALCUL DE TAUX ---

   async toggleStatus(stagiaire, dateString) {
  if (!stagiaire.presences) stagiaire.presences = {}
  const current = stagiaire.presences[dateString]
  
const cycleOptions = ["p", "a", "e", "r", "v", ""];
  let currentIndex = cycleOptions.indexOf(current);
  let newStatus = cycleOptions[(currentIndex + 1) % cycleOptions.length];

  // 1. Optimistic Update
  store.commit("stagiaire/SET_ATTENDANCE_STATUS", {
    stagiaireId: stagiaire.stagiaireId,
    date: dateString,
    status: newStatus
  })

  const token = localStorage.getItem("access_token")
  try {
    const payload = [{
      stagiaireId: stagiaire.stagiaireId,
      // CORRECTION DATE : Format strict sans timezone
      date: moment(dateString).format('YYYY-MM-DD'), 
      statut: newStatus // Assurez-vous que le backend accepte "" ou null
    }]

    const res = await fetch(`${API_BASE}/api/attendance/save`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify(payload)
    })

    if (!res.ok) {
      throw new Error(`Erreur sauvegarde`)
    }
    
    // On recharge pour être sûr d'avoir les données synchronisées
    await this.fetchAttendancesForPeriod();
    
  } catch (err) {
    console.error(err)
    // Rollback
    store.commit("stagiaire/SET_ATTENDANCE_STATUS", { 
        stagiaireId: stagiaire.stagiaireId,
        date: dateString,
        status: current
    })
  }
},

    getStatusText(stagiaire, date) {
      const s = stagiaire.presences?.[date];
      if (s === "r") return "R";
      if (s === "v") return "V"; 
      return s || "";
    },

    getStatusClass(stagiaire, date) {
      const s = stagiaire.presences?.[date]
      if (s === "p") return "present"
      if (s === "a") return "absent"
      if (s === "e") return "excused"
      if (s === "r") return "retard"
      if (s === "v") return "vacances" // Ajout
      return "empty"
    },

    countStatus(stagiaire, status) {
      const visibleDays = this.currentPeriodDays.map(day => day.fullDate);
      return visibleDays.filter(date => stagiaire.presences?.[date] === status).length
    },

    getMonth(date) {
      return moment(date).month() + 1
    },

    async fetchStagiaireStages(stagiaireId) {
      const token = localStorage.getItem("access_token")
      const url = `${API_BASE}/api/stages/ByStagiaire/${stagiaireId}`
      try {
        const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } })
        if (!res.ok) throw new Error("Erreur API stages")
        return await res.json()
      } catch (err) {
        console.error(err)
        return []
      }
    },

    async openModal(stagiaire) {
      this.modalVisible = true
      this.isLoadingModal = true
      this.selectedStagiaire = stagiaire
      this.selectedYear = null
      this.selectedMonth = null
      this.availableYears = []
      this.validMonths = []

      const months = new Set()
      const years = new Set()
      
      const allPresences = stagiaire.presences || {}; 

      Object.keys(allPresences).forEach(dateString => {
        const date = moment(dateString, 'ddd MMM DD YYYY');
        
        if (allPresences[dateString]) { 
          const year = date.year();
          const month = date.month() + 1;
          months.add(`${year}-${month}`);
          years.add(year);
        }
      });

      this.validMonths = Array.from(months)
      this.availableYears = Array.from(years).sort((a, b) => b - a)

      const currentYear = moment().year();
      const currentMonthValue = moment().month() + 1;

      if (this.availableYears.includes(currentYear)) {
          this.selectedYear = currentYear;
      } else if (this.availableYears.length > 0) {
          this.selectedYear = this.availableYears[0];
      }

      if (this.selectedYear) {
        if (this.validMonths.includes(`${this.selectedYear}-${currentMonthValue}`)) {
            this.selectedMonth = currentMonthValue;
        } else {
            const availableMonthsForYear = this.validMonths
                .filter(p => p.startsWith(`${this.selectedYear}-`))
                .map(p => parseInt(p.split("-")[1]));
            
            if (availableMonthsForYear.length > 0) {
                this.selectedMonth = availableMonthsForYear.reduce((a, b) => Math.max(a, b));
            }
        }
      }

      this.isLoadingModal = false
      
      if (this.availableYears.length === 0) {
          console.warn("Aucune présence enregistrée pour ce stagiaire.");
      }
    },

    async downloadAttestation() {
      this.isLoadingModal = true
      const stagiaireId = this.selectedStagiaire.stagiaireId
      const token = localStorage.getItem("access_token")

      const url = `${API_BASE}/api/attendance/attestation-mensuelle/${stagiaireId}?year=${this.selectedYear}&month=${this.selectedMonth}`

      try {
        const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } })
        if (!res.ok) {
          const errorText = await res.text()
          throw new Error(`Erreur lors du téléchargement du PDF: ${res.status} - ${errorText}`)
        }
        const blob = await res.blob()
        const link = document.createElement("a")
        link.href = window.URL.createObjectURL(blob)
        link.download = `attestation-mensuelle-${stagiaireId}-${this.selectedYear}-${this.selectedMonth}.pdf`
        link.click()
      } catch (err) {
        console.error(err)
      } finally {
        this.isLoadingModal = false
        this.modalVisible = false
      }
    },
    
    getRateClass(rate) {
        if (rate >= 80) return "success--text";
        if (rate >= 50) return "warning--text";
        return "error--text";
    },

  },

  beforeRouteEnter(routeTo, routeFrom, next) {
    store.dispatch("stagiaire/fetchStagiaires", true).then(() => {
      next(vm => {
        vm.fetchAttendancesForPeriod()
      })
    })
  }
}
</script>

<style scoped>

/* --- NOUVEAU STYLE POUR RETARD --- */
.stat-value.retard { color: #E91E63; } /* Pink color for Retard stat */
.presence-cell.retard { background-color: #F8BBD0; color: #E91E63; } /* Light pink cell */
/* --- FIN NOUVEAU STYLE --- */

.table-container {
  overflow-x: auto;
  max-height: 70vh;
}

.presence-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

/* Fixe les colonnes de nom et de taux */
.sticky-col {
  position: sticky;
  left: 0;
  background-color: white;
  z-index: 2;
  box-shadow: 2px 0 4px rgba(0,0,0,0.1);
}

.actions-col {
  position: sticky;
  right: 0;
  background-color: #f8f9fa;
  z-index: 2;
  box-shadow: -2px 0 4px rgba(0,0,0,0.1);
}

.rate-col {
  min-width: 120px;
  max-width: 120px;
  text-align: center;
}

/* Fixe le Taux Trimestre juste après le nom */
.rate-col.sticky-rate-session { 
    left: 200px;
    box-shadow: 2px 0 4px rgba(0,0,0,0.05);
    background-color: #f8f8f8;
    z-index: 2;
}

.timeline-days-container {
  flex-wrap: nowrap;
  overflow-x: hidden;
}

/* Ajustement des colonnes pour la vue 4 semaines */
.day-header,
.presence-cell {
  min-width: 45px;
  max-width: 45px;
  width: 45px;
}

.day-header,
.presence-table th,
.presence-table td {
  border: 1px solid #e0e0e0;
  text-align: center;
  padding: 8px 4px; 
}

.presence-table th {
    background-color: #f5f5f5;
    position: sticky;
    top: 0;
    z-index: 3;
}
.presence-table thead th.sticky-col {
    z-index: 4; 
}

.day-header.weekend {
  background-color: #f0f0f0;
}

.day-header-content {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.day-name {
  font-weight: 600;
  font-size: 0.75rem;
  color: #666;
}

.day-date {
  font-weight: 700;
  font-size: 1rem;
  color: #333;
}
.day-month-spacer {
  font-size: 0.7rem;
  color: #999;
}

.name-col {
  min-width: 200px;
  text-align: left;
  padding: 12px;
}

.stagiaire-info .name {
    font-weight: bold;
    color: #333;
}
.stagiaire-info .trigramme {
    font-size: 0.8rem;
    color: #888;
}
.stagiaire-info .affiliation {
    font-size: 0.7rem;
    color: #aaa;
}

.stats-col {
    min-width: 150px;
    padding: 8px 4px;
}

.stats {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    padding-left: 5px;
}
.stat-item {
    display: flex;
    justify-content: space-between;
    width: 100%;
    font-size: 0.75rem;
    line-height: 1.2;
}
.stat-label {
    margin-left: 5px;
    color: #666;
}
.stat-value {
    font-weight: bold;
}


.stat-value.present { color: #2E7D32; }
.stat-value.vacances { color: #039BE5; }
.stat-value.absent { color: #C62828; }
.stat-value.excused { color: #EF6C00; }
.presence-cell.present { background-color: #C8E6C9; color: #2E7D32; }
.presence-cell.absent { background-color: #FFCDD2; color: #C62828; }
.presence-cell.excused { background-color: #FFE0B2; color: #EF6C00; }
.presence-cell.empty { background-color: #F5F5F5; color: #9E9E9E; }
.presence-cell {
    cursor: pointer;
    transition: background-color 0.1s;
}
.presence-cell.vacances { 
    background-color: #E1F5FE; /* Bleu très clair */
    color: #0277BD; 
}
.presence-cell:hover {
    filter: brightness(0.95);
}


.timeline-day {
  flex-shrink: 0;
  padding: 8px 4px;
  border-radius: 8px;
  transition: background-color 0.2s;
  width: 5%; 
}

.timeline-day.today {
  background-color: #E3F2FD;
  color: #1976D2;
  font-weight: 600;
}
</style>