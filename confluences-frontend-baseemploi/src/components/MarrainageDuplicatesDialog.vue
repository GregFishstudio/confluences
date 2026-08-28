<!--
  Détecte les marrainages en double (même si Prénom/Nom sont inversés entre deux fiches)
  et propose une fusion en un clic : tout est rapatrié vers la fiche conservée (réponses au
  questionnaire, commentaires, fichiers, participant·e·s lié·e·s, champs vides complétés).
-->
<template>
  <v-dialog v-model="dialog" max-width="800" @update:model-value="onOpen">
    <template #activator="{ props }">
      <v-btn v-bind="props" variant="outlined" prepend-icon="mdi-set-merge">
        Détecter les doublons
      </v-btn>
    </template>

    <v-card rounded="lg">
      <v-card-title class="py-4">Doublons détectés</v-card-title>
      <v-divider />
      <v-card-text class="pt-6">
        <div v-if="loading" class="text-center pa-8">
          <v-progress-circular indeterminate color="primary" />
        </div>
        <div v-else-if="!groups.length" class="text-body-2 text-medium-emphasis text-center pa-4">
          Aucun doublon détecté (comparaison par prénom + nom, insensible à l'ordre, aux accents
          et aux petites fautes de frappe).
        </div>
        <div v-for="(group, gi) in groups" :key="gi" class="duplicate-group mb-4 pa-3">
          <div class="text-caption text-grey mb-2">
            Groupe {{ gi + 1 }} — {{ group.items.length }} fiches
            <span v-if="group.fuzzy" class="text-orange">— orthographe légèrement différente, à vérifier</span>
          </div>
          <div v-for="item in group.items" :key="item.marrainageId" class="d-flex align-center ga-2 mb-1">
            <v-radio-group v-model="group.keeperId" hide-details density="compact" class="mr-0">
              <v-radio :value="item.marrainageId" density="compact" />
            </v-radio-group>
            <div class="flex-grow-1">
              <div class="text-body-2">
                <strong>{{ item.prenom }} {{ item.nom }}</strong>
                <span class="text-caption text-grey ml-1">(#{{ item.marrainageId }})</span>
              </div>
              <div class="text-caption text-grey">
                {{ item.numero || '—' }} · {{ item.email || '—' }} · {{ item.commune || '—' }}
                · {{ item.actif ? 'Actif' : 'Inactif' }}
              </div>
            </div>
          </div>
          <div class="d-flex justify-end mt-2">
            <v-btn
              size="small" color="primary" variant="outlined"
              :loading="merging === gi"
              @click="mergeGroup(group, gi)"
            >
              Fusionner dans la fiche sélectionnée
            </v-btn>
          </div>
        </div>
      </v-card-text>
      <v-divider />
      <v-card-actions class="d-flex justify-end">
        <v-btn variant="text" color="grey" @click="dialog = false">Fermer</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref } from 'vue'
import { useStore } from 'vuex'
import MarrainageService from '@/services/marrainageService.js'

const store = useStore()
const emit = defineEmits(['merged'])

const dialog = ref(false)
const loading = ref(false)
const merging = ref(null)
const groups = ref([])

function normalize(str) {
  return (str || '')
    .toString()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/[^a-z0-9]+/g, ' ')
    .trim()
}

// Signature indépendante de l'ordre Prénom/Nom : regroupe même si les deux champs
// ont été inversés entre deux fiches (bug d'import fréquent).
function nameSignature(item) {
  const words = [...normalize(item.prenom).split(' '), ...normalize(item.nom).split(' ')]
    .filter(Boolean)
    .sort()
  return words.join(' ')
}

function levenshtein(a, b) {
  const m = a.length
  const n = b.length
  if (m === 0) return n
  if (n === 0) return m
  const dp = Array.from({ length: m + 1 }, () => new Array(n + 1).fill(0))
  for (let i = 0; i <= m; i++) dp[i][0] = i
  for (let j = 0; j <= n; j++) dp[0][j] = j
  for (let i = 1; i <= m; i++) {
    for (let j = 1; j <= n; j++) {
      const cost = a[i - 1] === b[j - 1] ? 0 : 1
      dp[i][j] = Math.min(dp[i - 1][j] + 1, dp[i][j - 1] + 1, dp[i - 1][j - 1] + cost)
    }
  }
  return dp[m][n]
}

// Fautes de frappe fréquentes ("Barile"/"Barille", "Mohamed"/"Mohammed") : le prénom doit
// être quasi identique, le nom peut différer de 1-2 lettres (mais pas être identique,
// sinon il aurait déjà été regroupé par nameSignature).
function isFuzzyMatch(a, b) {
  const prenomA = normalize(a.prenom)
  const prenomB = normalize(b.prenom)
  const nomA = normalize(a.nom)
  const nomB = normalize(b.nom)
  if (!prenomA || !prenomB || !nomA || !nomB) return false
  const prenomDist = levenshtein(prenomA, prenomB)
  const nomDist = levenshtein(nomA, nomB)
  return prenomDist <= 1 && nomDist >= 1 && nomDist <= 2
}

function completenessScore(item) {
  const fields = [
    'numero', 'commune', 'email', 'age', 'etatCivil', 'profession', 'periodeDemande',
    'remarqueParrain', 'filleulNom', 'telephoneFilleul', 'emailFilleul', 'suivi',
    'extraitCasier', 'relanceNotes', 'date',
  ]
  let score = fields.filter(f => item[f] !== null && item[f] !== undefined && item[f] !== '').length
  if (item.charteSignee) score += 1
  return score
}

async function onOpen(isOpen) {
  if (!isOpen) return
  loading.value = true
  try {
    const all = await store.dispatch('marrainage/fetchMarrainages', {})
    const bySignature = new Map()
    for (const item of all) {
      const sig = nameSignature(item)
      if (!sig) continue
      if (!bySignature.has(sig)) bySignature.set(sig, [])
      bySignature.get(sig).push(item)
    }

    const exactGroups = Array.from(bySignature.values()).filter(items => items.length > 1)
    const singletons = Array.from(bySignature.values())
      .filter(items => items.length === 1)
      .map(items => items[0])

    // Regroupement approximatif (fautes de frappe) parmi les fiches non déjà groupées
    // exactement, via un simple union-find sur les paires jugées similaires.
    const parent = singletons.map((_, i) => i)
    function find(i) { return parent[i] === i ? i : (parent[i] = find(parent[i])) }
    function union(i, j) { const ri = find(i), rj = find(j); if (ri !== rj) parent[ri] = rj }

    for (let i = 0; i < singletons.length; i++) {
      for (let j = i + 1; j < singletons.length; j++) {
        if (isFuzzyMatch(singletons[i], singletons[j])) union(i, j)
      }
    }
    const fuzzyClusters = new Map()
    singletons.forEach((item, i) => {
      const root = find(i)
      if (!fuzzyClusters.has(root)) fuzzyClusters.set(root, [])
      fuzzyClusters.get(root).push(item)
    })

    const fuzzyGroups = Array.from(fuzzyClusters.values()).filter(items => items.length > 1)

    groups.value = [
      ...exactGroups.map(items => ({ items, fuzzy: false })),
      ...fuzzyGroups.map(items => ({ items, fuzzy: true })),
    ].map(({ items, fuzzy }) => {
      const sorted = [...items].sort((a, b) => completenessScore(b) - completenessScore(a))
      return { items: sorted, keeperId: sorted[0].marrainageId, fuzzy }
    })
  } finally {
    loading.value = false
  }
}

async function mergeGroup(group, index) {
  merging.value = index
  try {
    const others = group.items.filter(i => i.marrainageId !== group.keeperId)
    for (const other of others) {
      await MarrainageService.mergeMarrainage(group.keeperId, other.marrainageId)
    }
    groups.value = groups.value.filter((_, i) => i !== index)
    emit('merged')
  } finally {
    merging.value = null
  }
}
</script>

<style scoped>
.duplicate-group {
  border: 1px solid rgba(0, 0, 0, 0.1);
  border-radius: 8px;
}
</style>
