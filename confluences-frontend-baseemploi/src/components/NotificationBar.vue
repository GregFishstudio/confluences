<!--  
  Projet: Gestion des stagiaires
  Migration Vue 3 + Vuetify 3
  Composant : NotificationBar.vue
-->

<template>
  <v-slide-y-transition>
    <div class="notification-bar">
      <v-alert
        :color="notificationColor"
        variant="tonal"
        rounded="lg"
        class="pa-3"
      >
        {{ notification.message }}
      </v-alert>
    </div>
  </v-slide-y-transition>
</template>

<script>
import { mapActions } from "vuex";

export default {
  props: {
    notification: {
      type: Object,
      required: true,
    },
  },

  data() {
    return {
      timeout: null,
    };
  },

  mounted() {
    this.timeout = setTimeout(() => {
      this.remove(this.notification);
    }, 2500);
  },

  beforeUnmount() {
    clearTimeout(this.timeout);
  },

  computed: {
    // Convertit "success", "error", "warning", "info" → vraies couleurs Vuetify 3
    notificationColor() {
      const colors = {
        success: "success",
        error: "error",
        warning: "warning",
        info: "info",
      };
      return colors[this.notification.type] || "primary";
    },
  },

  methods: {
    ...mapActions("notification", ["remove"]),
  },
};
</script>

<style scoped>
.notification-bar {
  margin: 12px 0;
}
</style>
