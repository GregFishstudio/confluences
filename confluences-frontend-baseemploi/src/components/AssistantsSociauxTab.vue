<!--
  Onglet "Assistant·e·s sociaux·ales" de la page Inscriptions : carnet d'adresses des AS
  (ajout / modification / suppression + import des emails déjà saisis sur les fiches participant).
-->
<template>
  <div>
    <v-card class="elevation-4" rounded="lg" :loading="loading">
      <v-row align="center" class="px-2">
        <v-col cols="12" md="4" class="pa-3 pa-md-6">
          <v-text-field
            v-model="search" append-icon="mdi-magnify" label="Rechercher (nom, email, service...)"
            variant="outlined" density="compact" clearable hide-details bg-color="white"
          />
        </v-col>
        <v-col cols="12" md="8" class="pa-3 pa-md-6 d-flex justify-end align-center ga-2 flex-wrap">
          <v-switch v-model="showInactive" label="Afficher les inactif·ve·s" color="primary" hide-details density="compact" class="flex-grow-0 mr-2" />
          <v-btn
            color="primary" variant="outlined" size="small" prepend-icon="mdi-database-import-outline" :loading="importing"
            title="Crée une fiche pour chaque email d'AS saisi sur les fiches participant et absent du carnet"
            @click="importFromParticipants"
          >
            Importer depuis les fiches participant
          </v-btn>
          <v-btn color="primary" variant="outlined" size="small" prepend-icon="mdi-plus" @click="openDialog(null)">Nouvel·le AS</v-btn>
        </v-col>
      </v-row>
      <v-divider />
      <div v-if="!loading && !filtered.length" class="insc-empty-state">
        <v-icon size="40" color="grey-lighten-1">mdi-card-account-phone-outline</v-icon>
        <div class="mt-2">Aucun·e AS dans le carnet.</div>
        <div class="text-caption">Importez les emails déjà saisis sur les fiches participant, ou ajoutez une fiche.</div>
      </div>
      <div v-else class="insc-scroll">
        <table class="insc-table">
          <thead>
            <tr>
              <th>Assistant·e social·e</th>
              <th>Contact</th>
              <th>Suivi·e·s</th>
              <th>Remarques</th>
              <th />
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="a in filtered" :key="a.assistantSocialId" class="clickable" tabindex="0"
              :class="{ 'is-muted': !a.actif }" :style="{ '--rail': a.actif ? RAIL.renouvele : RAIL.interrompu }"
              @click="openDialog(a)" @keydown.enter="openDialog(a)"
            >
              <td>
                <div class="insc-person">
                  <div class="insc-initials">{{ initiales(a.prenom, a.nom || a.email) }}</div>
                  <div>
                    <div class="insc-name">{{ a.nom || '—' }} <span class="first">{{ a.prenom }}</span></div>
                    <div class="insc-sub">{{ a.organisme || 'Service non renseigné' }}<span v-if="!a.actif"> · inactif·ve</span></div>
                  </div>
                </div>
              </td>
              <td>
                <a :href="`mailto:${a.email}`" class="insc-sub" @click.stop>{{ a.email }}</a>
                <div v-if="a.telephone" class="insc-sub">{{ a.telephone }}</div>
              </td>
              <td>
                <div class="insc-sub"><b class="insc-strong">{{ a.nbParticipants }}</b> participant·e·s</div>
                <div class="insc-sub"><b class="insc-strong">{{ a.nbCandidatures }}</b> candidature·s</div>
              </td>
              <td><div class="insc-note" :class="{ 'insc-empty': !a.remarques }">{{ a.remarques || '—' }}</div></td>
              <td class="text-right">
                <v-btn icon="mdi-delete-outline" size="small" variant="text" color="error" :aria-label="`Supprimer ${a.email}`" @click.stop="askDelete(a)" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </v-card>

    <AssistantSocialDialog v-model="dialog" :assistant="editing" @saved="emit('changed')" />

    <v-dialog v-model="deleteDialog" max-width="440">
      <v-card rounded="lg">
        <v-card-title class="py-4">Supprimer cette fiche AS ?</v-card-title>
        <v-card-text>
          {{ asLabel(toDelete) }}
          <div v-if="toDelete?.nbCandidatures" class="text-caption mt-2">
            {{ toDelete.nbCandidatures }} candidature(s) liée(s) perdront le lien vers cette AS.
          </div>
          <div class="text-caption mt-1">Les fiches participant gardent l'email tel quel.</div>
        </v-card-text>
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="deleteDialog = false">Annuler</v-btn>
          <v-btn color="error" @click="confirmDelete">Supprimer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { matchesSearch } from '@/utils/search'
import API_BASE, { apiFetch } from '@/services/api'
import AssistantSocialDialog from '@/components/AssistantSocialDialog.vue'
import '@/scss/inscriptions.css'
import { RAIL, asLabel, initiales } from '@/utils/inscriptionOptions'

const props = defineProps({
  assistants: { type: Array, required: true },
  loading: { type: Boolean, default: false },
})
const emit = defineEmits(['changed', 'notify'])

const search = ref('')
const showInactive = ref(false)
const dialog = ref(false)
const editing = ref(null)
const deleteDialog = ref(false)
const toDelete = ref(null)
const importing = ref(false)


const filtered = computed(() => {
  let list = props.assistants
  if (!showInactive.value) list = list.filter(a => a.actif)
  if (search.value) {
    list = list.filter(a => matchesSearch(search.value, a.prenom, a.nom, a.email, a.organisme, a.telephone))
  }
  return list
})

function openDialog(item) {
  editing.value = item
  dialog.value = true
}

function askDelete(item) {
  toDelete.value = item
  deleteDialog.value = true
}

async function confirmDelete() {
  await apiFetch(`${API_BASE}/api/AssistantsSociaux/${toDelete.value.assistantSocialId}`, { method: 'DELETE' })
  deleteDialog.value = false
  emit('changed')
}

async function importFromParticipants() {
  importing.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/AssistantsSociaux/import-participants`, { method: 'POST' })
    if (!res?.ok) return
    const { imported } = await res.json()
    emit('notify', imported ? `${imported} AS importé·e·s — vérifiez les noms devinés depuis l'email.` : 'Aucun nouvel email à importer.')
    emit('changed')
  } finally {
    importing.value = false
  }
}
</script>
