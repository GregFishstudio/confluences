<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const router = useRouter()

const validCreateEntreprise = ref(true)
const dialog = ref(false)
const menuCreation = ref(false)
const formCreateEntreprise = ref(null)

const entreprise = ref({
  entrepriseId: 0,
  nom: null,
  ville: null,
  telFix: null,
  telNatel: null,
  adr1: null,
  adr2: null,
  codePostal: null,
  email: null,
  remarque: null,
  dateCreation: null,
  typeEntrepriseId: null,
  typeDomaineId: null,
  typeMoyenId: null,
  dateDernierContact: null,
  createurId: null,
  formateurIdDernierContact: null,
  stagiaireIdDernierContact: null
})

// VALIDATIONS
const nameRules = [
  v => !!v || 'Le champ est obligatoire',
  v => (v && v.length <= 50) || 'Maximum 50 caractères'
]

const adressRules = [
  v => !v || v.length <= 50 || 'Maximum 50 caractères'
]

const codePostalRules = v => {
  if (!v) return true
  if (v >= 0 && v <= 9999) return true
  return 'En Suisse: 4 chiffres'
}

const emailRules = [
  v => !v || /.+@.+\..+/.test(v) || "Email invalide",
  v => !v || v.length <= 50 || 'Maximum 50 caractères'
]

const phonesRules = [
  v => !v || v.length <= 13 || 'Maximum 13 caractères'
]

// SUBMIT
async function submit() {
  const { valid } = await formCreateEntreprise.value.validate()

  if (!valid) return

  NProgress.start()

  store.dispatch('entreprise/createEntreprise', entreprise.value)
    .then(response => {
      formCreateEntreprise.value.reset()

      router.push({
        name: 'Entreprise-Modifier',
        params: { id: response.entrepriseId }
      })
    })
    .catch(error => {
      console.error("Erreur lors de la création:", error)
    })
    .finally(() => {
      dialog.value = false
      NProgress.done()
    })
}

// CLOSE
function closeDialog() {
  dialog.value = false
  if (formCreateEntreprise.value) {
    formCreateEntreprise.value.reset()
    formCreateEntreprise.value.resetValidation()
  }
}
</script>

<template>
  <v-row justify="end">
    <v-form
      ref="formCreateEntreprise"
      v-model="validCreateEntreprise"
      lazy-validation
    >
      <v-dialog v-model="dialog" max-width="720" persistent>

        <!-- Bouton -->
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            color="primary"
            class="elevation-2"
          >
            <v-icon left>mdi-plus</v-icon>
            Ajouter une entreprise
          </v-btn>
        </template>

        <!-- Carte -->
        <v-card>
          <v-card-title class="py-4">
            <v-icon class="mr-2" color="primary">mdi-domain-plus</v-icon>
            <span class="text-h6 font-weight-bold">Ajouter une entreprise</span>
          </v-card-title>

          <v-divider />

          <v-card-text>

            <v-text-field
              class="mb-4"
              v-model="entreprise.nom"
              :rules="nameRules"
              variant="outlined"
              density="comfortable"
              label="Nom"
              required
            />

            <v-text-field
              class="mb-4"
              v-model="entreprise.adr1"
              :rules="adressRules"
              variant="outlined"
              density="comfortable"
              label="Adresse"
            />

            <v-text-field
              class="mb-4"
              v-model="entreprise.adr2"
              :rules="adressRules"
              variant="outlined"
              density="comfortable"
              label="Complément d'adresse"
            />

            <v-text-field
              class="mb-4"
              v-model="entreprise.ville"
              :rules="nameRules"
              variant="outlined"
              density="comfortable"
              label="Ville"
              required
            />

            <v-row>
              <v-col cols="12" sm="6">
                <v-text-field
                  class="mb-4"
                  v-model="entreprise.codePostal"
                  :rules="[codePostalRules]"
                  variant="outlined"
                  density="comfortable"
                  label="Code postal"
                />
              </v-col>

              <v-col cols="12" sm="6">
                <v-menu v-model="menuCreation" max-width="300">
                  <template #activator="{ props }">
                    <v-text-field
                      class="mb-4"
                      v-model="entreprise.dateCreation"
                      variant="outlined"
                      density="comfortable"
                      label="Chez Confluences depuis"
                      readonly
                      v-bind="props"
                    />
                  </template>

                  <v-card>
                    <v-date-picker
                      v-model="entreprise.dateCreation"
                      @update:model-value="menuCreation = false"
                    />
                  </v-card>
                </v-menu>
              </v-col>
            </v-row>

            <v-text-field
              class="mb-4"
              v-model="entreprise.email"
              :rules="emailRules"
              variant="outlined"
              density="comfortable"
              label="Email"
            />

            <v-row>
              <v-col cols="12" sm="6">
                <v-text-field
                  class="mb-4"
                  v-model="entreprise.telFix"
                  :rules="phonesRules"
                  variant="outlined"
                  density="comfortable"
                  label="Téléphone fixe"
                />
              </v-col>

              <v-col cols="12" sm="6">
                <v-text-field
                  class="mb-4"
                  v-model="entreprise.telNatel"
                  :rules="phonesRules"
                  variant="outlined"
                  density="comfortable"
                  label="Natel"
                />
              </v-col>
            </v-row>

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
  box-shadow: 0px 4px 10px rgba(0,0,0,0.12);
}
</style>
