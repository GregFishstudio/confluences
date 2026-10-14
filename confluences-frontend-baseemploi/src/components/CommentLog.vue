<!--
  Journal de commentaires compact (une rubrique du suivi formatrices) : chaque entrée est datée
  et signée, les précédentes restent visibles (historique), une entrée erronée peut être
  supprimée. La saisie en cours (pas encore ajoutée) est exposée via v-model:draft, pour que la
  page puisse avertir avant qu'on la quitte.
-->
<template>
  <div class="comment-log">
    <div v-if="label" class="text-caption text-medium-emphasis mb-1">{{ label }}</div>
    <div v-for="c in visibleEntries" :key="c.participantCommentId" class="entry">
      <div class="entry-meta">
        <span class="font-weight-bold">{{ formatDate(c.createdAt) }}</span>
        <span v-if="c.authorName"> · {{ c.authorName }}</span>
        <v-btn icon="mdi-delete-outline" size="x-small" variant="text" color="grey" class="entry-delete" title="Supprimer cette entrée" @click="remove(c)" />
      </div>
      <div class="entry-text">{{ c.text }}</div>
    </div>
    <v-btn v-if="entries.length > maxVisible && !showAll" size="x-small" variant="text" color="primary" class="mb-1" @click="showAll = true">
      Voir l'historique complet ({{ entries.length }} entrées)
    </v-btn>
    <div class="d-flex align-start ga-2">
      <v-textarea
        :model-value="draft" :label="inputLabel" variant="outlined" density="compact" rows="1" auto-grow hide-details
        bg-color="white" class="flex-grow-1"
        @update:model-value="emit('update:draft', $event)"
        @keydown.enter.ctrl.prevent="add"
      />
      <v-btn color="primary" variant="tonal" :disabled="!draft?.trim()" :loading="sending" class="mt-1" @click="add">Ajouter</v-btn>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'

const props = defineProps({
  studentId: { type: String, required: true },
  rubrique: { type: String, required: true },
  // Toutes les entrées de la rubrique, la plus récente d'abord.
  entries: { type: Array, default: () => [] },
  label: { type: String, default: '' },
  inputLabel: { type: String, default: 'Nouveau commentaire (daté et signé automatiquement)' },
  draft: { type: String, default: '' },
  maxVisible: { type: Number, default: 3 },
})
const emit = defineEmits(['update:draft', 'changed'])

const sending = ref(false)
const showAll = ref(false)
const visibleEntries = computed(() => showAll.value ? props.entries : props.entries.slice(0, props.maxVisible))

const formatDate = (d) => d ? moment(d).format('DD.MM.YYYY') : ''

async function add() {
  const text = props.draft?.trim()
  if (!text) return
  sending.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/ParticipantComments`, {
      method: 'POST',
      body: JSON.stringify({ studentId: props.studentId, rubrique: props.rubrique, text }),
    })
    if (res?.ok) {
      emit('update:draft', '')
      emit('changed')
    }
  } finally {
    sending.value = false
  }
}

async function remove(c) {
  if (!window.confirm(`Supprimer cette entrée du ${formatDate(c.createdAt)} ?\n\n« ${c.text} »`)) return
  const res = await apiFetch(`${API_BASE}/api/ParticipantComments/${c.participantCommentId}`, { method: 'DELETE' })
  if (res?.ok) emit('changed')
}
</script>

<style scoped>
.entry { border-left: 3px solid var(--rubrique-border, #e0e0e0); padding: 2px 0 4px 10px; margin-bottom: 6px; }
.entry-meta { font-size: 0.75rem; color: #6b7a90; display: flex; align-items: center; }
.entry-delete { margin-left: 4px; opacity: 0.4; }
.entry:hover .entry-delete { opacity: 1; }
.entry-text { font-size: 0.875rem; white-space: pre-line; }
</style>
