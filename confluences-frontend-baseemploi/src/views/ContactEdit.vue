<template>
  <v-container>
    <v-row>
      <v-col>
        <h1>Contact</h1>
      </v-col>
    </v-row>
    
    <v-card class="pa-4 elevation-2">
      <v-card-text>
        <v-form
          ref="formCreateContact"
          v-model="validCreateContact"
          lazy-validation
        >
          <v-autocomplete
            v-model="contactRef.entrepriseId"
            :items="entreprise.entreprises"
            item-value="entrepriseId"
            item-text="nom"
            label="Entreprise"
            outlined
            dense
          ></v-autocomplete>
          <v-text-field
            v-model="contactRef.prenom"
            :counter="50"
            label="Prénom"
            :rules="nameRules"
            required
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="contactRef.nom"
            :counter="50"
            :rules="nameRules"
            label="Nom"
            required
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="contactRef.fonction"
            :counter="50"
            :rules="nameRules"
            label="Fonction"
            required
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="contactRef.email"
            :counter="50"
            :rules="emailRules"
            label="Email"
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="contactRef.telFix"
            :counter="13"
            :rules="phonesRules"
            label="Téléphone fixe"
            outlined
            dense
          ></v-text-field>
          <v-text-field
            v-model="contactRef.natel"
            :counter="13"
            :rules="phonesRules"
            label="Natel"
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
          <DeleteContact :contact="contactRef" class="ma-2" />
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
import DeleteContact from '@/components/DeleteContact.vue';

const store = useStore();
const route = useRoute();
const router = useRouter();

// Utilise la prop passée par le router guard
const contactRef = ref(route.params.contact || {});

// --- État Réactif ---
const validCreateContact = ref(true);
const formCreateContact = ref(null);

// --- Computed Properties ---
const entreprise = computed(() => store.state.entreprise);

// --- Fonctions de chargement ---
const loadInitialData = () => {
    store.dispatch('entreprise/fetchEntreprises', {});
};

// --- Hooks ---
onBeforeMount(loadInitialData);

// --- Règles de Validation ---
const nameRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
];
const emailRules = [
  v => !v || /.+@.+\..+/.test(v) || "L'email doit être valide",
  v => !v || v.length <= 50 || 'Le champ doit être moins que 50 caractères'
];
const phonesRules = [
  v => !v || v.length <= 13 || 'Le champ doit être moins que 13 caractères'
];

// --- Méthode de soumission ---
const submit = async () => {
  const { valid: formValid } = await formCreateContact.value.validate();
  
  if (formValid) {
    NProgress.start();
    store
      .dispatch('contact/editContact', contactRef.value)
      .then(() => {
        router.push({ name: 'Contacts' });
      })
      .catch((error) => {
          console.error("Erreur de sauvegarde du contact:", error);
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