// Bénévolat – domaines d'intérêt et types d'activité souhaités (fiche participant·e, choix
// multiples). Mêmes libellés côté API (AspNetUsersController.cs, classe Benevolat) : l'API ne
// garde que les valeurs de ces listes.
export const AUTRE_DOMAINE = 'Autre (à préciser)'

export const BENEVOLAT_DOMAINES = [
  'Social et solidarité',
  'Personnes âgées',
  'Enfance et jeunesse',
  'Santé et handicap',
  'Cuisine et restauration',
  'Nature et environnement',
  'Animaux',
  'Recyclage et seconde main',
  'Art et culture',
  'Sport et loisirs',
  'Commerce et logistique',
  'Entretien et travaux manuels',
  'Événementiel',
  'Administration et informatique',
  AUTRE_DOMAINE,
]

export const BENEVOLAT_TYPES_ACTIVITE = [
  'Activités manuelles ou pratiques',
  'Activités en contact avec des personnes',
  'Activités administratives ou informatiques',
  'Sans préférence',
]

// Libellés affichables des domaines d'une personne : « Autre » remplacé par sa précision.
export function domainesAffiches(p) {
  return (p?.benevolatDomaines || []).map(d =>
    d === AUTRE_DOMAINE && p.benevolatDomaineAutre ? `Autre : ${p.benevolatDomaineAutre}` : d)
}

// Filtre « au moins un des domaines choisis » ; « Autre » correspond aussi à la précision saisie.
export const aUnDesDomaines = (p, domaines) =>
  !domaines?.length || (p?.benevolatDomaines || []).some(d => domaines.includes(d))
