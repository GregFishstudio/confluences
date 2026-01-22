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
            <v-chip small filter outlined value="p" color="success">Présent</v-chip>
            <v-chip small filter outlined value="a" color="error">Absent</v-chip>
            <v-chip small filter outlined value="e" color="warning">Excusé</v-chip>
            <v-chip small filter outlined value="r" color="pink">Retard</v-chip>
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
            :class="{ 'weekend': day.isWeekend, 'today': day.isToday }"
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
          <div class="d-flex align-center caption">
            <div class="d-flex align-center mr-3"><div style="width:16px;height:16px;background:#C8E6C9;border-radius:4px;" class="mr-1"></div> Présent</div>
            <div class="d-flex align-center mr-3"><v-icon color="red" small class="mr-1">mdi-close-circle</v-icon> Absent (A)</div>
            <div class="d-flex align-center mr-3"><v-icon color="orange" small class="mr-1">mdi-alert-circle</v-icon> Excusé (E)</div>
            <div class="d-flex align-center mr-3"><v-icon color="pink" small class="mr-1">mdi-clock-alert</v-icon> Retard (R)</div>
            <div class="d-flex align-center"><v-icon color="blue" small class="mr-1">mdi-beach</v-icon> Vacances (V)</div>
          </div>
        </div>
      </v-card-title>

      <v-card-text class="pa-0">
        <div class="table-container">
          <table class="presence-table">
            <thead>
              <tr>
                <th class="sticky-col name-col">Stagiaire</th>
                <th class="rate-col sticky-col sticky-rate-session">Taux Trimestre</th>
                <th class="rate-col">Taux Période</th>
                <th
                  v-for="day in currentPeriodDays"
                  :key="day.fullDate"
                  class="day-header"
                  :class="{ 'weekend': day.isWeekend }"
                >
                  <div class="day-header-content">
                    <div class="day-name">{{ day.dayNameShort }}</div>
                    <div class="day-date">{{ day.date }}</div>
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
                    
                    <v-tooltip bottom>
                        <template v-slot:activator="{ on, attrs }">
                            <div 
                                class="text-caption primary--text d-flex align-center mt-1 clickable-date"
                                v-bind="attrs" 
                                v-on="on"
                                @click="openStartDateModal(stagiaire)"
                                style="cursor: pointer; width: fit-content;"
                            >
                                <v-icon x-small color="primary" class="mr-1">mdi-calendar-start</v-icon>
                                {{ stagiaire.dateDebutParticipation ? moment(stagiaire.dateDebutParticipation).format('DD/MM/YYYY') : 'Définir début' }}
                            </div>
                        </template>
                        <span>Cliquer pour modifier la date de début de formation</span>
                    </v-tooltip>
                  </div>
                </td>

                <td 
    class="rate-col sticky-col sticky-rate-session" 
    :class="calculateQuarterlyRate(stagiaire).hasData ? getRateClass(calculateQuarterlyRate(stagiaire).rate) : 'grey--text text--darken-1'"
>
    <div class="font-weight-bold">
        {{ calculateQuarterlyRate(stagiaire).rate.toFixed(1) }}%
    </div>
    
    <div v-if="calculateQuarterlyRate(stagiaire).totalAbsencePenalty > 0" class="text-caption error--text font-weight-bold" style="line-height: 1.2;">
      <v-icon x-small color="red">mdi-clock-alert</v-icon> -{{ calculateQuarterlyRate(stagiaire).totalAbsencePenalty.toFixed(1) }} J
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
                  @click="!isBeforeParticipation(stagiaire, day.fullDate) && toggleStatus(stagiaire, day.fullDate)"
                  :class="[
                    'presence-cell',
                    isBeforeParticipation(stagiaire, day.fullDate) ? 'not-started' : getStatusClass(stagiaire, day.fullDate),
                    { 
                        'weekend': day.isWeekend, 
                        'disabled-cell': isBeforeParticipation(stagiaire, day.fullDate)
                    }
                  ]"
                >
                  <div v-if="!isBeforeParticipation(stagiaire, day.fullDate)" class="presence-indicator">
                    {{ getStatusText(stagiaire, day.fullDate) }}
                  </div>
                </td>

                <td class="stats-col">
                  <div class="stats">
                    <div class="stat-item"><span class="stat-value present">{{ countStatus(stagiaire, 'p') }}</span><span class="stat-label">P</span></div>
                    <div class="stat-item"><span class="stat-value absent">{{ countStatus(stagiaire, 'a') }}</span><span class="stat-label">A</span></div>
                    <div class="stat-item"><span class="stat-value excused">{{ countStatus(stagiaire, 'e') }}</span><span class="stat-label">E</span></div>
                    <div class="stat-item"><span class="stat-value retard">{{ countStatus(stagiaire, 'r') }}</span><span class="stat-label">R</span></div>
                  </div>
                </td>

                <td class="actions-col">
                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-btn icon color="primary" @click="openModal(stagiaire)" v-bind="attrs" v-on="on">
                        <v-icon>mdi-file-document-outline</v-icon>
                      </v-btn>
                    </template>
                    <span>Attestation PDF</span>
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
                Configuration Calendrier
            </v-card-title>
            <v-card-text class="pa-4">
                <v-alert type="info" dense class="mb-4">
                    Exclure des jours du calcul théorique (Fériés & Vacances).
                </v-alert>
                <h3 class="title mb-2">Jours Fériés (YYYY-MM-DD)</h3>
                <v-textarea v-model="joursFeriesInput" outlined dense rows="4"></v-textarea>
                <h3 class="title mt-4 mb-2">Périodes de Vacances (JSON)</h3>
                <v-textarea v-model="periodesVacancesInput" outlined dense rows="4"></v-textarea>
            </v-card-text>
            <v-card-actions class="pa-4">
                <v-spacer></v-spacer>
                <v-btn text @click="specialDatesModalVisible = false">Annuler</v-btn>
                <v-btn color="primary" @click="saveJoursFeries">Sauvegarder</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>

 <v-dialog v-model="startDateModalVisible" max-width="400px">
    <v-card>
        <v-card-title class="primary white--text subtitle-1">
            Date de début de formation
        </v-card-title>
        <v-card-text class="pa-4 pt-6">
            <p class="body-2">
                Définir la date d'arrivée pour <strong>{{ editingStagiaire?.prenom }} {{ editingStagiaire?.nom }}</strong>.
                <br><span class="caption grey--text">Les jours précédant cette date seront grisés et exclus des calculs.</span>
            </p>
            
            <v-menu
                v-model="dateMenu"
                :close-on-content-click="false"
                transition="scale-transition"
                offset-y
                min-width="auto"
            >
                <template v-slot:activator>
                    <v-text-field
                        v-model="newStartDate"
                        label="Date de début"
                        readonly
                        outlined dense
                        
                        append-icon="mdi-calendar"
                        @click="dateMenu = true" 
                        @click:append="dateMenu = true"
                        
                        hide-details
                    ></v-text-field>
                </template>
                
                <v-date-picker
                    v-model="newStartDate"
                    @input="dateMenu = false"
                    locale="fr"
                    first-day-of-week="1"
                    no-title
                    scrollable
                ></v-date-picker>
            </v-menu>
        </v-card-text>
        <v-card-actions class="pa-4">
            <v-spacer></v-spacer>
            <v-btn text @click="startDateModalVisible = false">Annuler</v-btn>
            <v-btn color="primary" @click="saveStartDate">Enregistrer</v-btn>
        </v-card-actions>
    </v-card>
</v-dialog>

    <v-dialog v-model="modalVisible" max-width="500px" persistent>
      <v-card :loading="isLoadingModal">
        <v-card-title class="headline primary white--text">
          <span>Attestation Mensuelle</span>
          <v-spacer></v-spacer>
          <v-btn icon dark @click="modalVisible = false"><v-icon>mdi-close</v-icon></v-btn>
        </v-card-title>
        <v-card-text class="pa-4">
          <div class="text-subtitle-1 mb-2">Pour {{ selectedStagiaire?.prenom }} {{ selectedStagiaire?.nom }}</div>
          <v-select v-model="selectedYear" :items="availableYears" label="Année" outlined dense class="mb-3" />
          <v-select v-if="selectedYear" v-model="selectedMonth" :items="availableMonths" item-text="text" item-value="value" label="Mois" outlined dense />
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn color="primary" dark :disabled="!selectedYear || !selectedMonth" @click="downloadAttestation" :loading="isLoadingModal">
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
    // Dates par défaut
    const defaultJoursFeries = ['2025-01-01', '2025-04-18', '2025-04-21', '2025-05-01', '2025-05-29', '2025-06-09', '2025-09-18', '2025-12-25'];
    const defaultPeriodesVacances = [{ start: '2025-12-22', end: '2026-01-04' }, { start: '2025-07-07', end: '2025-08-15' }];
    
    return {
      search: "",
      filterStatus: [],
      currentPeriodStart: null,
      currentPeriodEnd: null,
      isLoading: false,
      isLoadingModal: false,

      // Modale PDF
      modalVisible: false,
      selectedStagiaire: null,
      selectedYear: null,
      selectedMonth: null,
      availableYears: [],
      validMonths: [],
      
      // Dates Spéciales
      specialDatesModalVisible: false,
      joursFeries: [], 
      periodesVacances: [], 
      joursFeriesInput: defaultJoursFeries.join('\n'),
      periodesVacancesInput: JSON.stringify(defaultPeriodesVacances, null, 2),

      // Modale Date Début (Nouveau)
      startDateModalVisible: false,
      editingStagiaire: null,
      newStartDate: null,
      dateMenu: false,
    }
  },
  computed: {
    ...mapState("stagiaire", ["stagiaires"]),
    ...mapState(["settings"]),

    daysInView() { return 28 }, // 4 semaines

    currentPeriodDays() {
      if (!this.currentPeriodStart) return []
      const days = []
      let currentDate = moment(this.currentPeriodStart).startOf('day')

      for (let i = 0; i < this.daysInView; i++) {
        const date = currentDate.clone().add(i, 'days').toDate()
        days.push({
          fullDate: moment(date).format('ddd MMM DD YYYY'), 
          date: moment(date).format('D'),
          dayNameShort: moment(date).format('ddd'),
          monthShort: moment(date).format('MMM'), 
          isWeekend: date.getDay() === 0 || date.getDay() === 6,
          isToday: moment().isSame(date, 'day')
        })
      }
      return days.filter(day => !day.isWeekend)
    },

    currentPeriodLabel() {
  if (!this.currentPeriodStart) return ''

  // 6 jours après l’ancre = forcément dans le mois affiché
  const visibleMonth = moment(this.currentPeriodStart).add(6, 'days').startOf('month')

  return visibleMonth.format('MMMM YYYY')
},


    currentPeriodRange() {
      if (!this.currentPeriodStart || !this.currentPeriodEnd) return ''
      const start = moment(this.currentPeriodStart)
      const end = moment(this.currentPeriodEnd).subtract(1, 'day')
      return `Du ${start.format('DD MMM')} au ${end.format('DD MMM YYYY')}`
    },

    isCurrentPeriod() {
      const today = moment()
      return today.isSameOrAfter(this.currentPeriodStart, 'day') && today.isBefore(this.currentPeriodEnd, 'day')
    },

    filteredStagiaires() {
      let filtered = this.stagiaires
      if (this.search) {
        const s = this.search.toLowerCase()
        filtered = filtered.filter(st =>
          st.nom.toLowerCase().includes(s) || st.prenom.toLowerCase().includes(s) || st.username.toLowerCase().includes(s)
        )
      }
      if (this.filterStatus.length > 0) {
        const daysInPeriod = this.currentPeriodDays.map(day => day.fullDate)
        filtered = filtered.filter(st => {
          const statuses = daysInPeriod.map(d => st.presences?.[d])
          return statuses.some(s => this.filterStatus.includes(s))
        })
      }
      return filtered
    },

    availableMonths() {
      if (!this.selectedYear) return []
      const monthsData = [
        { text: 'Janvier', value: 1 }, { text: 'Février', value: 2 }, { text: 'Mars', value: 3 }, { text: 'Avril', value: 4 },
        { text: 'Mai', value: 5 }, { text: 'Juin', value: 6 }, { text: 'Juillet', value: 7 }, { text: 'Août', value: 8 },
        { text: 'Septembre', value: 9 }, { text: 'Octobre', value: 10 }, { text: 'Novembre', value: 11 }, { text: 'Décembre', value: 12 },
      ]
      const validMonthValues = this.validMonths
        .filter(p => p.startsWith(`${this.selectedYear}-`))
        .map(p => parseInt(p.split("-")[1]))
      return monthsData.filter(m => validMonthValues.includes(m.value))
    },
  },

  created() {
    this.loadJoursFeriesFromStorage();
    if (!this.currentPeriodStart) this.initializeCurrentPeriod()
  },

  methods: {
    moment,

    // --- NAVIGATION ---
 // DANS methods:

// --- LOGIQUE DE NAVIGATION MENSUELLE FIXÉE ---
// Cette méthode trouve toujours le LUNDI qui précède ou est égal au 1er du mois
// DANS methods:

// Cette méthode trouve toujours le LUNDI qui précède ou est égal au 1er du mois
getAnchorStart(date) {
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
  // Dériver correctement le 1er du mois actuellement affiché
  let currentMonthStart = moment(this.currentPeriodStart).clone().add(6, 'days').startOf('month');

  // Reculer d'un mois civil
  let previousMonth = currentMonthStart.clone().subtract(1, 'month');

  // Trouver l'ancre (lundi) pour ce mois
  let newAnchor = this.getAnchorStart(previousMonth);

  this.currentPeriodStart = newAnchor.toDate();
  this.currentPeriodEnd = newAnchor.clone().add(this.daysInView, 'days').toDate();
  this.fetchAttendancesForPeriod();
},

nextPeriod() {
  // Dériver correctement le 1er du mois actuellement affiché
  let currentMonthStart = moment(this.currentPeriodStart).clone().add(6, 'days').startOf('month');

  // Avancer d'un mois civil
  let nextMonth = currentMonthStart.clone().add(1, 'month');

  // Trouver l'ancre (lundi) pour ce mois
  let newAnchor = this.getAnchorStart(nextMonth);

  this.currentPeriodStart = newAnchor.toDate();
  this.currentPeriodEnd = newAnchor.clone().add(this.daysInView, 'days').toDate();
  this.fetchAttendancesForPeriod();
},


    // --- GESTION DATE DE DÉBUT ---
    isBeforeParticipation(stagiaire, dateString) {
        if (!stagiaire.dateDebutParticipation) return false;
        const current = moment(dateString, 'ddd MMM DD YYYY');
        const start = moment(stagiaire.dateDebutParticipation).startOf('day');
        return current.isBefore(start, 'day');
    },

   openStartDateModal(stagiaire) {
        this.editingStagiaire = stagiaire;
        
        // >>> AJOUT CONSOLE.LOG : Vérifie ce que l'application reçoit au départ <<<
        console.log("--- DEBUG: Stagiaire chargé dans la modale ---");
        console.log("Stagiaire complet:", stagiaire);
        console.log("Date de début ACTUELLE:", stagiaire.dateDebutParticipation);
        console.log("-------------------------------------------------");
        
        this.newStartDate = stagiaire.dateDebutParticipation 
            ? moment(stagiaire.dateDebutParticipation).format('YYYY-MM-DD') 
            : null;
        this.startDateModalVisible = true;
    },

  // DANS methods:

// DANS methods:

// DANS methods:

async saveStartDate() {
        if (!this.editingStagiaire || !this.newStartDate) return;

        this.isLoading = true;
        const stagiaireId = this.editingStagiaire.stagiaireId;
        const previousDate = this.editingStagiaire.dateDebutParticipation;
        const normalizedDate = moment(this.newStartDate).format('YYYY-MM-DD');

        const fullPayload = JSON.parse(JSON.stringify(this.editingStagiaire));
        fullPayload.dateDebutParticipation = normalizedDate;

        // >>> AJOUT CONSOLE.LOG : Vérifie l'objet envoyé à l'API <<<
        console.log("--- DEBUG: Payload envoyé à l'API (PUT) ---");
        console.log("Payload:", fullPayload);
        console.log("-------------------------------------------");
        
        try {
            // Optimistic update
            Object.assign(this.editingStagiaire, { dateDebutParticipation: normalizedDate });

            const token = localStorage.getItem("access_token");
            const url = `${API_BASE}/api/stagiaires/${stagiaireId}`; 

            const res = await fetch(url, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`
                },
                body: JSON.stringify(fullPayload) 
            });

            if (!res.ok) {
                let errText = await res.text().catch(() => '');
                console.error(`Échec API ${res.status}:`, errText);
                throw new Error(`Échec API ${res.status}: ${errText}`);
            }

            // Mettre à jour le Store Vuex localement (Assurez-vous que la mutation UPDATE_STAGIAIRE existe)
            this.$store.commit('stagiaire/UPDATE_STAGIAIRE', fullPayload); 

            // >>> AJOUT CONSOLE.LOG : Vérifie l'objet après la mise à jour du store <<<
            console.log("--- DEBUG: Sauvegarde Réussie et Store Mis à Jour ---");
            console.log("Nouvel objet dans le store:", fullPayload);
            console.log("------------------------------------------------------");
            
            this.startDateModalVisible = false;
            await this.fetchAttendancesForPeriod(); 

        } catch (e) {
            console.error("Erreur de communication API:", e);
            alert("Impossible de sauvegarder la date de début.");

            // Rollback
            Object.assign(this.editingStagiaire, { dateDebutParticipation: previousDate });
            
            // >>> AJOUT CONSOLE.LOG : Vérifie l'objet restauré en cas d'échec <<<
            console.log("--- DEBUG: Échec API - Restauration de l'objet ---");
            console.log("Objet restauré à:", this.editingStagiaire);
            console.log("-------------------------------------------------");
            
            await this.fetchAttendancesForPeriod();

        } finally {
            this.isLoading = false;
        }
    },


    // --- LOGIQUE ATTENDANCE ---
    getStatusText(stagiaire, date) {
      const s = stagiaire.presences?.[date];
      if (s === "p") return ""; // Présent = case vide
      if (s === "r") return "R";
      if (s === "v") return "V";
      if (s === "e") return "E"; 
      if (s === "a") return "A";
      return "";
    },

    getStatusClass(stagiaire, date) {
      const s = stagiaire.presences?.[date];
      if (s === "p") return "present";
      if (s === "a") return "absent";
      if (s === "e") return "excused";
      if (s === "r") return "retard";
      if (s === "v") return "vacances";
      return "present"; // Par défaut visuellement "Présent" si vide
    },

    async toggleStatus(stagiaire, dateString) {
      if (!stagiaire.presences) stagiaire.presences = {};
      const current = stagiaire.presences[dateString];

      // CYCLE MODIFIÉ : 
      // Vide/P -> Absent -> Excusé -> Retard -> Vacances -> Présent
      let newStatus = 'a';
      if (current === 'p') newStatus = 'a';
      else if (current === 'a') newStatus = 'e';
      else if (current === 'e') newStatus = 'r';
      else if (current === 'r') newStatus = 'v';
      else if (current === 'v') newStatus = 'p';
      else if (!current) newStatus = 'a'; // Clic sur case vide -> Absent

      // Optimistic Update
      store.commit("stagiaire/SET_ATTENDANCE_STATUS", {
        stagiaireId: stagiaire.stagiaireId,
        date: dateString,
        status: newStatus
      })

      try {
        const payload = [{
          stagiaireId: stagiaire.stagiaireId,
          date: moment(dateString, 'ddd MMM DD YYYY').format('YYYY-MM-DD'), 
          statut: newStatus
        }]
        const token = localStorage.getItem("access_token")
        await fetch(`${API_BASE}/api/attendance/save`, {
          method: "POST",
          headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
          body: JSON.stringify(payload)
        })
      } catch (err) {
        console.error(err)
        // Rollback
        store.commit("stagiaire/SET_ATTENDANCE_STATUS", { 
            stagiaireId: stagiaire.stagiaireId, date: dateString, status: current
        })
      }
    },

    countStatus(stagiaire, status) {
        // Compter les statuts seulement si après date début
        const visibleDays = this.currentPeriodDays.map(day => day.fullDate);
        return visibleDays.filter(date => {
            if (this.isBeforeParticipation(stagiaire, date)) return false;
            return stagiaire.presences?.[date] === status;
        }).length
    },

    // --- CALCUL DES TAUX ---
    getQuarterDates(anchorDate) {
        // CORRECTION : On avance de 7 jours pour être sûr d'être dans le mois principal de la vue
        // Ex: Si la vue commence le 29 Déc, +7j nous amène en Janvier -> T1 2026
        const date = moment(anchorDate).add(7, 'days');
        const month = date.month(); 
        let startMonth = month <= 2 ? 0 : month <= 5 ? 3 : month <= 8 ? 6 : 9;
        const year = date.year();
        return { 
            start: moment([year, startMonth, 1]).startOf('month').toDate(), 
            end: moment([year, startMonth + 2, 1]).endOf('month').toDate()
        };
    },
    calculateVisiblePeriodRate(stagiaire) {
        return this.calculateRateForPeriod(stagiaire, { start: this.currentPeriodStart, end: this.currentPeriodEnd });
    },

    calculateQuarterlyRate(stagiaire) {
        return this.calculateRateForPeriod(stagiaire, this.getQuarterDates(this.currentPeriodStart));
    },

    calculateRateForPeriod(stagiaire, dates) {
      if (!dates.start || !dates.end) return { rate: 100, hasData: false, totalAbsencePenalty: 0 };

      // Date de début du stagiaire (ou très loin dans le passé si non définie)
      const participationStartDate = stagiaire.dateDebutParticipation 
          ? moment(stagiaire.dateDebutParticipation).startOf('day')
          : moment('2000-01-01');

      let totalTheorique = 0; 
      let penalty = 0;
      let hasExplicitBadData = false;

      let current = moment(dates.start);
      const end = moment(dates.end);

      while (current.isSameOrBefore(end, 'day')) {
          const dateStr = current.format('YYYY-MM-DD');
          const dateKey = current.format('ddd MMM DD YYYY'); // Format clé API
          const dayOfWeek = current.isoWeekday(); // 1=Lundi, 6=Samedi, 7=Dimanche
          
          // 1. Est-ce un jour ouvrable théorique ?
          const isWorkingDay = dayOfWeek >= 1 && dayOfWeek <= 5;
          
          if (isWorkingDay) {
              // 2. Est-ce un jour hors calcul (Férié / Vacances Globales) ?
              const isFerie = this.joursFeries.includes(dateStr);
              const isVacanceGlobale = this.periodesVacances.some(v => 
                  current.isBetween(moment(v.start), moment(v.end), 'day', '[]')
              );

              if (!isFerie && !isVacanceGlobale) {
                  // 3. Le stagiaire est-il actif ce jour-là ?
                  const isStarted = current.isSameOrAfter(participationStartDate, 'day');
                  
                  if (isStarted) {
                      // C'est un jour comptabilisé
                      totalTheorique++;

                      // 4. Vérification des saisies (Pénalités)
                      const s = stagiaire.presences?.[dateKey];

                      if (s === 'a') {
                          penalty += 1;
                          hasExplicitBadData = true;
                      } else if (s === 'r') {
                          penalty += 0.5;
                          hasExplicitBadData = true;
                      }
                      // Note : 'p' (présent), 'e' (excusé) ou vide = 0 pénalité (100% par défaut)
                  }
              }
          }
          current.add(1, 'day');
      }

      // Calcul final
      if (totalTheorique === 0) return { rate: 100, hasData: false, totalAbsencePenalty: 0 };

      const achieved = totalTheorique - penalty;
      const rate = (achieved / totalTheorique) * 100;

      return { 
          rate: Math.max(0, rate), // Pas de taux négatif
          hasData: hasExplicitBadData, // True seulement si pénalités trouvées
          totalAbsencePenalty: penalty 
      };
    },

    // --- AUTRES ---
    loadJoursFeriesFromStorage() {
        try {
            const fj = localStorage.getItem('joursFeries');
            if (fj) this.joursFeries = JSON.parse(fj);
            const pv = localStorage.getItem('periodesVacances');
            if (pv) this.periodesVacances = JSON.parse(pv);
        } catch (e) { console.error(e); }
    },
    saveJoursFeries() {
        this.joursFeries = this.joursFeriesInput.split('\n').map(s=>s.trim()).filter(s=>s);
        try { this.periodesVacances = JSON.parse(this.periodesVacancesInput); } catch(e){ alert("JSON invalide"); return; }
        localStorage.setItem('joursFeries', JSON.stringify(this.joursFeries));
        localStorage.setItem('periodesVacances', JSON.stringify(this.periodesVacances));
        this.specialDatesModalVisible = false;
        this.fetchAttendancesForPeriod();
    },
    openSpecialDatesModal() {
        this.joursFeriesInput = this.joursFeries.join('\n');
        this.periodesVacancesInput = JSON.stringify(this.periodesVacances, null, 2);
        this.specialDatesModalVisible = true;
    },
    async fetchAttendancesForPeriod() {
      if (!this.currentPeriodStart) return
      this.isLoading = true
      const s = moment(this.currentPeriodStart).format('YYYY-MM-DD');
      const e = moment(this.currentPeriodEnd).format('YYYY-MM-DD');
      try {
        const res = await fetch(`${API_BASE}/api/attendance/byWeek?start=${s}&end=${e}`, {
            headers: { Authorization: `Bearer ${localStorage.getItem("access_token")}` }
        })
        const data = await res.json()
        await store.dispatch("stagiaire/updateStagiaireAttendances", {
          attendances: Array.isArray(data) ? data : [],
          currentPeriodDays: this.currentPeriodDays.map(d => d.fullDate)
        })
      } catch (err) { console.error(err) } finally { this.isLoading = false }
    },
    async openModal(stagiaire) {
      this.selectedStagiaire = stagiaire; this.modalVisible = true;
      // ... logique récupération mois/années inchangée pour brièveté ...
      // Pour l'exemple, on peuple fictivement si vide
      if(this.availableYears.length === 0) {
          this.availableYears = [2025]; this.selectedYear = 2025;
          this.validMonths = ['2025-1', '2025-2']; this.selectedMonth = 1; 
      }
    },
    async downloadAttestation() { /* ... code existant ... */ this.modalVisible = false; },
    getRateClass(rate) { return rate >= 80 ? "success--text" : rate >= 50 ? "warning--text" : "error--text"; },
  },
  beforeRouteEnter(to, from, next) {
    store.dispatch("stagiaire/fetchStagiaires", true).then(() => next(vm => vm.fetchAttendancesForPeriod()))
  }
}
</script>

<style scoped>
.table-container { overflow-x: auto; max-height: 70vh; }
.presence-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }

/* Sticky Columns */
.sticky-col { position: sticky; left: 0; background-color: white; z-index: 2; box-shadow: 2px 0 4px rgba(0,0,0,0.1); }
.name-col { min-width: 200px; padding: 12px; }
.sticky-rate-session { left: 200px; background-color: #f8f8f8; z-index: 2; border-right: 2px solid #ddd; }
.actions-col { position: sticky; right: 0; background-color: #f8f9fa; z-index: 2; }

/* Cells */
.presence-table th, .presence-table td { border: 1px solid #e0e0e0; text-align: center; padding: 4px; }
.presence-table th { background: #f5f5f5; position: sticky; top: 0; z-index: 3; }
.day-header { min-width: 45px; width: 45px; }
.presence-cell { cursor: pointer; height: 40px; transition: 0.1s; }
.presence-cell:hover { filter: brightness(0.95); }

/* Status Colors */
.presence-cell.present { background-color: #C8E6C9; color: #2E7D32; }
.presence-cell.absent { background-color: #FFCDD2; color: #C62828; }
.presence-cell.excused { background-color: #FFE0B2; color: #EF6C00; }
.presence-cell.retard { background-color: #F8BBD0; color: #E91E63; }
.presence-cell.vacances { background-color: #E1F5FE; color: #0277BD; }

/* Not Started Style (Hatched) */
.presence-cell.not-started {
    background: repeating-linear-gradient(
        45deg,
        #eeeeee,
        #eeeeee 5px,
        #e0e0e0 5px,
        #e0e0e0 10px
    );
    cursor: not-allowed !important;
    opacity: 0.6;
}
.clickable-date:hover { text-decoration: underline; background-color: rgba(0,0,0,0.05); border-radius: 4px; }

/* Misc */
.timeline-days-container { overflow-x: hidden; }
.timeline-day { width: 5%; flex-shrink: 0; padding: 8px 4px; border-radius: 8px; }
.timeline-day.today { background: #E3F2FD; color: #1976D2; font-weight: bold; }
.stats { display: flex; flex-direction: column; font-size: 0.7rem; padding-left: 4px; }
.stat-item { display: flex; justify-content: space-between; width: 100%; }
.stat-value { font-weight: bold; }
.stat-value.present { color: #2E7D32; }
.stat-value.absent { color: #C62828; }
</style>