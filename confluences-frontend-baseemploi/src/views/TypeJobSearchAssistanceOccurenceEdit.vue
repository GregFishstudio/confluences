<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Occurence ARE</h1>
      </v-col>
    </v-row>
    
    <v-card class="pa-4 elevation-2">
      <v-card-text>
        <v-form
          ref="formCreatetypeJobSearchAssistance"
          v-model="validCreatetypeJobSearchAssistance"
          lazy-validation
        >
          <v-text-field
            v-model="typeJobSearchAssistanceOccurenceRef.description"
            :rules="descriptionRules"
            label="Description"
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
          <DeleteTypeJobSearchAssistanceOccurence
            :typeJobSearchAssistanceOccurence="typeJobSearchAssistanceOccurenceRef"
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
import { ref } from 'vue';
import { useStore } from 'vuex';
import { useRoute, useRouter } from 'vue-router';
import NProgress from 'nprogress';
import DeleteTypeJobSearchAssistanceOccurence from '@/components/DeleteTypeJobSearchAssistanceOccurence.vue';

const store = useStore();
const route = useRoute();
const router = useRouter();

// Utilise la prop passée par le router guard
const typeJobSearchAssistanceOccurenceRef = ref(route.params.typeJobSearchAssistanceOccurence || {});

// --- État Réactif ---
const validCreatetypeJobSearchAssistance = ref(true);
const formCreatetypeJobSearchAssistance = ref(null);

// --- Règles de Validation ---
const descriptionRules = [v => !!v || 'Le champ est obligatoire'];

// --- Méthode de soumission ---
const submit = async () => {
  const { valid: formValid } = await formCreatetypeJobSearchAssistance.value.validate();

  if (formValid) {
    NProgress.start();
    store
      .dispatch('typeJobSearchAssistanceOccurrence/editTypeSearchAssistanceOccurrence', typeJobSearchAssistanceOccurenceRef.value)
      .then(() => {
        router.push({ name: 'TypeJobSearchAssistancesOccurences' });
      })
      .catch((error) => {
          console.error("Erreur de sauvegarde de l'occurrence ARE:", error);
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