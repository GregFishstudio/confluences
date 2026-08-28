<template>
  <v-tooltip :text="label ? `Historique — ${label}` : 'Historique'" location="top">
    <template #activator="{ props: tooltipProps }">
      <v-btn v-bind="tooltipProps" icon="mdi-history" size="small" variant="text" @click="open" />
    </template>
  </v-tooltip>

  <v-dialog v-model="dialog" max-width="520">
    <v-card rounded="lg">
      <v-card-title class="py-4">Historique{{ label ? ` — ${label}` : '' }}</v-card-title>
      <v-divider />
      <v-card-text class="pt-4">
        <div v-if="loading" class="text-center pa-4">
          <v-progress-circular indeterminate color="primary" size="28" />
        </div>
        <div v-else-if="!entries.length" class="text-body-2 text-medium-emphasis">
          Aucun changement enregistré pour ce champ.
        </div>
        <div v-else>
          <div v-for="entry in entries" :key="entry.fieldChangeHistoryId" class="history-entry mb-3 pb-3">
            <div class="text-caption text-grey">
              {{ formatDate(entry.changedAt) }}<span v-if="entry.changedByName"> — par {{ entry.changedByName }}</span>
            </div>
            <div class="text-body-2">
              <span class="text-medium-emphasis">{{ entry.oldValue || '(vide)' }}</span>
              <v-icon size="14" class="mx-1">mdi-arrow-right</v-icon>
              <span class="font-weight-medium">{{ entry.newValue || '(vide)' }}</span>
            </div>
          </div>
        </div>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref } from 'vue'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'

moment.locale('fr')

const props = defineProps({
  entityType: { type: String, required: true },
  entityId: { type: String, required: true },
  fieldName: { type: String, required: true },
  label: { type: String, default: '' },
})

const dialog = ref(false)
const loading = ref(false)
const entries = ref([])

const formatDate = (d) => d ? moment(d).format('DD.MM.YYYY à HH:mm') : '—'

async function open() {
  dialog.value = true
  loading.value = true
  try {
    const params = new URLSearchParams({
      entityType: props.entityType,
      entityId: props.entityId,
      fieldName: props.fieldName,
    })
    const res = await apiFetch(`${API_BASE}/api/FieldChangeHistories?${params.toString()}`)
    if (res && res.ok) {
      entries.value = await res.json()
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.history-entry { border-bottom: 1px solid #eee; }
.history-entry:last-child { border-bottom: none; }
</style>
