<template>
  <v-navigation-drawer
    v-if="isDesktop"
    permanent
    location="left"
    width="280"
    color="surface"
    class="border-e"
    elevation="1"
  >
    <div class="logo">
      <img :src="currentLogo" alt="Logo Confluences" class="logo-img"/>
    </div>
    
    <v-list density="comfortable" nav class="pa-3">
      <v-list-item
        v-for="link in visibleLinks"
        :key="link.label"
        :to="link.external ? null : { name: link.routeName }"
        :href="link.external ? link.url : null"
        :target="link.external ? '_blank' : null"
        :prepend-icon="link.icon"
        :title="link.label"
        link
        color="primary"
        class="mb-1 rounded-lg"
        variant="text"
      />

      <v-expansion-panels v-if="loggedIn" variant="accordion">
        <v-expansion-panel value="config" class="mt-2">
          <v-expansion-panel-title class="px-2">
            <template #default="{ expanded }">
              <v-list-item class="pa-0">
                <template #prepend>
                  <v-icon :icon="expanded ? 'mdi-cog' : 'mdi-cog-outline'" />
                </template>
                <v-list-item-title class="text-body-1 font-weight-medium">
                  Configuration
                </v-list-item-title>
              </v-list-item>
            </template>
          </v-expansion-panel-title>
          <v-expansion-panel-text>
            <v-list density="compact" class="py-0">
              <v-list-item
                v-for="cLink in configurationLinks"
                :key="cLink.label"
                :to="{ name: cLink.routeName }"
                :title="cLink.label"
                link
                class="rounded-lg my-1"
                variant="text"
              />
            </v-list>
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>
    </v-list>

    <template #append>
      <div class="pa-3">
        <v-divider class="mb-3" />
        
        <v-btn
          @click="toggleTheme"
          variant="text"
          block
          class="justify-start mb-2 rounded-lg"
          height="48"
        >
          <template #prepend>
            <v-icon :icon="darkTheme ? 'mdi-weather-sunny' : 'mdi-weather-night'" />
          </template>
          <span class="text-body-2">
            {{ darkTheme ? 'Mode clair' : 'Mode sombre' }}
          </span>
        </v-btn>

        <v-btn
          block
          :color="loggedIn ? 'error' : 'primary'"
          :variant="loggedIn ? 'outlined' : 'flat'"
          class="font-weight-bold rounded-lg"
          size="large"
          @click="loggedIn ? authStore.logout() : goToLogin()"
        >
          <template #prepend>
            <v-icon :icon="loggedIn ? 'mdi-logout' : 'mdi-account-circle-outline'" />
          </template>
          {{ loggedIn ? 'Déconnexion' : 'Connexion' }}
        </v-btn>
      </div>
    </template>
  </v-navigation-drawer>

  <v-app-bar 
    color="surface"
    flat
    class="border-b"
    :class="{ 'ml-0': isDesktop }"
    elevation="1"
  >
    <template v-if="isMobile">
      <v-app-bar-nav-icon 
        @click="mobileDrawer = !mobileDrawer"
        variant="text"
        size="small"
      />
      
      <div class="logo">
        <img :src="currentLogo" alt="Logo Confluences" class="logo-img"/>
      </div>
    </template>

    <v-spacer />
    
  </v-app-bar>

  <v-navigation-drawer
    v-model="mobileDrawer"
    v-if="isMobile"
    location="left"
    temporary
    width="300"
    color="surface" 
  >
    <v-list density="comfortable" nav class="pa-3">
      <v-list-item
        :title="loggedIn ? 'Déconnexion' : 'Connexion'"
        :prepend-icon="loggedIn ? 'mdi-logout' : 'mdi-account-circle-outline'"
        :color="loggedIn ? 'error' : 'primary'"
        @click="loggedIn ? authStore.logout() : goToLogin()"
        link
        class="font-weight-bold mb-3 rounded-lg"
      ></v-list-item>

      <v-divider class="mb-3"></v-divider>
      
      <v-list-item
        v-for="link in visibleLinks"
        :key="link.label"
        :to="link.external ? null : { name: link.routeName }"
        :href="link.external ? link.url : null"
        :target="link.external ? '_blank' : null"
        :prepend-icon="link.icon"
        :title="link.label"
        link
        color="primary"
        class="mb-1 rounded-lg"
        variant="text"
        @click="mobileDrawer = false"
      />

      <v-expansion-panels v-if="loggedIn" variant="accordion">
        <v-expansion-panel value="config" class="mt-2">
          <v-expansion-panel-title class="px-2">
            <template #default="{ expanded }">
              <v-list-item class="pa-0">
                <template #prepend>
                  <v-icon :icon="expanded ? 'mdi-cog' : 'mdi-cog-outline'" />
                </template>
                <v-list-item-title class="text-body-1 font-weight-medium">
                  Configuration
                </v-list-item-title>
              </v-list-item>
            </template>
          </v-expansion-panel-title>
          <v-expansion-panel-text>
            <v-list density="compact" class="py-0">
              <v-list-item
                v-for="cLink in configurationLinks"
                :key="cLink.label"
                :to="{ name: cLink.routeName }"
                :title="cLink.label"
                link
                class="rounded-lg my-1"
                variant="text"
                @click="mobileDrawer = false"
              />
            </v-list>
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>
    </v-list>

    <template #append>
      <div class="pa-3">
        <v-divider class="mb-3" />
        
        <v-btn
          @click="toggleTheme"
          variant="text"
          block
          class="justify-start mb-2 rounded-lg"
          height="48"
        >
          <template #prepend>
            <v-icon :icon="darkTheme ? 'mdi-weather-sunny' : 'mdi-weather-night'" />
          </template>
          <span class="text-body-2">
            {{ darkTheme ? 'Mode clair' : 'Mode sombre' }}
          </span>
        </v-btn>
      </div>
    </template>
  </v-navigation-drawer>
</template>


<script setup>
import { ref, computed, onMounted } from 'vue'
import { useDisplay, useTheme } from 'vuetify'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/store/auth'
import logoImageDark from '@/assets/logo.png' // Logo foncé (pour fond clair)
import logoImageWhite from '@/assets/logo_white.png' // Logo clair (pour fond foncé)

const mobileDrawer = ref(false)
const router = useRouter()
const authStore = useAuthStore()
const { mdAndUp, mdAndDown } = useDisplay()
const { global: theme } = useTheme()

const isDesktop = computed(() => mdAndUp.value)
const isMobile = computed(() => mdAndDown.value)
const darkTheme = computed(() => theme.current.value.dark)
const loggedIn = computed(() => authStore.loggedIn)

// Déterminez l'URL de la plateforme MVC en fonction de l'environnement (Prod vs Dev)
const MVC_URL = import.meta.env.PROD 
    ? 'https://cours.confluences.ch/' 
    : 'http://localhost:5002'

const currentLogo = computed(() => {
    return darkTheme.value ? logoImageWhite : logoImageDark
})

const goToLogin = () => {
    router.push('/login')
    mobileDrawer.value = false
}

const toggleTheme = () => {
    theme.name.value = darkTheme.value ? 'light' : 'dark'
}

onMounted(() => {
    authStore.restoreSession()
})

// Liens de Navigation Internes
const internalLinks = [
    { label: 'Entreprises', routeName: 'Entreprises', icon: 'mdi-office-building-outline', external: false },
    { label: 'Expériences professionnelles', routeName: 'Stages', icon: 'mdi-briefcase-outline', external: false },
    { label: 'Stages par stagiaire', routeName: 'Stagiaires', icon: 'mdi-account-hard-hat', external: false },
    { label: 'Contacts', routeName: 'Contacts', icon: 'mdi-card-account-mail', external: false },
    { label: 'ARE', routeName: 'JobSearchAssistances', icon: 'mdi-account-cash', external: false },
    { label: 'Registre de Présence', routeName: 'AttendanceRegister', icon: 'mdi-calendar-check', external: false },
]

// Nouveau Lien de Navigation Externe
const externalLink = { 
    label: 'Plateforme MVC', 
    url: MVC_URL, 
    icon: 'mdi-web', 
    external: true 
}

const configurationLinks = [
    { label: 'Affiliations', routeName: 'Affiliations' },
    { label: 'Annonces de stages', routeName: 'Annonces' },
    { label: 'Domaines', routeName: 'Domaines' },
    { label: 'Métiers', routeName: 'Metiers' },
    { label: 'Communications', routeName: 'Moyens' },
    { label: 'Offres', routeName: 'Offres' },
    { label: "Catégorie d'entreprise", routeName: 'Type-Entreprises' },
    { label: "Taux d'occupation", routeName: 'Type-Stages' },
    { label: "Types d'activités", routeName: 'Activites' },
    { label: 'Types ARE', routeName: 'TypeJobSearchAssistances' },
    { label: 'Occurences ARE', routeName: 'TypeJobSearchAssistanceOccurences' }
]

// Affiche les liens internes + le lien externe si l'utilisateur est connecté
const visibleLinks = computed(() => (loggedIn.value ? [...internalLinks, externalLink] : []))
</script>

<style scoped>
.border-b {
  border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}

.border-e {
  border-right: 1px solid rgba(0, 0, 0, 0.12);
}

:deep(.v-list-item--active) {
  background-color: rgba(var(--v-theme-primary), 0.08);
  color: rgb(var(--v-theme-primary));
}

:deep(.v-expansion-panel-title__overlay) {
  background-color: transparent;
}

.logo {
  padding: 10px; 
  width: 100%; 
  height: 64px; 
  display: flex;
  align-items: center;
  justify-content: center;
}
.logo-img {
  max-width: 100%; 
  height: auto;
  object-fit: contain;
  max-height: 40px; 
}

@media (max-width: 720px) {
  .logo {
    height: 48px; 
    justify-content: flex-start;
  }
}
</style>