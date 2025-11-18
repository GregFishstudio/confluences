<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

// Props
const props = defineProps({
  contact: {
    type: Object,
    required: true
  }
})

// Router (au cas où nécessaire)
const router = useRouter()

// Dialog + Form
const dialog = ref(false)
const valid = ref(true)
const formEdit = ref(null)

// Règles
const nameRules = [
  v => !!v || 'Le nom est obligatoire',
  v => !v || v.length <= 50 || 'Maximum 50 caractères'
]

const emailRules = [
  v => !v || /.+@.+\..+/.test(v) || "L'email doit être valide",
  v => !v || v.length <= 50 || 'Maximum 50 caractères'
]

const phoneRules = [
  v => !v || v.length <= 13 || 'Maximum 13 caractères'
]

/**
 * Sauvegarde les modifications
 */
async function submit() {
  const { valid: isValid } = await formEdit.value.validate()
  if (!isValid) return

  NProgress.start()

  try {
    // 1. Modifier le contact dans son module
    await store.dispatch('contact/editContact', props.contact)

    // 2. Modifier le contact dans l’entreprise (ancien EditData)
    const clone = JSON.parse(JSON.stringify(props.contact))
    await store.dispatch('entreprise/editContact', clone)

    dialog.value = false
  } catch (e) {
    console.error('Erreur lors de la sauvegarde du contact:', e)
  } finally {
    NProgress.done()
  }
}
</script>

<template>
  <v-row justify="center">
    <v-form ref="formEdit" v-model="valid" lazy-validation>
      <v-dialog v-model="dialog" max-width="600">

        <!-- Bouton d'ouverture -->
        <template #activator="{ props: activatorProps }">
          <v-btn
            v-bind="activatorProps"
            icon
            color="primary"
            variant="tonal"
            class="ma-1"
          >
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
        </template>

        <v-card rounded="lg">
          <v-card-title class="py-4">
            <span class="text-h6 font-weight-bold">Modifier un contact</span>
          </v-card-title>

          <v-divider />

          <v-card-text class="pt-6">

            <v-text-field
              v-model="props.contact.prenom"
              label="Prénom"
              :rules="nameRules"
              :counter="50"
              required
            />

            <v-text-field
              v-model="props.contact.nom"
              label="Nom"
              :rules="nameRules"
              :counter="50"
              required
            />

            <v-text-field
              v-model="props.contact.fonction"
              label="Fonction"
              :rules="nameRules"
              :counter="50"
              required
            />

            <v-text-field
              v-model="props.contact.email"
              label="Email"
              :rules="emailRules"
              :counter="50"
            />

            <v-text-field
              v-model="props.contact.telFix"
              label="Téléphone fixe"
              :rules="phoneRules"
              :counter="13"
            />

            <v-text-field
              v-model="props.contact.natel"
              label="Natel"
              :rules="phoneRules"
              :counter="13"
            />

          </v-card-text>

          <v-divider />

          <v-card-actions class="d-flex justify-end">
            <v-btn variant="text" color="grey" @click="dialog = false">
              Fermer
            </v-btn>

            <v-btn color="blue-darken-1" variant="flat" @click="submit">
              Sauvegarder
            </v-btn>
          </v-card-actions>
        </v-card>

      </v-dialog>
    </v-form>
  </v-row>
</template>
