<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Métier</h1>
      </v-col>
    </v-row>
    
    <v-card class="pa-4 elevation-2">
      <v-card-text>
        <v-form
          ref="formCreateTypeMetier"
          v-model="validCreateTypeMetier"
          lazy-validation
        >
          <v-text-field
            v-model="typeMetierRef.libelle"
            :rules="libelleRules"
            label="Nom"
            required
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="typeMetierRef.oldNames"
            label="Autres noms"
            :rules="oldNamesRules"
            hint="Ex: Dev, IT Support"
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
          <DeleteTypeMetier :typeMetier="typeMetierRef" class="ma-2" />
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
import DeleteTypeMetier from '@/components/DeleteTypeMetier.vue';

const store = useStore();
const route = useRoute();
const router = useRouter();

// Utilise la prop passée par le router guard
const typeMetierRef = ref(route.params.typeMetier || {});

// --- État Réactif ---
const validCreateTypeMetier = ref(true);
const formCreateTypeMetier = ref(null);

// --- Règles de Validation ---
const oldNamesRules = [
  v => !v || v.length <= 300 || 'Le champ doit être moins que 300 caractères'
];
const libelleRules = [
  v => !!v || 'Le champ est obligatoire'
];

// --- Méthode de soumission ---
const submit = async () => {
  const { valid: formValid } = await formCreateTypeMetier.value.validate();

  if (formValid) {
    NProgress.start();
    store
      .dispatch('typeMetier/editTypeMetier', typeMetierRef.value)
      .then(() => {
        router.push({ name: 'Metiers' });
      })
      .catch((error) => {
          console.error("Erreur de sauvegarde du métier:", error);
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