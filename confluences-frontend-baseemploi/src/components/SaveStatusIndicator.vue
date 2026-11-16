<!--
  État des enregistrements de toute l'application, toujours au même endroit (en bas à gauche) :
  « Enregistrement… », « Enregistré à 14:32 », ou « Non enregistré : raison » tant que le
  problème n'est pas résolu — remplace les messages éphémères après chaque enregistrement.
-->
<template>
  <transition name="fade">
    <div v-if="visible" class="save-status" :class="`save-status--${state}`" role="status" aria-live="polite">
      <v-progress-circular v-if="state === 'saving'" indeterminate size="14" width="2" class="mr-2" />
      <v-icon v-else size="18" class="mr-2">{{ state === 'error' ? 'mdi-alert-circle' : 'mdi-check-circle' }}</v-icon>
      <span>{{ label }}</span>
      <v-btn v-if="state === 'error'" icon size="x-small" variant="text" class="ml-1" title="Masquer" @click="saveState.error = null">
        <v-icon size="16">mdi-close</v-icon>
      </v-btn>
    </div>
  </transition>
</template>

<script setup>
import { computed, ref, onMounted, onBeforeUnmount } from 'vue'
import moment from 'moment/min/moment-with-locales'
import { saveState } from '@/services/session'

// Rafraîchit « il y a … » sans dépendre d'un nouvel enregistrement.
const now = ref(Date.now())
let timer = null
onMounted(() => { timer = setInterval(() => { now.value = Date.now() }, 30000) })
onBeforeUnmount(() => clearInterval(timer))

const state = computed(() => saveState.pending > 0 ? 'saving' : saveState.error ? 'error' : 'saved')
const visible = computed(() => state.value !== 'saved' || !!saveState.lastSavedAt)
const label = computed(() => {
  if (state.value === 'saving') return 'Enregistrement…'
  if (state.value === 'error') return `Non enregistré : ${saveState.error}`
  void now.value
  return `Enregistré à ${moment(saveState.lastSavedAt).format('HH:mm')}`
})
</script>

<style scoped>
.save-status {
  position: fixed; left: 16px; bottom: 16px; z-index: 2400;
  display: flex; align-items: center; max-width: min(520px, calc(100vw - 32px));
  padding: 8px 14px; border-radius: 20px; font-size: .85rem; font-weight: 600;
  box-shadow: 0 2px 10px rgba(0, 0, 0, .15);
}
.save-status--saving { background: #fff8e1; color: #8d6e00; }
.save-status--saved  { background: #e8f5e9; color: #2e7d32; }
.save-status--error  { background: #ffebee; color: #c62828; }
.fade-enter-active, .fade-leave-active { transition: opacity .2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
@media print { .save-status { display: none; } }
</style>
