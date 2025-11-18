<!-- 
  -- Projet: Gestion des stagiaires
  -- Auteur : Tim Allemann
  -- Date : 16.09.2020
  -- Description : Composant permettant d'afficher des notifications à l'utilisateur
  -- Fichier : NotificationBar.vue
  -->

<template>
  <div class="notification-bar" :class="notificationTypeClass">
    <v-alert v-bind="{ type: notification.type }">
      {{ notification.message }}
    </v-alert>
  </div>
</template>

<script>
import { mapActions } from 'vuex'

export default {
  props: {
    notification: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      timeout: null
    }
  },
  // Supprime le composant après x secondes
  mounted() {
    this.timeout = setTimeout(() => this.remove(this.notification), 2000)
  },
  beforeDestroy() {
    clearTimeout(this.timeout)
  },
  computed: {
    notificationTypeClass() {
      return `-text-${this.notification.type}`
    }
  },
  methods: mapActions('notification', ['remove'])
}
</script>

<style scoped>
.notification-bar {
  margin: 1em 0 1em;
}
</style>
