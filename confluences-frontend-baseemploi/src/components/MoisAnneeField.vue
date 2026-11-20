<!--
  Date au mois près (début / fin d'un marrainage) : un choix de mois et un choix d'année.
  La valeur (v-model) reste une date AAAA-MM-JJ, toujours le 1er du mois ; null si vide.
-->
<template>
  <div class="mois-annee">
    <div v-if="label" class="text-caption text-medium-emphasis mb-1">{{ label }}</div>
    <div class="d-flex ga-1">
      <v-select
        :model-value="mois" :items="moisItems" placeholder="Mois"
        variant="outlined" density="compact" hide-details :clearable="clearable" class="flex-grow-1"
        :bg-color="bgColor" :disabled="disabled"
        @update:model-value="(m) => changer(m, annee)"
      />
      <v-select
        :model-value="annee" :items="anneeItems" placeholder="Année"
        variant="outlined" density="compact" hide-details style="max-width: 110px"
        :bg-color="bgColor" :disabled="disabled || !mois"
        @update:model-value="(a) => changer(mois, a)"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { MOIS_LONGS } from '@/utils/dates'

const props = defineProps({
  modelValue: { type: String, default: null },
  label: { type: String, default: '' },
  clearable: { type: Boolean, default: true },
  bgColor: { type: String, default: undefined },
  disabled: { type: Boolean, default: false },
})
const emit = defineEmits(['update:modelValue'])

const match = computed(() => (props.modelValue || '').match(/^(\d{4})-(\d{2})/))
const mois = computed(() => (match.value ? Number(match.value[2]) : null))
const annee = computed(() => (match.value ? Number(match.value[1]) : null))

const moisItems = MOIS_LONGS.map((title, i) => ({ title, value: i + 1 }))
const anneeItems = computed(() => {
  const courante = new Date().getFullYear()
  const annees = []
  for (let a = courante + 1; a >= 2015; a--) annees.push(a)
  if (annee.value && !annees.includes(annee.value)) annees.push(annee.value)
  return annees
})

// Mois effacé : plus de date. Mois choisi sans année : année en cours (modifiable ensuite).
function changer(m, a) {
  if (!m) return emit('update:modelValue', null)
  const an = a || new Date().getFullYear()
  emit('update:modelValue', `${an}-${String(m).padStart(2, '0')}-01`)
}
</script>
