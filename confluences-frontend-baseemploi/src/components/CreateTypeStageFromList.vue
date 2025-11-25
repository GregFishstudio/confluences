<template>
  <v-row class="d-flex justify-end">
    <v-dialog v-model="dialog" max-width="520px" persistent>
      <template #activator="{ props }">
        <v-btn
          color="primary"
          rounded
          v-bind="props"
          class="elevation-2"
        >
          <v-icon left>mdi-plus</v-icon>
          Ajouter un taux d'occupation
        </v-btn>
      </template>

      <v-card rounded="lg" class="pa-2">
        <v-card-title class="d-flex align-center py-4">
          <v-icon color="primary" class="mr-2">mdi-briefcase-clock-outline</v-icon>
          <span class="text-h6 font-weight-bold">
            Ajouter un taux d'occupation
          </span>
        </v-card-title>

        <v-divider></v-divider>

        <v-card-text class="pt-6">
          <v-form ref="formCreateTypeStage" v-model="validCreateTypeStage" lazy-validation>
            <v-text-field
              v-model="typeStage.nom"
              :counter="50"
              :rules="libelleRules"
              label="Nom du taux d'occupation"
              outlined
              dense
              clearable
              rounded
              hide-details="auto"
            ></v-text-field>
          </v-form>
        </v-card-text>

        <v-divider></v-divider>
        <v-card-actions class="d-flex justify-end">
          <v-btn text color="grey darken-1" @click="dialog = false">
            Annuler
          </v-btn>

          <v-btn color="primary" @click="submit" rounded>
            <v-icon left small>mdi-content-save-outline</v-icon>
            Sauvegarder
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-row>
</template>

<script setup>
import { ref } from 'vue';
import { useStore } from 'vuex';
import NProgress from 'nprogress';

const store = useStore();

// --- État Réactif (data() migré vers ref) ---
const dialog = ref(false);
const validCreateTypeStage = ref(true);
const formCreateTypeStage = ref(null); // Référence au V-Form

const typeStage = ref({
  typeStageId: 0,
  code: null,
  libelle: null,
  nom: null
});

const libelleRules = [
  v => !!v || 'Le champ est obligatoire',
  v => !v || v.length <= 50 || 'Le nom doit contenir moins de 50 caractères'
];

// --- Méthode de soumission (migrée vers la Composition API) ---
const submit = async () => {
  // Valider le formulaire via la référence ref
  const { valid: formValid } = await formCreateTypeStage.value.validate();

  if (formValid) {
    NProgress.start();

    store
      .dispatch('typeStage/createTypeStage', typeStage.value)
      .then(() => {
        // Succès : Réinitialiser le formulaire et fermer la modale
        if (formCreateTypeStage.value) {
            formCreateTypeStage.value.reset();
        }
        dialog.value = false;
      })
      .catch((error) => {
        console.error("Erreur lors de la création du type de stage:", error);
      })
      .finally(() => {
        NProgress.done();
      });
  }
};
</script>

<style scoped>
/* Style léger du hover bouton */
.v-btn {
  /* CORRIGÉ: Le côlon (:) est maintenant présent après 'transition' */
  transition: 0.2s ease-in-out;
}
.v-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.12);
}
</style>