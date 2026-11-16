<!--
  Tableau de marrainage : vue globale par région des binômes en cours, des participant·e·s qui
  attendent un parrain/une marraine et des parrains/marraines disponibles — remplace les tableaux
  Word « Marrainage en cours / à trouver / en suspens ».
  Région d'un·e participant·e : saisie sur sa fiche, sinon déduite de son adresse ; d'une marraine :
  saisie sur sa fiche, sinon déduite de sa commune. Un binôme est rangé dans la région du/de la filleul·e.
-->
<template>
  <v-container fluid class="pa-4 pa-md-6">

    <!-- ════════ En-tête ════════ -->
    <div class="d-flex align-center flex-wrap ga-2 mb-4">
      <div>
        <div class="text-h5 font-weight-bold text-primary">Tableau de marrainage</div>
        <div class="text-caption text-grey">Vue d'ensemble par région : binômes, filleul·e·s à trouver, parrains et marraines disponibles</div>
      </div>
      <v-spacer />
      <v-btn variant="text" prepend-icon="mdi-refresh" :loading="loading" @click="load">Actualiser</v-btn>
      <v-btn variant="tonal" prepend-icon="mdi-map-marker-radius-outline" @click="regionsDialog = true">Régions</v-btn>
      <v-btn color="primary" prepend-icon="mdi-account-plus-outline" :to="{ name: 'Marrainage-Nouveau' }">Nouvelle marraine</v-btn>
    </div>

    <!-- ════════ Chiffres clés ════════ -->
    <div class="kpis mb-4">
      <div v-for="k in kpis" :key="k.key" class="kpi" :class="{ 'kpi--active': focus === k.key }" @click="toggleFocus(k.key)">
        <v-icon :color="k.color" size="28" class="mr-3">{{ k.icon }}</v-icon>
        <div class="flex-grow-1">
          <div class="kpi__value">{{ k.value }}</div>
          <div class="kpi__label">{{ k.label }}</div>
          <div v-if="k.sub" class="kpi__sub" :class="`text-${k.subColor || 'grey'}`">{{ k.sub }}</div>
        </div>
      </div>
    </div>

    <!-- ════════ Filtres ════════ -->
    <v-card class="mb-4 elevation-1" rounded="lg">
      <v-card-text class="py-3">
        <div class="d-flex flex-wrap align-center ga-3">
          <v-text-field
            v-model="search" placeholder="Rechercher un nom, une commune…" prepend-inner-icon="mdi-magnify"
            variant="outlined" density="compact" hide-details clearable style="max-width: 260px; min-width: 200px"
          />
          <v-select
            v-model="fRegions" :items="regionOptions" label="Région" multiple chips closable-chips
            variant="outlined" density="compact" hide-details clearable style="min-width: 180px; max-width: 320px"
          />
          <div class="filter-group">
            <span class="filter-label">Sexe</span>
            <v-btn-toggle v-model="fSexe" multiple density="compact" variant="outlined" divided color="primary">
              <v-btn v-for="s in SEXES" :key="s.value" :value="s.value" size="small">{{ s.short }}</v-btn>
            </v-btn-toggle>
          </div>
          <div class="filter-group">
            <span class="filter-label">Âge</span>
            <v-btn-toggle v-model="fAge" multiple density="compact" variant="outlined" divided color="primary">
              <v-btn v-for="a in AGES" :key="a.value" :value="a.value" size="small">{{ a.label }}</v-btn>
            </v-btn-toggle>
          </div>
          <div class="filter-group">
            <span class="filter-label">Mesure</span>
            <v-btn-toggle v-model="fMesure" multiple density="compact" variant="outlined" divided color="primary">
              <v-btn v-for="m in MESURES" :key="m.value" :value="m.value" size="small">{{ m.label }}</v-btn>
            </v-btn-toggle>
          </div>
          <div class="filter-group">
            <span class="filter-label">Priorité</span>
            <v-btn-toggle v-model="fPriorite" multiple density="compact" variant="outlined" divided color="primary">
              <v-btn v-for="p in PRIORITES" :key="p.value" :value="p.value" size="small">
                <v-icon start size="small" :color="p.color">{{ p.icon }}</v-icon>{{ p.short }}
              </v-btn>
            </v-btn-toggle>
          </div>
          <v-spacer />
          <v-btn v-if="hasFilters" variant="text" size="small" prepend-icon="mdi-filter-remove-outline" @click="resetFilters">
            Effacer les filtres
          </v-btn>
        </div>
        <div class="text-caption text-grey mt-2">
          Sexe et âge s'appliquent aux filleul·e·s comme aux parrains/marraines ; mesure et priorité aux filleul·e·s.
        </div>
      </v-card-text>
    </v-card>

    <!-- ════════ Vues ════════ -->
    <v-tabs v-model="tab" color="primary" class="mb-4">
      <v-tab value="regions" prepend-icon="mdi-map-outline">Par région</v-tab>
      <v-tab value="liste" prepend-icon="mdi-format-list-bulleted">Toutes les personnes</v-tab>
      <v-tab value="binomes" prepend-icon="mdi-account-heart-outline">
        Binômes
        <v-chip size="x-small" class="ml-2" variant="tonal">{{ fBinomes.length }}</v-chip>
      </v-tab>
    </v-tabs>

    <div v-if="loading && !loaded" class="text-center pa-12"><v-progress-circular indeterminate color="primary" /></div>

    <v-window v-else v-model="tab">

      <!-- ──────── PAR RÉGION ──────── -->
      <v-window-item value="regions">
        <div class="text-caption text-grey mb-3 d-flex flex-wrap ga-4 align-center">
          <span><span class="dot" style="background:#2e7d32" />En cours</span>
          <span><span class="dot" style="background:#ef6c00" />À trouver</span>
          <span><span class="dot" style="background:#1565c0" />Disponibles</span>
          <span>« Manque » = filleul·e·s à trouver − parrains/marraines disponibles dans la région.</span>
        </div>

        <div class="region-grid">
          <v-card
            v-for="r in regionCards" :key="r.nom" class="region-card elevation-1" rounded="lg"
            :class="{ 'region-card--empty': !r.total }"
          >
            <div class="region-card__head" :style="{ borderTopColor: r.couleur }">
              <div class="d-flex align-center">
                <span class="region-card__name">{{ r.nom }}</span>
                <v-spacer />
                <v-chip v-if="r.manque > 0" size="small" color="error" variant="flat" class="font-weight-bold">
                  Manque {{ r.manque }}
                </v-chip>
                <v-chip v-else-if="r.aTrouver.length" size="small" color="success" variant="tonal">Couvert</v-chip>
              </div>
              <div class="region-card__counts">
                <span><b>{{ r.enCours.length }}</b> en cours</span>
                <span><b>{{ r.aTrouver.length }}</b> à trouver</span>
                <span><b>{{ r.dispo.length }}</b> dispo.</span>
              </div>
            </div>

            <div class="region-card__body">
              <!-- En cours -->
              <div class="section">
                <div class="section__title text-green-darken-3">
                  <v-icon size="16" class="mr-1">mdi-account-heart-outline</v-icon>En cours
                </div>
                <div v-if="!r.enCours.length" class="section__empty">Aucun binôme</div>
                <div v-for="b in r.enCours" :key="b.key" class="person person--binome" @click="openRelation(b)">
                  <div class="person__main">
                    <span class="person__name">{{ b.participant.firstname }} {{ b.participant.lastName }}</span>
                    <v-icon size="14" class="mx-1" color="grey">mdi-swap-horizontal</v-icon>
                    <span class="person__name">{{ b.marraine.prenom }} {{ b.marraine.nom }}</span>
                  </div>
                  <div class="person__meta">
                    <v-chip v-if="b.participant.mesure" size="x-small" label variant="tonal" :color="b.participant.mesure.color">{{ b.participant.mesure.label }}</v-chip>
                    <span v-if="b.dateDebut">depuis {{ fmtMois(b.dateDebut) }}</span>
                    <span v-if="b.commentaire" class="person__comment" :title="b.commentaire">
                      <v-icon size="12">mdi-comment-text-outline</v-icon> {{ b.commentaire }}
                    </span>
                  </div>
                </div>
              </div>

              <!-- À trouver -->
              <div class="section">
                <div class="section__title text-orange-darken-3">
                  <v-icon size="16" class="mr-1">mdi-account-search-outline</v-icon>À trouver
                </div>
                <div v-if="!r.aTrouver.length" class="section__empty">Personne en attente</div>
                <div v-for="p in r.aTrouver" :key="p.id" class="person" @click="goParticipant(p.id)">
                  <div class="person__main">
                    <v-icon v-if="p.prio" size="16" :color="p.prio.color" class="mr-1" :title="p.prio.label">{{ p.prio.icon }}</v-icon>
                    <span class="person__name">{{ p.firstname }} {{ p.lastName }}</span>
                    <span class="person__sub">{{ p.commune }}</span>
                  </div>
                  <div class="person__meta">
                    <v-chip v-if="p.mesure" size="x-small" label variant="tonal" :color="p.mesure.color">{{ p.mesure.label }}</v-chip>
                    <span v-if="identite(p)">{{ identite(p) }}</span>
                    <v-chip v-if="p.besoin === 'a_verifier'" size="x-small" color="orange" variant="tonal" label>À vérifier</v-chip>
                    <span v-if="p.souhaitGenreLabel" class="text-blue-grey">{{ p.souhaitGenreLabel }}</span>
                  </div>
                </div>
              </div>

              <!-- Disponibles -->
              <div class="section">
                <div class="section__title text-blue-darken-3">
                  <v-icon size="16" class="mr-1">mdi-account-clock-outline</v-icon>Parrains / marraines disponibles
                </div>
                <div v-if="!r.dispo.length" class="section__empty">Aucun·e disponible</div>
                <div v-for="m in r.dispo" :key="m.id" class="person" @click="goMarraine(m.id)">
                  <div class="person__main">
                    <span class="person__name">{{ m.prenom }} {{ m.nom }}</span>
                    <span class="person__sub">{{ m.commune }}</span>
                  </div>
                  <div class="person__meta">
                    <v-chip size="x-small" label variant="tonal" :color="m.statutInfo.color">{{ m.statutInfo.title }}</v-chip>
                    <span v-if="identite(m)">{{ identite(m) }}</span>
                  </div>
                </div>
              </div>

              <!-- Pistes de recherche -->
              <div v-if="r.region" class="pistes" @click="editPistes(r.region)">
                <v-icon size="14" class="mr-1">mdi-lightbulb-on-outline</v-icon>
                <span v-if="r.region.pistesRecherche" class="pistes__text">{{ r.region.pistesRecherche }}</span>
                <span v-else class="text-grey font-italic">Où chercher dans cette région ? Ajouter des pistes…</span>
              </div>
            </div>
          </v-card>
        </div>
      </v-window-item>

      <!-- ──────── TOUTES LES PERSONNES ──────── -->
      <v-window-item value="liste">
        <v-card class="elevation-1" rounded="lg">
          <div class="d-flex align-center flex-wrap ga-2 pa-3">
            <v-btn-toggle v-model="listeRole" mandatory density="compact" variant="outlined" divided color="primary">
              <v-btn value="tous" size="small">Tout le monde</v-btn>
              <v-btn value="filleul" size="small">Filleul·e·s</v-btn>
              <v-btn value="marraine" size="small">Parrains / marraines</v-btn>
            </v-btn-toggle>
            <v-spacer />
            <span class="text-caption text-grey">{{ personnes.length }} personne(s) — cliquer un en-tête pour trier</span>
          </div>
          <v-data-table
            :headers="personHeaders" :items="personnes" :items-per-page="50" density="comfortable" hover
            :sort-by="[{ key: 'regionOrdre', order: 'asc' }]" class="person-table"
            @click:row="(_, { item }) => item.role === 'filleul' ? goParticipant(item.id) : goMarraine(item.id)"
          >
            <template #item.nom="{ item }">
              <div class="font-weight-medium">{{ item.nomComplet }}</div>
              <div class="text-caption text-grey">{{ item.commune || '—' }}</div>
            </template>
            <template #item.role="{ item }">
              <v-chip size="x-small" label variant="tonal" :color="item.role === 'filleul' ? 'deep-orange' : 'indigo'">
                {{ item.role === 'filleul' ? 'Filleul·e' : 'Parrain / marraine' }}
              </v-chip>
            </template>
            <template #item.regionOrdre="{ item }">
              <span v-if="item.region" class="region-pill" :style="{ background: regionColor(item.region) }">{{ item.region }}</span>
              <span v-else class="text-caption text-error">Non renseignée</span>
            </template>
            <template #item.sexe="{ item }">{{ sexeShort(item.sexe) }}</template>
            <template #item.age="{ item }">{{ item.age ?? '—' }}</template>
            <template #item.mesureLabel="{ item }">
              <v-chip v-if="item.mesure" size="x-small" label variant="tonal" :color="item.mesure.color">{{ item.mesure.label }}</v-chip>
              <span v-else class="text-grey">—</span>
            </template>
            <template #item.situationOrdre="{ item }">
              <v-chip size="x-small" variant="flat" :color="item.situation.color" class="font-weight-bold">{{ item.situation.label }}</v-chip>
              <div v-if="item.situation.detail" class="text-caption text-grey mt-1">{{ item.situation.detail }}</div>
            </template>
            <template #item.prioOrdre="{ item }">
              <span v-if="item.prio" class="d-inline-flex align-center">
                <v-icon size="16" :color="item.prio.color" class="mr-1">{{ item.prio.icon }}</v-icon>{{ item.prio.short }}
              </span>
              <span v-else class="text-grey">—</span>
            </template>
            <template #item.souhait="{ item }">
              <span class="text-caption">{{ item.souhaitLabel || '—' }}</span>
            </template>
          </v-data-table>
        </v-card>
      </v-window-item>

      <!-- ──────── BINÔMES ──────── -->
      <v-window-item value="binomes">
        <v-card class="elevation-1" rounded="lg">
          <div class="d-flex align-center flex-wrap ga-2 pa-3">
            <v-btn-toggle v-model="binomeEtat" mandatory density="compact" variant="outlined" divided color="primary">
              <v-btn value="tous" size="small">Tous</v-btn>
              <v-btn value="en_cours" size="small">En cours ({{ fBinomes.filter(b => b.actif).length }})</v-btn>
              <v-btn value="termines" size="small">Terminés / interrompus ({{ fBinomes.filter(b => !b.actif).length }})</v-btn>
            </v-btn-toggle>
            <v-spacer />
            <span class="text-caption text-grey">Cliquer un binôme pour noter son suivi, l'interrompre ou le clore.</span>
          </div>
          <v-data-table
            :headers="binomeHeaders" :items="binomesAffiches" :items-per-page="50" density="comfortable" hover
            :sort-by="[{ key: 'dateDebut', order: 'desc' }]"
            @click:row="(_, { item }) => openRelation(item)"
          >
            <template #item.filleul="{ item }">
              <a href="#" class="link font-weight-medium" @click.stop.prevent="goParticipant(item.participant.id)">{{ item.participant.firstname }} {{ item.participant.lastName }}</a>
              <div><v-chip v-if="item.participant.mesure" size="x-small" label variant="tonal" :color="item.participant.mesure.color">{{ item.participant.mesure.label }}</v-chip></div>
            </template>
            <template #item.marraineNom="{ item }">
              <a href="#" class="link font-weight-medium" @click.stop.prevent="goMarraine(item.marraine.id)">{{ item.marraine.prenom }} {{ item.marraine.nom }}</a>
            </template>
            <template #item.region="{ item }">
              <span v-if="item.region" class="region-pill" :style="{ background: regionColor(item.region) }">{{ item.region }}</span>
              <span v-else class="text-caption text-grey">—</span>
            </template>
            <template #item.dateDebut="{ item }">{{ fmtDate(item.dateDebut) }}</template>
            <template #item.etat="{ item }">
              <v-chip size="x-small" variant="flat" :color="item.actif ? 'success' : (item.raisonInactivite ? 'deep-orange' : 'grey')" class="font-weight-bold">
                {{ item.actif ? 'En cours' : (item.raisonInactivite ? 'Interrompu' : 'Terminé') }}
              </v-chip>
              <div v-if="!item.actif" class="text-caption text-grey mt-1">
                {{ item.dateFin ? `le ${fmtDate(item.dateFin)}` : '' }}<span v-if="item.raisonInactivite"> — {{ item.raisonInactivite }}</span>
              </div>
            </template>
            <template #item.commentaire="{ item }">
              <span class="text-caption comment-cell">{{ item.commentaire || '—' }}</span>
            </template>
          </v-data-table>
        </v-card>
      </v-window-item>
    </v-window>

    <!-- ════════ Dialogue : suivi d'un binôme ════════ -->
    <v-dialog v-model="relDialog.open" max-width="560">
      <v-card v-if="relDialog.binome" rounded="lg">
        <v-card-title class="pa-4">
          <div class="text-subtitle-1 font-weight-bold">
            {{ relDialog.binome.participant.firstname }} {{ relDialog.binome.participant.lastName }}
            <v-icon size="18" class="mx-1">mdi-swap-horizontal</v-icon>
            {{ relDialog.binome.marraine.prenom }} {{ relDialog.binome.marraine.nom }}
          </div>
          <div class="text-caption text-grey">Suivi du binôme</div>
        </v-card-title>
        <v-card-text>
          <v-alert v-if="relDialog.binome.legacy" type="info" variant="tonal" density="compact" class="mb-3">
            Ancien lien sans dates : ouvrez la fiche de la marraine pour le convertir en relation suivie.
          </v-alert>
          <template v-else>
            <v-btn-toggle v-model="relForm.etat" mandatory density="compact" variant="outlined" divided color="primary" class="mb-4">
              <v-btn value="en_cours" prepend-icon="mdi-play-circle-outline">En cours</v-btn>
              <v-btn value="termine" prepend-icon="mdi-check-circle-outline">Terminé</v-btn>
              <v-btn value="interrompu" prepend-icon="mdi-stop-circle-outline">Interrompu</v-btn>
            </v-btn-toggle>
            <v-row dense>
              <v-col cols="6">
                <v-text-field v-model="relForm.dateDebut" type="date" label="Début" variant="outlined" density="compact" />
              </v-col>
              <v-col cols="6">
                <v-text-field v-if="relForm.etat !== 'en_cours'" v-model="relForm.dateFin" type="date" label="Fin" variant="outlined" density="compact" />
              </v-col>
              <v-col v-if="relForm.etat === 'interrompu'" cols="12">
                <v-combobox
                  v-model="relForm.raison" :items="raisons" label="Pourquoi ?" variant="outlined" density="compact"
                  hint="Choisir une raison ou en saisir une nouvelle" persistent-hint
                />
              </v-col>
              <v-col cols="12">
                <v-textarea
                  v-model="relForm.commentaire" label="Suivi / comment ça se passe" variant="outlined" rows="3" auto-grow
                  placeholder="Ex. absence de la marraine cet été, se revoient fin septembre…"
                />
              </v-col>
            </v-row>
          </template>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-btn variant="text" prepend-icon="mdi-open-in-new" @click="goMarraine(relDialog.binome.marraine.id)">Fiche marraine</v-btn>
          <v-spacer />
          <v-btn variant="text" @click="relDialog.open = false">Fermer</v-btn>
          <v-btn
            v-if="!relDialog.binome.legacy" color="primary" variant="flat" :loading="relDialog.saving"
            :disabled="relForm.etat === 'interrompu' && !relForm.raison"
            @click="saveRelation"
          >Enregistrer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Dialogue : pistes de recherche ════════ -->
    <v-dialog v-model="pistesDialog.open" max-width="520">
      <v-card rounded="lg">
        <v-card-title class="pa-4">Où chercher — {{ pistesDialog.region?.nom }}</v-card-title>
        <v-card-text>
          <v-textarea
            v-model="pistesDialog.text" variant="outlined" rows="5" auto-grow autofocus
            placeholder="Associations, paroisses, communes, annonces, contacts à relancer…"
          />
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="pistesDialog.open = false">Annuler</v-btn>
          <v-btn color="primary" variant="flat" :loading="pistesDialog.saving" @click="savePistes">Enregistrer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Dialogue : gestion des régions ════════ -->
    <v-dialog v-model="regionsDialog" max-width="640" scrollable>
      <v-card rounded="lg">
        <v-card-title class="pa-4">Régions de marrainage</v-card-title>
        <v-card-text>
          <div class="text-caption text-grey mb-3">
            Renommer une région met à jour toutes les fiches qui la portent. Supprimer une région remet ces fiches
            « sans région » (elle sera alors déduite de l'adresse ou de la commune).
          </div>
          <div v-for="(r, i) in regionsEdit" :key="r.regionId || `new-${i}`" class="d-flex align-center ga-2 mb-2">
            <input v-model="r.couleur" type="color" class="color-input" @change="saveRegion(r)" />
            <v-text-field v-model="r.nom" variant="outlined" density="compact" hide-details @blur="saveRegion(r)" @keyup.enter="saveRegion(r)" />
            <v-btn icon size="small" variant="text" :disabled="i === 0" @click="moveRegion(i, -1)"><v-icon>mdi-arrow-up</v-icon></v-btn>
            <v-btn icon size="small" variant="text" :disabled="i === regionsEdit.length - 1" @click="moveRegion(i, 1)"><v-icon>mdi-arrow-down</v-icon></v-btn>
            <v-btn icon size="small" variant="text" color="error" @click="deleteRegion(r)"><v-icon>mdi-delete-outline</v-icon></v-btn>
          </div>
          <div class="d-flex align-center ga-2 mt-4">
            <v-text-field
              v-model="newRegion" label="Nouvelle région" variant="outlined" density="compact" hide-details
              @keyup.enter="addRegion"
            />
            <v-btn color="primary" :disabled="!newRegion?.trim()" @click="addRegion">Ajouter</v-btn>
          </div>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="regionsDialog = false">Fermer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snack.show" :color="snack.color" timeout="2500" location="bottom right">{{ snack.text }}</v-snackbar>
  </v-container>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import { useRegions, regionDeCommune } from '@/utils/regions'
import { statutInfo } from '@/utils/marrainageStatut'
import { matchesSearch } from '@/utils/search'

moment.locale('fr')
const router = useRouter()
const { regions, regionNames, reloadRegions, regionColor } = useRegions()

// ─── Constantes ───────────────────────────────────────────────────────────────
const SEXES = [
  { value: 'F', short: 'F', label: 'Femme' },
  { value: 'M', short: 'M', label: 'Homme' },
  { value: 'autre', short: 'Autre', label: 'Autre' },
]
const AGES = [
  { value: 'u30', label: '< 30', test: a => a < 30 },
  { value: '30', label: '30–44', test: a => a >= 30 && a < 45 },
  { value: '45', label: '45–59', test: a => a >= 45 && a < 60 },
  { value: '60', label: '60 +', test: a => a >= 60 },
]
const MESURES = [
  { value: 'telecours', label: 'Télécours', color: 'deep-purple' },
  { value: 'fi_a1', label: 'F&I A1', color: 'teal' },
  { value: 'fi_a2', label: 'F&I A2', color: 'teal-darken-3' },
]
const PRIORITES = [
  { value: 'haute', short: 'Très preneur·euse', label: 'Très preneur·euse', icon: 'mdi-fire', color: 'deep-orange', ordre: 0 },
  { value: 'normale', short: 'Normale', label: 'Priorité normale', icon: 'mdi-circle-medium', color: 'grey', ordre: 1 },
  { value: 'basse', short: 'Pas prioritaire', label: 'Pas une priorité', icon: 'mdi-arrow-down-thin', color: 'blue-grey', ordre: 2 },
]
const SOUHAIT_GENRE = { homme: 'Souhaite un parrain', femme: 'Souhaite une marraine', egal: 'Parrain ou marraine' }
const SOUHAIT_AGE = { meme_age: 'environ son âge', plus_age: 'plus âgé·e', plus_jeune: 'plus jeune' }
const STATUTS_DISPO = ['nouveau', 'en_attente', 'disponible', 'a_former', 'a_verifier', 'a_relancer']
const SANS_REGION = 'Sans région'

// ─── État ─────────────────────────────────────────────────────────────────────
const loading = ref(false)
const loaded = ref(false)
const raw = ref({ marraines: [], relations: [], participants: [] })
const raisons = ref([])
const tab = ref('regions')
const focus = ref(null)
const listeRole = ref('tous')
const binomeEtat = ref('en_cours')
const snack = ref({ show: false, text: '', color: 'success' })

const search = ref('')
const fRegions = ref([])
const fSexe = ref([])
const fAge = ref([])
const fMesure = ref([])
const fPriorite = ref([])

const hasFilters = computed(() =>
  !!search.value || [fRegions, fSexe, fAge, fMesure, fPriorite].some(f => f.value.length)
)
function resetFilters() {
  search.value = ''
  ;[fRegions, fSexe, fAge, fMesure, fPriorite].forEach(f => (f.value = []))
  focus.value = null
}

const regionOptions = computed(() => [...regionNames.value, SANS_REGION])

// ─── Normalisation ────────────────────────────────────────────────────────────
function sexeDeGenre(g) {
  const s = (g || '').toLowerCase()
  if (s.startsWith('f')) return 'F'
  if (s.startsWith('h') || s.startsWith('m')) return 'M'
  if (s.startsWith('a')) return 'autre'
  return null
}
const sexeShort = s => SEXES.find(x => x.value === s)?.short || '—'

function mesureDe(classe) {
  const c = (classe || '').toLowerCase()
  if (!c) return null
  if (c.includes('tele') || c.includes('télé')) return MESURES[0]
  if (c.includes('a1')) return MESURES[1]
  if (c.includes('a2')) return MESURES[2]
  return null
}

// Commune = dernier élément de l'adresse (« Rue X 3, 1800 Vevey » → « Vevey »).
function communeDe(adresse) {
  if (!adresse) return ''
  const last = adresse.split(',').pop().trim()
  return last.replace(/^\d{4}\s*/, '')
}

const knownRegion = r => (r && regionNames.value.includes(r) ? r : null)

const participants = computed(() => raw.value.participants.map(p => {
  const region = knownRegion(p.region) || knownRegion(regionDeCommune(p.adresse))
  return {
    ...p,
    sexe: sexeDeGenre(p.genre),
    region,
    commune: communeDe(p.adresse),
    mesure: mesureDe(p.classe),
    prio: PRIORITES.find(x => x.value === p.priorite) || null,
    souhaitGenreLabel: SOUHAIT_GENRE[p.souhaitGenre] || '',
    souhaitLabel: [SOUHAIT_GENRE[p.souhaitGenre], SOUHAIT_AGE[p.souhaitAge]].filter(Boolean).join(', '),
  }
}))
const participantById = computed(() => Object.fromEntries(participants.value.map(p => [p.id, p])))

const marraines = computed(() => raw.value.marraines.map(m => ({
  ...m,
  region: knownRegion(m.region) || knownRegion(regionDeCommune(m.commune)),
  statutInfo: statutInfo(m.statut) || { title: 'Sans statut', color: 'grey' },
})))
const marraineById = computed(() => Object.fromEntries(marraines.value.map(m => [m.id, m])))

const binomes = computed(() => raw.value.relations
  .map(r => {
    const participant = participantById.value[r.participantId]
    const marraine = marraineById.value[r.marrainageId]
    if (!participant || !marraine) return null
    return {
      ...r,
      key: `${r.marrainageId}-${r.participantId}`,
      participant, marraine,
      region: participant.region || marraine.region,
      filleul: `${participant.firstname} ${participant.lastName}`,
      marraineNom: `${marraine.prenom} ${marraine.nom}`,
      etat: r.actif ? 0 : 1,
    }
  })
  .filter(Boolean))

const activeParticipantIds = computed(() => new Set(binomes.value.filter(b => b.actif).map(b => b.participantId)))
const activeMarraineIds = computed(() => new Set(binomes.value.filter(b => b.actif).map(b => b.marrainageId)))

const aTrouver = computed(() => participants.value.filter(p =>
  (p.besoin === 'oui' || p.besoin === 'a_verifier') && !activeParticipantIds.value.has(p.id)))

const disponibles = computed(() => marraines.value.filter(m =>
  STATUTS_DISPO.includes(m.statut) && !activeMarraineIds.value.has(m.id)))

// ─── Filtres ──────────────────────────────────────────────────────────────────
const inRegion = r => !fRegions.value.length || fRegions.value.includes(r || SANS_REGION)
const inSexe = s => !fSexe.value.length || fSexe.value.includes(s)
const inAge = a => !fAge.value.length || (a != null && AGES.some(x => fAge.value.includes(x.value) && x.test(a)))
const inMesure = m => !fMesure.value.length || fMesure.value.includes(m?.value)
const inPrio = p => !fPriorite.value.length || fPriorite.value.includes(p?.value || 'normale')
const inSearch = (...fields) => !search.value || matchesSearch(search.value, ...fields)

const personOk = p => inSexe(p.sexe) && inAge(p.age)

const fATrouver = computed(() => aTrouver.value.filter(p =>
  inRegion(p.region) && personOk(p) && inMesure(p.mesure) && inPrio(p.prio) &&
  inSearch(p.firstname, p.lastName, p.adresse)))

// Mesure et priorité ne concernent que les filleul·e·s : une marraine n'est filtrée que par région/sexe/âge.
const fDisponibles = computed(() => disponibles.value.filter(m =>
  inRegion(m.region) && personOk(m) && inSearch(m.prenom, m.nom, m.commune)))

const fBinomes = computed(() => binomes.value.filter(b =>
  inRegion(b.region) && (personOk(b.participant) || personOk(b.marraine)) &&
  inMesure(b.participant.mesure) && inPrio(b.participant.prio) &&
  inSearch(b.filleul, b.marraineNom, b.participant.adresse, b.marraine.commune)))

const binomesAffiches = computed(() =>
  binomeEtat.value === 'tous' ? fBinomes.value
    : fBinomes.value.filter(b => (binomeEtat.value === 'en_cours') === b.actif))

// ─── Chiffres clés ────────────────────────────────────────────────────────────
const kpis = computed(() => {
  const enCours = fBinomes.value.filter(b => b.actif).length
  const termines = fBinomes.value.filter(b => !b.actif)
  const tresPreneurs = fATrouver.value.filter(p => p.prio?.value === 'haute').length
  const aFormer = fDisponibles.value.filter(m => m.statut === 'a_former').length
  const manque = regionCards.value.reduce((n, r) => n + Math.max(0, r.manque), 0)
  return [
    { key: 'enCours', label: 'Binômes en cours', value: enCours, icon: 'mdi-account-heart', color: 'success' },
    { key: 'aTrouver', label: 'Filleul·e·s à trouver', value: fATrouver.value.length, icon: 'mdi-account-search', color: 'orange-darken-2',
      sub: tresPreneurs ? `dont ${tresPreneurs} très preneur·euse·s` : '', subColor: 'deep-orange' },
    { key: 'dispo', label: 'Parrains / marraines disponibles', value: fDisponibles.value.length, icon: 'mdi-account-clock', color: 'blue',
      sub: aFormer ? `dont ${aFormer} à former` : '' },
    { key: 'manque', label: 'Parrains / marraines à chercher', value: manque, icon: 'mdi-magnify', color: manque ? 'error' : 'success',
      sub: manque ? 'selon les régions en déficit' : 'toutes les régions couvertes' },
    { key: 'termines', label: 'Terminés / interrompus', value: termines.length, icon: 'mdi-account-cancel-outline', color: 'grey',
      sub: termines.filter(b => b.raisonInactivite).length ? `${termines.filter(b => b.raisonInactivite).length} interrompu(s)` : '' },
  ]
})

function toggleFocus(key) {
  if (key === 'termines') { tab.value = 'binomes'; binomeEtat.value = 'termines'; return }
  if (key === 'enCours') { tab.value = 'binomes'; binomeEtat.value = 'en_cours'; return }
  if (key === 'aTrouver') { tab.value = 'liste'; listeRole.value = 'filleul'; return }
  if (key === 'dispo') { tab.value = 'liste'; listeRole.value = 'marraine'; return }
  tab.value = 'regions'
}

// ─── Cartes par région ────────────────────────────────────────────────────────
const byPrioThenName = (a, b) =>
  (a.prio?.ordre ?? 1) - (b.prio?.ordre ?? 1) || `${a.firstname}`.localeCompare(`${b.firstname}`, 'fr')

const regionCards = computed(() => {
  const list = [...regions.value.map(r => ({ nom: r.nom, couleur: r.couleur || regionColor(r.nom), region: r })),
    { nom: SANS_REGION, couleur: '#BDBDBD', region: null }]
  return list
    .filter(r => !fRegions.value.length || fRegions.value.includes(r.nom))
    .map(r => {
      const match = x => (x.region || SANS_REGION) === r.nom
      const enCours = fBinomes.value.filter(b => b.actif && match(b))
      const aT = fATrouver.value.filter(match).sort(byPrioThenName)
      const dispo = fDisponibles.value.filter(match).sort((a, b) => `${a.prenom}`.localeCompare(`${b.prenom}`, 'fr'))
      return { ...r, enCours, aTrouver: aT, dispo, manque: aT.length - dispo.length, total: enCours.length + aT.length + dispo.length }
    })
    // « Sans région » seulement s'il y a quelqu'un à classer.
    .filter(r => r.nom !== SANS_REGION || r.total)
})

// ─── Liste de toutes les personnes ────────────────────────────────────────────
const regionOrdre = r => {
  const i = regionNames.value.indexOf(r)
  return i < 0 ? 999 : i
}

function situationFilleul(p) {
  const b = binomes.value.find(x => x.actif && x.participantId === p.id)
  if (b) return { label: 'En binôme', color: 'success', ordre: 2, detail: `avec ${b.marraineNom}` }
  return { label: 'À trouver', color: 'orange-darken-2', ordre: 0, detail: p.besoin === 'a_verifier' ? 'Souhait à vérifier' : '' }
}
function situationMarraine(m) {
  const actifs = binomes.value.filter(x => x.actif && x.marrainageId === m.id)
  if (actifs.length) return { label: 'En binôme', color: 'success', ordre: 2, detail: `avec ${actifs.map(b => b.participant.firstname).join(', ')}` }
  return { label: m.statutInfo.title, color: m.statutInfo.color, ordre: 1, detail: '' }
}

const personnes = computed(() => {
  const out = []
  if (listeRole.value !== 'marraine') {
    const enBinome = fBinomes.value.filter(b => b.actif).map(b => b.participant)
    const seen = new Set()
    for (const p of [...fATrouver.value, ...enBinome]) {
      if (seen.has(p.id)) continue
      seen.add(p.id)
      const situation = situationFilleul(p)
      out.push({
        role: 'filleul', id: p.id, nomComplet: `${p.firstname} ${p.lastName}`, nom: `${p.lastName} ${p.firstname}`,
        commune: p.commune, region: p.region, regionOrdre: regionOrdre(p.region), sexe: p.sexe, age: p.age,
        mesure: p.mesure, mesureLabel: p.mesure?.label || '', prio: p.prio, prioOrdre: p.prio?.ordre ?? 1,
        souhaitLabel: p.souhaitLabel, situation, situationOrdre: situation.ordre,
      })
    }
  }
  if (listeRole.value !== 'filleul') {
    const enBinome = fBinomes.value.filter(b => b.actif).map(b => b.marraine)
    const seen = new Set()
    for (const m of [...fDisponibles.value, ...enBinome]) {
      if (seen.has(m.id)) continue
      seen.add(m.id)
      const situation = situationMarraine(m)
      out.push({
        role: 'marraine', id: m.id, nomComplet: `${m.prenom} ${m.nom}`, nom: `${m.nom} ${m.prenom}`,
        commune: m.commune, region: m.region, regionOrdre: regionOrdre(m.region), sexe: m.sexe, age: m.age,
        mesure: null, mesureLabel: '', prio: null, prioOrdre: 9, souhaitLabel: '',
        situation, situationOrdre: situation.ordre,
      })
    }
  }
  return out
})

const personHeaders = [
  { title: 'Nom', key: 'nom' },
  { title: 'Rôle', key: 'role' },
  { title: 'Région', key: 'regionOrdre' },
  { title: 'Sexe', key: 'sexe', width: 70 },
  { title: 'Âge', key: 'age', width: 70 },
  { title: 'Mesure', key: 'mesureLabel' },
  { title: 'Situation', key: 'situationOrdre' },
  { title: 'Priorité', key: 'prioOrdre' },
  { title: 'Souhait', key: 'souhait', sortable: false },
]

const binomeHeaders = [
  { title: 'Filleul·e', key: 'filleul' },
  { title: 'Parrain / marraine', key: 'marraineNom' },
  { title: 'Région', key: 'region' },
  { title: 'Début', key: 'dateDebut' },
  { title: 'État', key: 'etat' },
  { title: 'Suivi', key: 'commentaire', sortable: false },
]

// ─── Formatage ────────────────────────────────────────────────────────────────
const fmtDate = d => (d ? moment(d).format('DD.MM.YYYY') : '—')
const fmtMois = d => moment(d).format('MMM YYYY')
function identite(x) {
  const parts = []
  if (x.sexe) parts.push(sexeShort(x.sexe))
  if (x.age != null) parts.push(`${x.age} ans`)
  return parts.join(' · ')
}
const toast = (text, color = 'success') => { snack.value = { show: true, text, color } }

// ─── Navigation ───────────────────────────────────────────────────────────────
const goParticipant = id => router.push({ name: 'Participant-Detail', params: { id } })
const goMarraine = id => router.push({ name: 'Marrainage-Modifier', params: { id } })

// ─── Chargement ───────────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const [res, resR] = await Promise.all([
      apiFetch(`${API_BASE}/api/Marrainages/tableau`),
      apiFetch(`${API_BASE}/api/MarrainageInactivityReasons`),
    ])
    if (res?.ok) raw.value = await res.json()
    else toast('Impossible de charger le tableau', 'error')
    if (resR?.ok) raisons.value = (await resR.json()).map(r => r.label)
    loaded.value = true
  } catch (e) {
    console.error(e)
    toast('Erreur serveur', 'error')
  } finally {
    loading.value = false
  }
}

onMounted(load)

// ─── Suivi d'un binôme ────────────────────────────────────────────────────────
const relDialog = ref({ open: false, binome: null, saving: false })
const relForm = ref({ etat: 'en_cours', dateDebut: '', dateFin: '', raison: null, commentaire: '' })

function openRelation(b) {
  relDialog.value = { open: true, binome: b, saving: false }
  relForm.value = {
    etat: b.actif ? 'en_cours' : (b.raisonInactivite ? 'interrompu' : 'termine'),
    dateDebut: b.dateDebut ? b.dateDebut.substring(0, 10) : '',
    dateFin: b.dateFin ? b.dateFin.substring(0, 10) : '',
    raison: b.raisonInactivite || null,
    commentaire: b.commentaire || '',
  }
}

watch(() => relForm.value.etat, etat => {
  if (etat !== 'en_cours' && !relForm.value.dateFin) relForm.value.dateFin = moment().format('YYYY-MM-DD')
})

async function saveRelation() {
  const b = relDialog.value.binome
  const f = relForm.value
  const actif = f.etat === 'en_cours'
  relDialog.value.saving = true
  try {
    // Une nouvelle raison saisie librement rejoint la liste partagée.
    if (f.etat === 'interrompu' && f.raison && !raisons.value.includes(f.raison)) {
      await apiFetch(`${API_BASE}/api/MarrainageInactivityReasons`, { method: 'POST', body: JSON.stringify({ label: f.raison }) })
      raisons.value.push(f.raison)
    }
    const res = await apiFetch(`${API_BASE}/api/Marrainages/${b.marrainageId}/participants/${b.participantId}`, {
      method: 'PATCH',
      body: JSON.stringify({
        dateDebut: f.dateDebut || null,
        dateFin: actif ? null : (f.dateFin || null),
        actif,
        raisonInactivite: f.etat === 'interrompu' ? f.raison : null,
        commentaire: f.commentaire || '',
      }),
    })
    if (!res?.ok) { toast("Erreur lors de l'enregistrement", 'error'); return }
    const rel = raw.value.relations.find(r => r.marrainageId === b.marrainageId && r.participantId === b.participantId)
    Object.assign(rel, {
      dateDebut: f.dateDebut || null, dateFin: actif ? null : (f.dateFin || null), actif,
      raisonInactivite: f.etat === 'interrompu' ? f.raison : null, commentaire: f.commentaire || null,
    })
    relDialog.value.open = false
    toast('Binôme mis à jour ✓')
  } finally {
    relDialog.value.saving = false
  }
}

// ─── Régions ──────────────────────────────────────────────────────────────────
const regionsDialog = ref(false)
const regionsEdit = ref([])
const newRegion = ref('')

watch(regionsDialog, open => {
  if (open) regionsEdit.value = regions.value.map(r => ({ ...r, _nom: r.nom }))
})

const regionPayload = r => ({ nom: r.nom, couleur: r.couleur, ordre: r.ordre, pistesRecherche: r.pistesRecherche })

async function saveRegion(r) {
  if (!r.regionId || !r.nom?.trim()) return
  const res = await apiFetch(`${API_BASE}/api/Regions/${r.regionId}`, { method: 'PUT', body: JSON.stringify(regionPayload(r)) })
  if (!res?.ok) { toast(res?.status === 409 ? 'Cette région existe déjà' : 'Erreur', 'error'); return }
  const renamed = r._nom !== r.nom
  r._nom = r.nom
  await reloadRegions()
  if (renamed) await load()
}

async function moveRegion(i, delta) {
  const list = regionsEdit.value
  ;[list[i], list[i + delta]] = [list[i + delta], list[i]]
  list.forEach((r, idx) => (r.ordre = idx + 1))
  await Promise.all([list[i], list[i + delta]].map(r =>
    apiFetch(`${API_BASE}/api/Regions/${r.regionId}`, { method: 'PUT', body: JSON.stringify(regionPayload(r)) })))
  await reloadRegions()
}

async function addRegion() {
  const nom = newRegion.value?.trim()
  if (!nom) return
  const res = await apiFetch(`${API_BASE}/api/Regions`, { method: 'POST', body: JSON.stringify({ nom, couleur: '#607D8B' }) })
  if (!res?.ok) { toast(res?.status === 409 ? 'Cette région existe déjà' : 'Erreur', 'error'); return }
  newRegion.value = ''
  await reloadRegions()
  regionsEdit.value = regions.value.map(r => ({ ...r, _nom: r.nom }))
}

async function deleteRegion(r) {
  if (!confirm(`Supprimer la région « ${r.nom} » ? Les fiches qui la portent repasseront sans région.`)) return
  const res = await apiFetch(`${API_BASE}/api/Regions/${r.regionId}`, { method: 'DELETE' })
  if (!res?.ok) { toast('Erreur', 'error'); return }
  await reloadRegions()
  regionsEdit.value = regions.value.map(x => ({ ...x, _nom: x.nom }))
  await load()
}

// ─── Pistes de recherche ──────────────────────────────────────────────────────
const pistesDialog = ref({ open: false, region: null, text: '', saving: false })

function editPistes(region) {
  pistesDialog.value = { open: true, region, text: region.pistesRecherche || '', saving: false }
}

async function savePistes() {
  const r = pistesDialog.value.region
  if (!r.regionId) { toast('Régions non chargées depuis la base', 'error'); return }
  pistesDialog.value.saving = true
  const res = await apiFetch(`${API_BASE}/api/Regions/${r.regionId}`, {
    method: 'PUT', body: JSON.stringify({ ...regionPayload(r), pistesRecherche: pistesDialog.value.text || null }),
  })
  pistesDialog.value.saving = false
  if (!res?.ok) { toast('Erreur', 'error'); return }
  await reloadRegions()
  pistesDialog.value.open = false
}
</script>

<style scoped>
/* ── Chiffres clés ── */
.kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; }
.kpi {
  display: flex; align-items: center; padding: 14px 16px;
  background: #fff; border: 1px solid #e4e9f2; border-radius: 12px; cursor: pointer;
  transition: border-color .15s, box-shadow .15s;
}
.kpi:hover { border-color: #90caf9; box-shadow: 0 2px 8px rgba(30, 60, 120, .08); }
.kpi--active { border-color: #1976d2; }
.kpi__value { font-size: 1.6rem; font-weight: 800; line-height: 1.1; color: #1a2233; }
.kpi__label { font-size: .8rem; color: #5a6578; font-weight: 600; }
.kpi__sub { font-size: .72rem; margin-top: 2px; }

/* ── Filtres ── */
.filter-group { display: flex; align-items: center; gap: 6px; }
.filter-label { font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .04em; color: #7a8599; }

.dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; margin-right: 6px; }

/* ── Cartes région ── */
.region-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(330px, 1fr)); gap: 16px; align-items: start; }
.region-card { overflow: hidden; }
.region-card--empty { opacity: .75; }
.region-card__head { padding: 12px 14px 10px; border-top: 6px solid; background: #fafbfd; border-bottom: 1px solid #eef1f6; }
.region-card__name { font-size: 1.05rem; font-weight: 800; color: #1a2233; }
.region-card__counts { display: flex; gap: 14px; font-size: .78rem; color: #5a6578; margin-top: 4px; }
.region-card__counts b { color: #1a2233; }
.region-card__body { padding: 6px 10px 10px; }

.section { padding: 6px 0; }
.section + .section { border-top: 1px dashed #e4e9f2; }
.section__title { font-size: .72rem; font-weight: 800; text-transform: uppercase; letter-spacing: .04em; display: flex; align-items: center; margin: 2px 4px 6px; }
.section__empty { font-size: .8rem; color: #a7b0bf; font-style: italic; padding: 0 6px 4px; }

.person { padding: 6px 8px; border-radius: 8px; cursor: pointer; transition: background .12s; }
.person:hover { background: #f0f5ff; }
.person__main { display: flex; align-items: center; flex-wrap: wrap; gap: 2px 6px; }
.person__name { font-weight: 600; font-size: .87rem; color: #1a2233; }
.person__sub { font-size: .75rem; color: #8a94a6; }
.person__meta { display: flex; align-items: center; flex-wrap: wrap; gap: 4px 8px; font-size: .74rem; color: #6b7588; margin-top: 2px; }
.person__comment { max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-style: italic; }
.person--binome .person__main { gap: 0; }

.pistes { margin-top: 6px; padding: 8px 10px; background: #fffbea; border-radius: 8px; font-size: .78rem; cursor: pointer; display: flex; align-items: flex-start; }
.pistes:hover { background: #fff4c2; }
.pistes__text { white-space: pre-line; }

/* ── Tableaux ── */
.region-pill { color: #fff; font-size: .72rem; font-weight: 700; padding: 2px 8px; border-radius: 10px; white-space: nowrap; }
.person-table :deep(tbody tr), .comment-cell { cursor: pointer; }
.comment-cell { display: inline-block; max-width: 340px; white-space: pre-line; }
.link { color: inherit; text-decoration: none; }
.link:hover { text-decoration: underline; color: #1976d2; }

.color-input { width: 34px; height: 34px; border: none; padding: 0; background: none; cursor: pointer; flex-shrink: 0; }
</style>
