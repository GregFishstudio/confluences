<template>
  <v-dialog
    :model-value="modelValue"
    @update:model-value="closeModal"
    max-width="820px"
    scrollable
  >
    <v-card class="d-flex flex-column" style="max-height: 92vh;">

      <!-- ══ Toolbar ══════════════════════════════════════════════════════════ -->
      <v-toolbar flat color="primary" dark dense>
        <v-toolbar-title>
          Bilan :
          <strong>{{ stagiaireData?.prenom }} {{ stagiaireData?.nom }}</strong>
          <span class="text-caption ml-2" style="opacity:0.8;">
            ({{ stagiaireData?.niveau }} — Session {{ periodInfo?.quarter }} / {{ periodInfo?.year }})
          </span>
        </v-toolbar-title>
        <v-btn icon @click="closeModal"><v-icon>mdi-close</v-icon></v-btn>
      </v-toolbar>

      <!-- ══ Onglets ══════════════════════════════════════════════════════════ -->
      <v-tabs v-model="activeTab" bg-color="grey-lighten-4" color="primary" grow>
        <v-tab>
          <v-icon start size="small">mdi-text-box-outline</v-icon>
          Partie 1
        </v-tab>
        <v-tab v-if="hasSpecificTab">
          <v-icon start size="small">mdi-clipboard-list-outline</v-icon>
          Partie 2
        </v-tab>
        <v-tab>
          <v-icon start size="small">mdi-flag-checkered</v-icon>
          Partie 3
        </v-tab>
      </v-tabs>

      <!-- ══ Contenu ══════════════════════════════════════════════════════════ -->
      <v-card-text class="pa-4 flex-grow-1" style="overflow-y: auto;">

        <div v-if="isLoadingData" class="d-flex justify-center align-center py-10">
          <v-progress-circular indeterminate color="primary" />
        </div>

        <v-window v-else v-model="activeTab" class="pt-2">

          <!-- ──────────────────────────────────────────────────────────────── -->
          <!-- ONGLET 1 — Évaluation linguistique                              -->
          <!-- ──────────────────────────────────────────────────────────────── -->
          <v-window-item>
            <label class="text-caption font-weight-bold text-grey-darken-1">
              Évaluation linguistique / Globale
            </label>
            <v-textarea
              v-model="bilanText"
              label="Progrès, assiduité, acquis..."
              variant="outlined"
              rows="12"
              class="mt-1"
              auto-grow
            />
          </v-window-item>

          <!-- ──────────────────────────────────────────────────────────────── -->
          <!-- ONGLET 2 — Spécifique (F&I ou Télécours)                       -->
          <!-- ──────────────────────────────────────────────────────────────── -->
          <v-window-item v-if="hasSpecificTab">

            <!-- ── Bloc F&I ── -->
            <template v-if="isFrancaisIntegration">

              <!-- Insertion sociale -->
              <label class="text-caption font-weight-bold text-grey-darken-1">
                Insertion Socio-Professionnelle
              </label>
              <v-textarea
                v-model="socialInsertionText"
                variant="outlined"
                density="compact"
                rows="3"
                class="mt-1 mb-4"
              />

              <!-- Compétences numériques -->
              <label class="text-caption font-weight-bold text-grey-darken-1">
                Compétences Numériques
              </label>
              <v-textarea
                v-model="digitalSkillsText"
                variant="outlined"
                density="compact"
                rows="3"
                class="mt-1 mb-4"
              />

              <!-- ── Accompagnement individualisé — AUTOMATIQUE ── -->
              <div class="mb-1 d-flex align-center gap-2">
                <label class="text-caption font-weight-bold text-grey-darken-1">
                  Accompagnement Individualisé
                </label>
                <v-chip size="x-small" color="primary" variant="tonal" class="ml-1">
                  Données de la session
                </v-chip>
                <v-progress-circular
                  v-if="isLoadingActivities"
                  indeterminate
                  size="14"
                  width="2"
                  color="primary"
                  class="ml-2"
                />
              </div>

              <!-- Tableau des activités récupérées -->
              <v-card
                variant="outlined"
                rounded="lg"
                class="mb-3"
                :color="accompanyingActivities.length ? 'blue-lighten-5' : 'grey-lighten-4'"
              >
                <v-card-text class="pa-3">

                  <div v-if="isLoadingActivities" class="text-caption text-grey text-center py-2">
                    Chargement des activités…
                  </div>

                  <div
                    v-else-if="accompanyingActivities.length === 0"
                    class="text-caption text-grey text-center py-2"
                  >
                    <v-icon size="small" color="grey-lighten-1" class="mr-1">mdi-information-outline</v-icon>
                    Aucune activité enregistrée pour cette session.
                  </div>

                  <div v-else class="d-flex flex-wrap gap-2">

                    <!-- Accompagnements -->
                    <template v-if="accoActivities.length">
                      <div class="w-100 text-caption font-weight-bold text-blue-darken-2 mb-1">
                        <v-icon size="x-small" class="mr-1">mdi-account-heart</v-icon>
                        Accompagnements
                      </div>
                      <v-chip
                        v-for="act in accoActivities"
                        :key="act.activityKey"
                        color="blue"
                        variant="tonal"
                        size="small"
                        class="mb-1"
                      >
                        <v-icon start size="x-small">mdi-calendar-check</v-icon>
                        {{ act.label }}
                        <strong class="ml-1">· {{ act.count }} séance{{ act.count > 1 ? 's' : '' }}</strong>
                      </v-chip>
                    </template>

                    <!-- Ateliers -->
                    <template v-if="atelierActivities.length">
                      <div class="w-100 text-caption font-weight-bold text-green-darken-2 mb-1 mt-2">
                        <v-icon size="x-small" class="mr-1">mdi-hammer-wrench</v-icon>
                        Ateliers
                      </div>
                      <v-chip
                        v-for="act in atelierActivities"
                        :key="act.activityKey"
                        color="green"
                        variant="tonal"
                        size="small"
                        class="mb-1"
                      >
                        <v-icon start size="x-small">mdi-calendar-check</v-icon>
                        {{ act.label }}
                        <strong class="ml-1">· {{ act.count }} séance{{ act.count > 1 ? 's' : '' }}</strong>
                      </v-chip>
                    </template>

                  </div>

                  <!-- Totaux -->
                  <div v-if="accompanyingActivities.length" class="d-flex gap-4 mt-3 pt-2 border-t">
                    <div class="text-caption text-grey">
                      <span class="font-weight-bold text-blue-darken-2">{{ totalAccoSeances }}</span>
                      séance{{ totalAccoSeances > 1 ? 's' : '' }} d'accompagnement
                    </div>
                    <div v-if="totalAtelierSeances > 0" class="text-caption text-grey">
                      <span class="font-weight-bold text-green-darken-2">{{ totalAtelierSeances }}</span>
                      séance{{ totalAtelierSeances > 1 ? 's' : '' }} d'atelier
                    </div>
                  </div>

                </v-card-text>
              </v-card>

              <!-- Note complémentaire libre -->
              <label class="text-caption font-weight-bold text-grey-darken-1">
                Notes complémentaires (accompagnement)
              </label>
              <v-textarea
                v-model="individualAccompanyingText"
                variant="outlined"
                density="compact"
                rows="3"
                class="mt-1"
                placeholder="Remarques supplémentaires sur l'accompagnement individualisé..."
              />

            </template>

            <!-- ── Bloc Télécours ── -->
            <template v-else-if="isTelecours">
              <label class="text-caption font-weight-bold text-grey-darken-1">
                Visites à Domicile
              </label>
              <v-textarea
                v-model="homeVisitsText"
                variant="outlined"
                rows="10"
                class="mt-1"
                placeholder="Compte rendu des visites…"
              />
            </template>

          </v-window-item>

          <!-- ──────────────────────────────────────────────────────────────── -->
          <!-- ONGLET 3 — Synthèse / Perspectives                             -->
          <!-- ──────────────────────────────────────────────────────────────── -->
          <v-window-item>
            <div class="d-flex align-center mb-2 gap-2">
              <v-icon :color="perspectiveConfig.color">{{ perspectiveConfig.icon }}</v-icon>
              <span class="text-subtitle-2 font-weight-bold" :class="`text-${perspectiveConfig.color}`">
                {{ perspectiveConfig.label }}
              </span>
            </div>

            <v-textarea
              v-model="followUpActions"
              :color="perspectiveConfig.color"
              variant="outlined"
              rows="10"
              :placeholder="perspectiveConfig.placeholder"
              clearable
              counter
              auto-grow
            />

            <div class="d-flex align-center mt-2 text-grey-darken-1">
              <v-icon size="16" class="mr-1">mdi-information-outline</v-icon>
              <span class="text-caption font-style-italic">
                Ce texte sera automatiquement intégré en bas du document PDF final.
              </span>
            </div>
          </v-window-item>

        </v-window>
      </v-card-text>

      <v-divider />

      <!-- ══ Actions ══════════════════════════════════════════════════════════ -->
      <v-card-actions class="bg-grey-lighten-5 pa-4">
        <v-spacer />
        <v-btn variant="text" color="grey-darken-1" @click="closeModal" :disabled="isSaving">
          Annuler
        </v-btn>
        <v-btn color="primary" variant="flat" @click="saveBilan" :loading="isSaving">
          <v-icon start>mdi-content-save</v-icon>
          Enregistrer
        </v-btn>
      </v-card-actions>

    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import API_BASE from '@/services/api'

// ─── Props & Emits ────────────────────────────────────────────────────────────
const props = defineProps({
  modelValue:    { type: Boolean, default: false },
  stagiaireData: { type: Object,  required: true },
  periodInfo:    { type: Object,  required: true },
})
const emit = defineEmits(['update:modelValue', 'bilan-saved'])

// ─── State ────────────────────────────────────────────────────────────────────
const activeTab                  = ref(0)
const isLoadingData              = ref(false)
const isLoadingActivities        = ref(false)
const isSaving                   = ref(false)

// Champs bilan
const bilanText                  = ref('')
const followUpActions            = ref('')
const socialInsertionText        = ref('')
const digitalSkillsText          = ref('')
const individualAccompanyingText = ref('')
const homeVisitsText             = ref('')

// Activités d'accompagnement (depuis la matrice)
// Structure : [{ activityKey, label, type, count }]
const accompanyingActivities     = ref([])

// ─── Computed — type de mesure ────────────────────────────────────────────────
const isFrancaisIntegration = computed(() => {
  const type = (props.stagiaireData?.mesureType || '').toLowerCase()
  return type === 'fi_a1' || type === 'fi_a2' || type.includes('intégration') || type.includes('integration')
})

const isTelecours = computed(() => {
  const type = (props.stagiaireData?.mesureType || '').toLowerCase()
  return type === 'telecours' || type.includes('télécours') || type.includes('telecours')
})

const hasSpecificTab = computed(() => isFrancaisIntegration.value || isTelecours.value)

// ─── Computed — filtres par type d'activité ───────────────────────────────────
const accoActivities = computed(() =>
  accompanyingActivities.value.filter(a => a.type === 'acco')
)
const atelierActivities = computed(() =>
  accompanyingActivities.value.filter(a => a.type === 'atelier')
)
const totalAccoSeances = computed(() =>
  accoActivities.value.reduce((s, a) => s + a.count, 0)
)
const totalAtelierSeances = computed(() =>
  atelierActivities.value.reduce((s, a) => s + a.count, 0)
)

// ─── Computed — config texte synthèse ────────────────────────────────────────
const perspectiveConfig = computed(() => {
  if (isTelecours.value) {
    return {
      label:       'Insertion sociale',
      icon:        'mdi-account-group-outline',
      color:       'teal-darken-1',
      placeholder: 'Décrivez ici les démarches d\'insertion et l\'évolution sociale du participant.',
    }
  }
  return {
    label:       'Perspectives / Objectifs',
    icon:        'mdi-trending-up',
    color:       'primary',
    placeholder: 'Indiquez les actions à entreprendre et les objectifs pour la suite du parcours.',
  }
})

// ─── Watcher — ouvre la modal ────────────────────────────────────────────────
watch(() => props.modelValue, async (isOpen) => {
  if (isOpen) {
    activeTab.value = 0
    resetForm()
    await fetchBilanData()
    if (isFrancaisIntegration.value) {
      await fetchAccompanyingActivities()
    }
  }
})

// ─── Méthodes ─────────────────────────────────────────────────────────────────
const closeModal = () => emit('update:modelValue', false)

const resetForm = () => {
  bilanText.value                  = ''
  followUpActions.value            = ''
  socialInsertionText.value        = ''
  digitalSkillsText.value          = ''
  individualAccompanyingText.value = ''
  homeVisitsText.value             = ''
  accompanyingActivities.value     = []
}

// ── Fetch données bilan existant ──────────────────────────────────────────────
const fetchBilanData = async () => {
  isLoadingData.value = true
  const userId  = props.stagiaireData?.id || props.stagiaireData?.stagiaireId
  const { year, quarter } = props.periodInfo
  const token   = localStorage.getItem('access_token')

  try {
    const res = await fetch(
      `${API_BASE}/api/documents/bilan-session-data/${userId}?year=${year}&quarter=${quarter}`,
      { headers: { Authorization: `Bearer ${token}` } }
    )
    if (res.ok) {
      const text = await res.text()
      if (text) {
        const data                       = JSON.parse(text)
        bilanText.value                  = data.evaluationText              || ''
        followUpActions.value            = data.followUpActions             || ''
        socialInsertionText.value        = data.socialInsertionText         || ''
        digitalSkillsText.value          = data.digitalSkillsText           || ''
        individualAccompanyingText.value = data.individualAccompanyingText  || ''
        homeVisitsText.value             = data.homeVisitsText              || ''
      }
    }
  } catch (e) {
    console.error('Erreur chargement données bilan:', e)
  } finally {
    isLoadingData.value = false
  }
}

// ── Fetch activités d'accompagnement depuis la matrice ────────────────────────
const fetchAccompanyingActivities = async () => {
  isLoadingActivities.value = true
  const userId  = props.stagiaireData?.id || props.stagiaireData?.stagiaireId
  const { year, quarter } = props.periodInfo
  const token   = localStorage.getItem('access_token')

  try {
    const res = await fetch(
      `${API_BASE}/api/accompanying/summary/${userId}?year=${year}&quarter=${quarter}`,
      { headers: { Authorization: `Bearer ${token}` } }
    )
    if (res.ok) {
      accompanyingActivities.value = await res.json()
    }
  } catch (e) {
    console.error('Erreur chargement activités accompagnement:', e)
    accompanyingActivities.value = []
  } finally {
    isLoadingActivities.value = false
  }
}

// ── Sauvegarde ────────────────────────────────────────────────────────────────
const saveBilan = async () => {
  isSaving.value = true

  const userId = props.stagiaireData?.id || props.stagiaireData?.stagiaireId
  if (!userId) {
    alert('Erreur : identifiant du participant introuvable.')
    isSaving.value = false
    return
  }

  const payload = {
    stagiaireId:                 String(userId),
    year:                        parseInt(props.periodInfo.year),
    quarter:                     String(props.periodInfo.quarter),
    evaluationText:              bilanText.value                  || '',
    followUpActions:             followUpActions.value            || '',
    selectedWorkshops:           [],
    socialInsertionText:         socialInsertionText.value        || '',
    digitalSkillsText:           digitalSkillsText.value          || '',
    individualAccompanyingText:  individualAccompanyingText.value || '',
    homeVisitsText:              homeVisitsText.value             || '',
    globalRate:                  parseFloat(props.stagiaireData?.globalRate || 0),
  }

  try {
    const token = localStorage.getItem('access_token')
    const res   = await fetch(`${API_BASE}/api/documents/save-bilan-session`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
      body:    JSON.stringify(payload),
    })
    if (!res.ok) throw new Error(await res.text())
    emit('bilan-saved', payload)
    closeModal()
  } catch (e) {
    console.error('Erreur BDD:', e)
    alert('Erreur serveur : ' + e.message)
  } finally {
    isSaving.value = false
  }
}
</script>