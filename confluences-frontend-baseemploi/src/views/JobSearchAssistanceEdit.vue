<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>ARE</h1>
      </v-col>
    </v-row>
    
    <v-card class="pa-4 elevation-2">
      <v-card-text>
        <v-form
          ref="formJobSearchAssistance"
          v-model="valid"
          lazy-validation
        >
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="jobSearchAssistanceRef.address"
                label="Adresse"
                outlined
                dense
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="jobSearchAssistanceRef.town"
                label="Ville"
                outlined
                dense
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="jobSearchAssistanceRef.zipCode"
                label="Code postale"
                outlined
                dense
              ></v-text-field>
            </v-col>
          </v-row>

          <v-row>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="jobSearchAssistanceRef.typeJobSearchAssistanceId"
                :items="typeJobSearchAssistance.typeSearchAssistances"
                item-value="typeJobSearchAssistanceId"
                item-text="description"
                label="Type"
                clearable
                outlined
                dense
              ></v-autocomplete>
            </v-col>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="jobSearchAssistanceRef.typeJobSearchAssistanceOccurrenceId"
                :items="typeJobSearchAssistanceOccurrence.typeSearchAssistanceOccurrences"
                item-value="typeJobSearchAssistanceOccurrenceId"
                item-text="description"
                label="Occurrence"
                clearable
                outlined
                dense
              ></v-autocomplete>
            </v-col>
            <v-col cols="12" md="4">
              <v-menu
                v-model="menuCreation"
                :close-on-content-click="false"
                transition="scale-transition"
                offset-y
                min-width="auto"
              >
                <template v-slot:activator="{ on, attrs }">
                  <v-text-field
                    v-model="jobSearchAssistanceRef.date"
                    label="Date"
                    readonly
                    v-bind="attrs"
                    v-on="on"
                    outlined
                    dense
                  ></v-text-field>
                </template>
                <v-date-picker
                  v-model="jobSearchAssistanceRef.date"
                  @input="menuCreation = false"
                  no-title
                  scrollable
                ></v-date-picker>
              </v-menu>
            </v-col>
          </v-row>

          <v-text-field
            v-model="jobSearchAssistanceRef.description"
            label="Description"
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="jobSearchAssistanceRef.website"
            label="Site internet"
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="jobSearchAssistanceRef.keyWords"
            label="Mots clés"
            hint="Séparés par des virgules"
            outlined
            dense
          ></v-text-field>
        </v-form>
      </v-card-text>
    </v-card>

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
          <DeleteJobSearchAssistance
            :jobSearchAssistance="jobSearchAssistanceRef"
            class="ma-2"
          />
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
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue';
import { useStore } from 'vuex';
import { useRoute, useRouter } from 'vue-router';
import NProgress from 'nprogress';
import DeleteJobSearchAssistance from '@/components/DeleteJobSearchAssistance.vue';
import moment from 'moment';

const store = useStore();
const route = useRoute();
const router = useRouter();

// Utilise la prop passée par le router guard
const jobSearchAssistanceRef = ref(route.params.jobSearchAssistance || {});

// --- État Réactif ---
const valid = ref(true);
const formJobSearchAssistance = ref(null);
const menuCreation = ref(false);

// --- Computed Properties ---
const typeJobSearchAssistanceOccurrence = computed(() => store.state.typeJobSearchAssistanceOccurrence);
const typeJobSearchAssistance = computed(() => store.state.typeJobSearchAssistance);

// --- Fonctions de chargement ---
const loadInitialData = () => {
  store.dispatch('typeJobSearchAssistance/fetchTypeJobSearchAssistances', {});
  store.dispatch('typeJobSearchAssistanceOccurrence/fetchtypeSearchAssistanceOccurrences', {});
};

// --- Formatage ---
const formatDate = (date) => {
  return moment(date, 'YYYY-MM-DD').format('YYYY-MM-DD');
};

// --- Hooks ---
onBeforeMount(loadInitialData);

// Initialisation des dates
jobSearchAssistanceRef.value.date = formatDate(jobSearchAssistanceRef.value.date);

// --- Méthode de soumission ---
const submit = async () => {
  const { valid: formValid } = await formJobSearchAssistance.value.validate();

  if (formValid) {
    NProgress.start();
    store
      .dispatch('jobSearchAssistance/editJobSearchAssistance', jobSearchAssistanceRef.value)
      .then(() => {
        router.push({ name: 'JobSearchAssistances' });
      })
      .catch((error) => {
          console.error("Erreur de sauvegarde de l'ARE:", error);
      })
      .finally(() => {
          NProgress.done();
      });
  }
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