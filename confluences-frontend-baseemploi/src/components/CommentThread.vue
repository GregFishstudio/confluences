<!--
  Fil de commentaires horodatés multi-auteurs, affichés en "bulles" de discussion.
-->
<template>
  <v-card class="pa-4 elevation-1 rounded-lg" variant="outlined">
    <div class="d-flex align-center mb-4">
      <v-icon start color="primary">mdi-comment-text-multiple-outline</v-icon>
      <span class="text-subtitle-1 font-weight-bold">{{ title }}</span>
    </div>

    <div class="comment-list mb-4">
      <div v-if="!comments.length" class="text-body-2 text-medium-emphasis text-center pa-4">
        {{ emptyText }}
      </div>
      <div v-for="c in comments" :key="c[idKey]" class="comment-bubble mb-3">
        <div class="d-flex align-center mb-1">
          <v-avatar size="24" color="primary" class="mr-2">
            <span class="text-caption text-white">{{ authorInitials(c) }}</span>
          </v-avatar>
          <span class="text-caption font-weight-bold mr-2">{{ authorName(c) }}</span>
          <span class="text-caption text-grey">{{ formatDate(c.createdAt) }}</span>
          <v-spacer />
          <v-icon size="16" color="grey" style="cursor: pointer" @click="emit('delete', c[idKey])">
            mdi-delete-outline
          </v-icon>
        </div>
        <div class="bubble-text text-body-2">{{ c.text }}</div>
      </div>
    </div>

    <v-divider class="mb-3" />

    <v-textarea
      v-model="newText"
      :label="inputLabel"
      variant="outlined"
      rows="2"
      auto-grow
      hide-details="auto"
      class="mb-2"
    />
    <div class="d-flex justify-end">
      <v-btn color="primary" :disabled="!newText.trim()" :loading="sending" @click="submit">
        Envoyer
      </v-btn>
    </div>
  </v-card>
</template>

<script setup>
import { ref } from 'vue'
import moment from 'moment/min/moment-with-locales'

moment.locale('fr')

const props = defineProps({
  comments: { type: Array, default: () => [] },
  // Réutilisé ailleurs que pour les marrainages (ex. remarques AS d'une candidature).
  idKey: { type: String, default: 'marrainageCommentId' },
  title: { type: String, default: 'Commentaires' },
  emptyText: { type: String, default: 'Aucun commentaire pour le moment.' },
  inputLabel: { type: String, default: 'Ajouter un commentaire' },
})

const emit = defineEmits(['submit', 'delete'])

const newText = ref('')
const sending = ref(false)

function authorName(c) {
  const a = c.author
  if (!a) return 'Utilisateur'
  return `${a.firstname || ''} ${a.lastName || ''}`.trim() || a.userName || 'Utilisateur'
}

function authorInitials(c) {
  const name = authorName(c)
  return name.split(' ').filter(Boolean).map(p => p[0]).join('').slice(0, 2).toUpperCase() || 'U'
}

function formatDate(d) {
  return d ? moment(d).format('DD.MM.YYYY HH:mm') : ''
}

function submit() {
  if (!newText.value.trim()) return
  emit('submit', newText.value.trim())
  newText.value = ''
}
</script>

<style scoped>
.comment-list { max-height: 340px; overflow-y: auto; }
.comment-bubble {
  background: rgba(25, 118, 210, 0.06);
  border-radius: 12px;
  padding: 10px 14px;
}
.bubble-text { white-space: pre-wrap; }
</style>
