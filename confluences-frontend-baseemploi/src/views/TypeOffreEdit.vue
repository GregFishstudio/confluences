<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Offre</h1>
      </v-col>
    </v-row>
    
    <v-card class="pa-4 elevation-2">
      <v-card-text>
        <v-form
          ref="formCreateTypeOffre"
          v-model="validCreateTypeOffre"
          lazy-validation
        >
          <v-text-field
            v-model="typeOffreRef.libelle"
            :counter="30"
            :rules="libelleRules"
            label="Nom"
            required
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
          <DeleteTypeOffre :typeOffre="typeOffreRef" class="ma-2" />
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
import DeleteTypeOffre from '@/components/DeleteTypeOffre.vue';

const store = useStore();
const route = useRoute();
const router = useRouter();

// Utilise la prop passée par le router guard
const typeOffreRef = ref(route.params.typeOffre || {});

// --- État Réactif ---
const validCreateTypeOffre = ref(true);
const formCreateTypeOffre = ref(null);

// --- Règles de Validation ---
const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 30 || 'Le champ doit être moins que 30 caractères'
];

// --- Méthode de soumission ---
const submit = async () => {
  const { valid: formValid } = await formCreateTypeOffre.value.validate();

  if (formValid) {
    NProgress.start();
    store
      .dispatch('typeOffre/editTypeOffre', typeOffreRef.value)
      .then(() => {
        router.push({ name: 'Offres' });
      })
      .catch((error) => {
          console.error("Erreur de sauvegarde de l'offre:", error);
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