<template>
  <v-container fluid>
    <v-card class="mb-6 pa-4 elevation-3" rounded="lg">
      <v-row align="center">
        <v-col cols="12" sm="8" md="9">
          <v-text-field
            v-model="search"
            append-icon="mdi-magnify"
            label="Rechercher un contact..."
            placeholder="Nom, Prénom, Entreprise, Email..."
            outlined
            dense
            clearable
            hide-details
            @focus="updatePageSearch"
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="4" md="3" class="d-flex justify-end">
          <CreateContactFromList />
        </v-col>
      </v-row>
    </v-card>
    
    <v-card class="elevation-4" rounded="lg">
      <v-data-table
        :headers="headers"
        :items="Array.isArray(contact.contacts) ? contact.contacts : []"
        :items-per-page="settings.itemsPerPage"
        :search="search"
        class="contact-table"
        @click:row="selectRow"
        @update:items-per-page="updateNumberItems"
        v-model:page="page"
        mobile-breakpoint="0"
      >
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title class="text-h5 font-weight-bold">
              Liste des contacts
            </v-toolbar-title>
          </v-toolbar>
        </template>

        <template v-slot:item.nom="{ item }">
          <span class="font-weight-medium text-capitalize">
            {{ item.nom }}
          </span>
        </template>
        <template v-slot:item.prenom="{ item }">
          <span class="font-weight-medium text-capitalize">
            {{ item.prenom }}
          </span>
        </template>

        <template v-slot:item.entreprise.nom="{ item }">
          <v-chip small color="blue-grey lighten-5" text-color="blue-grey darken-3">
            {{ item.entreprise.nom }}
          </v-chip>
        </template>
        
        <template v-slot:item.fonction="{ item }">
          <span class="text-caption grey--text text--darken-2">
            {{ item.fonction }}
          </span>
        </template>

        <template v-slot:item.email="{ item }">
            <a :href="'mailto:' + item.email" class="text-decoration-none">
              <v-icon small color="blue" class="mr-1">mdi-email</v-icon>{{ item.email }}
            </a>
        </template>
        
        <template v-slot:item.telFix="{ item }">
          <a v-if="item.telFix" :href="'tel:' + item.telFix" class="text-decoration-none">
            <v-icon small color="green darken-2" class="mr-1">mdi-phone</v-icon>{{ item.telFix }}
          </a>
          <span v-else class="grey--text">N/A</span>
        </template>

        <template v-slot:item.natel="{ item }">
          <a v-if="item.natel" :href="'tel:' + item.natel" class="text-decoration-none">
            <v-icon small color="green darken-3" class="mr-1">mdi-cellphone</v-icon>{{ item.natel }}
          </a>
          <span v-else class="grey--text">N/A</span>
        </template>
        
        <template v-slot:no-data>
            <div class="pa-4 text-center">
                <v-icon large color="grey lighten-1" class="mb-2">mdi-alert-circle-outline</v-icon>
                <p class="text-subtitle-1 grey--text">
                    Aucun contact trouvé.
                </p>
            </div>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, computed, onBeforeMount } from 'vue';
import { useStore } from 'vuex';
import { useRouter } from 'vue-router';
import { useListMemory } from '@/composables/useListMemory';
import CreateContactFromList from '@/components/CreateContactFromList.vue';

const store = useStore();
const router = useRouter();

// --- État Réactif ---
const search = ref('');
const page = ref(1);

// --- Computed Properties ---
const contact = computed(() => store.state.contact);
const settings = computed(() => store.state.settings);

// --- Données Statiques ---
const headers = [
    { text: 'Entreprise', value: 'entreprise.nom' },
    { text: 'Nom', value: 'nom', class: 'font-weight-bold' },
    { text: 'Prénom', value: 'prenom', class: 'font-weight-bold' },
    { text: 'Fonction', value: 'fonction' },
    { text: 'Email', value: 'email' },
    { text: 'Téléphone fixe', value: 'telFix' },
    { text: 'Natel', value: 'natel' }
];

// --- Hooks ---

// FIX: Charger les données au montage pour résoudre le problème de rechargement après l'édition
onBeforeMount(() => {
    store.dispatch('contact/fetchContacts', true);
});

// Retour depuis une fiche : même recherche, même page, même position.
// (`:options.sync` de Vuetify 2 ne mémorisait plus la page.)
useListMemory('contacts', { search }, {
    page,
    ready: () => Array.isArray(contact.value.contacts) && contact.value.contacts.length > 0,
});

// --- Méthodes ---
const selectRow = (event, row) => {
    const item = row ? row.item : event;
    const id = item.contactId || item.ContactId;

    if (id) {
        router.push({ name: 'Contact-Modifier', params: { id: id } });
    } else {
        console.error("Erreur: ID de contact manquant lors de la sélection.");
    }
};

const updateNumberItems = (event) => {
    store.dispatch('settings/setItemsPerPage', { number: event });
};

const updatePageSearch = () => {
    page.value = 1;
};
</script>

<style scoped>
/* Styles pour un look moderne et cliquable */
.contact-table >>> tbody tr:hover {
  cursor: pointer;
  /* Ombre plus douce */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
  transition: all 0.2s ease-in-out;
}

.contact-table >>> thead th {
    /* En-têtes discrets */
    color: #424242 !important; 
    font-size: 0.85rem;
    letter-spacing: 0.5px;
}
</style>