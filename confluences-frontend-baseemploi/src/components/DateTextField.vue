<!--
  Champ de date saisi au clavier ou par copier-coller (JJ.MM.AAAA, JJ/MM/AAAA, JJ-MM-AAAA,
  AAAA-MM-JJ, JJMMAAAA, année sur 2 chiffres acceptée). La valeur (v-model) reste au format
  AAAA-MM-JJ, comme un <input type="date">.
-->
<template>
  <v-text-field
    v-bind="$attrs"
    :model-value="text"
    placeholder="JJ.MM.AAAA"
    :error-messages="invalid ? 'Date invalide (format JJ.MM.AAAA)' : ($attrs['error-messages'] ?? $attrs.errorMessages)"
    @update:model-value="onInput"
    @blur="onBlur"
  />
</template>

<script setup>
import { ref, watch } from 'vue'
import moment from 'moment/min/moment-with-locales'
import { parseDateFr as parse } from '@/utils/dates'

defineOptions({ inheritAttrs: false })

const props = defineProps({
  modelValue: { type: String, default: null },
})
const emit = defineEmits(['update:modelValue'])

const toDisplay = (iso) => (iso ? moment(iso, 'YYYY-MM-DD').format('DD.MM.YYYY') : '')

const text = ref(toDisplay(props.modelValue))
const invalid = ref(false)

watch(() => props.modelValue, (iso) => {
  if (parse(text.value) !== iso) text.value = toDisplay(iso)
})

function onInput(value) {
  text.value = value
  const iso = parse(value)
  if (iso !== undefined) {
    invalid.value = false
    if (iso !== props.modelValue) emit('update:modelValue', iso)
  }
}

function onBlur() {
  const iso = parse(text.value)
  invalid.value = iso === undefined
  if (iso) text.value = toDisplay(iso)
}
</script>
