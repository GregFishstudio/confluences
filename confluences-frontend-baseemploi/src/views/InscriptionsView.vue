<!--
  Rubrique "Inscriptions" (menu Mesures) — remplace les deux tableaux Excel :
   - candidat·e·s : demandes d'inscription des AS ;
   - participant·e·s d'une session : inscrit·e·s (renouvellements, interruptions,
     non-renouvellements) + renouvellements prévus + nouveaux·elles candidat·e·s visant la session ;
   - carnet des assistant·e·s sociaux·ales.
  Sessions et groupes : même logique que Bilans & attestations (calendrier de formation).
-->
<template>
  <v-container fluid class="pa-2 pa-md-4 insc">
    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-tabs v-model="tab" class="px-4" color="primary">
        <v-tab value="candidatures" prepend-icon="mdi-account-clock">Candidat·e·s</v-tab>
        <v-tab value="participants" prepend-icon="mdi-account-group">Participant·e·s</v-tab>
        <v-tab value="as" prepend-icon="mdi-card-account-phone">Assistant·e·s sociaux·ales</v-tab>
      </v-tabs>
    </v-card>

    <v-window v-model="tab" :touch="false">
      <!-- ═══════════ Candidat·e·s ═══════════ -->
      <v-window-item value="candidatures">
        <CandidaturesTab
          ref="candidaturesTab" :assistants="assistants" :calendar-sessions="calendarSessions"
          @assistants-changed="loadAssistants" @changed="invalidateSessions"
        />
      </v-window-item>

      <!-- ═══════════ Participant·e·s d'une session ═══════════ -->
      <v-window-item value="participants">
        <!-- Navigation entre sessions (comme la navigation par session des autres pages) -->
        <v-card class="mb-4 elevation-2" rounded="lg">
          <v-card-text class="pa-4">
            <div class="d-flex justify-space-between align-center">
              <v-btn icon :disabled="navIndex <= 0" aria-label="Session précédente" @click="navIndex--">
                <v-icon>mdi-chevron-left</v-icon>
              </v-btn>
              <div class="text-center">
                <div class="text-h6 font-weight-bold text-primary">{{ selectedSession?.label }}</div>
                <div class="text-caption text-grey">
                  <v-icon v-if="selectedSession?.planned" size="x-small" class="mr-1">mdi-calendar-question</v-icon>
                  {{ selectedSession?.dates }}
                </div>
              </div>
              <v-btn icon :disabled="navIndex >= groupSessions.length - 1" aria-label="Session suivante" @click="navIndex++">
                <v-icon>mdi-chevron-right</v-icon>
              </v-btn>
            </div>
          </v-card-text>
        </v-card>

        <!-- Groupes (mêmes onglets que les autres pages) -->
        <v-card class="mb-4 elevation-1" rounded="lg">
          <v-tabs v-model="groupe" class="px-4" color="primary">
            <v-tab v-for="g in GROUPES" :key="g.value" :value="g.value">
              {{ g.label }}
              <v-chip size="x-small" class="ml-2" color="primary" variant="outlined">{{ groupTotal(g.value) }}</v-chip>
            </v-tab>
          </v-tabs>
        </v-card>

        <v-card class="elevation-4" rounded="lg" :loading="sessionLoading">
          <v-row align="center" class="px-2">
            <v-col cols="12" md="6" class="pa-3 pa-md-6">
              <v-text-field
                v-model="search" append-icon="mdi-magnify" label="Rechercher (nom, origine, AS, statut...)"
                variant="outlined" density="compact" clearable hide-details bg-color="white"
              />
            </v-col>
            <v-col cols="12" md="6" class="pa-3 pa-md-6 d-flex justify-end">
              <v-btn color="primary" variant="text" size="small" prepend-icon="mdi-file-import-outline" class="mr-2" @click="importForSession">
                Importer un CSV
              </v-btn>
              <v-btn color="primary" variant="outlined" size="small" prepend-icon="mdi-account-plus-outline" @click="newCandidateForSession">
                Nouveau·elle candidat·e pour cette session
              </v-btn>
            </v-col>
          </v-row>
          <v-divider />

          <div v-if="!sessionLoading && !allRows.length" class="insc-empty-state">
            <v-icon size="40" color="grey-lighten-1">mdi-account-search-outline</v-icon>
            <div class="mt-2">Personne pour cette session dans ce groupe.</div>
            <div class="text-caption">Ajoutez un·e candidat·e, ou orientez des participant·e·s de la session précédente vers un renouvellement.</div>
          </div>

          <template v-else-if="allRows.length">
            <!-- Récap en tête de tableau : le total reste visible quelle que soit la longueur de la liste -->
            <div class="insc-recap">
              <div class="insc-recap-total">
                <span class="n">{{ recap.total }}</span>
                <span class="l">participant·e·s {{ recap.prevus ? 'prévu·e·s' : '' }}</span>
              </div>
              <div class="insc-bar" role="img" :aria-label="recapAria">
                <span v-for="s in recap.segments" :key="s.key" :style="{ width: `${s.pct}%`, background: s.color }" />
              </div>
              <div class="insc-legend">
                <span v-for="s in recap.legend" :key="s.key"><i :style="{ background: s.color }" /><b>{{ s.n }}</b> {{ s.label }}</span>
              </div>
            </div>
            <div class="insc-scroll">
              <table class="insc-table">
                <thead>
                  <tr>
                    <th>Bénéficiaire</th>
                    <th>Statut</th>
                    <th>AS</th>
                    <th>Niveau contrat</th>
                    <th>Objectifs atteints</th>
                    <th>Orientation préconisée</th>
                    <th>Comm. AS</th>
                    <th class="text-center">Contrat Maori</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="r in visibleInscrits" :key="r.key" class="clickable" tabindex="0"
                    :class="{ 'is-muted': r.categorie === 'interrompu' }" :style="{ '--rail': RAIL[r.categorie] }"
                    @click="openRow(r)" @keydown.enter="openRow(r)"
                  >
                    <td>
                      <div class="insc-person">
                        <div class="insc-initials">{{ initiales(r.prenom, r.nom) }}</div>
                        <div>
                          <div class="insc-name">{{ r.nom }} <span class="first">{{ r.prenom }}</span></div>
                          <div class="insc-sub">{{ [r.origine, r.mandant].filter(Boolean).join(' · ') || '—' }}</div>
                        </div>
                      </div>
                    </td>
                    <td>
                      <span class="insc-pill">{{ statutLabel(r) }}</span>
                      <div v-if="r.dateFinParticipation" class="insc-sub">le {{ formatDate(r.dateFinParticipation) }}</div>
                    </td>
                    <td><span class="insc-sub" :class="{ 'insc-empty': !r.assistantSocialEmail }">{{ asDisplay(r.assistantSocialEmail) }}</span></td>
                    <td class="insc-strong" style="white-space: nowrap">
                      {{ r.niveauDebutContrat || '—' }} <span class="insc-empty">→</span> {{ r.niveauFinContrat || '—' }}
                    </td>
                    <td>
                      <v-chip v-if="r.objectifsAtteints" size="x-small" :color="objectifsColor(r.objectifsAtteints)" variant="tonal">{{ r.objectifsAtteints }}</v-chip>
                      <div v-if="r.objectifsCommentaire" class="insc-note quote mt-1">{{ r.objectifsCommentaire }}</div>
                    </td>
                    <td>
                      <div class="text-primary insc-strong">{{ r.orientationPreconisee }}</div>
                      <div v-if="r.orientationPrecisions" class="insc-note quote mt-1">{{ r.orientationPrecisions }}</div>
                    </td>
                    <td class="insc-sub">{{ r.communicationAS }}</td>
                    <td class="text-center" @click.stop>
                      <v-checkbox-btn
                        :model-value="!!r.contratMaori" color="red" class="d-inline-flex" :title="r.contratMaori ? 'Contrat Maori reçu' : 'Contrat Maori non reçu'"
                        @update:model-value="(v) => toggleContratMaori(r, v)"
                      />
                    </td>
                  </tr>

                  <tr v-if="visiblePrevus.length" class="insc-section">
                    <td colspan="8">
                      <div class="d-flex align-center flex-wrap ga-2">
                        <span>Pas encore inscrit·e·s dans cette session</span>
                        <v-spacer />
                        <v-btn size="small" color="primary" variant="flat" prepend-icon="mdi-account-check-outline" @click="openInscription">
                          Inscrire dans la session…
                        </v-btn>
                      </div>
                    </td>
                  </tr>
                  <tr
                    v-for="r in visiblePrevus" :key="r.key" class="clickable" tabindex="0"
                    :style="{ '--rail': RAIL[r.categorie] }"
                    @click="openRow(r)" @keydown.enter="openRow(r)"
                  >
                    <td>
                      <div class="insc-person">
                        <div class="insc-initials">{{ initiales(r.prenom, r.nom) }}</div>
                        <div>
                          <div class="insc-name">{{ r.civilite ? `${r.civilite} ` : '' }}{{ r.nom }} <span class="first">{{ r.prenom }}</span></div>
                          <div class="insc-sub">{{ [r.origine, r.mandant].filter(Boolean).join(' · ') || '—' }}</div>
                        </div>
                      </div>
                    </td>
                    <td>
                      <span class="insc-pill">{{ r.kind === 'renouvellement' ? statutLabel(r) : 'Nouveau·elle' }}</span>
                      <div class="insc-sub">
                        {{ r.kind === 'renouvellement' ? `reconduit·e depuis S${previous?.number} ${previous?.year}` : `Candidature : ${r.statut || '—'}` }}
                      </div>
                      <!-- Date de la saisie du statut de la candidature (date du contact AS) -->
                      <div v-if="r.kind === 'candidat' && r.statutDepuis" class="insc-sub">depuis le {{ formatDate(r.statutDepuis) }}</div>
                    </td>
                    <td><span class="insc-sub" :class="{ 'insc-empty': !r.assistantSocialEmail }">{{ asDisplay(r.assistantSocialEmail) }}</span></td>
                    <td class="insc-strong">
                      <template v-if="r.kind === 'renouvellement'">{{ r.niveauFinContrat || '—' }}</template>
                      <span v-else class="insc-note">{{ r.evaluationNiveau }}</span>
                    </td>
                    <td colspan="2">
                      <span v-if="r.kind === 'renouvellement'" class="insc-sub">Orientation S{{ previous?.number }} : {{ r.orientationPreconisee || 'non saisie (renouvellement par défaut)' }}</span>
                      <span v-else-if="r.inscriptionConfirmee" class="insc-sub text-success font-weight-bold">
                        <v-icon size="x-small" color="success">mdi-check-circle</v-icon> Inscription confirmée
                      </span>
                    </td>
                    <td />
                    <td />
                  </tr>

                </tbody>
              </table>
            </div>

          </template>
        </v-card>
      </v-window-item>

      <!-- ═══════════ AS ═══════════ -->
      <v-window-item value="as">
        <AssistantsSociauxTab :assistants="assistants" :loading="assistantsLoading" @changed="loadAssistants" @notify="notify" />
      </v-window-item>
    </v-window>

    <!-- ─── Dialogue participant·e ─── -->
    <v-dialog v-model="participantDialog" max-width="900" scrollable persistent @keydown.esc="attemptCloseParticipant">
      <v-card v-if="editing" rounded="lg">
        <v-card-title class="py-4">
          {{ editing.nom }} {{ editing.prenom }}
          <span class="text-body-2 text-grey ml-2">{{ editing.classe }} · {{ editingSessionLabel }}</span>
        </v-card-title>
        <v-divider />
        <v-card-text class="pt-4">
          <v-alert v-if="editing.kind === 'renouvellement'" type="info" variant="tonal" density="compact" class="mb-4">
            Reconduit·e depuis la session précédente. Pour ne pas renouveler, choisissez une autre orientation ci-dessous (MIS RLS, sans solution…).
          </v-alert>
          <div class="text-subtitle-2 font-weight-bold mb-2 text-primary">Fiche participant</div>
          <v-row dense>
            <v-col cols="6" md="3"><v-text-field v-model="pform.origine" label="Pays d'origine" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3"><v-select v-model="pform.mandant" :items="MANDANTS" label="Mandant" variant="outlined" density="compact" clearable /></v-col>
            <v-col cols="12" md="6">
              <v-combobox
                v-model="pform.assistantSocialEmail" :items="asEmailItems" item-title="label" item-value="email"
                :return-object="false" label="Assistant·e social·e (email)" variant="outlined" density="compact" clearable
              />
            </v-col>
            <v-col cols="4" md="3"><v-switch v-model="pform.contratMaori" label="Contrat Maori" color="red" hide-details density="compact" /></v-col>
            <v-col cols="8" md="9" class="d-flex align-center text-body-2">
              <span>
                Niveau début : <b>{{ editing.niveauDebutContrat || '—' }}</b>
                <span class="mx-2">→</span> fin : <b>{{ editing.niveauFinContrat || '—' }}</b>
                <span class="text-caption text-medium-emphasis ml-2">(d'après Bilans &amp; attestations)</span>
              </span>
            </v-col>
          </v-row>

          <div class="text-subtitle-2 font-weight-bold mb-2 mt-2 text-primary">Inscription pour {{ editingSessionLabel }}</div>
          <v-row dense>
            <v-col cols="12" md="6">
              <v-combobox
                v-model="pform.statutBeneficiaire" :items="STATUTS_BENEFICIAIRE" label="Statut bénéficiaire"
                variant="outlined" density="compact" clearable :hint="statutHint" persistent-hint
              />
            </v-col>
            <v-col cols="6" md="3">
              <v-text-field
                v-model="pform.dateFinParticipation" type="date" variant="outlined" density="compact"
                :label="interruption ? 'Interrompu le *' : 'Interrompu le'"
                :rules="[v => !interruption || !!v || 'Date obligatoire pour une interruption']"
              />
            </v-col>
            <v-col cols="6" md="3">
              <v-text-field
                v-model="pform.raisonInterruptionMesure" variant="outlined" density="compact"
                :label="interruption ? 'Raison de l\'interruption *' : 'Raison de l\'interruption'"
                :rules="[v => !interruption || !!(v && v.trim()) || 'Raison obligatoire pour une interruption']"
              />
            </v-col>
            <v-col cols="12" md="4"><v-select v-model="pform.objectifsAtteints" :items="OBJECTIFS_ATTEINTS" label="Objectifs atteints ?" variant="outlined" density="compact" clearable /></v-col>
            <v-col cols="12" md="8"><v-textarea v-model="pform.objectifsCommentaire" label="Si partiellement / non, pourquoi ?" rows="1" auto-grow variant="outlined" density="compact" /></v-col>
            <v-col cols="12" md="4"><v-combobox v-model="pform.orientationPreconisee" :items="ORIENTATIONS" label="Orientation préconisée" variant="outlined" density="compact" clearable /></v-col>
            <v-col cols="12" md="8"><v-textarea v-model="pform.orientationPrecisions" label="Précisions (« autre » ou « sans solution »)" rows="1" auto-grow variant="outlined" density="compact" /></v-col>
            <v-col cols="12" md="6"><v-text-field v-model="pform.communicationAS" label="Communication AS (ex. contactée le 18.08)" variant="outlined" density="compact" /></v-col>
          </v-row>
          <v-alert v-if="participantError" type="error" variant="tonal" density="compact" class="mt-2">{{ participantError }}</v-alert>
        </v-card-text>
        <v-divider />
        <v-card-actions class="d-flex">
          <v-btn variant="text" prepend-icon="mdi-account-details" :to="{ name: 'Participant-Detail', params: { id: editing.studentId } }">Fiche participant</v-btn>
          <v-btn
            v-if="editing.kind === 'inscrit'" variant="text" color="error" prepend-icon="mdi-account-remove-outline"
            @click="desinscrireDialog = true"
          >
            Retirer de cette session
          </v-btn>
          <v-spacer />
          <v-btn variant="text" color="grey" @click="attemptCloseParticipant">Annuler</v-btn>
          <v-btn color="primary" :loading="participantSaving" :disabled="!interruptionValide" @click="saveParticipant">Enregistrer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ─── Inscription réelle dans la session ─── -->
    <v-dialog v-model="inscriptionDialog" max-width="760" scrollable>
      <v-card rounded="lg">
        <v-card-title class="py-4">Inscrire dans {{ selectedSession?.label }}</v-card-title>
        <v-divider />
        <v-card-text class="pt-4">
          <div class="text-body-2 mb-3">
            Les personnes cochées sont inscrites dans la session : elles apparaissent ensuite dans la fiche de présence,
            Accomp. &amp; Ateliers et Bilans &amp; attestations pour {{ selectedSession?.label }}.
          </div>
          <v-alert v-if="selectedSession?.planned" type="warning" variant="tonal" density="compact" class="mb-3">
            Cette session n'est pas encore dans le calendrier de formation : ajoutez-la dans « Calendrier de formation »
            pour que les jours de cours apparaissent dans la fiche de présence.
          </v-alert>
          <div v-for="it in inscriptionItems" :key="it.key" class="d-flex align-center ga-3 py-1 inscription-item">
            <v-checkbox-btn v-model="it.checked" :disabled="!it.studentId" density="compact" class="flex-grow-0" />
            <div class="flex-grow-1">
              <div class="font-weight-bold">{{ it.nom }} {{ it.prenom }}</div>
              <div class="text-caption text-medium-emphasis">
                {{ it.kind === 'renouvellement' ? statutLabel(it.row) : 'Nouveau·elle candidat·e' }}
                <span v-if="!it.studentId" class="text-error"> — pas encore de fiche participant : liez-la dans la candidature</span>
              </div>
            </div>
            <v-select
              v-model="it.classeId" :items="classeItems" item-title="title" item-value="value"
              label="Classe" variant="outlined" density="compact" hide-details style="max-width: 200px"
              :disabled="!it.studentId"
            />
          </div>
          <v-alert v-if="inscriptionError" type="error" variant="tonal" density="compact" class="mt-3">{{ inscriptionError }}</v-alert>
        </v-card-text>
        <v-divider />
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="inscriptionDialog = false">Annuler</v-btn>
          <v-btn color="primary" :loading="inscriptionSaving" :disabled="!inscriptionSelection.length" @click="confirmInscription">
            Inscrire ({{ inscriptionSelection.length }})
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="desinscrireDialog" max-width="440">
      <v-card rounded="lg">
        <v-card-title class="py-4">Retirer de {{ selectedSession?.label }} ?</v-card-title>
        <v-card-text>
          {{ editing?.nom }} {{ editing?.prenom }} ne fera plus partie de cette session (fiche de présence, Accomp., Bilans).
          Les présences déjà saisies sont conservées. Pour un arrêt en cours de session, utilisez plutôt une interruption.
        </v-card-text>
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="desinscrireDialog = false">Annuler</v-btn>
          <v-btn color="error" :loading="participantSaving" @click="confirmDesinscription">Retirer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <CandidatureDialog
      v-model="candidatureDialog" :candidature="editingCandidature" :defaults="candidatureDefaults"
      :assistants="assistants" :calendar-sessions="calendarSessions"
      @saved="onCandidatureSaved" @assistants-changed="loadAssistants"
    />

    <CandidatureImportDialog v-model="importDialog" :assistants="assistants" :defaults="importDefaults" @imported="onImported" />

    <v-snackbar v-model="snackbar" timeout="5000">{{ snackbarText }}</v-snackbar>
  </v-container>
</template>

<script setup>
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import { matchesSearch } from '@/utils/search'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import '@/scss/inscriptions.css'
import CandidaturesTab from '@/components/CandidaturesTab.vue'
import AssistantsSociauxTab from '@/components/AssistantsSociauxTab.vue'
import CandidatureDialog from '@/components/CandidatureDialog.vue'
import CandidatureImportDialog from '@/components/CandidatureImportDialog.vue'
import { useUnsavedGuard, confirmDiscard } from '@/composables/useUnsavedGuard'
import {
  STATUTS_BENEFICIAIRE, OBJECTIFS_ATTEINTS, ORIENTATIONS, MANDANTS, GROUPES, RAIL,
  asLabel, readApiError, sessionsDuGroupe, indexSessionCourante, isRenouvellement,
  categorieInscrit, statutDeduit, initiales,
} from '@/utils/inscriptionOptions'

// ─── Onglet principal : la rubrique s'ouvre toujours sur les candidat·e·s ─────
const tab = ref('candidatures')

const snackbar = ref(false)
const snackbarText = ref('')
const notify = (text) => { snackbarText.value = text; snackbar.value = true }

const formatDate = (d) => d ? moment(d).format('DD.MM.YYYY') : ''
const objectifsColor = (o) => ({ oui: 'success', partiellement: 'orange', non: 'error' }[o] || 'grey')

// ─── Assistant·e·s sociaux·ales (partagé entre les onglets) ───────────────────
const assistants = ref([])
const assistantsLoading = ref(false)

async function loadAssistants() {
  assistantsLoading.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/AssistantsSociaux`)
    if (res?.ok) assistants.value = await res.json()
  } finally {
    assistantsLoading.value = false
  }
}

const asByEmail = computed(() => Object.fromEntries(assistants.value.map(a => [a.email.toLowerCase(), a])))
const asDisplay = (email) => {
  if (!email) return '—'
  const a = asByEmail.value[email.toLowerCase()]
  return a && (a.prenom || a.nom) ? [a.prenom, a.nom].filter(Boolean).join(' ') : email
}
const asEmailItems = computed(() => assistants.value.filter(a => a.actif).map(a => ({ email: a.email, label: asLabel(a) })))

// ─── Calendrier, groupe et session affichée ───────────────────────────────────
const calendarSessions = ref([])
const groupe = ref(GROUPES[0].value)
const navIndex = ref(0)

const groupSessions = computed(() => sessionsDuGroupe(calendarSessions.value, groupe.value))
const selectedSession = computed(() => groupSessions.value[navIndex.value] || null)

async function loadCalendar() {
  const res = await apiFetch(`${API_BASE}/api/calendrier`)
  if (res?.ok) calendarSessions.value = (await res.json()).sessions || []
  navIndex.value = indexSessionCourante(groupSessions.value)
}

// Changer de groupe garde la même session (année + numéro) si elle existe dans ce groupe,
// comme sur les autres pages ; sinon la session en cours du groupe.
watch(groupe, (g, old) => {
  const key = sessionsDuGroupe(calendarSessions.value, old)[navIndex.value]?.key
  const idx = groupSessions.value.findIndex(s => s.key === key)
  navIndex.value = idx >= 0 ? idx : indexSessionCourante(groupSessions.value)
})

// ─── Données d'une session (tous groupes, réparties côté client) ──────────────
const sessionData = ref({})       // clé "année-numéro" → réponse de l'API
const sessionLoading = ref(false)
const search = ref('')

async function loadSession(force = false) {
  const s = selectedSession.value
  if (!s || (!force && sessionData.value[s.key])) return
  sessionLoading.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/Inscriptions/session?year=${s.year}&number=${s.number}`)
    if (res?.ok) sessionData.value = { ...sessionData.value, [s.key]: await res.json() }
  } finally {
    sessionLoading.value = false
  }
}
watch(() => selectedSession.value?.key, () => loadSession())

// Une candidature modifiée dans l'autre onglet peut changer n'importe quelle session.
function invalidateSessions() {
  sessionData.value = {}
  loadSession(true)
}

const current = computed(() => sessionData.value[selectedSession.value?.key] || null)
const previous = computed(() => current.value?.previous || null)

const rowsOf = (data, g) => {
  if (!data) return { inscrits: [], prevus: [] }
  const byName = (a, b) => (a.nom || '').localeCompare(b.nom || '', 'fr')
  return {
    inscrits: data.inscrits.filter(r => r.groupe === g).sort(byName)
      .map(r => ({ ...r, kind: 'inscrit', key: `i-${r.studentId}`, categorie: categorieInscrit(r) })),
    prevus: [
      ...data.renouvellements.filter(r => r.groupe === g).sort(byName)
        .map(r => ({ ...r, kind: 'renouvellement', key: `r-${r.studentId}`, categorie: 'renouvele' })),
      ...data.candidats.filter(c => c.groupe === g).sort(byName)
        .map(c => ({ ...c, kind: 'candidat', key: `c-${c.candidatureId}`, categorie: 'candidat' })),
    ],
  }
}

const groupRows = computed(() => rowsOf(current.value, groupe.value))
const allRows = computed(() => [...groupRows.value.inscrits, ...groupRows.value.prevus])

// Recherche tolérante (accents, espaces, petite faute) : chaque mot cherché doit se retrouver
// dans l'un des champs de la ligne.
const rowMatchesSearch = (r) => matchesSearch(search.value, r.nom, r.prenom, r.origine, r.mandant,
  r.assistantSocialEmail, asDisplay(r.assistantSocialEmail), r.statutBeneficiaire, r.statut, r.orientationPreconisee)
const visibleInscrits = computed(() => groupRows.value.inscrits.filter(rowMatchesSearch))
const visiblePrevus = computed(() => groupRows.value.prevus.filter(rowMatchesSearch))

// Statut saisi, sinon déduit de l'historique : déjà suivi N session(s) → "Renouvelé-e N".
const statutLabel = (r) => r.statutBeneficiaire
  || (r.categorie === 'interrompu' ? 'Interrompu' : statutDeduit(r))

// Total d'un groupe = inscrit·e·s actif·ve·s + prévu·e·s (renouvellements + candidat·e·s).
const groupTotal = (g) => {
  const { inscrits, prevus } = rowsOf(current.value, g)
  return inscrits.filter(r => r.categorie !== 'interrompu').length + prevus.length
}

const recap = computed(() => {
  const { inscrits, prevus } = groupRows.value
  const count = (cat) => inscrits.filter(r => r.categorie === cat).length
  const actifs = inscrits.filter(r => r.categorie !== 'interrompu')
  const parts = [
    { key: 'nouveau', label: 'nouveaux·elles', n: count('nouveau'), color: RAIL.nouveau },
    { key: 'renouvele', label: 'renouvelé·e·s', n: count('renouvele'), color: RAIL.renouvele },
    { key: 'autre', label: 'autre statut', n: count('autre'), color: RAIL.autre },
    { key: 'prevu', label: 'reconduit·e·s de la session précédente', n: prevus.filter(r => r.kind === 'renouvellement').length, color: RAIL.renouvele },
    { key: 'candidat', label: 'nouveaux·elles candidat·e·s', n: prevus.filter(r => r.kind === 'candidat').length, color: RAIL.candidat },
  ]
  const total = parts.reduce((n, p) => n + p.n, 0)
  const interrompus = count('interrompu')
  const nonRenouveles = actifs.filter(r => r.orientationPreconisee && !isRenouvellement(r.orientationPreconisee)).length
  const legend = parts.filter(p => p.n)
  if (interrompus) legend.push({ key: 'interrompu', label: 'interrompu·e·s (hors total)', n: interrompus, color: RAIL.interrompu })
  if (nonRenouveles) legend.push({ key: 'nonrenouv', label: 'non renouvelé·e·s après cette session', n: nonRenouveles, color: 'transparent' })
  return {
    total,
    prevus: prevus.length,
    segments: parts.filter(p => p.n).map(p => ({ ...p, pct: (p.n / Math.max(total, 1)) * 100 })),
    legend,
  }
})
const recapAria = computed(() => recap.value.legend.map(s => `${s.n} ${s.label}`).join(', '))

// ─── Ouverture d'une ligne ────────────────────────────────────────────────────
const participantDialog = ref(false)
const participantSaving = ref(false)
const participantError = ref('')
const editing = ref(null)
const pform = ref({})

const candidatureDialog = ref(false)
const editingCandidature = ref(null)
const candidatureDefaults = ref({})
const candidaturesTab = ref(null)

const editingSessionLabel = computed(() => editing.value?.kind === 'renouvellement' && previous.value
  ? `Session ${previous.value.number} — ${previous.value.year} (précédente)`
  : selectedSession.value?.label)

const statutHint = computed(() => {
  const n = editing.value?.sessionsPrecedentes || 0
  return n ? `${n} session(s) suivie(s) avant celle-ci — suggestion : Renouvelé-e ${n}` : 'Aucune session précédente — suggestion : Nouveau-elle'
})

function openRow(r) {
  if (r.kind === 'candidat') {
    editingCandidature.value = { candidatureId: r.candidatureId }
    candidatureDialog.value = true
    return
  }
  editing.value = r
  participantError.value = ''
  pform.value = { ...r, dateFinParticipation: r.dateFinParticipation ? moment(r.dateFinParticipation).format('YYYY-MM-DD') : null }
  pformSnapshot.value = ''
  participantDialog.value = true
  // Après les watchers d'ouverture (ex. statut "Interrompu" déduit de la date de fin).
  nextTick(() => { pformSnapshot.value = JSON.stringify(pform.value) })
}

// Modifications non enregistrées de la fiche ouverte : confirmation avant de fermer ou de
// quitter la page (sinon un geste "retour" faisait perdre la saisie).
const pformSnapshot = ref('')
const isParticipantDirty = () => participantDialog.value && !!pformSnapshot.value && JSON.stringify(pform.value) !== pformSnapshot.value
useUnsavedGuard(isParticipantDirty)
function attemptCloseParticipant() {
  if (!isParticipantDirty() || confirmDiscard()) closeParticipantDialog()
}
function closeParticipantDialog() {
  pformSnapshot.value = ''
  participantDialog.value = false
}

async function toggleContratMaori(r, value) {
  const previous = r.contratMaori
  r.contratMaori = value
  const res = await apiFetch(`${API_BASE}/api/Inscriptions/participants/${r.studentId}/contrat-maori`, {
    method: 'PATCH',
    body: JSON.stringify({ contratMaori: value }),
  })
  if (!res?.ok) {
    r.contratMaori = previous
    notify("Le contrat Maori n'a pas pu être enregistré.")
  }
}

function newCandidateForSession() {
  const s = selectedSession.value
  editingCandidature.value = null
  candidatureDefaults.value = {
    mesureVisee: GROUPES.find(g => g.value === groupe.value)?.mesure,
    // F&I A1 / A2 : le niveau est porté par le statut de liste d'attente.
    ...(GROUPES.find(g => g.value === groupe.value)?.statutAttente
      ? { statut: GROUPES.find(g => g.value === groupe.value).statutAttente } : {}),
    anneeVisee: s?.year,
    sessionNumberVisee: s?.number,
    sessionsVisees: s ? [`${s.year}-${s.number}`] : [],
  }
  candidatureDialog.value = true
}

// Import CSV depuis une session : mesure, statut de liste d'attente et session pré-remplis.
const importDialog = ref(false)
const importDefaults = ref(null)
function importForSession() {
  const s = selectedSession.value
  const g = GROUPES.find(x => x.value === groupe.value)
  importDefaults.value = {
    mesure: g?.mesure,
    statut: g?.statutAttente || (g?.mesure === 'Télécours' ? 'En attente A1 Télécours' : undefined),
    annee: s?.year,
    session: s?.number,
  }
  importDialog.value = true
}
function onImported() {
  loadAssistants()
  onCandidatureSaved()
}

function onCandidatureSaved() {
  invalidateSessions()
  candidaturesTab.value?.load()
}

// Interruption : dès que le statut est "Interrompu" ou qu'une date est saisie, date + raison obligatoires.
const interruption = computed(() => pform.value.statutBeneficiaire === 'Interrompu' || !!pform.value.dateFinParticipation)
const interruptionValide = computed(() => !interruption.value
  || (!!pform.value.dateFinParticipation && !!pform.value.raisonInterruptionMesure?.trim()))
watch(() => pform.value.dateFinParticipation, (d) => { if (d) pform.value.statutBeneficiaire = 'Interrompu' })

async function saveParticipant() {
  participantSaving.value = true
  participantError.value = ''
  try {
    const { sessionId, studentId } = editing.value
    const res = await apiFetch(`${API_BASE}/api/Inscriptions/participants/${sessionId}/${studentId}`, {
      method: 'PUT',
      body: JSON.stringify({ ...pform.value, dateFinParticipation: pform.value.dateFinParticipation || null }),
    })
    if (!res) return
    if (!res.ok) {
      participantError.value = await readApiError(res)
      return
    }
    closeParticipantDialog()
    invalidateSessions()
  } finally {
    participantSaving.value = false
  }
}

// ─── Inscription réelle dans la session ───────────────────────────────────────
const classes = ref([])
const classeItems = computed(() => classes.value.map(c => ({ title: c.schoolClassRoomName, value: c.schoolClassRoomId })))
const classeIdParNom = (nom) => classes.value.find(c => c.schoolClassRoomName === nom)?.schoolClassRoomId ?? null

async function loadClasses() {
  const res = await apiFetch(`${API_BASE}/api/SchoolClassRooms`)
  if (res?.ok) classes.value = (await res.json()).filter(c => !c.isArchived)
}

// Classe proposée : celle de la session précédente (ou F&I A2 en cas de passage en A2) pour
// un renouvellement ; d'après la mesure visée pour un·e candidat·e.
const classeParDefaut = (r) => {
  if (r.kind === 'renouvellement') {
    return classeIdParNom(r.orientationPreconisee === 'Passage en A2' ? 'F&I A2' : r.classe)
  }
  return classeIdParNom({ fi_a1: 'F&I A1', fi_a2: 'F&I A2', telecours: 'Télécours A1' }[r.groupe])
}

const inscriptionDialog = ref(false)
const inscriptionSaving = ref(false)
const inscriptionError = ref('')
const inscriptionItems = ref([])
const inscriptionSelection = computed(() => inscriptionItems.value.filter(it => it.checked && it.studentId && it.classeId))

async function openInscription() {
  if (!classes.value.length) await loadClasses()
  inscriptionError.value = ''
  inscriptionItems.value = groupRows.value.prevus.map(r => {
    const studentId = r.kind === 'renouvellement' ? r.studentId : r.participantId
    return {
      key: r.key, kind: r.kind, row: r, nom: r.nom, prenom: r.prenom, studentId,
      candidatureId: r.kind === 'candidat' ? r.candidatureId : null,
      classeId: classeParDefaut(r),
      checked: !!studentId,
    }
  })
  inscriptionDialog.value = true
}

async function confirmInscription() {
  inscriptionSaving.value = true
  inscriptionError.value = ''
  try {
    const s = selectedSession.value
    const res = await apiFetch(`${API_BASE}/api/Inscriptions/inscrire`, {
      method: 'POST',
      body: JSON.stringify({
        year: s.year,
        number: s.number,
        items: inscriptionSelection.value.map(it => ({
          studentId: it.studentId, schoolClassRoomId: it.classeId, candidatureId: it.candidatureId,
        })),
      }),
    })
    if (!res) return
    if (!res.ok) { inscriptionError.value = await readApiError(res); return }
    const { inscrits } = await res.json()
    inscriptionDialog.value = false
    notify(`${inscrits} personne(s) inscrite(s) dans ${s.label}.`)
    invalidateSessions()
    candidaturesTab.value?.load()
  } finally {
    inscriptionSaving.value = false
  }
}

const desinscrireDialog = ref(false)
async function confirmDesinscription() {
  participantSaving.value = true
  try {
    const { sessionId, studentId } = editing.value
    const res = await apiFetch(`${API_BASE}/api/Inscriptions/participants/${sessionId}/${studentId}`, { method: 'DELETE' })
    if (!res?.ok) { participantError.value = res ? await readApiError(res) : 'Erreur.'; return }
    desinscrireDialog.value = false
    closeParticipantDialog()
    notify(`${editing.value.nom} ${editing.value.prenom} retiré·e de ${selectedSession.value?.label}.`)
    invalidateSessions()
  } finally {
    participantSaving.value = false
  }
}

onMounted(async () => {
  loadAssistants()
  await loadCalendar()
  loadSession()
})
</script>

<style scoped>
.inscription-item + .inscription-item { border-top: 1px solid #eef2f7; }
</style>
