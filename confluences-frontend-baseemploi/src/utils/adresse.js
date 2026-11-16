// Adresse structurée des participant·e·s : rue + n°, NPA, localité. Mêmes règles que l'API
// (AspNetUsersController.cs, classe AdresseRules), vérifiées ici pour prévenir avant l'envoi.

const MOTS_LIBRES = new Set([
  'dans', 'près', 'pres', 'vers', 'environ', 'hauts', 'haut', 'quartier', 'proche',
  'côté', 'cote', 'région', 'region', 'centre', 'à', 'au', 'aux',
])

export const npaRule = v => !v || /^\d{4}$/.test(String(v).trim()) || 'NPA : 4 chiffres (ex. 1004)'

export function localiteRule(v) {
  const s = (v || '').trim()
  if (!s) return true
  const mots = s.split(/\s+/)
  const ok = s.length <= 60 && /^\p{L}[\p{L}\s'’.\-()]*$/u.test(s)
    && mots.length <= 4 && !mots.some(m => MOTS_LIBRES.has(m.toLowerCase()))
  return ok || 'Un nom de localité (ex. Lausanne), sans indication libre'
}

// Adresse sur une ligne, comme recomposée par l'API.
export function composerAdresse(rue, npa, localite) {
  const ville = [npa, localite].map(x => (x || '').trim()).filter(Boolean).join(' ')
  return [(rue || '').trim(), ville].filter(Boolean).join(', ')
}
