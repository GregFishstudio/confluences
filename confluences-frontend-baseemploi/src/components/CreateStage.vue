<template>
  <v-row class="d-flex justify-end">
    <v-dialog v-model="dialog" max-width="720" persistent>

      <!-- Bouton -->
      <template #activator="{ props }">
        <v-btn
          v-bind="props"
          color="primary"
          class="elevation-2"
        >
          <v-icon left>mdi-plus</v-icon>
          Ajouter une expérience
        </v-btn>
      </template>

      <!-- Carte -->
      <v-card>
        <v-card-title class="py-4">
          <v-icon color="primary" class="mr-2">mdi-school-outline</v-icon>
          <span class="text-h6 font-weight-bold">Ajouter un stage</span>
        </v-card-title>

        <v-divider />

        <!-- Form -->
        <v-card-text>
          <v-form ref="formCreateStage" v-model="validCreateStage">

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeIntershipActivityId"
              :items="typeIntershipActivity.value?.typeIntershipActivities ?? []"
              item-title="nom"
              item-value="typeIntershipActivityId"
              label="Type d'activité"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeStageId"
              :items="typeStage.value?.typeStages ?? []"
              item-title="nom"
              item-value="typeStageId"
              label="Taux d'occupation"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.createurId"
              :items="user.value?.users ?? []"
              item-title="nom"
              item-value="id"
              label="Créateur·trice"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeAnnonceId"
              :items="typeAnnonce.value?.typeAnnonces ?? []"
              item-title="libelle"
              item-value="typeAnnonceId"
              label="Stage à annoncer"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              clearable
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeMetierId"
              :items="typeMetier.value?.typeMetiers ?? []"
              item-title="libelle"
              item-value="typeMetierId"
              label="Métier"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.entrepriseId"
              :items="entreprise.value?.entreprises ?? []"
              item-title="nom"
              item-value="entrepriseId"
              label="Entreprise"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              clearable
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.stagiaireId"
              :items="user.value?.users ?? []"
              item-title="nom"
              item-value="id"
              label="Stagiaire"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
            />

            <!-- Début -->
            <v-text-field
              class="mb-4"
              v-model="stage.debut"
              label="Début"
              variant="outlined"
              density="comfortable"
              readonly
              :rules="requiredRule"
              @click="menuDebut = true"
            />

            <v-dialog v-model="menuDebut" max-width="300">
              <v-card>
                <v-date-picker
                  v-model="stage.debut"
                  @update:model-value="menuDebut = false"
                />
              </v-card>
            </v-dialog>

            <!-- Fin -->
            <v-text-field
              class="mb-4"
              v-model="stage.fin"
              label="Fin"
              variant="outlined"
              density="comfortable"
              readonly
              :rules="dateFinRules"
              @click="menuFin = true"
            />

            <v-dialog v-model="menuFin" max-width="300">
              <v-card>
                <v-date-picker
                  v-model="stage.fin"
                  @update:model-value="menuFin = false"
                />
              </v-card>
            </v-dialog>

            <v-autocomplete
              class="mb-4"
              v-model="stage.sessionId"
              :items="session.value?.sessions ?? []"
              item-title="description"
              item-value="sessionId"
              label="Session"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              clearable
            />

          </v-form>
        </v-card-text>

        <v-divider />

        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" @click="dialog = false">Annuler</v-btn>

          <v-btn color="primary" @click="submit">
            <v-icon left small>mdi-content-save-outline</v-icon>
            Sauvegarder
          </v-btn>
        </v-card-actions>
      </v-card>

    </v-dialog>
  </v-row>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

const dialog = ref(false)
const validCreateStage = ref(true)
const formCreateStage = ref(null)

const menuDebut = ref(false)
const menuFin = ref(false)

const stage = ref({
  actionSuivi: null,
  attestation: false,
  bilan: null,
  createurId: null,
  debut: null,
  entrepriseId: null,
  fin: null,
  horaireApresMidi: null,
  horaireMatin: null,
  nom: null,
  rapport: false,
  remarque: null,
  repas: false,
  stageId: 0,
  stagiaireId: null,
  trajets: null,
  typeAnnonceId: null,
  typeMetierId: null,
  typeStageId: null,
  sessionId: null
})

const entreprise = computed(() => store.state.entreprise)
const typeStage = computed(() => store.state.typeStage)
const typeAnnonce = computed(() => store.state.typeAnnonce)
const typeMetier = computed(() => store.state.typeMetier)
const typeIntershipActivity = computed(() => store.state.typeIntershipActivity)
const user = computed(() => store.state.user)
const session = computed(() => store.state.session)

const requiredRule = [v => !!v || 'Champ obligatoire']

const dateFinRules = [
  () =>
    !stage.value.fin ||
    stage.value.debut <= stage.value.fin ||
    'La date de début doit être avant la fin'
]

onBeforeMount(() => {
  store.dispatch('entreprise/fetchEntreprises')
  store.dispatch('typeStage/fetchTypeStages')
  store.dispatch('typeAnnonce/fetchTypeAnnonces')
  store.dispatch('typeMetier/fetchTypeMetiers')
  store.dispatch('typeIntershipActivity/fetchTypeIntershipActivities')
  store.dispatch('user/fetchUsers')
  store.dispatch('session/fetchSessions')
})

function submit() {
  stage.value.nom =
    typeIntershipActivity.value?.typeIntershipActivities?.find(
      i => i.typeIntershipActivityId === stage.value.typeIntershipActivityId
    )?.nom || null

  if (formCreateStage.value.validate()) {
    NProgress.start()

    store
      .dispatch('stage/createStage', stage.value)
      .then(response => {
        formCreateStage.value.reset()
        dialog.value = false
        window.location.href = `/stage/modifier/${response.stageId}`
      })
      .finally(() => NProgress.done())
  }
}
</script>

<style scoped>
.v-btn {
  transition: 0.15s ease;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0px 4px 12px rgba(0,0,0,0.12);
}
</style>
