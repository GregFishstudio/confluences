<template>
  <v-container fluid class="pa-4">
    <div class="d-flex align-center mb-6">
      <v-btn icon variant="text" @click="goBack" class="mr-2">
        <v-icon>mdi-arrow-left</v-icon>
      </v-btn>
      <div class="text-h5 font-weight-bold flex-grow-1">
        {{ isNew ? 'Nouveau marrainage' : `${form.prenom} ${form.nom}` }}
      </div>

      <v-dialog v-if="!isNew" v-model="confirmDeleteDialog" max-width="480">
        <template #activator="{ props }">
          <v-btn v-bind="props" color="error" variant="outlined" prepend-icon="mdi-delete-outline">
            Supprimer
          </v-btn>
        </template>
        <v-card rounded="lg">
          <v-card-title class="py-4">Supprimer ce marrainage ?</v-card-title>
          <v-divider />
          <v-card-text class="pt-4">
            Cette action supprime définitivement {{ form.prenom }} {{ form.nom }}
            (commentaires et pièces jointes compris). Impossible à annuler.
          </v-card-text>
          <v-divider />
          <v-card-actions class="d-flex justify-end">
            <v-btn variant="text" color="grey" @click="confirmDeleteDialog = false">Annuler</v-btn>
            <v-btn color="error" :loading="deleting" @click="deleteThisMarrainage">Supprimer</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>

    <!-- ── Nouveau marrainage : questionnaire d'inscription ── -->
    <v-row v-if="isNew">
      <v-col cols="12">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4" variant="outlined">
          <div class="d-flex align-center justify-space-between mb-4">
            <div class="text-subtitle-1 font-weight-bold">Questionnaire d'inscription</div>
            <MarrainageQuestionManager @changed="loadQuestions" />
          </div>
          <MarrainageQuestionnaireForm :questions="questions" v-model="answers" />
          <div class="d-flex justify-end mt-2">
            <v-btn color="primary" :loading="saving" @click="createFromQuestionnaire">Créer le marrainage</v-btn>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-row v-else>
      <v-col cols="12" md="7">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-4">Informations</div>
          <v-row dense>
            <v-col cols="6"><v-text-field v-model="form.prenom" label="Prénom" variant="outlined" density="compact" /></v-col>
            <v-col cols="6"><v-text-field v-model="form.nom" label="Nom" variant="outlined" density="compact" /></v-col>
            <v-col cols="6"><v-text-field v-model="form.commune" label="Commune" variant="outlined" density="compact" /></v-col>
            <v-col cols="6"><v-text-field v-model="form.numero" label="Téléphone" variant="outlined" density="compact" /></v-col>
            <v-col cols="6"><v-text-field v-model="form.email" label="Email" variant="outlined" density="compact" /></v-col>
            <v-col cols="6"><v-text-field v-model="form.dateNaissance" type="date" label="Date de naissance" variant="outlined" density="compact" /></v-col>
            <v-col cols="6"><v-text-field v-model="form.etatCivil" label="État civil" variant="outlined" density="compact" /></v-col>
            <v-col cols="6"><v-text-field v-model="form.profession" label="Profession" variant="outlined" density="compact" /></v-col>
            <v-col cols="12">
              <v-textarea v-model="form.remarqueParrain" label="Remarques générales concernant le parrain/la marraine" variant="outlined" auto-grow rows="2" />
            </v-col>

            <v-col cols="6"><v-text-field v-model="form.periodeDemande" type="date" label="Date de demande" variant="outlined" density="compact" /></v-col>
            <v-col cols="6">
              <v-select
                v-model="form.statut"
                :items="[{ title: 'Aucun', value: null }, { title: 'En attente', value: 'en_attente' }, { title: 'À relancer', value: 'a_relancer' }]"
                label="Statut"
                variant="outlined" density="compact"
              />
            </v-col>

            <v-col cols="6">
              <v-switch v-model="form.actif" label="Actif (en tant que marraine/parrain)" :color="form.actif ? 'success' : 'error'" hide-details />
            </v-col>
            <v-col cols="6" v-if="!form.actif">
              <v-select
                v-model="form.raisonInactivite"
                :items="raisonOptions"
                label="Raison de l'inactivité"
                variant="outlined" density="compact" clearable
              />
            </v-col>
          </v-row>

          <v-divider class="my-4" />

          <div class="d-flex align-center justify-space-between mb-2">
            <div class="text-subtitle-2 font-weight-bold">Participant·e·s associé·e·s</div>
            <v-btn v-if="!isNew" size="small" variant="outlined" color="primary" icon="mdi-plus" @click="addParticipantDialog = true" />
          </div>

          <div v-if="!relations.length" class="text-caption text-grey mb-3">Aucun·e participant·e lié·e pour l'instant.</div>
          <v-card v-for="(rel, idx) in relations" :key="rel.participantId" variant="outlined" class="pa-3 mb-3">
            <div class="d-flex align-center justify-space-between mb-2">
              <div class="text-subtitle-2 font-weight-bold">Relation de marrainage/parrainage {{ idx + 1 }}</div>
              <v-btn icon="mdi-close" size="x-small" variant="text" title="Retirer cette relation" @click="removeParticipant(rel.participantId)" />
            </div>
            <v-row dense>
              <v-col cols="6" md="3">
                <v-text-field
                  :model-value="rel.dateDebut"
                  @update:model-value="v => updateRelation(rel, { dateDebut: v })"
                  type="date" label="Date de démarrage" variant="outlined" density="compact"
                />
              </v-col>
              <v-col cols="12" md="4">
                <div class="text-caption text-grey">Participant·e</div>
                <a href="#" class="participant-link font-weight-bold" @click.prevent="goToParticipant(rel.participantId)">
                  {{ rel.firstname }} {{ rel.lastName }}
                </a>
              </v-col>
              <v-col cols="6" md="2">
                <div class="text-caption text-grey">Téléphone</div>
                <div>{{ rel.phoneNumber || '—' }}</div>
              </v-col>
              <v-col cols="6" md="3">
                <div class="text-caption text-grey">Email</div>
                <div>{{ rel.email || '—' }}</div>
              </v-col>
              <v-col cols="6" md="3">
                <v-switch
                  :model-value="rel.actif"
                  @update:model-value="v => updateRelation(rel, { actif: v })"
                  label="Actif" :color="rel.actif ? 'success' : 'error'" hide-details
                />
              </v-col>
              <v-col cols="6" md="9" v-if="!rel.actif">
                <v-select
                  :model-value="rel.raisonInactivite"
                  @update:model-value="v => updateRelation(rel, { raisonInactivite: v })"
                  :items="raisonOptions"
                  label="Raison de l'inactivité"
                  variant="outlined" density="compact" clearable hide-details
                />
              </v-col>
            </v-row>
          </v-card>

          <v-dialog v-model="addParticipantDialog" max-width="480">
            <v-card rounded="lg">
              <v-card-title class="py-4">Associer un·e participant·e</v-card-title>
              <v-divider />
              <v-card-text class="pt-6">
                <v-autocomplete
                  v-model="newParticipantId"
                  :items="participantOptions"
                  item-title="label"
                  item-value="value"
                  label="Participant·e"
                  variant="outlined" density="compact"
                />
              </v-card-text>
              <v-divider />
              <v-card-actions class="d-flex justify-end">
                <v-btn variant="text" color="grey" @click="addParticipantDialog = false">Annuler</v-btn>
                <v-btn color="primary" :disabled="!newParticipantId" @click="addParticipant">Associer</v-btn>
              </v-card-actions>
            </v-card>
          </v-dialog>

          <v-text-field
            v-model="form.filleulNom"
            label="Nom du/de la filleul·e (si pas encore associé·e à un compte participant existant)"
            variant="outlined" density="compact"
            hint="Utile en attendant de créer/retrouver le compte participant correspondant."
            persistent-hint
            class="mb-4"
          />

          <v-row dense>
            <v-col cols="6" md="6">
              <v-text-field v-model="form.telephoneFilleul" label="Téléphone Filleul·e" variant="outlined" density="compact" />
            </v-col>
            <v-col cols="6" md="6">
              <v-text-field v-model="form.emailFilleul" label="Email Filleul·e" variant="outlined" density="compact" />
            </v-col>
            <v-col cols="12">
              <v-textarea v-model="form.suivi" label="Suivi" variant="outlined" auto-grow rows="2" />
            </v-col>
          </v-row>
          <div class="d-flex justify-end mt-2">
            <v-btn color="primary" :loading="saving" @click="save">Sauvegarder</v-btn>
          </div>
        </v-card>

        <FileAttachments
          v-if="!isNew"
          title="Convention et charte"
          controller="MarrainageFiles"
          id-key="marrainageFileId"
          :extra-fields="{ marrainageId: form.marrainageId }"
          :category-options="['Convention', 'Charte']"
          :exclude-categories="['Extrait de casier judiciaire']"
          v-model="files"
        />

        <v-card v-if="!isNew" class="pa-4 elevation-1 rounded-lg mt-4" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-2">Extrait de casier judiciaire</div>
          <FileAttachments
            title=""
            controller="MarrainageFiles"
            id-key="marrainageFileId"
            :extra-fields="{ marrainageId: form.marrainageId }"
            fixed-category="Extrait de casier judiciaire"
            filter-category="Extrait de casier judiciaire"
            v-model="files"
          />
        </v-card>

        <v-card v-if="nonCoreQuestions.length" class="pa-4 elevation-1 rounded-lg mt-4" variant="outlined">
          <div class="d-flex align-center justify-space-between mb-4">
            <div class="text-subtitle-1 font-weight-bold">Réponses au questionnaire</div>
            <MarrainageQuestionManager @changed="loadQuestions" />
          </div>
          <div class="text-caption text-grey mb-3">
            Prénom, nom, téléphone, email, commune, date de naissance, état civil et profession se modifient
            depuis "Informations" ci-dessus — ils ne sont pas répétés ici.
          </div>
          <MarrainageQuestionnaireForm :questions="nonCoreQuestions" v-model="answers" />
          <div class="d-flex justify-end mt-2">
            <v-btn color="primary" variant="outlined" :loading="savingAnswers" @click="saveAnswers">
              Sauvegarder les réponses
            </v-btn>
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="5">
        <CommentThread v-if="!isNew" :comments="comments" @submit="addComment" @delete="deleteComment" />
        <v-card v-else class="pa-4 elevation-1 rounded-lg" variant="outlined">
          <p class="text-body-2 text-medium-emphasis mb-0">
            Les commentaires et pièces jointes seront disponibles après la première sauvegarde.
          </p>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useStore } from 'vuex'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import FileAttachments from '@/components/FileAttachments.vue'
import CommentThread from '@/components/CommentThread.vue'
import MarrainageQuestionnaireForm from '@/components/MarrainageQuestionnaireForm.vue'
import MarrainageQuestionManager from '@/components/MarrainageQuestionManager.vue'
import MarrainageQuestionService from '@/services/marrainageQuestionService.js'
import MarrainageAnswerService from '@/services/marrainageAnswerService.js'

const API_URL = import.meta.env.VITE_API_URL

const store = useStore()
const route = useRoute()
const router = useRouter()

// Revient à la page précédente (restaure la pagination de la liste) plutôt que de
// toujours pousser une nouvelle navigation vers la page 1 de la liste.
function goBack() {
  if (window.history.state && window.history.state.back) {
    router.back()
  } else {
    router.push({ name: 'Marrainages' })
  }
}

const isNew = computed(() => route.name === 'Marrainage-Nouveau')
const saving = ref(false)
const savingAnswers = ref(false)
const deleting = ref(false)
const confirmDeleteDialog = ref(false)
const files = ref([])
const comments = ref([])
// Une entrée par relation de marrainage/parrainage (une par participant·e lié·e),
// avec ses propres dates et statut d'activité — indépendants du statut de la marraine elle-même.
const relations = ref([])
const addParticipantDialog = ref(false)
const newParticipantId = ref(null)
const questions = ref([])
const answers = ref({})

const raisonOptions = [
  'Contraintes familiales',
  'En emploi',
  'Déménagement',
  'Fin de formation du/de la filleul·e',
  'Manque de disponibilité',
  'Autre'
]

const form = ref({
  marrainageId: null,
  prenom: '',
  nom: '',
  // `date` (ex-"date de démarrage" globale), `age`, `charteSignee` et `relanceNotes` ne sont
  // plus affichés dans le formulaire (remplacés par les champs par relation / dateNaissance /
  // documents) mais restent chargés et renvoyés tels quels : le PUT remplace l'entité entière,
  // les omettre effacerait silencieusement ces valeurs historiques en base.
  date: null,
  numero: '',
  commune: '',
  email: '',
  age: null,
  dateNaissance: null,
  etatCivil: '',
  profession: '',
  actif: true,
  statut: null,
  raisonInactivite: null,
  participantId: null,
  periodeDemande: '',
  remarqueParrain: '',
  filleulNom: '',
  telephoneFilleul: '',
  emailFilleul: '',
  suivi: '',
  extraitCasier: '',
  charteSignee: false,
  relanceNotes: ''
})

const participantOptions = computed(() =>
  (store.state.user?.users || []).map(u => ({
    label: `${u.prenom} ${u.nom}`,
    value: u.id
  }))
)

function loadMarrainage(marrainage) {
  form.value = {
    marrainageId: marrainage.marrainageId,
    prenom: marrainage.prenom,
    nom: marrainage.nom,
    date: marrainage.date ? marrainage.date.substring(0, 10) : null,
    numero: marrainage.numero,
    commune: marrainage.commune,
    email: marrainage.email,
    age: marrainage.age,
    dateNaissance: marrainage.dateNaissance ? marrainage.dateNaissance.substring(0, 10) : null,
    etatCivil: marrainage.etatCivil,
    profession: marrainage.profession,
    actif: marrainage.actif,
    statut: marrainage.statut,
    raisonInactivite: marrainage.raisonInactivite,
    participantId: marrainage.participantId,
    periodeDemande: marrainage.periodeDemande ? marrainage.periodeDemande.substring(0, 10) : null,
    remarqueParrain: marrainage.remarqueParrain,
    filleulNom: marrainage.filleulNom,
    telephoneFilleul: marrainage.telephoneFilleul,
    emailFilleul: marrainage.emailFilleul,
    suivi: marrainage.suivi,
    extraitCasier: marrainage.extraitCasier,
    charteSignee: marrainage.charteSignee,
    relanceNotes: marrainage.relanceNotes
  }
  files.value = marrainage.files || []
  comments.value = (marrainage.comments || []).slice().sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))

  const links = (marrainage.participantLinks || []).filter(pl => pl.participant)
  // Le lien historique (Marrainage.ParticipantId/Participant) n'a pas de ligne
  // MarrainageParticipant associée avant la Phase 2 — on le représente ici sans dates.
  if (marrainage.participant && !links.some(pl => pl.participantId === marrainage.participant.id)) {
    links.unshift({ participant: marrainage.participant, participantId: marrainage.participant.id, actif: true })
  }
  relations.value = links.map(pl => ({
    participantId: pl.participantId,
    firstname: pl.participant?.firstname,
    lastName: pl.participant?.lastName,
    phoneNumber: pl.participant?.phoneNumber,
    email: pl.participant?.email,
    dateDebut: pl.dateDebut ? pl.dateDebut.substring(0, 10) : null,
    dateFin: pl.dateFin ? pl.dateFin.substring(0, 10) : null,
    actif: pl.actif ?? true,
    raisonInactivite: pl.raisonInactivite ?? null,
  }))
}

async function save() {
  saving.value = true
  try {
    if (isNew.value) {
      const created = await store.dispatch('marrainage/createMarrainage', form.value)
      router.push({ name: 'Marrainage-Modifier', params: { id: created.marrainageId } })
    } else {
      await store.dispatch('marrainage/editMarrainage', form.value)
    }
  } finally {
    saving.value = false
  }
}

async function deleteThisMarrainage() {
  deleting.value = true
  try {
    // route.params.id est toujours disponible dès le montage (contrairement à
    // form.value.marrainageId, qui ne se remplit qu'une fois loadMarrainage() terminé).
    await store.dispatch('marrainage/deleteMarrainage', route.params.id)
    confirmDeleteDialog.value = false
    router.push({ name: 'Marrainages' })
  } finally {
    deleting.value = false
  }
}

async function addParticipant() {
  if (!newParticipantId.value) return
  const dateDebut = new Date().toISOString().substring(0, 10)
  await axios.post(`${API_URL}/api/Marrainages/${form.value.marrainageId}/participants`, {
    participantId: newParticipantId.value,
    dateDebut
  })
  const participant = (store.state.user?.users || []).find(u => u.id === newParticipantId.value)
  if (participant && !relations.value.some(r => r.participantId === participant.id)) {
    relations.value.push({
      participantId: participant.id,
      firstname: participant.prenom,
      lastName: participant.nom,
      phoneNumber: participant.phoneNumber,
      email: participant.email,
      dateDebut,
      dateFin: null,
      actif: true,
      raisonInactivite: null,
    })
  }
  newParticipantId.value = null
  addParticipantDialog.value = false
}

async function removeParticipant(participantId) {
  await axios.delete(`${API_URL}/api/Marrainages/${form.value.marrainageId}/participants/${participantId}`)
  relations.value = relations.value.filter(r => r.participantId !== participantId)
}

// Sauvegarde immédiate (pas besoin du bouton "Sauvegarder" général) : chaque relation a son
// propre cycle de vie, indépendant des autres champs du formulaire principal.
async function updateRelation(rel, changes) {
  Object.assign(rel, changes)
  await axios.patch(`${API_URL}/api/Marrainages/${form.value.marrainageId}/participants/${rel.participantId}`, {
    dateDebut: rel.dateDebut,
    dateFin: rel.dateFin,
    actif: rel.actif,
    raisonInactivite: rel.actif ? null : rel.raisonInactivite,
  })
}

function goToParticipant(id) {
  router.push({ name: 'Participant-Detail', params: { id } })
}

function loadQuestions() {
  return MarrainageQuestionService.getQuestions(true).then(response => {
    questions.value = response.data
  })
}

// Insensible aux accents et à la casse, pour rester fiable même en cas de différences
// d'encodage entre le texte saisi ici et celui enregistré en base pour les questions.
function normalizeLabel(str) {
  return (str || '')
    .toString()
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
}

function findQuestionByLabel(needle) {
  const n = normalizeLabel(needle)
  return questions.value.find(q => normalizeLabel(q.label).includes(n))
}

function findQuestionByExactLabel(label) {
  const n = normalizeLabel(label)
  return questions.value.find(q => normalizeLabel(q.label) === n)
}

function answerFor(question) {
  if (!question) return ''
  return answers.value[question.marrainageQuestionId] || ''
}

// Questions déjà couvertes par les champs "Informations" (Prénom, Nom, Téléphone, Email,
// Commune, Âge, État civil, Profession) — inutile de les répéter dans le bloc questionnaire.
const CORE_LABELS = ['prenom', 'nom', 'numero de telephone', 'adresse e-mail', 'commune de residence', 'age', 'date de naissance', 'etat civil', 'profession']
const nonCoreQuestions = computed(() =>
  questions.value.filter(q => !CORE_LABELS.includes(normalizeLabel(q.label)))
)

async function createFromQuestionnaire() {
  saving.value = true
  try {
    const prenomQuestion = findQuestionByExactLabel('prénom')
    const nomQuestion = findQuestionByExactLabel('nom')
    const telQuestion = findQuestionByLabel('numéro de téléphone')
    const emailQuestion = findQuestionByLabel('adresse e-mail')
    const communeQuestion = findQuestionByLabel('commune de résidence')
    const ageQuestion = findQuestionByExactLabel('age')
    const dateNaissanceQuestion = findQuestionByLabel('date de naissance')
    const etatCivilQuestion = findQuestionByExactLabel('etat civil')
    const professionQuestion = findQuestionByExactLabel('profession')

    // On repart de form.value sans marrainageId (null pour un nouveau marrainage :
    // envoyer `null` casserait la désérialisation côté API, qui attend un int).
    const { marrainageId, ...formWithoutId } = form.value

    const ageValue = answerFor(ageQuestion)
    const dateNaissanceValue = answerFor(dateNaissanceQuestion)

    const payload = {
      ...formWithoutId,
      prenom: answerFor(prenomQuestion),
      nom: answerFor(nomQuestion),
      numero: answerFor(telQuestion),
      email: answerFor(emailQuestion),
      commune: answerFor(communeQuestion),
      age: ageValue ? Number(ageValue) : null,
      dateNaissance: dateNaissanceValue || null,
      etatCivil: answerFor(etatCivilQuestion),
      profession: answerFor(professionQuestion),
    }

    const created = await store.dispatch('marrainage/createMarrainage', payload)

    const answerList = Object.entries(answers.value)
      .filter(([, value]) => value !== undefined && value !== null && value !== '')
      .map(([questionId, value]) => ({ questionId: Number(questionId), value: String(value) }))

    if (answerList.length) {
      await MarrainageAnswerService.saveAnswers(created.marrainageId, answerList)
    }

    router.push({ name: 'Marrainage-Modifier', params: { id: created.marrainageId } })
  } finally {
    saving.value = false
  }
}

async function saveAnswers() {
  savingAnswers.value = true
  try {
    const answerList = Object.entries(answers.value)
      .filter(([, value]) => value !== undefined && value !== null && value !== '')
      .map(([questionId, value]) => ({ questionId: Number(questionId), value: String(value) }))
    await MarrainageAnswerService.saveAnswers(form.value.marrainageId, answerList)
  } finally {
    savingAnswers.value = false
  }
}

async function loadAnswers(marrainageId) {
  const response = await MarrainageAnswerService.getAnswers(marrainageId)
  const map = {}
  response.data.forEach(a => { map[a.marrainageQuestionId] = a.value })
  answers.value = map
}

async function addComment(text) {
  const response = await axios.post(`${API_URL}/api/MarrainageComments`, {
    marrainageId: form.value.marrainageId,
    text
  })
  comments.value = [response.data, ...comments.value]
}

async function deleteComment(commentId) {
  await axios.delete(`${API_URL}/api/MarrainageComments/${commentId}`)
  comments.value = comments.value.filter(c => c.marrainageCommentId !== commentId)
}

async function initFromRoute() {
  await loadQuestions()
  if (!isNew.value && route.params.marrainage) {
    loadMarrainage(route.params.marrainage)
    await loadAnswers(route.params.marrainage.marrainageId)
  }
}

onMounted(() => {
  store.dispatch('user/fetchUsers')
  initFromRoute()
})

// Naviguer de "Nouveau marrainage" vers la fiche fraîchement créée (ou d'une fiche à une
// autre) réutilise cette même instance de composant — sans ce watcher, "Informations"
// resterait vide tant que la page n'est pas rechargée manuellement.
watch(() => route.params.id, (newId, oldId) => {
  if (newId !== oldId) initFromRoute()
})
</script>

<style scoped>
.participant-link { color: #1976D2; text-decoration: underline; }
</style>
