<!--  
  Projet: Gestion des stagiaires
  Migration Vue 3 + Vuetify 3
  Fichier : NotificationContainer.vue
-->

<template>
  <div class="notification-container">
    <transition-group
      name="fade"
      tag="div"
      class="notification-list"
      appear
    >
      <NotificationBar
        v-for="notif in notifications"
        :key="notif.id"
        :notification="notif"
      />
    </transition-group>
  </div>
</template>

<script>
import NotificationBar from "@/components/NotificationBar.vue";
import { mapState } from "vuex";

export default {
  components: { NotificationBar },

  computed: {
    ...mapState("notification", ["notifications"]),
  },
};
</script>

<style scoped>
.notification-container {
  position: fixed;
  top: 70px;
  right: 20px;
  z-index: 9999;
  width: 360px;
  pointer-events: none; /* Permet de cliquer à travers */
}

.notification-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

/* Animation fade pour le groupe */
.fade-enter-active,
.fade-leave-active {
  transition: all 0.25s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
