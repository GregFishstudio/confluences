<!--
  « Changer mon mot de passe » : directement dans l'application (API /api/MotDePasse), pour le
  compte connecté. Le nouveau mot de passe vaut dès la prochaine connexion.
-->
<template>
  <v-dialog :model-value="modelValue" max-width="460" persistent @keydown.esc="fermer">
    <v-card rounded="lg">
      <v-card-title class="py-4 d-flex align-center">
        <v-icon start color="primary">mdi-lock-reset</v-icon>
        Changer mon mot de passe
      </v-card-title>
      <v-divider />

      <v-card-text class="pt-4">
        <v-alert v-if="succes" type="success" variant="tonal" density="compact" class="mb-2">
          Mot de passe changé. Utilisez-le dès votre prochaine connexion.
        </v-alert>
        <template v-else>
          <v-alert v-if="erreur" type="error" variant="tonal" density="compact" class="mb-4">{{ erreur }}</v-alert>
          <v-form ref="formRef" @submit.prevent="enregistrer">
            <v-text-field
              v-model="actuel" label="Mot de passe actuel" :type="voir ? 'text' : 'password'"
              autocomplete="current-password" variant="outlined" density="compact" class="mb-2"
              :rules="[v => !!v || 'Obligatoire']" autofocus
            />
            <v-text-field
              v-model="nouveau" label="Nouveau mot de passe" :type="voir ? 'text' : 'password'"
              autocomplete="new-password" variant="outlined" density="compact" class="mb-2"
              :rules="[v => !!v || 'Obligatoire', v => !v || v.length >= longueurMin || `Au moins ${longueurMin} caractères`]"
              :hint="reglesTexte" persistent-hint
            />
            <v-text-field
              v-model="confirmation" label="Confirmer le nouveau mot de passe" :type="voir ? 'text' : 'password'"
              autocomplete="new-password" variant="outlined" density="compact" class="mt-2"
              :rules="[v => v === nouveau || 'Ne correspond pas au nouveau mot de passe']"
              :append-inner-icon="voir ? 'mdi-eye-off' : 'mdi-eye'" @click:append-inner="voir = !voir"
            />
            <div class="text-caption text-medium-emphasis mt-2">
              Ce mot de passe est personnel : ne le partagez pas avec vos collègues.
            </div>
            <!-- Entrée valide le formulaire -->
            <button type="submit" hidden />
          </v-form>
        </template>
      </v-card-text>

      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="fermer">{{ succes ? 'Fermer' : 'Annuler' }}</v-btn>
        <v-btn v-if="!succes" color="primary" :loading="enCours" @click="enregistrer">Changer le mot de passe</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import API_BASE, { apiFetch } from '@/services/api'
import { readApiError } from '@/utils/inscriptionOptions'

const props = defineProps({ modelValue: { type: Boolean, default: false } })
const emit = defineEmits(['update:modelValue'])

const formRef = ref(null)
const actuel = ref('')
const nouveau = ref('')
const confirmation = ref('')
const voir = ref(false)
const enCours = ref(false)
const erreur = ref('')
const succes = ref(false)
const regles = ref(null)

const longueurMin = computed(() => regles.value?.longueurMin || 6)
const reglesTexte = computed(() => {
  const r = regles.value
  if (!r) return `Au moins ${longueurMin.value} caractères.`
  const parts = [`au moins ${r.longueurMin} caractères`]
  if (r.majuscule) parts.push('une majuscule')
  if (r.minuscule) parts.push('une minuscule')
  if (r.chiffre) parts.push('un chiffre')
  if (r.caractereSpecial) parts.push('un caractère spécial (ex. ! ? * -)')
  return `Il faut ${parts.join(', ')}.`
})

watch(() => props.modelValue, async (ouvert) => {
  if (!ouvert) return
  actuel.value = ''
  nouveau.value = ''
  confirmation.value = ''
  erreur.value = ''
  succes.value = false
  voir.value = false
  if (!regles.value) {
    const res = await apiFetch(`${API_BASE}/api/MotDePasse/regles`)
    if (res?.ok) regles.value = await res.json()
  }
})

function fermer() {
  emit('update:modelValue', false)
}

async function enregistrer() {
  erreur.value = ''
  const { valid } = await formRef.value.validate()
  if (!valid) return
  enCours.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/MotDePasse`, {
      method: 'POST',
      body: JSON.stringify({ actuel: actuel.value, nouveau: nouveau.value }),
    })
    if (!res) { erreur.value = 'Session expirée : reconnectez-vous puis réessayez.'; return }
    if (!res.ok) { erreur.value = await readApiError(res, "Le mot de passe n'a pas pu être changé."); return }
    succes.value = true
  } catch {
    erreur.value = 'Connexion au serveur impossible. Réessayez.'
  } finally {
    enCours.value = false
  }
}
</script>
