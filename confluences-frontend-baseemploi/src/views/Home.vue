<template>
  <v-container class="py-10">
    <h1 class="text-h4 mb-8 font-weight-bold">Bienvenue 👋</h1>

    <v-row dense>
      <!-- 1️⃣ Confluence Base Emploi -->
      <v-col cols="12" md="4">
        <v-card 
          class="elevation-4 hover-card"
          rounded="xl"
          @click="goTo('/entreprises')"
        >
          <v-img
            src="/images/base-emploi.jpg"
            height="160"
            cover
          ></v-img>

          <v-card-title class="text-h6">Confluence Base Emploi</v-card-title>
          <v-card-text>
            Accéder au frontend principal (Vue 3) :
            <ul class="ml-4 mt-2">
              <li>Entreprises</li>
              <li>Stages</li>
              <li>Contacts</li>
              <li>Stagiaires</li>
              <li>Types & Méta-données</li>
            </ul>
          </v-card-text>
        </v-card>
      </v-col>

      <!-- 2️⃣ Confluence MVC -->
      <v-col cols="12" md="4">
        <v-card
          class="elevation-4 hover-card"
          rounded="xl"
          @click="openMvc()"
        >
          <v-img
            src="/images/mvc.jpg"
            height="160"
            cover
          ></v-img>

          <v-card-title class="text-h6">Confluence MVC</v-card-title>
          <v-card-text>
            Interface administrative historique (.NET MVC).  
            <br>Permet la gestion complète des comptes, documents et paramètres avancés.
          </v-card-text>
        </v-card>
      </v-col>

      <!-- 3️⃣ Video Call -->
      <v-col cols="12" md="4">
        <v-card class="elevation-4 hover-card" rounded="xl">
          <v-img
            src="/images/video-call.jpg"
            height="160"
            cover
          ></v-img>

          <v-card-title class="text-h6 d-flex align-center">
            Video Call
            <v-chip
              class="ml-2"
              size="small"
              color="amber-darken-3"
              label
            >En développement</v-chip>
          </v-card-title>

          <v-card-text>
            Système de visioconférence intégré (bientôt disponible).
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { useRouter } from 'vue-router'
const router = useRouter()

const goTo = (path) => {
  router.push(path)
}

const openMvc = () => {
  // Mets ici l'URL réelle du MVC quand tu l’auras
  window.open("http://localhost:5002/Login", "_blank")
}
</script>

<style scoped>
.hover-card {
  transition: 0.25s ease;
  cursor: pointer;
}
.hover-card:hover {
  transform: translateY(-4px);
}
</style>
