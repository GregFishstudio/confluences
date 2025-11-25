<template>
  <v-container>
    <v-row class="mt-5">
      <v-col class="text-center" cols="12" tag="h1">
        <h2>❌ Stop</h2>
        <h3>
          Vous n'avez pas les droits nécessaires afin d'accéder à cette
          ressource
        </h3>
        <router-link :to="{ name: 'Home' }">Retourner à l'accueil </router-link>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
// Pas de logique à migrer, le composant utilise seulement le template.
</script>