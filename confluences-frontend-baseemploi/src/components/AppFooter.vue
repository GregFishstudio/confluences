<template>
  <v-footer 
    color="surface" 
    padless 
    class="border-t"
  >
    <v-row justify="center" no-gutters>
      <v-col 
        class="py-2 text-center text-caption" 
        cols="12"
      >
        <span class="text-medium-emphasis">
            {{ new Date().getFullYear() }} — 
        </span>
        <strong class="text-primary">{{ name }}</strong>
      </v-col>
    </v-row>
  </v-footer>
</template>

<script>
// Utiliser la Composition API est recommandé, mais en restant sur la syntaxe Options API:
export default {
  data() {
    return {
      name: 'Confluences - Gestion des stagiaires'
    }
  }
}
</script>

<style scoped>
/* Ajout d'une bordure fine pour séparer le footer du contenu, qui change avec le thème */
.border-t {
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}
</style>