<template>
  <v-container class="stage-edit-container">
    <v-row align="center" class="mb-2" no-gutters>
      <v-col cols="auto">
        <v-btn
          icon="mdi-arrow-left"
          variant="text"
          @click="retourOuListe(router, 'Stages')"
          aria-label="Retour"
        />
      </v-col>
      <v-col>
        <div class="d-flex align-center flex-wrap ga-3">
          <h1 class="text-h4">Expérience professionnelle</h1>
          <v-chip
            v-if="localStage.typeAnnonce?.libelle"
            size="small"
            :color="isAnnonce ? 'red' : 'grey-lighten-3'"
            :text-color="isAnnonce ? 'white' : 'grey-darken-3'"
            :class="{ 'font-weight-bold': isAnnonce }"
          >
            {{ localStage.typeAnnonce.libelle }}
          </v-chip>
        </div>
        <p class="text-body-2 text-medium-emphasis mb-0">
          Les champs marqués d'un <strong>*</strong> sont obligatoires.
        </p>
      </v-col>
    </v-row>

    <v-form ref="formStage" v-model="valid" lazy-validation>

      <v-card class="pa-4 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold d-flex align-center">
          <v-icon start color="primary">mdi-information-outline</v-icon>
          Détails du Stage
        </v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.typeIntershipActivityId"
                :items="typeIntershipActivitiesList"
                item-title="nom"
                item-value="typeIntershipActivityId"
                label="Type d'activité *"
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
                :items="typeStagesList"
                item-title="nom"
                item-value="typeStageId"
                label="Taux d'occupation *"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="6">
              <v-autocomplete
                v-model="localStage.createurId"
                :items="usersList"
                item-title="nom"
                item-value="id"
                label="Créateur-trice *"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="6">
              <v-autocomplete
                v-model="localStage.typeAnnonceId"
                :items="typeAnnoncesList"
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

      <v-card class="pa-4 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold d-flex align-center">
          <v-icon start color="primary">mdi-account-group-outline</v-icon>
          Rôles et Organisation
        </v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.typeMetierId"
                :items="typeMetiersList"
                item-title="libelle"
                item-value="typeMetierId"
                label="Métier *"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>

            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="localStage.entrepriseId"
                :items="entreprisesList"
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
                :items="usersList"
                item-title="nom"
                item-value="id"
                label="Stagiaire *"
                :rules="requiredRule"
                variant="outlined"
                density="compact"
                hide-details="auto"
              />
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
      
      <v-card class="pa-6 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold d-flex align-center pl-0">
          <v-icon start color="primary">mdi-calendar-clock-outline</v-icon>
          Période et Conditions
        </v-card-title>
        <v-card-text class="pb-0">
          <v-row>
            <v-col cols="12" md="4">
              <v-row>
                <v-col cols="12" sm="6">
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

                <v-col cols="12" sm="6">
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
                :items="sessionsList"
                item-title="description"
                item-value="sessionId"
                label="Session (Optionnel)"
                clearable
                variant="outlined"
                density="compact"
                class="mt-6 mb-6"
                hide-details="auto"
              />

              <v-row>
                <v-col cols="6">
                  <v-checkbox v-model="localStage.repas" label="Repas" density="compact" hide-details />
                </v-col>
                <v-col cols="6">
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

              <v-row class="mt-9">
                <v-col cols="12" sm="4">
                  <v-checkbox v-model="localStage.attestation" label="Attestation requise" density="compact" hide-details />
                </v-col>
                <v-col cols="12" sm="4">
                  <v-checkbox v-model="localStage.rapport" label="Rapport de stage" density="compact" hide-details />
                </v-col>
                <v-col cols="12" sm="4">
                  <v-checkbox v-model="localStage.hasContract" label="Contrat signé" density="compact" hide-details />
                </v-col>
              </v-row>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>

      <v-card class="pa-4 mb-4 elevation-2 rounded-lg">
        <v-card-title class="text-subtitle-1 font-weight-bold d-flex align-center">
          <v-icon start color="primary">mdi-note-text-outline</v-icon>
          Notes et Suivi
        </v-card-title>
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

      <v-row class="mb-10">
        <v-col>
          <StageFileList :stage="localStage" />
        </v-col>
      </v-row>

      <div class="action-container action-bar-fixed">
        <div class="action-bar-inner">
          <v-btn
            class="ma-1"
            color="secondary"
            variant="flat"
            min-width="130"
            @click="downloadBilan(localStage.stageId)"
            prepend-icon="mdi-file-pdf-box"
          >
            Générer Bilan
          </v-btn>

          <v-btn
            class="ma-1"
            color="primary"
            variant="flat"
            min-width="130"
            @click="submit"
            prepend-icon="mdi-content-save-outline"
          >
            Sauvegarder
          </v-btn>

          <DeleteStage :stage="localStage" class="ma-1" />

          <v-btn
            class="ma-1"
            color="grey-darken-1"
            variant="outlined"
            min-width="130"
            @click="retourOuListe(router, 'Stages')"
            prepend-icon="mdi-close"
          >
            Annuler
          </v-btn>
        </div>
      </div>
    </v-form>
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount, watch } from 'vue'
import { useRouter } from 'vue-router'
import { retourOuListe } from '@/composables/useListMemory'
import { useStore } from 'vuex';
import DeleteStage from '@/components/DeleteStage.vue'
import StageFileList from '@/components/StageFileList.vue'
import moment from 'moment'
import API_BASE, {apiFetch} from '@/services/api'

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

// 🚀 Correction Majeure ici : Accès direct et sécurisé (?. pour éviter les crashs)
// Ces variables ne nécessitent PAS .value dans le template
const entreprisesList = computed(() => store.state.entreprise?.entreprises || [])
const typeStagesList = computed(() => store.state.typeStage?.typeStages || [])
const typeAnnoncesList = computed(() => store.state.typeAnnonce?.typeAnnonces || [])
const typeMetiersList = computed(() => store.state.typeMetier?.typeMetiers || [])
const typeIntershipActivitiesList = computed(() => store.state.typeIntershipActivity?.typeIntershipActivities || [])
const usersList = computed(() => store.state.user?.users || [])
const sessionsList = computed(() => store.state.session?.sessions || [])

const isAnnonce = computed(() => localStage.value.typeAnnonce?.libelle === 'Annoncé')

/* Watcher pour copier les props vers le state local */
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
  // Assurez-vous que l'appel d'action correspond au nom du module
  store.dispatch('entreprise/fetchEntreprises')
  store.dispatch('typeStage/fetchTypeStages')
  store.dispatch('typeAnnonce/fetchTypeAnnonces')
  store.dispatch('typeMetier/fetchTypeMetiers')
  store.dispatch('typeIntershipActivity/fetchTypeIntershipActivities')
  store.dispatch('user/fetchUsers') // L'action doit être dans le module 'user'
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
  if (!id) return
  const now = new Date().toISOString()

  const res = await apiFetch(`${API_BASE}/api/documents/bilan/${id}?date=${now}`, {
    headers: { 'Content-Type': '' } // override pour pas envoyer application/json sur un PDF
  })

  if (res?.ok) {
    const blob = await res.blob()
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `bilan-${id}.pdf`
    link.click()
    window.URL.revokeObjectURL(url)
  } else {
    console.error('Échec du téléchargement du bilan:', res?.status)
  }
}

async function submit() {
  const { valid: formValid } = await formStage.value.validate();

  if (formValid) {
    // On envoie la COPIE LOCALE (localStage) au lieu de la prop
    store
      .dispatch('stage/editStage', localStage.value)
      .then(() => {
        retourOuListe(router, 'Stages');
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
  min-height: 64px;
  background: rgb(var(--v-theme-surface));
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08);
  z-index: 1000;
  padding: 8px 16px;
}

.action-bar-inner {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  justify-content: flex-end;
  width: 100%;
}

/* Fix pour les ombres des champs, si nécessaire */
.v-text-field :deep(.v-field__overlay),
.v-autocomplete :deep(.v-field__overlay),
.v-textarea :deep(.v-field__overlay) {
    transition: background-color 0.2s ease;
}
</style>