<script setup>
import { ref } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

// PROPS
const props = defineProps({
  typeEntrepriseId: {
    type: Number,
    required: true
  }
})

// STATE
const dialog = ref(false)
const validCreateContact = ref(true)
const formCreateContact = ref(null)

// CONTACT MODEL
const contact = ref({
  contactId: 0,
  createurId: null,
  dateCreation: null,
  dateModification: null,
  email: null,
  entrepriseId: props.typeEntrepriseId,
  fax: null,
  modificateurId: null,
  natel: null,
  fonction: null,
  nom: null,
  prenom: null,
  telFix: null
})

// VALIDATIONS
const nameRules = [
  v => !!v || 'Champ obligatoire',
  v => (v && v.length <= 50) || 'Maximum 50 caractères'
]

const fonctionRules = [
  v => !!v || 'La fonction est obligatoire'
]

const emailRules = [
  v => !v || /.+@.+\..+/.test(v) || 'Email invalide',
  v => !v || v.length <= 50 || 'Maximum 50 caractères'
]

const phonesRules = [
  v => !v || v.length <= 13 || 'Maximum 13 caractères'
]

// METHODS
function addNewData(data) {
  store.dispatch('entreprise/addContact', data)
}

async function submit() {
  const { valid } = await formCreateContact.value.validate()
  if (!valid) return

  NProgress.start()

  store.dispatch('contact/createContact', contact.value)
    .then(() => {
      let clone = JSON.parse(JSON.stringify(store.state.contact.contact))

      addNewData(clone)
      formCreateContact.value.reset()
      dialog.value = false
    })
    .catch(err => console.error('Erreur création contact:', err))
    .finally(() => NProgress.done())
}

function closeDialog() {
  dialog.value = false
  formCreateContact.value?.resetValidation()
}
</script>

<template>
  <v-row justify="end">
    <v-form ref="formCreateContact" v-model="validCreateContact">

      <v-dialog v-model="dialog" max-width="600" persistent>

        <!-- Bouton -->
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            class="elevation-2"
            size="small"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter un contact
          </v-btn>
        </template>

        <!-- Carte -->
        <v-card>
          <v-card-title class="py-4">
            <v-icon class="mr-2" color="primary">mdi-account-plus</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un contact</span>
          </v-card-title>

          <v-divider />

          <v-card-text>

            <v-text-field
              class="mb-4"
              v-model="contact.prenom"
              :rules="nameRules"
              variant="outlined"
              density="comfortable"
              label="Prénom"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.nom"
              :rules="nameRules"
              variant="outlined"
              density="comfortable"
              label="Nom"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.fonction"
              :rules="fonctionRules"
              variant="outlined"
              density="comfortable"
              label="Fonction"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.email"
              :rules="emailRules"
              variant="outlined"
              density="comfortable"
              label="Email"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.telFix"
              :rules="phonesRules"
              variant="outlined"
              density="comfortable"
              label="Téléphone fixe"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.natel"
              :rules="phonesRules"
              variant="outlined"
              density="comfortable"
              label="Natel"
            />

          </v-card-text>

          <v-divider />

          <!-- ACTIONS -->
          <v-card-actions class="d-flex justify-end">
            <v-btn variant="text" @click="closeDialog">Fermer</v-btn>

            <v-btn color="primary" @click="submit">
              <v-icon left small>mdi-content-save-outline</v-icon>
              Sauvegarder
            </v-btn>
          </v-card-actions>

        </v-card>
      </v-dialog>

    </v-form>
  </v-row>
</template>

<style scoped>
.v-btn {
  transition: 0.18s ease;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>
