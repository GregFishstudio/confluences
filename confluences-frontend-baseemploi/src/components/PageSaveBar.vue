<!--
  Barre d'enregistrement d'une fiche, toujours visible en haut de la page : un seul bouton
  « Enregistrer » et un état sans ambiguïté.
    saved   → « Tout est enregistré » (+ heure du dernier enregistrement)
    dirty   → modifications pas encore enregistrées (et, si `autosave`, enregistrement automatique imminent)
    drafts  → commentaire(s) écrit(s) mais pas encore ajouté(s)
    saving  → « Enregistrement… »
    error   → « Non enregistré : raison » + Réessayer
-->
<template>
  <div class="page-save-bar" :class="`page-save-bar--${state}`">
    <v-progress-circular v-if="state === 'saving'" indeterminate size="18" width="2" class="mr-3" />
    <v-icon v-else class="mr-3" size="22">{{ ICONS[state] }}</v-icon>
    <div class="flex-grow-1">
      <div class="font-weight-bold">{{ title }}</div>
      <div v-if="detail" class="text-caption">{{ detail }}</div>
    </div>
    <v-btn
      :color="state === 'error' ? 'error' : 'primary'" :variant="state === 'saved' ? 'tonal' : 'flat'"
      :disabled="state === 'saved' || state === 'saving'" :loading="state === 'saving'"
      prepend-icon="mdi-content-save-outline" @click="emit('save')"
    >
      {{ state === 'error' ? 'Réessayer' : 'Enregistrer' }}
    </v-btn>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import moment from 'moment/min/moment-with-locales'

const props = defineProps({
  state: { type: String, required: true },     // saved | dirty | drafts | saving | error
  error: { type: String, default: '' },
  draftCount: { type: Number, default: 0 },
  lastSavedAt: { type: [Date, String], default: null },
  autosave: { type: Boolean, default: false },
})
const emit = defineEmits(['save'])

const ICONS = { saved: 'mdi-check-circle', dirty: 'mdi-pencil-circle', drafts: 'mdi-comment-alert', error: 'mdi-alert-circle' }

const title = computed(() => ({
  saved: 'Tout est enregistré',
  dirty: 'Modifications pas encore enregistrées',
  drafts: `${props.draftCount} commentaire${props.draftCount > 1 ? 's' : ''} écrit${props.draftCount > 1 ? 's' : ''} mais pas encore ajouté${props.draftCount > 1 ? 's' : ''}`,
  saving: 'Enregistrement…',
  error: `Non enregistré : ${props.error}`,
}[props.state]))

const detail = computed(() => {
  if (props.state === 'saved') return props.lastSavedAt ? `Dernier enregistrement à ${moment(props.lastSavedAt).format('HH:mm')}` : ''
  if (props.state === 'dirty') return props.autosave ? 'Enregistrement automatique dans quelques secondes — ou cliquez sur Enregistrer.' : 'Cliquez sur Enregistrer pour ne pas les perdre.'
  if (props.state === 'drafts') return 'Cliquez sur Enregistrer pour les ajouter (datés et signés).'
  if (props.state === 'error') return 'Vos saisies sont toujours à l\'écran : rien n\'est perdu tant que vous ne quittez pas la page.'
  return ''
})
</script>

<style scoped>
.page-save-bar {
  position: sticky; top: 64px; z-index: 5;
  display: flex; align-items: center; gap: 4px;
  padding: 10px 16px; margin-bottom: 16px; border-radius: 10px;
  border: 1px solid; box-shadow: 0 2px 8px rgba(0, 0, 0, .06);
}
.page-save-bar--saved  { background: #f1f8f2; border-color: #c8e6c9; color: #2e7d32; }
.page-save-bar--dirty,
.page-save-bar--drafts { background: #fff8e1; border-color: #ffe082; color: #8d6e00; }
.page-save-bar--saving { background: #fffde7; border-color: #fff59d; color: #8d6e00; }
.page-save-bar--error  { background: #ffebee; border-color: #ef9a9a; color: #c62828; }
</style>
