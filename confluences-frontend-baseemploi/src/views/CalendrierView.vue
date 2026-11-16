<template>
  <v-container fluid class="pa-6">

    <!-- Header -->
    <div class="d-flex align-center flex-wrap mb-4 gap-2">
      <div>
        <div class="text-h5 font-weight-bold text-primary">Calendrier de formation</div>
        <div class="text-caption text-grey">Sessions, jours fériés et périodes de vacances — par année</div>
      </div>
      <v-spacer />
      <v-fade-transition>
        <v-chip v-if="saveStatus === 'saving'" color="orange" variant="tonal" size="small">
          <v-progress-circular size="12" width="2" indeterminate class="mr-1" />
          Enregistrement…
        </v-chip>
        <v-chip v-else-if="saveStatus === 'saved'" color="success" variant="tonal" size="small">
          <v-icon start size="small">mdi-check-circle</v-icon>
          Enregistré
        </v-chip>
        <v-chip v-else-if="saveStatus === 'error'" color="error" variant="tonal" size="small">
          <v-icon start size="small">mdi-alert-circle</v-icon>
          Erreur
        </v-chip>
      </v-fade-transition>
    </div>

    <!-- ════════ Barre des années ════════ -->
    <div class="year-bar d-flex align-center flex-wrap gap-2 mb-4">
      <v-btn-toggle
        v-model="selectedYear"
        mandatory divided density="comfortable"
        color="primary" variant="outlined" rounded="lg"
      >
        <v-btn v-for="y in activeYears" :key="y" :value="y" class="year-btn">
          {{ y }}
          <v-badge
            v-if="countForYear(y)"
            :content="countForYear(y)" inline
            :color="y === currentYear ? 'primary' : 'grey'"
          />
          <v-icon v-if="y === currentYear" size="x-small" class="ml-1" color="primary">mdi-circle-medium</v-icon>
        </v-btn>
      </v-btn-toggle>

      <v-btn variant="text" color="primary" prepend-icon="mdi-plus" @click="addYear">
        Année {{ maxYear + 1 }}
      </v-btn>

      <v-spacer />

      <v-menu v-if="archivedYears.length">
        <template #activator="{ props }">
          <v-btn
            v-bind="props"
            :variant="isArchivedYear ? 'flat' : 'tonal'"
            :color="isArchivedYear ? 'blue-grey' : undefined"
            prepend-icon="mdi-archive-outline"
            append-icon="mdi-chevron-down"
          >
            {{ isArchivedYear ? `Archive ${selectedYear}` : 'Archives' }}
            <v-chip size="x-small" class="ml-2" variant="tonal">{{ archivedYears.length }}</v-chip>
          </v-btn>
        </template>
        <v-list density="compact" min-width="220">
          <v-list-subheader>Années passées</v-list-subheader>
          <v-list-item
            v-for="y in archivedYears" :key="y"
            :active="selectedYear === y"
            @click="selectedYear = y"
          >
            <v-list-item-title>{{ y }}</v-list-item-title>
            <template #append>
              <span class="text-caption text-grey">{{ countForYear(y) }} session(s)</span>
            </template>
          </v-list-item>
        </v-list>
      </v-menu>
    </div>

    <!-- Bandeau archive -->
    <v-alert
      v-if="isArchivedYear"
      type="info" variant="tonal" color="blue-grey" density="compact"
      icon="mdi-archive-lock-outline" class="mb-4"
    >
      <div class="d-flex align-center flex-wrap gap-2">
        <span>
          <strong>{{ selectedYear }} est archivée.</strong>
          {{ archiveUnlocked ? 'Modifications autorisées — les registres de présence de cette année seront recalculés.' : 'Consultation seule.' }}
        </span>
        <v-spacer />
        <v-btn
          size="small" variant="text"
          :prepend-icon="archiveUnlocked ? 'mdi-lock-outline' : 'mdi-lock-open-variant-outline'"
          @click="archiveUnlocked = !archiveUnlocked"
        >
          {{ archiveUnlocked ? 'Verrouiller' : 'Déverrouiller' }}
        </v-btn>
      </div>
    </v-alert>

    <!-- ════════ Frise annuelle ════════ -->
    <v-card class="elevation-2 mb-4" rounded="lg">
      <v-card-text class="pa-4">
        <div class="d-flex align-center flex-wrap gap-3 mb-3">
          <div class="text-subtitle-1 font-weight-bold">
            <v-icon start size="small" color="primary">mdi-chart-timeline</v-icon>
            Vue d'ensemble {{ selectedYear }}
          </div>
          <v-spacer />
          <div class="legend d-flex align-center flex-wrap gap-3 text-caption text-grey-darken-1">
            <span><i class="lg lg--fi" />F&amp;I</span>
            <span><i class="lg lg--tc" />Télécours</span>
            <span><i class="lg lg--vs" />Vacances session</span>
            <span><i class="lg lg--vg" />Vacances générales</span>
            <span><i class="lg lg--fe" />Férié</span>
          </div>
        </div>

        <div class="timeline">
          <!-- En-tête mois -->
          <div class="tl-row tl-row--head">
            <div class="tl-label" />
            <div class="tl-track">
              <div
                v-for="m in months" :key="m.index"
                class="tl-month" :style="{ left: m.left + '%', width: m.width + '%' }"
              >{{ m.label }}</div>
            </div>
          </div>

          <div v-for="t in types" :key="t.value" class="tl-row">
            <div class="tl-label">
              <v-icon size="small" :color="t.color" class="mr-1">{{ t.icon }}</v-icon>{{ t.label }}
            </div>
            <div class="tl-track tl-track--body">
              <!-- grille mois -->
              <div v-for="m in months" :key="m.index" class="tl-grid" :style="{ left: m.left + '%' }" />
              <!-- vacances générales -->
              <div
                v-for="v in yearVacances" :key="'vg' + (v.id || v.label)"
                class="tl-band" :style="barStyle(v.startDate, v.endDate)"
                :title="`${v.label || 'Vacances'} : ${fmtDate(v.startDate)} → ${fmtDate(v.endDate)}`"
              />
              <!-- fériés -->
              <div
                v-for="f in yearFeries" :key="'fe' + f"
                class="tl-ferie" :style="{ left: pos(f) + '%' }"
                :title="`Férié : ${fmtDate(f)}`"
              />
              <!-- sessions -->
              <template v-for="s in sessionsFor(t.value, selectedYear, true)" :key="s._localId">
                <div
                  class="tl-bar" :class="[`tl-bar--${t.kind}`, { 'tl-bar--past': statusOf(s) === 'done' }]"
                  :style="barStyle(s.startDate, s.endDate)"
                  :title="`${s.name} : ${fmtDate(s.startDate)} → ${fmtDate(s.endDate)}`"
                  @click="openSessionDialog(s)"
                >
                  <span class="tl-bar__text">{{ shortName(s) }}</span>
                </div>
                <div
                  v-if="s.vacanceStart && s.vacanceEnd"
                  class="tl-vac" :style="barStyle(s.vacanceStart, s.vacanceEnd)"
                  :title="`Vacances ${s.name} : ${fmtDate(s.vacanceStart)} → ${fmtDate(s.vacanceEnd)}`"
                />
              </template>
              <!-- aujourd'hui -->
              <div v-if="selectedYear === currentYear" class="tl-today" :style="{ left: pos(today) + '%' }" />
            </div>
          </div>
        </div>
      </v-card-text>
    </v-card>

    <v-row>
      <!-- ════════ Sessions ════════ -->
      <v-col cols="12" lg="8">
        <v-card class="elevation-2" rounded="lg">
          <v-card-title class="d-flex align-center flex-wrap gap-2 pa-4">
            <v-icon start color="primary">mdi-calendar-clock</v-icon>
            Sessions {{ selectedYear }}
            <v-spacer />
            <v-btn
              v-if="canDuplicate"
              variant="tonal" color="primary" size="small" prepend-icon="mdi-content-copy"
              :disabled="readOnly"
              @click="openDuplicateDialog"
            >
              Reprendre {{ selectedYear - 1 }}
            </v-btn>
            <v-btn
              color="primary" size="small" prepend-icon="mdi-plus"
              :disabled="readOnly"
              @click="openSessionDialog(null)"
            >
              Nouvelle session
            </v-btn>
          </v-card-title>

          <div class="px-4 pb-2 d-flex flex-wrap gap-2">
            <v-chip size="small" variant="tonal" color="blue" prepend-icon="mdi-school">F&amp;I : 20 / 40 / 60 j</v-chip>
            <v-chip size="small" variant="tonal" color="teal" prepend-icon="mdi-monitor">Télécours : 10 / 20 / 32 j</v-chip>
          </div>

          <v-divider />

          <div v-if="!yearSessionCount" class="text-center pa-10">
            <v-icon size="48" color="grey-lighten-1">mdi-calendar-blank-outline</v-icon>
            <div class="text-body-2 text-grey mt-2">Aucune session saisie pour {{ selectedYear }}.</div>
            <div v-if="!readOnly" class="d-flex justify-center flex-wrap gap-2 mt-4">
              <v-btn
                v-if="canDuplicate" color="primary" prepend-icon="mdi-content-copy"
                @click="openDuplicateDialog"
              >
                Reprendre les sessions {{ selectedYear - 1 }}
              </v-btn>
              <v-btn variant="outlined" color="primary" prepend-icon="mdi-plus" @click="openSessionDialog(null)">
                Saisir manuellement
              </v-btn>
            </div>
          </div>

          <v-table v-else density="comfortable" class="sessions-table">
            <thead>
              <tr>
                <th>Session</th>
                <th>Période</th>
                <th class="text-center">Semaines</th>
                <th>Vacances</th>
                <th>Statut</th>
                <th />
              </tr>
            </thead>
            <tbody>
              <template v-for="t in types" :key="t.value">
                <tr class="group-row">
                  <td colspan="6">
                    <div class="d-flex align-center">
                      <v-icon size="small" :color="t.color" class="mr-2">{{ t.icon }}</v-icon>
                      <span class="font-weight-bold">{{ t.label }}</span>
                      <v-chip size="x-small" variant="tonal" :color="t.color" class="ml-2">
                        {{ sessionsFor(t.value, selectedYear).length }}
                      </v-chip>
                      <v-spacer />
                      <v-btn
                        v-if="!readOnly" size="x-small" variant="text" :color="t.color" prepend-icon="mdi-plus"
                        @click="openSessionDialog(null, t.value)"
                      >Ajouter</v-btn>
                    </div>
                  </td>
                </tr>
                <tr v-if="!sessionsFor(t.value, selectedYear).length">
                  <td colspan="6" class="text-caption text-grey font-italic pl-10">Aucune session</td>
                </tr>
                <tr
                  v-for="s in sessionsFor(t.value, selectedYear)" :key="s._localId"
                  class="session-tr" :class="{ 'session-tr--done': statusOf(s) === 'done' }"
                  @click="!readOnly && openSessionDialog(s)"
                >
                  <td class="pl-10 font-weight-medium">
                    {{ s.name }}
                    <v-icon v-if="overlaps(s).length" size="small" color="warning" class="ml-1"
                      :title="`Chevauche : ${overlaps(s).map(o => o.name).join(', ')}`">mdi-alert</v-icon>
                  </td>
                  <td class="text-no-wrap">{{ fmtShort(s.startDate) }} → {{ fmtShort(s.endDate) }}</td>
                  <td class="text-center">{{ weeks(s.startDate, s.endDate) }}</td>
                  <td class="text-no-wrap text-green-darken-2">
                    <template v-if="s.vacanceStart">
                      <v-icon size="x-small">mdi-beach</v-icon>
                      {{ fmtShort(s.vacanceStart) }} → {{ fmtShort(s.vacanceEnd) }}
                    </template>
                    <span v-else class="text-grey">—</span>
                  </td>
                  <td>
                    <v-chip size="x-small" variant="tonal" :color="statusMeta[statusOf(s)].color">
                      {{ statusMeta[statusOf(s)].label }}
                    </v-chip>
                  </td>
                  <td class="text-right text-no-wrap">
                    <template v-if="!readOnly">
                      <v-btn icon size="x-small" variant="text" title="Modifier" @click.stop="openSessionDialog(s)">
                        <v-icon size="small">mdi-pencil-outline</v-icon>
                      </v-btn>
                      <v-btn icon size="x-small" variant="text" color="error" title="Supprimer" @click.stop="askDelete(s)">
                        <v-icon size="small">mdi-delete-outline</v-icon>
                      </v-btn>
                    </template>
                  </td>
                </tr>
              </template>
            </tbody>
          </v-table>
        </v-card>
      </v-col>

      <!-- ════════ Fériés + Vacances ════════ -->
      <v-col cols="12" lg="4">

        <!-- Vacances générales -->
        <v-card class="elevation-2 mb-4" rounded="lg">
          <v-card-title class="d-flex align-center pa-4">
            <v-icon start color="green-darken-2">mdi-beach</v-icon>
            Vacances générales
            <v-chip size="x-small" variant="tonal" color="green-darken-2" class="ml-2">{{ yearVacances.length }}</v-chip>
            <v-spacer />
            <v-btn
              v-if="!readOnly" icon size="small" variant="tonal" color="green-darken-2"
              title="Ajouter une période" @click="openVacanceDialog(null)"
            >
              <v-icon>mdi-plus</v-icon>
            </v-btn>
          </v-card-title>
          <v-card-text class="pt-0">
            <div class="text-caption text-grey mb-3">
              S'appliquent à tous les groupes. Les vacances propres à une session se saisissent dans la session.
            </div>
            <div v-if="!yearVacances.length" class="text-caption text-grey font-italic">
              Aucune période pour {{ selectedYear }}.
            </div>
            <div v-else class="item-list">
              <div v-for="v in yearVacances" :key="v._localId" class="item-row">
                <div class="flex-grow-1">
                  <div class="font-weight-medium text-body-2">{{ v.label || 'Vacances' }}</div>
                  <div class="text-caption text-grey">{{ fmtShort(v.startDate) }} → {{ fmtShort(v.endDate) }}</div>
                </div>
                <template v-if="!readOnly">
                  <v-btn icon size="x-small" variant="text" @click="openVacanceDialog(v)">
                    <v-icon size="small">mdi-pencil-outline</v-icon>
                  </v-btn>
                  <v-btn icon size="x-small" variant="text" color="error" @click="deleteVacance(v)">
                    <v-icon size="small">mdi-delete-outline</v-icon>
                  </v-btn>
                </template>
              </div>
            </div>
          </v-card-text>
        </v-card>

        <!-- Jours fériés -->
        <v-card class="elevation-2" rounded="lg">
          <v-card-title class="d-flex align-center pa-4">
            <v-icon start color="amber-darken-3">mdi-calendar-star</v-icon>
            Jours fériés
            <v-chip size="x-small" variant="tonal" color="amber-darken-3" class="ml-2">{{ yearFeries.length }}</v-chip>
            <v-spacer />
            <v-btn
              v-if="!readOnly" size="small" variant="tonal" color="amber-darken-3"
              :prepend-icon="showFeriePicker ? 'mdi-chevron-up' : 'mdi-calendar-plus'"
              @click="showFeriePicker = !showFeriePicker"
            >
              {{ showFeriePicker ? 'Fermer' : 'Choisir' }}
            </v-btn>
          </v-card-title>
          <v-card-text class="pt-0">
            <v-expand-transition>
              <div v-if="showFeriePicker && !readOnly">
                <v-date-picker
                  :key="selectedYear"
                  :model-value="joursFeries"
                  v-model:month="pickerMonth"
                  v-model:year="pickerYear"
                  multiple width="100%"
                  color="amber-darken-3"
                  :first-day-of-week="1"
                  hide-header
                  class="elevation-0 mb-3 border rounded-lg"
                  @update:modelValue="onFeriesChange"
                />
              </div>
            </v-expand-transition>
            <div v-if="!yearFeries.length" class="text-caption text-grey font-italic">
              Aucun jour férié pour {{ selectedYear }}.
            </div>
            <div v-else class="d-flex flex-wrap gap-1">
              <v-chip
                v-for="d in yearFeries" :key="d"
                color="amber-darken-3" variant="tonal" size="small"
                :closable="!readOnly" @click:close="removeFerie(d)"
              >
                {{ fmtFerie(d) }}
              </v-chip>
            </div>
          </v-card-text>
        </v-card>

      </v-col>
    </v-row>

    <!-- ════════ Dialogue session ════════ -->
    <v-dialog v-model="sessionDialog.open" max-width="620" persistent>
      <v-card rounded="lg">
        <v-card-title class="d-flex align-center pa-4">
          <v-icon start :color="sessionDialog.editing ? 'orange' : 'primary'">
            {{ sessionDialog.editing ? 'mdi-pencil' : 'mdi-plus-circle' }}
          </v-icon>
          {{ sessionDialog.editing ? 'Modifier la session' : 'Nouvelle session' }}
          <v-spacer />
          <v-btn icon variant="text" size="small" @click="sessionDialog.open = false"><v-icon>mdi-close</v-icon></v-btn>
        </v-card-title>
        <v-card-text class="pt-2">
          <v-btn-toggle
            v-model="form.type" mandatory divided density="compact" variant="outlined"
            color="primary" class="mb-4" @update:modelValue="onFormTypeChange"
          >
            <v-btn v-for="t in types" :key="t.value" :value="t.value" size="small">
              <v-icon start size="small">{{ t.icon }}</v-icon>{{ t.label }}
            </v-btn>
          </v-btn-toggle>

          <v-row dense>
            <v-col cols="12">
              <v-text-field
                v-model="form.name" label="Nom" variant="outlined" density="compact"
                :hint="`Suggestion : ${suggestName(form.type, form.startDate)}`" persistent-hint
              />
            </v-col>
            <v-col cols="6">
              <v-text-field v-model="form.startDate" label="Début" type="date" variant="outlined" density="compact"
                @update:modelValue="onFormStartChange" />
            </v-col>
            <v-col cols="6">
              <v-text-field v-model="form.endDate" label="Fin" type="date" variant="outlined" density="compact"
                :error-messages="formErrors.end" />
            </v-col>
          </v-row>

          <div class="text-caption text-grey mt-1 mb-2">
            <v-icon size="x-small">mdi-beach</v-icon> Vacances après la session (optionnel)
          </div>
          <v-row dense>
            <v-col cols="6">
              <v-text-field v-model="form.vacanceStart" label="Début vacances" type="date" variant="outlined" density="compact"
                clearable />
            </v-col>
            <v-col cols="6">
              <v-text-field v-model="form.vacanceEnd" label="Fin vacances" type="date" variant="outlined" density="compact"
                clearable :error-messages="formErrors.vacance" />
            </v-col>
          </v-row>

          <v-alert v-if="formOverlaps.length" type="warning" variant="tonal" density="compact" class="mt-2">
            Chevauche : {{ formOverlaps.map(o => o.name).join(', ') }}
          </v-alert>
          <div v-if="form.startDate && form.endDate && !formErrors.end" class="text-caption text-grey mt-2">
            Durée : {{ weeks(form.startDate, form.endDate) }} semaines
          </div>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-btn
            v-if="sessionDialog.editing" color="error" variant="text" prepend-icon="mdi-delete-outline"
            @click="askDelete(sessionDialog.editing)"
          >Supprimer</v-btn>
          <v-spacer />
          <v-btn variant="text" @click="sessionDialog.open = false">Annuler</v-btn>
          <v-btn
            v-if="!sessionDialog.editing" variant="tonal" color="primary"
            :disabled="!formValid" :loading="isSavingSession"
            @click="submitSession(true)"
          >Ajouter et continuer</v-btn>
          <v-btn
            color="primary" variant="flat"
            :disabled="!formValid" :loading="isSavingSession"
            @click="submitSession(false)"
          >{{ sessionDialog.editing ? 'Mettre à jour' : 'Ajouter' }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Dialogue duplication ════════ -->
    <v-dialog v-model="dup.open" max-width="900" scrollable>
      <v-card rounded="lg">
        <v-card-title class="pa-4">
          <v-icon start color="primary">mdi-content-copy</v-icon>
          Reprendre les sessions {{ selectedYear - 1 }} pour {{ selectedYear }}
        </v-card-title>
        <v-card-text>
          <v-alert type="info" variant="tonal" density="compact" class="mb-4">
            Les dates sont décalées de 52 semaines (même jour de la semaine). Vérifiez et ajustez avant de valider.
          </v-alert>
          <v-table density="compact" class="dup-table">
            <thead>
              <tr>
                <th style="width:36px">
                  <v-checkbox-btn
                    :model-value="dup.rows.every(r => r.include)"
                    :indeterminate="dup.rows.some(r => r.include) && !dup.rows.every(r => r.include)"
                    @update:modelValue="v => dup.rows.forEach(r => (r.include = v))"
                  />
                </th>
                <th>Nom</th>
                <th>Début</th>
                <th>Fin</th>
                <th>Vac. début</th>
                <th>Vac. fin</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="t in types" :key="t.value">
                <tr v-if="dup.rows.some(r => r.type === t.value)" class="group-row">
                  <td colspan="6">
                    <v-icon size="small" :color="t.color" class="mr-1">{{ t.icon }}</v-icon>
                    <strong>{{ t.label }}</strong>
                  </td>
                </tr>
                <tr v-for="r in dup.rows.filter(r => r.type === t.value)" :key="r.key" :class="{ 'text-disabled': !r.include }">
                  <td><v-checkbox-btn v-model="r.include" /></td>
                  <td><input v-model="r.name" class="cell-input" :disabled="!r.include" /></td>
                  <td><input v-model="r.startDate" type="date" class="cell-input" :disabled="!r.include" /></td>
                  <td><input v-model="r.endDate" type="date" class="cell-input" :disabled="!r.include" /></td>
                  <td><input v-model="r.vacanceStart" type="date" class="cell-input" :disabled="!r.include" /></td>
                  <td><input v-model="r.vacanceEnd" type="date" class="cell-input" :disabled="!r.include" /></td>
                </tr>
              </template>
            </tbody>
          </v-table>
          <v-checkbox
            v-if="dup.vacances.length"
            v-model="dup.includeVacances" density="compact" hide-details class="mt-3"
            :label="`Reprendre aussi les ${dup.vacances.length} période(s) de vacances générales (à vérifier ensuite)`"
          />
        </v-card-text>
        <v-card-actions class="pa-4">
          <span class="text-caption text-grey">{{ dup.rows.filter(r => r.include).length }} session(s) sélectionnée(s)</span>
          <v-spacer />
          <v-btn variant="text" @click="dup.open = false">Annuler</v-btn>
          <v-btn
            color="primary" variant="flat" :loading="isSavingSession"
            :disabled="!dup.rows.some(r => r.include && r.name && r.startDate && r.endDate)"
            @click="confirmDuplicate"
          >Créer les sessions</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Dialogue vacances ════════ -->
    <v-dialog v-model="vacDialog.open" max-width="440">
      <v-card rounded="lg">
        <v-card-title class="pa-4">
          <v-icon start color="green-darken-2">mdi-beach</v-icon>
          {{ vacDialog.editing ? 'Modifier la période' : 'Nouvelle période de vacances' }}
        </v-card-title>
        <v-card-text>
          <v-text-field v-model="vacForm.label" :label="`Libellé (ex: Noël ${selectedYear})`" variant="outlined" density="compact" class="mb-2" />
          <v-row dense>
            <v-col cols="6">
              <v-text-field v-model="vacForm.startDate" label="Début" type="date" variant="outlined" density="compact" />
            </v-col>
            <v-col cols="6">
              <v-text-field v-model="vacForm.endDate" label="Fin" type="date" variant="outlined" density="compact"
                :error-messages="vacForm.startDate && vacForm.endDate && vacForm.endDate < vacForm.startDate ? 'Avant le début' : ''" />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="vacDialog.open = false">Annuler</v-btn>
          <v-btn
            color="green-darken-2" variant="flat" :loading="isSavingVacance"
            :disabled="!vacForm.startDate || !vacForm.endDate || vacForm.endDate < vacForm.startDate"
            @click="submitVacance"
          >{{ vacDialog.editing ? 'Mettre à jour' : 'Ajouter' }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ════════ Confirmation suppression ════════ -->
    <v-dialog v-model="confirmDel.open" max-width="400">
      <v-card rounded="lg">
        <v-card-title class="pa-4">Supprimer la session ?</v-card-title>
        <v-card-text>
          <strong>{{ confirmDel.session?.name }}</strong> sera supprimée. Les registres de présence qui s'y réfèrent ne trouveront plus ses dates.
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="confirmDel.open = false">Annuler</v-btn>
          <v-btn color="error" variant="flat" @click="deleteSession">Supprimer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar -->
    <v-snackbar v-model="snackbar.visible" :color="snackbar.color" timeout="2500" location="bottom right">
      <v-icon start>{{ snackbar.color === 'success' ? 'mdi-check-circle' : 'mdi-alert-circle' }}</v-icon>
      {{ snackbar.text }}
    </v-snackbar>

  </v-container>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'

moment.locale('fr')

// ─── Constantes ───────────────────────────────────────────────────────────────
const types = [
  { value: 'fi_a1',     label: 'F&I A1',    short: 'A1', icon: 'mdi-school',         color: 'blue', kind: 'fi' },
  { value: 'fi_a2',     label: 'F&I A2',    short: 'A2', icon: 'mdi-school-outline', color: 'indigo', kind: 'fi' },
  { value: 'telecours', label: 'Télécours', short: 'TC', icon: 'mdi-monitor',        color: 'teal', kind: 'tc' },
]
const statusMeta = {
  upcoming: { label: 'À venir',  color: 'primary' },
  current:  { label: 'En cours', color: 'success' },
  done:     { label: 'Terminée', color: 'grey' },
}
const today       = moment().format('YYYY-MM-DD')
const currentYear = moment().year()

// ─── State ────────────────────────────────────────────────────────────────────
const saveStatus       = ref('')   // 'saving' | 'saved' | 'error' | ''
const isSavingSession  = ref(false)
const isSavingVacance  = ref(false)
const sessions         = ref([])
const joursFeries      = ref([])   // YYYY-MM-DD, toutes années confondues
const periodesVacances = ref([])
const extraYears       = ref([])   // années ajoutées à la main, encore vides
const selectedYear     = ref(currentYear)
const archiveUnlocked  = ref(false)
const showFeriePicker  = ref(false)
const pickerYear       = ref(currentYear)
const pickerMonth      = ref(0)
const snackbar         = ref({ visible: false, text: '', color: 'success' })

const form          = ref(emptyForm())
const sessionDialog = ref({ open: false, editing: null })
const vacForm       = ref({ label: '', startDate: '', endDate: '' })
const vacDialog     = ref({ open: false, editing: null })
const confirmDel    = ref({ open: false, session: null })
const dup           = ref({ open: false, rows: [], vacances: [], includeVacances: true })

let _localCounter   = 0
let saveStatusTimer = null

// ─── Années ───────────────────────────────────────────────────────────────────
const yearOf = d => (d ? Number(String(d).substring(0, 4)) : null)

const allYears = computed(() => {
  const set = new Set([currentYear, currentYear + 1, ...extraYears.value])
  sessions.value.forEach(s => set.add(yearOf(s.startDate)))
  periodesVacances.value.forEach(v => set.add(yearOf(v.startDate)))
  joursFeries.value.forEach(f => set.add(yearOf(f)))
  set.delete(null)
  return [...set].sort((a, b) => a - b)
})
const activeYears    = computed(() => allYears.value.filter(y => y >= currentYear))
const archivedYears  = computed(() => allYears.value.filter(y => y < currentYear).reverse())
const maxYear        = computed(() => Math.max(...allYears.value))
const isArchivedYear = computed(() => selectedYear.value < currentYear)
const readOnly       = computed(() => isArchivedYear.value && !archiveUnlocked.value)

function countForYear(y) {
  return sessions.value.filter(s => yearOf(s.startDate) === y).length
}

function addYear() {
  const y = maxYear.value + 1
  extraYears.value.push(y)
  selectedYear.value = y
}

watch(selectedYear, y => {
  archiveUnlocked.value = false
  pickerYear.value  = y
  pickerMonth.value = y === currentYear ? moment().month() : 0
})

// ─── Données filtrées par année ───────────────────────────────────────────────
/** Sessions d'un type démarrant dans l'année ; `touching` inclut celles qui débordent sur l'année. */
function sessionsFor(type, year, touching = false) {
  return sessions.value
    .filter(s => s.type === type && (touching
      ? yearOf(s.startDate) <= year && yearOf(s.vacanceEnd || s.endDate) >= year
      : yearOf(s.startDate) === year))
    .sort((a, b) => a.startDate.localeCompare(b.startDate))
}

const yearSessionCount = computed(() => countForYear(selectedYear.value))

const yearVacances = computed(() =>
  periodesVacances.value
    .filter(v => yearOf(v.startDate) <= selectedYear.value && yearOf(v.endDate) >= selectedYear.value)
    .sort((a, b) => a.startDate.localeCompare(b.startDate))
)

const yearFeries = computed(() =>
  joursFeries.value.filter(f => yearOf(f) === selectedYear.value).sort()
)

const canDuplicate = computed(() =>
  countForYear(selectedYear.value - 1) > 0 && countForYear(selectedYear.value) < countForYear(selectedYear.value - 1)
)

// ─── Frise ────────────────────────────────────────────────────────────────────
const yearStart = computed(() => moment(`${selectedYear.value}-01-01`))
const yearDays  = computed(() => (moment([selectedYear.value]).isLeapYear() ? 366 : 365))

const months = computed(() =>
  Array.from({ length: 12 }, (_, i) => {
    const m = moment([selectedYear.value, i, 1])
    return {
      index: i,
      label: m.format('MMM'),
      left:  pos(m),
      width: (m.daysInMonth() / yearDays.value) * 100,
    }
  })
)

function pos(d) {
  const days = moment(d).startOf('day').diff(yearStart.value, 'days')
  return Math.min(100, Math.max(0, (days / yearDays.value) * 100))
}

function barStyle(start, end) {
  const l = pos(start)
  const r = pos(moment(end).add(1, 'day'))
  return { left: l + '%', width: Math.max(0.4, r - l) + '%' }
}

function shortName(s) {
  const m = /^S\d+/i.exec(s.name || '')
  return m ? m[0].toUpperCase() : s.name
}

// ─── Statuts / contrôles ──────────────────────────────────────────────────────
function statusOf(s) {
  const start = s.startDate.substring(0, 10)
  const end   = s.endDate.substring(0, 10)
  if (end < today) return 'done'
  if (start > today) return 'upcoming'
  return 'current'
}

function overlapsWith(candidate, excludeId) {
  if (!candidate.startDate || !candidate.endDate) return []
  const a0 = candidate.startDate.substring(0, 10)
  const a1 = candidate.endDate.substring(0, 10)
  return sessions.value.filter(s =>
    s._localId !== excludeId && s.type === candidate.type &&
    s.startDate.substring(0, 10) <= a1 && s.endDate.substring(0, 10) >= a0
  )
}
const overlaps = s => overlapsWith(s, s._localId)

function weeks(start, end) {
  if (!start || !end) return '—'
  return Math.round((moment(end).diff(moment(start), 'days') + 1) / 7)
}

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadSessions(), loadJoursFeries(), loadVacances()])
})

// ─── Helpers UI ───────────────────────────────────────────────────────────────
function showSaveStatus(status) {
  saveStatus.value = status
  clearTimeout(saveStatusTimer)
  if (status === 'saved') {
    saveStatusTimer = setTimeout(() => { saveStatus.value = '' }, 2500)
  }
}

function toast(text, color = 'success') {
  snackbar.value = { visible: true, text, color }
}

// Vuetify peut retourner des objets Date, des strings ISO avec timezone, ou YYYY-MM-DD.
// On normalise toujours en YYYY-MM-DD pour éviter les décalages de timezone.
function normalizeDate(d) {
  if (!d) return null
  if (typeof d === 'string') return d.substring(0, 10)
  if (d instanceof Date) {
    const y = d.getFullYear()
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${y}-${m}-${day}`
  }
  return moment(d).format('YYYY-MM-DD')
}

const toDay = d => (d ? String(d).substring(0, 10) : '')

// ─── API Load ─────────────────────────────────────────────────────────────────
async function loadSessions() {
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/sessions`)
    if (res?.ok) {
      const data = await res.json()
      sessions.value = data.map(s => ({
        ...s,
        startDate:    toDay(s.startDate),
        endDate:      toDay(s.endDate),
        vacanceStart: toDay(s.vacanceStart),
        vacanceEnd:   toDay(s.vacanceEnd),
        _localId:     ++_localCounter,
      }))
    }
  } catch (e) { console.error('loadSessions:', e) }
}

async function loadJoursFeries() {
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/feries`)
    if (res?.ok) {
      const data = await res.json()
      joursFeries.value = data.map(f => f.date.substring(0, 10))
    }
  } catch (e) { console.error('loadJoursFeries:', e) }
}

async function loadVacances() {
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/vacances`)
    if (res?.ok) {
      const data = await res.json()
      periodesVacances.value = data.map(v => ({
        ...v,
        startDate: toDay(v.startDate),
        endDate:   toDay(v.endDate),
        _localId:  ++_localCounter,
      }))
    }
  } catch (e) { console.error('loadVacances:', e) }
}

// ─── Sessions : dialogue ──────────────────────────────────────────────────────
function emptyForm(type = 'fi_a1') {
  return { type, name: '', startDate: '', endDate: '', vacanceStart: '', vacanceEnd: '' }
}

function suggestName(type, startDate) {
  const year = yearOf(startDate) || selectedYear.value
  const n = sessions.value.filter(s =>
    s.type === type && yearOf(s.startDate) === year && s._localId !== sessionDialog.value.editing?._localId
  ).length + 1
  return `S${n}-${year}-${type.toUpperCase()}`
}

/** Propose le début de la prochaine session : lundi suivant la fin (ou les vacances) de la dernière session du type. */
function suggestNextStart(type) {
  const prev = sessions.value
    .filter(s => s.type === type)
    .sort((a, b) => b.startDate.localeCompare(a.startDate))[0]
  if (!prev) return ''
  const after = moment(prev.vacanceEnd || prev.endDate).add(1, 'day')
  const monday = after.isoWeekday() === 1 ? after : after.add(8 - after.isoWeekday(), 'days')
  return yearOf(monday.format('YYYY-MM-DD')) === selectedYear.value ? monday.format('YYYY-MM-DD') : ''
}

function prefillNew(type) {
  const f = emptyForm(type)
  f.startDate = suggestNextStart(type)
  f.name = suggestName(type, f.startDate)
  form.value = f
}

function openSessionDialog(sess, type) {
  if (readOnly.value) return
  if (sess) {
    sessionDialog.value = { open: true, editing: sess }
    form.value = {
      type:         sess.type,
      name:         sess.name,
      startDate:    toDay(sess.startDate),
      endDate:      toDay(sess.endDate),
      vacanceStart: toDay(sess.vacanceStart),
      vacanceEnd:   toDay(sess.vacanceEnd),
    }
  } else {
    sessionDialog.value = { open: true, editing: null }
    prefillNew(type || form.value.type || 'fi_a1')
  }
}

function onFormTypeChange(type) {
  if (sessionDialog.value.editing) return
  prefillNew(type)
}

function onFormStartChange(v) {
  // Garde le nom auto tant qu'il n'a pas été personnalisé
  if (!sessionDialog.value.editing && /^S\d+-\d{4}-/.test(form.value.name)) {
    form.value.name = suggestName(form.value.type, v)
  }
}

const formErrors = computed(() => {
  const f = form.value
  return {
    end: f.startDate && f.endDate && f.endDate < f.startDate ? 'Avant le début' : '',
    vacance: (f.vacanceStart && !f.vacanceEnd) || (!f.vacanceStart && f.vacanceEnd)
      ? 'Saisir début et fin'
      : f.vacanceStart && f.vacanceEnd < f.vacanceStart ? 'Avant le début' : '',
  }
})
const formValid    = computed(() =>
  form.value.name && form.value.startDate && form.value.endDate && !formErrors.value.end && !formErrors.value.vacance
)
const formOverlaps = computed(() => overlapsWith(form.value, sessionDialog.value.editing?._localId))

async function submitSession(keepOpen) {
  const f = { ...form.value, vacanceStart: f0(form.value.vacanceStart), vacanceEnd: f0(form.value.vacanceEnd) }
  const editing = sessionDialog.value.editing
  if (editing) {
    const idx = sessions.value.findIndex(s => s._localId === editing._localId)
    if (idx >= 0) sessions.value[idx] = { ...sessions.value[idx], ...f }
  } else {
    sessions.value.push({ ...f, _localId: ++_localCounter })
  }
  const ok = await saveSessions()
  if (!ok) return
  if (keepOpen) prefillNew(f.type)
  else sessionDialog.value.open = false
}

const f0 = v => v || null

function askDelete(sess) {
  confirmDel.value = { open: true, session: sess }
}

async function deleteSession() {
  const sess = confirmDel.value.session
  confirmDel.value.open = false
  sessionDialog.value.open = false
  sessions.value = sessions.value.filter(s => s._localId !== sess._localId)
  await saveSessions()
}

async function saveSessions() {
  isSavingSession.value = true
  showSaveStatus('saving')
  try {
    const payload = sessions.value.map(({ _localId, ...rest }) => ({
      ...rest,
      vacanceStart: rest.vacanceStart || null,
      vacanceEnd:   rest.vacanceEnd   || null,
    }))
    const res = await apiFetch(`${API_BASE}/api/calendrier/sessions`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
    if (res?.ok) {
      showSaveStatus('saved')
      toast('Sessions enregistrées ✓')
      await loadSessions()
      return true
    }
    showSaveStatus('error')
    toast('Erreur lors de l\'enregistrement', 'error')
  } catch (e) {
    console.error(e)
    showSaveStatus('error')
    toast('Erreur serveur', 'error')
  } finally {
    isSavingSession.value = false
  }
  await loadSessions()
  return false
}

// ─── Duplication année précédente ─────────────────────────────────────────────
const shift = d => (d ? moment(d).add(52, 'weeks').format('YYYY-MM-DD') : '')

function openDuplicateDialog() {
  const from = selectedYear.value - 1
  const to   = selectedYear.value
  const already = new Set(sessions.value.filter(s => yearOf(s.startDate) === to).map(s => s.name))
  dup.value = {
    open: true,
    rows: sessions.value
      .filter(s => yearOf(s.startDate) === from)
      .sort((a, b) => a.type.localeCompare(b.type) || a.startDate.localeCompare(b.startDate))
      .map(s => {
        const name = s.name.replace(String(from), String(to))
        return {
          key:          s._localId,
          include:      !already.has(name),
          type:         s.type,
          name,
          startDate:    shift(s.startDate),
          endDate:      shift(s.endDate),
          vacanceStart: shift(s.vacanceStart),
          vacanceEnd:   shift(s.vacanceEnd),
        }
      }),
    vacances: periodesVacances.value.filter(v => yearOf(v.startDate) === from),
    includeVacances: !periodesVacances.value.some(v => yearOf(v.startDate) === to),
  }
}

async function confirmDuplicate() {
  const rows = dup.value.rows.filter(r => r.include && r.name && r.startDate && r.endDate)
  rows.forEach(({ key, include, ...r }) => sessions.value.push({ ...r, _localId: ++_localCounter }))
  if (dup.value.includeVacances && dup.value.vacances.length) {
    const from = String(selectedYear.value - 1)
    const to   = String(selectedYear.value)
    dup.value.vacances.forEach(v => periodesVacances.value.push({
      label:     (v.label || '').replace(from, to),
      startDate: shift(v.startDate),
      endDate:   shift(v.endDate),
      _localId:  ++_localCounter,
    }))
    await saveVacances()
  }
  const ok = await saveSessions()
  if (ok) {
    dup.value.open = false
    toast(`${rows.length} session(s) créée(s) pour ${selectedYear.value}`)
  }
}

// ─── Fériés ───────────────────────────────────────────────────────────────────
function onFeriesChange(newVal) {
  joursFeries.value = [...new Set((newVal || []).map(normalizeDate).filter(Boolean))]
  saveFeries()
}

function removeFerie(d) {
  joursFeries.value = joursFeries.value.filter(x => x !== d)
  saveFeries()
}

async function saveFeries() {
  showSaveStatus('saving')
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/feries`, {
      method: 'PUT',
      body: JSON.stringify(joursFeries.value),
    })
    if (res?.ok) { showSaveStatus('saved'); toast('Jours fériés enregistrés ✓') }
    else         { showSaveStatus('error'); toast('Erreur lors de l\'enregistrement', 'error') }
  } catch (e) { showSaveStatus('error'); console.error(e) }
}

// ─── Vacances générales ───────────────────────────────────────────────────────
function openVacanceDialog(v) {
  vacDialog.value = { open: true, editing: v }
  vacForm.value = v
    ? { label: v.label, startDate: v.startDate, endDate: v.endDate }
    : { label: '', startDate: '', endDate: '' }
}

async function submitVacance() {
  const editing = vacDialog.value.editing
  if (editing) Object.assign(editing, vacForm.value)
  else periodesVacances.value.push({ ...vacForm.value, _localId: ++_localCounter })
  vacDialog.value.open = false
  await saveVacances()
}

async function deleteVacance(v) {
  periodesVacances.value = periodesVacances.value.filter(x => x._localId !== v._localId)
  await saveVacances()
}

async function saveVacances() {
  isSavingVacance.value = true
  showSaveStatus('saving')
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier/vacances`, {
      method: 'PUT',
      body: JSON.stringify(periodesVacances.value.map(({ _localId, ...rest }) => rest)),
    })
    if (res?.ok) { showSaveStatus('saved'); toast('Vacances enregistrées ✓') }
    else         { showSaveStatus('error'); toast('Erreur lors de l\'enregistrement', 'error') }
  } catch (e) { showSaveStatus('error'); console.error(e) }
  finally {
    isSavingVacance.value = false
    await loadVacances()   // récupère les ids des nouvelles périodes
  }
}

// ─── Formatage ────────────────────────────────────────────────────────────────
const fmtDate  = d => (d ? moment(d).format('DD/MM/YYYY') : '—')
const fmtShort = d => (d ? moment(d).format('D MMM YYYY') : '—')
const fmtFerie = d => moment(d).format('ddd D MMM')
</script>

<style scoped>
.gap-1 { gap: 4px; }
.gap-2 { gap: 8px; }
.gap-3 { gap: 12px; }

.year-btn { min-width: 84px; font-weight: 600; }

/* ── Légende ── */
.legend span { display: inline-flex; align-items: center; }
.lg { display: inline-block; width: 14px; height: 10px; border-radius: 3px; margin-right: 6px; }
.lg--fi { background: #1e66d0; }
.lg--tc { background: #00897b; }
.lg--vs { background: #a5d6a7; }
.lg--vg { background: repeating-linear-gradient(45deg, #c8e6c9 0 3px, #e8f5e9 3px 6px); border: 1px solid #a5d6a7; }
.lg--fe { width: 3px; height: 12px; background: #ef8f00; border-radius: 1px; }

/* ── Frise ── */
.timeline { overflow-x: auto; }
.tl-row { display: flex; align-items: center; min-width: 720px; }
.tl-row + .tl-row { margin-top: 6px; }
.tl-label {
  width: 110px; flex-shrink: 0;
  font-size: 0.8rem; font-weight: 600; color: #2d3748;
  display: flex; align-items: center;
}
.tl-track { position: relative; flex: 1; height: 20px; }
.tl-track--body { height: 34px; background: #f7f8fb; border-radius: 6px; }
.tl-month {
  position: absolute; top: 0;
  font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.04em; color: #8a94a6;
  padding-left: 4px; border-left: 1px solid #e2e6ee;
}
.tl-grid { position: absolute; top: 0; bottom: 0; border-left: 1px solid #e8ebf2; }
.tl-band {
  position: absolute; top: 0; bottom: 0;
  background: repeating-linear-gradient(45deg, #c8e6c9 0 4px, #e8f5e9 4px 8px);
  opacity: 0.9;
}
.tl-ferie { position: absolute; top: 2px; bottom: 2px; width: 3px; margin-left: -1px; background: #ef8f00; border-radius: 1px; z-index: 3; }
.tl-bar {
  position: absolute; top: 5px; bottom: 5px;
  border-radius: 5px; color: #fff; cursor: pointer; z-index: 2;
  display: flex; align-items: center; padding: 0 6px; overflow: hidden;
  box-shadow: 0 1px 2px rgba(0,0,0,0.15);
  transition: transform 0.1s, box-shadow 0.1s;
}
.tl-bar:hover { transform: translateY(-1px); box-shadow: 0 3px 8px rgba(0,0,0,0.2); }
.tl-bar--fi { background: #1e66d0; }
.tl-bar--tc { background: #00897b; }
.tl-bar--past { opacity: 0.5; }
.tl-bar__text { font-size: 0.72rem; font-weight: 700; white-space: nowrap; }
.tl-vac { position: absolute; top: 11px; bottom: 11px; background: #a5d6a7; border-radius: 3px; z-index: 1; }
.tl-today { position: absolute; top: -4px; bottom: -4px; width: 2px; background: #e53935; z-index: 4; }

/* ── Tableau sessions ── */
.sessions-table th {
  font-size: 0.72rem !important;
  color: #718096 !important;
  font-weight: 700 !important;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.group-row td { background: #f4f6fa; height: 38px !important; }
.session-tr { cursor: pointer; }
.session-tr:hover td { background: #f0f4ff; }
.session-tr--done td { color: #9aa3b2; }

/* ── Listes latérales ── */
.item-list { display: flex; flex-direction: column; gap: 6px; }
.item-row {
  display: flex; align-items: center;
  padding: 8px 10px; border-radius: 8px;
  border: 1px solid #e8e8e8; background: #fafafa;
}

/* ── Dialogue duplication ── */
.cell-input {
  width: 100%; padding: 4px 6px;
  border: 1px solid #d5dbe5; border-radius: 4px; font-size: 0.82rem;
  background: #fff;
}
.cell-input:disabled { background: transparent; border-color: transparent; }
.dup-table td { padding: 2px 6px !important; }
</style>
