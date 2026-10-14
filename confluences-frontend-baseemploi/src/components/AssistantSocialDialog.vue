<!--
  Création / modification d'une fiche assistant·e social·e — utilisé par l'onglet
  "Assistant·e·s sociaux·ales" et en ajout rapide depuis une candidature.
-->
<template>
  <v-dialog :model-value="modelValue" max-width="560" @update:model-value="emit('update:modelValue', $event)">
    <v-card rounded="lg">
      <v-card-title class="py-4">{{ assistant ? 'Modifier' : 'Nouvel·le' }} assistant·e social·e</v-card-title>
      <v-divider />
      <v-card-text class="pt-4">
        <v-row dense>
          <v-col cols="6"><v-text-field v-model="form.prenom" label="Prénom" variant="outlined" density="compact" /></v-col>
          <v-col cols="6"><v-text-field v-model="form.nom" label="Nom" variant="outlined" density="compact" /></v-col>
          <v-col cols="12"><v-text-field v-model="form.email" label="Email *" type="email" variant="outlined" density="compact" /></v-col>
          <v-col cols="6"><v-text-field v-model="form.telephone" label="Téléphone" variant="outlined" density="compact" /></v-col>
          <v-col cols="6">
            <v-combobox v-model="form.organisme" :items="organismes" label="Service / organisme" variant="outlined" density="compact" />
          </v-col>
          <v-col cols="12"><v-textarea v-model="form.remarques" label="Remarques" rows="2" auto-grow variant="outlined" density="compact" /></v-col>
          <v-col cols="12"><v-switch v-model="form.actif" label="Actif·ve" color="primary" hide-details density="compact" /></v-col>
        </v-row>
        <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mt-2">{{ error }}</v-alert>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="emit('update:modelValue', false)">Annuler</v-btn>
        <v-btn color="primary" :loading="saving" :disabled="!form.email?.trim()" @click="save">Enregistrer</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import API_BASE, { apiFetch } from '@/services/api'
import { readApiError } from '@/utils/inscriptionOptions'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  assistant: { type: Object, default: null },
})
const emit = defineEmits(['update:modelValue', 'saved'])

const organismes = ['CSIR', 'CSR', 'ARAS', 'EVAM']

const empty = () => ({ prenom: '', nom: '', email: '', telephone: '', organisme: null, remarques: '', actif: true })
const form = ref(empty())
const saving = ref(false)
const error = ref('')

watch(() => props.modelValue, (open) => {
  if (!open) return
  error.value = ''
  form.value = props.assistant ? { ...empty(), ...props.assistant } : empty()
})

async function save() {
  saving.value = true
  error.value = ''
  try {
    const id = props.assistant?.assistantSocialId
    const res = await apiFetch(`${API_BASE}/api/AssistantsSociaux${id ? `/${id}` : ''}`, {
      method: id ? 'PUT' : 'POST',
      body: JSON.stringify(form.value),
    })
    if (!res) return
    if (!res.ok) {
      error.value = await readApiError(res)
      return
    }
    const saved = id ? { ...form.value, assistantSocialId: id } : await res.json()
    emit('saved', saved)
    emit('update:modelValue', false)
  } finally {
    saving.value = false
  }
}
</script>
