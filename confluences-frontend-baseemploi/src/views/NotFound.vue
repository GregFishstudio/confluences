<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Page 404 sert à signaler à l'utilisateur que la ressource qu'il cherche n'existe pas
  -- Fichier : NotFound.vue
  -->

<template>
  <v-container>
    <v-row class="mt-5">
      <v-col class="text-center" cols="12" tag="h1">
        <h2>Oups</h2>
        <h3 v-if="resource == 'page'">
          La {{ resource }} que vous cherchez n'existe pas
        </h3>
        <h3 v-else-if="resource != undefined && beginsVowel">
          L' {{ resource }} que vous cherchez n'existe pas
        </h3>
        <h3 v-else-if="resource != undefined">
          Le-la {{ resource }} que vous cherchez n'existe pas
        </h3>
        <h3 v-else>La ressource que vous cherchez n'existe pas</h3>
        <router-link :to="{ name: 'Login' }">Retourner à l'accueil</router-link>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
export default {
  props: {
    resource: {
      type: String,
      required: false
    }
  },
  computed: {
    beginsVowel() {
      const regex = new RegExp('^[aeiou].*', 'i')
      return regex.test(this.resource)
    }
  }
}
</script>
