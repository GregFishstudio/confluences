<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Communication</h1>
      </v-col>
    </v-row>
    
    <v-card class="pa-4 elevation-2">
      <v-card-text>
        <v-form
          ref="formCreateTypeMoyen"
          v-model="validCreateTypeMoyen"
          lazy-validation
        >
          <v-text-field
            v-model="typeMoyenRef.libelle"
            :counter="20"
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
          <DeleteTypeMoyen :typeMoyen="typeMoyenRef" class="ma-2" />
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
import DeleteTypeMoyen from '@/components/DeleteTypeMoyen.vue';

const store = useStore();
const route = useRoute();
const router = useRouter();

// Utilise la prop passée par le router guard
const typeMoyenRef = ref(route.params.typeMoyen || {});

// --- État Réactif ---
const validCreateTypeMoyen = ref(true);
const formCreateTypeMoyen = ref(null);

// --- Règles de Validation ---
const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 20 || 'Le champ doit être moins que 20 caractères'
];

// --- Méthode de soumission ---
const submit = async () => {
  const { valid: formValid } = await formCreateTypeMoyen.value.validate();

  if (formValid) {
    NProgress.start();
    store
      .dispatch('typeMoyen/editTypeMoyen', typeMoyenRef.value)
      .then(() => {
        router.push({ name: 'Moyens' });
      })
      .catch((error) => {
          console.error("Erreur de sauvegarde du moyen:", error);
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