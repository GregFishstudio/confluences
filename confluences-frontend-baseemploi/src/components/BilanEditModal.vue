<template>
  <v-dialog 
    :model-value="modelValue" 
    @update:model-value="closeModal"
    max-width="800px" 
    
    scrollable
  >
    <v-card class="d-flex flex-column" style="max-height: 90vh;">
      
      <v-toolbar flat color="primary" dark dense>
        <v-toolbar-title>
           Bilan : {{ stagiaireData.prenom }} {{ stagiaireData.nom }}
           <span class="text-caption ml-2" style="opacity: 0.8;">({{ periodInfo.label }})</span>
        </v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn icon @click="closeModal">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-toolbar>

      <v-tabs v-model="activeTab" background-color="grey lighten-4" color="primary" grow>
        <v-tab>
            <v-icon left small>mdi-school</v-icon> Cours
        </v-tab>
        <v-tab v-if="hasSpecificTab">
            <v-icon left small>mdi-puzzle</v-icon> Spécifique
        </v-tab>
        <v-tab>
            <v-icon left small>mdi-flag-checkered</v-icon> Synthèse
        </v-tab>
      </v-tabs>

      <v-card-text class="pa-4 flex-grow-1" style="overflow-y: auto;">
        
        <div v-if="isLoadingData" class="d-flex justify-center align-center py-10">
             <v-progress-circular indeterminate color="primary"></v-progress-circular>
        </div>

        <v-window v-else v-model="activeTab" class="pt-2">
            
            <v-window-item>
                <v-alert border="left" colored-border color="primary" elevation="1" class="mb-4">
                    <div class="text-subtitle-2">Formation suivie</div>
                    <div class="text-body-2 grey--text text--darken-2">
                        {{ stagiaireData.courseDetails?.mainInfo || 'Non spécifié' }}
                        <span v-if="stagiaireData.oralLevel"> ({{ stagiaireData.oralLevel }})</span>
                    </div>
                </v-alert>

                <v-textarea
                    v-model="bilanText"
                    label="Évaluation linguistique / Globale"
                    placeholder="Progrès, assiduité, acquis..."
                    outlined
                    rows="10"
                ></v-textarea>
            </v-window-item>

            <v-window-item v-if="hasSpecificTab">
                
                <template v-if="isFrancaisIntegration">
                    <div class="text-overline primary--text mb-2">Français & Intégration</div>
                    
                    <label class="text-caption font-weight-bold">Insertion Socio-Pro</label>
                    <v-textarea v-model="socialInsertionText" outlined dense rows="3" class="mb-2"></v-textarea>
                    
                    <label class="text-caption font-weight-bold">Compétences Numériques</label>
                    <v-textarea v-model="digitalSkillsText" outlined dense rows="3" class="mb-2"></v-textarea>
                    
                    <label class="text-caption font-weight-bold">Accompagnement Individualisé</label>
                    <v-textarea v-model="individualAccompanyingText" outlined dense rows="3"></v-textarea>
                </template>

                <template v-else-if="isTelecours">
                    <div class="text-overline primary--text mb-2">Spécifique Télécours</div>
                    <label class="text-caption font-weight-bold">Visites à Domicile</label>
                    <v-textarea 
                        v-model="homeVisitsText" 
                        outlined 
                        rows="8" 
                        placeholder="Compte rendu des visites..."
                    ></v-textarea>
                </template>
            </v-window-item>

            <v-window-item>
                <div class="text-subtitle-1 mb-2">Perspectives</div>
                <v-textarea
                    v-model="followUpActions"
                    label="Actions à entreprendre / Objectifs"
                    outlined
                    rows="6"
                ></v-textarea>
                <div class="caption grey--text mt-2">
                    * Ce texte apparaîtra en bas du PDF.
                </div>
            </v-window-item>

        </v-window>
      </v-card-text>

      <v-divider></v-divider>

      <v-card-actions class="grey lighten-5 pa-4">
        <v-spacer></v-spacer>
        <v-btn text color="grey darken-1" @click="closeModal" :disabled="isSaving">
            Annuler
        </v-btn>
        <v-btn color="primary" depressed @click="saveBilan" :loading="isSaving">
            Enregistrer
        </v-btn>
      </v-card-actions>

    </v-card>
  </v-dialog>
</template>

<script>
import API_BASE from "@/services/api";

export default {
    // Utilisation de modelValue pour compatibilité Vue 3 / Vuetify 3 (ou script setup parent)
    props: {
        modelValue: { type: Boolean, default: false }, 
        stagiaireData: { type: Object, required: true },
        periodInfo: { type: Object, required: true }
    },
    emits: ['update:modelValue', 'bilan-saved'],
    data() {
        return {
            activeTab: 0,
            isLoadingData: false,
            isSaving: false,

            // Champs Formulaire
            bilanText: '',
            followUpActions: '',
            socialInsertionText: '',
            digitalSkillsText: '',
            individualAccompanyingText: '',
            homeVisitsText: '',
        };
    },
    computed: {
        isFrancaisIntegration() {
            return this.stagiaireData.mesureType === 'Français & Intégration';
        },
        isTelecours() {
            const type = this.stagiaireData.mesureType || '';
            // Détection souple (gère "Télécours" ou "Français en Télécours")
            return type.toLowerCase().includes('télécours');
        },
        hasSpecificTab() {
            return this.isFrancaisIntegration || this.isTelecours;
        }
    },
    watch: {
        // Surveiller l'ouverture pour charger les données
        modelValue(isOpen) {
            if (isOpen) {
                this.activeTab = 0;
                this.fetchBilanData();
            }
        }
    },
    methods: {
        closeModal() {
            // Emet l'événement standard pour fermer v-model
            this.$emit('update:modelValue', false);
        },

        resetForm() {
            this.bilanText = '';
            this.followUpActions = '';
            this.socialInsertionText = '';
            this.digitalSkillsText = '';
            this.individualAccompanyingText = '';
            this.homeVisitsText = '';
        },

        async fetchBilanData() {
            this.isLoadingData = true;
            this.resetForm();

            const { stagiaireId } = this.stagiaireData;
            const { year, quarter } = this.periodInfo;
            const token = localStorage.getItem("access_token");

            // URL Endpoint : Adaptez si nécessaire
            // L'endpoint doit renvoyer le JSON sauvegardé pour ce trimestre
            const url = `${API_BASE}/api/documents/bilan-session-data/${stagiaireId}?year=${year}&quarter=${quarter}`;

            try {
                const response = await fetch(url, {
                    method: 'GET',
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (response.ok) {
            const text = await response.text(); // Lire en texte d'abord
            if (text) {
                const data = JSON.parse(text);
                this.bilanText = data.evaluationText || '';
                        this.followUpActions = data.followUpActions || '';
                        this.socialInsertionText = data.socialInsertionText || '';
                        this.digitalSkillsText = data.digitalSkillsText || '';
                        this.individualAccompanyingText = data.individualAccompanyingText || '';
                        this.homeVisitsText = data.homeVisitsText || '';
                    }
                    else {
                this.resetForm(); // Pas de données encore créées pour ce trimestre
            }
                }
            } catch (e) {
                console.error("Erreur chargement données:", e);
            } finally {
                this.isLoadingData = false;
            }
        },

        async saveBilan() {
    this.isSaving = true;

    const payload = {
        // L'ID doit correspondre à un ID existant dans AspNetUsers
        stagiaireId: this.stagiaireData.stagiaireId.toString(), 
       year: parseInt(this.periodInfo.year), // <-- Correction ici
        quarter: this.periodInfo.quarter.toString(), // <-- Correction ici
        
        // --- IMPORTANT : Forcer une chaîne vide "" si le texte est null/undefined ---
        evaluationText: this.bilanText || "", 
        followUpActions: this.followUpActions || "", 
        
        // Pour WorkshopsJson (not null), on envoie une liste vide qui sera sérialisée
        selectedWorkshops: [], 
        
        // Ces champs sont "Nullable" en BDD, donc moins risqués, mais restons prudents
        socialInsertionText: this.socialInsertionText || "",
        digitalSkillsText: this.digitalSkillsText || "",
        individualAccompanyingText: this.individualAccompanyingText || "",
        homeVisitsText: this.homeVisitsText || "",
        
        globalRate: parseFloat(this.stagiaireData.globalRate || 0)
    };

    try {
        const token = localStorage.getItem("access_token");
        const response = await fetch(`${API_BASE}/api/documents/save-bilan-session`, { 
            method: 'POST', 
            headers: { 
                'Content-Type': 'application/json', 
                'Authorization': `Bearer ${token}` 
            },
            body: JSON.stringify(payload) 
        });

        if (!response.ok) {
            const errorMsg = await response.text();
            throw new Error(errorMsg);
        }

        this.$emit('bilan-saved', payload);
        this.closeModal();
    } catch (error) {
        console.error("Erreur BDD précise:", error);
        alert("Erreur serveur : " + error.message);
    } finally {
        this.isSaving = false;
    }
}
    }
};
</script>

<style scoped>
/* Ajustement mineur si nécessaire */
</style>