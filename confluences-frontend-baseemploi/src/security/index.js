import Oidc from "oidc-client"

const mgr = new Oidc.UserManager({
  userStore: new Oidc.WebStorageStateStore(),
  // Mise à jour de process.env.VUE_APP_... à import.meta.env....
  authority: import.meta.env.VUE_APP_AUTHORITY_URL,
  client_id: import.meta.env.VUE_APP_CLIENT_ID,
  
  // Utilisation de import.meta.env.VUE_APP_APPLICATION_URL
  redirect_uri: import.meta.env.VUE_APP_APPLICATION_URL + "/callback",
  post_logout_redirect_uri: import.meta.env.VUE_APP_APPLICATION_URL + "/",
  
  response_type: "code", // code flow PKCE
  scope: "openid profile api1 roles",
  automaticSilentRenew: true,
  
  // Utilisation de import.meta.env.VUE_APP_APPLICATION_URL
  silent_redirect_uri: import.meta.env.VUE_APP_APPLICATION_URL + "/silent-renew.html",
  
  loadUserInfo: true
})

// --- Gestion du logging ---
// Mise à jour de process.env.NODE_ENV à import.meta.env.NODE_ENV
if (import.meta.env.NODE_ENV === "development") {
  Oidc.Log.logger = console
  Oidc.Log.level = Oidc.Log.INFO
}

export default mgr