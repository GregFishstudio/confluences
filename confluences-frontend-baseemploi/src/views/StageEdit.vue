<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Expérience professionnelle</h1>
      </v-col>
    </v-row>

    <v-form ref="formStage" v-model="valid">
      <v-row>
        <v-col cols="12" md="4">
          <v-autocomplete
            v-model="stage.typeIntershipActivityId"
            :items="typeIntershipActivity.value.typeIntershipActivities"
            item-title="nom"
            item-value="typeIntershipActivityId"
            label="Type d'activité"
            :rules="requiredRule"
            variant="outlined"
            rounded
          />

          <v-text-field
            v-model="stage.nom"
            :counter="30"
            :rules="nameRules"
            label="Nom (ancien champ)"
            variant="outlined"
            rounded
          />
        </v-col>

        <v-col cols="12" md="2">
          <v-autocomplete
            v-model="stage.typeStageId"
            :items="typeStage.value.typeStages"
            item-title="nom"
            item-value="typeStageId"
            label="Taux d'occupation"
            :rules="requiredRule"
            variant="outlined"
            rounded
          />
        </v-col>

        <v-col cols="12" md="3">
          <v-autocomplete
            v-model="stage.createurId"
            :items="user.value.users"
            item-title="nom"
            item-value="id"
            label="Créateur-trice"
            :rules="requiredRule"
            variant="outlined"
            rounded
          />
        </v-col>

        <v-col cols="12" md="3">
          <v-autocomplete
            v-model="stage.typeAnnonceId"
            :items="typeAnnonce.value.typeAnnonces"
            item-title="libelle"
            item-value="typeAnnonceId"
            label="Stage à annoncer"
            clearable
            variant="outlined"
            rounded
          />
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12" md="4">
          <v-autocomplete
            v-model="stage.typeMetierId"
            :items="typeMetier.value.typeMetiers"
            item-title="libelle"
            item-value="typeMetierId"
            label="Métier"
            :rules="requiredRule"
            variant="outlined"
            rounded
          />
        </v-col>

        <v-col cols="12" md="4">
          <v-autocomplete
            v-model="stage.entrepriseId"
            :items="entreprise.value.entreprises"
            item-title="nom"
            item-value="entrepriseId"
            label="Entreprise"
            clearable
            variant="outlined"
            rounded
          />
        </v-col>

        <v-col cols="12" md="4">
          <v-autocomplete
            v-model="stage.stagiaireId"
            :items="user.value.users"
            item-title="nom"
            item-value="id"
            label="Stagiaire"
            :rules="requiredRule"
            variant="outlined"
            rounded
          />
        </v-col>
      </v-row>

      <!-- Dates + session -->
      <v-row>
        <v-col cols="12" md="4">
          <v-row>
            <v-col cols="12" md="6">
              <!-- Début -->
              <v-text-field
                v-model="stage.debut"
                label="Début"
                readonly
                variant="outlined"
                rounded
                @click="menuDebut = true"
              />
              <v-dialog v-model="menuDebut" max-width="340">
                <v-card>
                  <v-date-picker
                    v-model="stage.debut"
                    color="primary"
                    @update:model-value="menuDebut = false"
                  />
                </v-card>
              </v-dialog>

              <v-autocomplete
                v-model="stage.sessionId"
                :items="session.value.sessions"
                item-title="description"
                item-value="sessionId"
                label="Session"
                clearable
                variant="outlined"
                rounded
              />
            </v-col>

            <v-col cols="12" md="6">
              <!-- Fin -->
              <v-text-field
                v-model="stage.fin"
                label="Fin"
                readonly
                variant="outlined"
                rounded
                clearable
                @click="menuFin = true"
              />
              <v-dialog v-model="menuFin" max-width="340">
                <v-card>
                  <v-date-picker
                    v-model="stage.fin"
                    color="primary"
                    @update:model-value="menuFin = false"
                  />
                </v-card>
              </v-dialog>
            </v-col>
          </v-row>

          <v-row>
            <v-col cols="6">
              <v-checkbox v-model="stage.repas" label="Repas" />
            </v-col>
            <v-col cols="6">
              <v-checkbox v-model="stage.trajets" label="Trajets" />
            </v-col>
          </v-row>
        </v-col>

        <v-col cols="12" md="6">
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="stage.horaireMatin"
                :counter="11"
                :rules="horaireRules"
                label="Horaire matin"
                variant="outlined"
                rounded
              />
            </v-col>

            <v-col cols="12" md="4">
              <v-text-field
                v-model="stage.horaireApresMidi"
                :counter="11"
                :rules="horaireRules"
                label="Horaire après-midi"
                variant="outlined"
                rounded
              />
            </v-col>

            <v-col cols="12" md="4">
              <v-text-field
                v-model="stage.horaireSamedi"
                :counter="30"
                :rules="horaireSamediRules"
                label="Horaire samedi"
                variant="outlined"
                rounded
              />
            </v-col>
          </v-row>
        </v-col>

        <v-col cols="12" md="2">
          <v-checkbox v-model="stage.attestation" label="Attestation" />
          <v-checkbox v-model="stage.rapport" label="Rapport" />
          <v-checkbox v-model="stage.hasContract" label="Contrat" />
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12" md="6">
          <v-textarea
            v-model="stage.remarque"
            label="Remarques"
            variant="outlined"
            auto-grow
            counter
          />
        </v-col>

        <v-col cols="12" md="6">
          <v-textarea
            v-model="stage.bilan"
            label="Bilan du stage"
            variant="outlined"
            auto-grow
            counter
          />
        </v-col>
      </v-row>

      <v-row>
        <v-col>
          <v-textarea
            v-model="stage.actionSuivi"
            label="Suivi"
            variant="outlined"
            auto-grow
            counter
          />
        </v-col>
      </v-row>

      <v-row class="mb-10">
        <v-col>
          <StageFileList :stage="stage" />
        </v-col>
      </v-row>

      <div class="action-container">
        <v-row>
          <v-col>
            <div class="text-center">
              <v-btn
                class="ma-2"
                color="success"
                min-width="150"
                @click="submit"
              >
                Sauvegarder
              </v-btn>

              <DeleteStage :stage="stage" />

              <v-btn
                class="ma-2"
                color="primary"
                min-width="150"
                @click="$router.go(-1)"
              >
                Annuler
              </v-btn>
            </div>
          </v-col>
        </v-row>
      </div>
    </v-form>
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import store from '@/store/index.js'
import DeleteStage from '@/components/DeleteStage.vue'
import StageFileList from '@/components/StageFileList.vue'
import moment from 'moment'

/* Props */
const props = defineProps({
  stage: Object
})

/* Form */
const formStage = ref(null)
const valid = ref(true)

/* Menus dates */
const menuDebut = ref(false)
const menuFin = ref(false)

/* Rules */
const nameRules = [
  v => !!v || 'Le nom est obligatoire',
  v => (v && v.length <= 30) || 'Max 30 caractères'
]

const horaireRules = [
  v => !v || v.length <= 11 || 'Max 11 caractères'
]

const horaireSamediRules = [
  v => !v || v.length <= 30 || 'Max 30 caractères'
]

const requiredRule = [v => !!v || 'Obligatoire']

/* Store state */
const entreprise = computed(() => store.state.entreprise)
const typeStage = computed(() => store.state.typeStage)
const typeAnnonce = computed(() => store.state.typeAnnonce)
const typeMetier = computed(() => store.state.typeMetier)
const typeIntershipActivity = computed(() => store.state.typeIntershipActivity)
const user = computed(() => store.state.user)
const session = computed(() => store.state.session)

/* Load data */
onBeforeMount(() => {
  store.dispatch('entreprise/fetchEntreprises')
  store.dispatch('typeStage/fetchTypeStages')
  store.dispatch('typeAnnonce/fetchTypeAnnonces')
  store.dispatch('typeMetier/fetchTypeMetiers')
  store.dispatch('typeIntershipActivity/fetchTypeIntershipActivities')
  store.dispatch('user/fetchUsers')
  store.dispatch('session/fetchSessions')

  props.stage.debut = formatDate(props.stage.debut)
  props.stage.fin = formatDate(props.stage.fin)
})

/* Methods */
function formatDate(date) {
  const d = moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD')
  return d === 'Invalid date' ? null : d
}

function submit() {
  if (formStage.value.validate()) {
    store
      .dispatch('stage/editStage', props.stage)
      .then(() => {
        window.location.href = '/stages'
      })
  }
}
</script>
