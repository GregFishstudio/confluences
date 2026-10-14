// Protection des saisies non enregistrées : tant que `isDirty()` est vrai, demande confirmation
// avant de quitter la page (navigation interne, bouton/geste "retour") et avant de fermer ou
// recharger l'onglet du navigateur.
import { onMounted, onBeforeUnmount } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'

export const UNSAVED_MESSAGE = 'Des modifications n\'ont pas été enregistrées. Quitter quand même et les perdre ?'

export function confirmDiscard(message = UNSAVED_MESSAGE) {
  return window.confirm(message)
}

// beforeLeave (facultatif) : appelé à la place de la confirmation standard quand on quitte la
// page avec des modifications (ex. enregistrer d'abord) ; renvoie true pour autoriser le départ.
export function useUnsavedGuard(isDirty, message = UNSAVED_MESSAGE, beforeLeave = null) {
  const onBeforeUnload = (event) => {
    if (!isDirty()) return
    event.preventDefault()
    event.returnValue = ''
  }

  onMounted(() => window.addEventListener('beforeunload', onBeforeUnload))
  onBeforeUnmount(() => window.removeEventListener('beforeunload', onBeforeUnload))

  onBeforeRouteLeave(async () => {
    if (!isDirty()) return true
    return beforeLeave ? await beforeLeave() : confirmDiscard(message)
  })
}
