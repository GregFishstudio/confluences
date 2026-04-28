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
          Bilan : {{ stagiaireData?.prenom }} {{ stagiaireData?.nom }} 
          <span class="text-caption ml-2" style="opacity: 0.8;">({{stagiaireData?.niveau }} Session {{ periodInfo?.quarter }} — {{ periodInfo?.year }})</span>
        </v-toolbar-title>
        
        <v-btn icon @click="closeModal">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-toolbar>

      <v-tabs v-model="activeTab" bg-color="grey-lighten-4" color="primary" grow>
        <v-tab>
          Partie 1
        </v-tab>
        <v-tab v-if="hasSpecificTab">
          Partie 2
        </v-tab>
        <v-tab>
          Partie 3
        </v-tab>
      </v-tabs>

      <v-card-text class="pa-4 flex-grow-1" style="overflow-y: auto;">

        <div v-if="isLoadingData" class="d-flex justify-center align-center py-10">
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
        </div>

        <v-window v-else v-model="activeTab" class="pt-2">

          <!-- Onglet Cours -->
          <v-window-item>
            <label class="text-caption font-weight-bold">Evaluation</label>
            <v-textarea
              v-model="bilanText"
              label="Évaluation linguistique / Globale"
              placeholder="Progrès, assiduité, acquis..."
              variant="outlined"
              rows="10"
            ></v-textarea>
          </v-window-item>

          <!-- Onglet Spécifique -->
          <v-window-item v-if="hasSpecificTab">
            <template v-if="isFrancaisIntegration">
              <label class="text-caption font-weight-bold">Insertion Socio</label>
              <v-textarea v-model="socialInsertionText" variant="outlined" density="compact" rows="3" class="mb-2"></v-textarea>
              <label class="text-caption font-weight-bold">Compétences Numériques</label>
              <v-textarea v-model="digitalSkillsText" variant="outlined" density="compact" rows="3" class="mb-2"></v-textarea>
              <label class="text-caption font-weight-bold">Accompagnement Individualisé</label>
              <v-textarea v-model="individualAccompanyingText" variant="outlined" density="compact" rows="3"></v-textarea>
            </template>
            <template v-else-if="isTelecours">
              <label class="text-caption font-weight-bold">Visites à Domicile</label>
              <v-textarea
                v-model="homeVisitsText"
                variant="outlined"
                rows="8"
                placeholder="Compte rendu des visites..."
              ></v-textarea>
            </template>
          </v-window-item>

          <!-- Onglet Synthèse -->
          <v-window-item>
            <label class="text-caption font-weight-bold">Synthèse/Insertion Socio</label>
  <div class="d-flex align-center mb-4">
    <div class="text-h6" :class="`text-${perspectiveConfig.color}`">
    </div>
  </div>


  <v-textarea
    v-model="followUpActions"
    :color="perspectiveConfig.color"
    variant="outlined"
    rows="8"
    placeholder="Insertion sociale/synthèse"
    clearable
    counter
  ></v-textarea>

  <div class="d-flex align-center mt-2 text-grey-darken-1">
    <v-icon size="16" class="mr-1">mdi-information-outline</v-icon>
    <span class="text-caption italic">Ce texte sera automatiquement intégré en bas du document PDF final.</span>
  </div>
</v-window-item>
        </v-window>
      </v-card-text>

      <v-divider></v-divider>

      <v-card-actions class="bg-grey-lighten-5 pa-4">
        <v-spacer></v-spacer>
        <v-btn variant="text" color="grey-darken-1" @click="closeModal" :disabled="isSaving">
          Annuler
        </v-btn>
        <v-btn color="primary" variant="flat" @click="saveBilan" :loading="isSaving">
          Enregistrer
        </v-btn>
      </v-card-actions>

    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import API_BASE, { apiFetch } from '@/services/api'

const props = defineProps({
  modelValue:    { type: Boolean, default: false },
  stagiaireData: { type: Object,  required: true },
  periodInfo:    { type: Object,  required: true },
})
const emit = defineEmits(['update:modelValue', 'bilan-saved'])

// ─── State ────────────────────────────────────────────────────────────────────
const activeTab                  = ref(0)
const isLoadingData              = ref(false)
const isSaving                   = ref(false)
const bilanText                  = ref('')
const followUpActions            = ref('')
const socialInsertionText        = ref('')
const digitalSkillsText          = ref('')
const individualAccompanyingText = ref('')
const homeVisitsText             = ref('')

// ─── Computed ─────────────────────────────────────────────────────────────────
const isFrancaisIntegration = computed(() => {
  const type = props.stagiaireData?.mesureType || ''
  return type === 'Français & Intégration' || type === 'fi_a1' || type === 'fi_a2'
})

const isTelecours = computed(() => {
  const type = props.stagiaireData?.mesureType || ''
  return type.toLowerCase().includes('télécours') || type === 'telecours'
})

const hasSpecificTab = computed(() => isFrancaisIntegration.value || isTelecours.value)

// ─── Watcher — charge les données à l'ouverture ───────────────────────────────
watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    activeTab.value = 0
    fetchBilanData()
  }
})

const perspectiveConfig = computed(() => {
  if (isTelecours.value) {
    return {
      label: 'Insertion sociale',
      icon: 'mdi-account-group-outline',
      color: 'teal-darken-1',
      help: 'Décrivez ici les démarches d’insertion et l’évolution sociale du participant.'
    };
  }
  return {
    label: 'Perspectives / Objectifs',
    icon: 'mdi-trending-up',
    color: 'primary',
    help: 'Indiquez les actions à entreprendre et les objectifs pour la suite du parcours.'
  };
});

// ─── Methods ──────────────────────────────────────────────────────────────────
const closeModal = () => emit('update:modelValue', false)

const resetForm = () => {
  bilanText.value                  = ''
  followUpActions.value            = ''
  socialInsertionText.value        = ''
  digitalSkillsText.value          = ''
  individualAccompanyingText.value = ''
  homeVisitsText.value             = ''
}

const fetchBilanData = async () => {
  isLoadingData.value = true
  resetForm()

  // Support des deux formats d'ID (ancien store stagiaireId, nouveau store id)
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
        const data               = JSON.parse(text)
        bilanText.value                  = data.evaluationText              || ''
        followUpActions.value            = data.followUpActions             || ''
        socialInsertionText.value        = data.socialInsertionText         || ''
        digitalSkillsText.value          = data.digitalSkillsText           || ''
        individualAccompanyingText.value = data.individualAccompanyingText  || ''
        homeVisitsText.value             = data.homeVisitsText              || ''
      }
    }
  } catch (e) {
    console.error('Erreur chargement données:', e)
  } finally {
    isLoadingData.value = false
  }
}

const saveBilan = async () => {
  isSaving.value = true

  // Support des deux formats d'ID
  const userId = props.stagiaireData?.id || props.stagiaireData?.stagiaireId
  if (!userId) {
    alert('Erreur : identifiant du participant introuvable.')
    isSaving.value = false
    return
  }

  const payload = {
    stagiaireId:                  String(userId),
    year:                         parseInt(props.periodInfo.year),
    quarter:                      String(props.periodInfo.quarter),
    evaluationText:               bilanText.value                  || '',
    followUpActions:              followUpActions.value            || '',
    selectedWorkshops:            [],
    socialInsertionText:          socialInsertionText.value        || '',
    digitalSkillsText:            digitalSkillsText.value         || '',
    individualAccompanyingText:   individualAccompanyingText.value || '',
    homeVisitsText:               homeVisitsText.value             || '',
    globalRate:                   parseFloat(props.stagiaireData?.globalRate || 0),
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