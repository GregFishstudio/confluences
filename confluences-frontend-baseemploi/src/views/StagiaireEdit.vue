<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Stagiaire</h1>
      </v-col>
    </v-row>
    
    <v-form ref="formStagiaire" v-model="valid" lazy-validation>
      <v-row>
        <v-col cols="12" md="12">
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="stagiaireRef.nom"
                :rules="lastnameRules"
                label="Nom du stagiaire"
                required
                outlined
                dense
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="stagiaireRef.prenom"
                :rules="firstnameRules"
                label="Prénom du stagiaire"
                required
                outlined
                dense
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="stagiaireRef.typeAffiliationId"
                :items="typeAffiliation.typeAffiliations"
                item-value="typeAffiliationId"
                item-text="libelle"
                label="Affiliation"
                clearable
                outlined
                dense
              ></v-autocomplete>
            </v-col>
          </v-row>
        </v-col>
      </v-row>

      <v-row>
        <v-col>
          <v-card class="mx-auto elevation-2" outlined>
            <v-list>
              <v-subheader>
                Stages effectués
                <v-spacer></v-spacer>
              </v-subheader>
              <v-data-table
                :headers="headers"
                :items="stagiaireRef.stageStagiaires || []"
                :items-per-page="10"
                @click:row="viewStage"
                class="elevation-0"
              >
                <template v-slot:item.debut="{ item }">
                  {{ formatDate(item.debut) }}
                </template>
                <template v-slot:item.fin="{ item }">
                  {{ formatDate(item.fin) }}
                </template>
                <template v-slot:item.rapport="{ item }">
                  <v-checkbox v-model="item.rapport" disabled></v-checkbox>
                </template>
                <template v-slot:item.attestation="{ item }">
                  <v-checkbox v-model="item.attestation" disabled></v-checkbox>
                </template>
              </v-data-table>
            </v-list>
          </v-card>
        </v-col>
      </v-row>

      <div class="action-container action-bar-fixed">
        <v-row class="fill-height ma-0">
          <v-col class="d-flex justify-end align-center py-2">
            <v-btn
              class="ma-2"
              color="success"
              min-width="150"
              @click="submit"
            >
              Sauvegarder
            </v-btn>
            <DeleteStagiaire :stagiaire="stagiaireRef" class="ma-2" />
            <v-btn
              class="ma-2"
              color="primary"
              min-width="150"
              @click="router.go(-1)"
            >
              Annuler
            </v-btn>
          </v-col>
        </v-row>
      </div>
    </v-form>
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue';
import { useStore } from 'vuex';
import { useRoute, useRouter } from 'vue-router';
import NProgress from 'nprogress';
import moment from 'moment';
import DeleteStagiaire from '@/components/DeleteStagiaire.vue';

const store = useStore();
const route = useRoute();
const router = useRouter();

// Utilise la prop passée par le router guard
const stagiaireRef = ref(route.params.stagiaire || {});

// --- État Réactif ---
const formStagiaire = ref(null);
const valid = ref(true);

// --- Computed Properties ---
const typeAffiliation = computed(() => store.state.typeAffiliation);

// --- Données Statiques ---
const headers = [
    { text: 'Entreprise', value: 'entreprise.nom' },
    { text: 'Adresse', value: 'entreprise.adr1' },
    { text: 'Ville', value: 'entreprise.ville' },
    { text: 'Début', value: 'debut' },
    { text: 'Fin', value: 'fin' },
    { text: 'Métier', value: 'typeMetier.libelle' },
    { text: 'Rapport', value: 'rapport' },
    { text: 'Attestation', value: 'attestation' }
];

// --- Règles de Validation ---
const lastnameRules = [
    v => /(\b[A-Z0-9]{1,}\b)/.test(v) || 'En majuscule seulement',
    v => !!v || 'Le champ est obligatoire',
    v => (v && v.length <= 50) || 'Le champ doit être moins que 50 caractères'
];
const firstnameRules = [
    v => !!v || 'Le champ est obligatoire',
    v => (v && v.length <= 50) || 'Le champ doit être moins que 50 caractères'
];

// --- Hooks ---
onBeforeMount(() => {
    store.dispatch('typeAffiliation/fetchTypeAffiliations', {});
});

// --- Méthodes ---
const submit = async () => {
    const { valid: formValid } = await formStagiaire.value.validate();

    if (formValid) {
        NProgress.start();
        
        // Nettoyer l'objet avant l'envoi
        if (stagiaireRef.value.typeAffiliationId == null) {
            stagiaireRef.value.typeAffiliation = null;
        }

        store
            .dispatch('stagiaire/editStagiaire', stagiaireRef.value)
            .then(() => {
                router.push({ name: 'Stagiaires' });
            })
            .catch((error) => {
                console.error("Erreur de sauvegarde du stagiaire:", error);
            })
            .finally(() => {
                NProgress.done();
            });
    }
};

const viewStage = (event, { item }) => {
    // Utilise la syntaxe Vuetify 3 pour extraire l'item
    const id = item.stageId || item.StageId;
    if (id) {
        router.push({ name: 'Stage-Modifier', params: { id: id } });
    }
};

const formatDate = (date) => {
    let dateFormat = moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD');
    return dateFormat === 'Invalid date' ? null : dateFormat;
};
</script>

<style scoped>
.action-bar-fixed {
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 64px;
  background: white;
  border-top: 1px solid #e0e0e0;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08);
  z-index: 1000;
}
.v-container {
    padding-bottom: 80px;
}
</style>