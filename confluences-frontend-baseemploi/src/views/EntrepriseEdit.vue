<template>
  <v-container fluid class="pb-16">
    <v-row>
      <v-col>
        <h1 class="text-h4 mb-4">Entreprise</h1>
      </v-col>
    </v-row>
    
    <v-form ref="formRef" v-model="isFormValid">
      <v-row>
        <v-col cols="12" md="5">
          <v-card variant="outlined" class="pa-4 mb-4">
            <v-card-title class="text-subtitle-1 font-weight-bold">Informations Générales</v-card-title>
            <v-card-text>
              <v-text-field
                v-model="entrepriseRef.nom"
                label="Nom"
                :rules="nameRules"
                counter="50"
                required
              ></v-text-field>

              <v-text-field
                v-model="entrepriseRef.email"
                label="Email"
                :rules="emailRules"
                counter="50"
              ></v-text-field>
              
              <v-text-field
                v-model="entrepriseRef.adr1"
                label="Adresse"
                :rules="adressRules"
                counter="50"
              ></v-text-field>

              <v-text-field
                v-model="entrepriseRef.adr2"
                label="Complément d'adresse"
                :rules="adressRules"
                counter="50"
              ></v-text-field>
              
              <v-row>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="entrepriseRef.codePostal"
                    label="Code postal"
                    :rules="[codePostalRules]"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="entrepriseRef.ville"
                    label="Ville"
                    :rules="nameRules"
                    required
                  ></v-text-field>
                </v-col>
              </v-row>

              <v-row>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="entrepriseRef.telFix"
                    label="Téléphone fixe"
                    :rules="phonesRules"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="entrepriseRef.telNatel"
                    label="Natel"
                    :rules="phonesRules"
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="12" md="7">
          <v-card variant="outlined" class="pa-4 mb-4">
            <v-card-title class="text-subtitle-1 font-weight-bold">Relations & Suivi</v-card-title>
            <v-card-text>
              <v-row>
                <v-col cols="12" sm="4">
                  <v-autocomplete
                    v-model="entrepriseRef.typeEntrepriseId"
                    :items="typeEntrepriseList"
                    item-value="typeEntrepriseId"
                    item-title="nom"
                    label="Catégorie"
                    clearable
                  ></v-autocomplete>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-text-field
                    v-model="entrepriseRef.dateCreation"
                    label="Chez Confluences depuis"
                    type="date"
                  ></v-text-field>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-autocomplete
                    v-model="entrepriseRef.typeMoyenId"
                    :items="typeMoyenList"
                    item-value="typeMoyenId"
                    item-title="libelle"
                    label="Moyen de communication"
                    clearable
                  ></v-autocomplete>
                </v-col>
              </v-row>

              <EntrepriseLastContactList :entreprise="entrepriseRef" class="mt-4" />
            </v-card-text>
          </v-card>
          
          <v-card variant="outlined" class="pa-4">
            <v-card-title class="text-subtitle-1 font-weight-bold">Remarques</v-card-title>
            <v-card-text>
              <v-textarea
                v-model="entrepriseRef.remarque"
                label="Notes internes"
                counter="10000"
                auto-grow
              ></v-textarea>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
      
      <v-row>
        <v-col cols="12" md="4"><EntrepriseDomaineList :entreprise="entrepriseRef" /></v-col>
        <v-col cols="12" md="4"><EntrepriseMetierList :entreprise="entrepriseRef" /></v-col>
        <v-col cols="12" md="4"><EntrepriseOffreList :entreprise="entrepriseRef" /></v-col>
      </v-row>

      <v-row class="mb-16">
        <v-col cols="12" md="6">
          <v-card variant="flat" border>
            <v-card-title class="text-subtitle-1 font-weight-bold">Contacts Associés</v-card-title>
            <EntrepriseContactList :entreprise="entrepriseRef" />
          </v-card>
        </v-col>
        <v-col cols="12" md="6">
          <v-card variant="flat" border>
            <v-card-title class="text-subtitle-1 font-weight-bold">Stages Précédents</v-card-title>
            <EntrepriseStageList :entreprise="entrepriseRef" />
          </v-card>
        </v-col>
      </v-row>
    </v-form>

    <v-footer app border height="70" class="justify-end px-6">
      <v-btn color="success" class="me-2" min-width="120" @click="submit" :loading="loading">
        Sauvegarder
      </v-btn>
      <DeleteEntreprise :entreprise="entrepriseRef" class="me-2" />
      <v-btn variant="outlined" color="primary" min-width="120" @click="retourOuListe(router, 'Entreprises')">
        Annuler
      </v-btn>
    </v-footer>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { retourOuListe } from '@/composables/useListMemory'
import { useStore } from 'vuex';
import NProgress from 'nprogress';
import moment from 'moment';

// Components imports (vérifiez vos chemins)
import EntrepriseOffreList from '@/components/EntrepriseOffreList.vue'; 
import EntrepriseDomaineList from '@/components/EntrepriseDomaineList.vue';
import EntrepriseMetierList from '@/components/EntrepriseMetierList.vue';
import EntrepriseStageList from '@/components/EntrepriseStageList.vue';
import EntrepriseContactList from '@/components/EntrepriseContactList.vue';
import EntrepriseLastContactList from '@/components/EntrepriseLastContactList.vue';
import DeleteEntreprise from '@/components/DeleteEntreprise.vue';

const route = useRoute();
const router = useRouter();
const store = useStore();

// --- État ---
const formRef = ref(null);
const isFormValid = ref(false);
const loading = ref(false);
// On clone l'objet pour éviter de modifier directement le state du store/route par référence
const entrepriseRef = ref({ ...route.params.entreprise } || {});

// --- Données du Store ---
const typeEntrepriseList = computed(() => store.state.typeEntreprise.typeEntreprises);
const typeMoyenList = computed(() => store.state.typeMoyen.typeMoyens);

// --- Initialisation ---
onMounted(async () => {
  NProgress.start();
  // Chargement en parallèle
  await Promise.all([
    store.dispatch('typeEntreprise/fetchTypeEntreprises'),
    store.dispatch('typeMoyen/fetchTypeMoyens'),
    store.dispatch('user/fetchUsers')
  ]);
  
  // Formatage des dates pour l'input type="date" (format YYYY-MM-DD requis)
  if (entrepriseRef.value.dateCreation) {
    entrepriseRef.value.dateCreation = moment(entrepriseRef.value.dateCreation).format('YYYY-MM-DD');
  }
  NProgress.done();
});

// --- Règles de Validation ---
const nameRules = [
  v => !!v || 'Requis',
  v => (v?.length <= 50) || 'Max 50 caractères'
];
const adressRules = [
  v => (!v || v.length <= 50) || 'Max 50 caractères'
];
const emailRules = [
  v => !v || /.+@.+\..+/.test(v) || "Email invalide",
  v => (!v || v.length <= 50) || 'Max 50 caractères'
];
const codePostalRules = v => {
  if (!v) return true;
  return (v >= 1000 && v <= 9999) || 'Suisse: 4 chiffres';
};
const phonesRules = [
  v => (!v || v.length <= 13) || 'Max 13 caractères'
];

// --- Actions ---
const submit = async () => {
  const { valid } = await formRef.value.validate();
  
  if (valid) {
    loading.value = true;
    NProgress.start();
    
    try {
      await store.dispatch('entreprise/editEntreprise', entrepriseRef.value);
      retourOuListe(router, 'Entreprises');
    } catch (error) {
      console.error("Sauvegarde échouée", error);
    } finally {
      loading.value = false;
      NProgress.done();
    }
  }
};
</script>

<style scoped>
/* Vuetify 3 gère mieux les espacements, mais v-footer app assure la position fixe */
:deep(.v-text-field .v-input__details) {
  min-height: 22px;
}
</style>