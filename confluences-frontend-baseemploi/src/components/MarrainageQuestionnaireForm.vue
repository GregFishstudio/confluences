<!--
  Rendu du questionnaire d'inscription parrain/marraine (dynamique, basé sur les questions
  configurées via MarrainageQuestionManager).
-->
<template>
  <div>
    <div v-for="q in questions" :key="q.marrainageQuestionId" class="mb-3">
      <!-- Liste de choix ; si les options de la question sont absentes ou mal séparées, on
           propose Oui / Non et la saisie libre reste possible (sinon la liste était vide et
           aucune réponse ne pouvait être choisie). -->
      <v-combobox
        v-if="q.type === 'choice'"
        :model-value="modelValue[q.marrainageQuestionId]"
        @update:model-value="(v) => update(q.marrainageQuestionId, v)"
        :items="choiceItems(q)"
        :label="q.label"
        :required="q.required"
        variant="outlined" density="compact" clearable
        :hint="q.label.length > 70 ? q.label : ''" persistent-hint
      />
      <v-textarea
        v-else-if="q.type === 'textarea'"
        :model-value="modelValue[q.marrainageQuestionId]"
        @update:model-value="(v) => update(q.marrainageQuestionId, v)"
        :label="q.label"
        :required="q.required"
        variant="outlined" auto-grow rows="2"
      />
      <v-text-field
        v-else
        :model-value="modelValue[q.marrainageQuestionId]"
        @update:model-value="(v) => update(q.marrainageQuestionId, v)"
        :type="q.type === 'number' ? 'number' : 'text'"
        :label="q.label"
        :required="q.required"
        variant="outlined" density="compact"
        :placeholder="estDateNaissance(q) ? 'JJ.MM.AAAA' : undefined"
        :hint="estDateNaissance(q) ? lectureDateNaissance(modelValue[q.marrainageQuestionId]) : undefined"
        :persistent-hint="estDateNaissance(q)"
      />
    </div>
  </div>
</template>

<script setup>
import moment from 'moment/min/moment-with-locales'
import { parseDateFr, ageDepuis } from '@/utils/dates'
const props = defineProps({
  questions: { type: Array, default: () => [] },
  modelValue: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['update:modelValue'])

// Date de naissance : la date lue et l'âge s'affichent pendant la saisie ; ils rempliront la
// fiche à la création.
const estDateNaissance = (q) => /naissance/i.test(q.label || '')
function lectureDateNaissance(value) {
  if (!value || !String(value).trim()) return 'Format JJ.MM.AAAA : la date et l’âge s’afficheront sur la fiche.'
  const iso = parseDateFr(String(value))
  if (!iso) return 'Date non reconnue : utilisez le format JJ.MM.AAAA (ex. 03.03.1992).'
  const age = ageDepuis(iso)
  return `${moment(iso).format('DD.MM.YYYY')}${age != null ? ` · ${age} ans` : ''}`
}

// Options séparées par "|" (format prévu), mais aussi ";", " / " ou retour à la ligne.
function parseOptions(raw) {
  return (raw || '').split(/\||;|\n|\s\/\s/).map(o => o.trim()).filter(Boolean)
}

function choiceItems(q) {
  const options = parseOptions(q.options)
  const items = options.length >= 2 ? options : ['Oui', 'Non', ...options]
  // La réponse déjà enregistrée reste visible même si elle ne fait pas partie des options.
  const current = props.modelValue[q.marrainageQuestionId]
  return current && !items.includes(current) ? [...items, current] : items
}

function update(questionId, value) {
  emit('update:modelValue', { ...props.modelValue, [questionId]: value })
}
</script>
