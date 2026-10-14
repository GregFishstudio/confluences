import moment from 'moment/min/moment-with-locales'

// Parsing explicite (le mode strict de moment refuse "12.03.1985" avec le format "D.M.YYYY").
// Renvoie AAAA-MM-JJ, null si vide, undefined si la saisie n'est pas (encore) une date valide.
export function parseDateFr(value) {
  const v = (value || '').trim().replace(/\s+/g, '')
  if (!v) return null
  let d, m, y
  let match = v.match(/^(\d{4})[-./](\d{1,2})[-./](\d{1,2})$/)
  if (match) [, y, m, d] = match
  else if ((match = v.match(/^(\d{1,2})[-./](\d{1,2})[-./](\d{2}|\d{4})$/))) [, d, m, y] = match
  else if ((match = v.match(/^(\d{2})(\d{2})(\d{4})$/))) [, d, m, y] = match
  else return undefined
  let year = Number(y)
  if (y.length === 2) year += year > (new Date().getFullYear() % 100) ? 1900 : 2000
  const date = moment({ year, month: Number(m) - 1, day: Number(d) })
  return date.isValid() && year > 1900 ? date.format('YYYY-MM-DD') : undefined
}
