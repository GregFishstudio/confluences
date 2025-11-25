<template>
  <v-container>
    <v-row class="mt-5">
      <v-col class="text-center" cols="12">
        <h2>Oups! 🤷‍♂️</h2>
        
        <h3 v-if="resource === 'page'">
          La page que vous cherchez n'existe pas
        </h3>
        
        <h3 v-else-if="resource && beginsVowel">
          L'{{ resource }} que vous cherchez n'existe pas
        </h3>
        
        <h3 v-else-if="resource">
          Le-la {{ resource }} que vous cherchez n'existe pas
        </h3>
        
        <h3 v-else>
          La ressource que vous cherchez n'existe pas
        </h3>
        
        <router-link :to="{ name: 'Login' }">Retourner à la page de connexion</router-link>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { computed } from 'vue';

// Définition des props dans la Composition API
const props = defineProps({
  resource: {
    type: String,
    required: false
  }
});

// Propriété calculée pour vérifier si la ressource commence par une voyelle
const beginsVowel = computed(() => {
  // Accès à la prop via props.resource
  if (!props.resource) return false;
  
  const regex = new RegExp('^[aeiouyàéèêëiîïoôœuùüÿ].*', 'i'); // Ajout de voyelles françaises courantes
  return regex.test(props.resource);
});

// Note: Le tag 'h1' est retiré du v-col car le tag h1 est inclus dans le h2/h3 pour l'accessibilité
</script>