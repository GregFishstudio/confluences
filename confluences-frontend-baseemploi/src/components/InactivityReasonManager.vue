<!--
  Petit bouton "gérer" à côté d'un select "Raison de l'inactivité" — permet aux formatrices
  d'ajouter/retirer elles-mêmes les raisons proposées, sans passer par le code.
-->
<template>
  <v-dialog v-model="dialog" max-width="420">
    <template #activator="{ props }">
      <v-btn v-bind="props" icon="mdi-cog-outline" size="x-small" variant="text" title="Gérer les raisons" />
    </template>
    <v-card rounded="lg">
      <v-card-title class="py-4">Raisons d'inactivité</v-card-title>
      <v-divider />
      <v-card-text class="pt-4">
        <div v-for="r in reasons" :key="r.marrainageInactivityReasonId" class="d-flex align-center ga-2 mb-1">
          <span class="flex-grow-1">{{ r.label }}</span>
          <v-btn icon="mdi-delete-outline" size="x-small" variant="text" color="error" @click="remove(r)" />
        </div>
        <div v-if="!reasons.length" class="text-caption text-grey mb-2">Aucune raison définie.</div>
        <v-text-field
          v-model="newLabel" label="Nouvelle raison" variant="outlined" density="compact"
          class="mt-3" hide-details
          @keyup.enter="add"
        />
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
        <v-btn color="primary" :disabled="!newLabel.trim()" @click="add">Ajouter</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref } from 'vue'
import API_BASE, { apiFetch } from '@/services/api'

const props = defineProps({
  reasons: { type: Array, required: true },
})
const emit = defineEmits(['changed'])

const dialog = ref(false)
const newLabel = ref('')

async function add() {
  const label = newLabel.value.trim()
  if (!label) return
  await apiFetch(`${API_BASE}/api/MarrainageInactivityReasons`, {
    method: 'POST',
    body: JSON.stringify({ label })
  })
  newLabel.value = ''
  emit('changed')
}

async function remove(reason) {
  await apiFetch(`${API_BASE}/api/MarrainageInactivityReasons/${reason.marrainageInactivityReasonId}`, { method: 'DELETE' })
  emit('changed')
}
</script>
