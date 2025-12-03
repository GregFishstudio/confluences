<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un stagiaire..."
            placeholder="Nom, Prénom, Trigramme, Affiliation..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          />
        </v-col>
      </v-row>
    </v-card>

    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="stagiaires"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        :options.sync="options"
        @update:items-per-page="updateNumberItems"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des stagiaires
            </v-toolbar-title>
          </v-toolbar>
        </template>

        <template v-slot:item.username="{ item }">
          <v-chip color="secondary lighten-5" text-color="secondary darken-4">
            {{ item.username }}
          </v-chip>
        </template>

        <template v-slot:item.nom="{ item }">
          <span class="text-uppercase">{{ item.nom }}</span>
        </template>

        <template v-slot:item.prenom="{ item }">
          <span class="text-capitalize">{{ item.prenom }}</span>
        </template>

        <template v-slot:item.typeAffiliation.libelle="{ item }">
          <v-chip small color="blue-grey lighten-5" text-color="blue-grey darken-3">
            {{ item.typeAffiliation.libelle }}
          </v-chip>
        </template>

        <template v-slot:item.documents="{ item }">
          <v-btn small color="primary" @click.stop="openModal(item)">
            ATTESTATIONS
          </v-btn>
        </template>
      </v-data-table>
    </v-card>

    <v-dialog v-model="modalVisible" max-width="500px">
      <v-card :loading="isLoading">
        <v-card-title class="headline">
          Documents de {{ selectedStagiaire?.prenom }} {{ selectedStagiaire?.nom }}
        </v-card-title>

        <v-card-text>
          <v-select
            v-model="selectedYear"
            :items="availableYears"
            label="Sélectionner une année (Stages disponibles)"
            outlined
            dense
            :disabled="availableYears.length === 0"
          >
          </v-select>
          <p v-if="!isLoading && availableYears.length === 0" class="caption red--text">
            Aucun stage trouvé pour ce stagiaire.
          </p>

          <v-select
            v-if="selectedYear"
            v-model="selectedTrimestre"
            :items="filteredTrimestres"
            label="Sélectionner un trimestre"
            outlined
            dense
            :disabled="filteredTrimestres.length === 0"
          />
        </v-card-text>

        <v-card-actions>
          <v-spacer />

          <v-btn text @click="modalVisible = false">Fermer</v-btn>

          <v-btn
            color="green darken-1"
            dark
            :disabled="!selectedYear || !selectedTrimestre"
            @click="downloadAttestation"
          >
            Télécharger PDF
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script>
import store from "@/store"
import { mapState } from "vuex"

export default {
  data() {
    return {
      options: {},
      search: "",
      isLoading: false, // Nouveau: pour l'état de chargement

      // Modale
      modalVisible: false,
      selectedStagiaire: null,

      selectedYear: null,
      selectedTrimestre: null,

      // Nouveau: Stocke les stages pour l'étudiant sélectionné
      stagiaireStages: [], 
      // Nouveau: Stocke les couples {year, trimestre} valides
      validPeriods: [], 

      availableYears: [],
      // Garde les trimestres dans l'ordre pour le filtrage
      availableTrimestres: ["T1", "T2", "T3", "T4"], 

      headers: [
        { text: "Trigramme", value: "username" },
        { text: "Prénom", value: "prenom" },
        { text: "Nom", value: "nom" },
        { text: "Affiliation", value: "typeAffiliation.libelle" },
        { text: "Documents", value: "documents", sortable: false }
      ]
    }
  },

  computed: {
    ...mapState("stagiaire", ["stagiaires"]),
    ...mapState(["settings"]),

    /* Trimestres filtrés par l'année sélectionnée */
    filteredTrimestres() {
      if (!this.selectedYear) return []

      // Filtrer validPeriods pour ne garder que ceux de l'année sélectionnée
      const validTrimestersForYear = this.validPeriods
        .filter(period => period.startsWith(`${this.selectedYear}-`))
        .map(period => period.split('-')[1]) // Extraire juste "T1", "T2", etc.

      // Retourner les trimestres disponibles dans l'ordre (T1, T2, T3, T4)
      return this.availableTrimestres.filter(t => validTrimestersForYear.includes(t))
    }
  },

  beforeRouteEnter(routeTo, routeFrom, next) {
    store.dispatch("stagiaire/fetchStagiaires", true).then(() => next())
  },

  created() {
    this.options.page = this.settings?.currentPageStagiaire || 1
  },

  methods: {
    /* 📅 CONVERTIR DATE EN TRIMESTRE */
    getTrimestre(date) {
      const month = date.getMonth() + 1 // Janvier = 1, Décembre = 12
      if (month >= 1 && month <= 3) return "T1"
      if (month >= 4 && month <= 6) return "T2"
      if (month >= 7 && month <= 9) return "T3"
      if (month >= 10 && month <= 12) return "T4"
      return null
    },

    /* 🌐 APPEL API pour les stages de l'étudiant */
    async fetchStagiaireStages(stagiaireId) {
      const token = localStorage.getItem("access_token")
      // Utilisation du endpoint corrigé dans la session précédente: api/stages/ByStagiaire/{stagiaireId}
      const url = `${API_BASE}/api/stages/ByStagiaire/${stagiaireId}` 

      try {
        const res = await fetch(url, {
          headers: { Authorization: `Bearer ${token}` }
        })
        if (!res.ok) throw new Error("Erreur de récupération des stages")
        return await res.json()
      } catch (error) {
        console.error("API Error:", error)
        return []
      }
    },

    /* 🔵 OUVERTURE MODALE */
    async openModal(stagiaire) {
      this.modalVisible = true
      this.isLoading = true // Début du chargement
      this.selectedStagiaire = stagiaire
      this.selectedYear = null
      this.selectedTrimestre = null
      this.stagiaireStages = []
      this.validPeriods = []
      this.availableYears = []

      // 1. Récupérer les stages
      const stages = await this.fetchStagiaireStages(stagiaire.stagiaireId)
      this.stagiaireStages = stages

      // 2. Analyser les périodes valides
      const periods = new Set()
      const uniqueYears = new Set()

      stages.forEach(stage => {
        // Le champ 'debut' est utilisé pour déterminer la période.
        const debutDate = new Date(stage.debut)
        const year = debutDate.getFullYear()
        const trimestre = this.getTrimestre(debutDate)

        if (trimestre) {
          // Stocke un identifiant unique (ex: "2024-T3")
          periods.add(`${year}-${trimestre}`)
          uniqueYears.add(year)
        }
      })

      // 3. Mettre à jour les listes de sélection
      this.validPeriods = Array.from(periods)
      this.availableYears = Array.from(uniqueYears).sort((a, b) => b - a)

      // Si des périodes sont disponibles, présélectionner l'année la plus récente
      if (this.availableYears.length > 0) {
        this.selectedYear = this.availableYears[0]
      }

      this.isLoading = false // Fin du chargement
    },

    /* 📄 TÉLÉCHARGER PDF */
    async downloadAttestation() {
      const stagiaireId = this.selectedStagiaire.stagiaireId
      const year = this.selectedYear
      const trimestre = this.selectedTrimestre

      const token = localStorage.getItem("access_token")

      // Utilisation du endpoint AttestationTrimestre dans le DocumentsController
      const url = `${API_BASE}/api/documents/attestation-trimestre/${stagiaireId}?year=${year}&trimestre=${trimestre}`

      const res = await fetch(url, {
        headers: { Authorization: `Bearer ${token}` }
      })

      if (!res.ok) {
        console.error("Erreur génération PDF:", res.status)
        // Vous pouvez ajouter une notification utilisateur ici
        return
      }

      const blob = await res.blob()
      const pdfURL = window.URL.createObjectURL(blob)

      const link = document.createElement("a")
      link.href = pdfURL
      link.download = `attestation-${stagiaireId}-${year}-${trimestre}.pdf`
      link.click()

      window.URL.revokeObjectURL(pdfURL)
    },

    updateNumberItems(value) {
      store.dispatch("settings/setItemsPerPage", { number: value })
    },

    updatePageSearch() {
      this.options.page = 1
    }
  }
}
</script>


<style scoped>
.stagiaires-table >>> tbody tr:hover {
  cursor: pointer;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.12);
  transform: translateY(-1px);
}
</style>