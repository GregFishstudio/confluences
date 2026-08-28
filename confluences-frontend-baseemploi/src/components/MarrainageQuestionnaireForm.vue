<!--
  Rendu du questionnaire d'inscription parrain/marraine (dynamique, basé sur les questions
  configurées via MarrainageQuestionManager).
-->
<template>
  <div>
    <div v-for="q in questions" :key="q.marrainageQuestionId" class="mb-3">
      <v-select
        v-if="q.type === 'choice'"
        :model-value="modelValue[q.marrainageQuestionId]"
        @update:model-value="(v) => update(q.marrainageQuestionId, v)"
        :items="(q.options || '').split('|').map(o => o.trim()).filter(Boolean)"
        :label="q.label"
        :required="q.required"
        variant="outlined" density="compact" clearable
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

function update(questionId, value) {
  emit('update:modelValue', { ...props.modelValue, [questionId]: value })
}
</script>
