<template>
  <v-dialog :value="visible" @input="$emit('update:visible', $event)" max-width="800px" persistent>
    <v-card :loading="isLoading">
     <v-card-title class="headline primary white--text">
    <span>
        Édition du Bilan de Mesure 
        <span v-if="stagiaireData.nom" class="font-weight-light ml-2 text-caption">
            ({{ stagiaireData.prenom }} {{ stagiaireData.nom }} - {{ periodInfo.label }} {{ periodInfo.year }})
        </span>
    </span>
    <v-spacer></v-spacer>
    <v-btn icon dark @click="$emit('update:visible', false)">
        <v-icon>mdi-close</v-icon>
    </v-btn>
</v-card-title>

<v-card-text class="pa-6">
    <div class="text-h6 mb-2">
        Bilan pour **{{ stagiaireData.prenom }} {{ stagiaireData.nom }}**
        <span class="text-subtitle-1 grey--text">
             (Période : {{ periodInfo.label }} {{ periodInfo.year }})
        </span>
    </div>

    <v-alert type="info" dense text class="mb-4">
        Ce texte sera inséré automatiquement dans le document "Bilan de Mesure".
    </v-alert>
    
    </v-card-text>

        <v-textarea
          v-model="bilanText"
          label="Évaluation Qualitatives (Compétences, Acquis)"
          hint="Rédigez ici le texte évaluant la progression du stagiaire sur la période."
          outlined
          rows="6"
          clearable
          class="mb-4"
        />

        <v-textarea
          v-model="followUpActions"
          label="Actions de Suivi Recommandées"
          hint="Ex: Prochaine rencontre, Orientation, Objectifs trimestriels."
          outlined
          rows="3"
          clearable
        />

        <v-divider class="my-4"></v-divider>
        <div class="text-caption grey--text">Données de Référence (Automatiques) :</div>
        <v-chip-group>
            <v-chip small color="primary" outlined>
                Taux de Présence Global: {{ (stagiaireData.globalRate || 0).toFixed(1) }}%
            </v-chip>
            <v-chip small v-for="act in activitySummary" :key="act.key" :color="act.color" outlined>
                {{ act.libelle }}: {{ act.value }}
            </v-chip>
        </v-chip-group>


      <v-card-actions class="pa-4">
        <v-spacer />
        <v-btn text @click="$emit('update:visible', false)">Annuler</v-btn>
        <v-btn color="success" dark :loading="isLoading" @click="saveBilan">
          <v-icon left>mdi-content-save</v-icon>
          Sauvegarder et Fermer
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
export default {
  props: {
    visible: { // Propriété pour le v-model de la modale (via .sync)
      type: Boolean,
      default: false
    },
    stagiaireData: { // Les données du stagiaire sélectionné
      type: Object,
      default: () => ({})
    },
    periodInfo: { // Informations sur la période (trimestre/année)
      type: Object,
      default: () => ({ year: 0, quarter: 0 })
    }
  },
  data() {
    return {
      isLoading: false,
      bilanText: '',
      followUpActions: '',
    };
  },
  computed: {
    periodKey() {
      // Clé pour identifier le bilan qualitatif spécifique à la période
      return `${this.periodInfo.year}-${this.periodInfo.quarter}`;
    },
    activitySummary() {
        // Cette logique devrait être tirée des données passées à la modale
        // Simuler la récupération des heures individuelles pour l'affichage
        const summary = [];
        const followUp = this.stagiaireData.followUp || {};

        // Pour l'exemple, nous allons juste afficher les clés
        for (const key in followUp) {
            const entry = followUp[key];
             if (entry.hours) {
                summary.push({ key: key, libelle: key, value: `${entry.hours}h`, color: 'blue' });
            } else if (entry.isCompleted) {
                summary.push({ key: key, libelle: key, value: 'Fait', color: 'green' });
            }
        }
        return summary;
    }
  },
  watch: {
    visible(val) {
      if (val) {
        this.loadBilanData();
      }
    }
  },
  methods: {

loadBilanData() {
    // Charger les données de bilan existantes pour cette période et ce stagiaire
    this.isLoading = true;
    
    // Tentative de trouver le rapport pour la période actuelle directement sur l'objet stagiaire
    // Ceci suppose que le parent (FollowUp.vue) est responsable de charger ces rapports
    const currentBilan = this.stagiaireData.sessionReports?.[this.periodKey] || {};

    this.bilanText = currentBilan.evaluationText || '';
    this.followUpActions = currentBilan.followUpActions || '';
    // Si vous aviez d'autres champs (ex: selectedWorkshops), chargez-les ici.
    
    this.isLoading = false;
},

async saveBilan() {
    this.isLoading = true;

    // Construction du payload... (reste inchangé, c'est correct pour la sauvegarde)
    const payload = {
      stagiaireId: this.stagiaireData.stagiaireId,
      year: this.periodInfo.year,
      quarter: this.periodInfo.quarter.toString(),
      evaluationText: this.bilanText,
      followUpActions: this.followUpActions,
      selectedWorkshops: this.stagiaireData.followUp?.workshopsAndAccompanying?.selectedItems || [],
      globalRate: this.stagiaireData.globalRate
    };

    try {
        // ... (Appel API de sauvegarde, reste inchangé) ...
        const token = localStorage.getItem("access_token");
        const response = await fetch(`${API_BASE}/api/documents/save-bilan-session`, { 
            method: 'POST', 
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify(payload) 
        });

        if (!response.ok) throw new Error("Erreur serveur lors de la sauvegarde");

        // Émettre un événement incluant le payload *complet*
        this.$emit('bilan-saved', payload); // <-- C'est ici que nous envoyons les données au parent
        this.$emit('update:visible', false); 
        
    } catch (error) {
        console.error("Erreur lors de la sauvegarde du bilan:", error);
    } finally {
        this.isLoading = false;
    }
}
  }
};
</script>

<style scoped>
/* Les styles sont principalement gérés par Vuetify */
</style>