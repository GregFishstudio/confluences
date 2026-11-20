<template>
  <v-container fluid class="pa-4">
    <div class="d-flex align-center mb-6">
      <v-btn icon variant="text" class="mr-2" title="Retour" @click="retourOuListe(router, 'Participants')">
        <v-icon>mdi-arrow-left</v-icon>
      </v-btn>
      <v-avatar size="48" :color="avatarColor" class="mr-4">
        <span class="text-subtitle-1 font-weight-bold text-white">{{ initials }}</span>
      </v-avatar>
      <div>
        <div class="text-h5 font-weight-bold">{{ user.prenom }} {{ user.nom }}</div>
        <div class="text-caption text-grey">{{ user.userName }}</div>
      </div>
      <v-spacer />
      <!-- Marrainage en cours : visible dès l'ouverture de la fiche -->
      <div v-if="!loading && marrainagesActifs.length" class="d-flex flex-column align-end ga-1">
        <v-chip
          v-for="m in marrainagesActifs" :key="m.marrainageId"
          color="pink-darken-1" variant="flat" size="large" prepend-icon="mdi-account-heart"
          title="Voir la fiche du parrain / de la marraine" @click="goToMarrainage(m.marrainageId)"
        >
          Marrainage en cours avec {{ m.prenom }} {{ m.nom }}<span v-if="m.dateDebut">&nbsp;· depuis {{ formatMoisAnnee(m.dateDebut) }}</span>
        </v-chip>
      </div>
    </div>

    <!-- Un seul endroit pour enregistrer : état de la fiche + bouton Enregistrer -->
    <PageSaveBar
      v-if="!loading" :state="pageState" :error="autosaveError" :draft-count="draftCount"
      :last-saved-at="lastSavedAt" autosave @save="saveAll"
    />

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
              <DateTextField v-model="form.birthday" label="Date de naissance" variant="outlined" density="compact" />
            </v-col>
            <v-col cols="6" md="3">
              <v-select v-model="form.genderId" :items="genderOptions" item-title="title" item-value="value" label="Genre" variant="outlined" density="compact" clearable />
            </v-col>

            <v-col cols="6" md="3"><v-text-field v-model="form.nationality" label="Pays d'origine" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3"><v-combobox
              v-model="form.language" :items="LANGUES" label="Langue maternelle" variant="outlined" density="compact" clearable
              hint="Tapez les premières lettres ; une langue absente de la liste peut être saisie librement." persistent-hint
            /></v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.autresLangues" label="Autre(s) langue(s)" variant="outlined" density="compact" /></v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.phoneNumber" label="Téléphone" variant="outlined" density="compact" /></v-col>

            <!-- Adresse structurée : rue + n° / NPA / localité -->
            <v-col cols="12" md="4"><v-text-field v-model="form.rue" label="Rue et n°" variant="outlined" density="compact" /></v-col>
            <v-col cols="4" md="2">
              <v-text-field
                v-model="form.npa" label="NPA" variant="outlined" density="compact" maxlength="4" inputmode="numeric"
                :rules="[npaRule]" @update:model-value="completerLocalite"
              />
            </v-col>
            <v-col cols="8" md="3">
              <v-combobox
                v-model="form.localite" :items="localiteOptions" label="Localité" variant="outlined" density="compact"
                :rules="[localiteRule]"
              />
            </v-col>
            <v-col cols="12" md="3">
              <v-select
                v-model="form.region" :items="regionNames" label="Région" variant="outlined" density="compact" clearable
                :placeholder="regionSuggeree || ''" persistent-placeholder
                :hint="!form.region && regionSuggeree ? `Déduite de la localité : ${regionSuggeree}` : ''" persistent-hint
              />
            </v-col>
            <v-col v-if="adresseACompleter" cols="12">
              <v-alert type="warning" variant="tonal" density="compact" icon="mdi-map-marker-alert-outline">
                Adresse à compléter — ancienne saisie : « {{ form.adresse }} ». Reportez-la dans les champs
                Rue et n°, NPA et Localité ci-dessus.
              </v-alert>
            </v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.email" type="email" label="Email" variant="outlined" density="compact" :rules="[emailRule]" /></v-col>
            <v-col cols="6" md="3"><v-text-field v-model="form.etatCivil" label="État civil" variant="outlined" density="compact" /></v-col>

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

            <!-- Modifiable ici aussi : une personne qui n'est plus inscrite dans une session en cours
                 n'apparaît plus dans le tableau d'Inscriptions, où seul il se cochait. -->
            <v-col cols="12" md="6" class="d-flex align-center ga-2">
              <v-switch
                :model-value="!!form.contratMaori" color="red" hide-details density="compact" class="flex-grow-0"
                :label="`Contrat Maori : ${form.contratMaori ? 'reçu' : 'non reçu'}`" :loading="savingMaori"
                @update:model-value="toggleContratMaori"
              />
              <FieldHistoryDialog
                v-if="user.id"
                entity-type="ApplicationUser" :entity-id="user.id" field-name="ContratMaori"
                label="Contrat Maori"
              />
            </v-col>
          </v-row>
        </v-card>
      </v-col>

      <!-- ── Rouge : mesures suivies ── -->
      <v-col cols="12">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4 section-red" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-4 text-red-darken-2">Mesures suivies</div>

          <v-row dense class="mb-2">
            <!-- Calculés d'après Bilans & attestations (plus de saisie manuelle) -->
            <v-col cols="6" md="3">
              <v-text-field
                :model-value="niveauxContrat.debut || '—'" label="Niveau début du contrat" variant="outlined" density="compact"
                readonly :hint="niveauxContrat.debutSource" persistent-hint
              />
            </v-col>
            <v-col cols="6" md="3">
              <v-text-field
                :model-value="niveauxContrat.fin || '—'" label="Niveau fin du contrat" variant="outlined" density="compact"
                readonly :hint="niveauxContrat.finSource" persistent-hint
              />
            </v-col>
          </v-row>
          <v-divider class="mb-3" />

          <div v-if="!userSessionLinks.length" class="text-caption text-grey">Aucune session trouvée.</div>
          <v-card v-for="link in userSessionLinks" :key="link.sessionId + link.studentId" variant="outlined" class="pa-3 mb-3">
            <div class="d-flex align-center justify-space-between flex-wrap ga-2">
              <a
                v-if="sessionGroupKey(link)" href="#" class="text-body-1 font-weight-bold marrainage-link"
                title="Voir la fiche de présence de cette session"
                @click.prevent="goToSessionPage('AttendanceRegister', link)"
              >
                {{ sessionLabel(link) }}
              </a>
              <div v-else class="text-body-1 font-weight-bold">{{ sessionLabel(link) }}</div>
              <v-chip v-if="link === currentSessionLink && isActif" size="x-small" color="success" label variant="tonal">En cours</v-chip>
            </div>
            <div class="text-caption text-grey">
              {{ formatDate(link.session?.dateStart) }} → {{ formatDate(link.session?.dateEnd) }}
            </div>

            <div class="mt-2 d-flex align-center flex-wrap ga-1">
              <v-chip
                v-if="getNiveauForSession(link)" size="x-small" color="green" label
                :class="{ 'cursor-pointer': !!sessionGroupKey(link) }"
                title="Voir sous Bilans & attestations" @click="sessionGroupKey(link) && goToSessionPage('StagiairesFollowUp', link)"
              >Oral : {{ getNiveauForSession(link)?.oralLevel || '—' }}</v-chip>
              <v-chip
                v-if="getNiveauForSession(link)" size="x-small" color="blue" label
                :class="{ 'cursor-pointer': !!sessionGroupKey(link) }"
                title="Voir sous Bilans & attestations" @click="sessionGroupKey(link) && goToSessionPage('StagiairesFollowUp', link)"
              >Écrit : {{ getNiveauForSession(link)?.writtenLevel || '—' }}</v-chip>
              <v-chip v-if="link.tauxParticipation !== null" size="x-small" :color="tauxColor(link.tauxParticipation)" label>
                Participation : {{ link.tauxParticipation }}%
              </v-chip>
              <v-chip
                size="x-small" color="grey-darken-1" label variant="tonal"
                :class="{ 'cursor-pointer': sessionGroupKey(link) === 'fi_a1' || sessionGroupKey(link) === 'fi_a2' }"
                title="Voir les accompagnements" @click="(sessionGroupKey(link) === 'fi_a1' || sessionGroupKey(link) === 'fi_a2') && goToSessionPage('AteliersFollowUp', link)"
              >
                Accomp. : {{ accompCounts[link.sessionId]?.acco ?? 0 }}
              </v-chip>
              <v-chip
                size="x-small" color="grey-darken-1" label variant="tonal"
                :class="{ 'cursor-pointer': sessionGroupKey(link) === 'fi_a1' || sessionGroupKey(link) === 'fi_a2' }"
                title="Voir les ateliers" @click="(sessionGroupKey(link) === 'fi_a1' || sessionGroupKey(link) === 'fi_a2') && goToSessionPage('AteliersFollowUp', link)"
              >
                Ateliers : {{ accompCounts[link.sessionId]?.atelier ?? 0 }}
              </v-chip>
            </div>

            <div class="mt-2 d-flex ga-2">
              <v-btn v-if="bilanAvailable[link.sessionId]" size="small" variant="tonal" color="primary" prepend-icon="mdi-file-pdf-box" @click="openProtected(sessionDocUrl('bilan-session', link))">
                Bilan
              </v-btn>
              <v-btn size="small" variant="tonal" color="primary" prepend-icon="mdi-file-pdf-box" @click="openProtected(sessionDocUrl('attestation-session', link))">
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

          <div class="d-flex align-center mb-1">
            <div class="text-caption text-grey">Expériences professionnelles</div>
            <v-spacer />
            <div v-if="user.id"><CreateStage :stagiaire-id="user.id" button-size="small" /></div>
          </div>
          <div v-if="!userStages.length" class="text-caption text-grey mb-3">Aucune expérience professionnelle.</div>
          <!-- Métier / intitulé, type (stage découverte, bénévolat…) et statut (en cours, terminé…) -->
          <div v-for="stage in userStagesTries" :key="stage.id" class="experience mb-2" :class="`experience-${statutExperience(stage).key}`">
            <div class="d-flex align-center ga-2 flex-wrap">
              <a href="#" class="font-weight-bold marrainage-link" title="Voir cette expérience professionnelle" @click.prevent="goToStage(stage.id)">
                {{ intituleExperience(stage) }}
              </a>
              <v-chip v-if="stage.type" size="x-small" label color="indigo" variant="tonal">{{ stage.type }}</v-chip>
              <v-chip size="x-small" label :color="statutExperience(stage).color" variant="flat">{{ statutExperience(stage).label }}</v-chip>
              <v-chip v-if="stage.typeStage" size="x-small" label variant="outlined" title="Taux / rythme">{{ stage.typeStage }}</v-chip>
            </div>
            <div class="text-caption text-grey">
              <span v-if="stage.nom && stage.metier && stage.nom !== stage.metier">Métier : {{ stage.metier }} · </span>
              <a v-if="stage.entreprise?.nom" href="#" class="marrainage-link" title="Voir la fiche entreprise" @click.prevent="goToEntreprise(stage.entreprise.id)">{{ stage.entreprise.nom }}</a>
              <span v-else>Entreprise non renseignée</span>
              — {{ formatDate(stage.dateDebut) }}<span v-if="stage.dateFin"> → {{ formatDate(stage.dateFin) }}</span>
            </div>
          </div>

          <FileAttachments
            v-if="user.id"
            title="CV, lettres de motivation, certificats…"
            controller="ParticipantFiles"
            id-key="participantFileId"
            :extra-fields="{ studentId: user.id }"
            :category-options="INSERTION_CATEGORIES"
            :filter-categories="INSERTION_CATEGORIES"
            v-model="files"
            class="mb-2"
          />

          <v-divider class="my-3" />

          <div class="text-subtitle-2 font-weight-bold mb-2 d-flex align-center">
            <v-icon size="20" color="pink-darken-1" class="mr-1">mdi-account-heart</v-icon> Marrainage en cours
          </div>
          <div v-if="!marrainagesActifs.length" class="text-caption text-grey mb-2">Pas de marrainage en cours.</div>
          <div v-for="m in marrainagesActifs" :key="m.marrainageId" class="marrainage-actif mb-3">
            <div class="d-flex align-center flex-wrap ga-2">
              <v-chip color="success" variant="flat" size="small" label>En cours</v-chip>
              <span>Marrainage avec</span>
              <a href="#" class="marrainage-link font-weight-bold text-body-1" @click.prevent="goToMarrainage(m.marrainageId)">
                {{ m.prenom }} {{ m.nom }}
              </a>
              <span v-if="m.dateDebut" class="text-caption text-grey">depuis {{ formatMoisAnnee(m.dateDebut) }}</span>
              <v-spacer />
              <v-switch
                :model-value="m.actif"
                @update:model-value="(v) => updateRelationActif(m, v)"
                label="Actif" color="success" hide-details density="compact" class="flex-grow-0"
                title="Désactiver quand la relation s'arrête : elle passe dans l'historique, où l'on choisit le motif d'arrêt"
              />
            </div>
            <v-textarea
              v-model="m.commentaire" label="Commentaire sur la relation (visible aussi sur la fiche du parrain / de la marraine)"
              variant="outlined" density="compact" rows="1" auto-grow hide-details bg-color="white" class="mt-2"
              @blur="saveCommentaireRelation(m)"
            />
            <div v-if="conventionFiles[m.marrainageId]?.length" class="mt-1 d-flex flex-wrap ga-1">
              <v-chip
                v-for="f in conventionFiles[m.marrainageId]" :key="f.marrainageFileId"
                size="x-small" variant="outlined" prepend-icon="mdi-file-check-outline"
                @click="openProtected(`${API_BASE}/api/MarrainageFiles/${f.marrainageFileId}`)"
              >
                Convention signée
              </v-chip>
            </div>
          </div>

          <!-- Les relations terminées restent visibles ici, même quand un nouveau marrainage commence. -->
          <div class="text-caption text-grey mb-1 mt-3">Historique des marrainages</div>
          <div v-if="!marrainagesHistorique.length" class="text-caption text-grey">Aucun marrainage terminé.</div>
          <v-table v-else density="compact" class="historique-table">
            <thead>
              <tr>
                <th>Parrain / marraine</th>
                <th>Début</th>
                <th>Fin</th>
                <th>Motif de l'arrêt</th>
                <th>Commentaire</th>
                <th class="text-right">Réactiver</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="m in marrainagesHistorique" :key="m.marrainageId">
                <td>
                  <a href="#" class="marrainage-link font-weight-bold" @click.prevent="goToMarrainage(m.marrainageId)">{{ m.prenom }} {{ m.nom }}</a>
                </td>
                <td class="text-no-wrap">{{ formatMoisAnnee(m.dateDebut) || '—' }}</td>
                <td style="min-width: 230px">
                  <MoisAnneeField
                    :model-value="m.dateFin ? String(m.dateFin).substring(0, 10) : null" class="py-1"
                    @update:model-value="(v) => updateMarrainageRelation(m, { dateFin: v || null })"
                  />
                </td>
                <td style="min-width: 240px">
                  <div class="d-flex align-center ga-1 py-1">
                    <v-chip size="x-small" color="grey" variant="flat" label class="mr-1">Terminé</v-chip>
                    <v-select
                      :model-value="m.raisonInactivite"
                      @update:model-value="(v) => updateRaison(m, v)"
                      :items="raisonOptions"
                      placeholder="Raison de l'arrêt"
                      variant="outlined" density="compact" clearable hide-details
                      class="flex-grow-1"
                    />
                    <InactivityReasonManager :reasons="inactivityReasons" @changed="loadInactivityReasons" />
                  </div>
                </td>
                <td style="min-width: 220px">
                  <v-textarea
                    v-model="m.commentaire" placeholder="Commentaire" variant="outlined" density="compact"
                    rows="1" auto-grow hide-details class="py-1" @blur="saveCommentaireRelation(m)"
                  />
                </td>
                <td class="text-right">
                  <v-switch
                    :model-value="m.actif"
                    @update:model-value="(v) => updateRelationActif(m, v)"
                    color="success" hide-details density="compact" class="d-inline-flex"
                  />
                </td>
              </tr>
            </tbody>
          </v-table>

          <FileAttachments
            v-if="user.id"
            title="Conventions de marrainage du/de la participant·e"
            controller="ParticipantFiles"
            id-key="participantFileId"
            :extra-fields="{ studentId: user.id }"
            :fixed-category="CONVENTION_CATEGORY"
            :filter-category="CONVENTION_CATEGORY"
            v-model="files"
            class="mt-3"
          />
        </v-card>
      </v-col>

      <!-- ── Vert : suivi formatrices, en 6 rubriques ── -->
      <v-col cols="12">
        <v-card class="pa-4 elevation-1 rounded-lg mb-4 section-green" variant="outlined">
          <div class="d-flex align-center flex-wrap mb-4">
            <div class="text-subtitle-1 font-weight-bold text-green-darken-2">Suivi formatrices</div>
            <span v-if="user.profilUpdatedAt" class="text-caption text-grey ml-2">
              — mis à jour le {{ formatDate(user.profilUpdatedAt) }}<span v-if="user.profilUpdatedByName"> par {{ user.profilUpdatedByName }}</span>
            </span>
          </div>

          <!-- 1. Parcours en Suisse -->
          <section class="rubrique rubrique-teal">
            <div class="rubrique-title">
              <span class="rubrique-num">1</span>
              <v-icon size="20" class="mr-2">mdi-map-marker-outline</v-icon>
              Parcours en Suisse
            </div>
            <v-row dense>
              <v-col cols="6" md="3">
                <v-text-field v-model.number="form.anneeArriveeSuisse" type="number" label="Année d'arrivée en Suisse" variant="outlined" density="compact" />
              </v-col>
              <v-col cols="6" md="3">
                <v-text-field v-model.number="form.anneeObtentionPermis" type="number" label="Année d'obtention du permis" variant="outlined" density="compact" />
              </v-col>
              <v-col cols="6" md="3">
                <DateTextField v-model="form.dateObtentionPermis" label="Date exacte du permis (facultatif)" variant="outlined" density="compact" />
              </v-col>
              <v-col cols="12">
                <v-textarea v-model="form.parcoursFormationSuisse" label="Parcours de formation en Suisse" placeholder="Cours et formations suivis jusqu'à présent" variant="outlined" auto-grow rows="2" />
              </v-col>
              <v-col cols="12">
                <CommentLog v-if="user.id" :student-id="user.id" rubrique="parcours_suisse" label="Commentaires" :entries="commentsBy('parcours_suisse')" v-model:draft="drafts.parcours_suisse" @changed="loadComments" />
              </v-col>
            </v-row>
          </section>

          <!-- 2. Parcours de formation dans le pays d'origine -->
          <section class="rubrique rubrique-indigo">
            <div class="rubrique-title">
              <span class="rubrique-num">2</span>
              <v-icon size="20" class="mr-2">mdi-school-outline</v-icon>
              Parcours de formation dans le pays d'origine
            </div>
            <v-row dense>
              <v-col cols="12" md="6">
                <v-text-field v-model="form.domaineEtudeTitreObtenu" label="Domaine d'études / de formation" variant="outlined" density="compact" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="form.titresObtenus" label="Titres ou diplômes obtenus" variant="outlined" density="compact" />
              </v-col>
              <v-col cols="12" md="4">
                <v-select v-model="form.plusHautesEtudes" :items="etudesOptions" label="Degré / niveau d'études" variant="outlined" density="compact" clearable />
              </v-col>
              <v-col cols="12">
                <CommentLog v-if="user.id" :student-id="user.id" rubrique="formation_origine" label="Commentaires" :entries="commentsBy('formation_origine')" v-model:draft="drafts.formation_origine" @changed="loadComments" />
              </v-col>
            </v-row>
          </section>

          <!-- 3. Activité professionnelle dans le pays d'origine -->
          <section class="rubrique rubrique-orange">
            <div class="rubrique-title">
              <span class="rubrique-num">3</span>
              <v-icon size="20" class="mr-2">mdi-briefcase-outline</v-icon>
              Activité professionnelle dans le pays d'origine
            </div>
            <v-row dense>
              <v-col cols="12" md="4">
                <v-radio-group v-model="form.activiteProfOrigine" label="Activité professionnelle" inline hide-details density="compact">
                  <v-radio label="Oui" :value="true" />
                  <v-radio label="Non" :value="false" />
                </v-radio-group>
              </v-col>
              <v-col cols="12" md="8">
                <CommentLog
                  v-if="user.id" :student-id="user.id" rubrique="activite_origine" label="Commentaires : métier(s), expériences, durée, etc."
                  :entries="commentsBy('activite_origine')" v-model:draft="drafts.activite_origine" @changed="loadComments"
                />
              </v-col>
            </v-row>
          </section>

          <!-- 4. Expérience professionnelle préalable en Suisse -->
          <section class="rubrique rubrique-green">
            <div class="rubrique-title">
              <span class="rubrique-num">4</span>
              <v-icon size="20" class="mr-2">mdi-briefcase-check-outline</v-icon>
              Expérience professionnelle préalable en Suisse (avant Confluences)
            </div>
            <v-row dense>
              <v-col cols="12" md="4">
                <v-radio-group v-model="form.activiteProfSuisse" label="Expérience professionnelle" inline hide-details density="compact">
                  <v-radio label="Oui" :value="true" />
                  <v-radio label="Non" :value="false" />
                </v-radio-group>
              </v-col>
              <v-col cols="12" md="8">
                <CommentLog
                  v-if="user.id" :student-id="user.id" rubrique="activite_suisse" label="Commentaires : métier(s), expériences, durée, etc."
                  :entries="commentsBy('activite_suisse')" v-model:draft="drafts.activite_suisse" @changed="loadComments"
                />
              </v-col>
            </v-row>
          </section>

          <!-- 5. Projet professionnel / de formation -->
          <section class="rubrique rubrique-blue">
            <div class="rubrique-title">
              <span class="rubrique-num">5</span>
              <v-icon size="20" class="mr-2">mdi-flag-checkered</v-icon>
              Projet professionnel / de formation
            </div>
            <v-textarea v-model="form.projetProfessionnel" label="Projet professionnel / de formation" variant="outlined" auto-grow rows="2" hide-details />
          </section>

          <!-- 6. Besoins et souhaits en accompagnement social -->
          <section class="rubrique rubrique-purple">
            <div class="rubrique-title">
              <span class="rubrique-num">6</span>
              <v-icon size="20" class="mr-2">mdi-hand-heart-outline</v-icon>
              Besoins et souhaits en accompagnement social
            </div>
            <v-row dense class="besoin-row">
              <v-col cols="12" md="3" class="d-flex align-center">
                <span class="text-body-2 font-weight-medium mr-2">Français</span>
                <v-chip size="small" color="success" variant="tonal" label>Oui (d'office)</v-chip>
              </v-col>
              <v-col cols="12" md="9">
                <CommentLog v-if="user.id" :student-id="user.id" rubrique="francais" :entries="commentsBy('francais')" v-model:draft="drafts.francais" @changed="loadComments" />
              </v-col>
            </v-row>
            <v-row v-for="b in BESOINS" :key="b.key" dense class="besoin-row">
              <v-col cols="12" md="3">
                <div class="text-body-2 font-weight-medium mb-1">{{ b.label }}</div>
                <v-btn-toggle v-model="form[b.key]" density="compact" variant="outlined" divided class="tri-toggle">
                  <v-btn value="oui" size="small" :color="form[b.key] === 'oui' ? 'success' : undefined">Oui</v-btn>
                  <v-btn value="non" size="small">Non</v-btn>
                  <v-btn value="a_verifier" size="small" :color="form[b.key] === 'a_verifier' ? 'orange' : undefined">À vérifier</v-btn>
                </v-btn-toggle>
              </v-col>
              <v-col cols="12" md="9">
                <v-row v-if="b.rubrique === 'marrainage'" dense class="mb-1">
                  <v-col cols="12" md="4">
                    <v-select v-model="form.prioriteMarrainage" :items="PRIORITES_MARRAINAGE" label="Priorité" variant="outlined" density="compact" clearable hide-details bg-color="white" />
                  </v-col>
                  <v-col cols="12" md="4">
                    <v-select v-model="form.souhaitGenreMarrainage" :items="SOUHAITS_GENRE" label="Souhait : parrain / marraine" variant="outlined" density="compact" clearable hide-details bg-color="white" />
                  </v-col>
                  <v-col cols="12" md="4">
                    <v-select v-model="form.souhaitAgeMarrainage" :items="SOUHAITS_AGE" label="Souhait : âge" variant="outlined" density="compact" clearable hide-details bg-color="white" />
                  </v-col>
                </v-row>
                <template v-if="b.rubrique === 'stages'">
                  <div class="text-caption text-grey mb-1">Intérêts pour un stage ou du bénévolat (tableau stages &amp; bénévolat)</div>
                  <v-row dense class="mb-1">
                    <v-col cols="12" md="5">
                      <v-autocomplete
                        v-model="interets.metierIds" :items="metierOptions" item-title="libelle" item-value="typeMetierId"
                        label="Métiers visés" multiple chips closable-chips clear-on-select variant="outlined" density="compact" hide-details bg-color="white"
                        @update:model-value="saveInterets"
                      />
                    </v-col>
                    <v-col cols="12" md="4">
                      <v-autocomplete
                        v-model="interets.domaineIds" :items="domaineOptions" item-title="libelle" item-value="typeDomaineId"
                        label="Domaines d'intérêt" multiple chips closable-chips clear-on-select variant="outlined" density="compact" hide-details bg-color="white"
                        @update:model-value="saveInterets"
                      />
                    </v-col>
                    <v-col cols="12" md="3">
                      <v-select v-model="form.degreInteretStage" :items="DEGRES_INTERET" label="Degré d'intérêt" variant="outlined" density="compact" clearable hide-details bg-color="white" />
                    </v-col>
                  </v-row>
                </template>
                <template v-if="b.rubrique === 'benevolat'">
                  <div class="text-caption text-grey mb-1">Bénévolat – domaines d'intérêt (plusieurs choix possibles)</div>
                  <v-row dense class="mb-1">
                    <v-col cols="12" md="7">
                      <v-autocomplete
                        v-model="form.benevolatDomaines" :items="BENEVOLAT_DOMAINES"
                        label="Domaines d'intérêt" multiple chips closable-chips clear-on-select variant="outlined" density="compact" hide-details bg-color="white"
                      />
                    </v-col>
                    <v-col cols="12" md="5">
                      <v-autocomplete
                        v-model="form.benevolatTypesActivite" :items="BENEVOLAT_TYPES_ACTIVITE"
                        label="Type d'activité souhaité (facultatif)" multiple chips closable-chips clear-on-select variant="outlined" density="compact" hide-details bg-color="white"
                      />
                    </v-col>
                    <v-col v-if="form.benevolatDomaines.includes(AUTRE_DOMAINE)" cols="12" md="7">
                      <v-text-field
                        v-model="form.benevolatDomaineAutre" label="Autre domaine : à préciser" maxlength="200"
                        variant="outlined" density="compact" hide-details bg-color="white"
                      />
                    </v-col>
                  </v-row>
                </template>
                <CommentLog v-if="user.id" :student-id="user.id" :rubrique="b.rubrique" :entries="commentsBy(b.rubrique)" v-model:draft="drafts[b.rubrique]" @changed="loadComments" />
              </v-col>
            </v-row>
            <v-row dense class="besoin-row">
              <v-col cols="12">
                <v-textarea v-model="form.disponibilites" label="Disponibilités" placeholder="Jours, horaires, contraintes…" variant="outlined" auto-grow rows="1" hide-details bg-color="white" />
              </v-col>
            </v-row>
            <v-row dense class="besoin-row">
              <v-col cols="12">
                <CommentLog v-if="user.id" :student-id="user.id" rubrique="autre" label="Autres besoins / remarques" :entries="commentsBy('autre')" v-model:draft="drafts.autre" @changed="loadComments" />
              </v-col>
            </v-row>
          </section>

        </v-card>
      </v-col>

      <!-- ── Documents ── -->
      <v-col cols="12">
        <FileAttachments
          title="Documents"
          controller="ParticipantFiles"
          id-key="participantFileId"
          :extra-fields="{ studentId: user.id }"
          fixed-category="Autre"
          :exclude-categories="[...INSERTION_CATEGORIES, CONVENTION_CATEGORY]"
          v-model="files"
        />

        <v-card v-if="stageDocuments.length" class="pa-4 elevation-1 rounded-lg mt-4" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-2">Documents des expériences professionnelles</div>
          <div class="text-caption text-grey mb-3">
            Déposés sur la page de chaque expérience professionnelle (sous "Expériences professionnelles" dans le menu Insertion).
          </div>
          <div v-for="stage in userStages.filter(s => s.files?.length)" :key="stage.id" class="mb-3">
            <div class="text-caption text-grey mb-1">{{ stage.metier || stage.entreprise?.nom || 'Expérience professionnelle' }}</div>
            <div class="d-flex flex-wrap ga-1">
              <v-chip
                v-for="f in stage.files" :key="f.id" size="small" variant="outlined" prepend-icon="mdi-file-outline"
                closable close-label="Supprimer ce document"
                @click="openProtected(`${API_BASE}/api/StageFiles/${f.id}`)"
                @click:close="stageFileToDelete = { stage, file: f }"
              >
                {{ f.fileName }}
              </v-chip>
            </div>
          </div>
        </v-card>

        <!-- Suppression d'un document d'expérience professionnelle (ex. CV déposé par erreur) -->
        <v-dialog :model-value="!!stageFileToDelete" max-width="440" @update:model-value="v => { if (!v) stageFileToDelete = null }">
          <v-card v-if="stageFileToDelete" rounded="lg">
            <v-card-title class="py-4">Supprimer ce document ?</v-card-title>
            <v-divider />
            <v-card-text class="pt-4">
              « {{ stageFileToDelete.file.fileName }} » sera définitivement supprimé de l'expérience
              « {{ stageFileToDelete.stage.metier || stageFileToDelete.stage.entreprise?.nom || 'Expérience professionnelle' }} ».
              <v-alert v-if="stageFileDeleteError" type="error" variant="tonal" density="compact" class="mt-3">{{ stageFileDeleteError }}</v-alert>
            </v-card-text>
            <v-divider />
            <v-card-actions class="d-flex justify-end">
              <v-btn variant="text" color="grey" @click="stageFileToDelete = null">Annuler</v-btn>
              <v-btn color="error" :loading="deletingStageFile" @click="deleteStageFile">Supprimer</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-col>
    </v-row>

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { retourOuListe } from '@/composables/useListMemory'
import { useStore } from 'vuex'
import moment from 'moment/min/moment-with-locales'
import API_BASE, { apiFetch } from '@/services/api'
import { findSession } from '@/utils/sessionNumber'
import FileAttachments from '@/components/FileAttachments.vue'
import FieldHistoryDialog from '@/components/FieldHistoryDialog.vue'
import MoisAnneeField from '@/components/MoisAnneeField.vue'
import { debutDuMois } from '@/utils/dates'
import InactivityReasonManager from '@/components/InactivityReasonManager.vue'
import DateTextField from '@/components/DateTextField.vue'
import CreateStage from '@/components/CreateStage.vue'
import CommentLog from '@/components/CommentLog.vue'
import { confirmDiscard } from '@/composables/useUnsavedGuard'
import { useUnsavedGuard } from '@/composables/useUnsavedGuard'
import { LANGUES } from '@/utils/langues'
import PageSaveBar from '@/components/PageSaveBar.vue'
import { useRegions, regionDeCommune } from '@/utils/regions'
import { BENEVOLAT_DOMAINES, BENEVOLAT_TYPES_ACTIVITE, AUTRE_DOMAINE } from '@/utils/benevolat'
import { npaRule, localiteRule } from '@/utils/adresse'

moment.locale('fr')
const route = useRoute()
const router = useRouter()
const store = useStore()

const loading = ref(true)
const saving = ref(false)
const user = ref({})
const files = ref([])
// Les fichiers du/de la participant·e sont répartis par catégorie entre les sections de la fiche :
// CV/LM… sous "Expériences professionnelles", conventions sous "Marrainage", le reste dans
// "Documents" en bas de page.
const INSERTION_CATEGORIES = ['CV', 'LM', 'Certificat de travail', 'Contrat de stage', 'Attestation']
const CONVENTION_CATEGORY = 'Convention'
const allStages = ref([])
const allNiveaux = ref([])
const genders = ref([])
const marrainages = ref([])
// Actifs d'abord, mais toujours dans la même liste — le switch et le sélecteur de raison
// restent visibles après bascule en inactif (sinon impossible de revenir en arrière ou de
// choisir la raison une fois la relation passée "Anciens marrainages").
const marrainagesActifs = computed(() => marrainages.value.filter(m => m.actif))
// Relations terminées, la plus récente d'abord (date de fin, à défaut date de début).
const marrainagesHistorique = computed(() =>
  marrainages.value
    .filter(m => !m.actif)
    .sort((a, b) => new Date(b.dateFin || b.dateDebut || 0) - new Date(a.dateFin || a.dateDebut || 0))
)
const conventionFiles = ref({})

async function loadConventionFiles() {
  await Promise.all(marrainages.value.map(async (m) => {
    try {
      const res = await apiFetch(`${API_BASE}/api/MarrainageFiles?marrainageId=${m.marrainageId}&category=Convention`)
      if (res && res.ok) {
        conventionFiles.value[m.marrainageId] = (await res.json()).map(f => ({
          marrainageFileId: pick(f, 'marrainageFileId', 'MarrainageFileId'),
        }))
      }
    } catch (e) { console.error(e) }
  }))
}

// Raisons d'inactivité éditables par les formatrices (plus de liste figée dans le code).
const inactivityReasons = ref([])
const raisonOptions = computed(() => inactivityReasons.value.map(r => r.label))

async function loadInactivityReasons() {
  const res = await apiFetch(`${API_BASE}/api/MarrainageInactivityReasons`)
  if (res && res.ok) inactivityReasons.value = await res.json()
}

const etudesOptions = ['aucune', 'école primaire', 'école secondaire', 'gymnase', 'université']

// Rubrique 6 "Besoins et souhaits en accompagnement social" (hors français, demandé d'office) :
// une réponse Oui / Non / À vérifier + un fil de commentaires datés par besoin.
const BESOINS = [
  { label: 'Logement', key: 'rechercheLogement', rubrique: 'logement' },
  { label: 'Stage', key: 'besoinsSouhaitsStages', rubrique: 'stages' },
  { label: 'Bénévolat', key: 'besoinsSouhaitsBenevolat', rubrique: 'benevolat' },
  { label: 'Emploi', key: 'besoinsSouhaitsEmploi', rubrique: 'emploi' },
  { label: 'Formation', key: 'besoinsSouhaitsFormation', rubrique: 'formation' },
  { label: 'Activité sportive', key: 'besoinsSouhaitsSport', rubrique: 'sport' },
  { label: 'Marrainage', key: 'besoinsSouhaitsMarrainage', rubrique: 'marrainage' },
]
const SOUHAITS_GENRE = [
  { title: 'Un homme (parrain)', value: 'homme' },
  { title: 'Une femme (marraine)', value: 'femme' },
  { title: "C'est égal", value: 'egal' },
]
// Région de marrainage : saisie, sinon proposée d'après l'adresse (tableau de marrainage).
const { regionNames } = useRegions()
const regionSuggeree = computed(() => regionDeCommune(form.value.localite || form.value.adresse))

// ─── Adresse structurée ───
// Suggestions de localités = celles déjà saisies sur les fiches ; un NPA connu complète la localité.
const localites = ref([])
const localiteOptions = computed(() => [...new Set(localites.value.map(l => l.localite))].sort((a, b) => a.localeCompare(b, 'fr')))
async function loadLocalites() {
  const res = await apiFetch(`${API_BASE}/api/ApplicationUsers/localites`)
  if (res?.ok) localites.value = await res.json()
}
function completerLocalite(npa) {
  if (!/^\d{4}$/.test(npa || '') || form.value.localite) return
  const connue = localites.value.find(l => l.npa === npa)
  if (connue) form.value.localite = connue.localite
}
// Ancienne adresse en saisie libre, pas encore reportée dans les champs structurés.
const adresseACompleter = computed(() =>
  !!(form.value.adresse || '').trim() && !form.value.rue && !form.value.npa && !form.value.localite)

// Stages & bénévolat : métiers/domaines d'intérêt (mêmes listes que les entreprises), enregistrés
// à chaque changement — à part du formulaire, qui ne porte que le degré d'intérêt.
const DEGRES_INTERET = [
  { title: 'Très motivé·e', value: 'fort' },
  { title: 'Normal', value: 'normal' },
  { title: 'À confirmer', value: 'a_confirmer' },
]
const metierOptions = ref([])
const domaineOptions = ref([])
const interets = ref({ metierIds: [], domaineIds: [] })

async function loadInterets(id) {
  const [rI, rM, rD] = await Promise.all([
    apiFetch(`${API_BASE}/api/StagesBenevolat/interets/${id}`),
    metierOptions.value.length ? null : apiFetch(`${API_BASE}/api/TypeMetiers`),
    domaineOptions.value.length ? null : apiFetch(`${API_BASE}/api/TypeDomaines`),
  ])
  if (rI?.ok) interets.value = await rI.json()
  if (rM?.ok) metierOptions.value = (await rM.json()).filter(m => m.libelle)
  if (rD?.ok) domaineOptions.value = (await rD.json()).filter(d => d.libelle)
}

async function saveInterets() {
  const res = await apiFetch(`${API_BASE}/api/StagesBenevolat/interets/${user.value.id}`, {
    method: 'PUT',
    body: JSON.stringify(interets.value),
  })
  if (!res?.ok) store.dispatch('notification/add', { type: 'error', message: "Intérêts stage/bénévolat non enregistrés." })
}

const PRIORITES_MARRAINAGE = [
  { title: 'Très preneur·euse', value: 'haute' },
  { title: 'Normale', value: 'normale' },
  { title: 'Pas une priorité', value: 'basse' },
]
const SOUHAITS_AGE = [
  { title: 'Environ son âge', value: 'meme_age' },
  { title: 'Plus âgé·e', value: 'plus_age' },
  { title: 'Plus jeune', value: 'plus_jeune' },
  { title: "C'est égal", value: 'egal' },
]

// ─── Commentaires datés des rubriques ───
const comments = ref([])
const drafts = ref({})
const commentsBy = (rubrique) => comments.value.filter(c => c.rubrique === rubrique)
const hasDrafts = () => Object.values(drafts.value).some(d => d && d.trim())
async function loadComments() {
  if (!user.value.id) return
  const res = await apiFetch(`${API_BASE}/api/ParticipantComments?studentId=${encodeURIComponent(user.value.id)}`)
  if (res?.ok) comments.value = await res.json()
}

const emailRule = (v) => !v || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v) || 'Adresse e-mail invalide'

const genderOptions = computed(() =>
  genders.value.map(g => ({ title: g.GenderName ?? g.genderName, value: g.GenderId ?? g.genderId }))
)

const form = ref({
  firstname: null,
  lastName: null,
  email: null,
  phoneNumber: null,
  nationality: null,
  language: null,
  birthday: null,
  genderId: null,
  contratMaori: false,
  etatCivil: null,
  nombreEnfants: null,
  adresse: null,
  rue: null,
  npa: null,
  localite: null,
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
  activiteProfSuisse: null,
  activiteProfSuisseCommentaire: null,
  rechercheLogement: null,
  rechercheLogementCommentaire: null,
  besoinsSouhaitsStages: null,
  besoinsSouhaitsStagesCommentaire: null,
  besoinsSouhaitsBenevolat: null,
  besoinsSouhaitsBenevoletCommentaire: null,
  benevolatDomaines: [],
  benevolatDomaineAutre: null,
  benevolatTypesActivite: [],
  besoinsSouhaitsMarrainage: null,
  besoinsSouhaitsMarrainageCommentaire: null,
  dateObtentionPermis: null,
  anneeObtentionPermis: null,
  projetProfessionnel: null,
  besoinsSouhaitsSport: null,
  disponibilites: null,
  souhaitGenreMarrainage: null,
  souhaitAgeMarrainage: null,
  prioriteMarrainage: null,
  degreInteretStage: null,
  region: null,
  titresObtenus: null,
  activiteProfOrigine: null,
  besoinsSouhaitsEmploi: null,
  besoinsSouhaitsEmploiCommentaire: null,
  besoinsSouhaitsFormation: null,
  besoinsSouhaitsFormationCommentaire: null,
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
      const res = await apiFetch(`${API_BASE}/api/SessionStudents/${link.sessionId}/${link.studentId}/taux?force=true`, { method: 'POST' })
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
// Expériences : en cours d'abord, puis à venir, puis terminées (la plus récente d'abord).
const debutJour = () => { const d = new Date(); d.setHours(0, 0, 0, 0); return d }
function statutExperience(stage) {
  const today = debutJour()
  if (stage.dateDebut && new Date(stage.dateDebut) > today) return { key: 'prevu', label: 'Prévu', color: 'blue', ordre: 1 }
  if (stage.dateFin && new Date(stage.dateFin) < today) return { key: 'termine', label: 'Terminé', color: 'grey', ordre: 2 }
  return { key: 'en_cours', label: 'En cours', color: 'success', ordre: 0 }
}
const intituleExperience = (stage) => stage.nom || stage.metier || stage.entreprise?.nom || 'Expérience professionnelle'
const userStagesTries = computed(() => [...userStages.value].sort((a, b) =>
  statutExperience(a).ordre - statutExperience(b).ordre || new Date(b.dateDebut || 0) - new Date(a.dateDebut || 0)))
const stageDocuments = computed(() => userStages.value.flatMap(s => s.files || []))

// Suppression d'un document d'expérience professionnelle directement depuis la fiche.
const stageFileToDelete = ref(null)
const deletingStageFile = ref(false)
const stageFileDeleteError = ref('')
watch(stageFileToDelete, () => { stageFileDeleteError.value = '' })
async function deleteStageFile() {
  const { stage, file } = stageFileToDelete.value
  deletingStageFile.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/StageFiles/${file.id}`, { method: 'DELETE' })
    if (res?.ok || res?.status === 404) {
      stage.files = stage.files.filter(f => f.id !== file.id)
      stageFileToDelete.value = null
    } else {
      stageFileDeleteError.value = `Le document n'a pas pu être supprimé (${res ? `erreur ${res.status}` : 'session expirée'}).`
    }
  } catch {
    stageFileDeleteError.value = 'Le document n\'a pas pu être supprimé (pas de connexion au serveur).'
  } finally {
    deletingStageFile.value = false
  }
}

const getNiveauForSession = (link) => {
  const s = link.session
  const year = s ? new Date(s.dateStart).getFullYear() : null
  const num = s?.sessionNumberId
  if (!year || !num) return null
  return allNiveaux.value.find(n => n.stagiaireId === user.value.id && n.annee === year && n.trimestre === num) || null
}

// Niveaux du contrat (= dernière session suivie), d'après Bilans & attestations, qui note un
// niveau oral/écrit par session (celui atteint en fin de session) :
//  - début : niveau noté à la session suivie juste avant (vide pour un·e nouveau·elle — il
//    viendra plus tard de l'évaluation de la candidature) ;
//  - fin : niveau noté pour la dernière session (vide tant qu'elle n'est pas évaluée).
// Même règle pour les participant·e·s en cours et pour celles et ceux qui ont terminé.
const formatNiveau = (n) => {
  const oral = n?.oralLevel?.trim() || null
  const ecrit = n?.writtenLevel?.trim() || null
  if (!oral || !ecrit || oral === ecrit) return oral || ecrit
  return `Oral ${oral} / Écrit ${ecrit}`
}
const niveauxContrat = computed(() => {
  const seen = new Set()
  const sessions = sessionLinks.value.filter(l => {
    const k = `${getSessionYear(l)}-${l.session?.sessionNumberId}`
    if (seen.has(k)) return false
    seen.add(k)
    return true
  })
  const [derniere, precedente] = sessions
  const debut = precedente ? formatNiveau(getNiveauForSession(precedente)) : null
  const fin = derniere ? formatNiveau(getNiveauForSession(derniere)) : null
  return {
    debut,
    fin,
    debutSource: !derniere ? '' : precedente
      ? `Bilans & attestations — ${sessionLabel(precedente)}${debut ? '' : ' (pas de niveau noté)'}`
      : 'Nouveau·elle : sera repris de l\'évaluation de la candidature',
    finSource: !derniere ? '' : `Bilans & attestations — ${sessionLabel(derniere)}${fin ? '' : ' (pas encore évalué)'}`,
  }
})

const getSessionYear = (link) => link.session ? new Date(link.session.dateStart).getFullYear() : '—'

const accompCounts = ref({})
const bilanAvailable = ref({})
// Le calendrier (SessionFormation) porte les VRAIES dates de session utilisées pour dater les
// présences aux accomp./ateliers, les bilans et attestations — distinctes des dates de la table
// Session/SessionStudent, qui peuvent ne pas correspondre (même numéro de session, dates
// différentes). On résout donc les dates via le calendrier, comme AttendanceRegister/
// AccompanyingPresence/FollowUpPage, plutôt que de faire confiance à link.session.dateStart/dateEnd.
const calendrierSessions = ref([])

async function loadCalendrier() {
  try {
    const res = await apiFetch(`${API_BASE}/api/calendrier`)
    if (res && res.ok) calendrierSessions.value = (await res.json())?.sessions || []
  } catch (e) { console.error(e) }
}

// Retrouve, dans le calendrier, la session correspondant au groupe+numéro de link — même
// filtrage/tri que sur les pages cibles (par type, trié par date de début, n-ième session de
// l'année) — pour obtenir ses vraies dates de début/fin.
const resolveCalendarSession = (link) => {
  const group = sessionGroupKey(link)
  const num = link.session?.sessionNumberId
  if (!group || !num) return null
  const sessions = calendrierSessions.value
    .filter(s => (s.type || '').toLowerCase() === group && s.startDate)
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
  return findSession(sessions, getSessionYear(link), num)
}

const sessionLabel = (link) => {
  const room = (link.schoolClassRoomName || '').replace(/-/g, ' ').trim()
  const num = link.session?.sessionNumberId
  const year = getSessionYear(link)
  const sessionPart = num ? `Session ${num} ${year}` : null
  return [room, sessionPart].filter(Boolean).join(' – ') || 'Session'
}

// Déduit la clé de groupe ('telecours'/'fi_a1'/'fi_a2') utilisée par les pages Fiche de
// présence / Ateliers / Bilans à partir du nom de la mesure (ex. "Télécours-A2", "F&I-A1") —
// aucune de ces pages n'a de notion de "session id" unique, elles naviguent par groupe+numéro.
const sessionGroupKey = (link) => {
  const name = (link.schoolClassRoomName || '').toLowerCase()
  if (!name || !link.session?.sessionNumberId) return null
  if (name.includes('telecours') || name.includes('télécours')) return 'telecours'
  if (name.includes('a1')) return 'fi_a1'
  if (name.includes('a2')) return 'fi_a2'
  return null
}

const goToSessionPage = (routeName, link) => {
  const group = sessionGroupKey(link)
  if (!group) return
  const resolved = router.resolve({
    name: routeName,
    query: { studentId: user.value.id, group, quarter: link.session.sessionNumberId, year: getSessionYear(link) },
  })
  window.open(resolved.href, '_blank')
}

const sessionDocUrl = (kind, link) => {
  const year = getSessionYear(link)
  const num = link.session?.sessionNumberId
  const calSession = resolveCalendarSession(link)
  const params = new URLSearchParams({
    year: String(year),
    trimestre: String(num),
    mesure: link.schoolClassRoomName || '',
  })
  const startDate = calSession?.startDate || link.session?.dateStart
  const endDate = calSession?.endDate || link.session?.dateEnd
  if (startDate) params.set('startDate', startDate)
  if (endDate) params.set('endDate', endDate)
  return `${API_BASE}/api/documents/${kind}/${user.value.id}?${params.toString()}`
}

// Documents protégés par la connexion : téléchargés avec le jeton puis ouverts dans un nouvel
// onglet (un simple lien ne transmet pas le jeton et serait refusé par l'API).
async function openProtected(url) {
  const tab = window.open('', '_blank')
  try {
    const res = await apiFetch(url, { headers: { 'Content-Type': '' } })
    if (!res?.ok) throw new Error(res ? `erreur ${res.status}` : 'session expirée')
    const blobUrl = window.URL.createObjectURL(await res.blob())
    if (tab) tab.location.href = blobUrl
    else window.location.href = blobUrl
    setTimeout(() => window.URL.revokeObjectURL(blobUrl), 60000)
  } catch (e) {
    if (tab) tab.close()
    store.dispatch('notification/add', { type: 'error', message: `Document indisponible : ${e.message}` })
  }
}

async function loadSessionExtras() {
  await Promise.all(sessionLinks.value.map(async (link) => {
    const year = getSessionYear(link)
    const num = link.session?.sessionNumberId
    if (!year || !num) return
    try {
      const calSession = resolveCalendarSession(link)
      const params = new URLSearchParams({ year: String(year), quarter: String(num) })
      const startDate = calSession?.startDate || link.session?.dateStart
      const endDate = calSession?.endDate || link.session?.dateEnd
      if (startDate) params.set('startDate', startDate)
      if (endDate) params.set('endDate', endDate)
      const res = await apiFetch(`${API_BASE}/api/accompanying/summary/${user.value.id}?${params.toString()}`)
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
const formatMoisAnnee = (d) => d ? moment(d).format('MMMM YYYY') : ''

const goToMarrainage = (id) => {
  router.push({ name: 'Marrainage-Modifier', params: { id } })
}

const goToStage = (id) => {
  router.push({ name: 'Stage-Modifier', params: { id } })
}

const goToEntreprise = (id) => {
  if (!id) return
  router.push({ name: 'Entreprise-Modifier', params: { id } })
}

// Modifie la relation (MarrainageParticipant) propre à ce·tte participant·e — pas le statut
// global de la marraine, qui peut concerner d'autres filleul·e·s.
async function updateMarrainageRelation(marrainage, changes) {
  Object.assign(marrainage, changes)
  await apiFetch(`${API_BASE}/api/Marrainages/${marrainage.marrainageId}/participants/${marrainage.participantId}`, {
    method: 'PATCH',
    body: JSON.stringify({
      dateDebut: marrainage.dateDebut,
      dateFin: marrainage.dateFin,
      actif: marrainage.actif,
      raisonInactivite: marrainage.actif ? null : marrainage.raisonInactivite,
      commentaire: marrainage.commentaire ?? '',
    })
  })
  marrainage.commentaireEnregistre = marrainage.commentaire ?? ''
}

// Commentaire de la relation : enregistré en quittant le champ, s'il a changé. C'est le même
// commentaire que sur la fiche du parrain / de la marraine (même relation).
function saveCommentaireRelation(m) {
  if ((m.commentaire ?? '') === (m.commentaireEnregistre ?? '')) return
  updateMarrainageRelation(m, {})
}

// Contrat Maori : enregistré tout de suite (même enregistrement que le tableau d'Inscriptions).
const savingMaori = ref(false)
async function toggleContratMaori(value) {
  const previous = form.value.contratMaori
  form.value.contratMaori = value
  savingMaori.value = true
  try {
    const res = await apiFetch(`${API_BASE}/api/Inscriptions/participants/${user.value.id}/contrat-maori`, {
      method: 'PATCH',
      body: JSON.stringify({ contratMaori: value }),
    })
    if (!res?.ok) throw new Error()
    // Déjà enregistré : ne compte pas comme une modification en attente de la fiche.
    if (formSnapshot.value) {
      const snap = JSON.parse(formSnapshot.value)
      snap.contratMaori = value
      formSnapshot.value = JSON.stringify(snap)
    }
  } catch {
    form.value.contratMaori = previous
    store.dispatch('notification/add', { type: 'error', message: "Le contrat Maori n'a pas pu être enregistré." })
  } finally {
    savingMaori.value = false
  }
}

const updateRaison = (marrainage, raison) => updateMarrainageRelation(marrainage, { raisonInactivite: raison })
const updateRelationActif = (marrainage, actif) => updateMarrainageRelation(marrainage, {
  actif,
  dateFin: actif ? null : (marrainage.dateFin || debutDuMois()),
})

// AddNewtonsoftJson() applique par défaut un CamelCasePropertyNamesContractResolver
// côté API : les clés JSON sont donc en camelCase (firstname, sessionStudents,
// session.dateStart, ...), pas en PascalCase. On garde un `?? Pascal` en secours
// uniquement pour les endpoints qui, eux, projettent des objets déjà nommés
// explicitement (donc non affectés par le resolver).
const pick = (o, camel, pascal) => o?.[camel] ?? o?.[pascal] ?? null

function fillFormFromUser(u) {
  form.value.firstname                          = pick(u, 'firstname', 'Firstname')
  form.value.lastName                            = pick(u, 'lastName', 'LastName')
  form.value.email                               = pick(u, 'email', 'Email')
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
  form.value.rue                                 = pick(u, 'rue', 'Rue')
  form.value.npa                                 = pick(u, 'npa', 'Npa')
  form.value.localite                            = pick(u, 'localite', 'Localite')
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
  form.value.activiteProfSuisse                  = pick(u, 'activiteProfSuisse', 'ActiviteProfSuisse') ?? null
  form.value.activiteProfSuisseCommentaire       = pick(u, 'activiteProfSuisseCommentaire', 'ActiviteProfSuisseCommentaire')
  form.value.rechercheLogement                   = pick(u, 'rechercheLogement', 'RechercheLogement')
  form.value.rechercheLogementCommentaire        = pick(u, 'rechercheLogementCommentaire', 'RechercheLogementCommentaire')
  form.value.besoinsSouhaitsStages               = pick(u, 'besoinsSouhaitsStages', 'BesoinsSouhaitsStages')
  form.value.besoinsSouhaitsStagesCommentaire    = pick(u, 'besoinsSouhaitsStagesCommentaire', 'BesoinsSouhaitsStagesCommentaire')
  form.value.besoinsSouhaitsBenevolat            = pick(u, 'besoinsSouhaitsBenevolat', 'BesoinsSouhaitsBenevolat')
  form.value.besoinsSouhaitsBenevoletCommentaire = pick(u, 'besoinsSouhaitsBenevoletCommentaire', 'BesoinsSouhaitsBenevoletCommentaire')
  form.value.benevolatDomaines                   = pick(u, 'benevolatDomaines', 'BenevolatDomaines') || []
  form.value.benevolatDomaineAutre               = pick(u, 'benevolatDomaineAutre', 'BenevolatDomaineAutre')
  form.value.benevolatTypesActivite              = pick(u, 'benevolatTypesActivite', 'BenevolatTypesActivite') || []
  form.value.besoinsSouhaitsMarrainage           = pick(u, 'besoinsSouhaitsMarrainage', 'BesoinsSouhaitsMarrainage')
  form.value.besoinsSouhaitsMarrainageCommentaire = pick(u, 'besoinsSouhaitsMarrainageCommentaire', 'BesoinsSouhaitsMarrainageCommentaire')
  const datePermis = pick(u, 'dateObtentionPermis', 'DateObtentionPermis')
  form.value.dateObtentionPermis                 = datePermis ? moment(datePermis).format('YYYY-MM-DD') : null
  form.value.titresObtenus                       = pick(u, 'titresObtenus', 'TitresObtenus')
  form.value.anneeObtentionPermis                = pick(u, 'anneeObtentionPermis', 'AnneeObtentionPermis')
  form.value.projetProfessionnel                 = pick(u, 'projetProfessionnel', 'ProjetProfessionnel')
  form.value.besoinsSouhaitsSport                = pick(u, 'besoinsSouhaitsSport', 'BesoinsSouhaitsSport')
  form.value.disponibilites                      = pick(u, 'disponibilites', 'Disponibilites')
  form.value.souhaitGenreMarrainage              = pick(u, 'souhaitGenreMarrainage', 'SouhaitGenreMarrainage')
  form.value.prioriteMarrainage                  = pick(u, 'prioriteMarrainage', 'PrioriteMarrainage')
  form.value.degreInteretStage                   = pick(u, 'degreInteretStage', 'DegreInteretStage')
  form.value.region                              = pick(u, 'region', 'Region')
  form.value.souhaitAgeMarrainage                = pick(u, 'souhaitAgeMarrainage', 'SouhaitAgeMarrainage')
  form.value.activiteProfOrigine                 = pick(u, 'activiteProfOrigine', 'ActiviteProfOrigine') ?? null
  form.value.besoinsSouhaitsEmploi               = pick(u, 'besoinsSouhaitsEmploi', 'BesoinsSouhaitsEmploi')
  form.value.besoinsSouhaitsEmploiCommentaire    = pick(u, 'besoinsSouhaitsEmploiCommentaire', 'BesoinsSouhaitsEmploiCommentaire')
  form.value.besoinsSouhaitsFormation            = pick(u, 'besoinsSouhaitsFormation', 'BesoinsSouhaitsFormation')
  form.value.besoinsSouhaitsFormationCommentaire = pick(u, 'besoinsSouhaitsFormationCommentaire', 'BesoinsSouhaitsFormationCommentaire')
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

// Les dates/statut affichés viennent de la relation (MarrainageParticipant) propre à CE
// participant·e — pas des champs globaux de la marraine (Marrainage.Date/Actif), qui ne
// concernent que son propre profil et sont partagés entre tous ses filleul·e·s éventuels.
function normalizeMarrainage(m, participantId) {
  const links = pick(m, 'participantLinks', 'ParticipantLinks') || []
  const link = links.find(pl => (pick(pl, 'participantId', 'ParticipantId') || '').toLowerCase() === (participantId || '').toLowerCase())
  return {
    marrainageId: pick(m, 'marrainageId', 'MarrainageId'),
    participantId,
    prenom: pick(m, 'prenom', 'Prenom'),
    nom: pick(m, 'nom', 'Nom'),
    dateDebut: link ? pick(link, 'dateDebut', 'DateDebut') : pick(m, 'date', 'Date'),
    dateFin: link ? pick(link, 'dateFin', 'DateFin') : null,
    actif: link ? (pick(link, 'actif', 'Actif') ?? true) : pick(m, 'actif', 'Actif'),
    raisonInactivite: link ? pick(link, 'raisonInactivite', 'RaisonInactivite') : pick(m, 'raisonInactivite', 'RaisonInactivite'),
    commentaire: link ? (pick(link, 'commentaire', 'Commentaire') || '') : '',
    commentaireEnregistre: link ? (pick(link, 'commentaire', 'Commentaire') || '') : '',
  }
}

function normalizeStage(s) {
  const entreprise = pick(s, 'entreprise', 'Entreprise')
  const typeMetier = pick(s, 'typeMetier', 'TypeMetier')
  const typeStage = pick(s, 'typeStage', 'TypeStage')
  const typeActivite = pick(s, 'typeIntershipActivity', 'TypeIntershipActivity')
  const stageFiles = pick(s, 'stageFiles', 'StageFiles') || []
  return {
    id: pick(s, 'stageId', 'StageId'),
    stagiaireId: pick(s, 'stagiaireId', 'StagiaireId'),
    dateDebut: pick(s, 'debut', 'Debut'),
    dateFin: pick(s, 'fin', 'Fin'),
    nom: (pick(s, 'nom', 'Nom') || '').trim() || null,
    metier: typeMetier ? pick(typeMetier, 'libelle', 'Libelle') : null,
    type: typeActivite ? pick(typeActivite, 'nom', 'Nom') : null,
    typeStage: typeStage ? pick(typeStage, 'nom', 'Nom') : null,
    entreprise: entreprise ? { id: pick(entreprise, 'entrepriseId', 'EntrepriseId'), nom: pick(entreprise, 'nom', 'Nom') } : null,
    files: stageFiles.map(f => ({
      id: pick(f, 'stageFileId', 'StageFileId'),
      fileName: pick(f, 'fileName', 'FileName'),
    })),
  }
}

// ─── Sauvegarde automatique ───
// Les champs de la fiche s'enregistrent seuls ~2 s après la dernière modification. En quittant
// la page avec une modification pas encore partie, elle est enregistrée avant de partir ; un
// commentaire commencé mais pas encore ajouté déclenche un avertissement.
const formSnapshot = ref('')
const takeSnapshot = () => nextTick(() => { formSnapshot.value = JSON.stringify(form.value) })
const isDirty = () => !!formSnapshot.value && JSON.stringify(form.value) !== formSnapshot.value
const autosaveError = ref('')
const lastSavedAt = ref(null)
const savingDrafts = ref(false)
const draftCount = computed(() => Object.values(drafts.value).filter(d => d && d.trim()).length)

// État affiché par la barre d'enregistrement (PageSaveBar).
const pageState = computed(() => {
  if (saving.value || savingDrafts.value) return 'saving'
  if (autosaveError.value) return 'error'
  if (isDirty()) return 'dirty'
  if (draftCount.value) return 'drafts'
  return 'saved'
})

// Commentaires écrits mais pas encore ajoutés : ajoutés (datés et signés) par le bouton Enregistrer.
async function addDrafts() {
  const pending = Object.entries(drafts.value).filter(([, text]) => text && text.trim())
  if (!pending.length) return true
  savingDrafts.value = true
  let ok = true
  try {
    for (const [rubrique, text] of pending) {
      const res = await apiFetch(`${API_BASE}/api/ParticipantComments`, {
        method: 'POST',
        body: JSON.stringify({ studentId: user.value.id, rubrique, text: text.trim() }),
      })
      if (res?.ok) drafts.value[rubrique] = ''
      else ok = false
    }
    if (!ok) autosaveError.value = "un commentaire n'a pas pu être ajouté"
    await loadComments()
    if (ok) lastSavedAt.value = new Date()
  } catch {
    autosaveError.value = 'connexion impossible'
    ok = false
  } finally {
    savingDrafts.value = false
  }
  return ok
}

// Bouton « Enregistrer » : la fiche (si modifiée ou en échec) puis les commentaires en cours.
async function saveAll() {
  clearTimeout(autosaveTimer)
  const formOk = isDirty() || autosaveError.value ? await save() : true
  const draftsOk = formOk ? await addDrafts() : false
  return formOk && draftsOk
}

let autosaveTimer = null
watch(form, () => {
  if (!isDirty()) return
  clearTimeout(autosaveTimer)
  autosaveTimer = setTimeout(() => save(), 2000)
}, { deep: true })

// En quittant la page : tout est enregistré d'abord (fiche + commentaires en cours) ; on ne
// demande confirmation que si l'enregistrement a échoué.
useUnsavedGuard(() => isDirty() || hasDrafts(), undefined, async () => {
  if (await saveAll()) return true
  return confirmDiscard(`Enregistrement impossible (${autosaveError.value}). Quitter quand même et perdre les modifications ?`)
})

async function load() {
  loading.value = true
  loadInactivityReasons()
  loadLocalites()
  try {
    const id = route.params.id
    const res = await apiFetch(`${API_BASE}/api/ApplicationUsers/${id}`)
    if (res && res.ok) {
      const data = await res.json()
      user.value = normalizeUser(data)
      fillFormFromUser(data)
      takeSnapshot()
      loadComments()
      loadInterets(id)
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
      loadCalendrier(),
    ])
    if (stagesRes && stagesRes.ok) allStages.value = (await stagesRes.json()).map(normalizeStage)
    if (niveauxRes && niveauxRes.ok) allNiveaux.value = await niveauxRes.json()
    if (gendersRes && gendersRes.ok) genders.value = await gendersRes.json()
    if (marrainagesRes && marrainagesRes.ok) {
      marrainages.value = (await marrainagesRes.json()).map(m => normalizeMarrainage(m, user.value.id))
      loadConventionFiles()
    }

    loadParticipationRates()
    loadSessionExtras()
  } finally {
    loading.value = false
  }
}

// Champs vides envoyés comme null (un nombre vide "" faisait échouer tout l'enregistrement).
const toPayload = (f) => Object.fromEntries(Object.entries(f).map(([k, v]) => [k, v === '' ? null : v]))

async function save() {
  clearTimeout(autosaveTimer)
  if (emailRule(form.value.email) !== true) {
    autosaveError.value = "l'adresse e-mail n'est pas valide"
    return false
  }
  if (npaRule(form.value.npa) !== true) {
    autosaveError.value = 'le NPA doit comporter 4 chiffres'
    return false
  }
  if (localiteRule(form.value.localite) !== true) {
    autosaveError.value = 'la localité doit être un nom de lieu, sans indication libre'
    return false
  }
  if (saving.value) {
    // Un enregistrement est déjà en cours : on attend sa fin, puis on n'enregistre que s'il reste
    // des modifications.
    while (saving.value) await new Promise(resolve => setTimeout(resolve, 150))
    if (!isDirty() && !autosaveError.value) return true
  }
  saving.value = true
  const sent = JSON.stringify(form.value)
  try {
    const res = await apiFetch(`${API_BASE}/api/ApplicationUsers/${user.value.id}/profile`, {
      method: 'PATCH',
      body: JSON.stringify(toPayload(form.value))
    })
    if (!res) { autosaveError.value = 'session expirée, reconnectez-vous'; return false }
    if (res.ok) {
      user.value.email = form.value.email
      formSnapshot.value = sent
      autosaveError.value = ''
      lastSavedAt.value = new Date()
      return true
    }
    // L'état est affiché par la barre d'enregistrement (pas de message éphémère en plus).
    autosaveError.value = res.status === 409 || res.status === 400
      ? (await res.text()).replace(/^"|"$/g, '') || 'données refusées'
      : res.status >= 500 ? 'erreur du serveur' : `erreur ${res.status}`
    return false
  } catch (e) {
    autosaveError.value = 'pas de connexion au serveur'
    return false
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
/* Rubriques du suivi formatrices : un bloc par thème, couleur et numéro propres, pour qu'on
   repère immédiatement où renseigner chaque information. */
.rubrique { border: 1px solid var(--rubrique-border); border-left: 5px solid var(--rubrique-color); border-radius: 8px; background: var(--rubrique-bg); padding: 12px 16px 16px; margin-bottom: 16px; }
.rubrique-title { display: flex; align-items: center; font-weight: 700; font-size: 1rem; color: var(--rubrique-color); margin-bottom: 12px; }
.rubrique-num { display: inline-flex; align-items: center; justify-content: center; width: 26px; height: 26px; border-radius: 50%; background: var(--rubrique-color); color: #fff; font-size: 0.85rem; margin-right: 10px; }
.rubrique :deep(.v-field) { background: #fff; }
.rubrique-teal { --rubrique-color: #00796B; --rubrique-bg: #F0F9F8; --rubrique-border: #B2DFDB; }
.rubrique-indigo { --rubrique-color: #3949AB; --rubrique-bg: #F3F4FB; --rubrique-border: #C5CAE9; }
.rubrique-orange { --rubrique-color: #E65100; --rubrique-bg: #FFF6EE; --rubrique-border: #FFCCBC; }
.rubrique-green { --rubrique-color: #2E7D32; --rubrique-bg: #F2F8F2; --rubrique-border: #C8E6C9; }
.rubrique-blue { --rubrique-color: #1565C0; --rubrique-bg: #F1F6FC; --rubrique-border: #BBDEFB; }
.tri-toggle .v-btn { text-transform: none; letter-spacing: 0; }
.rubrique-purple { --rubrique-color: #7B1FA2; --rubrique-bg: #F8F2FA; --rubrique-border: #E1BEE7; }
.besoin-row + .besoin-row { border-top: 1px dashed var(--rubrique-border); margin-top: 4px; padding-top: 4px; }
.marrainage-link { color: #1976D2; text-decoration: underline; }
/* Relation de marrainage en cours : bien visible dans la section insertion */
.marrainage-actif { border: 2px solid #D81B60; border-left-width: 6px; border-radius: 8px; background: #FCE4EC; padding: 10px 12px; }
.experience { border-left: 4px solid #BDBDBD; padding-left: 10px; }
.experience-en_cours { border-left-color: #2E7D32; }
.experience-prevu { border-left-color: #1976D2; }
.historique-table :deep(td), .historique-table :deep(th) { font-size: 0.8rem; }
.cursor-pointer { cursor: pointer; }
</style>
