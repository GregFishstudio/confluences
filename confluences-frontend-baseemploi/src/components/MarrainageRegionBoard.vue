<!--
  Vue "par région" de la liste de marrainage, sur le modèle du tableau des formatrices :
  binômes en cours, filleul·e·s à trouver et parrains/marraines disponibles, une colonne par région.
  Région d'une marraine/d'un parrain : saisie sur sa fiche, sinon déduite de sa commune ;
  d'un·e filleul·e : déduite de son adresse (fiche participant·e).
-->
<template>
  <div>
    <v-card v-for="table in tables" :key="table.key" class="mb-4 elevation-1" rounded="lg">
      <div class="d-flex align-center px-4 pt-3 pb-1">
        <v-icon :color="table.color" class="mr-2">{{ table.icon }}</v-icon>
        <span class="text-subtitle-1 font-weight-bold">{{ table.title }}</span>
        <v-chip size="small" class="ml-2" variant="tonal">{{ table.total }}</v-chip>
      </div>
      <div class="text-caption text-medium-emphasis px-4 pb-2">{{ table.hint }}</div>
      <div class="board-scroll">
        <div class="board">
          <div v-for="region in REGIONS" :key="region" class="board-col">
            <div class="board-head" :style="{ background: REGION_COLORS[region] }">
              {{ region }} <span class="board-count">{{ table.byRegion[region].length }}</span>
            </div>
            <div v-for="item in table.byRegion[region]" :key="item.key" class="board-card" @click="item.to && router.push(item.to)">
              <div class="font-weight-bold text-body-2">{{ item.title }}</div>
              <div v-if="item.sub" class="text-caption text-medium-emphasis">{{ item.sub }}</div>
              <div class="d-flex flex-wrap ga-1 mt-1">
                <v-chip v-for="t in item.tags" :key="t.label" size="x-small" :color="t.color" variant="tonal" label>{{ t.label }}</v-chip>
              </div>
            </div>
            <div v-if="!table.byRegion[region].length" class="board-empty">—</div>
          </div>
        </div>
      </div>
    </v-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import API_BASE, { apiFetch } from '@/services/api'
import { REGIONS, REGION_COLORS, regionDeCommune, regionMarrainage } from '@/utils/regions'
import { hasActiveRelation, statutInfo } from '@/utils/marrainageStatut'

const props = defineProps({
  // Marraines/parrains tels que renvoyés par l'API (statut brut, participantLinks...).
  marrainages: { type: Array, default: () => [] },
})

const router = useRouter()
const filleuls = ref([])

const mesureTag = (classe) => {
  const c = (classe || '').toLowerCase()
  if (!c) return null
  if (c.includes('tele') || c.includes('télé')) return { label: 'Télécours', color: 'deep-purple' }
  if (c.includes('a1')) return { label: 'F&I A1', color: 'teal' }
  if (c.includes('a2')) return { label: 'F&I A2', color: 'teal-darken-3' }
  return { label: classe, color: 'grey' }
}
const SOUHAIT_GENRE = { homme: 'souhaite un parrain', femme: 'souhaite une marraine' }

function group(items) {
  const byRegion = Object.fromEntries(REGIONS.map(r => [r, []]))
  for (const it of items) byRegion[REGIONS.includes(it.region) ? it.region : 'Autres'].push(it)
  Object.values(byRegion).forEach(list => list.sort((a, b) => a.title.localeCompare(b.title, 'fr')))
  return { byRegion, total: items.length }
}

const tables = computed(() => {
  const enCours = []
  for (const m of props.marrainages) {
    for (const pl of (m.participantLinks || []).filter(pl => pl.actif && pl.participant)) {
      enCours.push({
        key: `${m.marrainageId}-${pl.participantId}`,
        title: `${pl.participant.firstname} – ${m.prenom} ${m.nom}`,
        sub: m.commune || '',
        region: regionMarrainage(m),
        tags: [],
        to: { name: 'Marrainage-Modifier', params: { id: m.marrainageId } },
      })
    }
  }

  const aTrouver = filleuls.value.map(f => {
    const mesure = mesureTag(f.classe)
    return {
      key: `f-${f.id}`,
      title: `${f.firstname} ${f.lastName}`,
      sub: f.adresse || 'Adresse non renseignée',
      region: regionDeCommune(f.adresse),
      tags: [
        mesure,
        f.besoin === 'a_verifier' ? { label: 'À vérifier', color: 'orange' } : null,
        SOUHAIT_GENRE[f.souhaitGenreMarrainage] ? { label: SOUHAIT_GENRE[f.souhaitGenreMarrainage], color: 'blue-grey' } : null,
      ].filter(Boolean),
      to: { name: 'Participant-Detail', params: { id: f.id } },
    }
  })

  const disponibles = props.marrainages
    .filter(m => ['nouveau', 'en_attente', 'a_verifier', 'a_relancer'].includes(m.statut) && !hasActiveRelation(m))
    .map(m => ({
      key: `m-${m.marrainageId}`,
      title: `${m.prenom} ${m.nom}`,
      sub: m.commune || '',
      region: regionMarrainage(m),
      tags: [{ label: statutInfo(m.statut).title, color: statutInfo(m.statut).color }],
      to: { name: 'Marrainage-Modifier', params: { id: m.marrainageId } },
    }))

  return [
    { key: 'en-cours', title: 'Marrainages en cours', icon: 'mdi-account-heart-outline', color: 'success', hint: 'Filleul·e – parrain/marraine, par région du parrain/de la marraine.', ...group(enCours) },
    { key: 'a-trouver', title: 'Filleul·e·s : marrainage à trouver', icon: 'mdi-account-search-outline', color: 'orange', hint: "Participant·e·s avec « Marrainage : oui / à vérifier » sur leur fiche et sans relation active — région d'après leur adresse.", ...group(aTrouver) },
    { key: 'disponibles', title: 'Parrains / marraines disponibles', icon: 'mdi-account-clock-outline', color: 'blue', hint: 'Nouveaux contacts, en attente de binôme, à vérifier ou à relancer, sans relation active.', ...group(disponibles) },
  ]
})

onMounted(async () => {
  const res = await apiFetch(`${API_BASE}/api/Marrainages/filleuls-en-attente`)
  if (res?.ok) filleuls.value = await res.json()
})
</script>

<style scoped>
.board-scroll { overflow-x: auto; padding: 0 12px 12px; }
.board { display: grid; grid-template-columns: repeat(9, minmax(150px, 1fr)); gap: 8px; min-width: 1400px; }
.board-col { background: #f5f7fa; border-radius: 8px; padding-bottom: 6px; }
.board-head { color: #fff; font-weight: 700; font-size: 0.8rem; padding: 6px 10px; border-radius: 8px 8px 0 0; display: flex; justify-content: space-between; }
.board-count { opacity: 0.85; }
.board-card { background: #fff; margin: 6px 6px 0; padding: 6px 8px; border-radius: 6px; border: 1px solid #e4eaf2; cursor: pointer; }
.board-card:hover { border-color: #90caf9; }
.board-empty { color: #b0bac8; text-align: center; padding: 8px; font-size: 0.8rem; }
</style>
