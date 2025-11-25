<template>
  <div class="attendance-register p-6">
    <h1 class="text-3xl font-bold mb-6 text-primary">Registre de Présence</h1>
    
    <div class="controls mb-6 flex space-x-4 items-center">
      <div class="date-select">
        <label for="start-date" class="text-sm font-semibold text-gray-700">Début de période :</label>
        <input type="date" id="start-date" v-model="startDate" @change="generateDates"
               class="p-2 border rounded shadow-sm focus:ring-accent focus:border-accent">
      </div>
      <div class="date-select">
        <label for="end-date" class="text-sm font-semibold text-gray-700">Fin de période :</label>
        <input type="date" id="end-date" v-model="endDate" @change="generateDates"
               class="p-2 border rounded shadow-sm focus:ring-accent focus:border-accent">
      </div>
    </div>

    <div class="overflow-x-auto shadow-lg rounded-lg border border-gray-200">
      <table class="min-w-full divide-y divide-gray-300">
        <thead class="bg-primary text-white sticky top-0">
          <tr>
            <th class="p-3 text-left w-64">Stagiaire</th>
            <th v-for="date in days" :key="date.full" class="p-3 text-center w-12 border-l border-gray-400">
              <div class="text-xs">{{ date.dayOfWeek }}</div>
              <div class="text-sm font-bold">{{ date.dayOfMonth }}</div>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100 bg-white">
          <tr v-for="stagiaire in attendanceData" :key="stagiaire.id">
            <td class="p-3 font-semibold text-primary">
              {{ stagiaire.name }}
            </td>
            <td v-for="day in days" :key="day.full" class="p-1 text-center border-l border-gray-200 cursor-pointer">
              <div 
                :class="getStatusClass(stagiaire.attendance[day.full])"
                @click="cycleStatus(stagiaire.id, day.full)"
                class="w-full h-full p-2 rounded transition duration-150 ease-in-out transform hover:scale-105"
                v-tooltip="getStatusTooltip(stagiaire.attendance[day.full])"
              >
                {{ stagiaire.attendance[day.full] }}
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="legend mt-6 p-4 bg-gray-50 rounded-lg border">
        <h3 class="font-semibold text-primary mb-3">Légende</h3>
        <div class="flex flex-wrap gap-x-6 gap-y-2 text-sm">
            <div v-for="(label, code) in statusMapping" :key="code" class="flex items-center">
                <span :class="getStatusClass(code)" class="w-6 h-6 rounded mr-2 flex items-center justify-center text-xs font-bold text-white shadow-sm">
                    {{ code }}
                </span>
                <span>{{ label }}</span>
            </div>
        </div>
    </div>

    <div class="actions mt-8 flex justify-end space-x-4">
      <button @click="saveAttendance"
        class="bg-accent text-white py-3 px-6 rounded-lg font-bold shadow-md hover:bg-accent-dark transition duration-200 focus:outline-none focus:ring-2 focus:ring-accent-dark focus:ring-offset-2">
        Sauvegarder les Présences ({{ modifiedCount }} modifiés)
      </button>

      <button @click="generatePdf"
        class="bg-gray-500 text-white py-3 px-6 rounded-lg font-bold shadow-md hover:bg-gray-700 transition duration-200 focus:outline-none focus:ring-2 focus:ring-gray-700 focus:ring-offset-2">
        Générer PDF d'Attestation
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { format, eachDayOfInterval } from 'date-fns';
import { fr } from 'date-fns/locale';

// --- Configuration des Couleurs (Harmonisation UI) ---
const PRIMARY_COLOR = '#2D3748';
const ACCENT_COLOR = '#00B4B0';
const LIGHT_GREY = '#F7FAFC';

// --- Statuts et Légende ---
const statusMapping = {
  'p': 'Participation',
  'e': 'Excusé',
  'a': 'Non participation / Absent',
  'r': 'Retard de 15+ min',
  's': 'Stage / Externe',
  '': '--- (Non Noté)',
};

const statusCycle = ['', 'p', 'e', 'a', 'r', 's']; // Ordre de cycle

// --- État Local ---
const startDate = ref(format(new Date(), 'yyyy-MM-dd'));
const endDate = ref(format(new Date(), 'yyyy-MM-dd'));
const days = ref([]);

// Données fictives des stagiaires (à remplacer par un appel API)
const initialAttendanceData = [
  { id: 1, name: 'ABU SHALAH Walaa', attendance: {} },
  { id: 2, name: 'COLLU Servet', attendance: {} },
  { id: 3, name: 'DAHMAN Riham', attendance: {} },
  { id: 4, name: 'IBRAHIM Khadega', attendance: {} },
  { id: 5, name: 'KAPLAN Caner', attendance: {} },
  // ... ajoutez tous les stagiaires ...
];

const attendanceData = ref(JSON.parse(JSON.stringify(initialAttendanceData))); // Données actuelles
const originalAttendanceData = ref(JSON.parse(JSON.stringify(initialAttendanceData))); // Pour le suivi des modifications

// --- Fonctions de Période et Date ---

/** Génère la liste des jours entre les dates de début et de fin. */
function generateDates() {
  if (!startDate.value || !endDate.value) return;

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);

  if (start > end) return;

  const dates = eachDayOfInterval({ start, end });
  days.value = dates.map(date => ({
    full: format(date, 'yyyy-MM-dd'),
    dayOfMonth: format(date, 'dd'),
    dayOfWeek: format(date, 'eee', { locale: fr }),
  }));
}

// Initialiser les dates à la semaine en cours par défaut
// Ceci est une implémentation simple, à adapter selon vos besoins de période.
const today = new Date();
const oneWeekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
startDate.value = format(oneWeekAgo, 'yyyy-MM-dd');
endDate.value = format(today, 'yyyy-MM-dd');
generateDates();


// --- Logique d'Interaction et de Statut ---

/** Récupère la classe CSS pour le statut donné. */
function getStatusClass(status) {
  const base = 'text-center text-xs font-bold';
  switch (status) {
    case 'p': return `${base} bg-green-200 text-green-800`;
    case 'r': return `${base} bg-yellow-200 text-yellow-800`;
    case 'a': return `${base} bg-red-200 text-red-800`;
    case 'e': return `${base} bg-blue-200 text-blue-800`;
    case 's': return `${base} bg-purple-200 text-purple-800`;
    default: return `${base} bg-gray-100 text-gray-500`;
  }
}

/** Récupère l'info-bulle pour le statut. */
function getStatusTooltip(status) {
    return statusMapping[status] || statusMapping[''];
}

/** Fait défiler le statut pour un stagiaire et un jour donné. */
function cycleStatus(stagiaireId, date) {
  const stagiaire = attendanceData.value.find(s => s.id === stagiaireId);
  if (!stagiaire) return;

  const currentStatus = stagiaire.attendance[date] || '';
  const currentIndex = statusCycle.indexOf(currentStatus);
  const nextIndex = (currentIndex + 1) % statusCycle.length;
  
  stagiaire.attendance[date] = statusCycle[nextIndex];
}

/** Compte le nombre de modifications. */
const modifiedCount = computed(() => {
  let count = 0;
  attendanceData.value.forEach((stagiaire, index) => {
    const original = originalAttendanceData.value[index];
    days.value.forEach(day => {
      const dateKey = day.full;
      const currentStatus = stagiaire.attendance[dateKey] || '';
      const originalStatus = original.attendance[dateKey] || '';
      
      if (currentStatus !== originalStatus) {
        count++;
      }
    });
  });
  return count;
});


// --- Connexion au Backend (Simulée) ---

/** Prépare les données pour l'API et les envoie. */
async function saveAttendance() {
  const payload = [];

  attendanceData.value.forEach(stagiaire => {
    days.value.forEach(day => {
      const status = stagiaire.attendance[day.full] || '';
      
      // N'envoyer que les entrées notées (ou modifiées, selon la logique)
      if (status !== '') {
        payload.push({
          StagiaireId: stagiaire.id,
          Date: day.full,
          Statut: status
        });
      }
    });
  });

  if (payload.length === 0) {
      alert('Aucune présence à enregistrer.');
      return;
  }

  // SIMULATION D'APPEL API
  // try {
  //   await axios.post('/api/attendance/save', payload); // <-- Remplacez par votre endpoint
  //   alert('Présences sauvegardées avec succès !');
  //   // Mettre à jour l'état original après la sauvegarde réussie
  //   originalAttendanceData.value = JSON.parse(JSON.stringify(attendanceData.value));
  // } catch (error) {
  //   console.error('Erreur lors de la sauvegarde :', error);
  //   alert('Échec de la sauvegarde des présences.');
  // }
  
  console.log('Payload de sauvegarde :', payload);
  alert(`Simulation : ${payload.length} entrées prêtes à être sauvegardées. Vérifiez la console.`);
}

/** Déclenche la génération et le téléchargement du PDF. */
async function generatePdf() {
  // SIMULATION D'APPEL API
  const start = startDate.value;
  const end = endDate.value;
  
  // Dans une vraie application, vous appelleriez : 
  // const url = `/api/attendance/pdf?startDate=${start}&endDate=${end}`;
  // window.open(url, '_blank'); // Ouvre le PDF dans un nouvel onglet
  
  alert(`Simulation : Appel à l'API pour générer le PDF du ${start} au ${end}.`);
}

</script>

<style scoped>
/* Styles de base pour l'harmonisation */
.text-primary { color: v-bind(PRIMARY_COLOR); }
.bg-primary { background-color: v-bind(PRIMARY_COLOR); }
.text-accent { color: v-bind(ACCENT_COLOR); }
.bg-accent { background-color: v-bind(ACCENT_COLOR); }
.hover\:bg-accent-dark:hover { background-color: #008f8c; /* Darker accent */ }
.focus\:ring-accent { --tw-ring-color: v-bind(ACCENT_COLOR); }

/* Styles spécifiques au tableau */
.attendance-register {
  max-width: 1200px;
  margin: 0 auto;
}

.min-w-full {
  min-width: 1000px; /* Assure que le tableau ne soit pas trop étroit */
}

/* Fixer la colonne stagiaire (nécessite des CSS plus complexes ou une librairie) */
/* Pour une solution simple en Vue, on stylise juste la première colonne */
td:first-child, th:first-child {
  position: sticky;
  left: 0;
  z-index: 10;
  background-color: #fff; /* S'assurer qu'elle couvre les cellules derrière */
  box-shadow: 2px 0 3px rgba(0,0,0,0.05);
}

th:first-child {
    background-color: v-bind(PRIMARY_COLOR) !important;
    color: white;
}
</style>