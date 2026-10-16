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
      />
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  questions: { type: Array, default: () => [] },
  modelValue: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['update:modelValue'])

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
