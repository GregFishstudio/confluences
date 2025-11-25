<template>
  <v-container fluid class="main-content">
    <v-row>
      <v-col>
        <h1>Entreprise</h1>
      </v-col>
    </v-row>
    
    <v-form ref="formEntreprise" v-model="valid" lazy-validation>
      <v-row>
        <v-col cols="12" md="5">
          <v-card class="pa-4 mb-4 elevation-2">
            <v-card-title class="text-subtitle-1 font-weight-bold">Informations Générales</v-card-title>
            <v-card-text>
              <v-text-field
                v-model="entrepriseRef.nom"
                :counter="50"
                :rules="nameRules"
                label="Nom"
                required
                outlined
                dense
              ></v-text-field>
              <v-text-field
                v-model="entrepriseRef.email"
                :counter="50"
                :rules="emailRules"
                label="Email"
                outlined
                dense
              ></v-text-field>
              
              <v-text-field
                v-model="entrepriseRef.adr1"
                :counter="50"
                :rules="adressRules"
                label="Adresse"
                outlined
                dense
              ></v-text-field>
              <v-text-field
                v-model="entrepriseRef.adr2"
                :counter="50"
                :rules="adressRules"
                label="Complément d'adresse"
                outlined
                dense
              ></v-text-field>
              
              <v-row>
                <v-col cols="12" sm="6" class="py-0">
                  <v-text-field
                    v-model="entrepriseRef.codePostal"
                    :counter="4"
                    :rules="[codePostalRules]"
                    label="Code postal"
                    outlined
                    dense
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6" class="py-0">
                  <v-text-field
                    v-model="entrepriseRef.ville"
                    :counter="50"
                    :rules="nameRules"
                    label="Ville"
                    required
                    outlined
                    dense
                  ></v-text-field>
                </v-col>
              </v-row>

              <v-row>
                <v-col cols="12" sm="6" class="py-0">
                  <v-text-field
                    v-model="entrepriseRef.telFix"
                    :counter="13"
                    :rules="phonesRules"
                    label="Téléphone fixe"
                    outlined
                    dense
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6" class="py-0">
                  <v-text-field
                    v-model="entrepriseRef.telNatel"
                    :counter="13"
                    :rules="phonesRules"
                    label="Natel"
                    outlined
                    dense
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="12" md="7">
          <v-card class="pa-4 mb-4 elevation-2">
            <v-card-title class="text-subtitle-1 font-weight-bold">Relations & Suivi</v-card-title>
            <v-card-text>
              <v-row>
                <v-col cols="12" sm="4" class="py-0">
                  <v-autocomplete
                    v-model="entrepriseRef.typeEntrepriseId"
                    :items="typeEntreprise.typeEntreprises"
                    item-value="typeEntrepriseId"
                    item-text="nom"
                    label="Catégorie"
                    clearable
                    outlined
                    dense
                  ></v-autocomplete>
                </v-col>
                <v-col cols="12" sm="4" class="py-0">
                  <v-menu
                    v-model="menuCreation"
                    :close-on-content-click="false"
                    transition="scale-transition"
                    offset-y
                    min-width="auto"
                  >
                    <template v-slot:activator="{ on, attrs }">
                      <v-text-field
                        v-model="entrepriseRef.dateCreation"
                        label="Chez Confluences depuis"
                        readonly
                        v-bind="attrs"
                        v-on="on"
                        outlined
                        dense
                      ></v-text-field>
                    </template>
                    <v-date-picker v-model="entrepriseRef.dateCreation" @input="menuCreation = false" no-title scrollable></v-date-picker>
                  </v-menu>
                </v-col>
                <v-col cols="12" sm="4" class="py-0">
                  <v-autocomplete
                    v-model="entrepriseRef.typeMoyenId"
                    :items="typeMoyen.typeMoyens"
                    item-value="typeMoyenId"
                    item-text="libelle"
                    label="Moyen de communication"
                    clearable
                    outlined
                    dense
                  ></v-autocomplete>
                </v-col>
              </v-row>

              <EntrepriseLastContactList :entreprise="entrepriseRef" class="mt-4" />
              
            </v-card-text>
          </v-card>
          
          <v-card class="pa-4 elevation-2">
            <v-card-title class="text-subtitle-1 font-weight-bold">Remarques</v-card-title>
            <v-card-text>
              <v-textarea
                v-model="entrepriseRef.remarque"
                outlined
                label="Notes internes"
                counter
                maxlength="10000"
                auto-grow
                dense
              ></v-textarea>
            </v-card-text>
          </v-card>

        </v-col>
      </v-row>
      
      <v-row>
        <v-col cols="12" md="4">
          <EntrepriseDomaineList :entreprise="entrepriseRef" />
        </v-col>
        <v-col cols="12" md="4">
          <EntrepriseMetierList :entreprise="entrepriseRef" />
        </v-col>
        <v-col cols="12" md="4">
          <EntrepriseOffreList :entreprise="entrepriseRef" />
        </v-col>
      </v-row>

      <v-row class="mb-16">
        <v-col cols="12" md="6">
          <v-card class="elevation-2">
            <v-card-title class="text-subtitle-1 font-weight-bold">Contacts Associés</v-card-title>
            <EntrepriseContactList :entreprise="entrepriseRef" />
          </v-card>
        </v-col>
        <v-col cols="12" md="6">
          <v-card class="elevation-2">
            <v-card-title class="text-subtitle-1 font-weight-bold">Stages Précédents</v-card-title>
            <v-list>
              <EntrepriseStageList :entreprise="entrepriseRef" />
            </v-list>
          </v-card>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
  
  <div class="action-container action-bar-fixed">
    <v-row class="fill-height ma-0">
      <v-col class="d-flex justify-end align-center py-2">
        <v-btn
          class="ma-2"
          color="success"
          min-width="120"
          @click="submit"
        >
          Sauvegarder
        </v-btn>
        <DeleteEntreprise :entreprise="entrepriseRef" class="ma-2" />
        <v-btn
          class="ma-2"
          color="primary"
          min-width="120"
          @click="$router.go(-1)"
        >
          Annuler
        </v-btn>
      </v-col>
    </v-row>
  </div>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useStore } from 'vuex';
import NProgress from 'nprogress';
// Assurez-vous que tous ces chemins de composants sont corrects
import EntrepriseOffreList from '@/components/EntrepriseOffreList.vue'; 
import EntrepriseDomaineList from '@/components/EntrepriseDomaineList.vue';
import EntrepriseMetierList from '@/components/EntrepriseMetierList.vue';
import EntrepriseStageList from '@/components/EntrepriseStageList.vue';
import EntrepriseContactList from '@/components/EntrepriseContactList.vue';
import EntrepriseLastContactList from '@/components/EntrepriseLastContactList.vue';
import DeleteEntreprise from '@/components/DeleteEntreprise.vue';
import moment from 'moment';

const route = useRoute();
const router = useRouter();
const store = useStore();

// --- État Réactif ---
const entrepriseRef = ref(route.params.entreprise || {}); 
const date = ref(null);
const menuContact = ref(false);
const menuCreation = ref(false);
const menuContactRecall = ref(false);
const valid = ref(true);
const formEntreprise = ref(null);

// --- Computed Properties ---
const state = store.state;
const typeEntreprise = computed(() => state.typeEntreprise);
const typeDomaine = computed(() => state.typeDomaine);
const typeMoyen = computed(() => state.typeMoyen);
const user = computed(() => state.user);

// --- Fonctions de chargement ---
const loadInitialData = () => {
    store.dispatch('typeEntreprise/fetchTypeEntreprises', {});
    store.dispatch('typeDomaine/fetchTypeDomaines', {});
    store.dispatch('typeMoyen/fetchTypeMoyens', {});
    store.dispatch('user/fetchUsers', {});
};

// --- Formatage ---
const formatDate = (date) => {
    let dateFormat = moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD');
    if (dateFormat === 'Invalid date') {
        return null;
    } else {
        return dateFormat;
    }
};

// --- Hooks d'exécution ---
onBeforeMount(loadInitialData);

// Initialisation des dates
entrepriseRef.value.dateDernierContact = formatDate(entrepriseRef.value.dateDernierContact);
entrepriseRef.value.dateCreation = formatDate(entrepriseRef.value.dateCreation);
entrepriseRef.value.dateLastRecall = formatDate(entrepriseRef.value.dateLastRecall);


// --- Règles de Validation ---
const nameRules = [
    v => !!v || 'Le nom est obligatoire',
    v => (v && v.length <= 50) || 'Le nom doit être moins que 50 caractères'
];
const adressRules = [
    v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
];
const emailRules = [
    v => !v || /.+@.+\..+/.test(v) || "L'email doit être valide",
    v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
];
const codePostalRules = v => {
    if (v === null || v === undefined) return true;
    if (v >= 0 && v <= 9999 && String(v).length === 4) return true;
    return 'En Suisse, 4 chiffres';
};
const phonesRules = [
    v => !v || v.length <= 13 || 'Le champ doit être moins que 13 caractères'
];


// --- Méthode de soumission ---
const submit = async () => {
    const { valid: formValid } = await formEntreprise.value.validate();

    if (formValid) {
        NProgress.start();
        store
            .dispatch('entreprise/editEntreprise', entrepriseRef.value)
            .then(() => {
                router.push({ name: 'Entreprises' });
            })
            .catch((error) => {
                console.error("Erreur de sauvegarde de l'entreprise:", error);
            })
            .finally(() => {
                NProgress.done();
            });
    }
};
</script>

<style scoped>
/* Conteneur principal pour éviter que la barre fixe cache le contenu */
.main-content {
    padding-bottom: 80px !important; /* Espace pour la barre d'action fixe */
}

/* Style de la Barre d'Actions Fixe */
.action-bar-fixed {
  position: fixed;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 64px; /* Hauteur définie */
  background: white;
  border-top: 1px solid #e0e0e0;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08);
  z-index: 1000;
}

/* Correction de l'alignement des menus dans les colonnes */
.v-menu {
    width: 100%;
}
</style>