<!--
  Session expirée : reconnexion sur place, sans quitter la page. Les enregistrements refusés
  attendent la reconnexion puis repartent automatiquement — la saisie n'est pas perdue.
-->
<template>
  <v-dialog :model-value="sessionState.expired" persistent max-width="420">
    <v-card rounded="lg">
      <v-card-title class="d-flex align-center pa-4">
        <v-icon start color="warning">mdi-lock-clock</v-icon>
        Session expirée
      </v-card-title>
      <v-card-text>
        <v-alert type="info" variant="tonal" density="compact" class="mb-4">
          Votre saisie est conservée. Reconnectez-vous : l'enregistrement en attente se fera ensuite automatiquement.
        </v-alert>
        <v-form @submit.prevent="submit">
          <v-text-field
            v-model="username" label="Nom d'utilisateur" variant="outlined" density="compact"
            autocomplete="username" prepend-inner-icon="mdi-account-outline"
          />
          <v-text-field
            v-model="password" label="Mot de passe" type="password" variant="outlined" density="compact"
            autocomplete="current-password" prepend-inner-icon="mdi-lock-outline" autofocus
            :error-messages="error"
          />
          <v-btn type="submit" color="primary" block :loading="loading" :disabled="!username || !password">
            Se reconnecter et continuer
          </v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import { sessionState, loginWithPassword, reloginDone } from '@/services/session'

const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

watch(() => sessionState.expired, open => {
  if (!open) return
  password.value = ''
  error.value = ''
  try {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    username.value = username.value || user.profile?.preferred_username || user.profile?.email || ''
  } catch { /* pas de profil enregistré */ }
})

async function submit() {
  loading.value = true
  error.value = ''
  try {
    if (await loginWithPassword(username.value.trim(), password.value)) reloginDone()
    else error.value = 'Nom d’utilisateur ou mot de passe incorrect'
  } catch {
    error.value = 'Serveur de connexion injoignable'
  } finally {
    loading.value = false
  }
}
</script>
