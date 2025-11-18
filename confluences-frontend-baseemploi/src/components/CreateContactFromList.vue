<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

/* --- STATE --- */
const dialog = ref(false)
const validCreateContact = ref(true)
const formCreateContact = ref(null)

/* --- ENTREPRISES (Vuex) --- */
const entreprisesState = computed(
  () => store.state.entreprise.entreprises ?? []
)

onBeforeMount(() => {
  store.dispatch('entreprise/fetchEntreprises').catch(err =>
    console.error("Erreur chargement entreprises:", err)
  )
})

/* --- MODEL --- */
const contact = ref({
  contactId: 0,
  createurId: null,
  dateCreation: null,
  dateModification: null,
  email: null,
  entrepriseId: null,
  fax: null,
  modificateurId: null,
  natel: null,
  fonction: null,
  nom: null,
  prenom: null,
  telFix: null
})

/* --- RULES --- */
const nameRules = [
  v => !!v || 'Champ obligatoire',
  v => (v && v.length <= 50) || 'Maximum 50 caractères'
]

const emailRules = [
  v => !v || /.+@.+\..+/.test(v) || 'Email invalide',
  v => !v || v.length <= 50 || 'Maximum 50 caractères'
]

const phonesRules = [
  v => !v || v.length <= 13 || 'Maximum 13 caractères'
]

/* --- METHODS --- */
function addNewData(data) {
  store.dispatch('entreprise/addContact', data)
}

async function submit() {
  const { valid } = await formCreateContact.value.validate()
  if (!valid) return

  NProgress.start()

  store
    .dispatch('contact/createContact', contact.value)
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
  <v-row justify="start">
    <v-form ref="formCreateContact" v-model="validCreateContact">

      <v-dialog v-model="dialog" max-width="600" persistent>
        <!-- Bouton -->
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            class="elevation-2"
          >
            <v-icon left>mdi-account-plus</v-icon>
            Ajouter un contact
          </v-btn>
        </template>

        <!-- CONTENU -->
        <v-card>

          <v-card-title class="py-4">
            <v-icon class="mr-2" color="primary">mdi-account-plus</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter un contact</span>
          </v-card-title>

          <v-divider />

          <v-card-text>

            <v-autocomplete
              class="mb-4"
              v-model="contact.entrepriseId"
              :items="entreprisesState"
              item-title="nom"
              item-value="entrepriseId"
              :rules="[v => !!v || 'Entreprise obligatoire']"
              label="Entreprise"
              required
              variant="outlined"
              density="comfortable"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.prenom"
              :rules="nameRules"
              label="Prénom"
              variant="outlined"
              density="comfortable"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.nom"
              :rules="nameRules"
              label="Nom"
              variant="outlined"
              density="comfortable"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.fonction"
              :rules="nameRules"
              label="Fonction"
              variant="outlined"
              density="comfortable"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.email"
              :rules="emailRules"
              label="Email"
              variant="outlined"
              density="comfortable"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.telFix"
              :rules="phonesRules"
              label="Téléphone fixe"
              variant="outlined"
              density="comfortable"
            />

            <v-text-field
              class="mb-4"
              v-model="contact.natel"
              :rules="phonesRules"
              label="Natel"
              variant="outlined"
              density="comfortable"
            />

          </v-card-text>

          <v-divider />

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
