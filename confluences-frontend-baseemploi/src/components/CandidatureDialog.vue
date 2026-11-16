<!--
  Création / modification d'une candidature — utilisé par l'onglet "Candidat·e·s" et par la vue
  d'une session (bouton "Nouveau·elle candidat·e", clic sur une ligne de candidat·e).
  La session visée est choisie parmi les sessions du calendrier de formation de la mesure.
-->
<template>
  <v-dialog :model-value="modelValue" max-width="980" scrollable persistent @keydown.esc="attemptClose">
    <v-card rounded="lg">
      <v-card-title class="py-4 d-flex align-center">
        {{ form.candidatureId ? 'Modifier la candidature' : 'Nouvelle candidature' }}
        <v-chip v-if="isDirty()" size="small" color="orange" variant="tonal" class="ml-3">Modifications non enregistrées</v-chip>
        <v-spacer />
        <v-btn v-if="form.candidatureId" icon="mdi-delete-outline" variant="text" color="error" title="Supprimer" @click="deleteDialog = true" />
        <v-btn icon="mdi-close" variant="text" title="Fermer" @click="attemptClose" />
      </v-card-title>
      <v-divider />
      <!-- Hors de la zone qui défile : toujours visible, même en bas d'un long formulaire. -->
      <v-alert v-if="error" type="error" variant="flat" density="compact" rounded="0" closable @click:close="error = ''">
        {{ error }}
      </v-alert>
      <v-card-text class="pt-4">
        <!-- Ancien·ne participant·e (retour, changement de mesure) : reprend ses données de la
             base au lieu de tout ressaisir, et lie la candidature à sa fiche (nécessaire pour
             l'inscrire ensuite dans une session). -->
        <div class="reprise-box pa-3 mb-4">
          <v-autocomplete
            :model-value="form.participantId" :items="participantItems" item-title="label" item-value="id"
            label="Ancien·ne participant·e ? Rechercher dans la liste des participant·e·s"
            prepend-inner-icon="mdi-account-search-outline" variant="outlined" density="compact" clearable
            :loading="participantsLoading || reprenant" bg-color="white"
            :custom-filter="(_, query, item) => matchesSearch(query, item.raw.label)"
            :hint="form.participantId
              ? 'Données reprises de sa fiche participant — vous pouvez les compléter ci-dessous.'
              : 'Laisser vide pour une nouvelle personne.'"
            persistent-hint
            @update:model-value="reprendreParticipant"
          />
        </div>

        <div v-if="isNew" class="text-caption text-medium-emphasis mb-2">Les champs marqués * sont obligatoires.</div>

        <div class="text-subtitle-2 font-weight-bold mb-2 text-primary">Demande</div>
        <v-row dense>
          <v-col cols="6" md="3">
            <v-text-field v-model="form.dateDemande" type="date" :label="req('Date de la demande')" variant="outlined" density="compact" :error="isMissing('dateDemande')" />
          </v-col>
          <v-col cols="6" md="3">
            <v-select v-model="form.statut" :items="STATUTS_CANDIDATURE" :label="req('Statut')" variant="outlined" density="compact" clearable :error="isMissing('statut')" />
          </v-col>
          <v-col cols="6" md="2">
            <v-select v-model="form.mesureVisee" :items="MESURES" :label="req('Mesure')" variant="outlined" density="compact" clearable :error="isMissing('mesureVisee')" />
          </v-col>
          <v-col cols="6" md="4">
            <v-select
              v-model="form.sessionsVisees" :items="sessionItems" item-title="title" item-value="key"
              :label="req('Sessions visées')" variant="outlined" density="compact" clearable multiple chips closable-chips
              :error="isMissing('session')"
              :disabled="!form.mesureVisee" persistent-hint
              :hint="form.mesureVisee ? 'Plusieurs sessions possibles : la candidature apparaît dans chacune' : 'Choisir d\'abord la mesure'"
            >
              <template #chip="{ item, props: p }">
                <v-chip v-bind="p" size="small">{{ item.raw.short }}</v-chip>
              </template>
            </v-select>
          </v-col>
          <v-col cols="6" md="2">
            <v-select v-model="form.priorite" :items="[0, 1, 2, 3, 4, 5]" label="Priorité" variant="outlined" density="compact" clearable />
          </v-col>
        </v-row>

        <!-- Statut "AS à contacter" : remarques datées, conservées en historique -->
        <div v-if="form.statut === 'AS à contacter' || threads.as_a_contacter.length" class="status-box pa-3 mb-4">
          <div class="d-flex align-center mb-2 text-subtitle-2 font-weight-bold text-red-darken-2">
            <v-icon size="small" class="mr-2" color="red-darken-2">mdi-phone-alert-outline</v-icon>
            AS à contacter — remarques
          </div>
          <CommentThread
            v-if="form.candidatureId"
            :comments="threads.as_a_contacter" id-key="candidatureCommentId" title="Historique"
            empty-text="Aucune remarque : précisez pourquoi l'AS doit être contacté·e." input-label="Nouvelle remarque (datée automatiquement)"
            @submit="(t) => addComment('as_a_contacter', t)" @delete="deleteComment"
          />
          <v-textarea
            v-else v-model="pending.as_a_contacter" label="Remarque (datée et signée à l'enregistrement)"
            rows="2" auto-grow variant="outlined" density="compact" bg-color="white" hide-details
          />
        </div>

        <div class="text-subtitle-2 font-weight-bold mb-2 mt-2 text-primary">Bénéficiaire</div>
        <v-row dense>
          <v-col cols="4" md="2"><v-select v-model="form.civilite" :items="CIVILITES" :label="req('Civilité')" variant="outlined" density="compact" clearable :error="isMissing('civilite')" /></v-col>
          <v-col cols="8" md="4"><v-text-field v-model="form.nom" label="Nom *" variant="outlined" density="compact" :error="isMissing('nom')" /></v-col>
          <v-col cols="6" md="3"><v-text-field v-model="form.prenom" :label="req('Prénom')" variant="outlined" density="compact" :error="isMissing('prenom')" /></v-col>
          <v-col cols="6" md="3">
            <DateTextField v-model="form.dateNaissance" :label="evalReq('Date de naissance')" variant="outlined" density="compact" :error="isMissing('dateNaissance')" />
          </v-col>
          <v-col cols="12" md="6"><v-textarea v-model="form.adresse" :label="req('Adresse')" rows="1" auto-grow variant="outlined" density="compact" :error="isMissing('adresse')" /></v-col>
          <v-col cols="6" md="3"><v-text-field v-model="form.telephone" :label="req('Téléphone')" variant="outlined" density="compact" :error="isMissing('telephone')" /></v-col>
          <v-col cols="6" md="3"><v-text-field v-model="form.email" :label="req('Email')" variant="outlined" density="compact" :error="isMissing('email')" /></v-col>
          <v-col cols="6" md="3"><v-text-field v-model="form.origine" :label="evalReq('Pays d\'origine')" variant="outlined" density="compact" :error="isMissing('origine')" /></v-col>
          <v-col cols="6" md="3"><v-select v-model="form.mandant" :items="MANDANTS" :label="req('Mandant')" variant="outlined" density="compact" clearable :error="isMissing('mandant')" /></v-col>
        </v-row>

        <div class="text-subtitle-2 font-weight-bold mb-2 mt-2 text-primary">Assistant·e social·e</div>
        <v-row dense>
          <v-col cols="12" md="6" class="d-flex ga-1">
            <v-autocomplete
              v-model="form.assistantSocialId" :items="asItems" item-title="label" item-value="id"
              :label="req('AS')" variant="outlined" density="compact" clearable class="flex-grow-1" :error="isMissing('assistantSocialId')"
            />
            <v-btn icon="mdi-account-plus-outline" variant="text" title="Nouvel·le AS" @click="asDialog = true" />
          </v-col>
          <v-col cols="12">
            <CommentThread
              v-if="form.candidatureId"
              :comments="threads.as" id-key="candidatureCommentId" title="Remarques AS"
              empty-text="Aucune remarque pour le moment." input-label="Ajouter une remarque"
              @submit="(t) => addComment('as', t)" @delete="deleteComment"
            />
            <v-textarea
              v-else v-model="pending.as" label="Première remarque AS (datée et signée à l'enregistrement)"
              rows="2" auto-grow variant="outlined" density="compact"
            />
          </v-col>
        </v-row>

        <div class="text-subtitle-2 font-weight-bold mb-2 mt-4 text-primary">Responsable pédagogique</div>
        <v-row dense>
          <v-col cols="12" md="6"><v-textarea v-model="form.notesRdv" label="Notes rdv (propositions, suivi)" rows="2" auto-grow variant="outlined" density="compact" /></v-col>
          <v-col cols="12" md="6">
            <DateTextField
              v-model="form.dateEvaluation" :label="hasEvaluation ? 'Date de l’évaluation *' : 'Date de l’évaluation'"
              variant="outlined" density="compact" :error="isMissing('dateEvaluation')"
              hint="À renseigner avant de rédiger l'évaluation." persistent-hint class="mb-2"
            />
            <v-textarea
              v-model="form.evaluationNiveau" label="Évaluation du niveau de français" rows="2" auto-grow variant="outlined" density="compact"
              :disabled="!form.dateEvaluation && !hasEvaluation" :hint="!form.dateEvaluation ? 'Saisissez d’abord la date de l’évaluation.' : ''" persistent-hint
            />
          </v-col>
        </v-row>

        <!-- Une évaluation saisie rend obligatoires la date de naissance et le pays d'origine -->
        <v-alert v-if="evaluationMissing.length" type="warning" variant="tonal" density="compact" class="mb-4" icon="mdi-account-alert-outline">
          <div class="font-weight-bold mb-2">Pour enregistrer l'évaluation, complétez aussi :</div>
          <div v-if="!form.dateEvaluation" class="mb-2">· la date de l'évaluation (ci-dessus)</div>
          <v-row dense>
            <v-col v-if="!form.dateNaissance" cols="12" md="6">
              <DateTextField v-model="form.dateNaissance" label="Date de naissance *" variant="outlined" density="compact" bg-color="white" hide-details="auto" />
            </v-col>
            <v-col v-if="!form.origine?.trim()" cols="12" md="6">
              <v-text-field v-model="form.origine" label="Pays d'origine *" variant="outlined" density="compact" bg-color="white" hide-details />
            </v-col>
          </v-row>
        </v-alert>

        <!-- Remarques internes : jamais affichées dans les tableaux ni transmises -->
        <div class="internal-box pa-3 mt-2 mb-4">
          <div class="d-flex align-center mb-2 text-subtitle-2 font-weight-bold">
            <v-icon size="small" class="mr-2">mdi-lock-outline</v-icon>
            Pour nous — remarques internes
          </div>
          <div class="text-caption text-medium-emphasis mb-2">Non officielles : visibles seulement dans ce formulaire, jamais dans les tableaux.</div>
          <v-textarea v-model="form.remarquesPedagogiques" rows="2" auto-grow variant="outlined" density="compact" hide-details bg-color="white" />
        </div>

        <div class="text-subtitle-2 font-weight-bold mb-2 text-primary">Administration</div>
        <v-row dense>
          <v-col cols="12" class="d-flex flex-wrap ga-4">
            <v-switch v-model="form.inscriptionConfirmee" label="Inscription confirmée" color="success" hide-details density="compact" class="flex-grow-0" />
            <v-switch v-model="form.contratMaori" label="Contrat Maori" color="red" hide-details density="compact" class="flex-grow-0" />
          </v-col>
        </v-row>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="attemptClose">Annuler</v-btn>
        <v-btn color="primary" :loading="saving" @click="save">Enregistrer</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <v-dialog v-model="deleteDialog" max-width="420">
    <v-card rounded="lg">
      <v-card-title class="py-4">Supprimer cette candidature ?</v-card-title>
      <v-card-text>{{ form.civilite }} {{ form.nom }} {{ form.prenom }} — cette action est définitive. Pour garder l'historique, préférez le statut « Retiré-e ».</v-card-text>
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="deleteDialog = false">Annuler</v-btn>
        <v-btn color="error" @click="confirmDelete">Supprimer</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <AssistantSocialDialog v-model="asDialog" @saved="onAsCreated" />
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import AssistantSocialDialog from '@/components/AssistantSocialDialog.vue'
import CommentThread from '@/components/CommentThread.vue'
import DateTextField from '@/components/DateTextField.vue'
import { matchesSearch, normalizeSearch } from '@/utils/search'
import { useStore } from 'vuex'
import { useUnsavedGuard, confirmDiscard } from '@/composables/useUnsavedGuard'
import {
  STATUTS_CANDIDATURE, MESURES, MANDANTS, CIVILITES,
  asLabel, readApiError, sessionsDuGroupe, groupeCandidature, mesureDuStatut,
} from '@/utils/inscriptionOptions'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  // Candidature existante (au moins { candidatureId }) ou null pour une nouvelle.
  candidature: { type: Object, default: null },
  // Valeurs pré-remplies d'une nouvelle candidature (ex. mesure + session depuis la vue session).
  defaults: { type: Object, default: () => ({}) },
  assistants: { type: Array, required: true },
  calendarSessions: { type: Array, default: () => [] },
})
const emit = defineEmits(['update:modelValue', 'saved', 'assistants-changed'])
const store = useStore()

const form = ref({})
const saving = ref(false)
const error = ref('')
const deleteDialog = ref(false)
const asDialog = ref(false)
const triedSave = ref(false)

// Fils de remarques datées : "as" (Remarques AS), "as_a_contacter", "transmission".
const KINDS = ['as', 'as_a_contacter', 'transmission']
const threads = ref({ as: [], as_a_contacter: [], transmission: [] })
// Nouvelle candidature : premières remarques, publiées dans leur fil après la création.
const pending = ref({ as: '', as_a_contacter: '', transmission: '' })

const participants = ref([])
const participantsLoading = ref(false)

const isNew = computed(() => !form.value.candidatureId)

const asItems = computed(() => props.assistants
  .filter(a => a.actif || a.assistantSocialId === form.value.assistantSocialId)
  .map(a => ({ id: a.assistantSocialId, label: asLabel(a) })))
const participantItems = computed(() => participants.value.map(p => ({ id: p.id, label: `${p.nom} ${p.prenom}` })))

// Sessions proposées : celles de la mesure pas encore terminées (+ celle déjà choisie), avec
// toujours au moins les trois prochaines sessions après la session en cours.
const sessionItems = computed(() => {
  const groupe = groupeCandidature(form.value.mesureVisee, form.value.statut)
  if (!groupe) return []
  const today = new Date()
  return sessionsDuGroupe(props.calendarSessions, groupe, { minAVenir: 3 })
    .filter(s => s.planned || !s.end || s.end >= today || (form.value.sessionsVisees || []).includes(s.key))
    .map(s => ({ key: s.key, title: `${s.label} (${s.dates})`, short: `S${s.number} ${s.year}` }))
})

// Ancienne session visée unique (année + numéro) → liste.
const sessionsDe = (c) => c?.sessionsVisees?.length ? [...c.sessionsVisees]
  : (c?.anneeVisee && c?.sessionNumberVisee) ? [`${c.anneeVisee}-${c.sessionNumberVisee}`] : []

// ─── Champs obligatoires ───
// À la création : tous les champs de "Demande" sauf la priorité, tous ceux de "Bénéficiaire"
// sauf pays d'origine et date de naissance, et l'AS. Dès qu'une évaluation est saisie (création
// ou modification) : date de naissance et pays d'origine.
const blank = (v) => v === null || v === undefined || String(v).trim() === ''
const REQUIRED_AT_CREATION = [
  { key: 'dateDemande', label: 'date de la demande' },
  { key: 'statut', label: 'statut' },
  { key: 'mesureVisee', label: 'mesure' },
  { key: 'session', label: 'session visée', value: () => (form.value.sessionsVisees || []).join(',') },
  { key: 'civilite', label: 'civilité' },
  { key: 'nom', label: 'nom' },
  { key: 'prenom', label: 'prénom' },
  { key: 'adresse', label: 'adresse' },
  { key: 'telephone', label: 'téléphone' },
  { key: 'email', label: 'email' },
  { key: 'mandant', label: 'mandant' },
  { key: 'assistantSocialId', label: 'AS' },
]
const hasEvaluation = computed(() => !blank(form.value.evaluationNiveau))
const evaluationMissing = computed(() => !hasEvaluation.value ? [] : [
  { key: 'dateEvaluation', label: "date de l'évaluation" },
  { key: 'dateNaissance', label: 'date de naissance' },
  { key: 'origine', label: "pays d'origine" },
].filter(f => blank(form.value[f.key])))

const missingFields = computed(() => {
  const rules = isNew.value ? REQUIRED_AT_CREATION : [{ key: 'nom', label: 'nom' }]
  const missing = rules.filter(f => blank(f.value ? f.value() : form.value[f.key]))
  return [...missing, ...evaluationMissing.value]
})
const isMissing = (key) => triedSave.value && missingFields.value.some(f => f.key === key)
const req = (label) => isNew.value ? `${label} *` : label
const evalReq = (label) => hasEvaluation.value ? `${label} *` : label

// ─── Modifications non enregistrées ───
const snapshot = ref('')
const serialize = () => JSON.stringify({ form: form.value, pending: pending.value })
const isDirty = () => props.modelValue && !!snapshot.value && serialize() !== snapshot.value
useUnsavedGuard(isDirty)

function close() {
  snapshot.value = ''
  emit('update:modelValue', false)
}

function attemptClose() {
  if (!isDirty() || confirmDiscard()) close()
}

const toInputDate = (d) => d ? moment(d).format('YYYY-MM-DD') : null

// "En attente A1/A2 F&I" ou "… Télécours" : la mesure suit le statut choisi.
watch(() => form.value.statut, (statut) => {
  const mesure = mesureDuStatut(statut)
  if (mesure && form.value.mesureVisee !== mesure) form.value.mesureVisee = mesure
})
// …et inversement : changer de mesure garde le niveau de liste d'attente (A1 / A2).
watch(() => form.value.mesureVisee, (mesure) => {
  const niveau = (form.value.statut || '').match(/^En attente (A[12]) /)?.[1]
  if (niveau && (mesure === 'F&I' || mesure === 'Télécours')) form.value.statut = `En attente ${niveau} ${mesure}`
})

watch(() => props.modelValue, async (open) => {
  if (!open) return
  error.value = ''
  triedSave.value = false
  snapshot.value = ''
  loadParticipants()
  threads.value = { as: [], as_a_contacter: [], transmission: [] }
  pending.value = { as: '', as_a_contacter: '', transmission: '' }
  if (props.candidature?.candidatureId) {
    const res = await apiFetch(`${API_BASE}/api/Candidatures/${props.candidature.candidatureId}`)
    const c = res?.ok ? await res.json() : props.candidature
    form.value = { ...c, sessionsVisees: sessionsDe(c), dateDemande: toInputDate(c.dateDemande), dateNaissance: toInputDate(c.dateNaissance), dateEvaluation: toInputDate(c.dateEvaluation) }
    KINDS.forEach(loadThread)
  } else {
    form.value = {
      dateDemande: moment().format('YYYY-MM-DD'), statut: 'À évaluer', nom: '', inscriptionConfirmee: false, contratMaori: false,
      ...props.defaults,
      sessionsVisees: sessionsDe(props.defaults),
    }
  }
  snapshot.value = serialize()
})

// Reprend les données de la fiche participant (identité, coordonnées, mandant, AS, Maori).
const reprenant = ref(false)
async function reprendreParticipant(id) {
  form.value.participantId = id || null
  if (!id) return
  reprenant.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/ApplicationUsers/${id}`)
    if (!res?.ok) return
    const u = await res.json()
    const set = (key, value) => { if (value !== null && value !== undefined && value !== '') form.value[key] = value }
    const genre = (u.gender?.genderName || '').toLowerCase()
    set('civilite', genre.startsWith('f') ? 'Madame' : genre.startsWith('h') ? 'Monsieur' : null)
    set('prenom', u.firstname)
    set('nom', u.lastName)
    if (u.birthday && moment(u.birthday).year() > 1900) set('dateNaissance', toInputDate(u.birthday))
    set('adresse', u.adresse)
    set('telephone', u.phoneNumber)
    set('email', u.email)
    set('origine', u.nationality)
    set('mandant', u.beneficiaireDe)
    if (u.contratMaori) form.value.contratMaori = true
    const as = props.assistants.find(a => a.email.toLowerCase() === (u.assistantSocialEmail || '').toLowerCase())
    if (as) form.value.assistantSocialId = as.assistantSocialId
  } finally {
    reprenant.value = false
  }
}

async function loadParticipants() {
  if (participants.value.length) return
  participantsLoading.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/Users`)
    if (res?.ok) participants.value = (await res.json()).sort((a, b) => (a.nom || '').localeCompare(b.nom || '', 'fr'))
  } finally {
    participantsLoading.value = false
  }
}

async function save() {
  triedSave.value = true
  error.value = ''
  if (missingFields.value.length) {
    error.value = `Impossible d'enregistrer — ${missingFields.value.length > 1 ? 'champs manquants' : 'champ manquant'} : ${missingFields.value.map(f => f.label).join(', ')}.`
    return
  }
  saving.value = true
  try {
    const id = form.value.candidatureId
    if (!id) {
      // Évite les doublons (ex. nouvelle tentative après un message d'erreur alors que la
      // première création avait abouti).
      const existing = await findExistingCandidature()
      if (existing && !window.confirm(
        `Une candidature « ${existing.nom} ${existing.prenom || ''} » existe déjà (demande du ${existing.dateDemande ? moment(existing.dateDemande).format('DD.MM.YYYY') : '—'}, statut : ${existing.statut || '—'}).\n\nCréer quand même une nouvelle candidature ?`
      )) return
    }

    const res = await apiFetch(`${API_BASE}/api/Candidatures${id ? `/${id}` : ''}`, {
      method: id ? 'PUT' : 'POST',
      body: JSON.stringify(form.value),
    })
    if (!res) {
      error.value = 'Session expirée : reconnectez-vous puis réessayez.'
      return
    }
    if (!res.ok) {
      error.value = `Non enregistré : ${await readApiError(res)}`
      return
    }
    if (!id) {
      // La candidature existe désormais : une nouvelle tentative la modifiera au lieu d'en
      // créer une seconde.
      const { candidatureId } = await res.json()
      form.value.candidatureId = candidatureId
      try {
        for (const kind of KINDS) {
          const text = pending.value[kind].trim()
          if (text) await postComment(candidatureId, kind, text)
        }
      } catch (e) {
        emit('saved')
        error.value = "La candidature a été créée, mais une remarque n'a pas pu être enregistrée : ajoutez-la à nouveau ci-dessous."
        KINDS.forEach(loadThread)
        snapshot.value = serialize()
        return
      }
    }
    emit('saved')
    close()
    store.dispatch('notification/add', { type: 'success', message: 'Candidature enregistrée.' })
  } catch (e) {
    error.value = `Non enregistré : connexion au serveur impossible (${e.message || e}). Réessayez.`
  } finally {
    saving.value = false
  }
}

async function findExistingCandidature() {
  const key = normalizeSearch(`${form.value.nom || ''} ${form.value.prenom || ''}`)
  if (!key) return null
  const res = await apiFetch(`${API_BASE}/api/Candidatures`)
  if (!res?.ok) return null
  const list = await res.json()
  return list.find(c => normalizeSearch(`${c.nom || ''} ${c.prenom || ''}`) === key) || null
}

async function confirmDelete() {
  await apiFetch(`${API_BASE}/api/Candidatures/${form.value.candidatureId}`, { method: 'DELETE' })
  deleteDialog.value = false
  emit('saved')
  close()
}

// ─── Fils de remarques datées ───
async function loadThread(kind) {
  const res = await apiFetch(`${API_BASE}/api/Candidatures/${form.value.candidatureId}/comments?kind=${kind}`)
  if (res?.ok) threads.value[kind] = await res.json()
}

const postComment = (candidatureId, kind, text) => apiFetch(`${API_BASE}/api/Candidatures/${candidatureId}/comments`, {
  method: 'POST',
  body: JSON.stringify({ text, kind }),
})

async function addComment(kind, text) {
  await postComment(form.value.candidatureId, kind, text)
  await loadThread(kind)
  emit('saved')
}

async function deleteComment(commentId) {
  const res = await apiFetch(`${API_BASE}/api/Candidatures/comments/${commentId}`, { method: 'DELETE' })
  if (res?.status === 403) error.value = "Seul·e l'auteur·e d'une remarque peut la supprimer."
  KINDS.forEach(loadThread)
}

function onAsCreated(as) {
  form.value.assistantSocialId = as.assistantSocialId
  emit('assistants-changed')
}
</script>

<style scoped>
.internal-box {
  border: 1px dashed #b0a38a;
  background: #fbf8f1;
  border-radius: 8px;
}
.reprise-box {
  border: 1px solid #d6e4f5;
  background: #f4f8fd;
  border-radius: 8px;
}
.status-box {
  border: 1px solid #f5c6c6;
  border-left: 4px solid #d32f2f;
  background: #fdf3f3;
  border-radius: 8px;
}
.missing-alert { flex: 1 1 auto; }
</style>
