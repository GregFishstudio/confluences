<template>
  <v-container fluid class="pa-4">
    <div class="d-flex align-center mb-6">
      <v-btn icon variant="text" :to="{ name: 'Participants' }" class="mr-2">
        <v-icon>mdi-arrow-left</v-icon>
      </v-btn>
      <v-avatar size="48" :color="avatarColor" class="mr-4">
        <span class="text-subtitle-1 font-weight-bold text-white">{{ initials }}</span>
      </v-avatar>
      <div>
        <div class="text-h5 font-weight-bold">{{ user.prenom }} {{ user.nom }}</div>
        <div class="text-caption text-grey">{{ user.userName }}</div>
      </div>
    </div>

    <v-row v-if="loading">
      <v-col cols="12" class="text-center pa-8">
        <v-progress-circular indeterminate color="primary" />
      </v-col>
    </v-row>

    <v-row v-else>
      <!-- ── Identité & Contact (éditable, fusion Identité + Infos générales) ── -->
      <v-col cols="12">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-4">Identité & Contact</div>
          <v-row dense>
            <v-col cols="6" md="3"><v-text-field v-model="form.firstname" label="Prénom" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.lastName" label="Nom" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3">
              <v-text-field v-model="form.birthday" type="date" label="Date de naissance" variant="outlined" density="compact" />
            </v-col>
            <v-col cols="6" md="3">
              <v-select v-model="form.genderId" :items="genderOptions" item-title="title" item-value="value" label="Genre" variant="outlined" density="compact" clearable />
            </v-col>

            <v-col cols="6" md="3"><v-text-field v-model="form.nationality" label="Nationalité" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.language" label="Langue maternelle" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.autresLangues" label="Autre(s) langue(s)" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.phoneNumber" label="Téléphone" variant="outlined" density="compact" /></v-col>

            <v-col cols="6" md="4"><v-text-field v-model="form.adresse" label="Adresse postale" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="4"><v-text-field :model-value="user.email" label="Email" variant="outlined" density="compact" disabled hint="Se modifie depuis la fiche compte (identifiant de connexion)" persistent-hint /></v-col>
            <v-col cols="6" md="4"><v-text-field v-model="form.etatCivil" label="État civil" variant="outlined" density="compact" /></v-col>

            <v-col cols="6" md="3"><v-text-field v-model.number="form.nombreEnfants" type="number" label="Nombre d'enfants" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3">
              <v-select v-model="form.beneficiaireDe" :items="['CSIR', 'CSR']" label="Bénéficiaire de" variant="outlined" density="compact" clearable />
            </v-col>
            <v-col cols="12" md="6">
              <div class="d-flex align-center ga-2">
                <v-text-field v-model="form.assistantSocialEmail" label="Assistant·e social·e (email)" variant="outlined" density="compact" class="flex-grow-1" />
                <FieldHistoryDialog
                  v-if="user.id"
                  entity-type="ApplicationUser" :entity-id="user.id" field-name="AssistantSocialEmail"
                  label="Assistant·e social·e"
                />
              </div>
            </v-col>

            <v-col cols="6" md="3"><v-switch v-model="form.contratMaori" label="Contrat Maori" color="primary" hide-details /></v-col>
          </v-row>
          <div class="d-flex justify-end mt-2">
            <v-btn color="primary" :loading="saving" @click="save">Sauvegarder</v-btn>
          </div>
        </v-card>
      </v-col>

      <!-- ── Rouge : mesures suivies ── -->
      <v-col cols="12">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4 section-red" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-4 text-red-darken-2">Mesures suivies</div>

          <div v-if="!userSessionLinks.length" class="text-caption text-grey">Aucune session trouvée.</div>
          <v-card v-for="link in userSessionLinks" :key="link.sessionId + link.studentId" variant="outlined" class="pa-3 mb-3">
            <div class="d-flex align-center justify-space-between flex-wrap ga-2">
              <div class="text-body-1 font-weight-bold session-link" title="Lien vers la fiche de présence (disponible prochainement)">
                {{ sessionLabel(link) }}
              </div>
              <v-chip v-if="link === currentSessionLink && isActif" size="x-small" color="success" label variant="tonal">En cours</v-chip>
            </div>
            <div class="text-caption text-grey">
              {{ formatDate(link.session?.dateStart) }} → {{ formatDate(link.session?.dateEnd) }}
            </div>

            <div class="mt-2 d-flex align-center flex-wrap ga-1">
              <v-chip v-if="getNiveauForSession(link)" size="x-small" color="green" label title="Niveau (disponible prochainement sous Bilans & attest.)">O: {{ getNiveauForSession(link)?.oralLevel || '—' }}</v-chip>
              <v-chip v-if="getNiveauForSession(link)" size="x-small" color="blue" label title="Niveau (disponible prochainement sous Bilans & attest.)">É: {{ getNiveauForSession(link)?.writtenLevel || '—' }}</v-chip>
              <v-chip v-if="link.tauxParticipation !== null" size="x-small" :color="tauxColor(link.tauxParticipation)" label>
                Participation : {{ link.tauxParticipation }}%
              </v-chip>
              <v-chip size="x-small" color="grey-darken-1" label variant="tonal">
                Accomp. : {{ accompCounts[link.sessionId]?.acco ?? 0 }}
              </v-chip>
              <v-chip size="x-small" color="grey-darken-1" label variant="tonal">
                Ateliers : {{ accompCounts[link.sessionId]?.atelier ?? 0 }}
              </v-chip>
            </div>

            <div class="mt-2 d-flex ga-2">
              <v-btn v-if="bilanAvailable[link.sessionId]" size="small" variant="tonal" color="primary" prepend-icon="mdi-file-pdf-box" :href="sessionDocUrl('bilan-session', link)" target="_blank">
                Bilan
              </v-btn>
              <v-btn size="small" variant="tonal" color="primary" prepend-icon="mdi-file-pdf-box" :href="sessionDocUrl('attestation-session', link)" target="_blank">
                Attestation
              </v-btn>
            </div>
          </v-card>

          <div v-if="tauxTotal !== null" class="text-caption font-weight-bold text-right">
            Total participation : {{ tauxTotal }}%
          </div>
        </v-card>
      </v-col>

      <!-- ── Infos relatives à l'insertion ── -->
      <v-col cols="12">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4 section-blue" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-4 text-blue-darken-2">Infos relatives à l'insertion</div>

          <div class="text-caption text-grey mb-1">Expériences professionnelles</div>
          <div v-if="!userStages.length" class="text-caption text-grey mb-3">Aucune expérience professionnelle.</div>
          <div v-for="stage in userStages" :key="stage.id" class="mb-3">
            <div class="d-flex align-center ga-2 flex-wrap">
              <span class="font-weight-bold insertion-link" title="Lien vers la fiche stage (disponible prochainement)">{{ stage.metier || stage.entreprise?.nom || 'Expérience professionnelle' }}</span>
              <v-chip v-if="stage.typeStage" size="x-small" label variant="tonal">{{ stage.typeStage }}</v-chip>
            </div>
            <div class="text-caption text-grey">
              <span v-if="stage.entreprise?.nom" class="insertion-link" title="Lien vers la fiche entreprise (disponible prochainement)">{{ stage.entreprise.nom }}</span>
              — {{ formatDate(stage.dateDebut) }}<span v-if="stage.dateFin"> → {{ formatDate(stage.dateFin) }}</span>
            </div>
            <div v-if="stage.files?.length" class="mt-1 d-flex flex-wrap ga-1">
              <v-chip v-for="f in stage.files" :key="f.id" size="x-small" variant="outlined" prepend-icon="mdi-file-outline">{{ f.fileName }}</v-chip>
            </div>
          </div>

          <v-divider class="my-3" />

          <div class="text-caption text-grey mb-1">Marrainage</div>
          <div v-if="!marrainages.length" class="text-caption text-grey">Aucun marrainage.</div>
          <div v-for="m in marrainages" :key="m.marrainageId" class="mb-2">
            <div class="d-flex align-center ga-2">
              <a href="#" class="marrainage-link font-weight-bold" @click.prevent="goToMarrainage(m.marrainageId)">
                {{ m.prenom }} {{ m.nom }}
              </a>
              <v-chip size="x-small" :color="m.actif ? 'success' : 'error'" label :variant="m.actif ? 'tonal' : 'flat'">
                {{ m.actif ? 'Actif' : 'Inactif' }}
              </v-chip>
            </div>
            <div class="text-caption text-grey">Démarré le {{ formatDate(m.date) }}</div>
            <v-select
              v-if="!m.actif"
              :model-value="m.raisonInactivite"
              @update:model-value="(v) => updateRaison(m, v)"
              :items="raisonOptions"
              label="Raison de l'inactivité"
              variant="outlined" density="compact" clearable hide-details
              class="mt-1"
            />
          </div>
        </v-card>
      </v-col>

      <!-- ── Vert : suivi formatrices ── -->
      <v-col cols="12">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4 section-green" variant="outlined">
          <div class="d-flex align-center mb-4">
            <div class="text-subtitle-1 font-weight-bold text-green-darken-2">Suivi formatrices</div>
            <span v-if="user.profilUpdatedAt" class="text-caption text-grey ml-2">
              — mis à jour le {{ formatDate(user.profilUpdatedAt) }}<span v-if="user.profilUpdatedByName"> par {{ user.profilUpdatedByName }}</span>
            </span>
          </div>
          <v-row dense>
            <v-col cols="6">
              <v-text-field v-model.number="form.anneeArriveeSuisse" type="number" label="Année d'arrivée en Suisse" variant="outlined" density="compact" />
            </v-col>
            <v-col cols="6">
              <v-text-field v-model="form.niveauDebutContrat" label="Niveau début du contrat" variant="outlined" density="compact" />
            </v-col>
            <v-col cols="12" md="6">
              <v-textarea v-model="form.parcoursDepuisArriveeSuisse" label="Parcours depuis arrivée en Suisse (obtention du permis)" variant="outlined" auto-grow rows="2" />
            </v-col>
            <v-col cols="12" md="6">
              <v-textarea v-model="form.parcoursFormationSuisse" label="Parcours de formation en Suisse" variant="outlined" auto-grow rows="2" />
            </v-col>

            <v-col cols="12" md="4">
              <v-select v-model="form.plusHautesEtudes" :items="etudesOptions" label="Plus hautes études / études pays d'origine" variant="outlined" clearable />
            </v-col>
            <v-col cols="12" md="8" class="d-flex align-center ga-2">
              <v-textarea v-model="form.plusHautesEtudesCommentaire" label="Commentaire" variant="outlined" auto-grow rows="1" class="flex-grow-1" />
              <FieldHistoryDialog v-if="user.id" entity-type="ApplicationUser" :entity-id="user.id" field-name="PlusHautesEtudesCommentaire" label="Plus hautes études — commentaire" />
            </v-col>

            <v-col cols="12" md="6">
              <v-textarea v-model="form.domaineEtudeTitreObtenu" label="Domaine d'étude et titre obtenu" variant="outlined" auto-grow rows="2" />
            </v-col>
            <v-col cols="12" md="6">
              <v-textarea v-model="form.parcoursProfessionnel" label="Parcours professionnel" variant="outlined" auto-grow rows="2" />
            </v-col>

            <v-col cols="12" md="4">
              <v-switch v-model="form.activiteProfSuisse" label="Activité professionnelle en Suisse" color="primary" hide-details />
            </v-col>
            <v-col cols="12" md="8" class="d-flex align-center ga-2">
              <v-textarea v-model="form.activiteProfSuisseCommentaire" label="Commentaire" variant="outlined" auto-grow rows="1" class="flex-grow-1" />
              <FieldHistoryDialog v-if="user.id" entity-type="ApplicationUser" :entity-id="user.id" field-name="ActiviteProfSuisseCommentaire" label="Activité professionnelle en Suisse — commentaire" />
            </v-col>

            <v-col cols="12">
              <v-textarea v-model="form.besoinsSouhaitsFrancais" label="Besoins / souhaits (français)" variant="outlined" auto-grow rows="2" />
            </v-col>

            <v-col cols="12" md="4">
              <v-switch v-model="form.rechercheLogement" label="Recherche logement" color="primary" hide-details />
            </v-col>
            <v-col cols="12" md="8" class="d-flex align-center ga-2">
              <v-textarea v-model="form.rechercheLogementCommentaire" label="Commentaire" variant="outlined" auto-grow rows="1" class="flex-grow-1" />
              <FieldHistoryDialog v-if="user.id" entity-type="ApplicationUser" :entity-id="user.id" field-name="RechercheLogementCommentaire" label="Recherche logement — commentaire" />
            </v-col>

            <v-col cols="12" md="4">
              <v-switch v-model="form.besoinsSouhaitsStages" label="Besoins / souhaits - stages" color="primary" hide-details />
            </v-col>
            <v-col cols="12" md="8" class="d-flex align-center ga-2">
              <v-textarea v-model="form.besoinsSouhaitsStagesCommentaire" label="Commentaire" variant="outlined" auto-grow rows="1" class="flex-grow-1" />
              <FieldHistoryDialog v-if="user.id" entity-type="ApplicationUser" :entity-id="user.id" field-name="BesoinsSouhaitsStagesCommentaire" label="Besoins / souhaits stages — commentaire" />
            </v-col>

            <v-col cols="12" md="4">
              <v-switch v-model="form.besoinsSouhaitsBenevolat" label="Besoins / souhaits - bénévolat" color="primary" hide-details />
            </v-col>
            <v-col cols="12" md="8" class="d-flex align-center ga-2">
              <v-textarea v-model="form.besoinsSouhaitsBenevoletCommentaire" label="Commentaire" variant="outlined" auto-grow rows="1" class="flex-grow-1" />
              <FieldHistoryDialog v-if="user.id" entity-type="ApplicationUser" :entity-id="user.id" field-name="BesoinsSouhaitsBenevoletCommentaire" label="Besoins / souhaits bénévolat — commentaire" />
            </v-col>

            <v-col cols="12">
              <v-textarea v-model="form.autre" label="Autre" variant="outlined" auto-grow rows="2" />
            </v-col>

            <v-col cols="6">
              <v-text-field v-model="form.niveauFinContrat" label="Niveau fin du contrat" variant="outlined" density="compact" />
            </v-col>
          </v-row>

          <div class="d-flex justify-end mt-2">
            <v-btn color="primary" :loading="saving" @click="save">Sauvegarder</v-btn>
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" lg="6">
        <FileAttachments
          title="CV / Lettre de motivation"
          controller="ParticipantFiles"
          id-key="participantFileId"
          :extra-fields="{ studentId: user.id }"
          :category-options="['CV', 'LM']"
          v-model="files"
        />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import FileAttachments from '@/components/FileAttachments.vue'
import FieldHistoryDialog from '@/components/FieldHistoryDialog.vue'

moment.locale('fr')
const route = useRoute()
const router = useRouter()

const loading = ref(true)
const saving = ref(false)
const user = ref({})
const files = ref([])
const allStages = ref([])
const allNiveaux = ref([])
const genders = ref([])
const marrainages = ref([])

const raisonOptions = [
  'Contraintes familiales',
  'En emploi',
  'Déménagement',
  'Fin de formation du/de la filleul·e',
  'Manque de disponibilité',
  'Autre'
]

const etudesOptions = ['aucune', 'école primaire', 'école secondaire', 'gymnase', 'université']

const genderOptions = computed(() =>
  genders.value.map(g => ({ title: g.GenderName ?? g.genderName, value: g.GenderId ?? g.genderId }))
)

const form = ref({
  firstname: null,
  lastName: null,
  phoneNumber: null,
  nationality: null,
  language: null,
  birthday: null,
  genderId: null,
  contratMaori: false,
  etatCivil: null,
  nombreEnfants: null,
  adresse: null,
  beneficiaireDe: null,
  assistantSocialEmail: null,
  anneeArriveeSuisse: null,
  autresLangues: null,
  parcoursDepuisArriveeSuisse: null,
  parcoursFormationSuisse: null,
  domaineEtudeTitreObtenu: null,
  parcoursProfessionnel: null,
  besoinsSouhaitsFrancais: null,
  autre: null,
  plusHautesEtudes: null,
  plusHautesEtudesCommentaire: null,
  activiteProfSuisse: false,
  activiteProfSuisseCommentaire: null,
  rechercheLogement: false,
  rechercheLogementCommentaire: null,
  besoinsSouhaitsStages: false,
  besoinsSouhaitsStagesCommentaire: null,
  besoinsSouhaitsBenevolat: false,
  besoinsSouhaitsBenevoletCommentaire: null,
  niveauDebutContrat: null,
  niveauFinContrat: null
})

const sessionLinks = ref([])
const userSessionLinks = computed(() => sessionLinks.value)

// Le lien le plus récent (année puis n° de session décroissants) sert de "session actuelle" —
// plus fiable que ApplicationUser.CurrentSessionNumber/CurrentSessionYear, qui sont saisis à
// part et peuvent rester vides même quand le/la participant·e suit bien une session.
const currentSessionLink = computed(() => sessionLinks.value[0] || null)

const isActif = computed(() => {
  const link = currentSessionLink.value
  if (!link?.session) return false
  const linkYear = new Date(link.session.dateStart || 0).getFullYear()
  return linkYear === new Date().getFullYear()
})

const tauxTotal = computed(() => {
  const values = sessionLinks.value.map(l => l.tauxParticipation).filter(v => v !== null && v !== undefined)
  if (!values.length) return null
  return Math.round((values.reduce((sum, v) => sum + v, 0) / values.length) * 10) / 10
})

const tauxColor = (taux) => {
  if (taux === null || taux === undefined) return 'grey'
  if (taux >= 80) return 'success'
  if (taux >= 50) return 'orange'
  return 'error'
}

async function loadParticipationRates() {
  await Promise.all(sessionLinks.value.map(async (link) => {
    try {
      const res = await apiFetch(`${API_BASE}/api/SessionStudents/${link.sessionId}/${link.studentId}/taux`, { method: 'POST' })
      if (res && res.ok) {
        const data = await res.json()
        link.tauxParticipation = pick(data, 'tauxParticipation', 'TauxParticipation')
      }
    } catch (e) { console.error(e) }
  }))
}

const userStages = computed(() =>
  allStages.value.filter(s => String(s.stagiaireId || '').toLowerCase() === String(user.value.id).toLowerCase())
)

const getNiveauForSession = (link) => {
  const s = link.session
  const year = s ? new Date(s.dateStart).getFullYear() : null
  const num = s?.sessionNumberId
  if (!year || !num) return null
  return allNiveaux.value.find(n => n.stagiaireId === user.value.id && n.annee === year && n.trimestre === num) || null
}

const getSessionYear = (link) => link.session ? new Date(link.session.dateStart).getFullYear() : '—'

const accompCounts = ref({})
const bilanAvailable = ref({})

const sessionLabel = (link) => {
  const room = (link.schoolClassRoomName || '').replace(/-/g, ' ').trim()
  const num = link.session?.sessionNumberId
  const year = getSessionYear(link)
  const sessionPart = num ? `Session ${num} ${year}` : null
  return [room, sessionPart].filter(Boolean).join(' – ') || 'Session'
}

const sessionDocUrl = (kind, link) => {
  const year = getSessionYear(link)
  const num = link.session?.sessionNumberId
  const params = new URLSearchParams({
    year: String(year),
    trimestre: String(num),
    mesure: link.schoolClassRoomName || '',
  })
  if (link.session?.dateStart) params.set('startDate', link.session.dateStart)
  if (link.session?.dateEnd) params.set('endDate', link.session.dateEnd)
  return `${API_BASE}/api/documents/${kind}/${user.value.id}?${params.toString()}`
}

async function loadSessionExtras() {
  await Promise.all(sessionLinks.value.map(async (link) => {
    const year = getSessionYear(link)
    const num = link.session?.sessionNumberId
    if (!year || !num) return
    try {
      const res = await apiFetch(`${API_BASE}/api/accompanying/summary/${user.value.id}?year=${year}&quarter=${num}`)
      if (res && res.ok) {
        const rows = await res.json()
        const acco = rows.filter(r => pick(r, 'type', 'Type') === 'acco').reduce((sum, r) => sum + pick(r, 'count', 'Count'), 0)
        const atelier = rows.filter(r => pick(r, 'type', 'Type') === 'atelier').reduce((sum, r) => sum + pick(r, 'count', 'Count'), 0)
        accompCounts.value[link.sessionId] = { acco, atelier }
      }
    } catch (e) { console.error(e) }
    try {
      const res = await apiFetch(`${API_BASE}/api/documents/bilan-session-data/${user.value.id}?year=${year}&quarter=${num}`)
      if (res && res.ok) {
        const data = await res.json()
        bilanAvailable.value[link.sessionId] = !!data
      }
    } catch (e) { console.error(e) }
  }))
}

const COLORS = ['#1565C0', '#2E7D32', '#6A1B9A', '#E65100', '#00695C', '#AD1457']
const avatarColor = computed(() => COLORS[((user.value.prenom || '')[0] || '').charCodeAt(0) % COLORS.length])
const initials = computed(() => `${(user.value.prenom || '')[0] || ''}${(user.value.nom || '')[0] || ''}`.toUpperCase() || 'U')

const formatDate = (d) => d ? moment(d).format('DD.MM.YYYY') : '—'

const goToMarrainage = (id) => {
  router.push({ name: 'Marrainage-Modifier', params: { id } })
}

async function updateRaison(marrainage, raison) {
  await apiFetch(`${API_BASE}/api/Marrainages/${marrainage.marrainageId}/statut`, {
    method: 'PATCH',
    body: JSON.stringify({ actif: marrainage.actif, raisonInactivite: raison })
  })
  marrainage.raisonInactivite = raison
}

// AddNewtonsoftJson() applique par défaut un CamelCasePropertyNamesContractResolver
// côté API : les clés JSON sont donc en camelCase (firstname, sessionStudents,
// session.dateStart, ...), pas en PascalCase. On garde un `?? Pascal` en secours
// uniquement pour les endpoints qui, eux, projettent des objets déjà nommés
// explicitement (donc non affectés par le resolver).
const pick = (o, camel, pascal) => o?.[camel] ?? o?.[pascal] ?? null

function fillFormFromUser(u) {
  form.value.firstname                          = pick(u, 'firstname', 'Firstname')
  form.value.lastName                            = pick(u, 'lastName', 'LastName')
  form.value.phoneNumber                         = pick(u, 'phoneNumber', 'PhoneNumber')
  form.value.nationality                         = pick(u, 'nationality', 'Nationality')
  form.value.language                            = pick(u, 'language', 'Language')
  const birthday                                 = pick(u, 'birthday', 'Birthday')
  form.value.birthday                            = birthday ? moment(birthday).format('YYYY-MM-DD') : null
  form.value.genderId                            = pick(u, 'genderId', 'GenderId')
  form.value.contratMaori                        = pick(u, 'contratMaori', 'ContratMaori') ?? false
  form.value.etatCivil                           = pick(u, 'etatCivil', 'EtatCivil')
  form.value.nombreEnfants                       = pick(u, 'nombreEnfants', 'NombreEnfants')
  form.value.adresse                             = pick(u, 'adresse', 'Adresse')
  form.value.beneficiaireDe                      = pick(u, 'beneficiaireDe', 'BeneficiaireDe')
  form.value.assistantSocialEmail                = pick(u, 'assistantSocialEmail', 'AssistantSocialEmail')
  form.value.anneeArriveeSuisse                  = pick(u, 'anneeArriveeSuisse', 'AnneeArriveeSuisse')
  form.value.autresLangues                       = pick(u, 'autresLangues', 'AutresLangues')
  form.value.parcoursDepuisArriveeSuisse         = pick(u, 'parcoursDepuisArriveeSuisse', 'ParcoursDepuisArriveeSuisse')
  form.value.parcoursFormationSuisse             = pick(u, 'parcoursFormationSuisse', 'ParcoursFormationSuisse')
  form.value.domaineEtudeTitreObtenu             = pick(u, 'domaineEtudeTitreObtenu', 'DomaineEtudeTitreObtenu')
  form.value.parcoursProfessionnel               = pick(u, 'parcoursProfessionnel', 'ParcoursProfessionnel')
  form.value.besoinsSouhaitsFrancais             = pick(u, 'besoinsSouhaitsFrancais', 'BesoinsSouhaitsFrancais')
  form.value.autre                               = pick(u, 'autre', 'Autre')
  form.value.plusHautesEtudes                    = pick(u, 'plusHautesEtudes', 'PlusHautesEtudes')
  form.value.plusHautesEtudesCommentaire         = pick(u, 'plusHautesEtudesCommentaire', 'PlusHautesEtudesCommentaire')
  form.value.activiteProfSuisse                  = pick(u, 'activiteProfSuisse', 'ActiviteProfSuisse') ?? false
  form.value.activiteProfSuisseCommentaire       = pick(u, 'activiteProfSuisseCommentaire', 'ActiviteProfSuisseCommentaire')
  form.value.rechercheLogement                   = pick(u, 'rechercheLogement', 'RechercheLogement') ?? false
  form.value.rechercheLogementCommentaire        = pick(u, 'rechercheLogementCommentaire', 'RechercheLogementCommentaire')
  form.value.besoinsSouhaitsStages               = pick(u, 'besoinsSouhaitsStages', 'BesoinsSouhaitsStages') ?? false
  form.value.besoinsSouhaitsStagesCommentaire    = pick(u, 'besoinsSouhaitsStagesCommentaire', 'BesoinsSouhaitsStagesCommentaire')
  form.value.besoinsSouhaitsBenevolat            = pick(u, 'besoinsSouhaitsBenevolat', 'BesoinsSouhaitsBenevolat') ?? false
  form.value.besoinsSouhaitsBenevoletCommentaire = pick(u, 'besoinsSouhaitsBenevoletCommentaire', 'BesoinsSouhaitsBenevoletCommentaire')
  form.value.niveauDebutContrat                  = pick(u, 'niveauDebutContrat', 'NiveauDebutContrat')
  form.value.niveauFinContrat                    = pick(u, 'niveauFinContrat', 'NiveauFinContrat')
}

function normalizeUser(data) {
  const sessionStudents = pick(data, 'sessionStudents', 'SessionStudents') || []
  return {
    id: pick(data, 'id', 'Id'),
    prenom: pick(data, 'firstname', 'Firstname'),
    nom: pick(data, 'lastName', 'LastName'),
    userName: pick(data, 'userName', 'UserName'),
    email: pick(data, 'email', 'Email'),
    currentSessionNumber: pick(data, 'currentSessionNumber', 'CurrentSessionNumber'),
    currentSessionYear: pick(data, 'currentSessionYear', 'CurrentSessionYear'),
    profilUpdatedAt: pick(data, 'profilUpdatedAt', 'ProfilUpdatedAt'),
    profilUpdatedByName: pick(data, 'profilUpdatedByName', 'ProfilUpdatedByName'),
    sessionStudents: sessionStudents.map(ss => {
      const session = pick(ss, 'session', 'Session')
      const schoolClassRoom = session ? pick(session, 'schoolClassRoom', 'SchoolClassRoom') : null
      return {
        sessionId: pick(ss, 'sessionId', 'SessionId'),
        studentId: pick(ss, 'studentId', 'StudentId'),
        tauxParticipation: pick(ss, 'tauxParticipation', 'TauxParticipation'),
        schoolClassRoomName: schoolClassRoom ? pick(schoolClassRoom, 'schoolClassRoomName', 'SchoolClassRoomName') : null,
        session: session ? {
          sessionNumberId: pick(session, 'sessionNumberId', 'SessionNumberId'),
          dateStart: pick(session, 'dateStart', 'DateStart'),
          dateEnd: pick(session, 'dateEnd', 'DateEnd'),
        } : null,
      }
    }),
  }
}

function normalizeMarrainage(m) {
  return {
    marrainageId: pick(m, 'marrainageId', 'MarrainageId'),
    prenom: pick(m, 'prenom', 'Prenom'),
    nom: pick(m, 'nom', 'Nom'),
    date: pick(m, 'date', 'Date'),
    actif: pick(m, 'actif', 'Actif'),
    raisonInactivite: pick(m, 'raisonInactivite', 'RaisonInactivite'),
  }
}

function normalizeStage(s) {
  const entreprise = pick(s, 'entreprise', 'Entreprise')
  const typeMetier = pick(s, 'typeMetier', 'TypeMetier')
  const typeStage = pick(s, 'typeStage', 'TypeStage')
  const stageFiles = pick(s, 'stageFiles', 'StageFiles') || []
  return {
    id: pick(s, 'stageId', 'StageId'),
    stagiaireId: pick(s, 'stagiaireId', 'StagiaireId'),
    dateDebut: pick(s, 'debut', 'Debut'),
    dateFin: pick(s, 'fin', 'Fin'),
    metier: typeMetier ? pick(typeMetier, 'libelle', 'Libelle') : null,
    typeStage: typeStage ? pick(typeStage, 'nom', 'Nom') : null,
    entreprise: entreprise ? { id: pick(entreprise, 'entrepriseId', 'EntrepriseId'), nom: pick(entreprise, 'nom', 'Nom') } : null,
    files: stageFiles.map(f => ({
      id: pick(f, 'stageFileId', 'StageFileId'),
      fileName: pick(f, 'fileName', 'FileName'),
    })),
  }
}

async function load() {
  loading.value = true
  try {
    const id = route.params.id
    const res = await apiFetch(`${API_BASE}/api/ApplicationUsers/${id}`)
    if (res && res.ok) {
      const data = await res.json()
      user.value = normalizeUser(data)
      fillFormFromUser(data)
      files.value = pick(data, 'participantFiles', 'ParticipantFiles') || []
      sessionLinks.value = (user.value.sessionStudents || []).slice().sort((a, b) => {
        const aY = new Date(a.session?.dateStart || 0).getFullYear()
        const bY = new Date(b.session?.dateStart || 0).getFullYear()
        if (bY !== aY) return bY - aY
        return (b.session?.sessionNumberId || 0) - (a.session?.sessionNumberId || 0)
      })
    }

    const [stagesRes, niveauxRes, gendersRes, marrainagesRes] = await Promise.all([
      apiFetch(`${API_BASE}/api/stages`),
      apiFetch(`${API_BASE}/api/StagiaireNiveaux`),
      apiFetch(`${API_BASE}/api/Genders`),
      apiFetch(`${API_BASE}/api/Marrainages?participantId=${route.params.id}`),
    ])
    if (stagesRes && stagesRes.ok) allStages.value = (await stagesRes.json()).map(normalizeStage)
    if (niveauxRes && niveauxRes.ok) allNiveaux.value = await niveauxRes.json()
    if (gendersRes && gendersRes.ok) genders.value = await gendersRes.json()
    if (marrainagesRes && marrainagesRes.ok) marrainages.value = (await marrainagesRes.json()).map(normalizeMarrainage)

    loadParticipationRates()
    loadSessionExtras()
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    await apiFetch(`${API_BASE}/api/ApplicationUsers/${user.value.id}/profile`, {
      method: 'PATCH',
      body: JSON.stringify(form.value)
    })
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.section-blue { border-left: 4px solid #1976D2; }
.section-red { border-left: 4px solid #C62828; }
.section-green { border-left: 4px solid #2E7D32; }
.marrainage-link { color: #1976D2; text-decoration: underline; }
.session-link, .insertion-link { border-bottom: 1px dashed #90A4AE; cursor: default; }
</style>
