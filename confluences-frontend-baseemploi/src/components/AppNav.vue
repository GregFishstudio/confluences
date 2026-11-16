 <template>
  <v-navigation-drawer
    v-if="isDesktop"
    permanent
    location="left"
    width="280"
    :color="darkTheme ? '#1A237E' : '#E3F2FD'" 
    elevation="0"
    class="main-sidebar"
  >
    <div class="logo-container">
      <img :src="currentLogo" alt="Logo" class="logo-img"/>
    </div>

    <div v-if="loggedIn" class="px-4 pb-3 text-center">
      <span class="text-body-2 font-weight-bold" style="color: #1976D2;">
        Bonjour : {{ authStore.user?.profile?.given_name || authStore.user?.profile?.name || '—' }}
      </span>
    </div>

    <v-list density="comfortable" nav class="px-4">
      <v-list-subheader class="text-uppercase font-weight-bold text-caption">Menu Mesures</v-list-subheader>

      <v-list-item
        v-for="link in mesuresLinks"
        :key="link.label"
        :to="{ name: link.routeName }"
        :prepend-icon="link.icon"
        :title="link.label"
        link
        color="primary"
        class="mb-2 menu-item"
        variant="flat"
      />

      <v-list-subheader class="text-uppercase font-weight-bold text-caption">Menu Participant-e-s</v-list-subheader>

      <v-list-item
        v-for="link in participantsLinks"
        :key="link.label"
        :to="{ name: link.routeName }"
        :prepend-icon="link.icon"
        :title="link.label"
        link
        color="primary"
        class="mb-2 menu-item"
        variant="flat"
      />

      <v-list-subheader class="text-uppercase font-weight-bold text-caption">Menu Insertion</v-list-subheader>

      <v-list-item
        v-for="link in insertionLinks"
        :key="link.label"
        :to="{ name: link.routeName }"
        :prepend-icon="link.icon"
        :title="link.label"
        link
        color="primary"
        class="mb-2 menu-item"
        variant="flat"
      />

      <v-list-subheader class="text-uppercase font-weight-bold text-caption">Menu Marrainage</v-list-subheader>

      <v-list-item
        v-for="link in marrainageLinks"
        :key="link.label"
        :to="{ name: link.routeName }"
        :prepend-icon="link.icon"
        :title="link.label"
        link
        color="primary"
        class="mb-2 menu-item"
        variant="flat"
      />

      <v-list-subheader class="text-uppercase font-weight-bold text-caption">Menu Configuration</v-list-subheader>

      <v-list-group v-if="loggedIn" value="config">
        <template #activator="{ props }">
          <v-list-item
            v-bind="props"
            prepend-icon="mdi-tune-variant"
            title="Configuration"
            class="menu-item"
          ></v-list-item>
        </template>

        <v-list-item
          v-for="cLink in configurationLinks"
          :key="cLink.label"
          :to="{ name: cLink.routeName }"
          :title="cLink.label"
          size="small"
          class="sub-item"
        />
      </v-list-group>
    </v-list>

    <template #append>
      <div class="pa-4 bottom-actions">
        <div v-if="loggedIn && authStore.user?.profile?.email" class="text-caption mb-2 px-1" style="opacity:0.75; word-break:break-all;">
          Connecté·e : {{ authStore.user.profile.email }}
        </div>

        <v-btn
          block
          :color="loggedIn ? 'error' : 'primary'"
          variant="tonal"
          class="font-weight-bold"
          @click="loggedIn ? authStore.logout() : goToLogin()"
          :prepend-icon="loggedIn ? 'mdi-power' : 'mdi-login-variant'"
        >
          {{ loggedIn ? 'Déconnexion' : 'Connexion' }}
        </v-btn>
      </div>
    </template>
  </v-navigation-drawer>

<v-navigation-drawer
  v-model="mobileDrawer"
  v-if="isMobile"
  temporary
  width="300"
>
  <div class="logo-container">
    <img :src="currentLogo" alt="Logo" class="logo-img" style="max-height: 40px;"/>
  </div>

  <v-list nav class="pa-4">
    <v-list-subheader class="text-uppercase font-weight-bold text-caption">Menu Mesures</v-list-subheader>
    <v-list-item
      v-for="link in mesuresLinks"
      :key="link.label"
      :to="{ name: link.routeName }"
      :prepend-icon="link.icon"
      :title="link.label"
      color="primary"
      @click="mobileDrawer = false"
    />

    <v-list-subheader class="text-uppercase font-weight-bold text-caption mt-2">Menu Participant-e-s</v-list-subheader>
    <v-list-item
      v-for="link in participantsLinks"
      :key="link.label"
      :to="{ name: link.routeName }"
      :prepend-icon="link.icon"
      :title="link.label"
      color="primary"
      @click="mobileDrawer = false"
    />

    <v-list-subheader class="text-uppercase font-weight-bold text-caption mt-2">Menu Insertion</v-list-subheader>
    <v-list-item
      v-for="link in insertionLinks"
      :key="link.label"
      :to="{ name: link.routeName }"
      :prepend-icon="link.icon"
      :title="link.label"
      color="primary"
      @click="mobileDrawer = false"
    />

    <v-list-subheader class="text-uppercase font-weight-bold text-caption mt-2">Menu Marrainage</v-list-subheader>
    <v-list-item
      v-for="link in marrainageLinks"
      :key="link.label"
      :to="{ name: link.routeName }"
      :prepend-icon="link.icon"
      :title="link.label"
      color="primary"
      @click="mobileDrawer = false"
    />

    <v-list-subheader class="text-uppercase font-weight-bold text-caption mt-2">Menu Configuration</v-list-subheader>
    <v-list-item
      v-for="cLink in configurationLinks"
      :key="cLink.label"
      :to="{ name: cLink.routeName }"
      :title="cLink.label"
      size="small"
      @click="mobileDrawer = false"
    />
  </v-list>

  <template #append>
    <div class="pa-4">
      <div v-if="loggedIn && authStore.user?.profile?.email" class="text-caption mb-2 px-1" style="opacity:0.75; word-break:break-all;">
        Connecté·e : {{ authStore.user.profile.email }}
      </div>
      <v-btn
        block
        :color="loggedIn ? 'error' : 'primary'"
        variant="tonal"
        class="font-weight-bold"
        @click="loggedIn ? authStore.logout() : goToLogin()"
        :prepend-icon="loggedIn ? 'mdi-power' : 'mdi-login-variant'"
      >
        {{ loggedIn ? 'Déconnexion' : 'Connexion' }}
      </v-btn>
    </div>
  </template>
</v-navigation-drawer>

<v-app-bar v-if="isMobile" flat color="primary">
  <v-app-bar-nav-icon color="white" @click="mobileDrawer = !mobileDrawer" />
  <v-toolbar-title class="text-white font-weight-bold">Confluences</v-toolbar-title>
</v-app-bar>

  
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
  ? 'https://test-cours.confluences.ch/' 
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

onMounted(async () => {
  authStore.restoreSession()
  const token = localStorage.getItem('access_token')
  if (token && !authStore.user?.profile?.preferred_username) {
    try {
      const authority = import.meta.env.VITE_AUTHORITY_URL || 'http://localhost:5000'
      const res = await fetch(authority + '/connect/userinfo', {
        headers: { Authorization: 'Bearer ' + token }
      })
      if (res.ok) {
        const info = await res.json()
        console.log('[AppNav] userinfo reçu:', info)
        const stored = JSON.parse(localStorage.getItem('user') || '{}')
        const updated = { ...stored, profile: { ...(stored.profile || {}), preferred_username: info.preferred_username || '', given_name: info.given_name || '', name: info.name || '' } }
        localStorage.setItem('user', JSON.stringify(updated))
        authStore.$patch({ user: updated })
      }
    } catch (e) { console.warn('[AppNav] userinfo indisponible:', e) }
  }
})

// Menu Mesures : cours suivis chez nous
const mesuresLinks = [
  { label: 'Fiche de présence', routeName: 'AttendanceRegister', icon: 'mdi-calendar-check', external: false },
  { label: 'Accomp. & Ateliers', routeName: 'AteliersFollowUp', icon: 'mdi-progress-check', external: false },
  { label: 'Bilans & attestations', routeName: 'StagiairesFollowUp', icon: 'mdi-account-eye', external: false },
  { label: 'Calendrier de formation', routeName: 'calendrier', icon: 'mdi-calendar-edit',  external: false },
  { label: 'Inscriptions', routeName: 'Inscriptions', icon: 'mdi-account-multiple-plus-outline', external: false },
]

// Menu Participant-e-s : infos relatives aux participant·e·s
const participantsLinks = [
  { label: 'Liste des participant·e·s', routeName: 'Participants', icon: 'mdi-account',  external: false },
  { label: 'Expériences professionnelles', routeName: 'Stages', icon: 'mdi-briefcase-outline', external: false },
  { label: 'Stages par stagiaire', routeName: 'Stagiaires', icon: 'mdi-account-hard-hat', external: false },
  { label: 'Tableau de marrainage', routeName: 'Marrainage-Tableau', icon: 'mdi-map-marker-account-outline', external: false },
  { label: 'Stages & bénévolat', routeName: 'Stages-Tableau', icon: 'mdi-briefcase-search-outline', external: false },
]

// Menu Insertion : insertion socio-professionnelle
const insertionLinks = [
  { label: 'Stages & bénévolat', routeName: 'Stages-Tableau', icon: 'mdi-briefcase-search-outline', external: false },
  { label: 'Entreprises', routeName: 'Entreprises', icon: 'mdi-office-building-outline', external: false },
  { label: 'Contacts', routeName: 'Contacts', icon: 'mdi-card-account-mail', external: false },
  { label: 'ARE', routeName: 'JobSearchAssistances', icon: 'mdi-account-cash', external: false },
]

// Nouveau Lien de Navigation Externe
const externalLink = { 
  label: 'Plateforme MVC', 
  url: MVC_URL, 
  icon: 'mdi-web', 
  external: true 
}

const marrainageLinks = [
  { label: 'Tableau de marrainage', routeName: 'Marrainage-Tableau', icon: 'mdi-map-marker-account-outline', external: false },
  { label: 'Liste des marrainages', routeName: 'Marrainages', icon: 'mdi-account-heart-outline', external: false },
  { label: 'Nouveau marrainage', routeName: 'Marrainage-Nouveau', icon: 'mdi-account-plus-outline', external: false },
]

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
const visibleLinks = computed(() => (loggedIn.value ? [...insertionLinks, externalLink] : []))
</script>

<style scoped>
/* Conteneur principal du menu avec une bordure subtile plutôt qu'une ombre portée */
.main-sidebar {
  border-right: 1px solid v-bind('darkTheme ? "#283593" : "#BBDEFB"') !important;
}

.logo-container {
  padding: 32px 24px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.logo-img {
  max-height: 48px;
  /* On booste un peu le logo sur le fond bleu */
  filter: drop-shadow(0px 4px 6px rgba(0,0,0,0.1));
}

/* Éléments du menu */
.menu-item {
  margin-bottom: 4px;
  border-radius: 12px !important;
  font-weight: 600 !important;
  /* Couleur du texte quand non-actif */
  color: v-bind('darkTheme ? "#C5CAE9" : "#1976D2"') !important;
}

.menu-item:hover {
  background-color: v-bind('darkTheme ? "#283593" : "#BBDEFB"') !important;
  transform: translateX(5px);
}

/* État Actif : Très contrasté */
:deep(.v-list-item--active) {
  background: #1976D2 !important; /* Bleu soutenu */
  color: white !important;
  box-shadow: 0 4px 15px rgba(25, 118, 210, 0.4);
}

/* Icônes */
:deep(.v-list-item__prepend > .v-icon) {
  opacity: 1;
  color: inherit;
}

/* Configuration (Sous-menu) */
.sub-item {
  margin-left: 12px;
  font-size: 0.85rem !important;
  opacity: 0.8;
}

/* Zone du bas (Actions) */
.bottom-actions {
  /* Fond un peu plus sombre que le reste du menu pour créer un bloc */
  background: v-bind('darkTheme ? "#121858" : "#D1E9FF"');
  border-top: 1px solid v-bind('darkTheme ? "#283593" : "#BBDEFB"');
  padding: 16px;
}

.action-btn {
  font-weight: 600 !important;
  color: v-bind('darkTheme ? "#C5CAE9" : "#1565C0"') !important;
}
</style>