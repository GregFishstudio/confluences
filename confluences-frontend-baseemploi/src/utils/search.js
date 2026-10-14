// Recherche tolérante sur les noms : ignore majuscules, accents/trémas, espaces en trop,
// ponctuation et ordre des mots, et accepte une petite faute de frappe (lettre en plus, en
// moins ou différente) — "cakmak sener", "Çakmak  Şener" ou "Sener Cakmk" trouvent tous
// "CAKMAK SENER Arzu".

// "Çakmak  ŞENER-Arzu" → "cakmak sener arzu"
export function normalizeSearch(text) {
  return String(text ?? '')
    .normalize('NFD').replace(/[̀-ͯ]/g, '')   // accents, trémas, cédilles
    .replace(/ı/g, 'i').replace(/ß/g, 'ss').replace(/[øœ]/g, 'o')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, ' ')
    .trim()
}

// Distance d'édition (Levenshtein) bornée : s'arrête dès que `max` est dépassé.
function distance(a, b, max) {
  if (Math.abs(a.length - b.length) > max) return max + 1
  let prev = Array.from({ length: b.length + 1 }, (_, j) => j)
  for (let i = 1; i <= a.length; i++) {
    const cur = [i]
    let rowMin = i
    for (let j = 1; j <= b.length; j++) {
      cur[j] = Math.min(prev[j] + 1, cur[j - 1] + 1, prev[j - 1] + (a[i - 1] === b[j - 1] ? 0 : 1))
      rowMin = Math.min(rowMin, cur[j])
    }
    if (rowMin > max) return max + 1
    prev = cur
  }
  return prev[b.length]
}

// Fautes tolérées selon la longueur du mot cherché (rien sous 4 lettres, sinon trop de faux positifs).
const tolerance = (len) => (len >= 8 ? 2 : len >= 4 ? 1 : 0)

function tokenMatches(token, words, compact) {
  if (compact.includes(token)) return true                // sous-chaîne, espaces ignorés
  const tol = tolerance(token.length)
  if (!tol) return false
  return words.some(w =>
    distance(token, w, tol) <= tol ||                      // mot entier avec une faute
    (w.length > token.length && distance(token, w.slice(0, token.length), tol) <= tol))  // début de mot
}

// true si chaque mot de la recherche se retrouve (à une faute près) dans l'un des champs.
export function matchesSearch(query, ...fields) {
  const tokens = normalizeSearch(query).split(' ').filter(Boolean)
  if (!tokens.length) return true
  const haystack = normalizeSearch(fields.flat().filter(Boolean).join(' '))
  const words = haystack.split(' ').filter(Boolean)
  const compact = haystack.replace(/ /g, '')
  return tokens.every(t => tokenMatches(t, words, compact))
}
