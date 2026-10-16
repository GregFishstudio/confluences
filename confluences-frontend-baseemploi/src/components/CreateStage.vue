<template>
  <v-row class="d-flex justify-end">
    <v-dialog v-model="dialog" max-width="720" persistent>

      <!-- Bouton -->
      <template #activator="{ props }">
        <v-btn
          v-bind="props"
          color="primary"
          class="elevation-2"
          :size="buttonSize"
          @click="prefill"
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
          <v-alert v-if="listsError" type="warning" variant="tonal" density="compact" class="mb-4">
            {{ listsError }}
            <template #append>
              <v-btn size="small" variant="flat" color="warning" :loading="loadingLists" @click="loadLists">Réessayer</v-btn>
            </template>
          </v-alert>
          <v-form ref="formCreateStage" v-model="validCreateStage">

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeIntershipActivityId"
              :items="typeIntershipActivity?.typeIntershipActivities ?? []"
              item-title="nom"
              item-value="typeIntershipActivityId"
              label="Type d'activité"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeStageId"
              :items="typeStage?.typeStages ?? []"
              item-title="nom"
              item-value="typeStageId"
              label="Taux d'occupation"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.createurId"
              :items="user?.users ?? []"
              item-title="nom"
              item-value="id"
              label="Créateur·trice"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeAnnonceId"
              :items="typeAnnonce?.typeAnnonces ?? []"
              item-title="libelle"
              item-value="typeAnnonceId"
              label="Stage à annoncer"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
              clearable
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.typeMetierId"
              :items="typeMetier?.typeMetiers ?? []"
              item-title="libelle"
              item-value="typeMetierId"
              label="Métier"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.entrepriseId"
              :items="entreprise?.entreprises ?? []"
              item-title="nom"
              item-value="entrepriseId"
              label="Entreprise"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
              clearable
            />

            <v-autocomplete
              class="mb-4"
              v-model="stage.stagiaireId"
              :items="user?.users ?? []"
              item-title="nom"
              item-value="id"
              label="Stagiaire"
              :rules="requiredRule"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
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
              :items="session?.sessions ?? []"
              item-title="description"
              item-value="sessionId"
              label="Session"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              :loading="loadingLists"
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
import { ref, computed, watch } from 'vue'
import store from '@/store/index.js'
import NProgress from 'nprogress'

// stagiaireId : stagiaire pré-sélectionné·e quand le bouton est placé sur une fiche participant·e.
const props = defineProps({
  stagiaireId: { type: String, default: null },
  buttonSize: { type: String, default: 'default' },
})

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

// Listes de choix chargées à chaque ouverture du formulaire (et non une seule fois à l'affichage
// de la page) : si ce premier chargement échouait ou partait trop tôt (ex. juste après la
// connexion), toutes les listes restaient vides ("Aucune donnée disponible") jusqu'à un
// rafraîchissement de la page.
const loadingLists = ref(false)
const listsError = ref('')

async function loadLists() {
  loadingLists.value = true
  listsError.value = ''
  try {
    await Promise.allSettled([
      store.dispatch('entreprise/fetchEntreprises'),
      store.dispatch('typeStage/fetchTypeStages'),
      store.dispatch('typeAnnonce/fetchTypeAnnonces'),
      store.dispatch('typeMetier/fetchTypeMetiers'),
      store.dispatch('typeIntershipActivity/fetchTypeIntershipActivities'),
      store.dispatch('user/fetchUsers'),
      store.dispatch('session/fetchSessions'),
    ])
    const vides = [
      [typeStage.value?.typeStages, "taux d'occupation"],
      [typeMetier.value?.typeMetiers, 'métiers'],
      [user.value?.users, 'stagiaires / créateur·trice·s'],
    ].filter(([list]) => !list?.length).map(([, label]) => label)
    if (vides.length) {
      listsError.value = `Certaines listes n'ont pas pu être chargées (${vides.join(', ')}). Cliquez sur « Réessayer » ; si cela persiste, reconnectez-vous.`
    }
  } finally {
    loadingLists.value = false
  }
}

watch(dialog, (open) => { if (open) loadLists() })

function prefill() {
  if (props.stagiaireId) stage.value.stagiaireId = props.stagiaireId
}

async function submit() {
  stage.value.nom =
    typeIntershipActivity.value?.typeIntershipActivities?.find(
      i => i.typeIntershipActivityId === stage.value.typeIntershipActivityId
    )?.nom || null

  // Vuetify 3 : validate() renvoie une promesse ({ valid }), pas un booléen.
  const { valid } = await formCreateStage.value.validate()
  if (valid) {
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
