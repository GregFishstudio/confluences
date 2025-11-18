<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Page servant à signaler à l'utilisateur qu'il n'a pas les droits d'accès
  -- Fichier : Unauthorized.vue
  -->

<template>
  <v-container>
    <v-row class="mt-5">
      <v-col class="text-center" cols="12" tag="h1">
        <h2>Stop</h2>
        <h3>
          Vous n'avez pas les droits nécessaires afin d'accéder à cette
          ressource
        </h3>
        <router-link :to="{ name: 'Home' }">Retourner à l'accueil </router-link>
      </v-col>
    </v-row>
  </v-container>
</template>
