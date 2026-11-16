// Numérotation des sessions du calendrier de formation.
//
// Le numéro d'une session (le « quarter » / sessionNumberId côté API) est son rang parmi les
// sessions du MÊME groupe démarrant la MÊME année : la première session 2027 est la Session 1,
// même si le calendrier contient déjà les 4 sessions de 2026.
//
// `groupSessions` = sessions d'un seul groupe, triées par date de début (activeGroupSessions).

const yearOf = s => (s?.startDate ? Number(String(s.startDate).substring(0, 4)) : null)

/** Numéro (1-based) dans l'année de la session à l'index `index`. */
export function sessionNumberAt(groupSessions, index) {
  const s = groupSessions[index]
  if (!s) return index + 1
  const y = yearOf(s)
  return groupSessions.slice(0, index).filter(o => yearOf(o) === y).length + 1
}

/** Index de la session `number` de l'année `year`, ou -1. */
export function sessionIndexOf(groupSessions, year, number) {
  let n = 0
  return groupSessions.findIndex(s => yearOf(s) === Number(year) && ++n === Number(number))
}

/** Session `number` de l'année `year`, ou null. */
export function findSession(groupSessions, year, number) {
  return groupSessions[sessionIndexOf(groupSessions, year, number)] || null
}
