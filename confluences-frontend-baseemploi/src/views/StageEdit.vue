<template>
  <v-container class="stage-edit-container">
    <v-row>
      <v-col>
        <h1 class="text-h4 mb-4">Expérience professionnelle</h1>
      </v-col>
    </v-row>

    <v-form ref="formStage" v-model="valid" lazy-validation>
      
      <!-- Bloc 1 : Activité et Métadonnées -->
      <v-card class="pa-4 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold">Détails du Stage</v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.typeIntershipActivityId"
                :items="typeIntershipActivity.value?.typeIntershipActivities || []"
                item-title="nom"
                item-value="typeIntershipActivityId"
                label="Type d'activité"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="4">
              <v-text-field
                v-model="localStage.nom"
                :counter="30"
                :rules="nameRules"
                label="Nom du stage (Optionnel)"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>
            
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.typeStageId"
                :items="typeStage.value?.typeStages || []"
                item-title="nom"
                item-value="typeStageId"
                label="Taux d'occupation"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="6">
              <v-autocomplete
                v-model="localStage.createurId"
                :items="user.value?.users || []"
                item-title="nom"
                item-value="id"
                label="Créateur-trice"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="6">
              <v-autocomplete
                v-model="localStage.typeAnnonceId"
                :items="typeAnnonce.value?.typeAnnonces || []"
                item-title="libelle"
                item-value="typeAnnonceId"
                label="Stage à annoncer (Optionnel)"
                clearable
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>

      <!-- Bloc 2 : Personnes et Entreprise -->
      <v-card class="pa-4 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold">Rôles et Organisation</v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.typeMetierId"
                :items="typeMetier.value?.typeMetiers || []"
                item-title="libelle"
                item-value="typeMetierId"
                label="Métier"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.entrepriseId"
                :items="entreprise.value?.entreprises || []"
                item-title="nom"
                item-value="entrepriseId"
                label="Entreprise (Optionnel)"
                clearable
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.stagiaireId"
                :items="user.value?.users || []"
                item-title="nom"
                item-value="id"
                label="Stagiaire"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
      
      <!-- Bloc 3 : Dates, Horaires et Avantages -->
      <v-card class="pa-4 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold">Période et Conditions</v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-row>
                <v-col cols="12" sm="6" class="py-0">
                  <v-text-field
                    v-model="localStage.debut"
                    label="Début"
                    readonly
                    variant="outlined"
                    density="compact"
                    @click="menuDebut = true"
                    hide-details="auto"
                  />
                  <v-dialog v-model="menuDebut" max-width="340">
                    <v-card>
                      <v-date-picker
                        v-model="localStage.debut"
                        color="primary"
                        @update:model-value="menuDebut = false"
                      />
                    </v-card>
                  </v-dialog>
                </v-col>

                <v-col cols="12" sm="6" class="py-0">
                  <v-text-field
                    v-model="localStage.fin"
                    label="Fin"
                    readonly
                    clearable
                    variant="outlined"
                    density="compact"
                    @click="menuFin = true"
                    hide-details="auto"
                  />
                  <v-dialog v-model="menuFin" max-width="340">
                    <v-card>
                      <v-date-picker
                        v-model="localStage.fin"
                        color="primary"
                        @update:model-value="menuFin = false"
                      />
                    </v-card>
                  </v-dialog>
                </v-col>
              </v-row>
              
              <v-autocomplete
                v-model="localStage.sessionId"
                :items="session.value?.sessions || []"
                item-title="description"
                item-value="sessionId"
                label="Session (Optionnel)"
                clearable
                variant="outlined"
                density="compact"
                class="mt-4"
                hide-details="auto"
              />
              
              <v-row class="mt-4">
                <v-col cols="6" class="py-0">
                  <v-checkbox v-model="localStage.repas" label="Repas" density="compact" hide-details />
                </v-col>
                <v-col cols="6" class="py-0">
                  <v-checkbox v-model="localStage.trajets" label="Trajets" density="compact" hide-details />
                </v-col>
              </v-row>
            </v-col>

            <v-col cols="12" md="8">
              <v-row>
                <v-col cols="12" sm="4">
                  <v-text-field
                    v-model="localStage.horaireMatin"
                    :counter="11"
                    :rules="horaireRules"
                    label="Horaire matin"
                    placeholder="ex: 08:00-12:00"
                    variant="outlined"
                    density="compact"
                    hide-details="auto"
                  />
                </v-col>

                <v-col cols="12" sm="4">
                  <v-text-field
                    v-model="localStage.horaireApresMidi"
                    :counter="11"
                    :rules="horaireRules"
                    label="Horaire après-midi"
                    placeholder="ex: 13:00-17:00"
                    variant="outlined"
                    density="compact"
                    hide-details="auto"
                  />
                </v-col>

                <v-col cols="12" sm="4">
                  <v-text-field
                    v-model="localStage.horaireSamedi"
                    :counter="30"
                    :rules="horaireSamediRules"
                    label="Horaire samedi"
                    placeholder="ex: 08:00-12:00 ou N/A"
                    variant="outlined"
                    density="compact"
                    hide-details="auto"
                  />
                </v-col>
              </v-row>

              <v-row class="mt-4">
                <v-col cols="12" sm="4" class="py-0">
                  <v-checkbox v-model="localStage.attestation" label="Attestation requise" density="compact" hide-details />
                </v-col>
                <v-col cols="12" sm="4" class="py-0">
                  <v-checkbox v-model="localStage.rapport" label="Rapport de stage" density="compact" hide-details />
                </v-col>
                <v-col cols="12" sm="4" class="py-0">
                  <v-checkbox v-model="localStage.hasContract" label="Contrat signé" density="compact" hide-details />
                </v-col>
              </v-row>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>

      <!-- Bloc 4 : Textes et Bilans -->
      <v-card class="pa-4 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold">Notes et Suivi</v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="6">
              <v-textarea
                v-model="localStage.remarque"
                label="Remarques générales"
                variant="outlined"
                auto-grow
                counter
                density="compact"
                rows="3"
              />
            </v-col>

            <v-col cols="12" md="6">
              <v-textarea
                v-model="localStage.bilan"
                label="Bilan du stage"
                variant="outlined"
                auto-grow
                counter
                density="compact"
                rows="3"
              />
            </v-col>
          </v-row>

          <v-row>
            <v-col>
              <v-textarea
                v-model="localStage.actionSuivi"
                label="Suivi (Actions futures)"
                variant="outlined"
                auto-grow
                counter
                density="compact"
                rows="2"
              />
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>

      <!-- Bloc 5 : Fichiers -->
      <v-row class="mb-10">
        <v-col>
          <StageFileList :stage="localStage" />
        </v-col>
      </v-row>

      <!-- Barre d'Actions FIXE -->
      <div class="action-container action-bar-fixed">
        <v-row class="fill-height ma-0">
          <v-col class="d-flex justify-end align-center py-2">
            
            <!-- Bouton Générer Bilan -->
            <v-btn
              class="ma-2"
              color="secondary"
              variant="flat"
              min-width="150"
              @click="downloadBilan(localStage.stageId)"
              prepend-icon="mdi-file-pdf-box"
            >
              Générer Bilan
            </v-btn>

            <v-btn
              class="ma-2"
              color="primary"
              variant="flat"
              min-width="150"
              @click="submit"
              prepend-icon="mdi-content-save-outline"
            >
              Sauvegarder
            </v-btn>

            <DeleteStage :stage="localStage" class="ma-2" />

            <v-btn
              class="ma-2"
              color="grey-darken-1"
              variant="outlined"
              min-width="150"
              @click="router.go(-1)"
              prepend-icon="mdi-close"
            >
              Annuler
            </v-btn>
          </v-col>
        </v-row>
      </div>
    </v-form>
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount, watch } from 'vue'
import { useRouter } from 'vue-router' 
import { useStore } from 'vuex';
import DeleteStage from '@/components/DeleteStage.vue'
import StageFileList from '@/components/StageFileList.vue'
import moment from 'moment'
import API_BASE from "@/services/api";

const router = useRouter(); 
const store = useStore();

/* Props */
const props = defineProps({
  stage: {
    type: Object,
    required: true
  }
})

/* Local State (COPIE LOCALE POUR ÉDITION) */
// Initialisé vide, sera rempli par le watcher
const localStage = ref({});

/* Form State */
const formStage = ref(null)
const valid = ref(true)

/* Menus dates */
const menuDebut = ref(false)
const menuFin = ref(false)

/* Validation Rules */
const nameRules = [
  v => !v || v.length <= 30 || 'Max 30 caractères'
]

const horaireRules = [
  v => !v || v.length <= 11 || 'Max 11 caractères (ex: 08:00-12:00)'
]

const horaireSamediRules = [
  v => !v || v.length <= 30 || 'Max 30 caractères'
]

const requiredRule = [v => !!v || 'Obligatoire']

/* Store state */
const entreprise = computed(() => store.state.entreprise)
const typeStage = computed(() => store.state.typeStage)
const typeAnnonce = computed(() => store.state.typeAnnonce)
const typeMetier = computed(() => store.state.typeMetier)
const typeIntershipActivity = computed(() => store.state.typeIntershipActivity)
const user = computed(() => store.state.user)
const session = computed(() => store.state.session)

/* Watcher pour copier les props vers le state local */
// C'est crucial : quand les données arrivent du store, on met à jour la copie locale
watch(() => props.stage, (newVal) => {
  if (newVal) {
    // Copie superficielle (spread operator)
    localStage.value = { ...newVal };
    
    // Formatage spécifique pour les champs date
    localStage.value.debut = formatDate(newVal.debut);
    localStage.value.fin = formatDate(newVal.fin);
  }
}, { immediate: true, deep: true });

/* Data Loading */
onBeforeMount(() => {
  store.dispatch('entreprise/fetchEntreprises')
  store.dispatch('typeStage/fetchTypeStages')
  store.dispatch('typeAnnonce/fetchTypeAnnonces')
  store.dispatch('typeMetier/fetchTypeMetiers')
  store.dispatch('typeIntershipActivity/fetchTypeIntershipActivities')
  store.dispatch('user/fetchUsers')
  store.dispatch('session/fetchSessions')
})

/* Methods */

function formatDate(date) {
  if (!date) return null;
  const d = moment(date).format('YYYY-MM-DD'); 
  return d === 'Invalid date' ? null : d;
}

// Téléchargement du Bilan (Copié depuis StagesList)
const downloadBilan = async (id) => {
    if (!id) return;
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

async function submit() {
  const { valid: formValid } = await formStage.value.validate();

  if (formValid) {
    // On envoie la COPIE LOCALE (localStage) au lieu de la prop
    store
      .dispatch('stage/editStage', localStage.value)
      .then(() => {
        router.push({ name: 'Stages' }); 
      })
      .catch((error) => {
        console.error("Erreur de sauvegarde du stage:", error);
      })
  }
}
</script>

<style scoped>
.stage-edit-container {
    padding-bottom: 80px;
}

.action-bar-fixed {
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 64px; 
  background: white;
  border-top: 1px solid #e0e0e0;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08);
  z-index: 1000;
  padding: 0 40px;
}

.v-text-field :deep(.v-field__overlay),
.v-autocomplete :deep(.v-field__overlay),
.v-textarea :deep(.v-field__overlay) {
    transition: background-color 0.2s ease;
}
</style>